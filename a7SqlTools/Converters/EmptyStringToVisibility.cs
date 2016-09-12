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
    class EmptyStringToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emptyValue = Visibility.Collapsed;
            var notEmptyValue = Visibility.Visible;
            if (parameter?.ToString().ToUpper().Trim() == "NEGATE")
            {
                emptyValue = Visibility.Visible;
                notEmptyValue = Visibility.Collapsed;
            }
            if (value == null || value.ToString() == "")
            {
                return emptyValue;
            }
            else
            {
                return notEmptyValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
