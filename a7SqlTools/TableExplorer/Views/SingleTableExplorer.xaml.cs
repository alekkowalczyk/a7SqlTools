using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace a7SqlTools.TableExplorer.Views
{
    /// <summary>
    /// Interaction logic for DBExplorer.xaml
    /// </summary>
    public partial class SingleTableExplorer : UserControl
    {
        public a7SingleTableExplorer ViewModel
        {
            get
            {
                if (this.DataContext is a7SingleTableExplorer)
                    return this.DataContext as a7SingleTableExplorer;
                else
                    return null;
            }
        }

        public SingleTableExplorer()
        {
            InitializeComponent();
        }

        private void bFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Refresh();
        }

        private void bCommitChanges_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CommitChanges();
        }
    }
}
