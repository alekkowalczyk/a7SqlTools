using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableOnlyInRemove : a7IDbDifference
    {
        public string TableName { get; private set; }
        public string ButtonCaption { get; private set; }
        public string OnlyInDbName { get; private set; }
        public ICommand CopyTable { get; private set; }
        public Database OnlyInDb { get; private set; }

        private a7DbTableOnlyIn onlyInItem;

        public a7DbTableOnlyInRemove(string tableName, a7DbComparedDataBases onlyIn, a7DbStructureComparer comparer)
        {
            TableName = tableName;
            OnlyInDbName = comparer.GetDbName(onlyIn);
            OnlyInDb = comparer.GetDb(onlyIn);
            ButtonCaption = "Remove from '" + comparer.GetDb(onlyIn).Name + "'";
            CopyTable = new a7LambdaCommand((o) =>
                {
                    var tbl = comparer.GetTable(TableName, onlyIn);
                    tbl.Drop();
                    if (onlyIn == a7DbComparedDataBases.A)
                    {
                        comparer.TablesOnlyInB.Remove(this);
                        if (onlyInItem != null)
                            comparer.TablesOnlyInA.Remove(onlyInItem);
                    }
                    else
                    {
                        comparer.TablesOnlyInA.Remove(this);
                        if (onlyInItem != null)
                            comparer.TablesOnlyInB.Remove(onlyInItem);
                    }
                }
                );
        }

        public void SetOnlyInItem(a7DbTableOnlyIn item)
        {
            onlyInItem = item;
        }


        public string Text
        {
            get { return TableName; }
        }

        public ICommand ButtonClick
        {
            get { return CopyTable; }
        }

        public ICommand Button2Click
        {
            get { return null; }
        }

        public System.Windows.Visibility Button2Visibility
        {
            get { return System.Windows.Visibility.Collapsed; }
        }

        public string Button2Caption
        {
            get { return ""; }
        }
    }
}
