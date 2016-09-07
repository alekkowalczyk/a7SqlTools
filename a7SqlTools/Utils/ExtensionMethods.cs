using a7SqlTools.DbSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace a7SqlTools.Utils
{
    static class ExtensionMethods
    {
        //public static bool IsEmpty(this object obj)
        //{
        //    if (obj == null)
        //        return true;
        //    if (obj is List<string>)
        //        return (obj as List<string>).Count == 0;
        //    if (string.IsNullOrWhiteSpace(obj?.ToString()))
        //        return true;
        //    return false;
        //}

        //public static bool IsNotEmpty(this object obj)
        //{
        //    return !obj.IsEmpty();
        //}

        //public static bool IsNotEmpty(this string s)
        //{
        //    return !string.IsNullOrWhiteSpace(s);
        //}

        //public static bool IsEmpty(this string s)
        //{
        //    return string.IsNullOrWhiteSpace(s);
        //}

        //public static T ToEnum<T>(this object obj, T defaultValue = default(T))
        //{
        //    if (obj == null)
        //        return defaultValue;
        //    else
        //    {
        //        return (obj?.ToString() ?? "").ToEnum<T>(defaultValue);
        //    }
        //}

        //public static T ToEnum<T>(this string s, T defaultValue = default(T))
        //{
        //    if (s.IsEmpty())
        //        return defaultValue;
        //    else
        //    {
        //        try
        //        {
        //            return (T)Enum.Parse(typeof(T), s, true);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new Exception("couldn't not parse string:'" + s + "' to enum:'" + typeof(T).Name + "'" + Environment.NewLine + e.ToString());
        //        }
        //    }
        //}

        //public static bool ContainsSearchedValue(this List<a7SearchedValue> list, string valueName)
        //{
        //    foreach (var sv in list)
        //    {
        //        if (sv.Value == valueName)
        //            return true;
        //    }
        //    return false;
        //}

        public static void RemoveItem<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var toRemove = list.Where(predicate).ToList();
            foreach (var tr in toRemove)
            {
                list.Remove(tr);
            }
        }
    }
}
