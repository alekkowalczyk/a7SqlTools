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
using a7SqlTools.Utils;
using Microsoft.SqlServer.Management.Common;

namespace a7SqlTools.DbComparer.Struct.Views
{
    /// <summary>
    /// Interaction logic for a7DbSelection.xaml
    /// </summary>
    public partial class a7DbSelection : Window
    {
        public List<string> DbList { get; private set; }
        public string Db1 { get; set; }
        public string Db2 { get; set; }

        public a7DbSelection(ServerConnection connection)
        {
            //TODO: Wait() is not niceDbList = a7DbUtils.GetDbNames(connection).Result;
            InitializeComponent();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
