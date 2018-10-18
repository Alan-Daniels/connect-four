﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Connect_Four
{
    enum CoinType
    {
        None = 0,
        Red = -1,
        Blue = 1
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

        private CoinType[,] coinGrid;
        private int selectedColumn = 0;
        private bool YourTurn;
        private readonly CoinTosser coinTosser;

        public GameGrid() : base()
        {
            MouseLeftButtonUp += GameGrid_MouseUp;
            MouseMove += GameGrid_MouseMove;
            Loaded += GameGrid_Loaded;
            coinTosser = new CoinTosser(this);
        }

        private void GameGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Size gs = GridSize;
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 1; y < GridHeight + 1; y++)
                {
                    {
                        Image img = new Image()
                        {
                            Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/boardSegment.png")),
                            Width = gs.Width+0.55,
                            Height = gs.Height+0.55
                        };
                        Children.Add(img);
                        SetLeft(img, x * gs.Width);
                        SetTop(img, y * gs.Height);
                        SetZIndex(img, 2);
                    }
                }
            }
            coinTosser.Create(CoinType.Red, GridSize);
            coinGrid = new CoinType[GridWidth, GridHeight];
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
            if (y != -1)
            {
                coinTosser.Move(new Point(selectedColumn, y), GridSize, TimeSpan.FromMilliseconds(100 * y));
                coinGrid[selectedColumn, y - 1] = coinTosser.CoinType;
                coinTosser.Create((CoinType)((int)coinTosser.CoinType*-1), GridSize);
            }
        }

        private int GetHeight(int column)
        {
            for (int i = GridHeight - 1; i >= 0; i--)
            {
                if (coinGrid[column, i] == CoinType.None)
                {
                    return i + 1;
                }
            }
            return -1;
        }
    }
}
