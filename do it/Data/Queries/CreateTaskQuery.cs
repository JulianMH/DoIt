using System;
using System.Collections.Generic;
using System.Linq;
using DoIt.ViewModels;
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt.Data.Queries
{
    /// <summary>
    /// Eine einzelne Daten-Änderungsanfrage für die Eigenschaften einesTasks.
    /// </summary>
    public class CreateTaskQuery : Query
    {
        /// <summary>
        /// Eindeutige Id der Aufgabe.
        /// </summary>
        public string TaskId { get; set; }

        internal override async System.Threading.Tasks.Task Send(IEnumerable<Task> tasks, IMobileServiceTable<Task> tasksTable,
            IEnumerable<TaskAssignedTo> taskAssignedToRelationship, IMobileServiceTable<TaskAssignedTo> taskAssignedToTable,
            Person user)
        {
            var task = tasks.FirstOrDefault(p => p.TaskId == this.TaskId);
            if (task != null && task.Id == 0) //Tasks nur hochladen wenn ihre id unbekannt ist, sonst sind sie vermutlich eh schon hochgeladen.
                await tasksTable.InsertAsync(task);

            var taskAssignedTo = taskAssignedToRelationship.FirstOrDefault(p => p.TaskId == this.TaskId && p.PersonUserId == user.UserId);
            if (taskAssignedTo != null && taskAssignedTo.Id == 0) //Tasks nur hochladen wenn ihre id unbekannt ist, sonst sind sie vermutlich eh schon hochgeladen.
                await taskAssignedToTable.InsertAsync(taskAssignedTo);
        }

        /// <summary>
        /// Wendet die Anfrage auf die gegebenen Daten an
        /// </summary>
        /// <param name="tasks">Alle unerledigten Aufgabendaten des Users.</param>
        /// <param name="user">Der User.</param>
        internal override void Apply(IList<Task> tasks, IList<TaskAssignedTo> taskAssignedToRelationship, Person user)
        {
            if(this.TaskId == null || this.TaskId == "")
                this.TaskId = Guid.NewGuid().ToString();

            var taskAssignedTo = new TaskAssignedTo() { TaskId = this.TaskId, Score = 100, PersonUserId = user.UserId, IsDone = false };
            taskAssignedToRelationship.Add(taskAssignedTo);

            var task = new Task(0, this.TaskId, Localization.Strings.TaskNewDescription, user.UserId,
                DateTime.Today, DateTime.Today.AddDays(1), false, DateTime.Now);
            tasks.Add(task);
        }
    }
}
