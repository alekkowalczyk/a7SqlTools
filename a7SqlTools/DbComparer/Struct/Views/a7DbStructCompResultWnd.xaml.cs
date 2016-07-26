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

namespace a7SqlTools.DbComparer.Struct.Views
{
    /// <summary>
    /// Interaction logic for a7DbStructCompResultWnd.xaml
    /// </summary>
    public partial class a7DbStructCompResultWnd : Window
    {
        public a7DbStructureComparer Comparer { get; private set; }

        public a7DbStructCompResultWnd(a7DbStructureComparer comparer)
        {
            Comparer = comparer;
            this.DataContext = comparer;
            InitializeComponent();
        }
    }
}
