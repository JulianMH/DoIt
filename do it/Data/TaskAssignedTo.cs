using System;
using Newtonsoft.Json;

namespace DoIt.Data
{
    /// <summary>
    /// Relationship-Klasse
    /// </summary>
    public class TaskAssignedTo : NotifyPropertyChangedObject
    {
        public int Id { get; set; }

        /// <summary>
        /// Eindeutige ID des Tasks.
        /// </summary>
        [JsonProperty(PropertyName = "taskId")]
        public string TaskId { get; set; }

        /// <summary>
        /// Eindeutige ID der Person.
        /// </summary>
        [JsonProperty(PropertyName = "personUserId")]
        public string PersonUserId { get; set; }

        /// <summary>
        /// Gibt an, ob die Aufgabe erfüllt wurde.
        /// </summary>
        [JsonProperty(PropertyName = "isDone")]
        public bool IsDone { get { return isDone; } set { isDone = value; NotifyPropertyChanged("IsDone"); } }
        private bool isDone;

        /// <summary>
        /// Gibt an, ob die Aufgabe erfüllt wurde.
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value; NotifyPropertyChanged("Score");
            }
        }
        private int score;
    }
}
