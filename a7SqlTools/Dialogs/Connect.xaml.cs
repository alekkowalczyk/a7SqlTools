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

namespace a7SqlTools.Dialogs
{
    /// <summary>
    /// Interaction logic for Connect.xaml
    /// </summary>
    public partial class Connect : Window
    {
        public ConnectViewModel ViewModel => this.DataContext as ConnectViewModel;
        private Func<ConnectViewModel, Task<Tuple<bool, string>>> _testConnectionString;

        /// <summary>
        /// if provided function returnes in tuple item1 true, dialog closes, if false, displays item2 as error message
        /// </summary>
        /// <param name="testConnectionString"></param>
        public Connect(Func<ConnectViewModel, Task<Tuple<bool, string>>> testConnectionString)
        {
            InitializeComponent();
            this.DataContext = new ConnectViewModel();
            _testConnectionString = testConnectionString;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void bConnect_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
                this.ViewModel.Password = pswd.Password;
            var test = await _testConnectionString(this.ViewModel);
            if (test.Item1)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                this.ViewModel.ErrorMessage = test.Item2;
            }
        }
    }
}
