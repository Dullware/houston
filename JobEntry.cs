using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Dullware.OldVBControls;
using Microsoft.Win32;

public enum Display { name, stat, start, stop, cpu, mem, dir, where };
public enum Status { running, aborted, completed, idle, scheduled, rscheduled, queued, rqueued, waiting, resumed, schedulerequest, compiling };
public enum FileType { sfbox, mcrenko, mccompile, ctb };

public class JobEntry : ListViewItem
{
    public bool Locked;
    public bool RunAndDownloadIsActive = false;
    public Process _job;
    public SfbClient _net;
    Houston _houston;
    public TabControl _theTabs;
    TabPage _tabInput;
    TabPage _tabLog;
    System.Threading.Timer timer;
    System.Threading.TimerCallback timerCallback;
    delegate void TimerTickCallback(Object obj);
    TimerTickCallback timerTickCallback;
    public OpenAndSave _inputFile;
    public TextBox _log;
    public TextBox _input;
    public FileSystemWatcher _fileWatch;
    string _inputText = "";
    public string remotehost;
    public int remoteport;
    Byte[] _buf;
    StreamReader _jobStream;
    AsyncCallback asyncCallback;
    public delegate void AsyncReadCallback(string s);
    public AsyncReadCallback asyncReadCallback;
    public Status stat;
    public FileType filetype = FileType.sfbox;
    public DateTime _schedTime;
    bool _checkInputFile;
    delegate void PopupMessagesDelegate(ArrayList al);
    delegate void SetStatusDelegate(Status s);
    delegate void RunDelegate(SfbClient _net, string shortName, string directoryName, JobEntry je);
    delegate void FileChangedDelegate(object source, FileSystemEventArgs e);
    public float CPU; //Used by Get(Final)Status
    public int MEM; //Used by Get(Final)Status

    public JobEntry(Houston houston, TabControl theTabs, TabPage tabInput, TabPage tabLog)
    {
        _houston = houston;
        _theTabs = theTabs;
        _tabInput = tabInput;
        _tabLog = tabLog;

        _inputFile = new OpenAndSave();
        _inputFile.Filter = "SFBox Input Files (*.in)|*.in|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        //_inputFile.Multiselect = true; //werkt niet
        _inputFile.ReadData += new OpenAndSave.ReadDataEventHandler(ReadData);
        _inputFile.WriteData += new OpenAndSave.WriteDataEventHandler(WriteData);
        _inputFile.RevertData += new OpenAndSave.RevertDataEventHandler(RevertData);
        _inputFile.FileNameChanged += new OpenAndSave.FileNameChangedEventHandler(FileNameChanged_Handler);
        _inputFile.FolderNameChanged += new OpenAndSave.FolderNameChangedEventHandler(FolderNameChanged_Handler);

        _fileWatch = new FileSystemWatcher();
        _fileWatch.NotifyFilter = NotifyFilters.LastWrite;
        _fileWatch.Changed += FileChanged_Handler;


        for (int i = 1; i < Enum.GetNames(typeof(Display)).Length; i++)
        { //Init the subitems
            SubItems.Add("");
        }

        SetupJob();
    }

    public bool selectedText_input()
    {
        return _input.Focused && _input.SelectionLength > 0;
    }

    public bool selectedText_log()
    {
        return _log.Focused && _log.SelectionLength > 0;
    }

    delegate void WriteToLogDelegate(string s);
    public void WriteToLog(string s)
    {
        if (!_houston.InvokeRequired)
        {
            string addLine = DateTime.Now.ToString() + " " + FileName + ": " + s + "\r\n";
            if (_houston.logBox.Text.Length + addLine.Length > _houston.logBox.MaxLength)
                _houston.logBox.Text = _houston.logBox.Text.Substring(addLine.Length);
            _houston.logBox.AppendText(addLine);
        }
        else //We are on a non GUI thread.
        {
            WriteToLogDelegate wtlDel = new WriteToLogDelegate(WriteToLog);
            _houston.Invoke(wtlDel, new object[] { s });
        }
    }

    delegate void WriteToLogDelegate2(string s, string s2);
    private void WriteToLog(string oldName, string s)
    {
        if (!_houston.InvokeRequired)
            _houston.logBox.AppendText(DateTime.Now.ToString() + " " + oldName + ": " + s + "\r\n");
        else
            _houston.Invoke(new WriteToLogDelegate2(WriteToLog), new object[] { oldName, s });
    }


