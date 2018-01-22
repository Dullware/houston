using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class ButtonOK : Button
{
    public void MyClick()
    {
        OnClick(new EventArgs());
    }
}

public class StartupScreen : System.Windows.Forms.Form
{
	Dullware.UDPVerify.udp udp = null;
    private ButtonOK btOK;
    private System.Windows.Forms.Button btQUIT;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label result;

    delegate void SetResultTextDelegate(string s);
    delegate void SetOKButtonDelegate(bool s);

    public StartupScreen()
    {
        Init();

        Thread verify = new Thread(new ThreadStart(dotcp));
        verify.Start();
    }

    public void SetResultText(string s)
    {
        if (!result.InvokeRequired)
        {
            result.Text = s;
        }
        else //We are on a non GUI thread.
        {
            SetResultTextDelegate ssbtDel = new SetResultTextDelegate(SetResultText);
            result.Invoke(ssbtDel, new object[] { s });
        }
    }

    public void SetOKButton(bool s)
    {
        if (!result.InvokeRequired)
        {
            btOK.Enabled = s;
            if (btOK.Enabled)
            {
                btOK.Select();
                btOK.MyClick();
            }
        }
        else //We are on a non GUI thread.
        {
            SetOKButtonDelegate ssbtDel = new SetOKButtonDelegate(SetOKButton);
            btOK.Invoke(ssbtDel, new object[] { s });
        }
    }
    
    void dotcp()
    {
    	bool found = true;
        bool passed = true;
		string[] hosts = { };
        foreach (string host in hosts)
        {
        	TcpClient srv;
        	StreamReader sr;
        	StreamWriter sw;
        	
        	try
        	{
        		srv = new TcpClient(host, 7040);
        		NetworkStream stream = srv.GetStream();
        		sr = new StreamReader(stream, System.Text.Encoding.Default);
        		sw = new StreamWriter(stream);
        		sw.AutoFlush = true;
       		}
        	catch
        	{
        		continue;
        	}
        	
         	found = true;
        	SetResultText("Unknown error.");
        	sr.ReadLine();
        	
            if (Application.ProductName == "Houston")
                sw.WriteLine("VALIDATE " + Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "Houston" + "\t7");
            else
                sw.WriteLine("VALIDATE " + Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "Houston Lite" + "\t7");
            
            string answ = sr.ReadLine();
            if (answ == null) break;
            passed = answ.StartsWith("+");
            if (!passed && answ.Substring(5).Length>0 ) SetResultText(answ.Substring(5));
            else if ( passed ) SetResultText("Passed.");
            
            sw.WriteLine("QUIT");
            sr.ReadLine();
            srv.Close();
            break;
        }

        if (!found) SetResultText("Failed (no server found).\r\nBe sure to let your firewall allow us to connect to \r\n82.95.178.145 using TCP port 7040.");
        SetOKButton(passed);
    }

    void doudp()
    {
        bool passed = false;
        bool ready = false;
        string[] hosts = { "82.95.178.145", "137.224.24.210" };
        foreach (string host in hosts)
        {
            if (udp != null) udp.Close();
            udp = new Dullware.UDPVerify.udp(host);
            if (Application.ProductName == "Houston")
                udp.SendDatagram(Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "Houston" + "\t7");
            else
                udp.SendDatagram(Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "Houston Lite" + "\t7");
            for (; ; )
            {
                string[] splittedansw;
                string answ = udp.GetDatagram();
                if (answ == null) break;
                splittedansw = answ.Split(new Char[] { '\t' });
                if (splittedansw.Length > 2 && splittedansw[1] == "Houston")
                {
                    ready = true;
                    SetResultText(splittedansw[2].Substring(1));
                    passed = splittedansw[2].StartsWith("+");
                    break;
                }
            }
            if (ready) break;
        }

        if (result.Text == "Please wait...") SetResultText("Failed (no server found).\r\nBe sure to let your firewall allow us to connect to \r\n82.95.178.145 using UDP port 7040.");
        SetOKButton(passed);
    }

    private void Init()
    {
        this.btOK = new ButtonOK();
        this.btQUIT = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.result = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // btOK
        // 
        this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btOK.Enabled = false;
        this.btOK.Location = new System.Drawing.Point(336, 136);
        this.btOK.Name = "btOK";
        this.btOK.TabIndex = 0;
        this.btOK.Text = "OK";
        // 
        // btQUIT
        // 
        this.btQUIT.DialogResult = System.Windows.Forms.DialogResult.Abort;
        this.btQUIT.Location = new System.Drawing.Point(224, 136);
        this.btQUIT.Name = "btQUIT";
        this.btQUIT.TabIndex = 1;
        this.btQUIT.Text = "Quit";
        this.btQUIT.Click += new System.EventHandler(this.btQUIT_Click);
        // 
        // label1
        // 
        this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.label1.Location = new System.Drawing.Point(8, 16);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(280, 48);
        this.label1.TabIndex = 2;
        this.label1.Text = string.Format("Verifying your copy of {0}:", Application.ProductName);
        // 
        // result
        // 
        this.result.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.result.Location = new System.Drawing.Point(8, 64);
        this.result.Name = "result";
        this.result.Size = new System.Drawing.Size(424, 48);
        this.result.TabIndex = 3;
        this.result.Text = "Please wait...";
        // 
        // StartupScreen
        // 
        this.AcceptButton = this.btOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btQUIT;
        this.ClientSize = new System.Drawing.Size(426, 192);
        this.ControlBox = false;
        this.Controls.Add(this.result);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.btQUIT);
        this.Controls.Add(this.btOK);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "StartupScreen";
        this.Text = "StartupScreen";
        this.ResumeLayout(false);

    }

    private void btQUIT_Click(object sender, System.EventArgs e)
    {
        //if (!btOK.Enabled) System.Diagnostics.Process.Start("http://www.dullware.nl");
    }

	protected override void OnDeactivate(EventArgs e)
	{
		//udp.Close();
		base.OnDeactivate(e);
	}
}
