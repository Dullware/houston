using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Win32;
using Dullware.Library;

public class Houston : DullForm
{
    static System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
    public static string regKeyString = string.Format(@"Software\DullWare\{0}", Application.ProductName);
    public static string HoustonVersion = string.Format("{0}.{1} (Build {2})", info.ProductMajorPart, info.ProductMinorPart, info.ProductBuildPart);

    public System.Windows.Forms.StatusBar SB;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.Splitter splitter1;
    public System.Windows.Forms.ListView jobslistview;
    private System.Windows.Forms.TabPage tabInput;
    private System.Windows.Forms.TabPage tabLog;
    private System.Windows.Forms.MenuStrip mMenu;
    private System.Windows.Forms.ToolStripMenuItem mFile;
    private System.Windows.Forms.ToolStripMenuItem mFileNew;
    private System.Windows.Forms.ToolStripMenuItem mFileOpen;
    private System.Windows.Forms.ToolStripMenuItem mFileClose;
    private System.Windows.Forms.ToolStripMenuItem mFileSave;
    private System.Windows.Forms.ToolStripMenuItem mFileSaveAs;
    private System.Windows.Forms.ToolStripMenuItem mControl;
    private System.Windows.Forms.ToolStripMenuItem mControlAbort;
    private System.Windows.Forms.ToolStripMenuItem mFileExit;
    private System.Windows.Forms.ToolStripMenuItem mEdit;
    private System.Windows.Forms.ToolStripMenuItem mControlSchedule;
    private System.Windows.Forms.ToolStripMenuItem mControlQueue;
    private System.Windows.Forms.ToolStripMenuItem mControlDequeue;
    private System.Windows.Forms.ToolStripMenuItem mControlUnschedule;
    private System.Windows.Forms.ToolStripMenuItem mFileRevert;
    public System.Windows.Forms.TextBox logBox;
    private System.Windows.Forms.Splitter splitter2;
    private System.Windows.Forms.ToolStripMenuItem mEditCut;
    private System.Windows.Forms.ToolStripMenuItem mEditCopy;
    private System.Windows.Forms.ToolStripMenuItem mEditPaste;
    private System.Windows.Forms.ToolStripMenuItem mEditUndo;
    private System.Windows.Forms.ToolStripMenuItem mControlCheckInputFile;
    private System.Windows.Forms.ToolStripMenuItem menuItem9;
    private System.Windows.Forms.ToolStripMenuItem mEditSelectAll;
    private System.Windows.Forms.ContextMenuStrip CMJob;
    private System.Windows.Forms.ToolStripMenuItem cmControlQueue;
    private System.Windows.Forms.ToolStripMenuItem cmControlDequeue;
    private System.Windows.Forms.ToolStripMenuItem cmControlAbort;
    private System.Windows.Forms.ToolStripMenuItem cmControlSchedule;
    private System.Windows.Forms.ToolStripMenuItem cmControlUnschedule;
    private System.Windows.Forms.ToolStripMenuItem cmControlCheckInput;
    private System.Windows.Forms.ToolBar tBar;
    private System.Windows.Forms.ToolStripMenuItem mControlPrefetch;
    private System.Windows.Forms.ToolStripMenuItem cmControlPrefetch;
    private Scheduler sched, netsched;
    private System.Windows.Forms.ToolStripMenuItem mHelp;
    private System.Windows.Forms.ToolStripMenuItem mHelpRemoteJobs;
    private System.Windows.Forms.ToolStripMenuItem cmControlRemoveEntry;
    private System.Windows.Forms.ToolStripMenuItem mControlQueueNetwork;
    private System.Windows.Forms.ToolStripMenuItem cmControlQueueNetwork;
    private System.Windows.Forms.ToolStripMenuItem mControlNetworkSchedule;
    private System.Windows.Forms.ToolStripMenuItem cmControlNetworkSchedule;

    [STAThread]
    static void Main()
    {
        //Application.EnableVisualStyles();
        //Application.Run(new Houston());
        NewFormRequest += new NewFormEventHandler(Houston_NewFormRequest);
        Run(null, new StartupCheck(Verify), true);
    }

    static void Houston_NewFormRequest(NewFormEventArgs frea)
    {
        frea.form = new Houston();
    }


    public Houston()
    {
        Icon = new Icon(GetType(), "Houston.Houston.ico");
        Init();
    }

    static bool Verify()
    { // the first `true' disables `phone home'
        bool ok = true || System.Net.Dns.GetHostName() == "huiskamerq" || System.Net.Dns.GetHostName() == "qatv-pcc-03006" || System.Net.Dns.GetHostName() == "qkleinepc" || (new StartupScreen()).ShowDialog() == DialogResult.OK;
        return ok; 
    }

