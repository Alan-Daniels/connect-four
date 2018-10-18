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
        }

        private void ConnectGame_Unloaded(object sender, RoutedEventArgs e)
        {
            Advertizer.StopAdvertize();
            Advertizer.AdvertizersGotten -= LoadAdvertizers;
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            Advertizer.StartGetAdvertizers();
            BtnReload.IsEnabled = false;
        }

        private void RequestConnect(object sender, ConnectionInfo connection)
        {

        }
    }
}
