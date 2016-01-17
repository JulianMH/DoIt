using System;
using System.Linq;
using System.Collections.Generic;
using DoIt.Data;
using System.Collections.ObjectModel;
using DoIt.ViewModels;

namespace DoIt.Controls
{
    /// <summary>
    /// Hilfsklasse, ermöglicht alphabetische Sortierung der Freundesliste.
    /// </summary>
    public class FriendsByLetterGroup<T> : ReadOnlyCollection<T>, IGrouping<char, T>
        where T : PersonViewModel
    {
        /// <summary>
        /// Schlüssel für Gruppierung.
        /// </summary>
        public char Key
        {
            get;
            private set;
        }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="key">Zu verwendener Schlüssel für Gruppierung.</param>
        /// <param name="friends">Zu verwendene Liste an Personen in der Gruppe.</param>
        private FriendsByLetterGroup(char key, List<T> friends)
            : base(friends)
        {
            this.Key = key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friends"></param>
        /// <returns></returns>
        internal static IEnumerable<FriendsByLetterGroup<T>> Group(IEnumerable<T> friends)
        {
            var people = friends.OrderBy(p => p.FirstName);
            string groups = Localization.Strings.AlphabeticalOrder;

            Dictionary<char, List<T>> groupsDictionary = new Dictionary<char, List<T>>();
            List<FriendsByLetterGroup<T>> friendsByLetterGroup = new List<FriendsByLetterGroup<T>>();

            foreach (char c in groups)
            {
                groupsDictionary[c] = new List<T>();
            }

            foreach (T person in people)
            {
                if (person.FirstName == null)
                    continue;
                char c = person.FirstName.ToLower()[0];
                if (groupsDictionary.ContainsKey(c))
                    groupsDictionary[c].Add(person);
                else
                    groupsDictionary['#'].Add(person);
            }

            return groupsDictionary.Select(p => new FriendsByLetterGroup<T>(p.Key, p.Value));
        }
    }
}
