using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace a7SqlTools.Utils
{
    /// <summary>
    /// class used in extension methods for xml object, used for holding functions 
    /// that are converting xml node and attribute values
    /// to .net object properties.
    /// </summary>
    public class a7MappingXElement2Property
    {
        public string XElementName { get; private set; }
        public string PropertyName { get; private set; }
        private Func<string, object> _mapFunction;

        public a7MappingXElement2Property(string xElementName, string propertyName, Func<string,object> mapFunction = null)
        {
            this.XElementName = xElementName;
            this.PropertyName = propertyName;
            this._mapFunction = mapFunction;
        }

        public object Map(string value )
        {
            if (_mapFunction != null)
                return _mapFunction(value);
            else
                return value;
        }
    }
}
