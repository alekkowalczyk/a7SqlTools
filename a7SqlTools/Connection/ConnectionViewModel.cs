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
using System.Windows;
using a7SqlTools.DbComparer.Data;
using a7SqlTools.DbComparer.Data.Views;
using a7SqlTools.DbComparer.Struct;
using a7SqlTools.DbComparer.Struct.Views;
using a7SqlTools.DbComparer.Views;

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

        public bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }


        public ConnectionData ConnectionData { get; private set; }

        public a7LambdaCommand SearchDb => new a7LambdaCommand(async o =>
        {
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(ConnectionData);
            }
            var dlg = new DbSelector(list);
            if(dlg.ShowDialog() == true)
            {
                if (ConnectionData.DbSearches.Any(ds => ds.DatabaseName == dlg.SelectedDatabaseName))
                {
                    MessageBox.Show("A search for this database already exists.");
                    return;
                }
                ConnectionData.DbSearches.Add(new DbSearchData { DatabaseName = dlg.SelectedDatabaseName, CreatedAt = DateTime.Now });
                var dbSearch = new a7DbSearchEngine(dlg.SelectedDatabaseName, ConnectionData, this);
                this.Children.Add(dbSearch);
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
                AppViewModel.Instance.SelectedItem = dbSearch;
            }
        });

        public a7LambdaCommand TableExplorer => new a7LambdaCommand(async o =>
        {
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(ConnectionData);
            }
            var dlg = new DbSelector(list);
            if (dlg.ShowDialog() == true)
            {
                if (ConnectionData.TableExplorers.Any(ds => ds.DatabaseName == dlg.SelectedDatabaseName))
                {
                    MessageBox.Show("A table explorer for this database already exists.");
                    return;
                }
                ConnectionData.TableExplorers.Add(new TableExplorerData { DatabaseName = dlg.SelectedDatabaseName, CreatedAt = DateTime.Now });
                var tableExplorer = new a7TableExplorer(dlg.SelectedDatabaseName, ConnectionData, this);
                this.Children.Add(tableExplorer);
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
                AppViewModel.Instance.SelectedItem = tableExplorer;
            }
        });

        public a7LambdaCommand CompareStructure => new a7LambdaCommand(async o =>
        {
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(ConnectionData);
            }
            var dlg = new a7DbDoubleSelection(list);
            if (dlg.ShowDialog() == true)
            {
                if (ConnectionData.StructComparers.Any(ds => ds.DbA == dlg.Db1 && ds.DbB == dlg.Db2))
                {
                    MessageBox.Show("A structure comparison between those databases already exists.");
                    return;
                }
                a7DbStructureComparerVM vm = new a7DbStructureComparerVM(dlg.Db1, dlg.Db2, ConnectionData, this);
                ConnectionData.StructComparers.Add(new StructComparerData { DbA = dlg.Db1, DbB = dlg.Db2, CreatedAt = DateTime.Now });
                this.Children.Add(vm);
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
                AppViewModel.Instance.SelectedItem = vm;
                await vm.Init();
            }
        });

        public a7LambdaCommand CompareData => new a7LambdaCommand(async o =>
        {
            List<string> list;
            using (new BusyViewModel(_appVm, "Getting database collection..."))
            {
                list = await a7DbUtils.GetDbNames(ConnectionData);
            }
            var dlg = new a7DbDoubleSelection(list);
            if (dlg.ShowDialog() == true)
            {
                if (ConnectionData.DataComparers.Any(ds => ds.DbA == dlg.Db1 && ds.DbB == dlg.Db2))
                {
                    MessageBox.Show("A data comparison between those databases already exists.");
                    return;
                }
                a7DbDataComparerVM vm = new a7DbDataComparerVM(dlg.Db1, dlg.Db2, ConnectionData, this);
                ConnectionData.DataComparers.Add(new DataComparerData { DbA = dlg.Db1, DbB = dlg.Db2, CreatedAt = DateTime.Now });
                this.Children.Add(vm);
                this.IsExpanded = true;
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
                AppViewModel.Instance.SelectedItem = vm;
                await vm.Init();
            }
        });

        private AppViewModel _appVm;

        public ConnectionViewModel(ConnectionItem connectionConfigItem, AppViewModel appVm) : this(connectionConfigItem.ConnectionData, appVm)
        {
            this._isExpanded = connectionConfigItem.IsExpanded;
        }

        private ServerConnection GetConnection() =>
            ConnectionData.AuthType == AuthType.Sql
            ? new ServerConnection(ConnectionData.Name, ConnectionData.UserName, ConnectionData.Password)
            : new ServerConnection(ConnectionData.Name);

        public ConnectionViewModel(ConnectionData connectionData, AppViewModel appVm)
        {
            this._isExpanded = false;
            this._appVm = appVm;
            this.Name = connectionData.Name;
            this.ConnectionData = connectionData;
            //var ichildrenToOrder = new List<IWithCreatedAt>(this.ConnectionData.DbSearches);
            //ichildrenToOrder = ichildrenToOrder.Concat(this.ConnectionData.TableExplorers).ToList();
            //ichildrenToOrder = ichildrenToOrder.Concat(this.ConnectionData.StructComparers).ToList();
            //ichildrenToOrder = ichildrenToOrder.OrderByDescending(i => i.CreatedAt).ToList();

            var ichildrenToOrder = (this.ConnectionData.DbSearches as IEnumerable<IWithCreatedAt>)
                .Union(ConnectionData.DbSearches as IEnumerable<IWithCreatedAt>)
                .Union(ConnectionData.StructComparers as IEnumerable<IWithCreatedAt>)
                .Union(ConnectionData.DataComparers as IEnumerable<IWithCreatedAt>)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();


            this.Children = new ObservableCollection<object>();
            foreach(var childData in ichildrenToOrder)
            {
                var dbSearchData = childData as DbSearchData;
                if(dbSearchData != null)
                {
                    this.Children.Add(new a7DbSearchEngine(dbSearchData.DatabaseName, ConnectionData, this));
                    continue;
                }
                var tblExpData = childData as TableExplorerData;
                if (tblExpData != null)
                {
                    this.Children.Add(new a7TableExplorer(tblExpData.DatabaseName, ConnectionData, this));
                }
                var structComparer = childData as StructComparerData;
                if (structComparer != null)
                {
                    this.Children.Add(new a7DbStructureComparerVM(structComparer.DbA, structComparer.DbB, ConnectionData, this, true));
                }
                var dataComparer = childData as DataComparerData;
                if (dataComparer != null)
                {
                    this.Children.Add(new a7DbDataComparerVM(dataComparer.DbA, dataComparer.DbB, ConnectionData, this, true));
                }
            }
        }

        public void RemoveChild(object child)
        {
            this.Children.Remove(child);
            var dbSearch = child as a7DbSearchEngine;
            if (dbSearch != null)
            {
               ConnectionData.DbSearches.RemoveItem(d => d.DatabaseName == dbSearch.Name);
               Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
            var tableExplorer = child as a7TableExplorer;
            if (tableExplorer != null)
            {
                ConnectionData.TableExplorers.RemoveItem(d => d.DatabaseName == tableExplorer.DbName);
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
            var structComparer = child as a7DbStructureComparerVM;
            if (structComparer != null)
            {
                ConnectionData.StructComparers.RemoveItem(d => d.DbB == structComparer.DbB && d.DbA == structComparer.DbA);
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
            }
            var dataComparer = child as a7DbDataComparerVM;
            if (dataComparer != null)
            {
                ConnectionData.DataComparers.RemoveItem(d => d.DbB == dataComparer.DbB && d.DbA == dataComparer.DbA);
                Config.ConfigService.Instance.UpdateConnection(this.ToConfigModel());
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
