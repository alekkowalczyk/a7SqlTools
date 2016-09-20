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
    public class a7DbTableFieldDifferent : a7IDbDifference
    {
        public string ButtonCaption { get; private set; }

        public string Text { get; private set; }

        public System.Windows.Input.ICommand ButtonClick { get; private set; }
        public ICommand Button2Click { get; private set; }
        public Visibility Button2Visibility { get; private set; }
        public string Button2Caption { get; private set; }

        public a7DbTableFieldDifferent(Column colA, Column colB, Table tableA, Table tableB, Database dbA, Database dbB, a7DbStructureComparer comparer)
        {
            var aType = colA.DataType.ToString();
            if (colA.DataType.SqlDataType == SqlDataType.VarChar || colA.DataType.SqlDataType == SqlDataType.NVarChar
                || colA.DataType.SqlDataType == SqlDataType.Char || colA.DataType.SqlDataType == SqlDataType.NChar)
                aType += "(" + colA.DataType.MaximumLength + ")";
            var bType = colB.DataType.ToString();
            if (colB.DataType.SqlDataType == SqlDataType.VarChar || colB.DataType.SqlDataType == SqlDataType.NVarChar
                || colB.DataType.SqlDataType == SqlDataType.Char || colB.DataType.SqlDataType == SqlDataType.NChar)
                bType += "(" + colB.DataType.MaximumLength + ")";
            Text = colA.Name + "(" + aType +","+bType+")";
            ButtonCaption = string.Format("Copy type from '{0}' to '{1}'", dbA.Name, dbB.Name);
            ButtonClick = new a7LambdaCommand((o) =>
                {
                    if (colA.DataType.SqlDataType == SqlDataType.VarChar &&
                        colB.DataType.SqlDataType == SqlDataType.VarChar)
                    {
                        colB.DataType.MaximumLength = colA.DataType.MaximumLength;
                        tableB.Alter();
                    }
                    else
                        MessageBox.Show("Not supported for non-varchars");
                }
            );
            Button2Caption = string.Format("Copy type from '{0}' to '{1}'", dbB.Name, dbA.Name);
            Button2Visibility = Visibility.Visible;
            Button2Click = new a7LambdaCommand((o) =>
            {
                if (colA.DataType.SqlDataType == SqlDataType.VarChar &&
                        colB.DataType.SqlDataType == SqlDataType.VarChar)
                {
                    colA.DataType.MaximumLength = colB.DataType.MaximumLength;
                    tableA.Alter();
                }
                else
                    MessageBox.Show("Not supported for non-varchars");
            }
            );
        }
    }
}
