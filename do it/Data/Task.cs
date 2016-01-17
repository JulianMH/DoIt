using System;
using Newtonsoft.Json;

namespace DoIt.Data
{
    /// <summary>
    /// Eine Aufgabe des Users.
    /// </summary>
    public class Task : NotifyPropertyChangedObject
    {
        #region Eigenschaften
        /// <summary>
        /// Eindeutige ID des Tasks. Für Azure.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Eindeutige ID des Tasks.
        /// </summary>
        [JsonProperty(PropertyName = "taskId")]
        public string TaskId { get; set; }

        /// <summary>
        /// Person, die die Aufgabe erstellt hat.
        /// </summary>
        [JsonProperty(PropertyName = "creatorUserId")]
        public string CreatorUserId { get { return creatorUserId; } set { creatorUserId = value; NotifyPropertyChanged("Creator"); } }
        private string creatorUserId;

        /// <summary>
        /// Erstelldatum der Aufgabe.
        /// </summary>
        [JsonProperty(PropertyName = "createdDate")]
        public DateTime CreatedDate { get { return createdDate; } set { createdDate = value; NotifyPropertyChanged("CreatedDate"); } }
        private DateTime createdDate;

        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        [JsonProperty(PropertyName = "dueDate")]
        public DateTime DueDate
        {
            get { return dueDate; }
            set
            {
                if (value.Date == DateTime.MinValue.Date) //Dirty Fix dafür, dass Azure mir den Mist hier kaputt macht.
                    dueDate = DateTime.MinValue;
                else if (value.Date == DateTime.MaxValue.Date)
                    dueDate = DateTime.MaxValue;
                else
                    dueDate = value;
                NotifyPropertyChanged("DueDate");
            }
        }
        private DateTime dueDate;

        /// <summary>
        /// Ist Uhrzeit zum Durchführen der Aufgabe festgelegt.
        /// </summary>
        [JsonProperty(PropertyName = "isDueTimeSet")]
        public bool IsDueTimeSet { get { return isDueTimeSet; } set { isDueTimeSet = value; NotifyPropertyChanged("IsDueTimeSet"); } }
        private bool isDueTimeSet;

        /// <summary>
        /// Uhrzeit zum Durchführen der Aufgabe.
        /// </summary>
        [JsonProperty(PropertyName = "dueTime")]
        public DateTime DueTime { get { return dueTime; } set { dueTime = value; NotifyPropertyChanged("DueTime"); } }
        private DateTime dueTime;

        /// <summary>
        /// Beschreibung der Aufgabe.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        private string description;
        #endregion

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="id">Eindeutige Azure ID der Aufgabe.</param>
        /// <param name="taskId">Eindeutige ID der Aufgabe.</param>
        /// <param name="description">Beschreibung der Aufgabe.</param>
        /// <param name="creatorUserId">Person, die die Aufgabe erstellt hat.</param>
        /// <param name="score">Wert der Aufgabe, sobald sie erfüllt wurde.</param>
        /// <param name="createdDate">Erstelldatum der Aufgabe.</param>
        /// <param name="dueDate">Datum, an dem die Aufgabe erfüllt sein sollte.</param>
        /// <param name="dueTime">Uhrzeit zum Durchführen der Aufgabe.</param>
        internal Task(int id, string taskId, string description, string creatorUserId, DateTime createdDate, DateTime dueDate, bool isDueTimeSet, DateTime dueTime)
        {
            this.Id = id;
            this.TaskId = taskId;
            this.description = description;
            this.creatorUserId = creatorUserId;
            this.createdDate = createdDate;
            this.dueDate = dueDate;
            this.isDueTimeSet = isDueTimeSet;
            this.dueTime = dueTime;
        }

        /// <summary>
        /// Parameterloser Konstruktor für den XmlSerializer.
        /// </summary>
        public Task() { }

        /// <summary>
        /// Gibt an, ob die Aufgabe von User der App ist.
        /// </summary>
        /// <param name="user">Aktueller User der App.</param>
        /// <returns>true, falls die Aufgabe dem User gehört.</returns>
        internal bool IsFromUser(Person user)
        {
            return this.creatorUserId == user.UserId || this.creatorUserId == "";
        }


    }
}
