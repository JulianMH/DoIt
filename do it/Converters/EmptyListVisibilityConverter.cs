using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace DoIt.Converters
{
    public class EmptyListVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var enumerable = value as IEnumerable;

            if (enumerable != null)
                foreach (object o in enumerable)
                    return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Absichtlich nicht implementiert.
            return null;
        }
    }
}
