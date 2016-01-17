using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DoIt.Data;
using DoIt.ViewModels;

namespace DoIt
{
    public partial class FriendsDetailPage : PhoneApplicationPage
    {
        public FriendsDetailPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string userIdString = "";
            if (NavigationContext.QueryString.TryGetValue("id", out userIdString))
            {
                try
                {
                    Person friend = App.DataManager.User;
                    if (userIdString != App.DataManager.User.UserId)
                    {
                        friend = await App.DataManager.GetFriendData(userIdString, true);

                        //Nur in diesem Fall wird die App Bar gebraucht
                        SetUpAppBar();
                    }
                    this.DataContext = new PersonViewModel(friend);
                    return;
                }
                catch
                {
                    MessageBox.Show(Localization.Strings.FriendDetailPageNotFound);
                    this.NavigationService.GoBack();
                }
            }
            else
            {
                MessageBox.Show(Localization.Strings.FriendDetailPageNotFound);
                this.NavigationService.GoBack();
            }
        }

        private void SetUpAppBar()
        {
            this.ApplicationBar = new ApplicationBar();
            var button = new ApplicationBarIconButton(new Uri("/Icons/AppBar/delete.png", UriKind.Relative))
            {
                Text = Localization.Strings.ApplicationBarRemoveFriend
            };

            button.Click += new EventHandler(button_Click);
            this.ApplicationBar.Buttons.Add(button);
        }

        private void button_Click(object sender, EventArgs e)
        {
            var viewModel = ((PersonViewModel)this.DataContext);
            if (MessageBox.Show(String.Format(Localization.Strings.ApplicationBarRemoveFriendMessage, viewModel.FullName),
                Localization.Strings.ApplicationBarRemoveFriendMessageTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.NavigationService.GoBack();
                App.DataManager.RemoveFriend(viewModel.GetUserId());
            }
        }
    }
}