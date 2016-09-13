using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.TableExplorer.Enums;

namespace a7SqlTools.TableExplorer
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public ColumnDefinition()
        {

        }

        public static ColumnDefinition GetEmpty()
            => new ColumnDefinition
            {
                Name = "",
                Type = PropertyType.String
            };
    }
}