    void Init()
    {
        this.SB = new System.Windows.Forms.StatusBar();
        this.tabControl1 = new System.Windows.Forms.TabControl();
        this.tabInput = new System.Windows.Forms.TabPage();
        this.tabLog = new System.Windows.Forms.TabPage();
        this.splitter1 = new System.Windows.Forms.Splitter();
        this.jobslistview = new System.Windows.Forms.ListView();
        this.CMJob = new System.Windows.Forms.ContextMenuStrip();
        this.cmControlQueue = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlQueueNetwork = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlDequeue = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlPrefetch = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlAbort = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlRemoveEntry = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlSchedule = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlNetworkSchedule = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlUnschedule = new System.Windows.Forms.ToolStripMenuItem();
        this.cmControlCheckInput = new System.Windows.Forms.ToolStripMenuItem();
        this.mMenu = new System.Windows.Forms.MenuStrip();
        this.mFile = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileNew = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileOpen = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileClose = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileSave = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileRevert = new System.Windows.Forms.ToolStripMenuItem();
        this.mFileExit = new System.Windows.Forms.ToolStripMenuItem();
        this.mEdit = new System.Windows.Forms.ToolStripMenuItem();
        this.mEditUndo = new System.Windows.Forms.ToolStripMenuItem();
        this.mEditCut = new System.Windows.Forms.ToolStripMenuItem();
        this.mEditCopy = new System.Windows.Forms.ToolStripMenuItem();
        this.mEditPaste = new System.Windows.Forms.ToolStripMenuItem();
        this.mEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
        this.mControl = new ToolStripMenuItem();
        this.mControlQueue = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlQueueNetwork = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlDequeue = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlPrefetch = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlAbort = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlSchedule = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlNetworkSchedule = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlUnschedule = new System.Windows.Forms.ToolStripMenuItem();
        this.mControlCheckInputFile = new System.Windows.Forms.ToolStripMenuItem();
        this.mHelp = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItem9 = new System.Windows.Forms.ToolStripMenuItem();
        this.mHelpRemoteJobs = new System.Windows.Forms.ToolStripMenuItem();
        this.logBox = new System.Windows.Forms.TextBox();
        this.splitter2 = new System.Windows.Forms.Splitter();
        this.tBar = new System.Windows.Forms.ToolBar();
        this.tabControl1.SuspendLayout();
        this.SuspendLayout();
        // 
        // SB
        // 
        this.SB.Location = new System.Drawing.Point(0, 594);
        this.SB.Name = "SB";
        this.SB.Size = new System.Drawing.Size(840, 22);
        this.SB.TabIndex = 0;
        // 
        // tabControl1
        // 
        this.tabControl1.Controls.Add(this.tabInput);
        this.tabControl1.Controls.Add(this.tabLog);
        this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tabControl1.Location = new System.Drawing.Point(0, 199);
        this.tabControl1.Name = "tabControl1";
        this.tabControl1.SelectedIndex = 0;
        this.tabControl1.Size = new System.Drawing.Size(840, 272);
        this.tabControl1.TabIndex = 1;
        // 
        // tabInput
        // 
        this.tabInput.BackColor = System.Drawing.SystemColors.Window;
        this.tabInput.Location = new System.Drawing.Point(4, 22);
        this.tabInput.Name = "tabInput";
        this.tabInput.Size = new System.Drawing.Size(832, 246);
        this.tabInput.TabIndex = 0;
        this.tabInput.Text = "Input File";
        // 
        // tabLog
        // 
        this.tabLog.BackColor = System.Drawing.SystemColors.Window;
        this.tabLog.Location = new System.Drawing.Point(4, 22);
        this.tabLog.Name = "tabLog";
        this.tabLog.Size = new System.Drawing.Size(832, 246);
        this.tabLog.TabIndex = 1;
        this.tabLog.Text = "Log File";
        // 
        // splitter1
        // 
        this.splitter1.BackColor = System.Drawing.SystemColors.Control;
        this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.splitter1.Location = new System.Drawing.Point(0, 196);
        this.splitter1.Name = "splitter1";
        this.splitter1.Size = new System.Drawing.Size(840, 3);
        this.splitter1.TabIndex = 2;
        this.splitter1.TabStop = false;
        // 
        // jobs
        // 
        this.jobslistview.BackColor = System.Drawing.SystemColors.Window;
        this.jobslistview.ContextMenuStrip = this.CMJob;
        this.jobslistview.Dock = System.Windows.Forms.DockStyle.Fill;
        this.jobslistview.FullRowSelect = true;
        this.jobslistview.GridLines = true;
        this.jobslistview.HideSelection = false;
        this.jobslistview.Location = new System.Drawing.Point(0, 28);
        this.jobslistview.MultiSelect = false;
        this.jobslistview.Name = "jobs";
        this.jobslistview.Size = new System.Drawing.Size(840, 168);
        this.jobslistview.TabIndex = 3;
        this.jobslistview.View = System.Windows.Forms.View.Details;
        this.jobslistview.DoubleClick += new System.EventHandler(this.jobs_DoubleClick);
        this.jobslistview.SelectedIndexChanged += new System.EventHandler(this.jobs_SelectedIndexChanged);
        // 
        // CMJob
        // 
        if (Application.ProductName == "Houston")
        {
            this.CMJob.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                          this.cmControlQueue,
                                          this.cmControlQueueNetwork,
                                          this.cmControlDequeue,
                                          new ToolStripSeparator(),
                                          this.cmControlPrefetch,
                                          this.cmControlAbort,
                                          this.cmControlRemoveEntry,
                                          new ToolStripSeparator(),
                                          this.cmControlSchedule,
                                          this.cmControlNetworkSchedule,
                                          this.cmControlUnschedule,
                                          new ToolStripSeparator(),
                                          this.cmControlCheckInput});
        }
        else
        {
            this.CMJob.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                          this.cmControlQueueNetwork,
                                          this.cmControlDequeue,
                                          new ToolStripSeparator(),
                                          this.cmControlPrefetch,
                                          this.cmControlAbort,
                                          this.cmControlRemoveEntry,
                                          new ToolStripSeparator(),
                                          this.cmControlNetworkSchedule,
                                          this.cmControlUnschedule,
                                          new ToolStripSeparator(),
                                          this.cmControlCheckInput});
        }
        this.CMJob.Opened += new System.EventHandler(this.mControl_Popup);
        // 
        // cmControlQueue
        // 
        this.cmControlQueue.Text = "&Queue";
        this.cmControlQueue.Click += new System.EventHandler(this.mControlQueue_Click);
        // 
        // cmControlQueueNetwork
        // 
        this.cmControlQueueNetwork.Text = "Queue on Network";
        this.cmControlQueueNetwork.Click += new System.EventHandler(this.mControlQueueNetwork_Click);
        // 
        // cmControlDequeue
        // 
        this.cmControlDequeue.Text = "&Dequeue";
        this.cmControlDequeue.Click += new System.EventHandler(this.mControlDequeue_Click);
        // 
        // cmControlPrefetch
        // 
        this.cmControlPrefetch.Text = "&Prefetch Output";
        this.cmControlPrefetch.Click += new System.EventHandler(this.mControlPrefetch_Click);
        // 
        // cmControlAbort
        // 
        this.cmControlAbort.Text = "&Abort";
        this.cmControlAbort.Click += new System.EventHandler(this.mControlAbort_Click);
        // 
        // cmControlRemoveEntry
        // 
        this.cmControlRemoveEntry.Text = "&Remove entry";
        this.cmControlRemoveEntry.Click += new System.EventHandler(this.mFileClose_Click);
        // 
        // cmControlSchedule
        // 
        this.cmControlSchedule.Text = "&Schedule";
        this.cmControlSchedule.Click += new System.EventHandler(this.mControlSchedule_Click);
        // 
        // cmControlNetworkSchedule
        // 
        this.cmControlNetworkSchedule.Text = "Sc&hedule on Network";
        this.cmControlNetworkSchedule.Click += new System.EventHandler(this.mControlScheduleNetwork_Click);
        // 
        // cmControlUnschedule
        // 
        this.cmControlUnschedule.Text = "&Unschedule";
        this.cmControlUnschedule.Click += new System.EventHandler(this.mControlUnschedule_Click);
        // 
        // cmControlCheckInput
        // 
        this.cmControlCheckInput.Text = "&Check Input";
        this.cmControlCheckInput.Click += new System.EventHandler(this.mControlCheckInputFile_Click);
        // 
        // mMenu
        // 
        this.mMenu.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
                                      this.mFile,
                                      this.mEdit,
                                      this.mControl,
                                      HelpMenu});
        // 
        // mFile
        // 
        this.mFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                              this.mFileNew,
                                              this.mFileOpen,
                                              this.mFileClose,
                                              new ToolStripSeparator(),
                                              this.mFileSave,
                                              this.mFileSaveAs,
                                              this.mFileRevert,
                                              new ToolStripSeparator(),
                                              this.mFileExit});
        this.mFile.Text = "&File";
        this.mFile.DropDownOpening += new System.EventHandler(this.mFile_Popup);
        this.mFile.DropDownClosed += new EventHandler(mDropDownClosed);
        // 
        // mFileNew
        // 
        this.mFileNew.ShortcutKeys = Keys.Control | Keys.N;
        this.mFileNew.Text = "&New";
        this.mFileNew.Click += new System.EventHandler(this.mFileNew_Click);
        // 
        // mFileOpen
        // 

        this.mFileOpen.ShortcutKeys = Keys.Control | Keys.O;
        this.mFileOpen.Text = "&Open...";
        this.mFileOpen.Click += new System.EventHandler(this.mFileOpen_Click);
        // 
        // mFileClose
        // 
        this.mFileClose.ShortcutKeys = Keys.Control | Keys.W; ;
        this.mFileClose.Text = "&Close";
        this.mFileClose.Click += new System.EventHandler(this.mFileClose_Click);
        // 
        // mFileSave
        // 
        this.mFileSave.ShortcutKeys = Keys.Control | Keys.S;
        this.mFileSave.Text = "&Save";
        this.mFileSave.Click += new System.EventHandler(this.mFileSave_Click);
        // 
        // mFileSaveAs
        // 
        this.mFileSaveAs.Text = "Save &As...";
        this.mFileSaveAs.Click += new System.EventHandler(this.mFileSaveAs_Click);
        // 
        // mFileRevert
        // 
        this.mFileRevert.Text = "&Revert";
        this.mFileRevert.Click += new System.EventHandler(this.mFileRevert_Click);
        // 
        // mFileExit
        // 
        this.mFileExit.ShortcutKeys = Keys.Control | Keys.Q;
        this.mFileExit.Text = "E&xit";
        this.mFileExit.Click += new System.EventHandler(this.mFileExit_Click);
        // 
        // mEdit
        // 
        this.mEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                              this.mEditUndo,
                                              new ToolStripSeparator(),
                                              this.mEditCut,
                                              this.mEditCopy,
                                              this.mEditPaste,
                                              new ToolStripSeparator(),
                                              this.mEditSelectAll});
        this.mEdit.Text = "&Edit";
        this.mEdit.DropDownOpening += new System.EventHandler(this.mEdit_Popup);
        this.mEdit.DropDownClosed += new EventHandler(mDropDownClosed);
        // 
        // mEditUndo
        // 
        this.mEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
        this.mEditUndo.Text = "&Undo";
        this.mEditUndo.Click += new System.EventHandler(this.mEditUndo_Click);
        // 
        // mEditCut
        // 

        this.mEditCut.ShortcutKeys = Keys.Control | Keys.X;
        this.mEditCut.Text = "Cu&t";
        this.mEditCut.Click += new System.EventHandler(this.mEditCut_Click);
        // 
        // mEditCopy
        // 
        this.mEditCopy.ShortcutKeys = Keys.Control | Keys.C;
        this.mEditCopy.Text = "&Copy";
        this.mEditCopy.Click += new System.EventHandler(this.mEditCopy_Click);
        // 
        // mEditPaste
        // 
        this.mEditPaste.ShortcutKeys = Keys.Control | Keys.V;
        this.mEditPaste.Text = "&Paste";
        this.mEditPaste.Click += new System.EventHandler(this.mEditPaste_Click);
        // 
        // mEditSelectAll
        // 
        this.mEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
        this.mEditSelectAll.Text = "Select &All";
        this.mEditSelectAll.Click += new System.EventHandler(this.mEditSelectAll_Click);
        // 
        // mControl
        // 
        if (Application.ProductName == "Houston")
        {
            this.mControl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                     this.mControlQueue,
                                                     this.mControlQueueNetwork,
                                                     this.mControlDequeue,
                                                     new ToolStripSeparator(),
                                                     this.mControlPrefetch,
                                                     this.mControlAbort,
                                                     new ToolStripSeparator(),
                                                     this.mControlSchedule,
                                                     this.mControlNetworkSchedule,
                                                     this.mControlUnschedule,
                                                     new ToolStripSeparator(),
                                                     this.mControlCheckInputFile});
        }
        else
        {
            this.mControl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                     this.mControlQueueNetwork,
                                                     this.mControlDequeue,
                                                     new ToolStripSeparator(),
                                                     this.mControlPrefetch,
                                                     this.mControlAbort,
                                                     new ToolStripSeparator(),
                                                     this.mControlNetworkSchedule,
                                                     this.mControlUnschedule,
                                                     new ToolStripSeparator(),
                                                     this.mControlCheckInputFile});
        }
        this.mControl.Text = "&Control";
        this.mControl.DropDownOpening += new System.EventHandler(this.mControl_Popup);
        this.mControl.DropDownClosed += new EventHandler(mDropDownClosed);
        // 
        // mControlQueue
        // 
        this.mControlQueue.ShortcutKeys = Keys.F5;
        this.mControlQueue.Text = "&Queue for Immediate Start";
        this.mControlQueue.Click += new System.EventHandler(this.mControlQueue_Click);
        // 
        // mControlQueueNetwork
        // 
        this.mControlQueueNetwork.ShortcutKeys = Keys.F6;
        this.mControlQueueNetwork.Text = "Queue for Immediate &Network Start";
        this.mControlQueueNetwork.Click += new System.EventHandler(this.mControlQueueNetwork_Click);
        // 
        // mControlDequeue
        // 
        this.mControlDequeue.ShortcutKeys = Keys.F4;
        this.mControlDequeue.Text = "&Dequeue";
        this.mControlDequeue.Click += new System.EventHandler(this.mControlDequeue_Click);
        // 
        // mControlPrefetch
        // 
        this.mControlPrefetch.Text = "&Prefetch Output";
        this.mControlPrefetch.Click += new System.EventHandler(this.mControlPrefetch_Click);
        // 
        // mControlAbort
        // 
        this.mControlAbort.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F5;
        this.mControlAbort.Text = "&Abort";
        this.mControlAbort.Click += new System.EventHandler(this.mControlAbort_Click);
        // 
        // mControlSchedule
        // 
        this.mControlSchedule.ShortcutKeys = Keys.Control | Keys.F5;
        this.mControlSchedule.Text = "&Schedule Queuing...";
        this.mControlSchedule.Click += new System.EventHandler(this.mControlSchedule_Click);
        // 
        // mControlNetworkSchedule
        // 
        this.mControlNetworkSchedule.ShortcutKeys = Keys.Control | Keys.F6;
        this.mControlNetworkSchedule.Text = "Sc&hedule Network Queuing...";
        this.mControlNetworkSchedule.Click += new System.EventHandler(this.mControlScheduleNetwork_Click);
        // 
        // mControlUnschedule
        // 
        this.mControlUnschedule.ShortcutKeys = Keys.Control | Keys.F4;
        this.mControlUnschedule.Text = "&Unschedule";
        this.mControlUnschedule.Click += new System.EventHandler(this.mControlUnschedule_Click);
        // 
        // mControlCheckInputFile
        // 
        this.mControlCheckInputFile.ShortcutKeys = Keys.Control | Keys.K;
        this.mControlCheckInputFile.Text = "&Check Input File";
        this.mControlCheckInputFile.Click += new System.EventHandler(this.mControlCheckInputFile_Click);
        // 
        // mHelp
        // 
        this.mHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
                                              this.menuItem9});
        this.mHelp.DropDownClosed += new EventHandler(mDropDownClosed);
        this.mHelp.Text = "&Help";
        // 
        // menuItem9
        // 
        this.menuItem9.Text = "A&bout...";
        this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
        // 
        // logBox
        // 
        this.logBox.AcceptsReturn = true;
        this.logBox.AcceptsTab = true;
        this.logBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.logBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.logBox.Location = new System.Drawing.Point(0, 474);
        this.logBox.Multiline = true;
        this.logBox.Name = "logBox";
        this.logBox.ReadOnly = true;
        this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.logBox.Size = new System.Drawing.Size(840, 120);
        this.logBox.TabIndex = 4;
        this.logBox.Text = "";
        // 
        // splitter2
        // 
        this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.splitter2.Location = new System.Drawing.Point(0, 471);
        this.splitter2.Name = "splitter2";
        this.splitter2.Size = new System.Drawing.Size(840, 3);
        this.splitter2.TabIndex = 5;
        this.splitter2.TabStop = false;
        // 
        // tBar
        // 
        this.tBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.tBar.ButtonSize = new System.Drawing.Size(23, 22);
        this.tBar.DropDownArrows = true;
        this.tBar.Location = new System.Drawing.Point(0, 0);
        this.tBar.Name = "tBar";
        this.tBar.ShowToolTips = true;
        this.tBar.Size = new System.Drawing.Size(840, 28);
        this.tBar.TabIndex = 6;
        // 
        // Houston
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(840, 616);
        this.Controls.Add(this.jobslistview);
        this.Controls.Add(this.splitter1);
        this.Controls.Add(this.tabControl1);
        this.Controls.Add(this.splitter2);
        this.Controls.Add(this.logBox);
        this.Controls.Add(this.SB);
        this.Controls.Add(this.tBar);
        this.Controls.Add(this.mMenu);
        this.ForeColor = System.Drawing.SystemColors.WindowText;
        this.Name = Application.ProductName;
        this.Text = Application.ProductName;
        this.tabControl1.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    void mDropDownClosed(object sender, EventArgs e)
    {
        ToolStripMenuItem menu = sender as ToolStripMenuItem;
        foreach (ToolStripItem it in menu.DropDownItems)
        {
            if (it is ToolStripMenuItem) it.Enabled = true;
        }
    }

    protected override void OnLoad(EventArgs ea)
    {
        base.OnLoad(ea);

        //if (!Verify())
        //{
        //    Close();
        //    return;
        //}
        SetupJobView();
        sched = new Scheduler(this.jobslistview);
        netsched = new Scheduler(this.jobslistview);
        logBox.SelectionStart = 0;
        logBox.AppendText(string.Format("{0} this is {1} {2}\r\n", DateTime.Now, Application.ProductName, HoustonVersion));
        if (!System.Environment.OSVersion.ToString().StartsWith("Microsoft Windows NT"))
        {
            logBox.AppendText(DateTime.Now + " this program only runs on Windows NT, 2k or XP\r\n");
            mControl.Enabled = false;
        }

        DocumentName = "Control Center";
    }

    private void SetupJobView()
    {
        string[] disp = Enum.GetNames(typeof(Display));
        for (int i = 0; i < disp.Length; i++)
        {
            switch (disp[i])
            {
                case "name":
                    jobslistview.Columns.Add("Job", 150, HorizontalAlignment.Left);
                    break;
                case "start":
                    jobslistview.Columns.Add("Start", 80, HorizontalAlignment.Left);
                    break;
                case "stop":
                    jobslistview.Columns.Add("Stop", 80, HorizontalAlignment.Left);
                    break;
                case "stat":
                    jobslistview.Columns.Add("Status", 80, HorizontalAlignment.Left);
                    break;
                case "cpu":
                    jobslistview.Columns.Add("CPU", 80, HorizontalAlignment.Left);
                    break;
                case "mem":
                    jobslistview.Columns.Add("Memory", 80, HorizontalAlignment.Left);
                    break;
                case "prio":
                    jobslistview.Columns.Add("Priority", 80, HorizontalAlignment.Left);
                    break;
                case "dir":
                    jobslistview.Columns.Add("Folder", 200, HorizontalAlignment.Left);
                    break;
                case "where":
                    jobslistview.Columns.Add("Where", 80, HorizontalAlignment.Left);
                    break;
                default:
                    MessageBox.Show("Display enum item not found: " + disp[i]);
                    break;
            }
        }
    }

    private bool EnabledFileItem(ToolStripMenuItem toolStripMenuItem)
    {
        mFile_Popup(toolStripMenuItem, EventArgs.Empty);
        bool enabled = toolStripMenuItem.Enabled;
        mDropDownClosed(mFile, EventArgs.Empty);
        return enabled;
    }

    private bool EnabledEditItem(ToolStripMenuItem toolStripMenuItem)
    {
        mEdit_Popup(toolStripMenuItem, EventArgs.Empty);
        bool enabled = toolStripMenuItem.Enabled;
        mDropDownClosed(mEdit, EventArgs.Empty);
        return enabled;
    }

    private bool EnabledControlItem(ToolStripMenuItem toolStripMenuItem)
    {
        mControl_Popup(toolStripMenuItem, EventArgs.Empty);
        bool enabled = toolStripMenuItem.Enabled;
        mDropDownClosed(mControl, EventArgs.Empty);
        return enabled;
    }

    private void mFile_Popup(object sender, System.EventArgs e)
    {
        mFileNew.Enabled = true;
        mFileOpen.Enabled = true;
        mFileClose.Enabled = false;
        mFileSave.Enabled = false;
        mFileSaveAs.Enabled = false;
        mFileRevert.Enabled = false;
        mFileExit.Enabled = true;

        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            mFileClose.Enabled = true;
            if (je.InputChanged() || je._inputFile.Untitled)
                mFileSave.Enabled = true;
            mFileSaveAs.Enabled = true;
            mFileRevert.Enabled = je.InputChanged() && !je._inputFile.Untitled;
        }
    }

    private void mFileNew_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        JobEntry je = new JobEntry(this, tabControl1, tabInput, tabLog);
        jobslistview.BeginUpdate();
        jobslistview.Items.Add(je);
        if (je.AddNewJob())
        {
            je.SetStatus(Status.idle);
            this.tabControl1.SelectedTab = tabInput;
            je.Selected = true;
        }
        else
        {
            jobslistview.Items.Remove(je);
        }
        jobslistview.EndUpdate();
    }

    //private void mFileOpen_Click(object sender, System.EventArgs e)
    //{
    //    JobEntry je = new JobEntry(this, tabControl1, tabInput, tabLog);
    //    jobs.BeginUpdate();
    //    jobs.Items.Add(je);
    //    if (je.OpenInputFile())
    //    {
    //        je.SetStatus(Status.idle);
    //        this.tabControl1.SelectedTab = tabInput;
    //        je.Selected = true;
    //    }
    //    else
    //    {
    //        jobs.Items.Remove(je);
    //    }
    //    jobs.EndUpdate();
    //}

    private void mFileOpen_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Input Files (*.in,*.txt)|*.in;*.txt|All Files (*.*)|*.*";
        ofd.Multiselect = true;
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            foreach (string fn in ofd.FileNames)
            {
                JobEntry je = new JobEntry(this, tabControl1, tabInput, tabLog);
                jobslistview.BeginUpdate();
                jobslistview.Items.Add(je);
                if (je.OpenInputFile(fn))
                {
                    je.SetStatus(Status.idle);
                    this.tabControl1.SelectedTab = tabInput;
                    je.Selected = true;
                }
                else
                {
                    jobslistview.Items.Remove(je);
                }
                jobslistview.EndUpdate();
            }
        }
        ofd.Dispose();
    }

    private void mFileClose_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        int idx = jobslistview.Items.IndexOf(je);
        if (je.stat == Status.running)
        {
            MessageBox.Show("Cannot close a running job.", "Job Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }
        if (je._inputFile.CloseWindow(je.InputChanged()))
        {
            je.WriteToLog("removed from the job list");
            tabInput.Text = tabInput.Text.TrimEnd(new char[] { ' ', '*' });
            jobslistview.Items.Remove(je);
            //je._fileWatch -= FileChanged_Handler;
            je._fileWatch.Dispose();
            je.Dispose();

            if (jobslistview.Items.Count > 0)
            {
                if (jobslistview.Items.Count >= idx) idx = jobslistview.Items.Count - 1;
                jobslistview.Items[idx].Selected = true;
            }
        }
    }

    private void mFileSave_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        je.SaveInputFile();

        if (je.stat == Status.running)
        {
            MessageBox.Show("Your changes are saved.\n\nHowever, the changes won't affect the running job.", "Job Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
    }

    private void mFileRevert_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        ((JobEntry)jobslistview.SelectedItems[0])._inputFile.Revert(true);
    }

    private void mFileExit_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        Close();
    }

    private void mFileSaveAs_Click(object sender, System.EventArgs e)
    {
        if (!EnabledFileItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];

        je._inputFile.SaveFileAs("Save As");
        if (je.stat == Status.running)
        {
            MessageBox.Show("The operation is completed.\n\nHowever, your changes won't affect the running job.", "Job Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
    }

    private void mEdit_Popup(object sender, System.EventArgs e)
    {
        mEditCut.Enabled = mEditCopy.Enabled = mEditPaste.Enabled = mEditSelectAll.Enabled = mEditUndo.Enabled = false;


        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];

            mEditUndo.Enabled = je._input.Focused && je._input.CanUndo;
            mEditPaste.Enabled = je._input.Focused && Clipboard.GetDataObject().GetDataPresent(DataFormats.Text);

            if (je.selectedText_input())
                mEditCut.Enabled = mEditCopy.Enabled = true;
            else if (je.selectedText_log())
                mEditCopy.Enabled = true;

            if (!jobslistview.Focused) mEditSelectAll.Enabled = true;
        }

        if (logBox.Focused)
        {
            if (logBox.SelectionLength > 0)
                mEditCopy.Enabled = true;
            mEditSelectAll.Enabled = true;
        }
    }

    private void mEditUndo_Click(object sender, System.EventArgs e)
    {
        if (!EnabledEditItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        je._input.Undo();
    }

    private void mEditCut_Click(object sender, System.EventArgs e)
    {
        if (!EnabledEditItem(sender as ToolStripMenuItem)) return;

        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            if (je.selectedText_input())
                je._input.Cut();
        }
    }

    private void mEditCopy_Click(object sender, System.EventArgs e)
    {
        if (!EnabledEditItem(sender as ToolStripMenuItem)) return;

        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            if (je.selectedText_input())
                je._input.Copy();
            else if (je.selectedText_log())
                je._log.Copy();
        }

        if (logBox.Focused && logBox.SelectionLength > 0)
            logBox.Copy();

    }

    private void mEditPaste_Click(object sender, System.EventArgs e)
    {
        if (!EnabledEditItem(sender as ToolStripMenuItem)) return;

        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            if (je._input.Focused) je._input.Paste();
            else if (je._log.Focused) je._log.Paste();
        }

        if (logBox.Focused) logBox.Paste();
    }

    private void mEditSelectAll_Click(object sender, System.EventArgs e)
    {
        if (!EnabledEditItem(sender as ToolStripMenuItem)) return;

        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            if (je._input.Focused)
                je._input.SelectAll();
            else if (je._log.Focused)
                je._log.SelectAll();
        }

        if (logBox.Focused)
            logBox.SelectAll();
    }

    private void mControl_Popup(object sender, EventArgs e)
    {
        mControlQueue.Enabled = cmControlQueue.Enabled = false;
        mControlQueueNetwork.Enabled = cmControlQueueNetwork.Enabled = false;
        mControlPrefetch.Enabled = cmControlPrefetch.Enabled = false;
        mControlAbort.Enabled = cmControlAbort.Enabled = false;
        mControlSchedule.Enabled = cmControlSchedule.Enabled = false;
        mControlNetworkSchedule.Enabled = cmControlNetworkSchedule.Enabled = false;
        mControlCheckInputFile.Enabled = cmControlCheckInput.Enabled = false;
        mControlDequeue.Enabled = cmControlDequeue.Enabled = false;
        mControlUnschedule.Enabled = cmControlUnschedule.Enabled = false;
        this.cmControlRemoveEntry.Enabled = false;

        //if (DateTime.Compare(DateTime.Now, new DateTime(2006, 12, 15)) > 0)
        //{
        //    logBox.AppendText(DateTime.Now + " this version will expired soon\r\n" + DateTime.Now + " please apply for an update\r\n");
        //    //mFile.Enabled = false;
        //}

        //if (DateTime.Compare(DateTime.Now, new DateTime(2007, 1, 15)) > 0)
        //{
        //    logBox.AppendText(DateTime.Now + " this version has expired\r\n" + DateTime.Now + " please apply for an update\r\n");
        //    //mFile.Enabled = false;
        //}
        //else
        if (jobslistview.SelectedIndices.Count == 1)
        {
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            mControlQueue.Enabled = cmControlQueue.Enabled = je.stat == Status.idle || je.stat == Status.aborted || je.stat == Status.completed;
            mControlQueueNetwork.Enabled = cmControlQueueNetwork.Enabled = je.stat == Status.idle || je.stat == Status.aborted || je.stat == Status.completed;
            mControlDequeue.Enabled = cmControlDequeue.Enabled = je.stat == Status.queued || je.stat == Status.rqueued;
            mControlPrefetch.Enabled = cmControlPrefetch.Enabled = je.stat == Status.running && je._job == null;
            mControlAbort.Enabled = cmControlAbort.Enabled = je.stat == Status.running;
            mControlSchedule.Enabled = cmControlSchedule.Enabled = je.stat == Status.idle || je.stat == Status.aborted || je.stat == Status.completed;
            mControlUnschedule.Enabled = cmControlUnschedule.Enabled = je.stat == Status.scheduled || je.stat == Status.rscheduled;
            mControlCheckInputFile.Enabled = cmControlCheckInput.Enabled = je.stat != Status.running;
            cmControlRemoveEntry.Enabled = je.stat != Status.running;
        }
    }

    private void mControlQueue_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if (je._inputFile.SaveFile(je.InputChanged()))
        {
            je.SetStatus(Status.queued);
            je.SubItems[(int)Display.where].Text = "Locally";
        }
    }


    private void mControlQueueNetwork_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;
        JobDestinationDlg jobdestination = null;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if (je._inputFile.SaveFile(je.InputChanged()))
        {
            if (jobdestination == null)
            {
                jobdestination = new JobDestinationDlg();
            }
            if (jobdestination.ShowDialog(this) == DialogResult.OK)
            {
                if (jobdestination.lvHosts.SelectedItems.Count > 0)
                { //dit zou ook weg kunnen en de info alleen uit cbxHosts.text halen.
                    je.remotehost = jobdestination.lvHosts.SelectedItems[0].Text;
                }
                else
                {
                    je.remotehost = jobdestination.cbxHosts.Text;
                    jobdestination.UpdateList();
                }
                je.remoteport = (int)jobdestination.nudPortnr.Value;
                je.SetStatus(Status.rqueued);
                je.SubItems[(int)Display.where].Text = je.remotehost;
            }
            jobdestination.Close();
        }
    }

    private void mControlDequeue_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        ((JobEntry)jobslistview.SelectedItems[0]).SetStatus(Status.idle);
    }

    private void mControlPrefetch_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        je.WriteToLog("prefetching");
        je._net.Download(je.DirectoryName);
        je.WriteToLog("prefetching finished");
    }

    private void mControlAbort_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if (MessageBox.Show("Are you sure to abort `" + je.FileName + "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            this.tabControl1.SelectedTab = tabLog;
            je.AbortJob();
        }
    }

    private void mControlSchedule_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if (je._inputFile.SaveFile(je.InputChanged()))
        {
            PickTime pt = new PickTime();
            if (pt.ShowDialog() == DialogResult.OK)
            {
                je.Schedule(pt.dateTimePicker1.Value, false);
                je.SubItems[(int)Display.where].Text = "Locally";
            }
            pt.Dispose();
        }
    }

    private void mControlScheduleNetwork_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;
        JobDestinationDlg jobdestination = null;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if (je._inputFile.SaveFile(je.InputChanged()))
        {
            PickTime pt = new PickTime();
            if (pt.ShowDialog() == DialogResult.OK)
            {
                if (jobdestination == null)
                {
                    jobdestination = new JobDestinationDlg();
                }
                if (jobdestination.ShowDialog(this) == DialogResult.OK)
                {
                    if (jobdestination.lvHosts.SelectedItems.Count > 0)
                    {
                        je.remotehost = jobdestination.lvHosts.SelectedItems[0].Text;
                    }
                    else
                    {
                        je.remotehost = jobdestination.cbxHosts.Text;
                        jobdestination.UpdateList();
                    }
                    je.remoteport = (int)jobdestination.nudPortnr.Value;
                    je.Schedule(pt.dateTimePicker1.Value, true);
                    je.SubItems[(int)Display.where].Text = je.remotehost;
                }
            }
            pt.Dispose();
        }
    }

    private void mControlUnschedule_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        ((JobEntry)jobslistview.SelectedItems[0]).Unschedule();
    }

    private void mControlCheckInputFile_Click(object sender, System.EventArgs e)
    {
        if (!EnabledControlItem(sender as ToolStripMenuItem)) return;

        JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
        if ((je.stat == Status.scheduled || je.stat == Status.rscheduled) && je._schedTime.Subtract(DateTime.Now).TotalSeconds < 200)
        {
            MessageBox.Show("It is not possible to check the input file at this moment because the job is scheduled to start real soon.", "Already scheduled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (je._inputFile.SaveFile(je.InputChanged()))
            je.StartJob(true);
    }

    private void jobs_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        if (jobslistview.SelectedIndices.Count == 1)
        {
            //jobslistview.BeginUpdate();
            JobEntry je = (JobEntry)jobslistview.SelectedItems[0];
            je.Select();
            jobslistview.Select();
            //jobslistview.EndUpdate();
        }
        else // cannot occur
        {
            tabInput.Controls.Clear();
            tabLog.Controls.Clear();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        //base.OnClosing(e);
        foreach (JobEntry je in jobslistview.Items)
        {
            if (!je.CloseWindow())
            {
                e.Cancel = true;
                break;
            }
        }

        if (!e.Cancel)
        {
            foreach (JobEntry je in jobslistview.Items)
            {
                if (je.stat == Status.queued || je.stat == Status.scheduled || je.stat == Status.rqueued || je.stat == Status.rscheduled)
                {
                    if (MessageBox.Show("You have queued or scheduled jobs.\n\nDo you really want to quit?", "Queued jobs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        e.Cancel = true;
                    break;
                }
            }
        }

        if (!e.Cancel)
        {
            foreach (JobEntry je in jobslistview.Items)
            {
                if (je.stat == Status.running || je.stat == Status.waiting)
                {
                    if (MessageBox.Show("You have running jobs.\n\nDo you really want to quit?", "Running jobs", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                        e.Cancel = true;
                    break;
                }
            }
        }

        if (!e.Cancel)
        {
            ExitSplash splash = new ExitSplash();
            splash.StartPosition = FormStartPosition.CenterScreen;
            splash.Show(this);
            Application.DoEvents();

            foreach (JobEntry je in jobslistview.Items)
            { // eerst alle gequeue-de jobs cancelen
                switch (je.stat)
                {
                    case Status.running:
                    case Status.waiting:
                        break;
                    default:
                        je.SetStatus(Status.idle);
                        break;
                }
            }

            foreach (JobEntry je in jobslistview.Items)
            { // dan de draaiende jobs stoppen
                switch (je.stat)
                {
                    case Status.running:
                    case Status.waiting:
                        je.AbortJob();
                        break;
                    default:
                        break;
                }
            }

            foreach (JobEntry je in jobslistview.Items)
            { // wachten totdat alle netwerkjobs klaar zijn (alles gedownload etc)
                while (je.RunAndDownloadIsActive)
                {
                    Application.DoEvents(); //nodig om dead-locks te voorkomen (vanwegen invoked calls als SetSubItem)
                    System.Threading.Thread.Sleep(10);
                }

                je.Dispose();
            }

            if (sched != null) sched.Dispose();
            System.Threading.Thread.Sleep(500);
            splash.Hide();
            splash.Dispose();
        }
    }

    private void menuItem9_Click(object sender, System.EventArgs e)
    {
        if (!(sender as ToolStripMenuItem).Enabled) return;

        MessageBox.Show("Houston\n\n\n(c) 2003-2015 Dullware\n\nhttps://github.com/dullware\n\n", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void jobs_DoubleClick(object sender, System.EventArgs e)
    {
        this.tabControl1.SelectedTab = tabInput;
    }

    public void AddtoRegistry(string remotehost, int remoteport)
    {
        int maxsavedhosts = 6;
        int alreadypresentat = -1;
        RegistryKey reghouston11 = Registry.CurrentUser.OpenSubKey(regKeyString, true);
        if (reghouston11 == null) reghouston11 = Registry.CurrentUser.CreateSubKey(regKeyString);

        RegistryKey reghosts = reghouston11.OpenSubKey("Hosts", true);
        if (reghosts == null) reghosts = reghouston11.CreateSubKey("Hosts");

        string[] hosts = reghosts.GetValueNames();
        for (int i = 0; i < maxsavedhosts && i < hosts.Length; i++)
        {
            if (string.Compare(remotehost, reghosts.GetValue(hosts[i]).ToString(), true) == 0)
            {
                alreadypresentat = i;
                break;
            }
        }

        if (alreadypresentat == -1)
        {
            if (hosts.Length < maxsavedhosts)
            {
                for (int i = hosts.Length; i > 0; i--)
                {
                    reghosts.SetValue("host" + i.ToString(), reghosts.GetValue(hosts[i - 1]).ToString());
                }
            }
            else
            {
                for (int i = maxsavedhosts - 1; i > 0; i--)
                {
                    reghosts.SetValue(hosts[i], reghosts.GetValue(hosts[i - 1]).ToString());
                }
            }
        }
        else
        {
            for (int i = alreadypresentat; i > 0; i--)
            {
                reghosts.SetValue(hosts[i], reghosts.GetValue(hosts[i - 1]).ToString());
            }
        }
        reghosts.SetValue("host0", remotehost);


        for (int i = maxsavedhosts; i < hosts.Length; i++) reghosts.DeleteValue(hosts[i]);

        reghosts.Close();

        reghouston11.SetValue("lastport", remoteport);
        reghouston11.Close();
    }

    delegate void SetStatusBarTextDelegate(string s);
    public void SetStatusBarText(string s)
    {
        if (InvokeRequired) Invoke(new SetStatusBarTextDelegate(SetStatusBarText), new object[] { s });
        else SB.Text = s;
    }
}
