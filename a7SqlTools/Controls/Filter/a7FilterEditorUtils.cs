using System.Collections.Generic;
using a7SqlTools.TableExplorer;

namespace a7SqlTools.Controls.Filter
{
    class a7FilterEditorUtils
    {
        public static IEnumerable<a7FilterElementDefinition> GetFilterEditorElements(a7SingleTableExplorer entity)
        {
            if (entity != null)
            { 
                    //fields
                    var elements = new List<a7FilterElementDefinition>();
                    foreach (var fld in entity.AvailableColumns)
                        elements.Add(a7FilterElementDefinition.GetFieldFilterElement(fld));
                    return elements;
            }
            return new List<a7FilterElementDefinition>();
        }
    }
}
