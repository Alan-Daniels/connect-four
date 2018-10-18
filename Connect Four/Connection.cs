﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection
{
    public delegate void ConnectionEventHandler();

    static class Advertizer
    {
        public static ConnectionInfo hostConnectionInfo;
        private static readonly BackgroundWorker GetAdvertizers = new BackgroundWorker() { WorkerSupportsCancellation = true };
        private static readonly BackgroundWorker Advertize = new BackgroundWorker() { WorkerSupportsCancellation = true };
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static event RunWorkerCompletedEventHandler AdvertizersGotten { add { GetAdvertizers.RunWorkerCompleted += value; } remove { GetAdvertizers.RunWorkerCompleted -= value; } }

        public static List<ConnectionInfo> connections;

        static Advertizer()
        {
            hostConnectionInfo = new ConnectionInfo()
            {
                address = IP.LocalAddress,
                displayName = "Bob"
            };

            connections = new List<ConnectionInfo>();
            GetAdvertizers.DoWork += GetAdvertizers_DoWork;
            Advertize.DoWork += Advertize_DoWork;
        }

        public static void StartAdvertize()
        {
            if (!Advertize.IsBusy)
            {
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
            UdpClient server = new UdpClient();
            server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.ExclusiveAddressUse = false;
            server.Client.Bind(IP.AdvertizeBroadcastRecieve);

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
                }
            }
        }

        private static void WhoAmI(object from)
        {
            IPEndPoint to = new IPEndPoint((IPAddress)from, 8994);
            TcpClient client = new TcpClient();
            client.ExclusiveAddressUse = false;
            client.Connect(to);

            NetworkStream stream = client.GetStream();
            binaryFormatter.Serialize(stream, hostConnectionInfo);
            stream.Close();
            client.Close();
        }

        private static void GetAdvertizers_DoWork(object sender, DoWorkEventArgs e)
        {// can fit up to 35-40 connections before timeout
            Stopwatch getAdvertizersStopwatch = new Stopwatch();
            connections.Clear();

            UdpClient client = new UdpClient();
            TcpListener server = new TcpListener(IP.AdvertizeListenRecieve);
            server.ExclusiveAddressUse = false;
            server.Start();

            byte[] msg = Encoding.ASCII.GetBytes("?????");
            client.Send(msg, msg.Length, IP.AdvertizeBroadcastSend);

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
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private static IPAddress localAddress;
        public static IPAddress LocalAddress { get { return localAddress = (localAddress != null ? localAddress : GetLocalIP()); } }

        public static IPEndPoint AdvertizeBroadcastSend = new IPEndPoint(IPAddress.Broadcast, 8995);
        public static IPEndPoint AdvertizeBroadcastRecieve = new IPEndPoint(IPAddress.Any, 8995);
        public static IPEndPoint AdvertizeListenRecieve = new IPEndPoint(IPAddress.Any, 8994);
        public static IPEndPoint GameConnectionRecieve = new IPEndPoint(IPAddress.Any, 8996);
    }

    static class GameConnection
    {

    }

    [Serializable]
    public class ConnectionInfo
    {
        public string displayName;
        public IPAddress address;
    }
}
