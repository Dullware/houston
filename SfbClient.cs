using System;
using System.Collections;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class SfbClient
{
    public ServerIO io, io2;
    JobEntry je;
    string pid, pincode;
    public string server_version;
    System.Globalization.NumberFormatInfo ENU = new System.Globalization.CultureInfo("en-US",false).NumberFormat;

    public SfbClient(JobEntry je)
    {
        this.je = je;
    }

    public bool ServerVersionIsAtLeast(string version)
    {
        string[] current = server_version.Split(new char[] { '.' });
        string[] needed = version.Split(new char[] { '.' });

        if (int.Parse(current[0]) > int.Parse(needed[0]))
            return true;
        else if (int.Parse(current[0]) == int.Parse(needed[0]) && int.Parse(current[1]) >= int.Parse(needed[1]))
            return true;
        else return false;
    }

    public string Connect(string host, int port, FileType ftype)
    {
        lock (this)
        {
            string image;

            io = new ServerIO();
            lock (io)
            {
                if (io.Connect(host, port) && (image = io.ReadLine()) != null)
                {
                    if (image.Substring(0, 1) == "-")
                    {
                        Disconnect();
                        return image.Substring(5);
                    }
                    else
                    {
                        string[] s = image.Substring(5).Split();
                        pid = s[0];
                        pincode = s[1];
                    }
                }
                else return "Connection to the primary server failed.\r\n\r\nHost " + host + "\r\nPort " + port.ToString();

                io.WriteLine("VERSION 0.3");
                image = io.ReadLine();
                if (image.Substring(0, 1) == "+")
                {
                    server_version = image.Substring(5);

                    io.WriteLine("HOST");
                    image = io.ReadLine();
                    if (image.Substring(0, 1) == "+")
                    {
                        je.WriteToLog("connected to " + image.Substring(5));
                        je.SubItems[(int)Display.where].Text = image.Substring(5);
                    } // else ignore the error

                    je.WriteToLog("server version " + server_version);
                }
                else
                {
                    Disconnect();
                    return image.Substring(5);
                }

                io.WriteLine(string.Format("CLIENT {0} {1}", Application.ProductName, Houston.HoustonVersion.ToString()));
                image = io.ReadLine();

                io.WriteLine("USER " + Environment.UserName);
                image = io.ReadLine();

                if (!SupportedFileType(ftype))
                {
                    Disconnect();
                    return "The required program is not installed on this server";
                }
            }

            io2 = new ServerIO();
            if (io2.Connect(host, port) && (image = io2.ReadLine()) != null)
            {
                if (image.Substring(0, 1) == "-")
                {
                    Disconnect();
                    return image.Substring(5);
                } //else ignore pid/pin
            }
            else return "Connection to secondary server failed.\r\n\r\nHost " + host + "\r\nPort " + port.ToString();

            io2.WriteLine("VERSION -0.3");
            image = io2.ReadLine();
            if (image.Substring(0, 1) == "+")
            {
            }
            else
            {
                Disconnect();
                return image.Substring(5);
            }

            io2.WriteLine("PID " + pid);
            io2.ReadLine();
            io2.WriteLine("PIN " + pincode);
            io2.ReadLine();

            return null;
        }
    }

    public bool SupportedFileType(FileType ft)
    {
        string image;

        lock (io)
        {
            if (ft == FileType.sfbox) io.WriteLine("SUPPORTS sfbox");
            else if (ft == FileType.mcrenko) io.WriteLine("SUPPORTS mcrenko");
            else if (ft == FileType.mccompile) io.WriteLine("SUPPORTS mccompile");
            else if (ft == FileType.ctb) io.WriteLine("SUPPORTS ctb");
            else io.WriteLine("SUPPORTS fakeprogram to generate an error");

            image = io.ReadLine();
            if (image.Substring(0, 1) == "+")
            {
                return image.Substring(5) == "YES";
            }
            else return false;
        }
    }

    ArrayList CopyInputFile(string from, string to)
    {
        string line;
        ArrayList inpfiles = new ArrayList();
        StreamReader SR = new StreamReader(from, System.Text.Encoding.ASCII);
        StreamWriter SW = new StreamWriter(to, false, System.Text.Encoding.ASCII);

        string odname = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(new FileInfo(from).DirectoryName);

        while ((line = SR.ReadLine()) != null)
        {
            string[] parts;

            parts = line.Split(':');
            
            if (line.StartsWith("//"))
            { //Comment line.
            	if (parts.Length == 2 && parts[0].Trim() == "//houston" && parts[1].Trim() == "recompile")
            	{
            	}
                SW.WriteLine(line);
                continue;
            }

            if (parts.Length == 4 && (parts[2].Trim() == "template" || parts[2].Trim() == "initial_guess_input_file"))
            {
                FileInfo fi = new FileInfo(parts[3].Trim());
                if (fi.Exists)
                {
                    SW.WriteLine(parts[0].Trim() + " : " + parts[1].Trim() + " : " + parts[2].Trim() + " : " + fi.Name);
                    inpfiles.Add(fi);
                }
                else SW.WriteLine(line);
            }
            else if (parts.Length == 4 && parts[2].Trim().EndsWith("_range") && parts[3].Trim().StartsWith("<") )
            {
            	FileInfo fi = new FileInfo(parts[3].Trim().Substring(1).Trim());
                if (fi.Exists)
                {
                    SW.WriteLine(parts[0].Trim() + " : " + parts[1].Trim() + " : " + parts[2].Trim() + " : <" + fi.Name);
                    inpfiles.Add(fi);
                }
                else SW.WriteLine(line);         	
            }
            else SW.WriteLine(line);
        }
        SR.Close(); SW.Close();
        Directory.SetCurrentDirectory(odname);
        return inpfiles;
    }

    public string SendFile(FileInfo fi)
    {
        io.WriteLine("PUT" + " " + fi.Length.ToString() + " " + fi.Name + "");
        io.ReadLine(); //Eat the OK
        io.SendBinary(fi.FullName);
        return fi.Name;
    }

    public string SendInputFile(string inpfile)
    {
        ArrayList moreInputFiles;
        string tmpfile = Path.GetTempFileName();
        FileInfo tmpfileinfo = new FileInfo(tmpfile);

        moreInputFiles = CopyInputFile(inpfile, tmpfile);

        lock (io)
        {
            io.locker++;
            io.WriteLine("PUT " + tmpfileinfo.Length.ToString());
            io.ReadLine(); //Eat the OK
            io.SendBinary(tmpfile);
            tmpfileinfo.Delete();

            if (je.filetype == FileType.sfbox)
            {
                foreach (FileInfo fi in moreInputFiles) SendFile(fi);
            }
            io.locker--;
        }
        return new FileInfo(inpfile).Name;
    }

    public void Run(FileType what, string fname, JobEntry je, ref string errortext)
    {
        string image;
        lock (io)
        {
            io.locker++;
            switch (what)
            {
                case FileType.sfbox:
                    io.WriteLine("RUN " + fname);
                    break;
                case FileType.mcrenko:
                    io.WriteLine("RUNMC " + fname);
                    break;
                case FileType.mccompile:
                    io.WriteLine("RUNMCCOMPILE " + fname);
                    break;
                case FileType.ctb:
                    io.WriteLine("RUNCTB " + fname);
                    break;
            }
            image = io.ReadLine();
            if (image.Substring(0, 1) == "+")
            {
                //je.timer.Change(500,2000);
                image = io.ReadLine();
                while (true)
                {
                    if (image.Length >= 13 && image.Substring(0, 13) == "+OK  Finished") break;
                    else if (image.Length == 12 && image.Substring(0, 12) == "+OK  Stopped")
                        je.SetStatus(Status.waiting);
                    else if (image.Length == 14 && image.Substring(0, 14) == "+OK  Continued")
                        je.SetStatus(Status.resumed);
                    else {
                    	//throw new DivideByZeroException("Invalid Division");
                    	try {
                    		je._theTabs.BeginInvoke(je.asyncReadCallback, new Object[] { image + "\r\n" });
                    	}
                    	catch(Exception e) {
                    		errortext = e.Message;
                    		break;
                    	}
                    }
                    image = io.ReadLine();
                }
                //je.timer.Change(0,0);
            }
            else if (image.Length > 5) errortext = image.Substring(5);
            else errortext = "Unknown error whilst running. \r\nUnknown RUN command?";
        }
    }

    public void Download(string dname)
    {
        string reply;
        ArrayList la = new ArrayList();
        lock (io2)
        {
            io2.locker++;
            io2.WriteLine("LIST");
            reply = io2.ReadLine();
            while (reply.Substring(4, 1) == "-")
            {
                la.Add(reply.Substring(5));
                reply = io2.ReadLine();
            }

            string odname = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dname);
            foreach (string fname in la)
            {
                string fsize;
                io2.WriteLine("GET " + fname);
                fsize = io2.ReadLine().Substring(5);
                je.WriteToLog("downloading \"" + fname + "\" (" + fsize + " bytes)");
                io2.GetBinary(fname, Convert.ToInt64(fsize,ENU));
            }
            Directory.SetCurrentDirectory(odname);
            io2.locker--;
        }
    }

    public void GetStatus()
    {
        string reply;

        lock (io2)
        {
            io2.locker++;
            io2.WriteLine("STAT");
            reply = io2.ReadLine();
            if (reply.Substring(0, 1) == "+")
            {
                while (reply.Substring(4, 1) == "-")
                {
                    if (reply.Substring(5, 3) == "CPU")
                        je.CPU = Convert.ToSingle(reply.Substring(9),ENU);
                    else if (reply.Substring(5, 3) == "MEM")
                        je.MEM = Convert.ToInt32(reply.Substring(9),ENU);
                    reply = io2.ReadLine();
                }
            }
            io2.locker--;
        }
    }

    public void GetFinalStatus()
    {
        string reply;

        lock (io2)
        {
            io2.locker++;
            lock (io)
            {
                io.locker++;
                io.WriteLine("STAT");
                reply = io.ReadLine();
                if (reply.Substring(0, 1) == "+")
                {
                    while (reply.Substring(4, 1) == "-")
                    {
                        if (reply.Substring(5, 3) == "CPU")
                            je.CPU = Convert.ToSingle(reply.Substring(9),ENU);
                        else if (reply.Substring(5, 3) == "MEM")
                        {
                            if (reply.Substring(9) != "0")
                                je.MEM = Convert.ToInt32(reply.Substring(9),ENU);
                        }
                        reply = io.ReadLine();
                    }
                }
                io.locker--;
            }
            io2.locker--;
        }
    }

    public void Kill()
    {
        lock (io2)
        {
            io2.locker++;
            io2.WriteLine("STOP");
            io2.ReadLine();
            io2.locker--;
        }
    }

    public ArrayList GetNews()
    {
        ArrayList al = new ArrayList();
        io.WriteLine("NEWS");
        string reply = io.ReadLine();
        if (reply.Substring(0, 1) == "+")
        {
            while (reply.Substring(4, 1) == "-")
            {
                string msg = reply.Substring(5);
                //Check if the msg starts with a [id] tag
                if (msg.StartsWith("[") && msg.IndexOf(']') != -1) al.Add(msg);
                reply = io.ReadLine();
            }
        }
        return al;
    }

    public void Disconnect()
    {
        // First close the sec. server
        if (io2 != null)
        {
            lock (io2)
            {
                io2.locker++;
                io2.WriteLine("QUIT");
                io2.ReadLine(); //Eat the OK
                io2.Disconnect();
                io2.locker--;
            }
        }
        //Then the primary
        lock (io)
        {
            io.locker++;
            io.WriteLine("QUIT");
            io.ReadLine(); //Eat the OK
            io.Disconnect();
            io.locker--;
        }
    }
}

