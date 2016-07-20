using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using a7SqlTools.Dialogs;
using a7SqlTools.Utils;
using a7SqlTools.DbSearch;
using a7SqlTools.Connection;
using System.Windows;
using System.Data.SqlClient;
using a7SqlTools.Config.Model;
using a7SqlTools.Config;

namespace a7SqlTools
{
    public class AppViewModel : ViewModelBase
    {
        public ObservableCollection<ConnectionViewModel> Connections { get; set; }

        private ViewModelBase _selectedItem;

        public ViewModelBase SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        public ICommand ConnectToDb => new a7LambdaCommand((o) =>
        {
            var dlg = new Connect(async (vm) =>
            {
                if(string.IsNullOrWhiteSpace(vm.Name))
                {
                    return new Tuple<bool, string>(false, "Server name is required");
                }
                try
                {
                    using (new BusyViewModel(this, "Testing connection..."))
                    {
                        using (new BusyViewModel(vm))
                        {
                            using (var conn = new SqlConnection(vm.ConnectionString))
                            {
                                await conn.OpenAsync();
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                    return new Tuple<bool, string>(false, "Connection not succesful");
                }
                //TODO: test connection string
                return new Tuple<bool, string>(true, null);
            });
            if (dlg.ShowDialog() == true)
            {
                if(Connections.Any(c => c.Name == dlg.ViewModel.Name))
                {
                    MessageBox.Show("Connection with such server name is already added.");
                    return;
                }
                var connectionData = dlg.ViewModel.ToConnectionData();
                var connectionViewModel = new ConnectionViewModel(connectionData, this);
                Connections.Add(connectionViewModel);
                ConfigService.Instance.AddConnection(connectionViewModel.ToConfigModel());
                //DBSearchEngines.Add(new a7SqlToolsEngine(dlg.ViewModel.Name, connectionString));
            }
        });
        public ICommand DisconnectFromDb => new a7LambdaCommand((o) =>
        {
            if (this.SelectedItem is ConnectionViewModel)
            {
                var connectionViewModel = this.SelectedItem as ConnectionViewModel;
                Connections.Remove(connectionViewModel);
                ConfigService.Instance.RemoveConnection(
                    ConnectionStringGenerator.Get(connectionViewModel.ConnectionData)
                    );
            }
        });
        public ICommand GoToHome => new a7LambdaCommand(o =>
        {
            this.SelectedItem = null;
        });

        public AppViewModel()
        {
            Connections = new ObservableCollection<ConnectionViewModel>();
            foreach(var connItem in ConfigService.Instance.GetConfiguration().Connections)
            {
                if(connItem.ConnectionData!= null)
                    Connections.Add(new ConnectionViewModel(connItem, this));
            }
        }
        
    }
}
