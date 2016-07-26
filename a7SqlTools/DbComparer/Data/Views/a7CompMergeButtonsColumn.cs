using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace a7SqlTools.DbComparer.Data.Views
{
    class a7CompMergeButtonsColumn : DataGridBoundColumn
    {
        protected override System.Windows.FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new NotImplementedException();
        }

        protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return new a7CompMergeButtonsControl() { DataContext = dataItem, Width=60 };
        }
    }
}
