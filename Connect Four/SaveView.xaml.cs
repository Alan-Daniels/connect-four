using System;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            GameSelected?.Invoke(this, Save);
        }
    }
}
