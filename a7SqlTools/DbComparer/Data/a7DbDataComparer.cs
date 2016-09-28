using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using a7SqlTools.DbComparer.Data.Views;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Data
{
    public class a7DbDataComparer : ViewModelBase
    {
        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(IsBusy)));
            }
        }
        private bool _isLegendVisible = false;
        public bool IsLegendVisible
        {
            get { return _isLegendVisible; }
            set { _isLegendVisible = value; OnPropertyChanged(); }
        }
        public ObservableCollection<a7DbTableComparer> Tables { get; private set; }
        public Database DbA { get; private set; }
        public Database DbB { get; private set; }
        public string DbAName { get { return DbA.Name; } }
        public string DbBName { get { return DbB.Name; } }
        public ObservableCollection<string> TableNamesOnlyInA { get; private set; }
        public ObservableCollection<string> TableNamesOnlyInB { get; private set; }
        public ObservableCollection<string> TableNamesInBothDb { get; private set; }

        public a7DbComparerDirection MergeDirection
        {
            get { return _mergeDirection; }
            private set { _mergeDirection = value; }
        }

        private bool? _mergeWithDelete;
        /// <summary>
        /// if set, on setting merge of whole table - the records that don't exist in the source, will be deleted in the destination
        /// </summary>
        public bool? MergeWithDelete
        {
            get { return _mergeWithDelete; }
            set
            {
                if (value.HasValue)
                {
                    foreach (var tbl in this.Tables)
                    {
                        tbl.MergeWithDelete = value.Value;
                    }
                }
                _mergeWithDelete = value;
                OnPropertyChanged(nameof(MergeWithDelete));
            }
        }

        public ICommand MergeAtoB { get; private set; }
        public ICommand MergeBtoA { get; private set; }

        public ICommand ShowGeneratedQuery { get; private set; }
        public Server Srv { get; private set; }

        private Action<string> _log;
        private a7DbComparerDirection _mergeDirection;

        public a7DbDataComparer(string dbName, string dbA, string dbB, Action<string> log)
        {
            _mergeWithDelete = false;
            _log = log;
            Srv = new Server(dbName);
            DbA = Srv.Databases[dbA];
            DbB = Srv.Databases[dbB];

            MergeDirection = a7DbComparerDirection.None;
            MergeAtoB = new a7LambdaCommand(async (o) =>
            {
                if (MergeDirection != a7DbComparerDirection.AtoB)
                    await SetMergeDirection(a7DbComparerDirection.AtoB);
                else
                    await SetMergeDirection(a7DbComparerDirection.None);
            }
            );
            MergeBtoA = new a7LambdaCommand(async (o) =>
            {
                if (MergeDirection != a7DbComparerDirection.BtoA)
                    await SetMergeDirection(a7DbComparerDirection.BtoA);
                else
                    await SetMergeDirection(a7DbComparerDirection.None);
            }
            );

            ShowGeneratedQuery = new a7LambdaCommand((o) =>
                {
                    var sb = new StringBuilder();
                    foreach (var tbl in Tables)
                    {
                        if (tbl.Rows != null)
                        {
                            foreach (var rw in tbl.Rows)
                            {
                                if (rw.IsDifferent)
                                {
                                    sb.Append(rw.SQL);
                                }
                            }
                        }
                    }
                    var str = sb.ToString();
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        MessageBox.Show(
                            "Empty query, please select some at least one difference selector (like A>B or B>A) on a row, table or database level.");
                        return;
                    }
                    new a7GeneratedQueryWindow(str).Show();
                }
            );

            var tableNamesA = new List<string>();
            var tableNamesB = new List<string>();
            var tableNamesAorB = new List<string>();
            Log("Collecting table list");
            foreach (var tbA in DbA.Tables)
            {
                var table = tbA as Table;
                tableNamesA.Add(table.Name);
                if (!tableNamesAorB.Contains(table.Name))
                    tableNamesAorB.Add(table.Name);
            }
            foreach (var tbB in DbB.Tables)
            {
                var table = tbB as Table;
                tableNamesB.Add(table.Name);
                if (!tableNamesAorB.Contains(table.Name))
                    tableNamesAorB.Add(table.Name);
            }

            TableNamesOnlyInA = new ObservableCollection<string>();
            TableNamesOnlyInB = new ObservableCollection<string>();
            TableNamesInBothDb = new ObservableCollection<string>();
            Tables = new ObservableCollection<a7DbTableComparer>();
            Log("Searching tables with differences tables");
            foreach (var tblName in tableNamesAorB)
            {
                if (!tableNamesA.Contains(tblName) && tableNamesB.Contains(tblName))
                {
                    TableNamesOnlyInB.Add(tblName);
                    continue;
                }
                if (!tableNamesB.Contains(tblName) && tableNamesA.Contains(tblName))
                {
                    TableNamesOnlyInA.Add(tblName);
                    continue;
                }
                if (tableNamesA.Contains(tblName) && tableNamesB.Contains(tblName) && tblName != "adminQueriesLog" && tblName != "logExceptions"
                    && tblName != "logQueries")
                {
                    TableNamesInBothDb.Add(tblName);
                    var tblComp = new a7DbTableComparer(DbA.Tables[tblName], DbB.Tables[tblName],this);
                    if(tblComp.IsDifferentData)
                        Tables.Add(tblComp);
                }
            }
        }

        public void Log(string s)
        {
            if (_log != null)
                _log(s);
        }

        public async Task SetMergeDirection(a7DbComparerDirection direction)
        {
            if (this.Tables == null)
                return;

            MergeDirection = direction;
            if (direction != a7DbComparerDirection.Partial)
            {
                await Task.WhenAll(Tables.Where(t => t.IsOK)
                    .Select(t => t.SetMergeDirection(direction, true)));
            }
            OnPropertyChanged(nameof(MergeDirection));
        }

        public void RefreshMergeDirection()
        {
            var aToBExists = false;
            var bToAExists = false;
            var noneExists = false;
            var partialExists = false;
            var allAreWithDelete = true;
            var allAreWithoutDelete = true;
            foreach (var table in Tables.Where(t => t.IsOK))
            {
                if (table.MergeWithDelete)
                    allAreWithoutDelete = false;
                else
                    allAreWithDelete = false;
                switch (table.MergeDirection)
                {
                    case a7DbComparerDirection.AtoB:
                        aToBExists = true;
                        break;
                    case a7DbComparerDirection.BtoA:
                        bToAExists = true;
                        break;
                    case a7DbComparerDirection.None:
                        noneExists = true;
                        break;
                    case a7DbComparerDirection.Partial:
                        partialExists = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if(!noneExists && !partialExists && !bToAExists 
                && aToBExists)
                MergeDirection = a7DbComparerDirection.AtoB;
            else if (!noneExists && !partialExists && bToAExists
                && !aToBExists)
                MergeDirection = a7DbComparerDirection.BtoA;
            else if (noneExists && !partialExists && !bToAExists
                && !aToBExists)
                MergeDirection = a7DbComparerDirection.None;
            else
                MergeDirection = a7DbComparerDirection.Partial;
            if (allAreWithDelete)
                _mergeWithDelete = true;
            else if (allAreWithoutDelete)
                _mergeWithDelete = false;
            else
                _mergeWithDelete = null;
            OnPropertyChanged(nameof(MergeDirection));
            OnPropertyChanged(nameof(MergeWithDelete));
        }
    }
}
