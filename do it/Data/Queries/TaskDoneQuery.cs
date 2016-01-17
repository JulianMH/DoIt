using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt.Data.Queries
{
    /// <summary>
    /// Eine Anfrage für einen abgeschlossenen Task.
    /// </summary>
    public class TaskDoneQuery : Query
    {
        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// Die Belohungspunkte.
        /// </summary>
        public int ScoreReward { get; set; }

        internal override async System.Threading.Tasks.Task Send(IEnumerable<Task> tasks, IMobileServiceTable<Task> tasksTable,
            IEnumerable<TaskAssignedTo> taskAssignedToRelationship, IMobileServiceTable<TaskAssignedTo> taskAssignedToTable,
            Person user)
        {
            var taskAssignedTo = taskAssignedToRelationship.FirstOrDefault(p => p.TaskId == this.TaskId && p.PersonUserId == user.UserId);

            //TODO: Onlinescore. Score übernehmen geschieht beim LogIn. Bisher.

            if (taskAssignedTo == null || (taskAssignedTo.Id == 0))
            {
                //Fehlerfall silent lösen, sollte eigentlich nie vorkommen!
                //Gehe einfach davon aus es sein nichts passiert :D
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
            else
            {
                if (taskAssignedTo.PersonUserId == "")
                    taskAssignedTo.PersonUserId = user.UserId;

                await taskAssignedToTable.UpdateAsync(taskAssignedTo);
            }
        }

        public bool ScoreIncreased { get; set; }

        public TaskDoneQuery() { this.ScoreIncreased = false; }

        /// <summary>
        /// Wendet die Anfrage auf die gegebenen Daten an
        /// </summary>
        /// <param name="tasks">Alle unerledigten Aufgabendaten des Users.</param>
        /// <param name="user">Der User.</param>
        internal override void Apply(IList<Task> tasks, IList<TaskAssignedTo> taskAssignedToRelationship, Person user)
        {
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
                taskAssignedTo.Score = this.ScoreReward;
                taskAssignedTo.IsDone = true;
            }

            if (!this.ScoreIncreased) //Bei Mehrmaliger ausführung soll nichts kaputt gehen.
            {
                this.ScoreIncreased = true;
                user.Score += this.ScoreReward;
            }
        }
    }
}
