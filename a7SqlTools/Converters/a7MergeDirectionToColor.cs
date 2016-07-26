using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using a7SqlTools.DbComparer;
using a7SqlTools.Utils;

namespace a7SqlTools.Converters
{
    class a7MergeDirectionToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var type = parameter?.ToString();
            var direction = value.ToEnum<a7DbComparerDirection>(a7DbComparerDirection.None);
            if (direction == a7DbComparerDirection.None)
                return new SolidColorBrush(Colors.White);
            if (type.IsNotEmpty())
            {
                if (type.ToUpper() == "ATOB")
                {
                    if (direction == a7DbComparerDirection.AtoB)
                        return new SolidColorBrush(Colors.GreenYellow);
                    else if (direction == a7DbComparerDirection.BtoA)
                        return new SolidColorBrush(Colors.White);
                    else if (direction == a7DbComparerDirection.Partial)
                        return new SolidColorBrush(Colors.LightCoral);
                }
                else if (type.ToUpper() == "BTOA")
                {
                    if (direction == a7DbComparerDirection.AtoB)
                        return new SolidColorBrush(Colors.White);
                    else if (direction == a7DbComparerDirection.BtoA)
                        return new SolidColorBrush(Colors.GreenYellow);
                    else if (direction == a7DbComparerDirection.Partial)
                        return new SolidColorBrush(Colors.LightCoral);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
