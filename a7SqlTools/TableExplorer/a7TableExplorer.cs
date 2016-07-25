using a7SqlTools.Config.Model;
using a7SqlTools.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using a7SqlTools.Connection;

namespace a7SqlTools.TableExplorer
{
    public class a7TableExplorer : ViewModelBase
    {
        public ObservableCollection<string> TableNames { get; set; }
        private string _selectedTableName;
        private string _connectionString;
        private SqlConnection _sqlConnection;
        private SqlConnection SqlConnection
        {
            get
            {
                if (_sqlConnection == null)
                    _sqlConnection = new SqlConnection(_connectionString);
                return _sqlConnection;
            }
        }

        public string SelectedTableName
        {
            get
            {
                return _selectedTableName;
            }
            set
            {
                var oldValue = _selectedTableName;
                _selectedTableName = value;
                if (oldValue != value)
                {
                    SelectedTable = new a7SingleTableExplorer(_selectedTableName, SqlConnection);
                }
                OnPropertyChanged();
            }
        }

        private a7SingleTableExplorer _selectedTable;
        public a7SingleTableExplorer SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                _selectedTable = value;
                OnPropertyChanged();
            }
        }

        public string DbName { get; private set; }

        public ICommand Remove => new a7LambdaCommand(o =>
        {
            _parentVm.RemoveChild(this);
        });

        private readonly ConnectionViewModel _parentVm;

        public a7TableExplorer(string dbName, ConnectionData connectionData, ConnectionViewModel parentVm)
        {
            _parentVm = parentVm;
            DbName = dbName;
            _connectionString = ConnectionStringGenerator.Get(connectionData, dbName);
            RefreshDictTables("");
        }

        public void RefreshDictTables(string filter)
        {
            var tableNames = new List<string>();
            var columnNamesWithMacro = new DataTable();
            //SqlCommand columnsSelect = new SqlCommand("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('tbC%')", ExportConfiguration.SqlConnection);
            var sqlColumns = "select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('%" + filter + "%')";
            var columnsSelect = new SqlCommand(sqlColumns, SqlConnection);
            var columnsAdapter = new SqlDataAdapter(columnsSelect);
            columnsAdapter.Fill(columnNamesWithMacro);
            
            var lastTableName = "";
            foreach (DataRow dr in columnNamesWithMacro.Rows)
            {
                if (dr["TABLE_NAME"].ToString() != lastTableName)
                {
                    lastTableName = dr["TABLE_NAME"].ToString();
                    tableNames.Add(lastTableName);
                }
            }
            tableNames.Sort();
            TableNames = new ObservableCollection<string>(tableNames);
            OnPropertyChanged(nameof(TableNames));
        }
    }
}
