using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.TableExplorer;

namespace a7SqlTools.Controls.FilterEditor
{
    class FilterEditorUtils
    {
        public static IEnumerable<FilterElementDefinition> GetFilterEditorElements(a7SingleTableExplorer collection)
        {
            if (collection != null)
            {
                var retList = new List<FilterElementDefinition>();
                foreach(var prop in collection.AvailableProperties)
                {
                    retList.Add(FilterElementDefinition.GetFieldFilterElement(prop));
                }
                return retList;
            }
            return new List<FilterElementDefinition>();
        }
    }
}
