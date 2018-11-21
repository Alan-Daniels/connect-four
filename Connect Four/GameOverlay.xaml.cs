using Connection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for GameOverlay.xaml
    /// </summary>
    public partial class GameOverlay : Grid
    {
        public event EventHandler<GameMessage> NewGameMessage;

        public GameOverlay()
        {
            InitializeComponent();
            GameConnection.MessageRecieved += GameConnection_MessageRecieved;
        }

        private void GameConnection_MessageRecieved(object sender, GameConnectionEventArgs<string> e)
        {
            Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, new Message(e.GameObject, "Them"));
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void TxtSend_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Send();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.GoConnect });
        }

        private void SaveNExit_Click(object sender, RoutedEventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoSave });
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.GoConnect });
        }

        private void Send()
        {
            string line = TxtSend.Text.Trim();
            GameConnection.SendMessage(new Message<object>() { Type = typeof(string).ToString(), Data = line });
            AddMessage(new Message(line, "Me"));
            TxtSend.Text = "";
        }

        private void AddMessage(object msg)
        {
            var message = (Message)msg;
            TxtView.Text += $"\n{message.sender}:{message.line}";
        }
    }

    class Message
    {
        public Message(string line, string sender)
        {
            this.line = line;
            this.sender = sender;
        }
        public string line { get; set; }
        public string sender { get; set; }
    }
}
