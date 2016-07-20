using a7SqlTools.Config.Model;
using a7SqlTools.DbSearch;
using a7SqlTools.Dialogs;
using a7SqlTools.TableExplorer;
using a7SqlTools.Utils;
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Connection
{
    public class ConnectionViewModel : ViewModelBase
    {
        public string Name { get; private set; }
        public ObservableCollection<object> Children { get; private set; }
        public bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set {
                _isExpanded = value;
                OnPropertyChanged();
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
        }
        public ConnectionData ConnectionData { get; private set; }

        public a7LambdaCommand SearchDb => new a7LambdaCommand(async o =>
        {
            ServerConnection srvConnection;
            if (ConnectionData.AuthType == AuthType.Sql)
                srvConnection = new ServerConnection(ConnectionData.Name, ConnectionData.UserName, ConnectionData.Password);
            else
                srvConnection = new ServerConnection(ConnectionData.Name);
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(srvConnection);
            }
            var dlg = new DbSelector(list);
            if(dlg.ShowDialog() == true)
            {
                ConnectionData.DbSearches.Add(new DbSearchData { DatabaseName = dlg.SelectedDatabaseName, CreatedAt = DateTime.Now });
                this.Children.Add(new a7DbSearchEngine(dlg.SelectedDatabaseName, ConnectionData));
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
        });

        public a7LambdaCommand TableExplorer => new a7LambdaCommand(async o =>
        {
            ServerConnection srvConnection;
            if (ConnectionData.AuthType == AuthType.Sql)
                srvConnection = new ServerConnection(ConnectionData.Name, ConnectionData.UserName, ConnectionData.Password);
            else
                srvConnection = new ServerConnection(ConnectionData.Name);
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(srvConnection);
            }
            var dlg = new DbSelector(list);
            if (dlg.ShowDialog() == true)
            {
                ConnectionData.TableExplorers.Add(new TableExplorerData { DatabaseName = dlg.SelectedDatabaseName, CreatedAt = DateTime.Now });
                this.Children.Add(new a7TableExplorer(dlg.SelectedDatabaseName, ConnectionData));
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
        });

        private AppViewModel _appVm;

        public ConnectionViewModel(ConnectionItem connectionConfigItem, AppViewModel appVm) : this(connectionConfigItem.ConnectionData, appVm)
        {
            this._isExpanded = connectionConfigItem.IsExpanded;
        }

        public ConnectionViewModel(ConnectionData connectionData, AppViewModel appVm)
        {
            this._isExpanded = false;
            this._appVm = appVm;
            this.Name = connectionData.Name;
            this.ConnectionData = connectionData;
            var ichildrenToOrder = new List<IWithCreatedAt>(this.ConnectionData.DbSearches);
            ichildrenToOrder = ichildrenToOrder.Concat(this.ConnectionData.TableExplorers).ToList();
            ichildrenToOrder = ichildrenToOrder.OrderByDescending(i => i.CreatedAt).ToList();

            this.Children = new ObservableCollection<object>();
            foreach(var childData in ichildrenToOrder)
            {
                var dbSearchData = childData as DbSearchData;
                if(dbSearchData != null)
                {
                    this.Children.Add(new a7DbSearchEngine(dbSearchData.DatabaseName, ConnectionData));
                    continue;
                }
                var tblExpData = childData as TableExplorerData;
                if (tblExpData != null)
                {
                    this.Children.Add(new a7TableExplorer(tblExpData.DatabaseName, ConnectionData));
                }
            }
        }

        public ConnectionItem ToConfigModel() =>
            new ConnectionItem
            {
                ConnectionData = this.ConnectionData,
                IsExpanded = this.IsExpanded
            };
    }
}
