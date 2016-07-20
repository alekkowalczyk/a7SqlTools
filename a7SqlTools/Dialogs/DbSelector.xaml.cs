using a7SqlTools.Utils;
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace a7SqlTools.Dialogs
{
    /// <summary>
    /// Interaction logic for DbSelector.xaml
    /// </summary>
    public partial class DbSelector : Window, INotifyPropertyChanged
    {
        private string _selectedDatabaseName;
        public string SelectedDatabaseName {
            get { return _selectedDatabaseName; }
            set {
                _selectedDatabaseName = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedDatabaseName)));
            }
        }
        public List<string> DbList { get; private set; }

        public DbSelector(List<string> dbList)
        {
            DbList = dbList;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
