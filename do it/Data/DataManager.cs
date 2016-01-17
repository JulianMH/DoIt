using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DoIt.Data.Queries;
using System.IO;
using Microsoft.WindowsAzure.MobileServices;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Notification;
using System.Net;
using System.Net.Http;

namespace DoIt.Data
{
    internal partial class DataManager : NotifyPropertyChangedObject
    {
        #region Variablen
        /// <summary>
        /// Aktueller Datensatz.
        /// </summary>
        private Data data;

        /// <summary>
        /// Aktueller MobileServiceUser.
        /// </summary>
        private MobileServiceUser user;

        /// <summary>
        /// Notification Channel für die Push Benachrichtigungen.
        /// </summary>
        private HttpNotificationChannel notificationChannel;

        /// <summary>
        /// Client zur Kommunikation mit dem Windows Azure Server.
        /// </summary>
        private MobileServiceClient mobileService = new MobileServiceClient("https://doit-philbi-de.azure-mobile.net/", APIKeys.AzureAPIKey);

        internal System.Threading.Tasks.Task LogInTask { get; private set; }
        internal System.Threading.Tasks.Task SyncUploadTask { get; private set; }
        internal System.Threading.Tasks.Task SyncDownloadTask { get; private set; }
        #endregion

        #region Eigenschaften
        /// <summary>
        /// Der aktuelle Nutzer der App
        /// </summary>
        internal Person User { get { return data.User; } }

        /// <summary>
        /// Liste aller nicht erledigten Tasks des Nutzers.
        /// </summary>
        internal ObservableCollection<Task> Tasks { get { return data.Tasks; } }

        /// <summary>
        /// Gibt an, ob die App an einen Online Account gebunden ist.
        /// </summary>
        internal bool LogInKnown { get { return data.LogInKnown; } }

        /// <summary>
        /// Gibt an, ob die App gerade im Moment online ist.
        /// </summary>
        internal bool LoggedIn { get { return user != null; } }

        /// <summary>
        /// Daten zum Testen und Design.
        /// </summary>
        internal static DataManager SampleData
        {
            get
            {
                Person user = new Person(6, "MicrosoftAccount:8c0c7b2c1608d99d2b8eeba2628e84e7", "10e3df3749fcae2c", "Mustermann", "Julian", "", "", 2340, App.VersionNumber, DateTime.Today);

                DataManager manager = new DataManager(new Data()
                {
                    User = user,
                });


                //Person f1 = new Person(1, "a", "a", "Mustermann", "ATest1", "", 1000);
                //Person f2 = new Person(2, "b", "b", "Mustermann", "BTestTest2", "", 1200);
                //Person f3 = new Person(3, "c", "c", "Mustermann", "CTestTestTest3", "", 1200);
                //Person f4 = new Person(4, "d", "d", "Mustermann", "DTest4", "", 1000);
                //Person f5 = new Person(5, "e", "e", "Mustermann", "DTest5", "", 1000);

                //manager.data.Friends.Add(f1);
                //manager.data.Friends.Add(f2);
                //manager.data.Friends.Add(f3);
                //manager.data.Friends.Add(f4);
                //manager.data.Friends.Add(f5);

                //manager.data.Tasks.Add(new Task(-1, Guid.NewGuid().ToString(), "test", user.UserId, DateTime.Today, DateTime.Today.AddDays(1), false, DateTime.Now, false));
                //manager.data.Tasks.Add(new Task(-2, Guid.NewGuid().ToString(), "German Test", user.UserId, DateTime.Today, DateTime.Today, false, DateTime.Now, false));
                //manager.data.Tasks.Add(new Task(-2, Guid.NewGuid().ToString(), "Do this", user.UserId,  DateTime.Today, DateTime.Today, false, DateTime.Now, true));
                //manager.data.Tasks.Add(new Task(-2, Guid.NewGuid().ToString(), "123s", user.UserId, DateTime.Today, DateTime.Today, false, DateTime.Now, true));
                //manager.data.Tasks.Add(new Task(-3, Guid.NewGuid().ToString(), "Homework", user.UserId, DateTime.Today, DateTime.Today, false, DateTime.Now, false));
                //manager.data.Tasks.Add(new Task(-4, Guid.NewGuid().ToString(), "Dinner", user.UserId, DateTime.Today, DateTime.Today, true, DateTime.Today, false));
                //new Friend[] { f1, f2, f3 }.Select(p => p.Id).ToArray()));

                return manager;
            }
        }
        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="user">Der User der App.</param>
        private DataManager(Data data)
        {
            this.data = data;
            this.data.TaskAssignedToRelationship.CollectionChanged += TaskAssignedToRelationship_CollectionChanged;
            this.data.PropertyChanged += data_PropertyChanged;
            foreach (TaskAssignedTo item in this.data.TaskAssignedToRelationship)
                item.PropertyChanged += OnTaskAssignedToChanged;

            this.LogInTask = this.AuthenticateSilent();
        }

