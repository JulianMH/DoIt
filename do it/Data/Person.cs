using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DoIt.Data
{
    /// <summary>
    /// Eine Person, im Normalfall Freund des Nutzers, der die App auch nutzt.
    /// </summary>
    public class Person : NotifyPropertyChangedObject
    {
        /// <summary>
        /// Eindeutige ID der Person. Immer korrekt, da Online ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Eindeutige ID der Person. Kommt von Windows Azure Login. Wird zum identifizieren verwendet.
        /// </summary>
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Eindeutige ID der Person. Kommt vom authentifizierenden Service.
        /// </summary>
        [JsonProperty(PropertyName = "authenticationUserId")]
        public string AuthenticationUserId { get; set; }
        /// <summary>
        /// Nachname der Person.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Vorname der Person.
        /// </summary>
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Die URL des Push Channels.
        /// </summary>
        [JsonProperty(PropertyName = "pushChannel")]
        public string PushChannel { get; set; }

        /// <summary>
        /// Land und Sprache die der User verwendet.
        /// </summary>
        [JsonProperty(PropertyName = "culture")]
        public string Culture { get; set; }

        /// <summary>
        /// Punktestand der Person.
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public int Score
        {
            get { return score; }
            set
            {
                this.score = value;
                NotifyPropertyChanged("Score");
            }
        }
        private int score;

        /// <summary>
        /// Datum an dem der Account erstellt wurde.
        /// </summary>
        [JsonProperty(PropertyName = "accountCreatedDate")]
        public DateTime AccountCreatedDate { get; set; }

        /// <summary>
        /// Version die der User in der App verwendet.
        /// </summary>
        [JsonProperty(PropertyName = "wpAppVersion")]
        public string WPAppVersion { get; set; }

        /// <summary>
        /// Anzahl der nicht gesehenen Notifications des Nutzers.
        /// </summary>
        [JsonProperty(PropertyName = "notificationCount")]
        public int NotificationCount { get; set; }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="id">Eindeutige ID der Person.</param>
        /// <param name="userId">Eindeutige ID der Person. Kommt von Windows Azure Login.</param>
        /// <param name="authenticationUserId">Eindeutige ID der Person. Kommt vom authentifizierenden Service.</param>
        /// <param name="name">Name der Person.</param>
        /// <param name="firstName">Vorname der Person.</param>
        /// <param name="pushChannel">Die URL des Push Channels.</param>
        /// <param name="culture">Land der Nutzers.</param>
        /// <param name="score">Punktestand der Person.</param>
        /// <param name="accountCreatedDate">Datum an dem der Account erstellt wurde.</param>
        internal Person(int id, string userId, string authenticationUserId, string name, string firstName, 
            string pushChannel, string culture, int score, string wpAppVersion, DateTime accountCreatedDate)
        {
            this.Id = Id;
            this.Name = name;
            this.UserId = userId;
            this.AuthenticationUserId = authenticationUserId;
            this.FirstName = firstName;
            this.PushChannel = pushChannel;
            this.Culture = culture;
            this.Score = score;
            this.WPAppVersion = wpAppVersion;
            this.AccountCreatedDate = accountCreatedDate;
        }

        /// <summary>
        /// Parameterloser Konstruktor für den XmlSerializer.
        /// </summary>
        public Person() { }

        /// <summary>
        /// Setzt aus vor und Nachnahmen einen vollen Namen zusammen.
        /// </summary>
        /// <returns>Den vollen Namen der Person.</returns>
        internal string GetFullName()
        {
            return String.Format(Localization.Strings.PersonFullName, this.FirstName, this.Name);
        }
    }
}