    ArrayList GetOldMessagesIDs()
    {
        RegistryKey reghouston11 = Registry.CurrentUser.OpenSubKey(Houston.regKeyString, true);
        if (reghouston11 == null) reghouston11 = Registry.CurrentUser.CreateSubKey(Houston.regKeyString);

        RegistryKey regnews = reghouston11.OpenSubKey("News", true);
        if (regnews == null) regnews = reghouston11.CreateSubKey("News");

        string[] news = regnews.GetValueNames();
        ArrayList al = new ArrayList();

        foreach (string item in news)
        {
            al.Add(regnews.GetValue(item).ToString());
            regnews.DeleteValue(item);
        }

        regnews.Close();
        reghouston11.Close();
        return al;
    }

    void StoreNewMessagesIDs(ArrayList newMsgs)
    {
        RegistryKey reghouston11 = Registry.CurrentUser.OpenSubKey(Houston.regKeyString, true);
        if (reghouston11 == null) reghouston11 = Registry.CurrentUser.CreateSubKey(Houston.regKeyString);

        RegistryKey regnews = reghouston11.OpenSubKey("News", true);
        if (regnews == null) regnews = reghouston11.CreateSubKey("News");

        for (int i = 0; i < newMsgs.Count; i++)
        {
            string msg = newMsgs[i].ToString();
            regnews.SetValue("item" + i.ToString(), msg.Substring(1, msg.IndexOf(']') - 1));
        }

        regnews.Close();
        reghouston11.Close();
    }

    public void PopupMessages(ArrayList curMsgs)
    {
        if (!_houston.InvokeRequired)
        {
            if (curMsgs.Count > 0)
            { //do nothing if there are no message (don't clear the registry as well)
                ArrayList oldMsgs = GetOldMessagesIDs();
                foreach (string cur in curMsgs)
                {
                    if (!oldMsgs.Contains(cur.Substring(1, cur.IndexOf(']') - 1)))
                    {
                        MessageBox.Show(_houston, "[id = " + cur.Substring(1, cur.IndexOf(']') - 1) + "]\r\n\r\n" + cur.Substring(cur.IndexOf(']') + 1).Trim(), "Server Message", MessageBoxButtons.OK);
                    }
                }
                StoreNewMessagesIDs(curMsgs);
            }
        }
        else //We are on a non GUI thread.
        {
            PopupMessagesDelegate popmsgDel = new PopupMessagesDelegate(PopupMessages);
            _houston.Invoke(popmsgDel, new object[] { curMsgs });
        }

    }

    delegate void SetSubItemDelegate(Display i, string s);
    void SetSubItem(Display i, string s)
    {
        if (_houston.InvokeRequired) _houston.Invoke(new SetSubItemDelegate(SetSubItem), new object[] { i, s });
        else SubItems[(int)i].Text = s;
    }

    public void SetCpuAndMem()
    {
        SetSubItem(Display.cpu, CPU.ToString("#0.0") + "s");
        SetSubItem(Display.mem, MEM.ToString("n0") + "kb");
    }

    public void SetStatus(Status s)
    {
        if (!_houston.InvokeRequired)
        {
            if (_checkInputFile) return;
            switch (s)
            {
                case Status.aborted:
                    SubItems[(int)Display.stat].Text = "Aborted";
                    WriteToLog("aborted");
                    Locked = false;
                    break;
                case Status.completed:
                    SubItems[(int)Display.stat].Text = "Completed";
                    WriteToLog("job completed");
                    Locked = false;
                    break;
                case Status.idle:
                    SubItems[(int)Display.stat].Text = "Idle";
                    if (stat == Status.queued || stat == Status.rqueued)
                        WriteToLog("dequeued");
                    if (stat == Status.scheduled || stat == Status.rscheduled)
                        WriteToLog("unscheduled");
                    else
                        WriteToLog("added to the job list");
                    WriteToLog("folder name set to " + new FileInfo(_inputFile.Filename).DirectoryName);
                    _fileWatch.Path = new FileInfo(_inputFile.Filename).DirectoryName;
                    break;
                case Status.running:
                    SubItems[(int)Display.stat].Text = "Running";
                    WriteToLog("started");
                    break;
                case Status.queued:
                case Status.rqueued:
                    SubItems[(int)Display.stat].Text = "Queued";
                    WriteToLog("queued for immediate startup");
                    if (stat == Status.scheduled || stat == Status.rscheduled)
                        SubItems[(int)Display.start].Text = "";
                    break;
                case Status.scheduled:
                case Status.rscheduled:
                    SubItems[(int)Display.stat].Text = "Scheduled";
                    WriteToLog("scheduled for " + _schedTime.ToString());
                    break;
                case Status.schedulerequest:
                    SubItems[(int)Display.stat].Text = "Waiting to be scheduled";
                    WriteToLog("waiting to be scheduled");
                    Locked = false;
                    break;

                case Status.waiting:
                    SubItems[(int)Display.stat].Text = "Paused";
                    WriteToLog("paused by the remote scheduler and waiting to be resumed");
                    break;
                case Status.resumed:
                    SubItems[(int)Display.stat].Text = "Running";
                    WriteToLog("calculation is resumed");
                    break;
                case Status.compiling:
                    SubItems[(int)Display.stat].Text = "compiling";
                    WriteToLog("compiling the source code");
                    break;
                default:
                    break;
            }
            stat = s;
            if (s == Status.resumed) s = Status.running;
        }
        else //We are on a non GUI thread.
        {
            SetStatusDelegate ssDel = new SetStatusDelegate(SetStatus);
            _houston.Invoke(ssDel, new object[] { s });
        }
    }

