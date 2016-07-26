using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using a7SqlTools.DbComparer.Data;

namespace a7SqlTools.DbComparer.Data.Views
{
    class a7ComparisonRowHeaderColumn : DataGridBoundColumn
    {
        protected override System.Windows.FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return null;
        }

        protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var row = dataItem as a7ComparisonRow;
            if (row != null)
            {
                if(row.IsDifferent)
                    return new TextBox() {  Background = new SolidColorBrush(Colors.Red), Width=30 };
                else
                    return new TextBox() {  Background = new SolidColorBrush(Colors.White), Width = 30 };
            }
            return new TextBox() { Text = dataItem?.ToString(), Background = new SolidColorBrush(Colors.LightPink) };
        }
    }
}
