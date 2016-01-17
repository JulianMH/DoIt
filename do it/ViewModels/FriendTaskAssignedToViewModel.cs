using DoIt.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DoIt.ViewModels
{
    /// <summary>
    /// Erweitertes FriendPreviewViewModel. Zum hinzufügen und entfernen von Personen zu einem Task.
    /// </summary>
    public sealed class FriendTaskAssignedToViewModel : PersonViewModel
    {
        /// <summary>
        /// Gibt an ob die Person zum Task zugewiesen ist.
        /// </summary>
        public bool IsIncluded { get; set; }

        /// <summary>
        /// Gibt an ob die Person den Task bereits erledigt hat.
        /// </summary>
        public bool IsDone { get; private set; }
        public Visibility IsDoneVisibility { get { return IsDone ? Visibility.Visible : Visibility.Collapsed; } }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="friend">Friend aus dem das ViewModel initialisiert wird.</param>
        /// <param name="isIncluded">Gibt an ob die Person zum Task zugewiesen ist.</param>
        /// <param name="isDone">Gibt an ob die Person den Task bereits erledigt hat.</param>
        internal FriendTaskAssignedToViewModel(Person friend, bool isIncluded, bool isDone)
            : base(friend)
        {
            this.IsIncluded = isIncluded;
            this.IsDone = isDone;
        }
    }
}
