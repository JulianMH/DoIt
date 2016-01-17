using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DoIt.Data;
using DoIt.ViewModels;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace DoIt
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static ApplicationBar taskApplicationBar;
        private static ApplicationBar friendsApplicationBar;

        // Constructor
        public MainPage()
        {
            Diagnostics.ProfilingTakeTime("MainPage() Start");
            InitializeComponent();
            Diagnostics.ProfilingTakeTime("MainPage() Initialize");

            //Animationen erstellen
            new LongListSelectorAnimator(this.Dispatcher, FutureListBox);
            new LongListSelectorAnimator(this.Dispatcher, FriendsListBox);
            Diagnostics.ProfilingTakeTime("MainPage()LongListSelectorAnimator");

            //ViewModel festlegen
            DataContext = new ViewModels.MainPageViewModel(App.DataManager);
            App.DataManager.PropertyChanged += DataManager_PropertyChanged;
            Diagnostics.ProfilingTakeTime("MainPage() ViewModel");

            Loaded += MainPage_Loaded;
        }

        private bool firstLoad = true;
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ApplicationBar == null)  //App Bar laden, aber nur einmal
                SetupAppBar();

            DoSystemTrayStuff();

            if (firstLoad && App.IsAdvertismentVisible)
            {
                firstLoad = false;
                var adControl = AdControlHelper.CreateAdControlSmall("10167810");
                adControl.Margin = new Thickness(6);
                this.FriendsGrid.Children.Add(adControl);
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string friendsString; bool friendsValue;
            //Wenn die App durch eine Freundes Push Benachrichtigung geöffnet wird, soll sie die Freundesseite zuerst zeigen.
            if (this.NavigationContext.QueryString.TryGetValue("friends", out friendsString)
            && bool.TryParse(friendsString, out friendsValue) && friendsValue)
                this.MainPanorama.DefaultItem = this.FriendsPanoramaItem;

            string taskId;
            //Wenn die App durch eine Task Push Benachrichtigung geöffnet wird, soll sie zum Task Navigieren.
            if (this.NavigationContext.QueryString.TryGetValue("taskId", out taskId))
                this.NavigationService.Navigate(new Uri("/TaskPage.xaml?id=" + taskId, UriKind.Relative));

            this.NavigationContext.QueryString.Clear(); //Beim ersten Start der Seite auf Query String achten, sonst nicht.

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Aktualisiert das SystemTray gemäß dem was der DataManager grade macht.
        /// </summary>
        private async void DoSystemTrayStuff()
        {
            SystemTrayHelper.ShowProgress(this, Localization.Strings.MainPageProgressIndicatorLoggingIn, 0.5);
            if (App.DataManager.LogInTask != null)
                await App.DataManager.LogInTask;

            SystemTrayHelper.ShowProgress(this, Localization.Strings.MainPageProgressIndicatorUploading, 0.5);
            if (App.DataManager.SyncUploadTask != null)
                await App.DataManager.SyncUploadTask;

            SystemTrayHelper.ShowProgress(this, Localization.Strings.MainPageProgressIndicatorSyncing, 0.5);
            if (App.DataManager.SyncDownloadTask != null)
                await App.DataManager.SyncDownloadTask;

            SystemTrayHelper.Hide(this);
        }

        #region ApplicationBar
        /// <summary>
        /// Erstellt die ApplicationBar.
        /// </summary>
        private void SetupAppBar()
        {
            taskApplicationBar = new ApplicationBar();
            friendsApplicationBar = new ApplicationBar();
            taskApplicationBar.StateChanged += ApplicationBar_StateChanged;
            friendsApplicationBar.StateChanged += ApplicationBar_StateChanged;

            taskApplicationBar.Opacity = 0.5;
            friendsApplicationBar.Opacity = 0.5;

            friendsApplicationBar.Buttons.Add(ApplicationBarHelpers.CreateButton(Localization.Strings.ApplicationBarFindFriend, "/Icons/AppBar/feature.search.png",
                this.NavigationService, "/SearchFriendsPage.xaml"));

            taskApplicationBar.Buttons.Add(ApplicationBarHelpers.CreateButton(Localization.Strings.ApplicationBarAdd, "/Icons/AppBar/add.png",
                this.NavigationService, "/TaskPage.xaml"));

            var item1 = ApplicationBarHelpers.CreateMenuItem(Localization.Strings.ApplicationBarAbout, this.NavigationService, "/AboutPage.xaml");
            var item2 = ApplicationBarHelpers.CreateMenuItem(Localization.Strings.ApplicationBarFinishedTasks, this.NavigationService, "/FinishedTaskPage.xaml");

            taskApplicationBar.MenuItems.Add(item2);
            taskApplicationBar.MenuItems.Add(item1);
            friendsApplicationBar.MenuItems.Add(item2);
            friendsApplicationBar.MenuItems.Add(item1);

            if (App.IsAdvertismentVisible)
            {
                var item3 = new ApplicationBarMenuItem(Localization.Strings.ApplicationBarRemoveAds);
                item3.Click += item3_Click;
                friendsApplicationBar.MenuItems.Add(item3);
                taskApplicationBar.MenuItems.Add(item3);
            }

            UpdateAppBar();
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            ApplicationBar appBar = (ApplicationBar)sender;

            if (e.IsMenuVisible == true)
                appBar.Opacity = 1;
            else
                appBar.Opacity = 0.5;
        }

        void item3_Click(object sender, EventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.Show();
        }

        private void UpdateAppBar()
        {
            if (MainPanorama.SelectedItem == (FriendsPanoramaItem) && App.DataManager.LoggedIn)
            {
                if (this.ApplicationBar != friendsApplicationBar)
                    this.ApplicationBar = friendsApplicationBar;
            }
            else if (this.ApplicationBar != taskApplicationBar)
                this.ApplicationBar = taskApplicationBar;
        }
        #endregion

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TaskPreviewViewModel task = e.AddedItems[0] as TaskPreviewViewModel;
                PersonViewModel friend = e.AddedItems[0] as PersonViewModel;

                if (task != null)
                    this.NavigationService.Navigate(new Uri("/TaskPage.xaml?id=" + task.GetTaskID(), UriKind.Relative));
                else if (friend != null)
                    this.NavigationService.Navigate(new Uri("/FriendDetailPage.xaml?id=" + friend.GetUserId(), UriKind.Relative));

                TodayListBox.SelectedItem = null;
                FutureListBox.SelectedItem = null;
                FriendsListBox.SelectedItem = null;
            }
        }

        private void MainPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppBar();
        }

        private void DataManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogInKnown")
            {
                UpdateAppBar();
            }
        }

        private void FriendRequestListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox senderListBox = (ListBox)sender;
            PersonViewModel model = senderListBox.SelectedItem as PersonViewModel;
            if (model != null)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(
                    String.Format(Localization.Strings.MainPageAddFriendMessage, model.FullName),
                    Localization.Strings.MainPageAddFriendMessageTitle, MessageBoxButton.OKCancel);

                if (messageBoxResult != MessageBoxResult.None)
                    App.DataManager.AnswerFriendRequest(messageBoxResult == MessageBoxResult.OK, model.GetUserId());

                senderListBox.SelectedItem = null;
            }
        }

    }
}