using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using DoIt.ViewModels;

namespace DoIt
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            if (App.IsAdvertismentVisible)
            {
                var adControl = AdControlHelper.CreateAdControl("10167811");
                Grid.SetRow(adControl, 1);
                adControl.Margin = new Thickness(-12, 6, -12, 0);
                this.SettingsGrid.Children.Add(adControl);
            }
            this.DataContext = new AboutPageViewModel(App.DataManager);
            VersionTextBlock.Text = String.Format("Version {0}", App.VersionNumber);
        }

        private void MailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();

            task.To = "support@philbi.de";
            task.Subject = string.Format(Localization.Strings.AboutPageSupportMailSubject, Localization.Strings.ApplicationTitle);
            task.Body = string.Format(Localization.Strings.AboutPageSupportMailBody, Localization.Strings.ApplicationTitle, App.VersionNumber);

            task.Show();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://www.philbi.de/");
            task.Show();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Localization.Strings.MainPageDisconnectAccountMessage, Localization.Strings.MainPageDisconnectAccountMessageTitle,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.NavigationService.GoBack();
                App.DataManager.DisconnectAccount();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var viewModel = (AboutPageViewModel)this.DataContext;
            if (App.DataManager.LoggedIn)
                viewModel.Apply();

            base.OnNavigatedFrom(e);
        }
    }
}