    public Boolean AddNewJob()
    {
        _input.SelectionLength = 0;
        _input.SelectionStart = 0;
        _input.Select();
        return true;
    }

    public Boolean OpenInputFile(string filename)
    {
        if (_inputFile.OpenFile(filename, false))
        {
            _fileWatch.Path = new FileInfo(_inputFile.Filename).DirectoryName;
            _fileWatch.Filter = new FileInfo(_inputFile.Filename).Name;
            _fileWatch.EnableRaisingEvents = true;
            _input.SelectionLength = 0;
            _input.SelectionStart = 0;
            _input.Select();
            return true;
        }
        return false;
    }

    public Boolean SaveInputFile()
    {
        return _inputFile.SaveFile(InputChanged());
    }

    public Boolean CloseWindow()
    {
        return _inputFile.CloseWindow(InputChanged());
    }

    private void SetupJob()
    {
        _input = new TextBox();
        _input.Multiline = true;
        _input.AcceptsReturn = true;
        _input.AcceptsTab = true;
        _input.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        _input.Dock = DockStyle.Fill;
        _input.SelectionStart = 0; //Work-around for the AppendText bug.
        _input.TextChanged += new EventHandler(_input_TextChanged);

        _log = new TextBox();
        _log.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        _log.Multiline = true;
        _log.AcceptsReturn = true;
        _log.AcceptsTab = true;
        _log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        _log.Dock = DockStyle.Fill;
        _log.SelectionStart = 0; //Work-around for the AppendText bug.
        _log.ReadOnly = true;

        SetSubItem(Display.name, FileName);
        SetSubItem(Display.dir, DirectoryName);

        _buf = new Byte[512];
        asyncCallback = new AsyncCallback(AsyncRead);
        asyncReadCallback = new AsyncReadCallback(InvokedAsyncRead);

        SetStatus(Status.idle);
        timerCallback = new System.Threading.TimerCallback(TimerTick);
        timerTickCallback = new TimerTickCallback(InvokedTimerTick);
        timer = new System.Threading.Timer(timerCallback, this, 0, 2000);
    }

    delegate void SelectDel();
    public void Select()
    {
        if (!_houston.InvokeRequired)
        {
            _tabInput.Controls.Clear();
            _tabInput.Controls.Add(_input);
            //if ( _tabInput.Controls.Count > 1) _tabInput.Controls.RemoveAt(0);
            _tabLog.Controls.Clear();
            _tabLog.Controls.Add(_log);
            //if ( _tabLog.Controls.Count > 1) _tabLog.Controls.RemoveAt(0);

            _input.SelectionLength = 0;
            _input.SelectionStart = 0;
            _log.ScrollToCaret();
            //_input.Select();
            Focused = true;

            _input_TextChanged(null, new System.EventArgs());
        }
        else _houston.Invoke(new SelectDel(Select));
    }

    public string SetTime(DateTime dt)
    {
        return _checkInputFile ? "" : dt.ToString("ddd HH:mm:ss");
    }

    public void SetStartTime(DateTime dt)
    {
        SetSubItem(Display.start, SetTime(dt));
        SetSubItem(Display.stop, "");
    }

