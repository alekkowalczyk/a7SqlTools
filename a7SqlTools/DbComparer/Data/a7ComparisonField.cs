using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.DbComparer.Data
{
    public class a7ComparisonField
    {
        public object ValueA { get; }
        public object ValueB { get; }
        public bool RowAExists { get; }
        public bool RowBExists { get;  }
        public string Name { get; }
        public bool AisB => ValueA?.ToString() == ValueB?.ToString(); 

        private a7DbDataComparer _comparer;

        public a7ComparisonField(object valA, object valB, bool rowAExists, bool rowBExists, string name, a7DbDataComparer comparer)
        {
            ValueA = valA;
            ValueB = valB;
            RowAExists = rowAExists;
            RowBExists = rowBExists;
            Name = name;
            _comparer = comparer;
        }


        public override string ToString() => ValueA?.ToString() + ":" + ValueB?.ToString();
    }
}
