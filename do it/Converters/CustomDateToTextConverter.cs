using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace DoIt.Converters
{
    public class CustomDateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var date = ((DateTime)value);

            if (date == DateTime.MinValue)
                return Localization.Strings.CustomDateToTextConverterSoon;
            else if (date == DateTime.MaxValue)
                return Localization.Strings.CustomDateToTextConverterFuture;
            else
                return Localization.Strings.CustomDateToTextConverterCustom;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = (string)value;
            if (text == Localization.Strings.CustomDateToTextConverterSoon)
                return DateTime.MinValue;
            else if (text == Localization.Strings.CustomDateToTextConverterFuture)
                return DateTime.MaxValue;
            else
                return DateTime.Today.AddDays(1);
        }
    }
}
