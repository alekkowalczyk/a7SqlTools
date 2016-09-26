using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for a7DbTableComparerRowsWindow.xaml
    /// </summary>
    public partial class a7DbTableComparerRowsWindow : Window
    {
        public ObservableCollection<DataGridColumn> Columns { get; private set; }

        public a7DbTableComparerRowsWindow(ObservableCollection<DataGridColumn> columns)
        {
            Columns = columns;
            InitializeComponent();
        }
    }
}
