using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DoIt
{
    internal static class SystemTrayHelper
    {
        internal static void ShowProgress(PhoneApplicationPage page, string text, double opacity)
        {
            SystemTray.SetOpacity(page, opacity);
            SystemTray.SetIsVisible(page, true);
            SystemTray.SetProgressIndicator(page, new ProgressIndicator()
            {
                IsIndeterminate = true,
                IsVisible = true,
                Text = text
            });
        }

        internal static void Hide(PhoneApplicationPage page)
        {
            SystemTray.SetIsVisible(page, false);
            SystemTray.SetOpacity(page, 1);
            SystemTray.SetProgressIndicator(page, null);

        }

        internal static void HideProgress(PhoneApplicationPage page)
        {
            SystemTray.SetProgressIndicator(page, null);
        }
    }
}
