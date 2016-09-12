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

namespace a7SqlTools.DbSearch.Views
{
    /// <summary>
    /// Interaction logic for SeperatorSelector.xaml
    /// </summary>
    public partial class SeperatorSelector : Window
    {
        public string Seperator => tbSeperator.Text;
        public string AndSeperator => tbAndSeperator.Text;

        public SeperatorSelector(string Seperator, string AndSeperator)
        {
            InitializeComponent();
            tbSeperator.Text = Seperator;
            tbAndSeperator.Text = AndSeperator;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