        #region Save/Load/Create
        private static string fileName = "data.xml";
        internal static DataManager TryLoad()
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (file.FileExists(fileName))
                    {
                        using (var stream = file.OpenFile(fileName, FileMode.Open))
                            return new DataManager(Data.Deserialize(stream));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't load file.", e);
            }
        }

        internal static DataManager CreateNew()
        {
            Data data = new Data();

            //Ein paar Anweisungen für die App erstellen.
            var task1 = new Task(0, Guid.NewGuid().ToString(), Localization.Strings.DataNewCreatedTask1, data.User.UserId, DateTime.Now - TimeSpan.FromDays(1),
                DateTime.Today - TimeSpan.FromDays(1), false, DateTime.Now);
            var task2 = new Task(0, Guid.NewGuid().ToString(),
                Localization.Strings.DataNewCreatedTask2,
                data.User.UserId, DateTime.Now,
                DateTime.Today, false, DateTime.Now);
            var task3 = new Task(0, Guid.NewGuid().ToString(),
                Localization.Strings.DataNewCreatedTask3,
                data.User.UserId, DateTime.Now,
                DateTime.Today.AddDays(1), false, DateTime.Now);

            data.TaskAssignedToRelationship.Add(new TaskAssignedTo() { Score = 200, IsDone = false, PersonUserId = data.User.UserId, TaskId = task1.TaskId });
            data.TaskAssignedToRelationship.Add(new TaskAssignedTo() { Score = 100, IsDone = false, PersonUserId = data.User.UserId, TaskId = task2.TaskId });
            data.TaskAssignedToRelationship.Add(new TaskAssignedTo() { Score = 50, IsDone = false, PersonUserId = data.User.UserId, TaskId = task3.TaskId });
            data.Tasks.Add(task1);
            data.Tasks.Add(task2);
            data.Tasks.Add(task3);

            return new DataManager(data);
        }

