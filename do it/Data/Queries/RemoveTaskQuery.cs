using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace DoIt.Data.Queries
{
    /// <summary>
    /// Eine einzelne Daten-Änderungsanfrage für die Eigenschaften einesTasks.
    /// </summary>
    public class RemoveTaskQuery : Query
    {
        /// <summary>
        /// Datum, an dem die Aufgabe erfüllt sein sollte.
        /// </summary>
        public string TaskId { get; set; }

        public int AzureTaskId { get; set; }
        public bool DeleteRelationshipOnly { get; set; }

        internal override async System.Threading.Tasks.Task Send(IEnumerable<Task> tasks, IMobileServiceTable<Task> tasksTable,
            IEnumerable<TaskAssignedTo> taskAssignedToRelationship, IMobileServiceTable<TaskAssignedTo> taskAssignedToTable,
            Person user)
        {
            if (DeleteRelationshipOnly)
            {
                var list = await taskAssignedToTable.Where(p => p.TaskId == this.TaskId && p.PersonUserId == user.UserId).ToEnumerableAsync();
                foreach (var taskAssignedTo in list)
                    await taskAssignedToTable.DeleteAsync(taskAssignedTo);
            }
            else
            {
                var list = await taskAssignedToTable.Where(p => p.TaskId == this.TaskId).ToEnumerableAsync();
                foreach (var taskAssignedTo in list)
                    await taskAssignedToTable.DeleteAsync(taskAssignedTo);

                if (this.AzureTaskId != 0)
                {
                    await tasksTable.DeleteAsync(new Task() { Id = AzureTaskId });
                }
            }
        }
        
        /// <summary>
        /// Wendet die Anfrage auf die gegebenen Daten an
        /// </summary>
        /// <param name="tasks">Alle unerledigten Aufgabendaten des Users.</param>
        /// <param name="user">Der User.</param>
        internal override void Apply(IList<Task> tasks, IList<TaskAssignedTo> taskAssignedToRelationship, Person user)
        {
            var task = tasks.First(p => p.TaskId == TaskId);

            //Wenn der Task nicht von User selber ist dann auch die Verlinkung rauswerfen.
            if (!task.IsFromUser(user))
            {
                this.DeleteRelationshipOnly = true;

                var list =  taskAssignedToRelationship.Where(p => p.TaskId == this.TaskId).ToList();
                foreach (var item in list)
                    taskAssignedToRelationship.Remove(item);
            }
            else
                this.DeleteRelationshipOnly = false;

            tasks.Remove(task);
            this.AzureTaskId = task.Id;

        }
    }
}
