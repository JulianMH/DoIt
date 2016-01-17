using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using DoIt.Data;
using System.Collections.Specialized;
using System.Windows;


namespace DoIt.ViewModels
{
    /// <summary>
    /// ViewModel für die MainPage.
    /// </summary>
    public class MainPageViewModel : NotifyPropertyChangedObject
    {
        /// <summary>
        /// Daten für das ViewModel.
        /// </summary>
        private DataManager dataManager;

        #region Eigenschaften
        /// <summary>
        /// Aktueller Score des Users.
        /// </summary>
        public int UserScore { get { return dataManager.User.Score; } }

        /// <summary>
        /// Alle heutigen unerledigten Aufgaben.
        /// </summary>
        public IEnumerable<TaskPreviewViewModel> TasksToday { get; private set; }

        /// <summary>
        /// Alle heutigen Aufgaben gruppiert nach Datum.
        /// </summary>
        public IEnumerable<PublicGrouping<string, TaskPreviewViewModel>> GroupedTasks { get; private set; }

        /// <summary>
        /// Alle Freunde und der User sortiert nach Highscores und gruppiert nach Platz.
        /// </summary>
        public IEnumerable<PublicGrouping<int, PersonViewModel>> FriendsRanking { get; private set; }

        /// <summary>
        /// Alle Freundschaftsanfragen.
        /// </summary>
        public IEnumerable<PersonViewModel> FriendRequests { get; private set; }

        public Visibility FriendRequestsVisibility { get { return (FriendRequests != null && FriendRequests.Any()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility FriendsNotLoadedVisibility { get { return (dataManager.LogInKnown && (!(FriendsRanking != null && FriendsRanking.Any()))) ?
            Visibility.Visible : Visibility.Collapsed; } }

        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="dataManager">Daten für das ViewModel.</param>
        internal MainPageViewModel(DataManager dataManager)
        {
            this.dataManager = dataManager;

            //foreach (Task task in this.dataManager.Tasks)
            //    task.PropertyChanged += new PropertyChangedEventHandler(task_PropertyChanged);
            DataManagerTasks_CollectionChanged(null, null);

            this.dataManager.TaskAssignedToChanged += DataManagerTasks_CollectionChanged;
            this.dataManager.Tasks.CollectionChanged += DataManagerTasks_CollectionChanged;
            this.dataManager.PropertyChanged += dataManager_PropertyChanged;
            dataManager.User.PropertyChanged += User_PropertyChanged;
        }

        void dataManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "User")
            {
                dataManager.User.PropertyChanged += User_PropertyChanged;
                DataManagerFriends_CollectionChanged(null, null);
                NotifyPropertyChanged("UserScore");
            }
            else if (e.PropertyName == "Tasks")
            {
                dataManager.Tasks.CollectionChanged += DataManagerTasks_CollectionChanged;
                //foreach (Task task in this.dataManager.Tasks)
                //    task.PropertyChanged += new PropertyChangedEventHandler(task_PropertyChanged);
                DataManagerTasks_CollectionChanged(null, null);
            }
            else if (e.PropertyName == "Friends")
                DataManagerFriends_CollectionChanged(null, null);
        }

        void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Score")
            {
                DataManagerFriends_CollectionChanged(null, null);
                NotifyPropertyChanged("UserScore");
            }
        }

        /// <summary>
        /// Weißt einem Datum in der Zukunft eine Textkategorie zu.
        /// </summary>
        /// <param name="date">Das Datum dem eine Kategorie zugewiesen wird.</param>
        /// <returns>Eine lokalisiert Kategorie als String.</returns>
        private string GetDateCategory(DateTime date)
        {
            if (date.Date == DateTime.MinValue)
                return Localization.Strings.CustomDateToTextConverterSoon;
            else if (date.Date == DateTime.MaxValue)
                return Localization.Strings.CustomDateToTextConverterFuture;
            else if(date.Date == DateTime.Now.AddDays(1).Date)
                return Localization.Strings.MainPageTaskGroupTomorrow;
            else if (date.Date == DateTime.Now.Date)
                return Localization.Strings.MainPageTaskGroupToday;
            else if (date.Date < DateTime.Now.Date)
                return Localization.Strings.MainPageTaskGroupOverdue;
            else if (date.Date <= DateTime.Now.AddDays(3).Date)
                return Localization.Strings.MainPageTaskGroupThreeDays;
            else if (date.Date < DateTime.Now.AddDays(7).Date)
                return Localization.Strings.MainPageTaskGroupWeek;
            else if (date.Date < DateTime.Now.AddMonths(1).Date)
                return Localization.Strings.MainPageTaskGroupMonth;
            else
                return Localization.Strings.MainPageTaskGroupLater;
        }

        private void DataManagerTasks_CollectionChanged(object sender, EventArgs e)
        {
            task_PropertyChanged(null, null);
        }

        void task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tasks = this.dataManager.Tasks.Where(p => !this.dataManager.GetTaskIsDone(p)).Select(p => new TaskPreviewViewModel(this.dataManager, p));
            TasksToday = tasks.Where(p => p.DueDate.Date <= DateTime.Today.Date).OrderBy(p => p.DueTimeVisibility).ThenBy(p => p.DueTime.TimeOfDay);
            NotifyPropertyChanged("TasksToday");

            GroupedTasks = tasks.OrderBy(p => p.DueDate).ThenBy(p => p.DueTimeVisibility).ThenBy(p => p.DueTime.TimeOfDay).GroupBy(p => GetDateCategory(p.DueDate))
                    .Select(p => new PublicGrouping<string, TaskPreviewViewModel>(p));
            NotifyPropertyChanged("GroupedTasks");
        }

        private async void DataManagerFriends_CollectionChanged(object sender, EventArgs e)
        {
            if (dataManager.LoggedIn)
            {
                var friendsWithUser = (await dataManager.GetFriends()).Select(p => new PersonViewModel(p)).ToList();
                friendsWithUser.Add(new PersonViewModel(dataManager.User));

                var sortedFriends = friendsWithUser.OrderByDescending(p => p.Score);
                var scoreIndices = sortedFriends.Select(p => p.Score).Distinct().ToList();

                this.FriendsRanking =
                    sortedFriends.GroupBy(p => (scoreIndices.IndexOf(p.Score) + 1))
                    .Select(q => new PublicGrouping<int, PersonViewModel>(q));

                this.FriendRequests = (await dataManager.GetFriendRequests()).Select(p => new PersonViewModel(p)).ToList();
            }
            else
                this.FriendsRanking = null;

            NotifyPropertyChanged("FriendsRanking");
            NotifyPropertyChanged("FriendRequests");
            NotifyPropertyChanged("FriendsNotLoadedVisibility");
            NotifyPropertyChanged("FriendRequestsVisibility");
        }

    }
}