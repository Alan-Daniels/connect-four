using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection
{
    public delegate void ConnectionEventHandler();

    static class Advertizer
    {
        private static Task Advertize;
        public static ConnectionInfo hostConnectionInfo;

        private static bool isAdvertizing;
        public static bool IsAdvertizing
        {
            get { return isAdvertizing; }
            set
            {
                if (value != isAdvertizing)
                {
                    isAdvertizing = value;
                    if (value && Advertize.IsCompleted)
                    {
                        Advertize.Start();
                    }
                }
            }
        }

        const int MAXSESSIONS = 64;

        static Advertizer()
        {
            Advertize = new Task(()=> { ListenForAdvertizers(ref isAdvertizing, ref hostConnectionInfo); });
        }

        public static void GetAdvertizers(ref bool shouldClose, ref List<ConnectionInfo> connections)
        {
            byte[] buffer = new byte[1024];
            string data = string.Empty;


            IPAddress ipAddress = IP.GetIPFromSettings();
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, 8995);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Igmp);

            sender.Connect(remoteEndPoint);
            sender.Listen(MAXSESSIONS);


            byte[] msg = Encoding.ASCII.GetBytes("?WhoAmI?;");

            sender.Send(msg);
            while (!shouldClose)
            {
                Socket handler = sender.Accept();
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

                if (data.Contains(":"))
                {
                    int ofMiddle, ofEnd;
                    ofMiddle = data.IndexOf(':');
                    ofEnd = data.IndexOf(';');

                    string dispName = data.Substring(0, ofMiddle);
                    string strIP = data.Substring(ofMiddle, ofEnd - ofMiddle);

                    IPAddress addr = new IPAddress(Encoding.ASCII.GetBytes(strIP));

                    connections.Add(new ConnectionInfo() { displayName = dispName, address = addr });
                }
                else
                {
                    Console.WriteLine($"Error: {data}");
                }

            }
        }

        private static void ListenForAdvertizers(ref bool shouldClose, ref ConnectionInfo hostConnectionInfo)
        {
            byte[] buffer = new byte[1024];
            string data = string.Empty;


            IPAddress ipAddress = IP.GetIPFromSettings();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8995);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Igmp);

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
        }
    }

    class IP
    {
        public static IPAddress GetIPFromSettings()
        {
            const int addressSize = 4;//ipv4=4, ipv6=16
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(Connect_Four.Properties.Settings.Default.IP_Seed);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            Array.Resize(ref hashBytes, addressSize);

            return new IPAddress(hashBytes);
        }
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
