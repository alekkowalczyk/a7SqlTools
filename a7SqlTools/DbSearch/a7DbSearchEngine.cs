using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.ComponentModel;
using a7SqlTools.Utils;
using a7SqlTools.Config.Model;

namespace a7SqlTools.DbSearch
{
    public class a7DbSearchEngine : ViewModelBase
    {
        Dictionary<string, List<a7TableColumn>> dictTable_ColumnNames;

        private Dictionary<string, a7TableSelection> _dictTables;
        public Dictionary<string, a7TableSelection> DictTables {
            get {
                if (_dictTables == null)
                    RefreshDictTables("");
                return _dictTables;
            }
            set { _dictTables = value;  OnPropertyChanged(); }
        }
        
        //=============
        //search props:
        public string Seperator { get; set;}
        public string AndSeperator { get; set; }
        public Dictionary<string, a7SearchedValue> SearchedValues { get; set; }
        public a7SearchedValue SelectedSearchedValue { get; set; }
        public DataTable SelectedTable { get; set; }
        public string NotFoundItems { get; set; }
        //=============
        public event EventHandler<DBSearchEventArgs> ActualizedWork;
        public event EventHandler<DBSearchFinishedEventArgs> FinishedSearch;
        private bool abortSearch = false;

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

        public class DBSearchEventArgs :EventArgs
        {
            public string ActualAnalizedTable { get; private set; }
            public string ActualAnalizedValue { get; private set; }
            public int ActualTable { get; private set; }
            public int TableCount { get; private set; }
            public int ActualTableValue { get; private set; }
            public int ValuesCount { get; private set; }

            public DBSearchEventArgs(string table, string value, int actualTable, int tableCount, int actualTableValue, int tableValueCount)
            {
                ActualAnalizedTable = table;
                ActualAnalizedValue = value;
                ActualTable = actualTable;
                TableCount = tableCount;
                ActualTableValue = actualTableValue;
                ValuesCount = tableValueCount;
            }
        }

        public class DBSearchFinishedEventArgs : EventArgs
        {
            public string NotUsedValuesList { get; private set; }

            public DBSearchFinishedEventArgs(string notUsedValues)
            {
                NotUsedValuesList = notUsedValues;
            }
        }

        public class a7TableSelection : IComparer<string>
        {
            public string TableName { get; set; }
            public bool IsSelected { get; set; }

            #region IComparer<string> Members

            public int Compare(string x, string y)
            {
                return x.CompareTo(y);
            }

            #endregion
        }

        class a7TableColumn
        {
            public string TableName;
            public string ColumnName;
            public bool IsStringColumn;
            public a7TableColumn(object tableName, object columnName, object dataType)
            {
                TableName = tableName.ToString();
                ColumnName = columnName.ToString();
                IsStringColumn = dataType.ToString().IndexOf("char") != -1 || dataType.ToString().IndexOf("text") != -1;
            }
        }

        public string Name { get; set; }

        public a7DbSearchEngine(string name, ConnectionData connectionData)
        {
            this.Name = name;
            _connectionString = ConnectionStringGenerator.Get(connectionData, name);
            Seperator = ",";
        }
        

        public void RefreshDictTables(string filter)
        {
            SearchedValues = new Dictionary<string, a7SearchedValue>();
            DataTable columnNamesWithMacro = new DataTable();
            //SqlCommand columnsSelect = new SqlCommand("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('tbC%')", ExportConfiguration.SqlConnection);
            string sqlColumns = "select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE ('%" + filter + "%')";
            SqlCommand columnsSelect = new SqlCommand(sqlColumns, SqlConnection);
            SqlDataAdapter columnsAdapter = new SqlDataAdapter(columnsSelect);
            columnsAdapter.Fill(columnNamesWithMacro);

            dictTable_ColumnNames = new Dictionary<string, List<a7TableColumn>>();
            _dictTables = new Dictionary<string, a7TableSelection>();
            string lastTableName = "";
            List<string> tableNames = new List<string>(); ;
            foreach (DataRow dr in columnNamesWithMacro.Rows)
            {
                if (dr["TABLE_NAME"].ToString() != lastTableName)
                {
                    lastTableName = dr["TABLE_NAME"].ToString();
                    dictTable_ColumnNames[lastTableName] = new List<a7TableColumn>();
                    tableNames.Add(lastTableName);
                }
                dictTable_ColumnNames[lastTableName].Add(new a7TableColumn(dr["TABLE_NAME"], dr["COLUMN_NAME"], dr["DATA_TYPE"]));
            }
            tableNames.Sort();
            foreach (string tn in tableNames)
            {
                DictTables.Add(tn, new a7TableSelection() { TableName = tn, IsSelected = true });
            }
            OnPropertyChanged(nameof(DictTables));
        }

        public int ExecuteSQL(string sql)
        {
            SqlCommand sqlComm = new SqlCommand(sql, SqlConnection);
            return sqlComm.ExecuteNonQuery();
        }

        public string GetValue(string sql)
        {
            SqlCommand sqlComm = new SqlCommand(sql, SqlConnection);
            return sqlComm.ExecuteScalar().ToString();
        }

        public void SelectAllTables(bool select)
        {
            foreach (KeyValuePair<string, a7DbSearchEngine.a7TableSelection> kv in DictTables)
            {
                kv.Value.IsSelected = select;
            }
            OnPropertyChanged("DictTables");
        }

        public void BeginSearchValues(string values)
        {
            abortSearch = false;
            ParameterizedThreadStart pt = new ParameterizedThreadStart(SearchValues);
            Thread t = new Thread(pt);
            t.Start(values);
        }

