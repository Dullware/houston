using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

public class JobDestinationDlg : System.Windows.Forms.Form
{
    delegate void SetStatusBarTextDelegate(string s);
    delegate void ResetListViewDelegate();
    delegate void PopulateListViewDelegate(string[] sa);

    public System.Threading.Timer timer;
    private System.Threading.TimerCallback timerCallback;
    private udp udp;

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.ComboBox cbxHosts;
    private System.Windows.Forms.Label lbHost;
    public System.Windows.Forms.NumericUpDown nudPortnr;
    private System.Windows.Forms.Label lbPort;
    private System.Windows.Forms.CheckBox cbxAdvanced;
    public System.Windows.Forms.ListView lvHosts;
    private System.Windows.Forms.StatusBar statusBar1;
    private System.Windows.Forms.ColumnHeader Host;
    private System.Windows.Forms.ColumnHeader ncpu;
    private System.Windows.Forms.ColumnHeader avgLoad;
    private System.Windows.Forms.ColumnHeader Programs;
    object locker = new object();

    public JobDestinationDlg()
    {
        Init();

        PopulateCombobox();
        //udp = new udp();
        timerCallback = new System.Threading.TimerCallback(TimerTick);
        timer = new System.Threading.Timer(timerCallback, this, Timeout.Infinite, 0);
    }

