using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Connect_Four
{
    /// <summary>
    /// A helper class for GameGrid that controls the animation and position of coins.
    /// </summary>
    [Serializable]
    public class CoinTosser
    {
        public event EventHandler BlueTurn;
        public event EventHandler RedTurn;

        public Image Coin;
        private Point location;
        private readonly Canvas canvas;

        public BitmapImage redCoin = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/coin_red.png"));
        public BitmapImage blueCoin = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/coin_blue.png"));
        public BitmapImage goldCoin = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/coin_gold.png"));

        public CoinType CoinType { get; private set; }

        public CoinTosser(Canvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Moves the current coin.
        /// </summary>
        /// <param name="newLocation">A point describing the new location the coin should move to.</param>
        /// <param name="gridSize">The width and height of one grid.</param>
        /// <param name="timeSpan">The amount of time to take moving</param>
        public void Move(Point newLocation, Size gridSize, TimeSpan timeSpan)
        {
            if (Coin != null)
            {
                TranslateTransform trans = new TranslateTransform();
                Coin.RenderTransform = trans;
                DoubleAnimation animX = new DoubleAnimation((location.X * gridSize.Width), (newLocation.X * gridSize.Width), timeSpan);
                DoubleAnimation animY = new DoubleAnimation((location.Y * gridSize.Height), (newLocation.Y * gridSize.Height), timeSpan);
                trans.BeginAnimation(TranslateTransform.XProperty, animX);
                trans.BeginAnimation(TranslateTransform.YProperty, animY);
                location = newLocation;
            }
        }

        /// <summary>
        /// Removes the current coin from the canvas and its own memory.
        /// </summary>
        public void Delete()
        {
            canvas.Children.Remove(Coin);
            Coin = null;
            CoinType = CoinType.None;
        }

        /// <summary>
        /// Cancels creation of a new coin before it is finished.
        /// </summary>
        bool deleteNext = false;
        public void DeleteNext()
        {
            deleteNext = true;
        }

        /// <summary>
        /// Creates a new coin, dropping reference to the previouse and showing it on the canvas.
        /// </summary>
        /// <param name="coinType">Type of coin to use.</param>
        /// <param name="gridSize">The size of one grid.</param>
        public void Create(CoinType coinType, Size gridSize)
        {
            CoinType = coinType;
            switch (coinType)
            {
                case CoinType.None:
                case CoinType.Red:
                    Coin = new Image()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Source = redCoin
                    };
                    break;
                case CoinType.Blue:
                    Coin = new Image()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Source = blueCoin
                    };
                    break;
            }
            canvas.Children.Add(Coin);
            location = new Point(location.X, 0);
            Move(location, gridSize, TimeSpan.FromMilliseconds(1));

            if (coinType == CoinType.None || deleteNext)
            {
                deleteNext = false;
                Delete();
            }
            else
            {
                switch (coinType)
                {
                    case CoinType.Red:
                        RedTurn?.Invoke(this, null);
                        break;
                    case CoinType.Blue:
                        BlueTurn?.Invoke(this, null);
                        break;
                }
            }
        }
    }
}
