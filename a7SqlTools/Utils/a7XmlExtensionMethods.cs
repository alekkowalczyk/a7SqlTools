using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace a7SqlTools.Utils
{
    /// <summary>
    /// xml related extension methods
    /// </summary>
    public static class a7XmlExtensionMethods
    {
        /// <summary>
        /// Parses the string to XElement, and converts it to .net object. 
        /// The XElement should have simple flat structure, the sub-elements and attributes of the main element will be mapped.
        /// XElement and XAttribute values are directly mapped to according (by name) object properties.
        /// Or mapped by given in parameters mapping classes for custom mapping (other name of properties or some converting logic)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static T XmlToObject<T>(this string s,params a7MappingXElement2Property[] mappings) where T : new()
        {
            if (!string.IsNullOrEmpty(s))
                return XElement.Parse(s).ToObject<T>(mappings);
            else
                return default(T);
        }

        /// <summary>
        /// Converting XElement to .net object
        /// The XElement should have simple flat structure, the sub-elements and attributes of the main element will be mapped.
        /// XElement and XAttribute values are directly mapped to according (by name) object properties.
        /// Or mapped by given in parameters mapping classes for custom mapping (other name of properties or some converting logic)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static T ToObject<T>(this XElement x, params a7MappingXElement2Property[] mappings) where T : new()
        {
            T t = new T();
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string value = null;
                a7MappingXElement2Property fieldMap = mappings.FirstOrDefault<a7MappingXElement2Property>
                                    ((map) => map.PropertyName == prop.Name );
                string xElementName = prop.Name;
                if (fieldMap != null)
                    xElementName = fieldMap.XElementName;
                XAttribute atr = x.Attribute(xElementName);
                if (atr != null)
                    value = atr.Value;
                else
                {
                    XElement xe = x.Element(xElementName);
                    if (xe != null)
                        value = xe.Value;
                }
                if (value != null)
                {
                    if (fieldMap != null)
                        prop.SetValue(t, fieldMap.Map(value), null);
                    else
                        prop.SetValue(t, value, null);
                }
            }
            return t;
        }

        /// <summary>
        /// Parsing string to XElement and mapping it to .net object list.
        /// Each subnode of this xelement will be mapped to an .net object with the .ToObject extension method, mapping each
        /// subnode of it and attribute to object properties (optionally using given mapping classes for custom mapping on other 
        /// property names or using some converting functions)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static List<T> XmlToObjectList<T>(this string s, params a7MappingXElement2Property[] mappings) where T : new()
        {
            if (!string.IsNullOrEmpty(s))
                return XElement.Parse(s).ToObjectList<T>(mappings);
            else
                return null;
        }

        /// <summary>
        /// Mapping an XElement to .net object list.
        /// Each subnode of this xelement will be mapped to an .net object with the .ToObject extension method, mapping each
        /// subnode of it and attribute to object properties (optionally using given mapping classes for custom mapping on other 
        /// property names or using some converting functions)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static List<T> ToObjectList<T>(this XElement x, params a7MappingXElement2Property[] mappings) where T : new()
        {
            List<T> retList = new List<T>();
            foreach (XElement xChild in x.Elements())
            {
                retList.Add(xChild.ToObject<T>(mappings));
            }
            return retList;
        }
    }
}
