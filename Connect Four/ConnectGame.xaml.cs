using Connection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for ConnectGame.xaml
    /// </summary>
    public partial class ConnectGame : Grid
    {
        public event EventHandler<GameMessage> NewGameMessage;
        
        public ConnectGame()
        {
            InitializeComponent();
            Advertizer.StartAdvertize();
            GameConnection.ListenForGame();
            GameConnection.GameConnected += GameConnection_GameConnected;
            Advertizer.AdvertizersGotten += LoadAdvertizers;
            Advertizer.NewConnection += (object s, ConnectionInfo e) => { Application.Current.Dispatcher.Invoke(new Action<ConnectionInfo>(AddConnection), e); };
            Unloaded += ConnectGame_Unloaded;
            Advertizer.GetNameAction = new Func<string>(GetPreferedName);
        }

        private void GameConnection_GameConnected(object sender, EventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.GoGame });
        }

        public string GetPreferedName()
        {
            return NameBox.Text.Trim();
        }

        private void LoadAdvertizers(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            ConnectionList.Children.Clear();
            lock (Advertizer.connections)
            {
                foreach (ConnectionInfo connection in Advertizer.connections)
                {
                    AddConnection(connection);
                }
            }
            BtnReload.IsEnabled = true;
            ConnectionList.IsEnabled = true;
        }

        public void AddConnection(ConnectionInfo connection)
        {
            ConnectionView connectionView = new ConnectionView(connection);
            connectionView.Connect += RequestConnect;
            ConnectionList.Children.Add(connectionView);
        }

        private void ConnectGame_Unloaded(object sender, RoutedEventArgs e)
        {
            Advertizer.StopAdvertize();
            Advertizer.AdvertizersGotten -= LoadAdvertizers;
            GameConnection.StopListening();// this is not needed but used if the connectionListener never stops.
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
