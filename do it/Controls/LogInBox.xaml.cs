using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DoIt.Controls
{
    public partial class LogInBox : UserControl
    {
        public LogInBox()
        {
            InitializeComponent();
            UpdateVisibility();

            if (App.DataManager != null)
                App.DataManager.PropertyChanged += DataManager_PropertyChanged;
        }

        private void UpdateVisibility()
        {
            this.Visibility = (App.DataManager != null && App.DataManager.LogInKnown) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void DataManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogInKnown")
                UpdateVisibility();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            PerfomanceProgressBar.Visibility = Visibility.Visible;
            PerfomanceProgressBar.IsIndeterminate = true;

            try
            {
                await App.DataManager.Authenticate();
                this.Visibility = Visibility.Visible; //Sichtbar bleiben

                //Schlage dem User vor, Notifications zu verwenden.
                App.DataManager.SetNotificationsEnabled(MessageBox.Show(Localization.Strings.LogInBoxNotificationWarning,
                    Localization.Strings.LogInBoxNotificationWarningTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK);

                //First Login Logik hier: Upload von Tasks wenn gewünscht.
                if (App.DataManager.Tasks.Count > 0 && App.DataManager.User.Score != 0)
                {
                    if (MessageBox.Show(Localization.Strings.LogInBoxMessage, Localization.Strings.LogInBoxMessageTitle,
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        App.DataManager.UploadAllData();
                    }
                    else
                    {
                        //TODO: Lösung für Score Dilemma. Darf eigentlich nicht null sein.
                        App.DataManager.User.Score = 0;
                        App.DataManager.User.AccountCreatedDate = DateTime.Today;
                    }
                }
            }
            catch
            {
                MessageBox.Show(Localization.Strings.MainPageLoginError);
            }


            UpdateVisibility();
            this.IsEnabled = true;
            PerfomanceProgressBar.IsIndeterminate = false;
            PerfomanceProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
