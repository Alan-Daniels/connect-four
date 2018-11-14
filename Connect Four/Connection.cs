using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace Connection
{
    public delegate void ConnectionEventHandler();

    public static class Advertizer
    {
        private static readonly BackgroundWorker GetAdvertizers = new BackgroundWorker() { WorkerSupportsCancellation = true };
        private static readonly BackgroundWorker Advertize = new BackgroundWorker() { WorkerSupportsCancellation = true };
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static event RunWorkerCompletedEventHandler AdvertizersGotten { add { GetAdvertizers.RunWorkerCompleted += value; } remove { GetAdvertizers.RunWorkerCompleted -= value; } }
        public static event EventHandler<ConnectionInfo> NewConnection;

        public static HashSet<ConnectionInfo> connections;
        public static HashSet<ConnectionInfo> InboundReq;
        public static HashSet<ConnectionInfo> OutboundReq;

        public static Func<string> GetNameAction = new Func<string>(() => { return "Bob"; });
        private static string GetName()
        {
            try
            {
                return Application.Current.Dispatcher.Invoke(GetNameAction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encountered an exception while grabbing preferred name.\n{ex}");
                return "Bob";
            }
        }

        static Advertizer()
        {
            connections = new HashSet<ConnectionInfo>();
            InboundReq = new HashSet<ConnectionInfo>();
            OutboundReq = new HashSet<ConnectionInfo>();
            GetAdvertizers.DoWork += GetAdvertizers_DoWork;
            Advertize.DoWork += Advertize_DoWork;
        }

        public static ConnectionInfo GetFirstRequestPair()
        {
            foreach (var request in InboundReq)
            {
                if (OutboundReq.Contains(request))
                {
                    return request;
                }
            }
            return null;
        }

        public static void SendRequest(ConnectionInfo to)
        {
            UdpClient udp = new UdpClient(AddressFamily.InterNetwork);
            IPEndPoint ep = new IPEndPoint(to.address, IP.broadcastPort);
            byte[] msg = Encoding.ASCII.GetBytes("!!!!!");
            udp.Send(msg, msg.Length, ep);
        }

        public static void StartAdvertize()
        {
            if (!Advertize.IsBusy)
            {
                InboundReq.Clear();
                OutboundReq.Clear();
                Advertize.RunWorkerAsync();
            }
        }

        public static void StopAdvertize()
        {
            if (Advertize.IsBusy)
            {
                Advertize.CancelAsync();
            }
        }

        public static void StartGetAdvertizers()
        {
            if (!GetAdvertizers.IsBusy)
            {
                GetAdvertizers.RunWorkerAsync();
            }
        }

        private static async void Advertize_DoWork(object sender, DoWorkEventArgs e)
        {
            UdpClient server = new UdpClient(AddressFamily.InterNetwork);
            server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.ExclusiveAddressUse = false;
            server.Client.Bind(IP.BroadcastRecieve);

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
                    else if (request.Contains("!"))
                    {
                        new Task(AddInboundRequest, result.RemoteEndPoint.Address).Start();
                    }
                }
            }
        }

        private static void WhoAmI(object from)
        {
            IPEndPoint to = new IPEndPoint((IPAddress)from, IP.advertizePort);
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ExclusiveAddressUse = false;
            client.Connect(to);

            NetworkStream stream = client.GetStream();
            if (to.Address.Equals(IP.LocalAddress))
            {
                binaryFormatter.Serialize(stream, new ConnectionInfo() { address = IP.LocalAddress, displayName = "Me" });
            }
            else
            {
                binaryFormatter.Serialize(stream, new ConnectionInfo() { address = IP.LocalAddress, displayName = GetName() });
            }
            stream.Close();
            client.Close();
        }

        private static void AddInboundRequest(object from)
        {
            IPAddress sender = (IPAddress)from;
            bool found = false;
            foreach (ConnectionInfo connection in connections)
            {
                if (connection.address.Equals(sender))
                {
                    found = true;
                    if (!InboundReq.Contains(connection))
                    {
                        InboundReq.Add(connection);
                        connection.InvokeInboundRequest();
                    }
                    else
                    {
                        GameConnection.RequestGame(connection);
                    }
                    break;
                }
            }

            if (found == false && NewConnection != null)
            {
                ConnectionInfo connection = new ConnectionInfo() { address = sender, displayName = "Unknown" };
                connections.Add(connection);
                InboundReq.Add(connection);
                NewConnection.Invoke(null, connection);
                connection.InvokeInboundRequest();
            }
            else
            {
                Console.WriteLine($"Could not add inbound connection {sender} as no new connections could be made.");
            }
        }

        private static void GetAdvertizers_DoWork(object sender, DoWorkEventArgs e)
        {// can fit up to 35-40 connections before timeout
            Stopwatch getAdvertizersStopwatch = new Stopwatch();
            connections.Clear();

            UdpClient client = new UdpClient(AddressFamily.InterNetwork);
            TcpListener server = new TcpListener(IP.AdvertizeRecieve);
            server.ExclusiveAddressUse = false;
            server.Start();

            byte[] msg = Encoding.ASCII.GetBytes("?????");
            client.Send(msg, msg.Length, IP.BroadcastSend);

            getAdvertizersStopwatch.Start();
            while (getAdvertizersStopwatch.ElapsedMilliseconds <= 4000)
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
            getAdvertizersStopwatch.Stop();
            server.Stop();
        }
    }

    static class IP
    {
        public static IPAddress GetLocalIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private static IPAddress localAddress;
        public static IPAddress LocalAddress { get { return localAddress = localAddress ?? GetLocalIP(); } }

        public static int broadcastPort = Connect_Four.Properties.Settings.Default.AdvertizeUdp;
        public static int advertizePort = Connect_Four.Properties.Settings.Default.AdvertizeTcp;
        public static int gamePort = Connect_Four.Properties.Settings.Default.GameTcp;

        public static IPEndPoint BroadcastSend = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
        public static IPEndPoint BroadcastRecieve = new IPEndPoint(IPAddress.Any, broadcastPort);

        public static IPEndPoint AdvertizeRecieve = new IPEndPoint(IPAddress.Any, advertizePort);
        public static IPEndPoint GameRecieve = new IPEndPoint(IPAddress.Any, gamePort);
    }

    public delegate void GameConnectionEventHandler<T>(object sender, GameConnectionEventArgs<T> e);
    public class GameConnectionEventArgs<T>
    {
        public T GameObject { get; }
        public string BaseMessage { get; }
        public GameConnectionEventArgs(T GameObject, string BaseMessage)
        {
            this.GameObject = GameObject;
            this.BaseMessage = BaseMessage;
        }
    }

    public enum ConnectionType
    {
        Disconnected,
        Server,
        Client
    }

    [Serializable]
    public class Message
    {
        public string Type { get; set; }
        public object Data { get; set; }
    }

    public static class GameConnection
    {
        public static event GameConnectionEventHandler<string> MessageRecieved;
        public static event GameConnectionEventHandler<Point> LocationRecieved;
        public static event EventHandler GameConnected;
        public static event EventHandler GameDisconnected;
        private static readonly BackgroundWorker ListenerBackgroundWorker;
        private static readonly BackgroundWorker GameBackgroundWorker;
        public static ConnectionType ConnectionType { get; private set; }

        private static readonly LinkedList<string> messages;

        static GameConnection()
        {
            messages = new LinkedList<string>();

            ListenerBackgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            GameBackgroundWorker = new BackgroundWorker();
            ListenerBackgroundWorker.DoWork += ListenerBackgroundWorker_DoWork;
            GameBackgroundWorker.DoWork += GameBackgroundWorker_DoWork;
            GameBackgroundWorker.RunWorkerCompleted += GameBackgroundWorker_RunWorkerCompleted;
        }

        public static void ListenForGame()
        {
            if (!ListenerBackgroundWorker.IsBusy)
            {
                ListenerBackgroundWorker.RunWorkerAsync();
            }
        }

        public static void StopListening()
        {
            if (ListenerBackgroundWorker.IsBusy)
            {
                ListenerBackgroundWorker.CancelAsync();
            }
        }

        public static void SendMessage(Message message)
        {
            lock (messages)
            {
                messages.AddLast(JsonConvert.Serialise(message));
            }
        }

        public static void RequestGame(ConnectionInfo connectTo)
        {
            new Task(Connect, connectTo).Start();
        }

        private static void Connect(object connectTo)
        {
            ConnectionInfo connectionInfo = (ConnectionInfo)connectTo;
            if (!GameBackgroundWorker.IsBusy)
            {
                IPEndPoint to = new IPEndPoint(connectionInfo.address, IP.gamePort);
                TcpClient tcp = new TcpClient(AddressFamily.InterNetwork);
                try
                {
                    tcp.Connect(to);
                    if (!GameBackgroundWorker.IsBusy)
                    {
                        GameBackgroundWorker.RunWorkerAsync(tcp);
                        ConnectionType = ConnectionType.Client;
                    }
                    else
                    {
                        tcp.Close();
                    }
                }
                catch (Exception)
                {
                    // the connection has failed
                    throw;
                }
            }
        }

        private static void ListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = new TcpListener(IP.GameRecieve) { ExclusiveAddressUse = false };
            listener.Start();
            while (!e.Cancel)
            {
                if (listener.Pending() && !GameBackgroundWorker.IsBusy)
                {
                    TcpClient tcp = listener.AcceptTcpClient();
                    GameBackgroundWorker.RunWorkerAsync(tcp);
                    ConnectionType = ConnectionType.Server;
                }
            }
            listener.Stop();
        }

        private static void GameBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpClient tcp = (TcpClient)e.Argument;
            ListenerBackgroundWorker.CancelAsync();
            GameConnected?.Invoke(null, null);
            NetworkStream stream = tcp.GetStream();
            stream.ReadTimeout = 300;
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);
            while (tcp.Connected)
            {
                RecieveMessages(reader);
                SendMessages(writer, reader);
                Thread.Sleep(50);
            }
            GameDisconnected?.Invoke(null, null);
        }

        private static void RecieveMessages(StreamReader reader)
        {
            bool finished = false;
            string currentString;
            Message currentMessage;
            while (!finished)
            {
                currentString = reader.ReadLine();
                if (currentString != null)
                {
                    currentMessage = JsonConvert.Deserialise<Message>(currentString);
                    if (currentMessage.Type == typeof(string).ToString())
                    {
                        MessageRecieved?.Invoke(null, new GameConnectionEventArgs<string>((string)currentMessage.Data, currentString));
                    }
                    else if (currentMessage.Type == typeof(Point).ToString())
                    {
                        LocationRecieved?.Invoke(null, new GameConnectionEventArgs<Point>((Point)currentMessage.Data, currentString));
                    }
                }
                else
                {
                    finished = true;
                }
            }
        }

        private static void SendMessages(StreamWriter writer, StreamReader reader)
        {
            if (messages.Count > 0)
            {
                lock (messages)
                {
                    foreach (var message in messages)
                    {
                        writer.WriteLine(message);
                    }
                }
            }
        }

        private static void GameBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ConnectionType = ConnectionType.Disconnected;
            messages.Clear();
        }
    }

    public class JsonConvert
    {
        public static string Serialise<T>(T input)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            string output = json_serializer.Serialize(input);
            return output;
        }

        public static T Deserialise<T>(string input)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            T output = json_serializer.Deserialize<T>(input);
            return output;
        }
    }

    [Serializable]
    public class ConnectionInfo
    {
        public event EventHandler InboundRequestRecieved;
        public event EventHandler OutboundRequestSent;

        public void InvokeInboundRequest()
        {
            InboundRequestRecieved?.Invoke(this, EventArgs.Empty);
        }

        public void InvokeOutboundRequest()
        {
            OutboundRequestSent?.Invoke(this, EventArgs.Empty);
        }

        public string displayName;
        public IPAddress address;
    }
}
