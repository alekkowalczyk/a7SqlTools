using a7SqlTools.TableExplorer;

namespace a7SqlTools.Controls.Filter
{
    public class a7FilterElementDefinition
    {
        public ColumnDefinition FieldData { get; private set; }

        public string Caption => FieldData.Name;

        private a7FilterElementDefinition(ColumnDefinition fieldData)
        {
            FieldData = fieldData;
        }

        public static a7FilterElementDefinition GetFieldFilterElement(ColumnDefinition fieldData)
        {
            return new a7FilterElementDefinition(fieldData);
        }
    }
}