public class ServerIO
{
    public int locker = 0;
    TcpClient srv;
    public StreamReader sr;
    StreamWriter sw;

    public bool Connect(string host, int port)
    {
        try
        {
            NetworkStream stream;
            srv = new TcpClient(host, port);
            stream = srv.GetStream();
            sr = new StreamReader(stream, System.Text.Encoding.Default);
            sw = new StreamWriter(stream);
            sw.AutoFlush = true;

            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public void Disconnect()
    {
        srv.Close();
    }

    public void SendBinary(string ifname)
    {
        char[] buf;
        long nleft; int nread;
        FileInfo rfile = new FileInfo(ifname);
        StreamReader sr = new StreamReader(rfile.OpenRead(), Encoding.ASCII);

        nleft = rfile.Length;
        while (nleft > 0)
        {
            buf = new char[4 * 1024];
            nread = sr.Read(buf, 0, buf.Length);
            sw.Write(buf, 0, nread);
            nleft -= nread;
        }
        sr.Close();
    }

    public void GetBinary(string path, long nleft)
    {
        char[] buf; int nread;
        FileInfo wfile = new FileInfo(path);
        StreamWriter sw = new StreamWriter(wfile.Create(), System.Text.Encoding.Default);

        while (nleft > 0)
        {
            buf = new char[4 * 1024];
            nread = sr.Read(buf, 0, buf.Length);
            sw.Write(buf, 0, nread);
            nleft -= nread;
        }
        sw.Close();
    }

    public string ReadLine()
    {
        return sr.ReadLine();
    }

    public void WriteLine(string str)
    {
        sw.WriteLine(str);
    }
}
