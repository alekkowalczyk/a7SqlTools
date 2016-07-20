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

namespace a7SqlTools.TableExplorer.Views
{
    /// <summary>
    /// Interaction logic for TableExplorer.xaml
    /// </summary>
    public partial class TableExplorer : UserControl
    {
        public a7TableExplorer ViewModel
        {
            get
            {
                if (DataContext is a7TableExplorer)
                    return DataContext as a7TableExplorer;
                else return null;
            }
        }

        public TableExplorer()
        {
            InitializeComponent();
        }

        private void tbTableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel?.RefreshDictTables(tbTableFilter.Text);
        }
    }
}
