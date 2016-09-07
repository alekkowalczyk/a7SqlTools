using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace a7SqlTools.Utils
{
    /// <summary>
    /// class for extension methods for the string class
    /// </summary>
    public static class a7StringExtensionMethods
    {
        public static bool EqualsTrimmedInvariant(this string s, string compare)
        {
            return s.Trim().Equals(compare, StringComparison.InvariantCulture);
        }
        public static string BeforeBrackets(this string s)
        {
            return s.BeforeString("(", false, s);
        }

        public static string BetweenBrackets(this string s)
        {
            return s.BetweenStrings("(", ")", false, "");
        }

        public static bool EqualsOneOf(this string s, params string[] equals)
        {
            foreach (var eq in equals)
            {
                if (s.Equals(eq))
                    return true;
            }
            return false;
        }

        public static string Cut(this string s,int length)
        {
            if (s.Length > length)
                return s.Substring(0, length);
            else
                return s;
        }

        public static string CutFromRight(this string s, int length)
        {
            if (s.Length > length)
                return s.Substring(0, s.Length- length);
            else
                return s;
        }

        /// <summary>
        /// escapes the string in a sql way, currently escapes only an apostrophe with a double apostrophe
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SqlEscape(this string s)
        {
            return s.Replace("'", "''");
        }

        /// <summary>
        /// splitting the string with the comma "," char and removing empty entries
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] SplitComma(this string s)
        {
            if(s.IsNotEmpty())
                return s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            else
                return new string[0];
        }

        public static bool IsNotEmpty(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static bool IsEmpty(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// splitting the string with the given char and removing empty entries
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] SplitChar(this string s, char splitter, bool removeEmptyEntries = true)
        {
            if (s.IsNotEmpty())
            {
                if(removeEmptyEntries)
                    return s.Split(new char[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                else
                    return s.Split(new char[] { splitter }, StringSplitOptions.None);
            }
            else
                return new string[0];
        }

        public static string[] Split(this string s, string splitter, bool removeEmptyEntries = true)
        {
            if (s.IsNotEmpty())
            {
                if (removeEmptyEntries)
                    return s.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                else
                    return s.Split(new string[] { splitter }, StringSplitOptions.None);
            }
            else
                return new string[0];
        }

        public static List<string> SplitCharList(this string s, char splitter, bool removeEmptyEntries = true)
        {
            var list = s.SplitChar(splitter, removeEmptyEntries).ToList<string>();
            if (list == null)
                list = new List<string>();
            return list;
        }

        /// <summary>
        /// splitting the string with the dot "." char and removing empty entries
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] SplitDot(this string s)
        {
            if (s.IsNotEmpty())
                return s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            else
                return new string[0];
        }

        public static Dictionary<string,object> ToObjectDictionary(this string s, char keyValueSeperator = ':', char collectionSeperator = ';')
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            string[] collection = s.SplitChar(collectionSeperator);
            foreach (var skv in collection)
            {
                var skvArray = skv.SplitChar(keyValueSeperator);
                retDict.Add(skvArray[0], skvArray[1]);
            }
            return retDict;
        }

        public static Dictionary<string, string> ToStringDictionary(this string s, char keyValueSeperator = ':', char collectionSeperator = ';')
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            if (s.IsEmpty())
                return retDict;
            var sEscaped = s.Replace(string.Format("{0}{1}",keyValueSeperator, keyValueSeperator), "###");
            sEscaped = sEscaped.Replace(string.Format("{0}{1}", collectionSeperator, collectionSeperator), "$$$");
            string[] collection = sEscaped.SplitChar(collectionSeperator);
            foreach (var skv in collection)
            {
                var skvArray = skv.SplitChar(keyValueSeperator);
                var key = skvArray[0];
                var value = "";
                if (skvArray.Length > 1)
                    value = skvArray[1];
                retDict.Add(key, value.Replace("###", keyValueSeperator.ToStringAllowsNull())
                                                    .Replace("$$$", collectionSeperator.ToStringAllowsNull()));
            }
            return retDict;
        }

        /// <summary>
        /// returns the string that is before the given search string, optionally searchstring included
        /// </summary>
        /// <param name="s"></param>
        /// <param name="search"></param>
        /// <param name="withSearchedString"></param>
        /// <returns></returns>
        public static string BeforeString(this string s, string search, bool withSearchedString = false, string valueIfSearchedNotFound = "")
        {
            int iIndexOfSearch = s.IndexOf(search);
            if (iIndexOfSearch != -1)
            {
                if (withSearchedString)
                    return s.Substring(0, iIndexOfSearch + search.Length);
                else
                    return s.Substring(0, iIndexOfSearch);
            }
            else
                return valueIfSearchedNotFound;
        }

        public static string BeforeString(this string s, string search, string valueIfSearchedNotFound, bool withSearchedString = false)
        {
            return s.BeforeString(search,withSearchedString, valueIfSearchedNotFound);
        }

        public static string BetweenStrings(this string s, string after, string before, bool withSearchedString = false, string valueIfSearchedNotFound = "")
        {
            if (s.IsEmpty())
                return valueIfSearchedNotFound;
            int iIndexOfAfter = s.IndexOf(after);
            int iIndexOfBefore = s.LastIndexOf(before);
            if (iIndexOfAfter != -1 && iIndexOfBefore != -1 && iIndexOfBefore>iIndexOfAfter)
            {
                iIndexOfAfter += after.Length;
                string between = s.Substring(iIndexOfAfter, iIndexOfBefore - iIndexOfAfter);
                if (withSearchedString)
                    return after + between + before;
                else
                    return between;
            }
            else
                return valueIfSearchedNotFound;
        }

        /// <summary>
        /// returns an parsed int from a string, before the given search string, if no int before the search string, defaultValue is returned
        /// </summary>
        /// <param name="s"></param>
        /// <param name="search"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int BeforeInt(this string s, string search, int defaultValue = -1)
        {
            int iIndexOfSearch = s.IndexOf(search);
            int iReturn = defaultValue;
            int.TryParse(s.BeforeString(search, false), out iReturn);
            return iReturn;
        }

        /// <summary>
        /// returns the string that is after the given search string, optionally searchstring included
        /// </summary>
        /// <param name="s"></param>
        /// <param name="search"></param>
        /// <param name="withSearchedString"></param>
        /// <returns></returns>
        public static string AfterString(this string s, string search, bool withSearchedString = false, string valueIfSearchedNotFound = "")
        {
            int iIndexOfSearch = s.IndexOf(search);
            if (iIndexOfSearch != -1)
            {
                if (withSearchedString)
                    return s.Substring(iIndexOfSearch);
                else
                    return s.Substring(iIndexOfSearch+search.Length);
            }
            else
                return valueIfSearchedNotFound;
        }

        public static string AfterString(this string s, string search, string valueIfSearchedNotFound, bool withSearchedString = false)
        {
            return s.AfterString(search, withSearchedString, valueIfSearchedNotFound);
        }

        /// <summary>
        /// returns an parsed int from a string, after the given search string, if no int before the search string, defaultValue is returned
        /// </summary>
        /// <param name="s"></param>
        /// <param name="search"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int AfterInt(this string s, string search, int defaultValue = -1)
        {
            int iIndexOfSearch = s.IndexOf(search);
            int iReturn = defaultValue;
            int.TryParse(s.AfterString(search, false), out iReturn);
            return iReturn;
        }

        public static double ToDouble(this string s, double defaultValue = double.NaN)
        {
            double retD;
            if (double.TryParse(s, out retD))
                return retD;
            else
                return defaultValue;
        }

        public static T ToEnum<T>(this string s, T defaultValue = default(T))
        {
            if (s.IsEmpty())
                return defaultValue;
            else
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), s, true);
                }
                catch (Exception e)
                {
                    throw new Exception("couldn't not parse string:'" + s + "' to enum:'" + typeof(T).Name + "'" + Environment.NewLine + e.ToString());
                }
            }
        }

        public static int ToInt(this string s, int defaultValue)
        {
            int i = defaultValue;
            int.TryParse(s, out i);
            return i;
        }

        public static List<object> XStringToObjectList(this string s)
        {
            List<object> Args = new List<object>();
            if (s.IsNotEmpty())
            {
                XDocument x = XDocument.Parse(s);
                foreach (XElement node in x.Root.Elements())
                {
                    if (node.HasElements)
                        Args.Add(node.FirstNode.ToString());
                    else
                        Args.Add(node.Value);
                }
            }
            return Args;
        }

        public static object[] XStringToObjectArray(this string s)
        {
            return s.XStringToObjectList().ToArray();
        }

        
    }
}