        /// <summary>
        /// Speichert die Offline Mode-Daten in das Isolated Storage.
        /// </summary>
        internal void Save()
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = file.OpenFile(fileName, FileMode.Create))
                    {
                        data.Serialize(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't save file.", e);
            }
        }
        #endregion

        #region Authentication
        internal async System.Threading.Tasks.Task Authenticate()
        {
            if (Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
                Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None)
            {
                if (this.user == null)
                {
                    this.user = await LiveAuthenticator.Authenticate(this.mobileService);

                    if (this.user != null)
                    {
                        await LogInSuccessful();
                    }
                }
            }

            this.StartSyncing();
        }

        private async System.Threading.Tasks.Task AuthenticateSilent()
        {
            if (this.LogInKnown &&
                Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
                Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None && this.user == null)
            {
                //LogIn bekannt aber Fehlschlag braucht keine Reaktion, dann geht die App einfach in den Offline Modus.
                this.user = await LiveAuthenticator.AuthenticateSilent(this.mobileService);
            }

            if (this.user != null)
                await LogInSuccessful();

            this.StartSyncing();
        }

        private async System.Threading.Tasks.Task LogInSuccessful()
        {
            Diagnostics.ProfilingTakeTime("LogInSuccessful Start");

            //Userdaten checken.
            var personTable = this.mobileService.GetTable<Person>();
            var results = await personTable.Where(p => p.UserId == this.user.UserId).ToEnumerableAsync();

            if (results.Any())
            {
                //Online Userinformationen weitgehend übernehmen.
                var oldUser = this.data.User;
                this.data.User = results.First();

                //Übernehme immer den höheren Score :D
                if (oldUser.Score > this.data.User.Score)
                    this.data.User.Score = oldUser.Score;
            }
            else
            {
                //FirstLogin, User unbekannt oder gelöscht.
                //Nicht weiter drum kümmern ob bestehende Daten hochgeladen werden oder nicht.
                //Notifikation Channel steht.
                this.data.User = await LiveAuthenticator.GetNewUserData(this.user.UserId, "", this.User.Score, this.User.AccountCreatedDate);
                await personTable.InsertAsync(this.data.User);
            }
            Diagnostics.ProfilingTakeTime("LogInSuccessful User Update");

            //Fertig eingeloggt
            data.LogInKnown = true;
            NotifyPropertyChanged("LogInKnown");
            NotifyPropertyChanged("LoggedIn");
            NotifyPropertyChanged("User");

            //Push Notification Channel
            AcquirePushChannelAsync();
            Diagnostics.ProfilingTakeTime("LogInSuccessful Ended");
        }

        internal async System.Threading.Tasks.Task ApplyUser()
        {
            var personTable = this.mobileService.GetTable<Person>();

            if (this.notificationChannel != null)
                this.data.User.PushChannel = this.notificationChannel.ChannelUri.ToString();
            else
                this.data.User.PushChannel = "";

            this.data.User.NotificationCount = 0;

            this.data.User.Culture = System.Globalization.CultureInfo.CurrentCulture.ToString();
            this.data.User.WPAppVersion = App.VersionNumber;
            await personTable.UpdateAsync(this.data.User);
        }

        internal void DisconnectAccount()
        {
            mobileService.Logout();
            LiveAuthenticator.Logout();
            this.user = null;
            this.data.LogInKnown = false;
            this.data.User.AuthenticationUserId = "";
            this.data.User.Id = 0;
            this.data.User.UserId = "";
            this.data.Queries.Clear();
            this.data.FriendsCache = new ObservableCollection<Person>();
            AcquirePushChannelAsync();

            foreach (var taskAssignedTo in this.data.TaskAssignedToRelationship)
                taskAssignedTo.PersonUserId = this.data.User.UserId;
            foreach (var task in this.data.Tasks)
                task.CreatorUserId = this.data.User.UserId;
            NotifyPropertyChanged("LogInKnown");
            NotifyPropertyChanged("Friends");

            this.Save();
        }

        internal void UploadAllData()
        {
            //Sicher ist sicher.
            this.data.Queries.Clear();

            //UserId Anpassen.
            foreach (var taskAssignedTo in this.data.TaskAssignedToRelationship)
            {
                taskAssignedTo.PersonUserId = this.data.User.UserId;
                taskAssignedTo.Id = 0;
            }

            foreach (Task task in this.Tasks)
            {
                //Clear voher kann man sich eigentlich sparen, passiert eh wenn die Queries applied werden.
                this.data.Queries.Add(new CreateTaskQuery() { TaskId = task.TaskId });

                int score = 100;
                var taskAssignedTo = this.data.TaskAssignedToRelationship.Where(p => p.TaskId == task.TaskId).FirstOrDefault();

                if (taskAssignedTo != null)
                    score = taskAssignedTo.Score;
                else if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                this.data.Queries.Add(new ModifyTaskQuery()
                {
                    TaskId = task.TaskId,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    IsDueTimeSet = task.IsDueTimeSet,
                    DueTime = task.DueTime,
                    Score = score
                });

                if (taskAssignedTo != null && taskAssignedTo.IsDone)
                    this.data.Queries.Add(new TaskDoneQuery() { TaskId = task.TaskId });

                //UserId anpassen.
                task.CreatorUserId = this.data.User.UserId;
                task.Id = 0;
            }

        }
        #endregion

        #region ToastNotifications
        internal bool GetNotificationsEnabled()
        {
            return data.NotificationsEnabled;
        }

        internal void SetNotificationsEnabled(bool value)
        {
            this.data.NotificationsEnabled = value;
            this.AcquirePushChannelAsync();
        }

        private System.Threading.Tasks.TaskScheduler mainTaskScheduler;
        private void AcquirePushChannelAsync()
        {
            mainTaskScheduler = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();

            //Im Hintergrund ausführen ist geschickter.
            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    string pushChannelName = "DoItPushChannel";

                    this.notificationChannel = HttpNotificationChannel.Find(pushChannelName);

                    if (this.data.NotificationsEnabled)
                    {
                        if (this.notificationChannel == null)
                        {
                            this.notificationChannel = new HttpNotificationChannel(pushChannelName);
                            this.notificationChannel.Open();
                            this.notificationChannel.BindToShellToast();
                            this.notificationChannel.BindToShellTile();
                        }

                        this.notificationChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                        this.notificationChannel.ShellToastNotificationReceived += notificationChannel_ShellToastNotificationReceived;
                    }
                    else if (this.notificationChannel != null)
                    {
                        this.notificationChannel.Close();
                        this.notificationChannel = null;
                    }

                    //Jetzt die Zahl auf dem Live Tile resetten.
                    foreach (var tile in Microsoft.Phone.Shell.ShellTile.ActiveTiles)
                    {
#if WP8
                        tile.Update(new Microsoft.Phone.Shell.FlipTileData()
                        {
                            Count = 0,
                            Title = Localization.Strings.ApplicationTitle,
                            SmallBackgroundImage = new Uri("Background.png", UriKind.Relative),
                            BackgroundImage = new Uri("Background.png", UriKind.Relative)
                        });
#else
                        tile.Update(new Microsoft.Phone.Shell.StandardTileData()
                        {
                            Count = 0,
                            Title = Localization.Strings.ApplicationTitle,
                            BackgroundImage = new Uri("Background.png", UriKind.Relative)
                        });
#endif
                    }
                }
                catch
                {
                    //Sind ja nur Pushbenachrichtigungen. Kann man auch silent ignorieren.
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                }
            });
            task.ContinueWith<System.Threading.Tasks.Task>(async (System.Threading.Tasks.Task x) => { await ApplyUser(); },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskContinuationOptions.None,
                mainTaskScheduler);
        }

        void notificationChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            //Toast notification erhalten, einfach schnell aktualisieren.
            //Sicher gehen dass das im Hauptthread passiert.
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.StartSyncing();
            }, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, mainTaskScheduler);
        }

        private void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
        #endregion

        #region Friends
        bool friendsUpdated = false;
        internal async System.Threading.Tasks.Task<ReadOnlyObservableCollection<Person>> GetFriends()
        {
            if (!friendsUpdated && this.LoggedIn) //Wenn Cache nicht aktuell, dann aktualisiere Cache.
            {
                try
                {
                    //Daten vom Server laden.
                    var friendData = await this.mobileService.GetTable<PersonHasFriend>()
                        .Where(p => p.IsConfirmed && p.PersonId == User.UserId).ToEnumerableAsync();

                    var personTable = this.mobileService.GetTable<Person>();
                    var friends = new ObservableCollection<Person>();
                    //TODO: Das hier effizienter!
                    foreach (PersonHasFriend personHasFriend in friendData)
                    {
                        friends.Add((await personTable.Where(p => p.UserId == personHasFriend.FriendId)
                            .ToEnumerableAsync()).First());
                    }

                    friendsUpdated = true;
                    this.data.FriendsCache = friends;
                }
                catch
                {
                    if (System.Diagnostics.Debugger.IsAttached) //Freunde konnten nicht geladen werden, kein Grund für eine Fehlermeldung.
                        System.Diagnostics.Debugger.Break();
                }
            }
            return new ReadOnlyObservableCollection<Person>(this.data.FriendsCache);
        }

        internal async System.Threading.Tasks.Task AddFriend(string friendUserId)
        {
            await this.mobileService.GetTable<PersonHasFriend>().InsertAsync(new PersonHasFriend() { PersonId = User.UserId, FriendId = friendUserId });
            this.friendsUpdated = false;
        }

        internal async void AnswerFriendRequest(bool isAccepted, string friendUserId)
        {
            if (isAccepted)
            {
                await this.AddFriend(friendUserId);
                NotifyPropertyChanged("Friends");
            }
            else
            {
                var table = this.mobileService.GetTable<PersonHasFriend>();
                var requests = await table
                    .Where(p => p.PersonId == friendUserId && p.FriendId == User.UserId).ToEnumerableAsync();

                foreach (var request in requests)
                    await table.DeleteAsync(request);

                this.friendsUpdated = false;
            }
        }

        internal async System.Threading.Tasks.Task<Person> GetFriendData(string userId, bool lookOnline)
        {
            try
            {
                if (userId == this.User.UserId)
                    return this.User;

                Person person = null;

                if (this.data.FriendsCache != null) //Sind die Daten offline da?
                    person = this.data.FriendsCache.FirstOrDefault(p => p.UserId == userId);

                if (person == null && lookOnline)
                    person = (await this.mobileService.GetTable<Person>().Where(p => p.UserId == userId).ToEnumerableAsync()).FirstOrDefault();

                if (person != null)
                    return person;
            }
            catch
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }

            //Unbekannte Person zurückliefern
            return new Person(0, "", "", Localization.Strings.PersonUnknownLastName, Localization.Strings.PersonUnknownFirstName, "", "", 0, App.VersionNumber, DateTime.Now);
        }

        #region TaskAssignedTo
        internal async void AddTaskAssignedTo(TaskAssignedTo item)
        {
            await this.mobileService.GetTable<TaskAssignedTo>().InsertAsync(item);
        }

        internal async void UpdateTaskAssignedTo(TaskAssignedTo item)
        {
            await this.mobileService.GetTable<TaskAssignedTo>().UpdateAsync(item);
        }

        internal async void RemoveTaskAssignedTo(TaskAssignedTo item)
        {
            await this.mobileService.GetTable<TaskAssignedTo>().DeleteAsync(item);
        }

        internal async System.Threading.Tasks.Task<IEnumerable<TaskAssignedTo>> GetTaskAssignedTo(string taskId)
        {
            var table = this.mobileService.GetTable<TaskAssignedTo>();
            return await table.Where(p => p.TaskId == taskId).ToEnumerableAsync();
        }
        #endregion

        internal async void RemoveFriend(string friendUserId)
        {
            var table = this.mobileService.GetTable<PersonHasFriend>();
            var personHasFriends = await table.Where(p => p.PersonId == user.UserId && p.FriendId == friendUserId).ToEnumerableAsync();

            foreach (var personHasFriend in personHasFriends)
                await table.DeleteAsync(personHasFriend);

            //Offlinedaten aktualisieren.
            var friend = this.data.FriendsCache.FirstOrDefault(p => p.UserId == friendUserId);
            if (friend != null)
                this.data.FriendsCache.Remove(friend);

            NotifyPropertyChanged("Friends");
        }

        internal async System.Threading.Tasks.Task<IEnumerable<Person>> GetFriendRequests()
        {
            var friendRequestsData = await this.mobileService.GetTable<PersonHasFriend>()
                .Where(p => !p.IsConfirmed && p.FriendId == User.UserId).ToEnumerableAsync();

            var personTable = this.mobileService.GetTable<Person>();
            var friendRequests = new List<Person>();

            //Recht ineffizient, aber wird schon passen, meist gibt es ja nur wenige Freundesanfragen.
            foreach (PersonHasFriend personHasFriend in friendRequestsData)
            {
                friendRequests.Add((await personTable.Where(p => p.UserId == personHasFriend.PersonId)
                    .ToEnumerableAsync()).First());
            }

            return friendRequests;
        }

        internal async System.Threading.Tasks.Task<IEnumerable<Person>> SearchPersons(string searchFor)
        {
            var personTable = this.mobileService.GetTable<Person>();

            //TODO: Das muss hoffentlich nicht der Client evaluaten, wobei selbst wenn :D
            var peopleFromServer = await personTable.Where(p => (p.FirstName + p.Name).Contains(searchFor))
                .OrderBy(p => p.Name).ThenBy(p => p.FirstName).ToEnumerableAsync();

            //Nur noch User und Freunde des Users aussortieren.
            return peopleFromServer.Where(p => p.UserId != App.DataManager.User.UserId && !data.FriendsCache.Any(q => q.UserId == p.UserId));
        }
        #endregion

        #region Tasks
        internal bool GetTaskIsDone(Task task)
        {
            int score;
            return GetTaskIsDone(task, out score);
        }

        internal bool GetTaskIsDone(Task task, out int score)
        {
            var taskAssignedTo = this.data.TaskAssignedToRelationship.FirstOrDefault(p => p.TaskId == task.TaskId && p.PersonUserId == this.data.User.UserId);

            if (taskAssignedTo == null)
            {
                //Fehlerfall Silent lösen, sollte eigentlich nie vorkommen, außer wenn eine einem Nutzer zugewiesene Aufgabe sich verändert und
                //die App über Push gestartet wird.
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                taskAssignedTo = new TaskAssignedTo() { TaskId = task.TaskId, IsDone = false, PersonUserId = this.data.User.UserId, Score = 200 };
            }

            score = taskAssignedTo.Score;
            return taskAssignedTo.IsDone;
        }

        internal async System.Threading.Tasks.Task<Task> GetTask(string taskId)
        {
            //Versuche Daten offline zu finden.
            var task = this.data.Tasks.FirstOrDefault(p => p.TaskId == taskId);

            if (task == null && this.LoggedIn) //Im Notfall Daten vom Server holen.
            {
                try
                {
                    var tasksFromServer = await mobileService.GetTable<Task>().Where(p => p.TaskId == taskId).ToEnumerableAsync();
                    return tasksFromServer.FirstOrDefault();

                }
                catch
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                }
            }

            return task; //Fehlerfall: Der Task kann null sein.
        }

        #region TaskAssignedToChangedEvent
        internal event EventHandler TaskAssignedToChanged;

        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TaskAssignedToRelationship")
                foreach (TaskAssignedTo item in this.data.TaskAssignedToRelationship)
                    item.PropertyChanged += OnTaskAssignedToChanged;
        }

        void TaskAssignedToRelationship_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (TaskAssignedTo item in e.NewItems)
                item.PropertyChanged += OnTaskAssignedToChanged;
            OnTaskAssignedToChanged(null, null);
        }

        void OnTaskAssignedToChanged(object sender, PropertyChangedEventArgs e)
        {
            if (TaskAssignedToChanged != null)
                TaskAssignedToChanged(this, new EventArgs());
        }
        #endregion
        #endregion

        #region Queries
        /// <summary>
        /// Fügt eine Anfrage auf Datenänderung in die Warteschlange ein.
        /// Die lokalen Datenänderungen werden sofort vorgenommen, 
        /// sobald wieder Verbindung besteht werden die Queries an den Server gesendet.
        /// </summary>
        /// <param name="query">Die entsprechende Anfrage.</param>
        internal void AddQuery(Query query)
        {
            query.Apply(data.Tasks, data.TaskAssignedToRelationship, this.User);
            if (data.LogInKnown)
                this.data.Queries.Add(query);
            StartSyncing();
        }

        private void StartSyncing()
        {
            SyncUploadTask = SyncUploadAsync();
        }

        private async System.Threading.Tasks.Task SyncUploadAsync()
        {
            if (SyncUploadTask == null || SyncUploadTask.IsCanceled
                || SyncUploadTask.IsCompleted || SyncUploadTask.IsFaulted) //Nur ausführen wenn grade nicht Aktiv
            {
                //Exceptions werden nicht behandelt, sie sollen weitergegeben werden.
                if (this.LoggedIn && (Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
                    Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None))
                {
                    var tasksTable = this.mobileService.GetTable<Task>();
                    var taskAssignedToTable = this.mobileService.GetTable<TaskAssignedTo>();

                    while (this.data.Queries.Any())
                    {
                        var current = this.data.Queries.First();

                        try
                        {
                            await current.Data.Send(data.Tasks, tasksTable, data.TaskAssignedToRelationship, taskAssignedToTable, this.User);
                            this.data.Queries.Remove(current);
                        }
                        catch (MobileServiceInvalidOperationException)
                        {
                            //Bei dem Fehler ist nicht viel zu machen. Einfach silent ignorieren und nicht nochmal versuchen.
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                            this.data.Queries.Remove(current);
                        }
                        catch (HttpRequestException)
                        {
                            //Bei dem Fehler ist nicht viel zu machen. Einfach silent ignorieren.
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                            return;
                        }
                        catch (WebException)
                        {
                            //Bei dem Fehler ist nicht viel zu machen. Einfach silent ignorieren.
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                            return;
                        }
                        catch (Exception e)
                        {
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();

                            //Einfach nichts tun und später wieder versuchen.
                            //Die Queries versuchen normalerweise keine Exceptions auszulösen, also ist dashier vermutlich ein Verbindungsproblem.
                            throw new Exception("Uploading Queries Failed", e);
                        }
                    }

                    Diagnostics.ProfilingTakeTime("DataManager.UploadQueries ApplyQueries");
                    SyncDownloadTask = this.SyncDownloadAsync(); //Starten die Daten zu aktualisieren.
                    Diagnostics.ProfilingTakeTime("DataManager.UploadQueries Save");
                }
            }
        }

        private async System.Threading.Tasks.Task SyncDownloadAsync()
        {
            if (SyncDownloadTask == null || SyncDownloadTask.IsCanceled || SyncDownloadTask.IsCompleted || SyncDownloadTask.IsFaulted)
            {
                var queriesBeforeDownload = this.data.Queries.ToArray();
                var tasksTable = this.mobileService.GetTable<Task>();

                //TODO: Das hier effizienter machen.
                var taskAssignedToRelationship = new ObservableCollection<TaskAssignedTo>(await mobileService.GetTable<TaskAssignedTo>()
                    .Where(p => p.PersonUserId == user.UserId).ToEnumerableAsync());
                var tasks = new ObservableCollection<Task>(await tasksTable.Where(p => p.CreatorUserId == user.UserId).ToListAsync());

                foreach (var x in taskAssignedToRelationship)
                {
                    //Wenn Task-Daten nicht bekannt, dann lade sie vom Server.
                    if (!tasks.Any(p => p.TaskId == x.TaskId))
                    {
                        var tasksFound = await tasksTable.Where(p => p.TaskId == x.TaskId).ToEnumerableAsync();
                        foreach (var task in tasksFound)
                            tasks.Add(task);
                    }
                }

                foreach (Query query in queriesBeforeDownload) //Wenn Queries vorhanden sind diese sicherheitshalber anwenden.
                    query.Apply(tasks, taskAssignedToRelationship, this.data.User);

                this.data.TaskAssignedToRelationship = taskAssignedToRelationship;
                this.data.Tasks = tasks;
                NotifyPropertyChanged("Tasks");
                this.Save();
            }
        }
        #endregion
    }
}
