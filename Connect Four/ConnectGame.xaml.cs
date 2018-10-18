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
            Connection.Advertizer.Advertize.RunWorkerAsync();
            Connection.Advertizer.GetAdvertizers.RunWorkerCompleted += LoadAdvertizers;
            Unloaded += ConnectGame_Unloaded;
        }

        private void LoadAdvertizers(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            BtnReload.IsEnabled = true;
        }

        private void ConnectGame_Unloaded(object sender, RoutedEventArgs e)
        {
            Connection.Advertizer.Advertize.CancelAsync();
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            if (!Connection.Advertizer.GetAdvertizers.IsBusy)
            {
                Connection.Advertizer.GetAdvertizers.RunWorkerAsync();
            }
            BtnReload.IsEnabled = false;
        }
    }
}
