using System;
using System.Net;
using System.Net.Sockets;

public class udp
{
    private const string mcastIP = "224.100.0.11";
    private const int port = 7040;
    private Socket sock;
    private IPEndPoint iep0;
    private IPEndPoint iep;
    private IPEndPoint sender;
    private EndPoint tmpRemote;

    public udp()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
        iep0 = new IPEndPoint(IPAddress.Any, port);
        iep = new IPEndPoint(IPAddress.Parse(mcastIP), port);

        sock.Bind(iep0);
        sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
            new MulticastOption(IPAddress.Parse(mcastIP)));
        sock.SetSocketOption(SocketOptionLevel.IP,
            SocketOptionName.MulticastTimeToLive, 50);

        sender = new IPEndPoint(IPAddress.Any, 0);
        tmpRemote = (EndPoint)sender;
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
    }

    public void Close()
    {
        sock.Close();
    }

    public void SendDatagram(string s)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
        sock.SendTo(data, iep);
    }

    public string GetDatagram()
    {
        int recv;
        byte[] data = new byte[1024];

        try
        {
            recv = sock.ReceiveFrom(data, ref tmpRemote);
        }
        catch (System.Net.Sockets.SocketException)
        {
            return null;
        }
        return System.Text.Encoding.ASCII.GetString(data, 0, recv);
    }

    public string Getipaddress()
    {
        return ((IPEndPoint)tmpRemote).Address.ToString();
    }
}
