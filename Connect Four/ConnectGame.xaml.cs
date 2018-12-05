using Connection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Connect_Four
{
    /// <summary>
    /// Connects two seperate instances of the game via TCP and starts a new game.
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

        /// <summary>
        /// Function that is passed to Advertizer's GetName method.
        /// </summary>
        /// <returns>The text written in NameBox.</returns>
        public string GetPreferedName()
        {
            return NameBox.Text.Trim();
        }

        /// <summary>
        /// Once all connections have been collected, add them all to a view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            Advertizer.OutboundReq.Add(connection);
            connection.InvokeOutboundRequest();

            Advertizer.SendRequest(connection);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.GoLoad });
        }
    }
}
