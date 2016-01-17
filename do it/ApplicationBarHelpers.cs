using System;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;

namespace DoIt
{
    /// <summary>
    /// Codesparende Methoden für die ApplicationBar.
    /// </summary>
    internal static class ApplicationBarHelpers
    {
        internal static ApplicationBarIconButton CreateButton(string text, string iconPath, NavigationService navigationService, string navigateTo)
        {
            ApplicationBarIconButton button = new ApplicationBarIconButton(new Uri(iconPath, UriKind.Relative));
            button.Text = text;
            button.Click += (sender, e) => { navigationService.Navigate(new Uri(navigateTo, UriKind.Relative)); };
            return button;
        }

        internal static ApplicationBarMenuItem CreateMenuItem(string text, NavigationService navigationService, string navigateTo)
        {
            ApplicationBarMenuItem item = new ApplicationBarMenuItem(text);
            item.Click += (sender, e) => { navigationService.Navigate(new Uri(navigateTo, UriKind.Relative)); };
            return item;
        }
    }
}
