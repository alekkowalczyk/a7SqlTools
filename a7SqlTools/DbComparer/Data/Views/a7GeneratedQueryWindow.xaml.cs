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
using System.Windows.Shapes;

namespace a7SqlTools.DbComparer.Data.Views
{
    /// <summary>
    /// Interaction logic for a7GeneratedQueryWindow.xaml
    /// </summary>
    public partial class a7GeneratedQueryWindow : Window
    {
        public a7GeneratedQueryWindow(string w)
        {
            InitializeComponent();
            tb.Text = w;
        }

        private void bClose_Click(object s, RoutedEventArgs re)
        {
            this.Close();
        }
    }
}
