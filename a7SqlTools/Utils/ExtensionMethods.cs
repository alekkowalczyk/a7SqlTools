using a7SqlTools.DbSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace a7SqlTools.Utils
{
    static class ExtensionMethods
    {

        public static bool ContainsSearchedValue(this List<a7SearchedValue> list, string valueName)
        {
            foreach (var sv in list)
            {
                if (sv.Value == valueName)
                    return true;
            }
            return false;
        }

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
