using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Connect_Four
{
    [Serializable]
    class CoinTosser
    {
        private Ellipse coin;
        private Point location;
        private readonly Canvas canvas;

        public CoinTosser(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void Move(Point newLocation, Size gridSize, TimeSpan timeSpan)
        {
            TranslateTransform trans = new TranslateTransform();
            coin.RenderTransform = trans;
            DoubleAnimation animX = new DoubleAnimation((location.X * gridSize.Width), (newLocation.X * gridSize.Width), timeSpan);
            DoubleAnimation animY = new DoubleAnimation((location.Y * gridSize.Height), (newLocation.Y * gridSize.Height), timeSpan);
            trans.BeginAnimation(TranslateTransform.XProperty, animX);
            trans.BeginAnimation(TranslateTransform.YProperty, animY);
            location = newLocation;
        }

        public void Create(CoinType coinType, Size gridSize)
        {
            switch (coinType)
            {
                case CoinType.None:
                    throw new InvalidOperationException();
                case CoinType.Red:
                    coin = new Ellipse()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Fill = Brushes.Red
                    };
                    break;
                case CoinType.Blue:
                    coin = new Ellipse()
                    {
                        Width = gridSize.Width,
                        Height = gridSize.Height,
                        Fill = Brushes.Blue
                    };
                    break;
            }
            location = new Point(location.X, 0);
            Move(location, gridSize, TimeSpan.FromMilliseconds(1));
            canvas.Children.Add(coin);
        }
    }
}
