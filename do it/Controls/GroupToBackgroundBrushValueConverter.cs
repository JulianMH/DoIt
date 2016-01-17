using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace DoIt.Controls
{
    //Aus ToolkitSample
    public class GroupToBackgroundBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FriendsByLetterGroup<ViewModels.FriendTaskAssignedToViewModel> group = value as FriendsByLetterGroup<ViewModels.FriendTaskAssignedToViewModel>;
            object result = null;

            if (group != null)
            {
                if (group.Count == 0)
                {
                    result = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
                }
                else
                {
                    result = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
