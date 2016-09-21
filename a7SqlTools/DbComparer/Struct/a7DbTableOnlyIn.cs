using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableOnlyIn
    {
        public string TableName { get; private set; }
        public ICommand AddToOtherCommand { get; }
        public string AddToOtherDbName { get; }
        public ICommand RemoveFromThisCommand { get; }
        public string RemoveFromThisDbName { get; }
   

        public a7DbTableOnlyIn(string tableName, a7DbComparedDataBases onlyIn, a7DbStructureComparer comparer)
        {
            TableName = tableName;
            AddToOtherDbName = comparer.GetOtherDb(onlyIn).Name;
            AddToOtherCommand = new a7LambdaCommand((o) =>
                {
                    comparer.IsBusy = true;
                    Task.Factory.StartNew(() =>
                    {
                        comparer.CopyTable(TableName, onlyIn);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (onlyIn == a7DbComparedDataBases.A)
                            {
                                comparer.TablesOnlyInA.Remove(this);
                            }
                            else
                            {
                                comparer.TablesOnlyInB.Remove(this);
                            }
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
            RemoveFromThisDbName = comparer.GetDb(onlyIn).Name;
            RemoveFromThisCommand = new a7LambdaCommand((o) =>
                {
                    comparer.IsBusy = true;
                    Task.Factory.StartNew(() =>
                    {
                        var tbl = comparer.GetTable(TableName, onlyIn);
                        tbl.Drop();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (onlyIn == a7DbComparedDataBases.A)
                            {
                                comparer.TablesOnlyInA.Remove(this);
                            }
                            else
                            {
                                comparer.TablesOnlyInB.Remove(this);
                            }
                            comparer.IsBusy = false;
                        });
                    });
                }
            );
        }
    }
}
