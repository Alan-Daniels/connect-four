using System.Windows;
using System.Windows.Media;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGameStateChanger
    {
        private GameState gameState = GameState.Null;

        public MainWindow()
        {
            InitializeComponent();

            SetState(GameState.Connect, null);
        }

        public GameState GetState()
        {
            return gameState;
        }

        public void SetState(GameState state, object gameArgs)
        {
            if (gameState != state)
            {
                switch (state)
                {
                    case GameState.Connect:
                        root.Child = new ConnectGame()
                        {
                            GameStateChanger = this
                        };
                        break;
                    case GameState.Game:
                        root.Child = new GameGrid()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            GridHeight = 6,
                            GridWidth = 7,
                            Height = 400,
                            Width = 400,
                            Background = Brushes.Aqua
                        };
                        break;
                }
                gameState = state;
            }
        }
    }

    public enum GameState
    {
        Null,
        Connect,
        Game
    }

    public interface IGameStateChanger
    {
        GameState GetState();
        void SetState(GameState state, object gameArgs);
    }
}
