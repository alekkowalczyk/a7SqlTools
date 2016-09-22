using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace a7SqlTools.Converters
{
    public class CollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IList<string>)
            {
                var ret = "";
                var isFirst = true;
                foreach (var v in (value as IList<string>))
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        ret += "; ";
                    ret += v;
                }
                return ret;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
