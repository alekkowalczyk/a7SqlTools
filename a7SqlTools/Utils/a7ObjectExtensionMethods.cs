using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace a7SqlTools.Utils
{
    /// <summary>
    /// general extension methods for object class
    /// </summary>
    public static class a7ObjectExtensionMethods
    {
        /// <summary>
        /// runs action if object is of type T, if not or null, doesn't run it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static void Execute<T>(this object obj, Action<T> action)
        {
            if(obj!=null && obj is T)
            {
                T t = (T)obj;
                action(t);
            }
        }

        /// <summary>
        /// sets a value of a property by reflection
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            Type t = obj.GetType();
            PropertyInfo pi = t.GetProperty(propertyName);
            if (pi != null)
            {
                if (value == System.DBNull.Value)
                    value = null;
                pi.SetValue(obj, value, null);
            }
        }

        /// <summary>
        /// get a value of a property by reflection
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;
            Type t = obj.GetType();
            PropertyInfo pi = t.GetProperty(propertyName.BeforeString(".", propertyName));
            string subProperties = propertyName.AfterString(".");
            if (pi != null)
            {
                object mainPropertyObject = pi.GetValue(obj, null);
                if (subProperties == "")
                    return mainPropertyObject;
                else
                    return mainPropertyObject.GetPropertyValue(subProperties);
            }
            return null;
        }

        /// <summary>
        /// checks if the property exists on the object by reflection
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists(this object obj, string propertyName)
        {
            Type t = obj.GetType();
            return t.GetProperty(propertyName) != null;
        }

        /// <summary>
        /// gets an array of properties that the object have
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this object obj)
        {
            Type t = obj.GetType();
            return t.GetProperties();
        }

        /// <summary>
        /// returns the ToString value but with ToUpper and Trim applied
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToUpperTrimmedString(this object obj)
        {
            if (obj == null)
                return "";
            else
                return obj.ToString().ToUpper().Trim();
        }

        /// <summary>
        /// returns the ToString value, works on null values too
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToStringAllowsNull(this object obj)
        {
            return obj.ToStringAllowsNull(false);
        }

        public static string ToStringAllowsNull(this object obj, bool withNumberFormat)
        {
            if (obj == null)
                return "";
            else
            {
                if (!withNumberFormat)
                    return obj.ToString();

                if (obj is int)
                    return ((int)obj).ToString("D8");
                if (obj is long)
                    return ((long)obj).ToString("D8");
                if (obj is DateTime)
                    return ((DateTime)obj).ToString("yyyy-MM-dd HH:mm.ss");

                return obj.ToString();
            }
        }

        public static int ToInt(this object obj, int defValue = -1)
        {
            if (obj == null)
                return defValue;
            if(obj is string)
            {
                int i = defValue;
                int.TryParse(obj as string, out i);
                return i;
            }
            if(obj is double)
            {
                return (int)Math.Round((double)obj, 0, MidpointRounding.AwayFromZero);
            }
            if(obj is decimal)
            {
                return decimal.ToInt32((decimal)obj);
            }
            if(obj is int)
                return (int)obj;
            return defValue;
        }


        public static long ToLong(this object obj, long defValue = -1)
        {
            if (obj == null)
                return defValue;
            if (obj is string)
            {
                long i = defValue;
                if (long.TryParse(obj as string, out i))
                    return i;
                else
                    return defValue;
            }
            if (obj is long)
                return (long)obj;
            if (obj is int)
                return (int)obj;
            return defValue;
        }

        public static long? ToLongNullable(this object obj)
        {
            if (obj == null)
                return null;
            if (obj is string)
            {
                long i = -1;
                if (long.TryParse(obj as string, out i))
                    return i;
                else
                    return null;
            }
            if (obj is long)
                return (long)obj;
            if (obj is int)
                return (int)obj;
            return null;
        }

        public static decimal ToDecimal(this object obj, decimal defValue = -1)
        {
            if (obj == null)
                return defValue;
            if (obj is string)
            {
                decimal i = defValue;
                decimal.TryParse(obj as string, out i);
                return i;
            }
            if (obj is decimal)
                return (decimal)obj;
            return defValue;
        }


        public static DateTime ToDateTime(this object obj, DateTime? defaultValue = null)
        {
            DateTime defValue = DateTime.MinValue;
            if (defaultValue != null)
                defValue = defaultValue.Value;

            if (obj == null)
                return defValue;
            if (obj is string)
            {
                DateTime i = defValue;
                DateTime.TryParse(obj as string, out i);
                return i;
            }
            if (obj is DateTime)
                return (DateTime)obj;
            return defValue;
        }

        public static DateTime? ToDateTimeNullable(this object obj)
        {
            DateTime defValue = DateTime.MinValue;

            if (obj == null)
                return null;
            if (obj is string)
            {
                DateTime i = defValue;
                if (DateTime.TryParse(obj as string, out i))
                    return i;
                else
                    return null;
            }
            if (obj is DateTime)
                return (DateTime)obj;
            return null;
        }

        public static TimeSpan? ToTimeSpanNullable(this object obj)
        {
            TimeSpan defValue = TimeSpan.MinValue;

            if (obj == null)
                return null;
            if (obj is string)
            {
                TimeSpan i = defValue;
                if (TimeSpan.TryParse(obj as string, out i))
                    return i;
                else
                    return null;
            }
            if (obj is TimeSpan)
                return (TimeSpan)obj;
            return null;
        }

        public static T ToEnum<T>(this object obj, T defaultValue = default(T))
        {
            if (obj == null)
                return defaultValue;
            else
            {
                return obj.ToStringAllowsNull().ToEnum<T>(defaultValue);
            }
        }

        public static bool IsEmpty(this object obj)
        {
            if (obj == null)
                return true;
            if (obj is List<string>)
                return (obj as List<string>).Count == 0;
            if (string.IsNullOrWhiteSpace(obj.ToStringAllowsNull()))
                return true;
            return false;
        }

        public static bool IsNotEmpty(this object obj)
        {
            return !obj.IsEmpty();
        }

        public static T Cast<T>(this object obj)
        {
            if (obj is T)
                return (T)obj;
            else
                return default(T);
        }

        public static bool ToBool(this object obj, bool defaultValue = false)
        {
            bool b;
            if (obj == null)
                b = defaultValue;
            else if (obj is bool)
                b = (bool)obj;
            else if (obj is int)
                b = ((int)obj) != 0;
            else if (obj is string)
            {
                var s = obj as string;
                if (s == "1")
                    return true;
                else if (s == "0")
                    return false;
                else if (s.Trim().ToUpper() == "TRUE")
                    return true;
                else if (s.Trim().ToUpper() == "FALSE")
                    return false;
                else if (!bool.TryParse(s, out b))
                    b = defaultValue; //todo:error handling
            }
            else
            {
                if (!bool.TryParse(obj.ToString(), out b))
                        b = defaultValue; //todo:error handling
            }
            return b;
        }

        public static bool? ToBoolNullable(this object obj)
        {
            bool b;
            if (obj == null)
                return null;
            else if (obj is bool)
                return (bool)obj;
            else if (obj is int)
                return ((int)obj) != 0;
            else if (obj is string)
            {
                var s = obj as string;
                if (s == "1")
                    return true;
                else if (s == "0")
                    return false;
                else if (s.Trim().ToUpper() == "TRUE")
                    return true;
                else if (s.Trim().ToUpper() == "FALSE")
                    return false;
                else if (!bool.TryParse(s, out b))
                    return null; //todo:error handling
            }
            else
            {
                if (!bool.TryParse(obj.ToString(), out b))
                    return null; //todo:error handling
            }
            return b;
        }

        public static List<T> ToElementList<T>(this T obj, params T[] moreObject)
        {
            var retList = new List<T>() { obj };
            foreach (var mor in moreObject)
                retList.Add(mor);
            return retList;
        }
    }
}