        public void AbortSearch()
        {
            abortSearch = true;
        }

        private void SearchValues(object values)
        {
            string sRet = "";
            values = values.ToString().Replace("'", "");
            
            string[] asValues;
            if(string.IsNullOrEmpty(Seperator))
                asValues = new string[]{ values.ToString() };
            else
            {
                string seperator;
                if (Seperator.ToLower() == "[enter]")
                    seperator = "\r\n";
                else 
                    seperator = Seperator;
                asValues = values.ToString().Split(new string[] { seperator } , StringSplitOptions.RemoveEmptyEntries);
            }
            
            Dictionary<string, int> dictValueFoundCount = new Dictionary<string, int>();
            SearchedValues = new Dictionary<string,a7SearchedValue>();
            int tablesAnalyzed = 0;
            int selectedTablesCount = 0;
            foreach (KeyValuePair<string, a7TableSelection> kv in DictTables)
            {
                if (kv.Value.IsSelected)
                    selectedTablesCount++;
            }
            foreach (KeyValuePair<string, List<a7TableColumn>> kv in dictTable_ColumnNames)
            {
                if (!DictTables[kv.Key].IsSelected)
                    continue;
                int valuesAnalyzed = 0;
                foreach (string value in asValues)
                {
                    a7SearchedValue searchedValue;
                    if (SearchedValues.ContainsKey(value))
                        searchedValue = SearchedValues[value];
                    else
                    {
                        searchedValue = new a7SearchedValue(value);
                        SearchedValues[value] = searchedValue;
                    }
                    string sql = "SELECT * FROM " + kv.Key + " WHERE ";
                    bool stringColumnFound = false;
                    if (AndSeperator == "")
                    {
                        bool firstWhere = true;
                        foreach (a7TableColumn tc in kv.Value)
                        {
                            if (tc.IsStringColumn)
                            {
                                stringColumnFound = true;
                                if (firstWhere)
                                    firstWhere = false;
                                else
                                    sql += " OR ";
                                sql += tc.ColumnName + " LIKE ('%" + value.Trim() + "%')";
                            }
                        }
                    }
                    else
                    {
                        string[] andValues = value.Split(new string[] { AndSeperator }, StringSplitOptions.RemoveEmptyEntries);
                        bool firstAnd = true;
                        foreach (string singleAndValue in andValues)
                        {
                            if (firstAnd)
                                firstAnd = false;
                            else
                                sql += " AND ";
                            sql += " ( ";
                            bool firstWhere = true;
                            foreach (a7TableColumn tc in kv.Value)
                            {
                                if (tc.IsStringColumn)
                                {
                                    stringColumnFound = true;
                                    if (firstWhere)
                                        firstWhere = false;
                                    else
                                        sql += " OR ";
                                    sql += "["+tc.ColumnName+"]" + " LIKE ('%" + singleAndValue.Trim() + "%')";
                                }
                            }
                            sql += " ) ";
                        }
                    }
                    if (abortSearch)
                        return;
                    if (!stringColumnFound)
                        continue;
                    SqlCommand comm = new SqlCommand(sql, SqlConnection);
                    SqlDataAdapter adapter = new SqlDataAdapter(comm);
                    try
                    { //TODO - sypie sie brzydkie to jest :)
                        SqlCommandBuilder sqlBuilder = new SqlCommandBuilder(adapter);
                        adapter.UpdateCommand = sqlBuilder.GetUpdateCommand();
                        adapter.InsertCommand = sqlBuilder.GetInsertCommand();
                        adapter.DeleteCommand = sqlBuilder.GetDeleteCommand();
                    }
                    catch
                    {
                    
                    }
                    DataTable dt = new DataTable(kv.Key);
                    adapter.Fill(dt);
                    searchedValue.AddDataTable(dt, adapter);
                    if (!dictValueFoundCount.ContainsKey(value))
                        dictValueFoundCount[value] = int.Parse(dt.Rows.Count.ToString());
                    else
                        dictValueFoundCount[value] += int.Parse(dt.Rows.Count.ToString());
                    valuesAnalyzed++;
                    OnActualizedWork(kv.Key, value, tablesAnalyzed, selectedTablesCount, valuesAnalyzed, asValues.Length);
                }
                tablesAnalyzed++;
            }
            foreach (KeyValuePair<string, int> kv in dictValueFoundCount)
            {
                if (kv.Value == 0)
                    sRet += "'" + kv.Key + "',";
            }
            if (FinishedSearch != null)
                FinishedSearch(this, new DBSearchFinishedEventArgs(sRet));
            OnPropertyChanged("SearchedValues");
        }

        public void OnActualizedWork(string table, string value, int actualTable, int tableCount, int actualTableValue, int tableValueCount)
        {
            if (this.ActualizedWork != null)
                ActualizedWork(this, new DBSearchEventArgs(table, value,  actualTable,  tableCount,  actualTableValue,  tableValueCount));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public void SelectSearchedValue(a7SearchedValue selected)
        {
            SelectedSearchedValue = selected;
            OnPropertyChanged("SelectedSearchedValue");
        }

        public void SelectTable(string name)
        {
            SelectedTable = SelectedSearchedValue.SearchResultSet.Tables[name];
            OnPropertyChanged("SelectedTable");
        }

        public void CommitSelectedTable()
        {
            if (SelectedTable!=null && SelectedSearchedValue!=null)
                SelectedSearchedValue.CommitUpdates(SelectedTable.TableName);
        }
    }
}