    public void SetStopTime(DateTime dt)
    {
        SetSubItem(Display.stop, SetTime(dt));
    }

    public void Schedule(DateTime dt, bool remote)
    {
        _schedTime = dt;
        SetStatus(remote ? Status.rscheduled : Status.scheduled);
        SetStartTime(dt);
        SetSubItem(Display.stop, "");
        SetSubItem(Display.cpu, "");
        SetSubItem(Display.mem, "");
    }

    public void Unschedule()
    {
        SetStatus(Status.idle);
        SetSubItem(Display.start, "");
    }

    public void StartJob(bool checkInputFile)
    {
        _checkInputFile = checkInputFile;
        _theTabs.SelectedTab = _tabLog;
        _log.Text = "";

        Interlocked.Increment(ref Scheduler.numJobsRunning);
        _houston.SetStatusBarText(Scheduler.numJobsRunning.ToString());

        _net = null;
        _job = new Process();
        _job.EnableRaisingEvents = true;
        if (filetype == FileType.sfbox)
        {
            _job.StartInfo.FileName = Application.StartupPath + "\\Plugins\\sfbox";
            _job.StartInfo.Arguments = "\"" + FullName + "\"";
        }
        else if (filetype == FileType.mcrenko)
        {
            string fname = Path.GetFileNameWithoutExtension(FullName);
            string fext = Path.GetExtension(FullName);
            _job.StartInfo.FileName = Application.StartupPath + "\\Plugins\\MCRenko";
            _job.StartInfo.Arguments = string.Format("\"{0}\" {1}a{2} {1}b{2}", FullName, fname, fext);
        }
        else if (filetype == FileType.ctb)
        {
            string fname = Path.GetFileNameWithoutExtension(FullName);
            string fext = Path.GetExtension(FullName);
            _job.StartInfo.FileName = Application.StartupPath + "\\Plugins\\ctb";
            _job.StartInfo.Arguments = "\"" + FullName + "\"";
        }

        _job.StartInfo.RedirectStandardOutput = true;
        _job.StartInfo.UseShellExecute = false;
        _job.StartInfo.CreateNoWindow = true;
        if (checkInputFile)
            _job.StartInfo.Arguments = "-c " + _job.StartInfo.Arguments;
        _job.StartInfo.WorkingDirectory = DirectoryName;

        try
        {
            _job.Exited += new System.EventHandler(JobHasExited);
            _job.Start();
        }
        catch (Exception e)
        {
            SetStatus(Status.aborted);
            MessageBox.Show("Starting the job failed:\n\n" + e.Message + "\n\n" + _job.StartInfo.FileName, "Start Failed");
            _log.AppendText("\r\nCould not start.");
            Interlocked.Decrement(ref Scheduler.numJobsRunning);
            _houston.SetStatusBarText(Scheduler.numJobsRunning.ToString());
            return;
        }
        try
        {
            if (System.Environment.OSVersion.ToString().StartsWith("Microsoft Windows NT"))
                _job.PriorityClass = ProcessPriorityClass.Idle;
            //else MessageBox.Show("Priority not set\n\n"+System.Environment.OSVersion.ToString());
            _jobStream = _job.StandardOutput;
            _jobStream.BaseStream.BeginRead(_buf, 0, 512, asyncCallback, null);
        }
        catch (Exception e)
        {
            WriteToLog(e.Message);
        }

        if (!checkInputFile)
        {
            SetStartTime(_job.StartTime);
            SetSubItem(Display.cpu, "0.0s");
            SetStatus(Status.running);
        }
        else WriteToLog("input checking started");

        _houston.jobslistview.Select();
        Focused = true;

    }

