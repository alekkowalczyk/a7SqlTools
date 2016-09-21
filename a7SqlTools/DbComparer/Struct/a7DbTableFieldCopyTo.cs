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
    public class a7DbTableFieldCopyTo
    {
        public string Text { get; private set; }
        public ICommand AddToOtherCommand { get; }
        public string AddToOtherDbName { get; }
        public ICommand RemoveFromThisCommand { get; }
        public string RemoveFromThisDbName { get; }

        public a7DbTableFieldCopyTo(string dbName, Column column, int colPos, Table tableExists, Table tableNotExists, Database isInDb, Database isNotIntDb, a7DbStructureComparer comparer, a7DbTableFieldDifferences tableFieldDifferencesExists)
        {
            Text = column.Name;

            AddToOtherDbName = isNotIntDb.Name;
            AddToOtherCommand = new a7LambdaCommand((o) =>
            {
                comparer.IsBusy = true;
                Task.Factory.StartNew(() =>
                {
                    a7DbTableUtils.CopyColumn(column, tableExists, tableNotExists, new Server(dbName), colPos);
                    tableNotExists.Alter();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tableFieldDifferencesExists.TableFieldsOnlyInA.Remove(this);
                        tableFieldDifferencesExists.TableFieldsOnlyInB.Remove(this);
                        comparer.IsBusy = false;
                    });
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        throw t.Exception;
                    }
                });
            }
            );
            RemoveFromThisDbName = isInDb.Name;
            RemoveFromThisCommand = new a7LambdaCommand((o) =>
            {
                comparer.IsBusy = true;
                Task.Factory.StartNew(() =>
                {
                    column.Drop();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tableFieldDifferencesExists.TableFieldsOnlyInA.Remove(this);
                        tableFieldDifferencesExists.TableFieldsOnlyInB.Remove(this);
                        comparer.IsBusy = false;
                    });
                });
            }
            );
        }
    }
}
