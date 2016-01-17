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
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt
{
    public partial class SearchFriendsPage : PhoneApplicationPage
    {
        public SearchFriendsPage()
        {
            //Hier kein MVVM weil zu kompliziert für den kleinkram.
            InitializeComponent();
        }

        private static bool isSearching = false;
        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSearching)
            {
                isSearching = true;

                SystemTray.SetProgressIndicator(this, new ProgressIndicator()
                {
                    IsIndeterminate = true,
                    IsVisible = true,
                    Text = Localization.Strings.SearchFriendsPageProgressIndicatorSearching
                });

                string currentSearchText = "";
                while (SearchTextBox.Text != currentSearchText)
                {
                    currentSearchText = SearchTextBox.Text;

                    if (SearchTextBox.Text != null && SearchTextBox.Text != "")
                    {
                        this.FriendsListBox.ItemsSource = (await App.DataManager.SearchPersons(SearchTextBox.Text))
                             .Select(p => new PersonViewModel(p));
                    }
                }

                SystemTray.SetProgressIndicator(this, null);
                isSearching = false;
            }
        }

        private async void FriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsListBox.SelectedItem != null)
            {
                PersonViewModel item = (PersonViewModel)FriendsListBox.SelectedItem;
                if (MessageBox.Show(
                    String.Format(Localization.Strings.SearchFriendsPageAddMessage, item.FullName),
                    Localization.Strings.SearchFriendsPageAddMessageTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    await App.DataManager.AddFriend(item.GetUserId());
                    MessageBox.Show(Localization.Strings.SearchFriendsPageSuccessMessage);
                }

                FriendsListBox.SelectedItem = null;
            }
        }
    }
}