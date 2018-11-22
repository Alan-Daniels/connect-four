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

        private Image coin;
        private Point location;
        private readonly Canvas canvas;

        private readonly BitmapImage redCoin = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/coin_red.png"));
        private readonly BitmapImage blueCoin = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/coin_blue.png"));

        public CoinType CoinType { get; private set; }

        public CoinTosser(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void Move(Point newLocation, Size gridSize, TimeSpan timeSpan)
        {
            if (coin != null)
            {
                TranslateTransform trans = new TranslateTransform();
                coin.RenderTransform = trans;
                DoubleAnimation animX = new DoubleAnimation((location.X * gridSize.Width), (newLocation.X * gridSize.Width), timeSpan);
                DoubleAnimation animY = new DoubleAnimation((location.Y * gridSize.Height), (newLocation.Y * gridSize.Height), timeSpan);
                trans.BeginAnimation(TranslateTransform.XProperty, animX);
                trans.BeginAnimation(TranslateTransform.YProperty, animY);
                location = newLocation;
            }
        }

        public void Delete()
        {
            canvas.Children.Remove(coin);
            coin = null;
            CoinType = CoinType.None;
        }

        public void Create(CoinType coinType, Size gridSize)
        {
            CoinType = coinType;
            switch (coinType)
            {
                case CoinType.None:
                case CoinType.Red:
                    coin = new Image()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Source = redCoin
                    };
                    break;
                case CoinType.Blue:
                    coin = new Image()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Source = blueCoin
                    };
                    break;
            }
            location = new Point(location.X, 0);
            Move(location, gridSize, TimeSpan.FromMilliseconds(1));
            canvas.Children.Add(coin);

            if (coinType == CoinType.None)
            {
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
