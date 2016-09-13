using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.Utils;

namespace a7SqlTools.TableExplorer
{
    public class a7SingleTableExplorer : INotifyPropertyChanged
    {
        public string TableName { get; private set; }

        public DataTable Data
        {
            get { return _data; }
            private set { _data = value; OnPropertyChanged(nameof(Data)); }
        }

        private SqlDataAdapter DataAdapter;
        private string _combinedSql;

        public string SQL
        {
            get { return _sql; }
            set { _sql = value; OnPropertyChanged(nameof(SQL)); }
        }

        private string _columnWhere;
        public event EventHandler PleaseClearColumnFilters;
        public Dictionary<string, string> FilterFields
        {
            get { return _filterFields; }
            set
            {
                IsSqlEditMode = false;
                _filterFields = value;
                Filter2Sql();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        private ObservableCollection<ColumnDefinition> _availableColumns;
        public ObservableCollection<ColumnDefinition> AvailableColumns
        {
            get { return _availableColumns; }
            private set
            {
                _availableColumns = value;
                OnPropertyChanged(nameof(AvailableColumns));
            }
        }

        public bool IsSqlEditMode
        {
            get { return _isSqlEditMode; }
            set
            {
                _isSqlEditMode = value; OnPropertyChanged(nameof(IsSqlEditMode));
                if (value)
                {
                    ClearFieldFilters();
                }
            }
        }

        public ICommand Remove => new a7LambdaCommand((o) => _parentVm.OpenedTables.Remove(this));

        private readonly a7TableExplorer _parentVm;
        private readonly SqlConnection _sqlConnection;
        private bool _isBusy;
        private DataTable _data;
        private bool _isSqlEditMode;
        private string _sql;
        private Dictionary<string, string> _filterFields;
        private static object _syncLock = new object();
        private readonly string _sqlBase;
        public a7SingleTableExplorer(a7TableExplorer parentVm, string name, SqlConnection sqlConnection)
        {
            IsBusy = false;
            _parentVm = parentVm;
            _sqlConnection = sqlConnection;
            this.TableName = name;
            this._sqlBase = "Select top 100 * from " + TableName;
            this.SQL = _sqlBase;
            this._combinedSql = this.SQL;
            this._columnWhere = "";
            SetAvailableColumns();
        }

        public void SetAvailableColumns()
        {
            var tmpData = new DataTable();
            Dispatcher.CurrentDispatcher.Invoke(() => IsBusy = true);
            var columnsSelect = new SqlCommand($"SELECT * FROM {TableName} WHERE 1=2", _sqlConnection);
            DataAdapter = new SqlDataAdapter(columnsSelect);
            var sqlBuilder = new SqlCommandBuilder(DataAdapter);
            DataAdapter.Fill(tmpData);
            this.AvailableColumns = new ObservableCollection<ColumnDefinition>(
                tmpData.Columns.Cast<DataColumn>().Select(col => 
                            new ColumnDefinition
                            {
                                Name = col.ColumnName,
                                Type = col.DataType == typeof(int)
                                        ? PropertyType.Integer 
                                        : col.DataType == typeof(float)
                                        ? PropertyType.Float  
                                        : col.DataType == typeof(bool)
                                        ? PropertyType.Bool 
                                        : col.DataType == typeof(DateTime)
                                        ? PropertyType.DateTime 
                                        : PropertyType.String
                            })
                );
   
            Dispatcher.CurrentDispatcher.Invoke(() => IsBusy = false);
        }

        public void Refresh()
        {
            Data = new DataTable();
            var tmpData = new DataTable();
            Dispatcher.CurrentDispatcher.Invoke(() => IsBusy = true);
            if(!IsSqlEditMode)
                Filter2Sql();

            var columnsSelect = new SqlCommand(this.SQL, _sqlConnection);

            DataAdapter = new SqlDataAdapter(columnsSelect);
            var sqlBuilder = new SqlCommandBuilder(DataAdapter);
            try
            {
                DataAdapter.UpdateCommand = sqlBuilder.GetUpdateCommand();
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message);
            }
            try
            {
                DataAdapter.InsertCommand = sqlBuilder.GetInsertCommand();
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message);
            }
            try
            {
                DataAdapter.DeleteCommand = sqlBuilder.GetDeleteCommand();
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message);
            }
            Task.Delay(250).ContinueWith((t) =>
            {
                DataAdapter.Fill(tmpData);
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => {
                    Data = tmpData;
                    IsBusy = false;
                }));
               
            });
            
        }


        public void ClearFieldFilters()
        {
            FilterFields.Clear();
            PleaseClearColumnFilters?.Invoke(this, null);
        }

        public void Filter2Sql()
        {

            if (FilterFields != null)
            {
                var where = "";
                var isFirst = true;
                foreach (var kv in FilterFields)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        where += " AND ";
                    where += kv.Key + " LIKE ('%" + kv.Value + "%') ";
                }

                if (string.IsNullOrWhiteSpace(where))
                {
                    this._combinedSql = this._sqlBase;
                }
                else
                {
                    this._combinedSql = this._sqlBase + " WHERE " + where;
                }
                //else if (this.SQL.IndexOf("WHERE", StringComparison.CurrentCultureIgnoreCase) == -1)
                //{
                //    this._combinedSql = this.SQL.Replace(this.TableName, this.TableName + " WHERE " + where);
                //}
                //else
                //{
                //    var pos1 = this.SQL.IndexOf("WHERE", StringComparison.CurrentCultureIgnoreCase);
                //    var pos2 = pos1 + "WHERE".Length;
                //    var beforeWhere = this.SQL.Substring(0, pos2 + 1);
                //    var afterWhere = this.SQL.Substring(pos2 + 1);
                //    this._combinedSql = beforeWhere + " ( " + where + " ) AND ( " + afterWhere + " ) ";
                //}
            }
            else
            {
                _combinedSql = this.SQL;
            }
            this.SQL = _combinedSql;
            OnPropertyChanged("SQL");
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion


        public void CommitChanges()
        {
            try
            {
                var ret = DataAdapter.Update(Data);
            //    DataAdapter.Fill(Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
