using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt.Data.Queries
{
    /// <summary>
    /// Eine einzelne Daten-Änderungsanfrage für die Eigenschaften einesTasks.
    /// </summary>
    public class ModifyTaskQuery : Query
    {
        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Uhrzeit zum Durchführen der Aufgabe.
        /// </summary>
        public bool IsDueTimeSet { get; set; }

        /// <summary>
        /// Uhrzeit zum Durchführen der Aufgabe.
        /// </summary>
        public DateTime DueTime { get; set; }

        /// <summary>
        /// Beschreibung der Aufgabe.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Beschreibung der Aufgabe.
        /// </summary>
        public int Score { get; set; }

        internal override async System.Threading.Tasks.Task Send(IEnumerable<Task> tasks, IMobileServiceTable<Task> tasksTable,
            IEnumerable<TaskAssignedTo> taskAssignedToRelationship, IMobileServiceTable<TaskAssignedTo> taskAssignedToTable,
            Person user)
        {
            var task = tasks.FirstOrDefault(p => p.TaskId == TaskId);
            if (task != null && task.Id != 0) //Wenn einfügen nicht erfolgreich war ist auch alles egal.
                await tasksTable.UpdateAsync(task);

            var taskAssignedTo = taskAssignedToRelationship.FirstOrDefault(p => p.TaskId == this.TaskId && p.PersonUserId == user.UserId);
            if (taskAssignedTo != null && taskAssignedTo.Id != 0) //Wenn einfügen nicht erfolgreich war ist auch alles egal.
                await taskAssignedToTable.UpdateAsync(taskAssignedTo);
        }

        /// <summary>
        /// Wendet die Anfrage auf die gegebenen Daten an
        /// </summary>
        /// <param name="tasks">Alle unerledigten Aufgabendaten des Users.</param>
        /// <param name="user">Der User.</param>
        internal override void Apply(IList<Task> tasks, IList<TaskAssignedTo> taskAssignedToRelationship, Person user)
        {
            var task = tasks.First(p => p.TaskId == TaskId);

            task.Description = this.Description;
            task.DueDate = this.DueDate;
            task.IsDueTimeSet = this.IsDueTimeSet;
            task.DueTime = this.DueTime;

            var taskAssignedTo = taskAssignedToRelationship.FirstOrDefault(p => p.TaskId == this.TaskId && p.PersonUserId == user.UserId);

            if (taskAssignedTo == null)
            {
                //Fehlerfall silent lösen, sollte eigentlich nie vorkommen!
                //Gehe einfach davon aus es sein nichts passiert :D
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
            else
            {
                taskAssignedTo.Score = this.Score;
            }
        }
    }
}
