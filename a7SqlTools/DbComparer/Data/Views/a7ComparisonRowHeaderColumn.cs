using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
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
            this.Header = new TextBlock()
            {
                Text = "Status",
                FontWeight = FontWeights.Light,
                FontSize = 12.0,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var row = dataItem as a7ComparisonRow;
            if (row != null)
            {
                var tb = new TextBlock();
                tb.VerticalAlignment = VerticalAlignment.Stretch;
                tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                tb.Padding = new Thickness(5);
                tb.TextAlignment = TextAlignment.Center;
                if (row.IsOnlyInA)
                {
                    tb.Inlines.Add("Only in ");
                    tb.Inlines.Add(new Run() { Text="A", FontWeight = FontWeights.Bold });
                    tb.Background = new SolidColorBrush(Colors.Yellow);
                }
                else if (row.IsOnlyInB)
                {
                    tb.Inlines.Add("Only in ");
                    tb.Inlines.Add(new Run() { Text = "B", FontWeight = FontWeights.Bold });
                    tb.Background = new SolidColorBrush(Colors.Yellow);
                }
                else if (row.IsInBothDB && row.IsDifferent)
                {
                    tb.Inlines.Add("In ");
                    tb.Inlines.Add(new Run() { Text = "A", FontWeight = FontWeights.Bold });
                    tb.Inlines.Add(" and ");
                    tb.Inlines.Add(new Run() { Text = "B", FontWeight = FontWeights.Bold });
                    tb.Background = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    return new TextBlock() {Text = "Same A and B", Background = new SolidColorBrush(Colors.White)};
                }
                return tb;
                //if (row.IsDifferent)
                //    return new Rectangle() {  Fill = new SolidColorBrush(Colors.Yellow), Width=30 };
                //else
                //    return new Rectangle() {  Fill = new SolidColorBrush(Colors.White), Width = 30 };
            }
            return new TextBox() { Text = dataItem?.ToString(), Background = new SolidColorBrush(Colors.LightPink) };
        }
    }
}