    delegate void MessageBoxShowDelegate(Houston owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    static void MessageBoxShow(Houston owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        if (!owner.InvokeRequired)
            MessageBox.Show(owner, text, caption, buttons, icon);
        else
            owner.Invoke(new MessageBoxShowDelegate(MessageBoxShow), new object[] { owner, text, caption, buttons, icon });

    }

    static void RunAndDownload(SfbClient _net, string shortName, string directoryName, JobEntry je)
    {
        je.RunAndDownloadIsActive = true;
        string errortext = null;

        _net.Run(je.filetype, shortName, je, ref errortext);

        if (errortext == "Maximum number of concurrent jobs exceeded.")
        {
            _net.Disconnect();
            je.WriteToLog(errortext.ToLower());
            je.SetStatus(Status.schedulerequest);
        }
        else if (errortext != null)
        {
            _net.Disconnect();
            MessageBoxShow(je._houston, errortext, "Run error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            _net.GetFinalStatus();
            je.SetCpuAndMem();

            _net.Download(je.DirectoryName);
            //je.WriteToLog("downloading finished");
            _net.Disconnect();
            if (je.stat == Status.aborted) je.WriteToLog("abort completed");
        }
    }

    public void StartRemoteJob()
    {
        string error;
        _checkInputFile = false;
        _theTabs.SelectedTab = _tabLog;
        _log.Text = "";

        Interlocked.Increment(ref Scheduler.numNetJobsRunning);
        //_houston.SetStatusBarText(Scheduler.numJobsRunning.ToString());

        _job = null;
        _net = new SfbClient(this);
        SetSubItem(Display.cpu, "0.0s");
        SetSubItem(Display.mem, "0kb");
        if ((error = _net.Connect(remotehost, remoteport, filetype)) == null)
        {
            string requiredversion = "";

            if (filetype == FileType.sfbox && _net.ServerVersionIsAtLeast(requiredversion = "0.5") || (filetype == FileType.mcrenko || filetype == FileType.ctb ) && _net.ServerVersionIsAtLeast(requiredversion = "0.7"))
            {
                this._houston.AddtoRegistry(remotehost, remoteport);

                ArrayList news = _net.GetNews();

                string shortName = _net.SendInputFile(FullName);

                if (filetype == FileType.mcrenko) RemoteCompileMC(_net);

                SetStartTime(DateTime.Now);
                SetStatus(Status.running);

                RunDelegate run = new RunDelegate(RunAndDownload);
                AsyncCallback myAsyncCallback = new AsyncCallback(NetHasExited);
                run.BeginInvoke(_net, shortName, DirectoryName, this, myAsyncCallback, null);

                PopupMessages(news);
            }
            else
            {
                SetStatus(Status.aborted);
                MessageBox.Show("You need at least server version " + requiredversion + " for what you are trying to do.", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Interlocked.Decrement(ref Scheduler.numNetJobsRunning);

            }
        }
        else
        {
            SetStatus(Status.aborted);
            MessageBox.Show("Server connection error:\n\n" + error, "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _log.AppendText("\r\nCould not connect: " + error);
            Interlocked.Decrement(ref Scheduler.numNetJobsRunning);
            //_houston.SetStatusBarText(Scheduler.numJobsRunning.ToString());
        }
    }

    private void RemoteCompileMC(SfbClient _net)
    {
        string[] sources = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.f90", SearchOption.TopDirectoryOnly);
        if (sources.Length > 0 && _net.ServerVersionIsAtLeast("0.8") && _net.SupportedFileType(FileType.mccompile))
        {
            string errortext = null;

            foreach (string source in sources)
            {
                WriteToLog(string.Format("uploaded {0}", _net.SendFile(new FileInfo(source))));
            }
            SetStartTime(DateTime.Now);
            SetStatus(Status.compiling);

            // Compileer op de gui thread (hoort eigenlijk niet)
            _net.Run(FileType.mccompile,"mc.f90", this, ref errortext);

            if (errortext != null)
                MessageBox.Show(errortext, "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (sources.Length > 0)
            MessageBox.Show("There are source files found with your input file. \r\nHowever, " + SubItems[(int)Display.where].Text + " does not support remote compiling.", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void AbortJob()
    {
        if (stat == Status.running)
        {
            try
            { //Job might be dead already
                if (_job != null) _job.Kill(); else _net.Kill();
            }
            catch (Exception e)
            {
                WriteToLog("QQ1" + e.Message);
            }

            SetStatus(Status.aborted);
            //_log.AppendText("\r\nAborted.\r\n");
        }
    }

    private void InvokedAsyncRead(string s)
    {
        if (_log.Text.Length + s.Length > _log.MaxLength)
            _log.Text = _log.Text.Substring(s.Length);
        _log.AppendText(s);
        //WriteToLog(_log.Text.Length.ToString());
    }

    private void AsyncRead(IAsyncResult res)
    {
        int nbytes = _jobStream.BaseStream.EndRead(res);
        if (nbytes > 0)
        {
            _houston.Invoke(asyncReadCallback, new Object[] { System.Text.Encoding.ASCII.GetString(_buf, 0, nbytes) });
            _jobStream.BaseStream.BeginRead(_buf, 0, 512, asyncCallback, null);
        }
    }

    delegate void JHE_del(object sender, EventArgs e);
    private void JobHasExited(object sender, System.EventArgs ea)
    {
        if (!_houston.InvokeRequired)
        {
            try
            { //De job kan helemaal weg zijn (_job niet geassocieerd met een process)
                if (_job.HasExited)
                {
                    SetStopTime(_job.ExitTime);
                    try
                    {
                        SetSubItem(Display.cpu, _job.TotalProcessorTime.TotalSeconds.ToString("#0.0") + "s");
                        SetSubItem(Display.mem, (_job.PeakWorkingSet64 / 1024).ToString("n0") + "kb");
                    }
                    catch (Exception e) { WriteToLog("QQ2" + e.Message); }
                }
                else WriteToLog("JobHasExited: NOT HasExited");

                Interlocked.Decrement(ref Scheduler.numJobsRunning);
                _houston.SetStatusBarText(Scheduler.numJobsRunning.ToString());
            }
            catch (Exception e)
            {
                WriteToLog("QQ3" + e.Message);
            }

            if (stat != Status.aborted)
            {
                SetStatus(Status.completed);
            }

            if (_checkInputFile)
            {
                WriteToLog("input checking completed");
                _checkInputFile = false;
            }
        }
        else
            _houston.Invoke(new JHE_del(JobHasExited), new object[] { sender, ea });
    }

    private void NetHasExited(IAsyncResult res)
    {
        //Draait niet op de gui thread
        SetStopTime(DateTime.Now);

        if (_net != null)
        {
            _net = null;
        }

        Interlocked.Decrement(ref Scheduler.numNetJobsRunning);
        RunAndDownloadIsActive = false;

        if (stat == Status.schedulerequest)
        {
            Schedule(DateTime.Now.AddMinutes(1), true);
        }
        else if (stat != Status.aborted)
        {
            SetStatus(Status.completed);
        }

    }

    public string DirectoryName
    {
        get
        {
            return !_inputFile.Untitled ? new FileInfo(_inputFile.Filename).DirectoryName : "Not set";
        }
    }

    public string FullName
    {
        get
        {
            return new FileInfo(_inputFile.Filename).FullName;
        }
    }

    public string FileName
    {
        get
        {
            return new FileInfo(_inputFile.Filename).Name;
        }
    }

    public static void InvokedTimerTick(Object obj)
    {
        JobEntry je = (JobEntry)obj;

        try
        {
            if (je._job != null)
            {
                int id = je._job.Id; //Test if the process exists.
                je._job.Refresh();
                if (!je._job.HasExited)
                {
                    System.TimeSpan diff1 = DateTime.Now.Subtract(je._job.StartTime);
                    //je.SubItems[(int)Display.cpu].Text = diff1.TotalSeconds.ToString("#0.0")+"s";
                    je.SubItems[(int)Display.cpu].Text = je._job.TotalProcessorTime.TotalSeconds.ToString("#0.0") + "s";
                    je.SubItems[(int)Display.mem].Text = (1 + je._job.WorkingSet64 / 1024).ToString("n0") + "kb";
                }
            }
            else if (je._net != null)
            {
                je.SetCpuAndMem();
            }
        }
        catch //This might be a _net job
        {
            if (je._net != null)
            {
            }
        }

        if (je.stat == Status.scheduled && DateTime.Compare(DateTime.Now, je._schedTime) > 0)
        {
            je.SetStatus(Status.queued);
        }
        else if (je.stat == Status.rscheduled && DateTime.Compare(DateTime.Now, je._schedTime) > 0)
        {
            je.SetStatus(Status.rqueued);
        }
    }

    public static void TimerTick(Object obj)
    {
        try
        {
            JobEntry je = (JobEntry)obj;
            if (je.ListView != null)
            {
                if (je._net != null && je.stat == Status.running)
                {
                    je._net.GetStatus();
                }
                je.ListView.Invoke(je.timerTickCallback, new Object[] { obj });
            }
        }
        catch { }
    }

    public Boolean InputChanged()
    {
        return _input.Text != _inputText;
    }

    private void FolderNameChanged_Handler(string oldName, string newName)
    {
        _fileWatch.Path = newName;
        _fileWatch.EnableRaisingEvents = true;
        WriteToLog("folder name changed to " + newName);
    }

    private void FileNameChanged_Handler(string oldName, string newName)
    {
        _fileWatch.Filter = newName;
        WriteToLog(oldName, "job name changed to " + newName);
    }

    private void FileChanged_Handler(object o, FileSystemEventArgs e)
    {
        if (!_houston.InvokeRequired)
        {
            _fileWatch.EnableRaisingEvents = false;
            WriteToLog("input file updated by another application");
            //			MessageBox.Show("Another application has updated file " + _inputFile.Filename + "\nReload it?","Houston",MessageBoxButtons.YesNo);
            Reload reload = new Reload();
            reload.Question.Text = "Another application has updated file\n" + _inputFile.Filename + "\nReload it?";
            _houston.Enabled = false;
            if (reload.ShowDialog(_houston) == DialogResult.Yes)
            {
                ReadData(e.FullPath);
                WriteToLog("input file update by other application applied");
            }
            else
            {
                WriteToLog("input file update by other application discarded");
                if (reload.undochanges.Checked)
                {
                    StreamWriter SW = new StreamWriter(_inputFile.Filename, false, System.Text.Encoding.ASCII);
                    SW.Write(_inputText);
                    SW.Close();
                    WriteToLog("input file on disk restored");
                }
            }
            _houston.Enabled = true;
            _fileWatch.EnableRaisingEvents = true;
        }
        else //We are on a non GUI thread.
        {
            _houston.Invoke(new FileChangedDelegate(FileChanged_Handler), new object[] { o, e });
        }
    }

    private void ReadData(string fileName)
    {
        StreamReader SR = new StreamReader(fileName, System.Text.Encoding.ASCII);
        string s, str = "";
        bool mcrenkotag1 = false, mcrenkotag2 = false, ctbtag = false;

        s = SR.ReadLine();
        if ( s != null && s.TrimStart().ToLower().StartsWith("%inputfile for propulsion simulation")) ctbtag = true;
        while (s != null)
        {
            if (s.TrimStart().ToLower().StartsWith("begin cnf")) mcrenkotag1 = true;
            if (s.TrimStart().ToLower().StartsWith("end cnf")) mcrenkotag2 = true;
            str += s + "\r\n";
            s = SR.ReadLine();
        }
        SR.Close();

        _inputText = str; _input.Text = str;
        SetSubItem(Display.name, FileName);
        SetSubItem(Display.dir, DirectoryName);

        if (mcrenkotag1 && mcrenkotag2) filetype = FileType.mcrenko;
        else if ( ctbtag ) filetype = FileType.ctb;
    }

    private void WriteData(string fileName)
    {
        _fileWatch.EnableRaisingEvents = false;
        StreamWriter SW = new StreamWriter(fileName, false, System.Text.Encoding.ASCII);
        SW.Write(_input.Text);
        SW.Close();
        _fileWatch.EnableRaisingEvents = true;

        WriteToLog("input file saved");

        SetSubItem(Display.name, FileName);
        SetSubItem(Display.dir, DirectoryName);
        if (_tabInput.Text.EndsWith(" *"))
        {
            _tabInput.Text = _tabInput.Text.TrimEnd(new char[] { ' ', '*' });
        }
        _inputText = _input.Text;

        bool mcrenkotag1 = false, mcrenkotag2 = false, ctbtag = false;
        string[] lines = _inputText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string s in lines)
        {
            if (s.TrimStart().ToLower().StartsWith("begin cnf")) mcrenkotag1 = true;
            if (s.TrimStart().ToLower().StartsWith("end cnf")) mcrenkotag2 = true;
        	if ( s.TrimStart().ToLower().StartsWith("%inputfile for propulsion simulation")) ctbtag = true;
        }
        if (mcrenkotag1 && mcrenkotag2) filetype = FileType.mcrenko;
        else if ( ctbtag ) filetype = FileType.ctb;
    }

    private void RevertData(string fileName)
    {
        _input.Text = _inputText;
    }

    private void _input_TextChanged(object sender, System.EventArgs e)
    {
        if (InputChanged())
        {
            if (!_tabInput.Text.EndsWith(" *")) _tabInput.Text += " *";
        }
        else if (_tabInput.Text.EndsWith(" *"))
        {
            _tabInput.Text = _tabInput.Text.TrimEnd(new char[] { ' ', '*' });
        }

    }

    public void Dispose()
    {
        _tabInput.Controls.Clear();
        _tabLog.Controls.Clear();
        if (_job != null) _job.Dispose();
        timer.Dispose();
        _inputFile = null;
    }
}
