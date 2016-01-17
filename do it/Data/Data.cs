using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DoIt.Data.Queries;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace DoIt.Data
{
    /// <summary>
    /// Enthält alle Daten der App außer den Ausstehenden Anfragen. "Dumme" Datenklasse.
    /// </summary>
    public class Data : NotifyPropertyChangedObject
    {
        #region Eigenschaften
        /// <summary>
        /// Der aktuelle Nutzer der App.
        /// </summary>
        public Person User { get; set; }

        /// <summary>
        /// Liste aller Tasks des Nutzers.
        /// </summary>
        public ObservableCollection<Task> Tasks { get { return tasks; } set { tasks = value; NotifyPropertyChanged("Tasks"); } }
        private ObservableCollection<Task> tasks;

        /// <summary>
        /// Liste aller Taskzuweisungen, die den Nutzer etwas zuweisen.
        /// </summary>
        public ObservableCollection<TaskAssignedTo> TaskAssignedToRelationship { get { return taskAssignedToRelationship; } set { taskAssignedToRelationship = value; NotifyPropertyChanged("TaskAssignedToRelationship"); } }
        private ObservableCollection<TaskAssignedTo> taskAssignedToRelationship;

        /// <summary>
        /// Warteschlange aller aktiven Anfragen an den Server.
        /// </summary>
        public List<AbstractXmlSerializer<Query>> Queries;

        public ObservableCollection<Person> FriendsCache { get { return this.friendsCache; } set { friendsCache = value; NotifyPropertyChanged("FriendsCache"); } }
        private ObservableCollection<Person> friendsCache;
        /// <summary>
        /// Gibt an, ob die App an einen Online Account gebunden ist.
        /// </summary>
        public bool LogInKnown { get { return logInKnown; } set { logInKnown = value; NotifyPropertyChanged("LogInKnown"); } }
        private bool logInKnown;

        /// <summary>
        /// Gibt an, ob die App Pushbenachrichtigungen erlaubt.
        /// </summary>
        public bool NotificationsEnabled { get { return notificationsEnabled; } set { notificationsEnabled = value; NotifyPropertyChanged("NotificationsEnabled"); } }
        private bool notificationsEnabled;
        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        public Data()
        {
            this.tasks = new ObservableCollection<Task>();
            this.taskAssignedToRelationship = new ObservableCollection<TaskAssignedTo>();
            this.Queries = new List<AbstractXmlSerializer<Query>>();
            this.FriendsCache = new ObservableCollection<Person>();
            this.User = new Person() { FirstName = "User", UserId = "", Score = 0, AccountCreatedDate = DateTime.Today };
            this.logInKnown = false;
            this.notificationsEnabled = false;
        }

        #region Serialize
        /// <summary>
        /// Serializert alle Daten des DataManagers außer den Queries in einen XML-String.
        /// </summary>
        /// <param name="stream">Filestream zum schreiben.</param>
        /// <returns>Data als XML-String.</returns>
        internal void Serialize(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Data));
            serializer.Serialize(stream, this);
        }

        /// <summary>
        /// Deserializert alle Daten des DataManagers außer den Queries aus einen XML-String.
        /// </summary>
        /// <param name="stream">Filestream zum lesen.</param>
        /// <returns>Der DataManager.</returns>
        internal static Data Deserialize(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Data));

            return (Data)serializer.Deserialize(stream);
        }
        #endregion
    }
}
