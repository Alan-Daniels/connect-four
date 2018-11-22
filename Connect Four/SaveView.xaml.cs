using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            TxtName.Text = save.Name;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            GameSelected?.Invoke(this, Save);
        }
    }
}
