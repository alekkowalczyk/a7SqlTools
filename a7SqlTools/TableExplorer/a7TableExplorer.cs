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
                _selectedTableName = value;
                OnPropertyChanged();
            }
        }

        public string DbName { get; private set; }

        public a7TableExplorer(string dbName, ConnectionData connectionData)
        {
            DbName = dbName;
            _connectionString = ConnectionStringGenerator.Get(connectionData, dbName);
            RefreshDictTables("");
        }

        public void RefreshDictTables(string filter)
        {
            var tableNames = new List<string>();
            DataTable columnNamesWithMacro = new DataTable();
            //SqlCommand columnsSelect = new SqlCommand("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('tbC%')", ExportConfiguration.SqlConnection);
            string sqlColumns = "select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('%" + filter + "%')";
            SqlCommand columnsSelect = new SqlCommand(sqlColumns, SqlConnection);
            SqlDataAdapter columnsAdapter = new SqlDataAdapter(columnsSelect);
            columnsAdapter.Fill(columnNamesWithMacro);
            
            string lastTableName = "";
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
