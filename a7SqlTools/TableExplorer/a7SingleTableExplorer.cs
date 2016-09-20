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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.TableExplorer.Filter;
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

        public FilterExpressionData AdvFilter
        {
            get { return _advFilter; }
            set
            {
                _advFilter = value;
                if (value != null)
                {
                    IsSqlEditMode = false;
                    ClearFieldFilters();
                }
                OnPropertyChanged(nameof(AdvFilter));
            }
        }

        private SqlDataAdapter DataAdapter;
        private string _sql;

        public string DisplayedSql
        {
            get { return _displayedSql; }
            set { _displayedSql = value; OnPropertyChanged(nameof(DisplayedSql)); }
        }

        private string _columnWhere;
        public event EventHandler PleaseClearColumnFilters;
        public Dictionary<string, string> FilterFields
        {
            get { return _filterFields; }
            set
            {
                IsSqlEditMode = false;
                AdvFilter = null;
                _filterFields = value;
                WhereClauseBuilder dummy;
                Filter2Sql(out dummy);
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

        public Brush EditorBackground { get; set; } = new SolidColorBrush(Colors.White);
        public bool IsSqlEditMode
        {
            get { return _isSqlEditMode; }
            set
            {
                _isSqlEditMode = value; OnPropertyChanged(nameof(IsSqlEditMode));
                if (value)
                {
                    ClearFieldFilters();
                    AdvFilter = null;
                    EditorBackground = new SolidColorBrush(Color.FromRgb(255, 255, 225));
                }
                else
                {
                    EditorBackground = new SolidColorBrush(Color.FromRgb(239, 239, 239));
                }
                OnPropertyChanged(nameof(EditorBackground));
            }
        }

        public ICommand Remove => new a7LambdaCommand((o) => _parentVm.OpenedTables.Remove(this));

        private readonly PoorMansTSqlFormatterLib.Interfaces.ISqlTokenizer _tokenizer;
        private readonly PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter _formatter;
        private readonly PoorMansTSqlFormatterLib.Interfaces.ISqlTokenParser _parser;
        private readonly a7TableExplorer _parentVm;
        private readonly SqlConnection _sqlConnection;
        private bool _isBusy;
        private DataTable _data;
        private bool _isSqlEditMode;
        private Dictionary<string, string> _filterFields;
        private static object _syncLock = new object();
        private readonly string _sqlBase;
        private FilterExpressionData _advFilter;
        private string _displayedSql;

        public a7SingleTableExplorer(a7TableExplorer parentVm, string name, SqlConnection sqlConnection)
        {
            IsBusy = false;
            _parentVm = parentVm;
            _sqlConnection = sqlConnection;
            this.TableName = name;
            this._sqlBase = "Select top 100 * from " + TableName;
            this._sql = _sqlBase;
            this.DisplayedSql = _sql;
            this._columnWhere = "";
            SetAvailableColumns();

            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();
            _formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(
                indentString: "\t",
                spacesPerTab: 4,
                maxLineWidth: 999,
                expandCommaLists: true,
                trailingCommas: false,
                spaceAfterExpandedComma: false,
                expandBooleanExpressions: true,
                expandCaseStatements: true,
                expandBetweenConditions: true,
                breakJoinOnSections: false,
                uppercaseKeywords: true,
                htmlColoring: false,
                keywordStandardization: false);
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
                                Type = col.DataType == typeof(int) || col.DataType == typeof(long)
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

        public void Refresh(bool withoutData = false)
        {           
            WhereClauseBuilder whereClauseBuilder = null;
            if (!IsSqlEditMode)
                Filter2Sql(out whereClauseBuilder);
            else
                this._sql = this.DisplayedSql;

            if (withoutData)
                return;

            Dispatcher.CurrentDispatcher.Invoke(() => IsBusy = true);

            Data = new DataTable();
            var tmpData = new DataTable();

            var columnsSelect = new SqlCommand(this._sql, _sqlConnection);

            if (AdvFilter != null && whereClauseBuilder != null)
            {
                foreach (var kv in whereClauseBuilder.Parameters)
                {
                    var sqlParam = new SqlParameter
                    {
                        ParameterName = kv.Key,
                        Value = kv.Value
                    };
                    columnsSelect.Parameters.Add(sqlParam);
                }
            }

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
            FilterFields?.Clear();
            PleaseClearColumnFilters?.Invoke(this, null);
        }

        public void Filter2Sql(out WhereClauseBuilder whereBuilder)
        {
            whereBuilder = null;
            if (AdvFilter != null)
            {
                whereBuilder = new WhereClauseBuilder(AdvFilter, this);
                this._sql = this._sqlBase + whereBuilder.WhereClause;
                var generatedSql = whereBuilder.Parameters.Aggregate(_sql, (current, param) =>
                {
                    var strValue = param.Value?.ToString();
                    if (param.Value is string || param.Value is DateTime)
                    {
                        strValue = $"'{strValue}'";
                    }
                    return current.Replace($"@{param.Key}", strValue);
                });
                //var tokenized = _tokenizer.TokenizeSQL(generatedSql);
                //var parsed = _parser.ParseSQL(tokenized);
                //DisplayedSql = _formatter.FormatSQLTree(parsed);
                DisplayedSql = generatedSql;
            }
            else if (FilterFields != null)
            {
                var where = "";
                var isFirst = true;
                foreach (var kv in FilterFields)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        where += " AND ";
                    var val = kv.Value.Replace("'", "''");
                    where += kv.Key + " LIKE ('%" + val + "%') ";
                }

                if (string.IsNullOrWhiteSpace(where))
                {
                    this._sql = this._sqlBase;
                }
                else
                {
                    this._sql = this._sqlBase + " WHERE " + where;
                }
                //var tokenized = _tokenizer.TokenizeSQL(_sql);
                //var parsed = _parser.ParseSQL(tokenized);
                //DisplayedSql = _formatter.FormatSQLTree(parsed);
                DisplayedSql = _sql;
            }
        }

        public void FormatSql()
        {
            var tokenized = _tokenizer.TokenizeSQL(DisplayedSql);
            var parsed = _parser.ParseSQL(tokenized);
            DisplayedSql = _formatter.FormatSQLTree(parsed);
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
