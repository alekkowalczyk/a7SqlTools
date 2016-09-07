using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace a7SqlTools.Utils
{
    /// <summary>
    /// class for extension methods for the XElement class
    /// </summary>
    public static class a7XElementExtensionMethods
    {
        /// <summary>
        /// return the attribute as an int, if attribute doesn't exist in the given xelement, or isn't a integer, returns defaultValue (default of defaultValue is -1 :) ).
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName">name of the attribute which value should be returned</param>
        /// <param name="defaultValue">default value if attribute doesn't exist or it value isn't a integer</param>
        /// <param name="caseSensitive">is the name of the attribute case sensitive</param>
        /// <returns></returns>
        public static int AttributeAsInteger(this XElement el, string attributeName, int defaultValue=-1, bool caseSensitive = false)
        {
            if (el == null)
                return defaultValue;
            XAttribute xaFound = el.AttributeAsXAttribute(attributeName, caseSensitive);          
            int iReturn = defaultValue;
            if (xaFound != null)
                int.TryParse(xaFound.Value, out iReturn);
            return iReturn;
        }

        public static string InnerXml(this XElement el)
        {
            if (el == null)
                return "";
            var reader = el.Parent.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        /// <summary>
        /// return the attribute as an double, if attribute doesn't exist in the given xelement, or isn't a double, returns defaultValue (default of defaultValue is -1 :) ).
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName">name of the attribute which value should be returned</param>
        /// <param name="defaultValue">default value if attribute doesn't exist or it value isn't a double</param>
        /// <param name="caseSensitive">is the name of the attribute case sensitive</param>
        /// <returns></returns>
        public static double AttributeAsDouble(this XElement el, string attributeName, double defaultValue = -1, bool caseSensitive = false)
        {
            if (el == null)
                return defaultValue;
            XAttribute xaFound = el.AttributeAsXAttribute(attributeName, caseSensitive);
            double dReturn = defaultValue;
            if (xaFound != null)
                double.TryParse(xaFound.Value, out dReturn);
            return dReturn;
        }

        /// <summary>
        /// return the attribute as an nullable double, if attribute doesn't exist in the given xelement, or isn't a double, returns defaultValue (default of defaultValue is null :) ).
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static double? AttributeAsNullableDouble(this XElement el, string attributeName, double? defaultValue = null, bool caseSensitive = false)
        {
            if (el == null)
                return null;
            XAttribute xaFound = el.AttributeAsXAttribute(attributeName, caseSensitive);
            double? dReturn = defaultValue;
            double d;
            if (xaFound != null)
                if (double.TryParse(xaFound.Value, out d))
                    dReturn = d;
            return dReturn;
        }

        /// <summary>
        /// return the attribute as an string, if attribute doesn't exist in the given xelement return empty string "".
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName">name of the attribute which value should be returned</param>
        /// <param name="caseSensitive">is the name of the attribute case sensitive</param>
        /// <returns></returns>
        public static string AttributeAsString(this XElement el, string attributeName, bool caseSensitive = false)
        {
            if (el == null)
                return "";
            XAttribute xaFound = el.AttributeAsXAttribute(attributeName, caseSensitive);
            if (xaFound == null)
                return "";
            else
                return xaFound.Value;
        }

        /// <summary>
        /// return the attribute as bool, if attribute doesn't exist in the given xelement, or isn't a bool, returns defaultValue (default of defaultValue is false :) ).
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool AttributeAsBool(this XElement el, string attributeName, bool defaultValue = false)
        {
            if (el == null)
                return defaultValue;
            string sBool = el.AttributeAsString(attributeName).Trim().ToUpper();
            if (sBool == "TRUE" || sBool == "1" || sBool == "-1")
                return true;
            else if (sBool == "FALSE" || sBool == "0")
                return false;
            else
                return defaultValue;
        }


        /// <summary>
        /// returns the XAttribute from the XElement, the difference to normal Attributes function of XElement is
        /// that this can be caseInsensitive
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static XAttribute AttributeAsXAttribute(this XElement el, string attributeName, bool caseSensitive = false)
        {
            if (el == null)
                return null;
            XAttribute xaFound = null;
            foreach (XAttribute xa in el.Attributes())
            {
                if (caseSensitive && xa.Name.LocalName == attributeName)
                {
                    xaFound = xa;
                    break;
                }
                else if (!caseSensitive && xa.Name.LocalName.ToUpper() == attributeName.ToUpper())
                {
                    xaFound = xa;
                    break;
                }
            }
            return xaFound;
        }

        public static void OvertakeAttributes(this XElement el, XElement x)
        {
            if (x == null || el == null)
                return;
            foreach (var at in x.Attributes())
            {
                var existing = el.Attribute(at.Name);
                if (existing == null)
                    el.Add(new XAttribute(at.Name, at.Value));
            }
        }
    }
}
