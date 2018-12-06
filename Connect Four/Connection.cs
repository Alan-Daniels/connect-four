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

    /// <summary>
    /// Controls discovery of other users on the same network.
    /// </summary>
    public static class Advertizer
    {
        private static readonly BackgroundWorker GetAdvertizers = new BackgroundWorker() { WorkerSupportsCancellation = true };
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        private static CancellationTokenSource AdvertizeCancel;
        private static Task AdvertizeTask;

        public static event RunWorkerCompletedEventHandler AdvertizersGotten { add { GetAdvertizers.RunWorkerCompleted += value; } remove { GetAdvertizers.RunWorkerCompleted -= value; } }
        public static event EventHandler<ConnectionInfo> NewConnection;

        public static ICollection<ConnectionInfo> connections;
        public static ICollection<ConnectionInfo> InboundReq;
        public static ICollection<ConnectionInfo> OutboundReq;

        public static Func<string> GetNameAction = new Func<string>(() => { return "Unknown"; });
        /// <summary>
        /// Attemps to retrieve a name from the given function.
        /// </summary>
        /// <returns>Name</returns>
        private static string GetName()
        {
            try
            {
                return Application.Current.Dispatcher.Invoke(GetNameAction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encountered an exception while grabbing preferred name.\n{ex}");
                return "Unknown";
            }
        }

        static Advertizer()
        {
            connections = new SortedSet<ConnectionInfo>();//change to binary tree variant
            InboundReq = new SortedSet<ConnectionInfo>();
            OutboundReq = new SortedSet<ConnectionInfo>();
            GetAdvertizers.DoWork += GetAdvertizers_DoWork;
            AdvertizeCancel = new CancellationTokenSource();
            AdvertizeCancel.Cancel();
        }

        /// <summary>
        /// Clears the previouse session's connection info.
        /// </summary>
        public static void ClearConnections()
        {
            connections.Clear();
            InboundReq.Clear();
            OutboundReq.Clear();
        }

        /// <summary>
        /// Attempts to find a match between Inbound and Outbound requests.
        /// </summary>
        /// <returns>A mutual connection</returns>
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

        /// <summary>
        /// Sends a UDP packet to the desired location to request starting a game.
        /// </summary>
        /// <param name="to"></param>
        public static void SendRequest(ConnectionInfo to)
        {
            UdpClient udp = new UdpClient(AddressFamily.InterNetwork);
            IPEndPoint ep = new IPEndPoint(to.address, IP.broadcastPort);
            byte[] msg = Encoding.ASCII.GetBytes("!!!!!");
            udp.Send(msg, msg.Length, ep);
        }

        public static void StartAdvertize()
        {
            if (AdvertizeTask == null || AdvertizeTask.Status == TaskStatus.RanToCompletion)
            {
                AdvertizeCancel = new CancellationTokenSource();
                InboundReq.Clear();
                OutboundReq.Clear();
                AdvertizeTask = new Task(DoAdvertize, AdvertizeCancel.Token);
                AdvertizeTask.Start();
            }
        }

        public static void StopAdvertize()
        {
            AdvertizeCancel.Cancel();
        }

        public static void StartGetAdvertizers()
        {
            if (!GetAdvertizers.IsBusy)
            {
                GetAdvertizers.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Listens for UDP packets.
        /// Thease packsts can be either discovery packsts("?") or request packsts("!").
        /// </summary>
        private static async void DoAdvertize()
        {
            UdpClient server = new UdpClient(AddressFamily.InterNetwork);
            server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.Client.Bind(IP.BroadcastRecieve);
            while (!AdvertizeCancel.Token.IsCancellationRequested)
            {
                var result = await server.ReceiveAsync();
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

        /// <summary>
        /// Sends an address and name as response to a discovery packet("?").
        /// </summary>
        /// <param name="from">IPAddress of the request</param>
        private static void WhoAmI(object from)
        {
            IPEndPoint to = new IPEndPoint((IPAddress)from, IP.advertizePort);
            if (!to.Address.Equals(IP.LocalAddress))
            {
                TcpClient client = new TcpClient(AddressFamily.InterNetwork);
                client.Connect(to);
                NetworkStream stream = client.GetStream();
                binaryFormatter.Serialize(stream, new ConnectionInfo() { address = IP.LocalAddress, displayName = GetName() });
                stream.Close();
                client.Close();
            }
        }

        /// <summary>
        /// Adds to the Inbound Requests que and adds to the connections list if a matching IP does not exist.
        /// </summary>
        /// <param name="from">IPAddress of the request</param>
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
                    else if (OutboundReq.Contains(connection))
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
        }

        /// <summary>
        /// Sends a broadcast to collect all other valid users on the network and fills them into a que.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GetAdvertizers_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch getAdvertizersStopwatch = new Stopwatch();
            connections.Clear();

            UdpClient client = new UdpClient(AddressFamily.InterNetwork);
            TcpListener server = new TcpListener(IP.AdvertizeRecieve);
            server.Start();

            byte[] msg = Encoding.ASCII.GetBytes("?????");
            client.Send(msg, msg.Length, IP.BroadcastSend);

            getAdvertizersStopwatch.Start();
            while (getAdvertizersStopwatch.ElapsedMilliseconds <= 1500)
            {
                if (server.Pending())
                {
                    TcpClient tcp = server.AcceptTcpClient();
                    NetworkStream stream = tcp.GetStream();
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
                Thread.Sleep(50);
            }
            getAdvertizersStopwatch.Stop();
            server.Stop();
        }
    }

    /// <summary>
    /// Keeps track of all IP-related stuff.
    /// </summary>
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
    public class Message<T>
    {
        public string Type { get; set; }
        public T Data { get; set; }
    }

    /// <summary>
    /// Handles sending and recieving messages during an online game.
    /// </summary>
    public static class GameConnection
    {
        public static event GameConnectionEventHandler<string> MessageRecieved;
        public static event GameConnectionEventHandler<Point> LocationRecieved;
        public static event EventHandler GameConnected;
        public static event EventHandler GameDisconnected;
        private static readonly BackgroundWorker ListenerBackgroundWorker;
        private static readonly BackgroundWorker GameReader;
        private static readonly BackgroundWorker GameWriter;
        public static ConnectionType ConnectionType { get; private set; }
        private static TcpClient tcpClient;
        private static readonly Timer KeepAliveTimer;

        private static readonly LinkedList<string> messages;

        static GameConnection()
        {
            messages = new LinkedList<string>();

            ListenerBackgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            GameReader = new BackgroundWorker() { WorkerSupportsCancellation = true };
            GameWriter = new BackgroundWorker() { WorkerSupportsCancellation = true };

            KeepAliveTimer = new Timer(new TimerCallback(KeepAlive), null, 500, 1000);

            ListenerBackgroundWorker.DoWork += ListenerBackgroundWorker_DoWork;
            GameReader.DoWork += GameReader_DoWork;
            GameWriter.DoWork += GameWriter_DoWork;
        }

        /// <summary>
        /// Sends a message to the client to make sure the connection is still alive.
        /// </summary>
        /// <param name="info"></param>
        public static void KeepAlive(object info)
        {
            if (ConnectionType != ConnectionType.Disconnected)
            {
                SendMessage(new Message<object>() { Data = "", Type = "keepAlive" });
            }
        }

        /// <summary>
        /// Writes all new messages to the other client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GameWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            var writer = (StreamWriter)e.Argument;
            writer.AutoFlush = true;
            while (tcpClient.Connected)
            {
                if (messages.Count > 0)
                {
                    lock (messages)
                    {
                        foreach (var message in messages)
                        {
                            writer.WriteLine(message);
                        }
                        messages.Clear();
                    }
                }
                Thread.Sleep(150);
            }
        }

        /// <summary>
        /// Reads all messages from the other client and raises the appropriate event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GameReader_DoWork(object sender, DoWorkEventArgs e)
        {
            var reader = (StreamReader)e.Argument;
            string currentString;
            Message<object> currentMessage;
            while (tcpClient.Connected)
            {
                try
                {
                    currentString = reader.ReadLine();
                }
                catch (Exception)
                {
                    currentString = null;
                }

                if (!(currentString == null || currentString == ""))
                {
                    currentMessage = JsonConvert.Deserialise<Message<object>>(currentString);
                    if (currentMessage != default(Message<object>))
                    {
                        if (currentMessage.Type == typeof(string).ToString())
                        {
                            MessageRecieved?.Invoke(null, new GameConnectionEventArgs<string>(JsonConvert.Deserialise<Message<string>>(currentString).Data, currentString));
                        }
                        else if (currentMessage.Type == typeof(Point).ToString())
                        {
                            LocationRecieved?.Invoke(null, new GameConnectionEventArgs<Point>(JsonConvert.Deserialise<Message<Point>>(currentString).Data, currentString));
                        }
                    }
                }
            }
            StopGame();
        }

        /// <summary>
        /// Starts the reading and writing of the game and raises the GameConnected event.
        /// </summary>
        private static void StartGame()
        {
            if (!(GameReader.IsBusy || GameWriter.IsBusy))
            {
                StopListening();
                var stream = tcpClient.GetStream();
                stream.ReadTimeout = 5000;
                GameReader.RunWorkerAsync(new StreamReader(stream));
                GameWriter.RunWorkerAsync(new StreamWriter(stream));
                GameConnected?.Invoke(null, null);
            }
        }

        /// <summary>
        /// Stops the reading and writing and raises the GameDisconnected event.
        /// </summary>
        private static void StopGame()
        {
            if (ConnectionType != ConnectionType.Disconnected)
            {
                GameDisconnected?.Invoke(null, null);
                ConnectionType = ConnectionType.Disconnected;
            }
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

        public static void SendMessage(Message<object> message)
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

        /// <summary>
        /// Attepmts to Start the game by connecting to another client.
        /// </summary>
        /// <param name="connectTo"></param>
        private static void Connect(object connectTo)
        {
            ConnectionInfo connectionInfo = (ConnectionInfo)connectTo;
            if (ConnectionType == ConnectionType.Disconnected)
            {
                IPEndPoint to = new IPEndPoint(connectionInfo.address, IP.gamePort);
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                try
                {
                    tcpClient.Connect(to);
                    ConnectionType = ConnectionType.Client;
                    StartGame();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static void Disconnect()
        {
            if (ConnectionType != ConnectionType.Disconnected)
            {
                tcpClient.Close();
            }
        }

        /// <summary>
        /// Waits for any incoming connections.
        /// When a client connects runs StartGame.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = new TcpListener(IP.GameRecieve);
            listener.Start();
            while (!ListenerBackgroundWorker.CancellationPending)
            {
                if (listener.Pending() && ConnectionType == ConnectionType.Disconnected)
                {
                    tcpClient = listener.AcceptTcpClient();
                    ConnectionType = ConnectionType.Server;
                    StartGame();
                }
                Thread.Sleep(100);
            }
            listener.Stop();
        }
    }

    /// <summary>
    /// Converts c# objects to json strings.
    /// </summary>
    public class JsonConvert
    {
        /// <summary>
        /// Serializes a given object.
        /// </summary>
        /// <typeparam name="T">A serializable type</typeparam>
        /// <param name="input">Object to serialize</param>
        /// <returns>Json representation</returns>
        public static string Serialise<T>(T input)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            string output = json_serializer.Serialize(input);
            return output;
        }

        /// <summary>
        /// Converts from json string to c# object.
        /// </summary>
        /// <typeparam name="T">A serializable type</typeparam>
        /// <param name="input">Json string</param>
        /// <returns>An object of type T</returns>
        public static T Deserialise<T>(string input)
        {
            try
            {
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                T output = json_serializer.Deserialize<T>(input);
                return output;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }

    [Serializable]
    public class ConnectionInfo : IComparable<ConnectionInfo>
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

        public int CompareTo(ConnectionInfo other)
        {
            int a = this.address.GetHashCode();
            int b = other.address.GetHashCode();
            return a.CompareTo(b);
        }

        public string displayName;
        public IPAddress address;
    }
}
