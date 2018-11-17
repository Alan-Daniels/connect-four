﻿using System;
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
    /// Interaction logic for GameOverlay.xaml
    /// </summary>
    public partial class GameOverlay : Grid
    {
        public GameOverlay()
        {
            InitializeComponent();
            GameConnection.MessageRecieved += GameConnection_MessageRecieved;
        }

        private void GameConnection_MessageRecieved(object sender, GameConnectionEventArgs<string> e)
        {
            Application.Current.Dispatcher.Invoke((Action<object>)AddMessage, e.GameObject);
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

        }

        private void SaveNExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Send()
        {
            string line = TxtSend.Text.Trim();
            GameConnection.SendMessage(new Message<object>() { Type = typeof(string).ToString(), Data = line });
            AddMessage(line);
            TxtSend.Text = "";
        }

        private void AddMessage(object line)
        {
            TxtView.Text += $"\n{(string)line}";
        }
    }
}
