using System;
using Mono.Nat;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public delegate void ConnectionEventHandler();

    class Node : IDisposable
    {
        public event ConnectionEventHandler MessageRecieved;

        public Node()
        {
            searchMapping = new Mapping(Protocol.Tcp, 9091, 9091);
            portMapper = new PortMap();
        }

        private Mapping searchMapping;
        PortMap portMapper;

        private bool isAdvertizing;
        public bool IsAdvertizing
        {
            get { return isAdvertizing; }
            set
            {
                if (value != isAdvertizing)
                {
                    isAdvertizing = value;
                    if (isAdvertizing)
                    {
                        Advertize();
                    }
                    else
                    {
                        UnAdvertize();
                    }
                }
            }
        }

        private string identifier = Environment.MachineName;
        public string Identifier {
            get { return identifier; }
            set
            {
                if (value != "" && value != identifier)
                {
                    identifier = value;
                    if (isAdvertizing)
                    {
                        UnAdvertize();
                        Advertize();
                    }
                }
            }
        } 

        private void Advertize()
        {

        }
        private void UnAdvertize()
        {

        }

        public ConnectionInfo[] GetConnections()
        {
            return new ConnectionInfo[0];
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionInfo
    {
        string displayName;
        string address;
    }

    public class PortMap : IDisposable
    {
        public PortMap()
        {
            NatUtility.DeviceFound += NatUtility_DeviceFound;
            NatUtility.DeviceLost += NatUtility_DeviceLost;
            NatUtility.StartDiscovery();
        }

        public INatDevice NatDevice { get; private set; }

        private void NatUtility_DeviceLost(object sender, DeviceEventArgs e)
        {
            NatDevice = null;
        }

        private void NatUtility_DeviceFound(object sender, DeviceEventArgs e)
        {
            NatDevice = e.Device;
        }

        public void Dispose()
        {
            NatUtility.DeviceFound -= NatUtility_DeviceFound;
            NatUtility.DeviceLost -= NatUtility_DeviceLost;
            NatUtility.StopDiscovery();
        }
    }


    
}
