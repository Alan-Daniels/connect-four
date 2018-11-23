using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Connect_Four
{
    [Serializable]
    class CoinTosser
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

        public void Delete()
        {
            canvas.Children.Remove(Coin);
            Coin = null;
            CoinType = CoinType.None;
        }

        bool deleteNext = false;
        public void DeleteNext()
        {
            deleteNext = true;
        }

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
