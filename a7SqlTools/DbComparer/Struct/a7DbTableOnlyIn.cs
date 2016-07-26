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
    public class a7DbTableOnlyIn : a7IDbDifference
    {
        public string TableName { get; private set; }
        public string ButtonCaption { get; private set; }
        public string OnlyInDbName { get; private set; }
        public ICommand CopyTable { get; private set; }
        public Database OnlyInDb { get; private set; }

        private a7DbTableOnlyInRemove removeItem;

        public a7DbTableOnlyIn(string tableName, a7DbComparedDataBases onlyIn, a7DbStructureComparer comparer)
        {
            TableName = tableName;
            OnlyInDbName = comparer.GetDbName(onlyIn);
            OnlyInDb = comparer.GetDb(onlyIn);
            ButtonCaption = "Copy to '" + comparer.GetOtherDb(onlyIn).Name + "'";
            CopyTable = new a7LambdaCommand((o) =>
                    {
                        comparer.CopyTable(TableName, onlyIn);
                        if (onlyIn == a7DbComparedDataBases.A)
                        {
                            comparer.TablesOnlyInA.Remove(this);
                            if (removeItem != null)
                                comparer.TablesOnlyInB.Remove(removeItem);
                        }
                        else
                        {
                            comparer.TablesOnlyInB.Remove(this);
                            if (removeItem != null)
                                comparer.TablesOnlyInA.Remove(removeItem);
                        }
                    }
                );
        }

        public void SetRemoveItem(a7DbTableOnlyInRemove remove)
        {
            removeItem = remove;
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
