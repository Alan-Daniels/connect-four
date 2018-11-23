using Connection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Connect_Four
{
    [Serializable]
    public enum CoinType
    {
        None = 0,
        Red = 1,
        Blue = -1
    }

    public class GameGrid : Canvas
    {
        public static readonly DependencyProperty GridWidthProperty = DependencyProperty.Register("GridWidth", typeof(int), typeof(GameGrid));
        public int GridWidth
        {
            get { return (int)GetValue(GridWidthProperty); }
            set
            {
                SetValue(GridWidthProperty, value);
            }
        }

        public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register("GridHeight", typeof(int), typeof(GameGrid));
        public int GridHeight
        {
            get { return (int)GetValue(GridHeightProperty); }
            set
            {
                SetValue(GridHeightProperty, value);
            }
        }

        public Size GridSize
        {
            get
            {
                return new Size
                    (
                    Width / GridWidth,
                    Height / (GridHeight + 1)
                    );
            }
        }

        private bool friendlyAIEnabled = false;
        public bool FriendlyAIEnabled
        {
            get
            {
                return friendlyAIEnabled;
            }
            set
            {
                if (value != friendlyAIEnabled)
                {
                    if (value)
                    {
                        coinTosser.RedTurn += CoinTosser_RedTurn;
                        if (coinTosser.CoinType == CoinType.Red)
                        {
                            FriendlyAI.MakeMove();
                        }
                    }
                    else
                    {
                        coinTosser.RedTurn -= CoinTosser_RedTurn;
                    }
                    friendlyAIEnabled = value;
                }
            }
        }

        private CoinType[][] coinGrid;
        private int selectedColumn = 0;
        private CoinTosser coinTosser;
        private readonly SaveGame save;

        private readonly Random random = new Random();
        private AI FriendlyAI;
        private AI EnemyAI;

        public GameGrid(SaveGame save) : base()
        {
            this.save = save;
            Setup();
        }

        private void Setup()
        {
            coinTosser = new CoinTosser(this);
            MouseLeftButtonUp += GameGrid_MouseUp;
            MouseMove += GameGrid_MouseMove;
            Loaded += GameGrid_Loaded;
            Unloaded += GameGrid_Unloaded;
        }

        public void Load(SaveGame save)
        {
            GridWidth = (int)save.GridSize.Width;
            GridHeight = (int)save.GridSize.Height;

            for (int i = 0; i < save.CoinGrid.Length; i++)
            {
                if (save.CoinGrid[i] == null)
                {
                    save.CoinGrid[i] = new CoinType[GridHeight];
                }
            }
            coinGrid = save.CoinGrid;

            coinTosser.Delete();
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (save.CoinGrid[x][y] != CoinType.None)
                    {
                        coinTosser.Create(save.CoinGrid[x][y], GridSize);
                        coinTosser.Move(new Point(x, y + 1), GridSize, TimeSpan.FromMilliseconds(250));
                    }
                }
            }
            EnemyAI = new AI(ref coinGrid, new Size(GridWidth, GridHeight), random.Next());
            EnemyAI.LocationRecieved += GameConnection_LocationRecieved;
            coinTosser.BlueTurn += CoinTosser_BlueTurn;

            coinTosser.Create(save.Turn, GridSize);
        }

        private void CoinTosser_BlueTurn(object sender, EventArgs e)
        {
            EnemyAI.MakeMove();
        }
        private void CoinTosser_RedTurn(object sender, EventArgs e)
        {
            FriendlyAI.MakeMove();
        }

        public SaveGame Save()
        {
            return new SaveGame(coinGrid, coinTosser.CoinType, new Size(GridWidth, GridHeight), "placeholder");
        }

        public void EndGame(bool serverMessage)
        {
            if (GameConnection.ConnectionType != ConnectionType.Disconnected)
            {
                if (!serverMessage)
                    GameConnection.SendMessage(new Message<object>() { Type = typeof(GameMessage).ToString(), Data = new GameMessage() { operation = GameOperation.DoExit, arg = true } });
                GameConnection.StopGame();
            }
        }

        private void GameGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            EndGame(false);
        }

        private void GameGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (save != null)
            {
                Load(save);
            }
            else
            {
                Size s = SaveGame.DefaultSize;
                GridWidth = (int)s.Width;
                GridHeight = (int)s.Height;

                GameConnection.LocationRecieved += GameConnection_LocationRecieved;
                coinGrid = new CoinType[GridWidth][];
                for (int i = 0; i < coinGrid.Length; i++)
                {
                    coinGrid[i] = new CoinType[6];
                }
                if (GameConnection.ConnectionType == ConnectionType.Server || GameConnection.ConnectionType == ConnectionType.Disconnected)
                {
                    coinTosser.Create(CoinType.Red, GridSize);
                }
                else
                {
                    coinTosser.Create(CoinType.Blue, GridSize);
                }
            }

            FriendlyAI = new AI(ref coinGrid, new Size(GridWidth, GridHeight), random.Next());
            FriendlyAI.LocationRecieved += FriendlyAI_LocationRecieved;

            Size gs = GridSize;
            var source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/boardSegment.png"));
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 1; y < GridHeight + 1; y++)
                {
                    {
                        Image img = new Image()
                        {
                            Source = source,
                            Width = gs.Width + 0.55,
                            Height = gs.Height + 0.55
                        };
                        Children.Add(img);
                        SetLeft(img, x * gs.Width);
                        SetTop(img, y * gs.Height);
                        SetZIndex(img, 2);
                    }
                }
            }

        }

        private void FriendlyAI_LocationRecieved(object sender, GameConnectionEventArgs<Point> e)
        {
            Application.Current.Dispatcher.Invoke((Action<Point>)DropCoin, e.GameObject);
            GameConnection.SendMessage(new Message<object>() { Type = typeof(Point).ToString(), Data = e.GameObject });
        }

        private void GameConnection_LocationRecieved(object sender, GameConnectionEventArgs<Point> e)
        {
            Point point = e.GameObject;
            Application.Current.Dispatcher.Invoke((Action<Point>)DropCoin, point);
        }

        private void DropCoin(Point point)
        {
            coinGrid[(int)point.X][(int)point.Y - 1] = coinTosser.CoinType;
            CheckForWin(null, new Point(point.X, point.Y - 1));

            coinTosser.Move(new Point(point.X, 0), GridSize, TimeSpan.FromMilliseconds(1));
            coinTosser.Move(point, GridSize, TimeSpan.FromMilliseconds(100 * point.Y));
            coinTosser.Create((CoinType)((int)coinTosser.CoinType * -1), GridSize);
            coinTosser.Move(new Point(selectedColumn, 0), GridSize, TimeSpan.FromMilliseconds(150));
        }

        private void CheckForWin(object sender, Point p)
        {
            bool emptyFound = false;
            byte len = 4;
            CoinType[] types = new CoinType[len];
            byte[] counts = new byte[len];
            Func<Point, int, Point>[] funcs = new Func<Point, int, Point>[] {
                (Point a, int b) => { return new Point(a.X, a.Y + b); },
                (Point a, int b) => { return new Point(a.X + b, a.Y + b); },
                (Point a, int b) => { return new Point(a.X + b, a.Y); },
                (Point a, int b) => { return new Point(a.X + b, a.Y - b); }
            };

            for (int i = -3; i <= 3; i++)
            {
                for (int w = 0; w < len; w++)
                {
                    Point a = funcs[w].Invoke(p, i);
                    CoinType ct;
                    if (a.X >= 0 && a.Y >= 0 && a.X < GridWidth && a.Y < GridHeight)
                    {
                        ct = coinGrid[(int)a.X][(int)a.Y];
                        emptyFound = ct == CoinType.None ? true : emptyFound;
                    }
                    else
                        ct = CoinType.None;

                    if (ct != CoinType.None && ct == types[w])
                        counts[w]++;
                    else
                    {
                        counts[w] = 1;
                        types[w] = ct;
                    }

                    if (counts[w] >= 4)
                    {
                        // a win has been found
                        //Console.WriteLine($"{types[w]} has won!");
                        System.Windows.Forms.MessageBox.Show($"{types[w]} has won!");
                        coinTosser.Coin.Source = coinTosser.goldCoin;
                        coinTosser.DeleteNext();
                        return;
                    }
                }
            }

            // make sure the board isnt full
            if (!emptyFound)
            {
                foreach (var coins in coinGrid)
                {
                    foreach (CoinType coin in coins)
                    {
                        if (coin == CoinType.None)
                        {
                            emptyFound = true;
                            break;
                        }
                    }
                }
            }
            if (!emptyFound)
            {
                coinTosser.DeleteNext();
                System.Windows.Forms.MessageBox.Show("The game is a Tie!");
                return;
            }
        }

        private void GameGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double x = e.GetPosition(this).X - 1;
            if (x > 0)
            {
                x /= Width;
                int tempGridX = (int)Math.Floor(x / (1D / GridWidth));
                if (tempGridX != selectedColumn)
                {
                    selectedColumn = tempGridX;
                    coinTosser.Move(new Point(selectedColumn, 0), GridSize, TimeSpan.FromMilliseconds(150));
                }
            }
        }

        private void GameGrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int y = GetHeight(selectedColumn);
            if (y != -1 && coinTosser.CoinType == CoinType.Red)
            {
                Point point = new Point(selectedColumn, y);
                DropCoin(point);
                GameConnection.SendMessage(new Message<object>() { Type = typeof(Point).ToString(), Data = point });
            }
        }

        private int GetHeight(int column)
        {
            for (int i = GridHeight - 1; i >= 0; i--)
            {
                if (coinGrid[column][i] == CoinType.None)
                {
                    return i + 1;
                }
            }
            return -1;
        }
    }

    [Serializable]
    public class SaveGame
    {
        public SaveGame(CoinType[][] CoinGrid, CoinType Turn, Size GridSize, string Name)
        {
            this.CoinGrid = CoinGrid;
            this.Turn = Turn;
            this.GridSize = GridSize;
            this.Name = Name;
        }
        public SaveGame() { }
        public CoinType[][] CoinGrid { get; set; }
        public CoinType Turn { get; set; }
        public Size GridSize { get; set; }
        public string Name { get; set; }

        public static SaveGame Default
        {
            get
            {
                return new SaveGame(new CoinType[(int)DefaultSize.Width][], CoinType.Red, DefaultSize, "default");
            }
        }

        public static Size DefaultSize
        {
            get
            {
                return new Size(7, 6);
            }
        }
    }
}
