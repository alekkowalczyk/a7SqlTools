using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableFieldDifferences
    {
        public string Header { get; private set; }
        public string TableName { get; private set; }
        public a7DbStructureComparer Comparer { get; private set; }
        public ObservableCollection<a7IDbDifference> TableFieldDifferent { get; private set; }

        public a7DbTableFieldDifferences(string tableName, a7DbStructureComparer comparer)
        {
            TableFieldDifferent = new ObservableCollection<a7IDbDifference>();
            Header = string.Format("Table '{0}':", tableName);
            TableName = tableName;
            Comparer = comparer;
        }
    }
}
