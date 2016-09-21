using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbStructureComparer : ViewModelBase
    {
        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set {
                _isBusy = value;
                Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(IsBusy)));
            }
        }
        public string DbAName { get { return DbA.Name; } }
        public string DbBName { get { return DbB.Name; } }

        private Server _dbServer;
        public Database DbA { get; private set; }
        public Database DbB { get; private set; }

        public bool TablesAreInSync => TablesOnlyInA.Count == 0 && TablesOnlyInB.Count == 0;
        public bool TablesOnlyInAExist => TablesOnlyInA.Count > 0;
        public bool TablesOnlyInBExist => TablesOnlyInB.Count > 0;
        public bool TableFieldsInSync => TableFieldDifferences.Count == 0;
        public ObservableCollection<a7DbTableOnlyIn> TablesOnlyInA { get; private set; }
        public ObservableCollection<a7DbTableOnlyIn> TablesOnlyInB { get; private set; }
        public List<string> TableNamesOnlyInA { get; private set; }
        public List<string> TableNamesOnlyInB { get; private set; }

        public ObservableCollection<a7DbTableFieldDifferences> TableFieldDifferences { get; private set; }


        private readonly string _dbName;
        private Action<string> _log;

        public a7DbStructureComparer(string dbName, string dbA, string dbB, Action<string> log)
        {
            _dbName = dbName;
            _log = log;
            _dbServer = new Server(dbName);
            DbA = _dbServer.Databases[dbA];
            DbB = _dbServer.Databases[dbB];
            compareTableExistence();
            compareFieldExistence();
        }

        #region get db/names based on A/B

        public Table GetTable(string tableName, a7DbComparedDataBases db)
        {
            Database database;
            if (db == a7DbComparedDataBases.A)
                database = DbA;
            else
                database = DbB;
            return database.Tables[tableName];
        }

        public string GetDbName(a7DbComparedDataBases db)
        {
            if (db == a7DbComparedDataBases.A)
                return DbAName;
            else
                return DbBName;
        }

        public Database GetDb(a7DbComparedDataBases db)
        {
            if (db == a7DbComparedDataBases.A)
                return DbA;
            else
                return DbB;
        }

        public Database GetOtherDb(a7DbComparedDataBases db)
        {
            if (db == a7DbComparedDataBases.B)
                return DbA;
            else
                return DbB;
        }

        #endregion

        public void CopyTable(string tableName, a7DbComparedDataBases fromDbSource)
        {
            var fromDb = GetDb(fromDbSource);
            var toDb = GetOtherDb(fromDbSource);

            var table = fromDb.Tables[tableName];
            a7DbTableUtils.CopyTable(table, tableName, toDb);
        }

        public void RemoveTable(string tableName, a7DbComparedDataBases fromDbSource)
        {
            var fromDb = GetDb(fromDbSource);
            var toDb = GetOtherDb(fromDbSource);

            var table = fromDb.Tables[tableName];
            throw new NotImplementedException();
        }

        private void compareTableExistence()
        {
            _log("Checking table existence");
            TablesOnlyInA = new ObservableCollection<a7DbTableOnlyIn>();
            TablesOnlyInB = new ObservableCollection<a7DbTableOnlyIn>();
            TableNamesOnlyInA = new List<string>();
            TableNamesOnlyInB = new List<string>();
            foreach (var tb in DbA.Tables)
            {
                
                var table = tb as Table;
                _log(string.Format("Table '{0}' in db '{1}'", table.Name, DbA.Name));
                if (!DbB.Tables.Contains(table.Name))
                {
                    var onlyIn = new a7DbTableOnlyIn(table.Name, a7DbComparedDataBases.A, this);
                    //var onlyInRemove = new a7DbTableOnlyInRemove(table.Name, a7DbComparedDataBases.A, this);
                    //onlyIn.SetRemoveItem(onlyInRemove);
                    //onlyInRemove.SetOnlyInItem(onlyIn);
                    TablesOnlyInA.Add(onlyIn);
                    //TablesOnlyInB.Add(onlyInRemove);
                    TableNamesOnlyInA.Add(table.Name);
                }
            }

            foreach (var tb in DbB.Tables)
            {
                var table = tb as Table;
                _log(string.Format("Table '{0}' in db '{1}'", table.Name, DbB.Name));
                if (!DbA.Tables.Contains(table.Name))
                {
                    var onlyIn = new a7DbTableOnlyIn(table.Name, a7DbComparedDataBases.B, this);
                    //var onlyInRemove = new a7DbTableOnlyInRemove(table.Name, a7DbComparedDataBases.B, this);
                    //onlyIn.SetRemoveItem(onlyInRemove);
                    //onlyInRemove.SetOnlyInItem(onlyIn);
                    TablesOnlyInB.Add(onlyIn);
                    //TablesOnlyInA.Add(onlyInRemove);
                    TableNamesOnlyInB.Add(table.Name);
                }
            }
            TablesOnlyInA.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(TablesOnlyInAExist));
                OnPropertyChanged(nameof(TablesAreInSync));
            };
            TablesOnlyInB.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(TablesOnlyInBExist));
                OnPropertyChanged(nameof(TablesAreInSync));
            };
        }

        private void compareFieldExistence()
        {
            TableFieldDifferences = new ObservableCollection<a7DbTableFieldDifferences>();
            _log("Checking columns of Db "+DbAName);
            var alreadyWrittenTableFieldDifferences = new Dictionary<string, a7DbTableFieldDifferences>();

            foreach (var tb in DbA.Tables)
            {
                var table = tb as Table;
                var tableOnB = DbB.Tables[table.Name];
                if (tableOnB==null)
                    continue;
                var tableFieldDiffOnlyInA = new a7DbTableFieldDifferences(table.Name, this);
                var someFieldDifferent = false;
                var pos = 0;
                foreach (var fld in table.Columns)
                {
                    var col = fld as Column;
                    _log(string.Format("Field in '{2}' in Table '{0}' in db '{1}'", table.Name, DbA.Name, col.Name));
                    var colOnB = tableOnB.Columns[col.Name];
                    if (colOnB == null)
                    {
                        someFieldDifferent = true;
                        var copyToItem =
                            new a7DbTableFieldCopyTo(_dbName, col, pos, table, tableOnB, DbA, DbB, this, tableFieldDiffOnlyInA);
                        tableFieldDiffOnlyInA.TableFieldsOnlyInA.Add(copyToItem);
                    }
                    else
                    {
                        if (col.DataType.MaximumLength != colOnB.DataType.MaximumLength ||
                            col.DataType.SqlDataType != colOnB.DataType.SqlDataType)
                        {
                            someFieldDifferent = true;
                            tableFieldDiffOnlyInA.TableFieldsDifferentType.Add(new a7DbTableFieldDifferent(col, colOnB, table, tableOnB, DbA, DbB, this, tableFieldDiffOnlyInA));
                            //a7DbTableFieldDifferences tableFieldDifferent;
                            //if (alreadyWrittenTableFieldDifferences.ContainsKey(table.Name))
                            //{
                            //    tableFieldDifferent = alreadyWrittenTableFieldDifferences[table.Name];
                            //}
                            //else
                            //{
                            //    tableFieldDifferent = new a7DbTableFieldDifferences(table.Name, this);
                            //    alreadyWrittenTableFieldDifferences[table.Name] = tableFieldDifferent;
                            //}
                            //tableFieldDifferent.TableFieldsOnlyInA.Add(new a7DbTableFieldDifferent(col, colOnB, table, tableOnB, DbA, DbB, this));
                        }
                    }
                    pos++;
                }
                if (someFieldDifferent)
                {
                    TableFieldDifferences.Add(tableFieldDiffOnlyInA);
                }
            }

            //foreach (var tableDifferent in alreadyWrittenTableFieldDifferences.Values)
            //    this.TableFieldDifferences.Add(tableDifferent);

            _log("Checking columns of Db " + DbBName);
            foreach (var tb in DbB.Tables)
            {
                var table = tb as Table;
                var tableOnA = DbA.Tables[table.Name];
                if (tableOnA == null)
                    continue;

                var tableFieldDiffOnlyInA = TableFieldDifferences.FirstOrDefault((s) => s.TableName == table.Name);
                var addA = false;
                if (tableFieldDiffOnlyInA == null)
                {
                    addA = true;
                    tableFieldDiffOnlyInA = new a7DbTableFieldDifferences(table.Name, this);
                }
                var someFieldDifferent = false;
                var pos = 0;
                foreach (var fld in table.Columns)
                {
                    var col = fld as Column;
                    _log(string.Format("Field in '{2}' in Table '{0}' in db '{1}'", table.Name, DbB.Name, col.Name));
                    var colOnA = tableOnA.Columns[col.Name];
                    if (colOnA == null)
                    {
                        someFieldDifferent = true;
                        var copyTo =
                            new a7DbTableFieldCopyTo(_dbName, col,pos, table, tableOnA, DbB, DbA, this, tableFieldDiffOnlyInA);
                        tableFieldDiffOnlyInA.TableFieldsOnlyInB.Add(copyTo);
                    }
                    pos++;
                }
                if (someFieldDifferent && addA)
                {
                    TableFieldDifferences.Add(tableFieldDiffOnlyInA);
                }
            }
        }
    }
}
