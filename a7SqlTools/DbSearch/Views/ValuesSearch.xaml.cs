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
using System.IO;
using System.Diagnostics;

namespace a7SqlTools.DbSearch.Views
{
    /// <summary>
    /// Interaction logic for ValuesSearch.xaml
    /// </summary>
    public partial class ValuesSearch : UserControl
    {
        public AppViewModel ViewModel
        {
            get { return DataContext as AppViewModel; }
        }


        public ValuesSearch()
        {
            InitializeComponent();
 
        }

    }
}