    private void Init()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(JobDestinationDlg));
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.cbxHosts = new System.Windows.Forms.ComboBox();
        this.lbHost = new System.Windows.Forms.Label();
        this.nudPortnr = new System.Windows.Forms.NumericUpDown();
        this.lbPort = new System.Windows.Forms.Label();
        this.cbxAdvanced = new System.Windows.Forms.CheckBox();
        this.lvHosts = new System.Windows.Forms.ListView();
        this.Host = new System.Windows.Forms.ColumnHeader();
        this.ncpu = new System.Windows.Forms.ColumnHeader();
        this.avgLoad = new System.Windows.Forms.ColumnHeader();
        this.Programs = new System.Windows.Forms.ColumnHeader();
        this.statusBar1 = new System.Windows.Forms.StatusBar();
        ((System.ComponentModel.ISupportInitialize)(this.nudPortnr)).BeginInit();
        this.SuspendLayout();
        // 
        // btnOK
        // 
        this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btnOK.Location = new System.Drawing.Point(264+100, 176);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(72, 24);
        this.btnOK.TabIndex = 0;
        this.btnOK.Text = "OK";
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(352+100, 176);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(72, 24);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        // 
        // cbxHosts
        // 
        this.cbxHosts.Location = new System.Drawing.Point(8, 24);
        this.cbxHosts.Name = "cbxHosts";
        this.cbxHosts.Size = new System.Drawing.Size(120, 21);
        this.cbxHosts.TabIndex = 3;
        this.cbxHosts.Click += new System.EventHandler(this.cbxHosts_Click);
        // 
        // lbHost
        // 
        this.lbHost.Location = new System.Drawing.Point(8, 8);
        this.lbHost.Name = "lbHost";
        this.lbHost.Size = new System.Drawing.Size(104, 24);
        this.lbHost.TabIndex = 4;
        this.lbHost.Text = "Remote host";
        // 
        // nudPortnr
        // 
        this.nudPortnr.Enabled = false;
        this.nudPortnr.Location = new System.Drawing.Point(8, 72);
        this.nudPortnr.Maximum = new System.Decimal(new int[] {
																	  65535,
																	  0,
																	  0,
																	  0});
        this.nudPortnr.Minimum = new System.Decimal(new int[] {
																	  1024,
																	  0,
																	  0,
																	  0});
        this.nudPortnr.Name = "nudPortnr";
        this.nudPortnr.TabIndex = 5;
        this.nudPortnr.Value = new System.Decimal(new int[] {
																	7040,
																	0,
																	0,
																	0});
        // 
        // lbPort
        // 
        this.lbPort.Enabled = false;
        this.lbPort.Location = new System.Drawing.Point(8, 56);
        this.lbPort.Name = "lbPort";
        this.lbPort.Size = new System.Drawing.Size(104, 24);
        this.lbPort.TabIndex = 6;
        this.lbPort.Text = "Port number";
        // 
        // cbxAdvanced
        // 
        this.cbxAdvanced.Location = new System.Drawing.Point(8, 112);
        this.cbxAdvanced.Name = "cbxAdvanced";
        this.cbxAdvanced.TabIndex = 7;
        this.cbxAdvanced.Text = "Advanced";
        this.cbxAdvanced.CheckedChanged += new System.EventHandler(this.cbxAdvanced_CheckedChanged);
        // 
        // lvHosts
        // 
        this.lvHosts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.Host,
																					  this.ncpu,
																					  this.avgLoad,
        this.Programs});
        this.lvHosts.FullRowSelect = true;
        this.lvHosts.GridLines = true;
        this.lvHosts.Location = new System.Drawing.Point(160, 0);
        this.lvHosts.MultiSelect = false;
        this.lvHosts.Name = "lvHosts";
        this.lvHosts.Size = new System.Drawing.Size(272+100, 160);
        this.lvHosts.TabIndex = 8;
        this.lvHosts.View = System.Windows.Forms.View.Details;
        this.lvHosts.SelectedIndexChanged += new System.EventHandler(this.lvHosts_SelectedIndexChanged);
        // 
        // Host
        // 
        this.Host.Text = "Host";
        this.Host.Width = 148;
        // 
        // ncpu
        // 
        this.ncpu.Text = "#CPU\'s";
        // 
        // avgLoad
        // 
        this.avgLoad.Text = "Load";
        this.Programs.Text = "Features";
        Programs.Width = 200;
        // 
        // statusBar1
        // 
        this.statusBar1.Location = new System.Drawing.Point(0, 210);
        this.statusBar1.Name = "statusBar1";
        this.statusBar1.Size = new System.Drawing.Size(434, 22);
        this.statusBar1.SizingGrip = false;
        this.statusBar1.TabIndex = 9;
        this.statusBar1.Text = "statusBar1";
        // 
        // JobDestinationDlg
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(434+100, 232);
        this.ControlBox = false;
        this.Controls.Add(this.statusBar1);
        this.Controls.Add(this.lvHosts);
        this.Controls.Add(this.cbxAdvanced);
        this.Controls.Add(this.nudPortnr);
        this.Controls.Add(this.cbxHosts);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.lbHost);
        this.Controls.Add(this.lbPort);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "JobDestinationDlg";
        this.ShowInTaskbar = false;
        this.Text = "Choose an SFB server";
        this.Activated += new System.EventHandler(this.JobDestinationDlg_Activated);
        this.Deactivate += new System.EventHandler(this.JobDestinationDlg_Deactivate);
        ((System.ComponentModel.ISupportInitialize)(this.nudPortnr)).EndInit();
        this.ResumeLayout(false);

    }

    private void cbxAdvanced_CheckedChanged(object sender, System.EventArgs e)
    {
        this.lbPort.Enabled = ((CheckBox)sender).Checked;
        this.nudPortnr.Enabled = ((CheckBox)sender).Checked;
    }

    private void PopulateCombobox()
    {
        RegistryKey reghouston11 = Registry.CurrentUser.OpenSubKey(Houston.regKeyString, true);
        if (reghouston11 == null) reghouston11 = Registry.CurrentUser.CreateSubKey(Houston.regKeyString);

        RegistryKey reghosts = reghouston11.OpenSubKey("Hosts", true);
        if (reghosts == null) reghosts = reghouston11.CreateSubKey("Hosts");

        List<string> hosts = new List<string>(reghosts.GetValueNames());
        hosts.Sort();

        foreach (string host in hosts)
        {
            if (!cbxHosts.Items.Contains(reghosts.GetValue(host).ToString()))
                cbxHosts.Items.Add(reghosts.GetValue(host).ToString());
        }
        if (cbxHosts.Items.Count > 0) cbxHosts.Text = cbxHosts.Items[0].ToString();

        reghosts.Close();
        reghouston11.Close();
    }

    public void UpdateList()
    {
        int i;
        for (i = 0; i < cbxHosts.Items.Count; i++)
        {
            if (string.Compare(cbxHosts.Items[i].ToString(), cbxHosts.Text, true) == 0) break;
        }
        if (i == cbxHosts.Items.Count)
        {
            cbxHosts.Items.Insert(0, cbxHosts.Text);
            cbxHosts.SelectedIndex = 0;
        }
    }

    private void JobDestinationDlg_Activated(object sender, System.EventArgs e)
    {
        //timer.Change(polling ? 3000 : 0,30000);
    	udp = new udp();
        timer.Change(0, 0); // One time and only one time. Runs in a different thread.
        //TimerTick(this);
        //Console.WriteLine("poll");
        //if (!polling) TimerTick(this); // One time.
    }

    private void JobDestinationDlg_Deactivate(object sender, System.EventArgs e)
    {
    	lock(locker)
    	{
    		udp.Close();
    		udp=null;
    	}
    }

    private static void TimerTick(Object obj)
    {
    	JobDestinationDlg rjo = (JobDestinationDlg)obj;
    	rjo.SetStatusBarText("Polling for sfb servers...");

    	if ( rjo.udp != null )
    	{
    		lock(rjo.locker)
    		{
    			rjo.udp.SendDatagram(Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "SystemLoad?");
    		}
    	}
    	else return;

    	rjo.ResetListView();
    	int i = 0;
    	rjo.SetStatusBarText(i.ToString() + " sfb server" + (i != 1 ? "s" : "") + " found.");
    	for (; ; )
    	{
    		string answ;
    		string[] splittedansw;
    		if ( rjo.udp != null ){
    			try
    			{
    				answ = rjo.udp.GetDatagram();
    				if (answ == null) break;
    			}
    			catch
    			{
    				break;
    			}
    		}
    		else break;
    		
    		splittedansw = answ.Split(new Char[] { '\t' });
    		if (splittedansw.Length > 2 && splittedansw[1] == "SystemLoad")
    		{
    			i++;
    			rjo.SetStatusBarText(i.ToString() + " sfb server" + (i != 1 ? "s" : "") + " found.");
    			rjo.PopulateListView(splittedansw);
    		}
    	}
    }

    public void SetStatusBarText(string s)
    {
        if (!statusBar1.InvokeRequired)
        {
            statusBar1.Text = s;
        }
        else //We are on a non GUI thread.
        {
            SetStatusBarTextDelegate ssbtDel = new SetStatusBarTextDelegate(SetStatusBarText);
            statusBar1.Invoke(ssbtDel, new object[] { s });
        }
    }

    public void ResetListView()
    {
        if (!lvHosts.InvokeRequired)
        {
            lvHosts.Items.Clear();
        }
        else //We are on a non GUI thread.
        {
            ResetListViewDelegate rlvDel = new ResetListViewDelegate(ResetListView);
            lvHosts.Invoke(rlvDel);
        }
    }

    public void PopulateListView(string[] sa)
    {
        if (!lvHosts.InvokeRequired)
        {
            ListViewItem lvi = lvHosts.Items.Add(sa[0]);
            lvi.SubItems.Add(sa[2]);
            lvi.SubItems.Add(sa[3]);
            if ( sa.Length>4) lvi.SubItems.Add(sa[4]);
            lvHosts.Sort();
        }
        else //We are on a non GUI thread.
        {
            PopulateListViewDelegate plvDel = new PopulateListViewDelegate(PopulateListView);
            lvHosts.Invoke(plvDel, new object[] { sa });
        }
    }

    private void cbxHosts_Click(object sender, System.EventArgs e)
    {
        if (this.lvHosts.SelectedItems.Count > 0)
        {
            this.lvHosts.SelectedItems[0].Selected = false;
        }
    }

    private void lvHosts_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        if (lvHosts.SelectedItems.Count == 1)
            cbxHosts.Text = lvHosts.SelectedItems[0].Text;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        //udp.Close();
        base.OnFormClosed(e);
    }
}
