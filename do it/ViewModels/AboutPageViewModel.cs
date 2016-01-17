using System;
using System.Collections.Generic;
using DoIt.Data;
using System.Windows;

namespace DoIt.ViewModels
{
    /// <summary>
    /// ViewModel für die Einstellungen auf der AboutPage
    /// </summary>
    public class AboutPageViewModel : NotifyPropertyChangedObject
    {
        /// <summary>
        /// Daten für das ViewModel.
        /// </summary>
        private DataManager dataManager;

        public Visibility SettingsVisibility { get { return dataManager.LogInKnown ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility NameNotEditableVisibility { get { return dataManager.LoggedIn ? Visibility.Collapsed : Visibility.Visible; } }
        public bool IsNameEditable { get { return dataManager.LoggedIn; } }

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public bool PushNotficationsEnabled { get; set; }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="dataManager">Der DataManager, mit dessen Daten das View Model befüllt wird.</param>
        internal AboutPageViewModel(DataManager dataManager)
        {
            this.dataManager = dataManager;

            this.UserFirstName = this.dataManager.User.FirstName;
            this.UserLastName = this.dataManager.User.Name;
            this.PushNotficationsEnabled = this.dataManager.GetNotificationsEnabled();
        }

        /// <summary>
        /// Wendet die Änderungen an den Einstellungen an.
        /// </summary>
        internal async void Apply()
        {
            //Vornamen auf Länge überprüfen und gegebenfalls übernehmen.
            if (this.UserFirstName.Length >= 1 && this.UserFirstName.Length < 20)
            {
                this.dataManager.User.FirstName = this.UserFirstName;
                this.dataManager.User.Name = this.UserLastName;

                try
                {
                    await this.dataManager.ApplyUser();
                }
                catch
                {
                    //Fehler hier einfach ignorieren :D :D
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                }
            }
            else
                MessageBox.Show(String.Format(Localization.Strings.AboutPageNameWarning, this.UserFirstName),
                    Localization.Strings.AboutPageNameWarningTitle, MessageBoxButton.OK);

            this.dataManager.SetNotificationsEnabled(this.PushNotficationsEnabled);
        }
    }
}
