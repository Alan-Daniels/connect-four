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

        public ConnectionView(ConnectionInfo info)
        {
            InitializeComponent();
            Connection = info;
            HostName.Text = info.displayName;

            ConnectHost.Click += ConnectHost_Click;
        }

        private void ConnectHost_Click(object sender, RoutedEventArgs e)
        {
            Connect?.Invoke(this, Connection);
        }
    }
}
