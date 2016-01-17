using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DoIt.Data
{
    /// <summary>
    /// Relationshipklasse für Freunde zwischen Personen.
    /// </summary>
    public class PersonHasFriend
    {
        /// <summary>
        /// Eindeutige ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Eindeutige ID der Person. Kommt von Windows Azure Login.
        /// </summary>
        [JsonProperty(PropertyName = "personId")]
        public string PersonId { get; set; }

        /// <summary>
        /// Eindeutige ID des Freundes der Person. Kommt von Windows Azure Login.
        /// </summary>
        [JsonProperty(PropertyName = "friendId")]
        public string FriendId { get; set; }

        /// <summary>
        /// Gibt an, ob die Freundschaftsanfrage bestätigt wurde.
        /// </summary>
        [JsonProperty(PropertyName = "isConfirmed")]
        public bool IsConfirmed { get; set; }
    }
}
