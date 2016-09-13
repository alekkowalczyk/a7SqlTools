using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.TableExplorer;

namespace a7SqlTools.Controls.FilterEditor
{
    public class FilterElementDefinition
    {
        public ColumnDefinition FieldData { get; private set; }

        public string Caption => FieldData?.Name;

        private FilterElementDefinition(ColumnDefinition fieldData)
        {
            FieldData = fieldData;
        }

        public static FilterElementDefinition GetFieldFilterElement(ColumnDefinition fieldData)
        {
            return new FilterElementDefinition(fieldData);
        }
    }
}
