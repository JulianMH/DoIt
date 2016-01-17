using System;
using System.Linq;
using DoIt.Data;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using DoIt.Data.Queries;
using DoIt.Controls;
using Microsoft.Phone.Scheduler;

namespace DoIt.ViewModels
{
    public class TaskPageViewModel : NotifyPropertyChangedObject
    {
        private DataManager dataManager;
        private string taskId;
        private bool isFromUser;

        #region Darstellungseigenschaften
        public bool IsDoneChangeable { get; private set; }

        /// <summary>
        /// Gibt an, ob die Werte bearbeitet werden können.
        /// </summary>
        public bool IsEditable { get { return isFromUser && !IsDone; } }

        /// <summary>
        /// Gibt an, ob die DueTime grade bearbeitbar ist.
        /// </summary>
        public bool IsDueTimeEditable { get { return IsDueTimeSet && isFromUser && !IsDone; } }

        private bool isFirstEdit = false;
        public Visibility NotUserCreatedVisibility { get { return isFromUser ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility AssigningFriendsPossibleVisibility { get { return (this.IsEditable) ? Visibility.Visible : Visibility.Collapsed; } }
        public string FriendsListEmptyString
        {
            get
            {
                if (dataManager.LogInKnown && !(FriendsTaskAssignedToAlphabetical != null && FriendsTaskAssignedToAlphabetical.Any()))
                {
                    if (this.isFromUser)
                        return Localization.Strings.TaskPageFriendsEmtpyString;
                    else
                        return Localization.Strings.TaskPageFriendsEmtpyStringNotUserCreated;
                }
                else
                    return "";
            }
        }

        public Visibility NotFirstEditVisibility { get { return isFirstEdit ? Visibility.Collapsed : Visibility.Visible; } }
        public string TitleText { get { return isFirstEdit ? Localization.Strings.TaskPageHeaderAdd : Localization.Strings.TaskPageHeaderEdit; } }
        public PersonViewModel Creator { get; private set; }
        #endregion

        #region Task Eigenschaften
        /// <summary>
        /// Wert der Aufgabe, sobald sie erfüllt wurde.
        /// </summary>
        public int Score
        {
            get
            {
                return score;
            }
            private set
            {
                this.score = value;
                NotifyPropertyChanged("Score");
            }
        }
        private int score;
        private int oldScore;

        /// <summary>
        /// Beschrebung der Aufgabe.
        /// </summary>
        public string Description { get { return this.description; } set { this.description = value; NotifyPropertyChanged("Description"); } }
        private string description;

        public bool IsCustomDueDate { get { return this.DueDate != DateTime.MinValue && this.DueDate != DateTime.MaxValue; } }
        public IEnumerable<DateTime> PossiblePickerDates
        {
            get
            {
                var customDate = this.DueDate;
                if (!IsCustomDueDate)
                    customDate = DateTime.Today.AddDays(1);
                return new DateTime[] { DateTime.MinValue, customDate, DateTime.MaxValue };
            }
        }

        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public DateTime DueDate
        {
            get { return dueDate; }
            set
            {
                if (!isFirstEdit)
                {
                    //Score abhängig vom verschieben verändern.
                    if (value > oldDueDate)
                    {
                        int newScore = (int)(0.8f * oldScore);

                        //50% Abzug für zu spät Deadline bewegt.
                        if (oldDueDate.Date < DateTime.Today.Date)
                            newScore /= 2;

                        this.Score = Math.Max(newScore, 20);
                    }
                    else
                        this.Score = oldScore;
                }

                dueDate = value;
                NotifyPropertyChanged("DueDate");
                NotifyPropertyChanged("PossiblePickerDates");
                NotifyPropertyChanged("IsCustomDueDate");
            }
        }
        private DateTime dueDate;
        private DateTime oldDueDate;

        /// <summary>
        /// Ist Uhrzeit zum Durchführen der Aufgabe festgelegt.
        /// </summary>
        public bool IsDueTimeSet { get { return isDueTimeSet; } set { isDueTimeSet = value; NotifyPropertyChanged("IsDueTimeSet"); NotifyPropertyChanged("IsDueTimeEditable"); } }
        private bool isDueTimeSet;

        /// <summary>
        /// Uhrzeit zum Durchführen der Aufgabe.
        /// </summary>
        public DateTime DueTime { get; set; }

        private bool isDone = false;
        public bool IsDone
        {
            get { return isDone; }
            set
            {
                this.isDone = value;
                NotifyPropertyChanged("IsDone");
                NotifyPropertyChanged("IsEditable");
                NotifyPropertyChanged("IsDueTimeEditable");
            }
        }
        #endregion

        #region TaskAssignedTo Eigenschaften
        /// <summary>
        /// Personen, die eine Aufgabe durchführen sollen.
        /// </summary>
        public IEnumerable<Controls.FriendsByLetterGroup<FriendTaskAssignedToViewModel>> FriendsTaskAssignedToAlphabetical { get; set; }

        private IEnumerable<TaskAssignedTo> oldTaskAssignedTo;
        #endregion

        #region Reminder Eigenschaften
        /// <summary>
        /// Gibt an, ob eine Erinnerung für diese Aufgabe festgelegt ist.
        /// </summary>
        public bool IsReminderSet
        {
            get { return this.isReminderSet; }
            set
            {
                isReminderSet = value;
                if (value)
                {
                    this.ReminderDateTime = this.DueDate.Date;
                    if (!IsCustomDueDate)
                        this.ReminderDateTime = DateTime.Today.AddDays(1);
                    if (this.IsDueTimeSet)
                        this.ReminderDateTime += DueTime.TimeOfDay.Subtract(new TimeSpan(0, 15, 0));
                    else
                        this.ReminderDateTime += new TimeSpan(12, 0, 0);
                    NotifyPropertyChanged("ReminderDateTime");
                }
                NotifyPropertyChanged("IsReminderSet");
            }
        }
        private bool isReminderSet;

        /// <summary>
        /// Zeitpunkt der Erinnerung.
        /// </summary>
        public DateTime ReminderDateTime { get; set; }
        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="dataManager">Daten für das ViewModel.</param>
        /// <param name="task">Aktueller Task für das ViewModel.</param>
        internal TaskPageViewModel(DataManager dataManager, Task task)
        {
            this.dataManager = dataManager;
            this.taskId = task.TaskId;
            this.isFromUser = task.IsFromUser(dataManager.User);

            //IsDone und Score
            this.IsDone = this.dataManager.GetTaskIsDone(task, out this.oldScore);
            this.IsDoneChangeable = !this.IsDone;
            this.Score = this.oldScore;

            //Sonstige Eigenschaften
            this.Description = task.Description;
            this.dueDate = task.DueDate;
            this.oldDueDate = task.DueDate;
            this.IsDueTimeSet = task.IsDueTimeSet;
            this.DueTime = task.DueTime;

            //TaskAssigned To
            LoadTaskAssignedTo(task);

            //Reminder
            var reminder = ScheduledActionService.Find(this.GetReminderName()) as Reminder;
            if (reminder != null)
            {
                this.IsReminderSet = true;
                this.ReminderDateTime = reminder.BeginTime;
                ScheduledActionService.Remove(this.GetReminderName());
            }
            else if (this.isDueTimeSet)
                this.ReminderDateTime = this.DueDate.Date + this.DueTime.TimeOfDay;
            else
                this.ReminderDateTime = this.DueDate.Date + DateTime.Now.TimeOfDay;

            //Events
            task.PropertyChanged += AllPropertiesChanged;
            dataManager.PropertyChanged += dataManager_PropertyChanged;
        }

        private string GetReminderName() { return "DoItTask" + this.taskId; }

        void dataManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoggedIn" || e.PropertyName == "LogInKnown")
            {
                var task = dataManager.Tasks.FirstOrDefault(p => p.TaskId == this.taskId);
                if (task != null)
                    LoadTaskAssignedTo(task);
            }
        }

        private async void LoadTaskAssignedTo(Task task)
        {
            try
            {
                this.Creator = new PersonViewModel(await dataManager.GetFriendData(task.CreatorUserId, true));
                NotifyPropertyChanged("Creator");

                if (this.dataManager.LoggedIn)
                {
                    oldTaskAssignedTo = await dataManager.GetTaskAssignedTo(task.TaskId);

                    IEnumerable<FriendTaskAssignedToViewModel> friendsTaskAssignedTo;
                    if (this.IsEditable)
                    {
                        friendsTaskAssignedTo = (await dataManager.GetFriends()).Select(p =>
                        {
                            //Aus TaskAssignedTo Relationship alles nötige lesen
                            var taskAssignedTo = oldTaskAssignedTo.Where(q => q.PersonUserId == p.UserId).FirstOrDefault();

                            bool isIncluded = false;
                            bool isDone = false;

                            if (taskAssignedTo != null)
                            {
                                isIncluded = true;
                                isDone = taskAssignedTo.IsDone;
                            }

                            return new FriendTaskAssignedToViewModel(p, isIncluded, isDone);
                        });
                    }
                    else
                    {
                        //Lade nur ne Liste an Leuten zum anschauen.
                        var list = new List<FriendTaskAssignedToViewModel>();
                        foreach (var taskAssignedTo in oldTaskAssignedTo) //Nicht mit LINQ Select wegen den async Problemen.
                            list.Add(new FriendTaskAssignedToViewModel(await dataManager.GetFriendData(taskAssignedTo.PersonUserId, true), true, taskAssignedTo.IsDone));
                        friendsTaskAssignedTo = list;
                    }
                    this.FriendsTaskAssignedToAlphabetical = Controls.FriendsByLetterGroup<FriendTaskAssignedToViewModel>.Group(friendsTaskAssignedTo);
                    NotifyPropertyChanged("FriendsTaskAssignedToAlphabetical");
                    NotifyPropertyChanged("FriendsListEmptyString");
                }
            }
            catch
            {
                //Keiner Verbindugn etc. Silent behandeln.
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
        }

        internal static TaskPageViewModel CreateTask(DataManager dataManager)
        {
            var query = new Data.Queries.CreateTaskQuery();
            dataManager.AddQuery(query);
            return new TaskPageViewModel(dataManager, dataManager.Tasks.First(p => p.TaskId == query.TaskId)) { isFirstEdit = true };
        }

        internal static TaskPageViewModel CreateTask(DataManager dataManager, string taskCopyId)
        {
            var viewModel = CreateTask(dataManager);
            Task copyTask = dataManager.Tasks.First(p => p.TaskId == taskCopyId);

            viewModel.Description = copyTask.Description;
            viewModel.isDueTimeSet = copyTask.IsDueTimeSet;
            viewModel.DueTime = copyTask.DueTime;

            return viewModel;
        }

        internal void Remove()
        {
            dataManager.AddQuery(new RemoveTaskQuery() { TaskId = taskId });
        }

        internal void Apply()
        {
            //Was wenn Task inzwischen gelöscht?
            Task task = dataManager.Tasks.FirstOrDefault(p => p.TaskId == taskId);

            if (task != null)
            {
                //Änderungen am Task übertragen
                if ((this.DueTime != task.DueTime) || (this.IsDueTimeSet != task.IsDueTimeSet) ||
                    (this.DueDate != task.DueDate) || (this.Description != task.Description))
                {
                    dataManager.AddQuery(new ModifyTaskQuery()
                    {
                        TaskId = this.taskId,
                        DueDate = this.DueDate,
                        IsDueTimeSet = this.IsDueTimeSet,
                        DueTime = this.DueTime,
                        Description = this.Description,
                        Score = this.Score
                    });
                }

                //TaskAssignedTo updaten
                if (oldTaskAssignedTo != null)
                {
                    try
                    {
                        var handledTasksAssignedTo = new List<TaskAssignedTo>();

                        foreach (var friendAssignedToGroup in FriendsTaskAssignedToAlphabetical)
                        {
                            foreach (var friendAssignedTo in friendAssignedToGroup)
                            {
                                if (friendAssignedTo.IsIncluded)
                                {
                                    var taskAssignedTo = oldTaskAssignedTo.FirstOrDefault(p => p.PersonUserId == friendAssignedTo.GetUserId());

                                    if (taskAssignedTo != null)
                                        handledTasksAssignedTo.Add(taskAssignedTo);
                                    else
                                    {
                                        dataManager.AddTaskAssignedTo(new TaskAssignedTo()
                                        {
                                            TaskId = this.taskId,
                                            PersonUserId = friendAssignedTo.GetUserId(),
                                            Score = 200, //Hier ist der Score Bonus für Freunde.
                                            IsDone = false
                                        });
                                    }
                                }
                            }
                        }

                        //Überflüssige Tasks löschen
                        var taskAssignedToDelete = oldTaskAssignedTo.Where(p => !(p.PersonUserId == this.dataManager.User.UserId ||
                            handledTasksAssignedTo.Any(q => q.PersonUserId == p.PersonUserId)));
                        foreach (var taskAssignedTo in taskAssignedToDelete)
                            dataManager.RemoveTaskAssignedTo(taskAssignedTo);
                    }
                    catch
                    {
                        //Einfach silent ignorieren. :D :D
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                    }
                }

                //Sende, wenn task jetzt erledigt ist.
                if (this.IsDone && this.IsDoneChangeable)
                {
                    string s = "";
                    int score = GetTaskFinishedScore(ref s);
                    dataManager.AddQuery(new TaskDoneQuery() { TaskId = taskId, ScoreReward = score });
                }

                //Reminder aktualisieren. Nicht für Aufgabe in der Vergangenheit anwenden, da Crash gefahr!
                if (this.IsReminderSet && this.ReminderDateTime.AddSeconds(5) > DateTime.Now)
                {
                    if (ScheduledActionService.Find(this.GetReminderName()) != null)
                        ScheduledActionService.Remove(this.GetReminderName());
                    ScheduledActionService.Add(new Reminder(this.GetReminderName())
                    {
                        BeginTime = this.ReminderDateTime,
                        Content = this.Description,
                        ExpirationTime = this.ReminderDateTime.AddDays(1),
                        Title = Localization.Strings.TaskReminderHeader,
                        NavigationUri = new Uri("/MainPage.xaml?taskId=" + this.taskId, UriKind.Relative),
                        RecurrenceType = RecurrenceInterval.None
                    });
                }
            }
            else
            {
                Controls.SafeMessageBox.Show(Localization.Strings.TaskPageNotFoundMessage, "");
            }
        }

        public int GetTaskFinishedScore(ref string bonusString)
        {
            int returnScore = this.Score;
            DateTime finishedDate = DateTime.Today;
            //2 fach Bonus für Fremde Aufgaben wird in Apply implementiert durch einen Basisscore von 200.

            //Bei nicht genau festgelegtem Datum immer Normaler Score.
            if (this.dueDate == DateTime.MaxValue || this.dueDate == DateTime.MinValue)
                return returnScore;

            //50% Abzug für zu spät
            if (this.dueDate.Date < finishedDate.Date)
            {
                returnScore /= 2;
                bonusString = String.Format(Localization.Strings.TaskPageBonusScoreLate, returnScore);
            }
            else if (this.dueDate.Date > finishedDate.Date)
            {
                int bonus = 0;

                //Ein Bonus wird ausgezahlt umso früher der Score eingelöst wird.
                if (this.dueDate.AddDays(1).Date == finishedDate.Date)
                    bonus = 15;
                else
                    bonus = 25;

                bonusString = String.Format(Localization.Strings.TaskPageBonusScoreEarly, bonus);
                returnScore += bonus;
            }

            return returnScore;
        }

        internal string GetTaskId()
        {
            return this.taskId;
        }
    }
}
