using System;
using System.Linq;
using DoIt.Data;
using System.Windows;

namespace DoIt.ViewModels
{
    /// <summary>
    /// ViewModel für die Minivorschau einer Aufgabe.
    /// </summary>
    public class TaskPreviewViewModel : NotifyPropertyChangedObject
    {
        private DataManager dataManager;
        /// <summary>
        /// Der Task als Datenquelle des ViewModels.
        /// </summary>
        private Task task;

        #region Bindable Properties
        /// <summary>
        /// Beschreibung der Aufgabe.
        /// </summary>
        public string Description { get { return task.Description; } }
        /// <summary>
        /// Beschreibung der Aufgabe.
        /// </summary>
        public string DescriptionCapped
        {
            get
            {
                string description = task.Description;
                description = description.Split('\r', '\n')[0];

                if (description.Length > 14) //Sichergehen das Der Text auf die Tiles passt.
                    return description.Substring(0, 14) + "...";
                else return description;
            }
        }

        /// <summary>
        /// Wert der Aufgabe, sobald sie erfüllt wurde.
        /// </summary>
        public int Score
        {
            get
            {
                int score;
                dataManager.GetTaskIsDone(task, out score);
                return score;
            }
        }

        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public DateTime DueDate { get { return task.DueDate; } }
        public bool IsCustomDueDate { get { return this.DueDate != DateTime.MinValue && this.DueDate != DateTime.MaxValue; } }
        public DateTime DueTime { get { return task.DueTime; } }
        public string DueDateString { get { if (IsCustomDueDate) return String.Format("{0:dddd}, {0:d}", task.DueDate); else return ""; } }

        public string DueTimeString { get { return task.DueTime.ToShortTimeString(); } }
        public Visibility DueTimeVisibility { get { return task.IsDueTimeSet ? Visibility.Visible : Visibility.Collapsed; } }

        /// <summary>
        /// Ist Collapsed, wenn die Aufgabe allein dem User gehört.
        /// Ansonsten Visible.
        /// </summary>
        public Visibility ContactIconVisibility
        {
            get
            {
                return task.IsFromUser(dataManager.User) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public PersonViewModel CreatorIfNotUser { get; private set; }

        public Visibility OverdueMarkerVisibility
        {
            get
            {
                return task.DueDate.Date < DateTime.Now.Date ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="task">Der Task als Datenquelle des ViewModels.</param>
        internal TaskPreviewViewModel(DataManager dataManager, Task task)
        {
            this.task = task;
            this.dataManager = dataManager;

            LoadCreatorIfNotUser();

            this.dataManager.PropertyChanged += dataManager_PropertyChanged;
            this.task.PropertyChanged += AllPropertiesChanged;
        }

        void dataManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoggedIn")
                LoadCreatorIfNotUser();
        }

        private async void LoadCreatorIfNotUser()
        {
            if (!task.IsFromUser(dataManager.User))
            {
                CreatorIfNotUser = new PersonViewModel(await dataManager.GetFriendData(task.CreatorUserId, false));
                NotifyPropertyChanged("CreatorIfNotUser");
            }
        }

        /// <summary>
        /// Gibt die ID vom Task weiter.
        /// Ermöglicht Navigieren zu Detailseite.
        /// </summary>
        /// <returns>Interne ID vom Task.</returns>
        internal string GetTaskID()
        {
            return task.TaskId;
        }
    }
}