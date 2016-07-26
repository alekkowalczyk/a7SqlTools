using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableFieldCopyTo : a7IDbDifference
    {
        public string ButtonCaption { get; private set; }

        public string Text { get; private set; }

        public System.Windows.Input.ICommand ButtonClick { get; private set; }
        public ICommand Button2Click { get; private set; }
        public Visibility Button2Visibility { get; private set; }
        public string Button2Caption { get; private set; }

        private a7DbTableFieldRemove tableRemoveItem;

        public a7DbTableFieldCopyTo(string dbName, Column column, int colPos, Table tableExists, Table tableNotExists, Database isInDb, Database isNotIntDb, a7DbStructureComparer comparer, a7DbTableFieldDifferences tableFieldDifferencesExists, a7DbTableFieldDifferences tableFieldDifferencesNotExists)
        {
            Text = column.Name;
            Button2Visibility = Visibility.Collapsed;
            ButtonCaption = string.Format("Copy field to '{0}'", isNotIntDb.Name);
            ButtonClick = new a7LambdaCommand((s) =>
                {
                    a7DbTableUtils.CopyColumn(column, tableExists, tableNotExists, new Server(dbName), colPos);
                    tableNotExists.Alter();
                    tableFieldDifferencesExists.TableFieldDifferent.Remove(this);
                    tableFieldDifferencesNotExists.TableFieldDifferent.Remove(tableRemoveItem);
                }
            );
        }

        public void SetTableFieldRemoveItem(a7DbTableFieldRemove remove)
        {
            tableRemoveItem = remove;
        }
    }
}
