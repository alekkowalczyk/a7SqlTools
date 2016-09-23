using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Data
{
    public class a7ComparisonRow : INotifyPropertyChanged
    {
        public Dictionary<string, a7ComparisonField> Fields { get; private set; }
        public a7ComparisonField this[string key]
        {
            get
            {
                if (Fields.ContainsKey(key))
                    return Fields[key];
                return null;
            }
        }
        public bool IsDifferent { get; private set; }
        public bool IsOnlyInA { get; private set; }
        public bool IsOnlyInB { get; private set; }
        public bool IsInBothDB { get { return !IsOnlyInA && !IsOnlyInB; } }
        public string TableName { get { return _tblComparer.TableName; } }
        public string DbAName { get { return _tblComparer.DbAName; } }
        public string DbBName { get { return _tblComparer.DbBName; } }
        public string AtoBCaption { get; private set; }
        public string BtoACaption { get; private set; }

        public string SQL { get; private set; }
        public a7DbComparerDirection MergeDirection { get; private set; }

        public ICommand MergeAtoB { get; private set; }
        public ICommand MergeBtoA { get; private set; }

        private DataRow _dataRowA;
        private DataRow _dataRowB;

        private a7DbDataComparer _comparer;
        private a7DbTableComparer _tblComparer;

        public a7ComparisonRow(DataRow rowA, DataRow rowB, a7DbTableComparer tableComparer, a7DbDataComparer comparer)
        {
            MergeDirection = a7DbComparerDirection.None;
            MergeAtoB = new a7LambdaCommand((o) =>
                {
                    if (MergeDirection != a7DbComparerDirection.AtoB)
                        SetMergeDirection(a7DbComparerDirection.AtoB, false);
                    else
                        SetMergeDirection(a7DbComparerDirection.None, false);
                }
            );
            MergeBtoA = new a7LambdaCommand((o) =>
                {
                    if (MergeDirection != a7DbComparerDirection.BtoA)
                        SetMergeDirection(a7DbComparerDirection.BtoA, false);
                    else
                        SetMergeDirection(a7DbComparerDirection.None, false);
                }
            );

            _dataRowA = rowA;
            _dataRowB = rowB;
            _comparer = comparer;
            _tblComparer = tableComparer;
            if (_dataRowA == null && _dataRowB != null)
            {
                IsOnlyInB = true;
                IsOnlyInA = false;
                AtoBCaption = "B>X";
                BtoACaption = "B>A";
            }
            else if (_dataRowB == null && _dataRowA != null)
            {
                IsOnlyInB = false;
                IsOnlyInA = true;
                AtoBCaption = "A>B";
                BtoACaption = "A>X";
            }
            else
            {
                IsOnlyInA = false;
                IsOnlyInB = false;
                AtoBCaption = "A>B";
                BtoACaption = "B>A";
            }
            Fields = new Dictionary<string, a7ComparisonField>();



            IsDifferent = false;

            for (var i = 0; i < tableComparer.ColumnsInBothTables.Count; i++)
            {
                var colName = tableComparer.ColumnsInBothTables[i];
                object valA = null;
                if (rowA != null)
                    valA = rowA[colName];
                object valB = null;
                if (rowB != null)
                    valB = rowB[colName];
                Fields.Add(colName, new a7ComparisonField(valA, valB, rowA != null, rowB != null, colName, this._comparer));
                if (valA?.ToString() != valB?.ToString() &&
                    colName != "dbVersion" && colName != "dbTimestamp" && colName != "dbUser")
                    IsDifferent = true;
            }

            for (var i = 0; i < tableComparer.ColumnsOnlyInA.Count; i++)
            {
                var colName = tableComparer.ColumnsOnlyInA[i];
                object valA = null;
                if(rowA!=null)
                    valA = rowA[colName];
                Fields.Add(colName, new a7ComparisonField(valA, null, true, false, colName, this._comparer));
                IsDifferent = true;
            }

            for (var i = 0; i < tableComparer.ColumnsOnlyInB.Count; i++)
            {
                var colName = tableComparer.ColumnsOnlyInB[i];
                object valB = null;
                if(rowB!=null)
                    valB = rowB[colName];
                Fields.Add(colName, new a7ComparisonField(null, valB, false, true, colName, this._comparer));
                IsDifferent = true;
            }
            
        }

        public void SetMergeDirection(a7DbComparerDirection direction, bool fromTableComparer)
        {
            if (!fromTableComparer)
                _tblComparer.SetMergeDirection(a7DbComparerDirection.Partial);
            if (!IsDifferent)
            {
                MergeDirection = a7DbComparerDirection.None;
                SQL = "";
                OnPropChanged("MergeDirection");
                return;
            }

            MergeDirection = direction;
            if (direction == a7DbComparerDirection.None)
            {
                SQL = "";
            }
            else if (direction == a7DbComparerDirection.AtoB)
            {
                SQL = getMergeQuery(_tblComparer.DbAName, _dataRowA, _tblComparer.DbBName, _dataRowB, _tblComparer.PrimaryKeyColumnsB, _tblComparer.ColumnsB, _tblComparer.ColumnsA);
            }
            else if (direction == a7DbComparerDirection.BtoA)
            {
                SQL = getMergeQuery(_tblComparer.DbBName, _dataRowB, _tblComparer.DbAName, _dataRowA, _tblComparer.PrimaryKeyColumnsA, _tblComparer.ColumnsA, _tblComparer.ColumnsB);
            }
            OnPropChanged("MergeDirection");
        }

        private string getMergeQuery(string fromDbName, DataRow fromRow, string toDbName, DataRow toRow, List<string> toPrimaryKeys, List<string> toColumns, List<string> fromColumns)
        {
            if (fromRow == null)
            {
                //delete on toDbName
                var sb = new StringBuilder();
                sb.Append(string.Format("DELETE FROM {0}.dbo.{1}", toDbName, _tblComparer.TableName));
                sb.Append(" WHERE ");
                var isFirst = true;
                foreach (var pk in toPrimaryKeys)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(" AND ");
                    sb.Append(string.Format("[{0}]='{1}' ", pk, toRow[pk]?.ToString().Replace("'","''")));
                }
                sb.Append(";");
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
            else if (toRow == null)
            {
                //insert to on toDbName
                var sb = new StringBuilder();
                sb.Append(string.Format("INSERT INTO {0}.dbo.{1} ", toDbName, _tblComparer.TableName));
                sb.Append("(");
                var isFirst = true;
                foreach (var col in toColumns)
                {
                    if (col != "dbVersion" && col != "dbTimestamp" && col != "dbUser")
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            sb.Append(",");
                        sb.Append(string.Format(" [{0}] ", col));
                    }
                }
                sb.Append(")");
                sb.Append(" VALUES ");
                sb.Append("(");
                isFirst = true;
                foreach (var col in toColumns)
                {
                    if (col != "dbVersion" && col != "dbTimestamp" && col != "dbUser" && fromColumns.Contains(col))
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            sb.Append(",");
                        sb.Append(string.Format(" '{0}' ", fromRow[col]?.ToString().Replace("'", "''")));
                    }
                }
                sb.Append(")");
                sb.Append(";");
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
            else
            {
                //update values on toDbName from the fromDbName
                var sb = new StringBuilder();
                sb.Append(string.Format("UPDATE {0}.dbo.{1} SET ", toDbName, _tblComparer.TableName));
                
                //fieldName = [fieldValue from fromDbName]...
                var isFirst = true;
                foreach(var col in toColumns)
                {
                    if (col != "dbVersion" && col != "dbTimestamp" && col != "dbUser" && fromColumns.Contains(col))
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            sb.Append(",");
                        sb.Append(string.Format(" [{0}]='{1}' ", col, fromRow[col]?.ToString().Replace("'", "''")));
                    }
                }

                sb.Append(" WHERE ");
                isFirst = true;
                foreach (var pk in toPrimaryKeys)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(" AND ");
                    sb.Append(string.Format("[{0}]='{1}' ", pk, toRow[pk]?.ToString().Replace("'", "''")));
                }
                sb.Append(";");
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
            return "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
