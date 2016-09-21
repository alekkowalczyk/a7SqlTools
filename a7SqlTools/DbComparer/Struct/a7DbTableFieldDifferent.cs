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
    public class a7DbTableFieldDifferent
    {
        public string Text { get; private set; }
        public string TypeInA { get; }
        public string TypeInB { get; }
        public string DbAName { get; }
        public string DbBName { get; }
        public ICommand CopyTypeToA { get; private set; }
        public ICommand CopyTypeToB { get; private set; }

        public a7DbTableFieldDifferent(Column colA, Column colB, Table tableA, Table tableB, Database dbA, Database dbB, a7DbStructureComparer comparer, a7DbTableFieldDifferences differences)
        {
            DbAName = comparer.DbAName;
            DbBName = comparer.DbBName;
            TypeInA = colA.DataType.ToString();
            if (colA.DataType.SqlDataType == SqlDataType.VarChar || colA.DataType.SqlDataType == SqlDataType.NVarChar
                || colA.DataType.SqlDataType == SqlDataType.Char || colA.DataType.SqlDataType == SqlDataType.NChar)
                TypeInA += "(" + colA.DataType.MaximumLength + ")";
            TypeInB = colB.DataType.ToString();
            if (colB.DataType.SqlDataType == SqlDataType.VarChar || colB.DataType.SqlDataType == SqlDataType.NVarChar
                || colB.DataType.SqlDataType == SqlDataType.Char || colB.DataType.SqlDataType == SqlDataType.NChar)
                TypeInB += "(" + colB.DataType.MaximumLength + ")";
            Text = colA.Name;
            CopyTypeToA = new a7LambdaCommand((o) =>
                {
                    comparer.IsBusy = true;
                    Task.Factory.StartNew(() =>
                    {
                        if ((colA.DataType.SqlDataType == SqlDataType.VarChar &&
                             colB.DataType.SqlDataType == SqlDataType.VarChar)
                            || (colA.DataType.SqlDataType == SqlDataType.NVarChar &&
                                colB.DataType.SqlDataType == SqlDataType.NVarChar))
                        {
                            colA.DataType.MaximumLength = colB.DataType.MaximumLength;
                            tableA.Alter();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                differences.TableFieldsDifferentType.Remove(this);
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("Not supported for non-varchar types");
                            });
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
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
            CopyTypeToB = new a7LambdaCommand((o) =>
            {
                if ((colA.DataType.SqlDataType == SqlDataType.VarChar &&
                             colB.DataType.SqlDataType == SqlDataType.VarChar)
                            || (colA.DataType.SqlDataType == SqlDataType.NVarChar &&
                                colB.DataType.SqlDataType == SqlDataType.NVarChar))
                {
                    colB.DataType.MaximumLength = colA.DataType.MaximumLength;
                    tableB.Alter();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        differences.TableFieldsDifferentType.Remove(this);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Not supported for non-varchar types");
                    });
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    comparer.IsBusy = false;
                });
            }
            );
        }
    }
}
