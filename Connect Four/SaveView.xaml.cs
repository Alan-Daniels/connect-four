using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Connect_Four
{
    /// <summary>
    /// Interaction logic for SaveView.xaml
    /// </summary>
    public partial class SaveView : Grid
    {
        public event EventHandler<SaveGame> GameSelected;
        public SaveGame Save { get; private set; }

        public SaveView(SaveGame save)
        {
            InitializeComponent();
            Save = save;
            BtnLoad.Content = save.Name;
            Preview.Height = 100 * (save.GridSize.Height / save.GridSize.Width);
            Height = Preview.Height + BtnLoad.Height;
            LoadPreview();
        }

        private void LoadPreview()
        {
            var coinTosser = new CoinTosser(Preview);

            Size gs = Save.GridSize;
            Size rgs = new Size(Preview.Width/Save.GridSize.Width, Preview.Height/Save.GridSize.Height);
            Size rrgs = new Size(Preview.Width, Preview.Height);
            var source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/boardSegment.png"));

            for (int x = 0; x < gs.Width; x++)
            {
                for (int y = 0; y < gs.Height; y++)
                {
                    {
                        Image img = new Image()
                        {
                            Source = source,
                            Width = rgs.Width,
                            Height = rgs.Height
                        };
                        Preview.Children.Add(img);
                        Canvas.SetLeft(img, x * rgs.Width);
                        Canvas.SetTop(img, y * rgs.Height);
                        SetZIndex(img, 2);
                    }
                }
            }

            for (int x = 0; x < gs.Width; x++)
            {
                for (int y = 0; y < gs.Height; y++)
                {
                    if (Save.CoinGrid[x][y] != CoinType.None)
                    {
                        coinTosser.Create(Save.CoinGrid[x][y], rgs);
                        coinTosser.Move(new Point(x, y), rgs, TimeSpan.FromMilliseconds(250));
                    }
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            GameSelected?.Invoke(this, Save);
        }
    }
}
