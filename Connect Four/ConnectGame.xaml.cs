using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for ConnectGame.xaml
    /// </summary>
    public partial class ConnectGame : Grid
    {
        public DependencyObject dependencyObject = new DependencyObject();
        public static DependencyProperty GameStateChangerProperty = DependencyProperty.Register("GameState", typeof(IGameStateChanger), typeof(ConnectGame));

        public IGameStateChanger GameStateChanger
        {
            get { return(IGameStateChanger)dependencyObject.GetValue(GameStateChangerProperty); }
            set { dependencyObject.SetValue(GameStateChangerProperty, value); }
        }

        public GameState GameState
        {
            get { return GameStateChanger.GetState(); }
            set { GameStateChanger.SetState(value, null); }
        }

        public ConnectGame()
        {
            InitializeComponent();
            Advertizer.StartAdvertize();
            Advertizer.AdvertizersGotten += LoadAdvertizers;
            Unloaded += ConnectGame_Unloaded;
            Advertizer.GetNameAction = new Func<string>(GetPreferedName);
        }

        public string GetPreferedName()
        {
            return "billy";
        }

        private void LoadAdvertizers(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            ConnectionList.Children.Clear();
            lock (Advertizer.connections)
            {
                foreach (ConnectionInfo connection in Advertizer.connections)
                {
                    ConnectionView connectionView = new ConnectionView(connection);
                    connectionView.Connect += RequestConnect;
                    ConnectionList.Children.Add(connectionView);
                }
            }
            BtnReload.IsEnabled = true;
            ConnectionList.IsEnabled = true;
        }

        private void ConnectGame_Unloaded(object sender, RoutedEventArgs e)
        {
            Advertizer.StopAdvertize();
            Advertizer.AdvertizersGotten -= LoadAdvertizers;
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            Advertizer.StartGetAdvertizers();
            ConnectionList.IsEnabled = false;
            BtnReload.IsEnabled = false;
        }

        private void RequestConnect(object sender, ConnectionInfo connection)
        {
            //((ConnectionView)sender).Fill = Brushes.Yellow;
            Advertizer.OutboundReq.Add(connection);
            connection.InvokeOutboundRequest();

            Advertizer.SendRequest(connection);
            /*
            ConnectionInfo collision = Advertizer.GetFirstRequestPair();
            if (collision != null)
            {
                // from here we make a game request.
                // if the other user hasnt started a game or left the game it should be successful.
                // otherwize remove the inbound request and leave the outbound marker
            }
            else
            {
                // lodge the request to the other user.
            }
            */
        }
    }
}
