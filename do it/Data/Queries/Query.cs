using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt.Data.Queries
{
    /// <summary>
    /// Eine einzelne Daten-Änderungsanfrage.
    /// </summary>
    public abstract class Query
    {
        /// <summary>
        /// Sendet die Anfrage an das Server Backend.
        /// </summary>
        internal abstract System.Threading.Tasks.Task Send(IEnumerable<Task> tasks, IMobileServiceTable<Task> tasksTable,
            IEnumerable<TaskAssignedTo> taskAssignedToRelationship, IMobileServiceTable<TaskAssignedTo> taskAssignedToTable,
            Person user);

        /// <summary>
        /// Wendet die Anfrage auf die gegebenen Daten an
        /// </summary>
        /// <param name="tasks">Alle unerledigten Aufgabendaten des Users.</param>
        /// <param name="user">Der User.</param>
        internal abstract void Apply(IList<Task> tasks, IList<TaskAssignedTo> taskAssignedToRelationship, Person user);
    }
}
