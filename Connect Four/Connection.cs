using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.Serialization.Formatters.Binary;

namespace Connection
{
    public delegate void ConnectionEventHandler();

    static class Advertizer
    {
        public static ConnectionInfo hostConnectionInfo;
        public static readonly BackgroundWorker GetAdvertizers = new BackgroundWorker() { WorkerSupportsCancellation = true};
        public static readonly BackgroundWorker Advertize = new BackgroundWorker();
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();
        private static readonly System.Timers.Timer timeout = new System.Timers.Timer(4000);

        static Advertizer()
        {
            connections = new List<ConnectionInfo>();
            GetAdvertizers.DoWork += GetAdvertizers_DoWork;
            Advertize.DoWork += Advertize_DoWork;
            timeout.Elapsed += (object sender, ElapsedEventArgs e) => { timeout.Stop(); GetAdvertizers.CancelAsync(); };
        }

        private static async void Advertize_DoWork(object sender, DoWorkEventArgs e)
        {
            UdpClient server = new UdpClient();
            server.Connect(IP.AdvertizeBroadcastRecieve);

            while (!e.Cancel)
            {
                UdpReceiveResult result = await server.ReceiveAsync();
                if (!e.Cancel)
                {
                    string request = Encoding.ASCII.GetString(result.Buffer);
                    if (request.Contains("?"))
                    {
                        new Task(WhoAmI, result.RemoteEndPoint.Address).Start();
                    }
                    else
                    {

                    }
                }
            }
        }

        private static void WhoAmI(object from)
        {
            IPEndPoint to = new IPEndPoint((IPAddress)from, 8994);
            TcpClient client = new TcpClient(to);
            NetworkStream stream = client.GetStream();
            binaryFormatter.Serialize(stream, hostConnectionInfo);
            stream.Close();
        }

        public static List<ConnectionInfo> connections;
        private static void GetAdvertizers_DoWork(object sender, DoWorkEventArgs e)
        {// can fit up to 35-40 connections before timeout
            connections.Clear();
            UdpClient client = new UdpClient();
            TcpListener server = new TcpListener(IP.AdvertizeListenRecieve);
            server.Start();

            byte[] msg = Encoding.ASCII.GetBytes("?????");
            client.Send(msg, msg.Length, IP.AdvertizeBroadcastSend);
            client.Close();

            timeout.Start();
            while (!e.Cancel)
            {
                if (server.Pending())
                {
                    TcpClient tcp = server.AcceptTcpClient();
                    NetworkStream stream = tcp.GetStream();

                    //twiddle thumbs i guess
                    Thread.Sleep(100);

                    try
                    {
                        connections.Add((ConnectionInfo)binaryFormatter.Deserialize(stream));
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    stream.Close();
                    tcp.Close();
                }
            }
            server.Stop();
        }

        private static void ListenForAdvertizers(ref bool shouldClose, ref ConnectionInfo hostConnectionInfo)
        {
            /*
            byte[] buffer = new byte[1024];
            string data = string.Empty;


            IPAddress ipAddress = IP.GetBroadcastIP();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8995);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(MAXSESSIONS);

            while (!shouldClose)
            {
                Socket handler = listener.Accept();
                data = "";

                while (true)
                {
                    int bytesRec = handler.Receive(buffer);
                    data += Encoding.ASCII.GetString(buffer, 0, bytesRec);
                    if (data.IndexOf(';') > -1)
                    {
                        break;
                    }
                }

                data = data.ToLower();
                Console.WriteLine("<" + data);

                if (data.Contains("whoami"))
                {
                    string ip = Encoding.ASCII.GetString(hostConnectionInfo.address.GetAddressBytes());
                    byte[] msg = Encoding.ASCII.GetBytes($"{hostConnectionInfo.displayName}:{ip};");
                    Console.WriteLine(">" + buffer);
                    handler.Send(msg);
                }
                else
                {
                    byte[] msg = Encoding.ASCII.GetBytes("!inv!;");
                    Console.WriteLine(">" + buffer);
                    handler.Send(msg);
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            */
        }
    }

    class IP
    {
        private static byte[] BroadcastMask { get; } = { 0b11111111, 0b11110000, 0b00000000, 0b00000000 };
        [Obsolete]
        public static IPAddress GetIPFromSettings()
        {
            const int addressSize = 4;//ipv4=4, ipv6=16
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(Connect_Four.Properties.Settings.Default.IP_Seed);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            Array.Resize(ref hashBytes, addressSize);

            return new IPAddress(hashBytes);
        }

        [Obsolete]
        public static IPAddress GetBroadcastIP()
        {
            uint ipAddress = BitConverter.ToUInt32(GetLocalIP().GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(BroadcastMask, 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }

        public static IPAddress GetLocalIP()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList[3];
        }

        public static IPEndPoint AdvertizeBroadcastSend = new IPEndPoint(IPAddress.Broadcast, 8995);
        public static IPEndPoint AdvertizeBroadcastRecieve = new IPEndPoint(GetLocalIP(), 8995);
        public static IPEndPoint AdvertizeListenRecieve = new IPEndPoint(GetLocalIP(), 8994);
        public static IPEndPoint GameConnectionRecieve = new IPEndPoint(GetLocalIP(), 8996);
    }

    static class GameConnection
    {

    }

    public class ConnectionInfo
    {
        public string displayName;
        public IPAddress address;
    }
}
