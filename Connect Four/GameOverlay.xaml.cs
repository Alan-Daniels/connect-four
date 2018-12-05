using Connection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Connect_Four
{
    /// <summary>
    /// Visual elemts that go over the game engine.
    /// </summary>
    public partial class GameOverlay : Grid
    {
        public event EventHandler<GameMessage> NewGameMessage;

        public GameOverlay()
        {
            InitializeComponent();
            GameConnection.MessageRecieved += GameConnection_MessageRecieved;
            GameConnection.GameDisconnected += GameConnection_GameDisconnected;
            GameGrid.GameMessage += GameMessage_Recieved;
        }

        /// <summary>
        /// Lets the game engine communicate an end result to the user via the game chat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameMessage_Recieved(object sender, CoinType e)
        {
            switch (e)
            {
                case CoinType.None:
                    Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, new Message($"The Game has ended in a tie!", "Server"));
                    break;
                case CoinType.Red:
                    Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, new Message($"You have won!", "Server"));
                    break;
                case CoinType.Blue:
                    Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, new Message($"Your opponent has won!", "Server"));
                    break;
            }
        }

        /// <summary>
        /// Communicates a connection drop to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameConnection_GameDisconnected(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, new Message("Game Disconnected", "Server"));
        }

        /// <summary>
        /// Passes messages sent from the opponent to the game chat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">message</param>
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
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoExit, arg = false });
        }

        /// <summary>
        /// Saves the game with user choice for the name and exits the current game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveNExit_Click(object sender, RoutedEventArgs e)
        {
            AskSaveName askName = new AskSaveName();
            askName.ShowDialog();
            if (askName.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoSave, arg = askName.NameText });
                NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoExit, arg = false });
            }
        }

        /// <summary>
        /// Sends the typed message to the opponent.
        /// </summary>
        private void Send()
        {
            string line = TxtSend.Text.Trim();
            GameConnection.SendMessage(new Message<object>() { Type = typeof(string).ToString(), Data = line });
            AddMessage(new Message(line, "Me"));
            TxtSend.Text = "";
        }

        /// <summary>
        /// Adds a message to the game chat.
        /// </summary>
        /// <param name="msg">message</param>
        private void AddMessage(object msg)
        {
            var message = (Message)msg;
            TxtView.Text += $"\n{message.Sender}: {message.Line}";
        }

        /// <summary>
        /// Turns on the AFK AI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAIOn_Click(object sender, RoutedEventArgs e)
        {
            BtnAIOn.IsEnabled = false;
            BtnAIOff.IsEnabled = true;
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoSetAI, arg = true });
        }

        /// <summary>
        /// Turns off the AFK AI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAIOff_Click(object sender, RoutedEventArgs e)
        {
            BtnAIOn.IsEnabled = true;
            BtnAIOff.IsEnabled = false;
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoSetAI, arg = false });
        }
    }

    class Message
    {
        public Message(string Line, string Sender)
        {
            this.Line = Line;
            this.Sender = Sender;
        }
        public string Line { get; set; }
        public string Sender { get; set; }
    }
}
