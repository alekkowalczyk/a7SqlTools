using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace a7SqlTools.Converters
{
    public class NullToCollapsedVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter != null && parameter.ToString().ToUpper() == "NEGATE")
            {
                if (value == null)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            if(value == null)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
