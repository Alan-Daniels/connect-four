using System;
using System.Windows;
using System.Windows.Controls;

namespace Connect_Four
{
    enum CoinType
    {
        None,
        Red,
        Blue
    }

    [Serializable]
    class GameGrid : Canvas
    {
        public static readonly DependencyProperty GridWidthProperty = DependencyProperty.Register("GridWidth", typeof(int), typeof(GameGrid));
        public int GridWidth
        {
            get { return (int)GetValue(GridWidthProperty); }
            set
            {
                SetValue(GridWidthProperty, value);
                coinGrid = new CoinType[GridWidth, GridHeight];
            }
        }

        public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register("GridHeight", typeof(int), typeof(GameGrid));
        public int GridHeight
        {
            get { return (int)GetValue(GridHeightProperty); }
            set
            {
                SetValue(GridHeightProperty, value);
                coinGrid = new CoinType[GridWidth, GridHeight];
            }
        }

        private CoinType[,] coinGrid;
        private int selectedColumn = 0;
        private bool YourTurn;

        public GameGrid() : base()
        {
            MouseLeftButtonUp += GameGrid_MouseUp;
            MouseMove += GameGrid_MouseMove;
        }

        private void GameGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double x = e.GetPosition(this).X;
            if (x > 0)
            {
                x /= Width;
                int tempGridX = (int)Math.Floor(x / (1D / GridWidth));
                if (tempGridX != selectedColumn)
                {
                    selectedColumn = tempGridX;
                }
            }
        }

        private void GameGrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AddCoin())
            {
                YourTurn = !YourTurn;
            }
        }

        private bool AddCoin()
        {
            return !ColumnFull();
        }

        private bool ColumnFull()
        {
            return true;
        }
    }
}
