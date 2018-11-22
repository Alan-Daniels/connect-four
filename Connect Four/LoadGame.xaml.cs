﻿using System;
using System.Collections.Generic;
using System.IO;
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

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for LoadGame.xaml
    /// </summary>
    public partial class LoadGame : Grid
    {
        public event EventHandler<GameMessage> NewGameMessage;
        SavableList<SaveGame> SavedGames;
        public LoadGame()
        {
            InitializeComponent();
            SavedGames = new SavableList<SaveGame>(new FileInfo("savegames.json"));
            SavedGames.Load();

            foreach (SaveGame save in SavedGames)
            {
                SaveView view = new SaveView(save);
                view.GameSelected += View_GameSelected;
                PnlSaved.Children.Add(view);
            }
        }

        private void View_GameSelected(object sender, SaveGame e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoLoad, arg = e });
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.DoLoad, arg = SaveGame.Default });
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            NewGameMessage?.Invoke(this, new GameMessage() { operation = GameOperation.GoConnect });
        }
    }
}