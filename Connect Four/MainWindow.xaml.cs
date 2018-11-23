using Connection;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGameStateChanger
    {
        SavableList<SaveGame> SavedGames;
        private GameState gameState = GameState.None;

        public MainWindow()
        {
            InitializeComponent();
            SavedGames = new SavableList<SaveGame>(new FileInfo("savegames.json"));
            SavedGames.Load();
            SetState(GameState.Load);

            GameConnection.GameMessageRecieved += RecieveMessage;
        }

        public GameState GetState()
        {
            return gameState;
        }

        public void SetState(GameState state)
        {
            SetState(state, null);
        }

        public void SetState(GameState state, object args = null)
        {
            if (gameState != state)
            {
                Size s = SaveGame.DefaultSize;
                switch (state)
                {
                    case GameState.Connect:
                        var connectGame = new ConnectGame();
                        connectGame.NewGameMessage += RecieveMessage;
                        root.Child = connectGame;
                        overlay.Child = null;
                        break;
                    case GameState.Game:
                        root.Child = new GameGrid((SaveGame)args)
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Height = 1050,
                            Width = 1050,
                            Background = Brushes.Linen
                        };
                        var gameOverlay = new GameOverlay();
                        gameOverlay.NewGameMessage += RecieveMessage;
                        overlay.Child = gameOverlay;
                        break;
                    case GameState.Load:
                        var LoadGame = new LoadGame();
                        LoadGame.NewGameMessage += RecieveMessage;
                        root.Child = LoadGame;
                        overlay.Child = null;
                        break;
                }
                gameState = state;
            }
        }

        public void RecieveMessage(object sender, GameMessage e)
        {
            switch (e.operation)
            {
                case GameOperation.DoSave:
                    SaveGame save = ((GameGrid)root.Child).Save();
                    save.Name = (string)e.arg;
                    SavedGames.Add(save);
                    SavedGames.Save();
                    break;
                case GameOperation.GoConnect:
                    SetState(GameState.Connect);
                    break;
                case GameOperation.DoLoad:
                    SetState(GameState.Game, e.arg);
                    break;
                case GameOperation.DoSetAI:
                    ((GameGrid)root.Child).FriendlyAIEnabled = (bool)e.arg;
                    break;
                case GameOperation.DoExit:
                    ((GameGrid)root.Child).EndGame((bool)e.arg);
                    SetState(GameState.Load);
                    break;
                case GameOperation.GoGame:
                    Application.Current.Dispatcher.Invoke((Action<GameState>)SetState, GameState.Game);
                    SetState(GameState.Game);
                    break;
                case GameOperation.GoLoad:
                    SetState(GameState.Load);
                    break;
            }
        }
    }

    public struct GameMessage
    {
        public GameOperation operation;
        public object arg;
    }

    public enum GameOperation
    {
        DoSave,
        DoLoad,
        DoExit,
        DoSetAI,
        GoConnect,
        GoLoad,
        GoGame
    }

    public enum GameState
    {
        None,
        Connect,
        Game,
        Load
    }

    public interface IGameStateChanger
    {
        GameState GetState();
        void SetState(GameState state, object args = null);
        void SetState(GameState state);
    }
}
