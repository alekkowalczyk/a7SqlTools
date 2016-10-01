using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using a7SqlTools.Annotations;
using a7SqlTools.Utils;
using Microsoft.SqlServer.Management.Common;

namespace a7SqlTools.DbComparer.Views
{
    /// <summary>
    /// Interaction logic for a7DbDoubleSelection.xaml
    /// </summary>
    public partial class a7DbDoubleSelection : Window, INotifyPropertyChanged
    {
        private string _db1;
        private string _db2;
        public List<string> DbList { get; private set; }
        public bool IsOkEnabled { get; private set; }
        public string Db1
        {
            get { return _db1; }
            set
            {
                _db1 = value;
                IsOkEnabled = !string.IsNullOrWhiteSpace(_db1) && !string.IsNullOrWhiteSpace(_db2);
                OnPropertyChanged(nameof(IsOkEnabled));
            }
        }

        public string Db2
        {
            get { return _db2; }
            set
            {
                _db2 = value;
                IsOkEnabled = !string.IsNullOrWhiteSpace(_db1) && !string.IsNullOrWhiteSpace(_db2);
                OnPropertyChanged(nameof(IsOkEnabled));
            }
        }

        public a7DbDoubleSelection(List<string> dbList)
        {
            IsOkEnabled = false;
            DbList = dbList;
            InitializeComponent();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
