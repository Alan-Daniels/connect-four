using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGameStateChanger
    {
        SavableList<GameGrid> SavedGames;
        private GameState gameState = GameState.None;

        public MainWindow()
        {
            InitializeComponent();
            SavedGames = new SavableList<GameGrid>(new FileInfo("savegames.json"));
            SavedGames.Load();
            SetState(GameState.Connect);
        }

        public GameState GetState()
        {
            return gameState;
        }

        public void SetState(GameState state)
        {
            SetState(state, null, null);
        }

        public void SetState(GameState state, UIElement element = null, object args = null)
        {
            if (gameState != state)
            {
                switch (state)
                {
                    case GameState.Connect:
                        var connectGame = new ConnectGame();
                        connectGame.NewGameMessage += RecieveMessage;
                        root.Child = connectGame;
                        overlay.Child = null;
                        break;
                    case GameState.Game:
                        if (element == null)
                        {
                            root.Child = new GameGrid()
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                GridHeight = 6,
                                GridWidth = 7,
                                Height = 1050,
                                Width = 1050,
                                Background = Brushes.Linen
                            };
                        }
                        else
                        {
                            root.Child = element;
                        }
                        var gameOverlay = new GameOverlay();
                        gameOverlay.NewGameMessage += RecieveMessage;
                        overlay.Child = gameOverlay;
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
                    SavedGames.Add((GameGrid)overlay.Child);
                    SavedGames.Save();
                    break;
                case GameOperation.GoConnect:
                    SetState(GameState.Connect);
                    break;
                case GameOperation.DoLoad:
                    SetState(GameState.Game, SavedGames[(int)e.arg]);
                    break;
                case GameOperation.GoGame:
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
        void SetState(GameState state, UIElement element = null, object args = null);
        void SetState(GameState state);
    }
}
