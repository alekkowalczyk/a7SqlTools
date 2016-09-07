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
        public PropertyDefinitionModel FieldData { get; private set; }

        public string Caption => FieldData?.Path;

        private FilterElementDefinition(PropertyDefinitionModel fieldData)
        {
            FieldData = fieldData;
        }

        public static FilterElementDefinition GetFieldFilterElement(PropertyDefinitionModel fieldData)
        {
            return new FilterElementDefinition(fieldData);
        }
    }
}
