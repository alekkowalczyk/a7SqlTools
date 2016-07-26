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
    public class a7DbTableFieldRemove : a7IDbDifference
    {
        public string ButtonCaption { get; private set; }

        public string Text { get; private set; }

        public System.Windows.Input.ICommand ButtonClick { get; private set; }
        public ICommand Button2Click { get; private set; }
        public Visibility Button2Visibility { get; private set; }
        public string Button2Caption { get; private set; }

        private a7DbTableFieldCopyTo copyToItem;

        public a7DbTableFieldRemove(Column column, Table tableExists, Table tableNotExists, Database isInDb, Database isNotIntDb, a7DbStructureComparer comparer, a7DbTableFieldDifferences tableFieldDifferencesExists, a7DbTableFieldDifferences tableFieldDifferencesNotExists)
        {
            Text = column.Name;
            Button2Visibility = Visibility.Collapsed;
            ButtonCaption = string.Format("Remove field from '{0}'", isInDb.Name);
            ButtonClick = new a7LambdaCommand((o) =>
                {
                    column.Drop();
                    tableFieldDifferencesExists.TableFieldDifferent.Remove(this);
                    tableFieldDifferencesNotExists.TableFieldDifferent.Remove(copyToItem);
                }
            );
        }

        public void SetCopyToItem(a7DbTableFieldCopyTo item)
        {
            copyToItem = item;
        }
    }
}
