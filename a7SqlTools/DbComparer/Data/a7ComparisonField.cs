using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.DbComparer.Data
{
    public class a7ComparisonField
    {
        public object ValueA { get; private set; }
        public object ValueB { get; private set; }
        public string Name { get; private set; }
        public bool AisB => ValueA?.ToString() == ValueB?.ToString(); 

        private a7DbDataComparer _comparer;

        public a7ComparisonField(object valA, object valB, string name, a7DbDataComparer comparer)
        {
            ValueA = valA;
            ValueB = valB;
            Name = name;
            _comparer = comparer;
        }


        public override string ToString() => ValueA?.ToString() + ":" + ValueB?.ToString();
    }
}
