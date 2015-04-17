using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;


public class RemoteJobsOverview : System.Windows.Forms.Form
{
    delegate void SetStatusBarTextDelegate(string s);
    delegate void PopulateListViewDelegate(ArrayList al);

    public System.Threading.Timer timer;
    System.Threading.TimerCallback timerCallback;

    private System.Windows.Forms.ListView remoteJobsLV;
    private Thread startupThread;
    private System.Windows.Forms.StatusBar statusBar1;
    private ServerIO io;
    private bool connected = false;

    public RemoteJobsOverview()
    {
        Init();

        this.remoteJobsLV.Columns.Add("Server load", this.remoteJobsLV.Width, System.Windows.Forms.HorizontalAlignment.Left);
        startupThread = new Thread(new ThreadStart(ClientConnect));
        timerCallback = new System.Threading.TimerCallback(TimerTick);
        timer = new System.Threading.Timer(timerCallback, this, Timeout.Infinite, 0);
        startupThread.Start();
    }


    private void Init()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RemoteJobsOverview));
        this.remoteJobsLV = new System.Windows.Forms.ListView();
        this.statusBar1 = new System.Windows.Forms.StatusBar();
        this.SuspendLayout();
        // 
        // remoteJobsLV
        // 
        this.remoteJobsLV.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.remoteJobsLV.Dock = System.Windows.Forms.DockStyle.Fill;
        this.remoteJobsLV.GridLines = true;
        this.remoteJobsLV.Location = new System.Drawing.Point(0, 0);
        this.remoteJobsLV.Name = "remoteJobsLV";
        this.remoteJobsLV.Size = new System.Drawing.Size(304, 266);
        this.remoteJobsLV.TabIndex = 0;
        this.remoteJobsLV.View = System.Windows.Forms.View.Details;
        // 
        // statusBar1
        // 
        this.statusBar1.Location = new System.Drawing.Point(0, 244);
        this.statusBar1.Name = "statusBar1";
        this.statusBar1.Size = new System.Drawing.Size(304, 22);
        this.statusBar1.TabIndex = 1;
        this.statusBar1.Text = "statusBar1";
        // 
        // RemoteJobsOverview
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(304, 266);
        this.Controls.Add(this.statusBar1);
        this.Controls.Add(this.remoteJobsLV);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RemoteJobsOverview";
        this.ShowInTaskbar = false;
        this.Text = "Remote Uptime";
        this.ResumeLayout(false);

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

    public void PopulateListView(ArrayList al)
    {
        if (!remoteJobsLV.InvokeRequired)
        {
            remoteJobsLV.Items.Clear();
            foreach (string s in al)
            {
                remoteJobsLV.Items.Add(s);
            }
        }
        else //We are on a non GUI thread.
        {
            PopulateListViewDelegate plvDel = new PopulateListViewDelegate(PopulateListView);
            remoteJobsLV.Invoke(plvDel, new object[] { al });
        }
    }

    private IPAddress getrwhodserver()
    {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
        IPEndPoint iep0 = new IPEndPoint(IPAddress.Any, 7040);
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse("224.100.0.11"), 7040);
        byte[] data = System.Text.Encoding.ASCII.GetBytes(Environment.UserName + "@" + Dns.GetHostName() + "\t" + "CANRWHOD?");

        sock.Bind(iep0);
        sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
            new MulticastOption(IPAddress.Parse("224.100.0.11")));
        sock.SetSocketOption(SocketOptionLevel.IP,
            SocketOptionName.MulticastTimeToLive, 50);

        SetStatusBarText("Looking for a SFB rwhod server...");
        sock.SendTo(data, iep);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint tmpRemote = (EndPoint)sender;
        data = new byte[1024];
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
        try
        {
            for (; ; )
            {
                int recv = sock.ReceiveFrom(data, ref tmpRemote);
                //Console.WriteLine("Message received from {0}:", tmpRemote.ToString());
                string answer = System.Text.Encoding.ASCII.GetString(data, 0, recv);
                string[] answ = answer.Split(new Char[] { '\t' });
                if (answ.Length > 1 && answ[1] == "YESCANRWHOD")
                {
                    sock.Close();
                    return ((IPEndPoint)tmpRemote).Address;
                }
            }
        }
        catch (SocketException)
        {
            SetStatusBarText("No server found.");
            sock.Close();
            return IPAddress.None;
        }

        //sock.Close();
    }

    private void ClientConnect()
    {
        string image;

        IPAddress rwhodip = getrwhodserver();
        if (rwhodip == IPAddress.None) return;

        SetStatusBarText("Connecting to server...");
        io = new ServerIO();
        try
        {
            if (io.Connect(rwhodip.ToString(), 7040))
            {
                SetStatusBarText("Connected to the server.");

                if ((image = io.ReadLine()) != null)
                {
                    if (image.Substring(0, 1) == "+")
                    {
                        io.WriteLine("MODE ruptime");
                        image = io.ReadLine();
                        if (image.Substring(0, 1) == "+")
                        {
                            connected = true;
                            timer.Change(500, 0);
                        }
                        else
                        {
                            SetStatusBarText("The server doesnot support this feature.");

                            io.WriteLine("QUIT");
                            io.ReadLine(); //Eat the OK
                            io.Disconnect();
                        }
                    }
                    else
                    {
                        SetStatusBarText("Bad response from server: " + image.Substring(5));

                        io.WriteLine("QUIT");
                        io.ReadLine(); //Eat the OK
                        io.Disconnect();
                    }
                }
                else
                {
                    SetStatusBarText("Bad response from server.");
                    io.Disconnect();
                    Thread.Sleep(1000);
                    SetStatusBarText("Disconnected.");
                }
            }
            else SetStatusBarText("No response from server: host not found");
        }
        catch (Exception e)
        {
            SetStatusBarText("No response from server: " + e.Message);
        }
    }

    private static void TimerTick(Object obj)
    {
        RemoteJobsOverview rjo = (RemoteJobsOverview)obj;
        lock (rjo.io)
        {
            string reply;
            ArrayList la = new ArrayList();

            rjo.SetStatusBarText("refreshing info...");
            rjo.io.WriteLine("RUPTIME");
            reply = rjo.io.ReadLine();
            if (reply.Substring(0, 1) == "+")
            {
                while (reply.Substring(4, 1) == "-")
                {
                    la.Add(reply.Substring(5));
                    reply = rjo.io.ReadLine();
                }
                rjo.PopulateListView(la);
                rjo.SetStatusBarText("Connected to the server.");
            }
            else
            {
                rjo.timer.Change(Timeout.Infinite, 0);
                rjo.SetStatusBarText("Bad response from server.");
                rjo.io.WriteLine("QUIT");
                rjo.io.ReadLine(); //Eat the OK
                rjo.io.Disconnect();
                rjo.connected = false;
                Thread.Sleep(1000);
                rjo.SetStatusBarText("Disconnected.");
            }
            rjo.timer.Change(30000, 0);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (connected)
        {
            timer.Change(Timeout.Infinite, 0);
            lock (io)
            {
                io.WriteLine("QUIT");
                io.ReadLine(); //Eat the OK
                io.Disconnect();
                timer.Dispose();
            }
            connected = false;
        }
        //((Houston)Owner).rjo = null;
    }
}
