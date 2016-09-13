using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace a7SqlTools.Utils
{
    public class ResourcesManager
    {
        private static ResourcesManager _instance;
        public static ResourcesManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ResourcesManager(@"pack://application:,,,/a7SqlTools;component/Themes");
                return _instance;
            }
        }

        private readonly Dictionary<string, ResourceDictionary> _cachedResourceDictionaries;
        private string _uri;
        private string _templatesUri;

        private ResourcesManager(string uri)
        {
            _cachedResourceDictionaries = new Dictionary<string, ResourceDictionary>();
            _uri = uri;
            _templatesUri = uri + @"/Templates";
        }

        public void ClearCache()
        {
            _cachedResourceDictionaries.Clear();
        }

        private ResourceDictionary GetResourceDictionary(string name, bool fromTemplates = true)
        {
            if (!_cachedResourceDictionaries.ContainsKey(name))
            {
                ResourceDictionary rd = new ResourceDictionary();
                Uri uri;
                try
                {
                    uri = new Uri(
                        string.Format("{0}/{1}.xaml", fromTemplates ? _templatesUri : _uri, name),
                        UriKind.Absolute);
                    rd.Source = uri;
                    //WalkDictionary(rd);
                    //return rd;
                    _cachedResourceDictionaries[name] = rd;
                }
                catch (System.IO.IOException ex)
                {
                    return null;
                }
            }
            return _cachedResourceDictionaries[name];
        }

        private void WalkDictionary(ResourceDictionary resources)
        {
            foreach (DictionaryEntry entry in resources)
            {
            }

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
                WalkDictionary(rd);
        }

        public T GetResource<T>(string dictionaryName, string resourceKey)
        {
            object o = GetResourceDictionary(dictionaryName)[resourceKey];
            if (o is T)
                return (T)o;
            else
                return default(T);
        }

        public T GetResource<T>(string resourceKey)
        {
            object o = GetResourceDictionary("Templates")[resourceKey];
            if (o is T)
                return (T)o;
            else
                return default(T);
        }

        public ControlTemplate GetControlTemplate(string resourceKey)
        {
            return GetResource<ControlTemplate>(resourceKey);
        }

        public Style GetStyle(string resourceKey)
        {
            return GetResource<Style>(resourceKey);
        }

        public Brush GetBrush(string resourceKey)
        {
            return GetResourceDictionary("CommonBrushes", false)[resourceKey] as Brush;
        }
    }
}
