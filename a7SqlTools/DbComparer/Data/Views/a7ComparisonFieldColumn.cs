using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace a7SqlTools.DbComparer.Data.Views
{
    class a7ComparisonFieldColumn : DataGridBoundColumn
    {
        public string ColumnName { get; set; }

        protected override System.Windows.FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return new TextBox() { Text = dataItem?.ToString(), Background = new SolidColorBrush(Colors.Blue) };
        }

        protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var row = dataItem as a7ComparisonRow;
            if (row != null)
            {
                var fld = row[ColumnName];
                return new a7ComparisonFieldControl(fld);
            }
            return new TextBox() { Text = dataItem?.ToString(), Background = new SolidColorBrush(Colors.LightPink) };
        }
    }
}
