using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Connection;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for ConnectionView.xaml
    /// </summary>

    public partial class ConnectionView : Grid
    {
        public delegate void ConnectEventHandler(object source, ConnectionInfo connection);
        public event ConnectEventHandler Connect;
        public ConnectionInfo Connection { get; private set; }
        public Brush Fill { get { return root.Background; } set { root.Background = value; } }
        private State state = State.None;

        public ConnectionView(ConnectionInfo info)
        {
            InitializeComponent();
            Connection = info;
            HostName.Text = info.displayName;

            ConnectHost.Click += ConnectHost_Click;
            info.InboundRequestRecieved += Info_InboundRequestRecieved;
            info.OutboundRequestSent += Info_OutboundRequestSent;
        }

        private void Info_OutboundRequestSent(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.None:
                    state = State.Sent;
                    break;
                case State.Recieved:
                    state = State.Both;
                    break;
            }
            Application.Current.Dispatcher.Invoke(UpdateColour);
        }

        private void Info_InboundRequestRecieved(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.None:
                    state = State.Recieved;
                    break;
                case State.Sent:
                    state = State.Both;
                    break;
            }
            Application.Current.Dispatcher.Invoke(UpdateColour);
        }

        private void UpdateColour()
        {
            switch (state)
            {
                case State.None:
                    Background = Brushes.White;
                    break;
                case State.Sent:
                    Background = Brushes.Yellow;
                    break;
                case State.Recieved:
                    Background = Brushes.Blue;
                    break;
                case State.Both:
                    Background = Brushes.Green;
                    break;
            }
        }

        private void ConnectHost_Click(object sender, RoutedEventArgs e)
        {
            Connect?.Invoke(this, Connection);
        }

        private enum State
        {
            None,
            Sent,
            Recieved,
            Both
        }
    }
}
