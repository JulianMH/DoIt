using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DoIt.Data;
using DoIt.Data.Queries;

namespace DoIt.ViewModels
{
    /// <summary>
    /// ViewModel für die UnfinishedTasksPage.
    /// </summary>
    public class FinishedTasksViewModel : NotifyPropertyChangedObject
    {
        /// <summary>
        /// Daten für das ViewModel.
        /// </summary>
        private DataManager dataManager;

        /// <summary>
        /// Alle erledigten Aufgaben gruppiert nach Name.
        /// </summary>
        public IEnumerable<PublicGrouping<string, TaskPreviewViewModel>> GroupedTasks { get; private set; }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="dataManager">Der DataManager, mit dessen Daten das View Model befüllt wird.</param>
        internal FinishedTasksViewModel(DataManager dataManager)
        {
            this.dataManager = dataManager;

            this.dataManager.Tasks.CollectionChanged += DataManagerTasks_CollectionChanged;
            this.dataManager.PropertyChanged += dataManager_PropertyChanged;

            foreach (Task task in this.dataManager.Tasks)
                task.PropertyChanged += new PropertyChangedEventHandler(task_PropertyChanged);
            DataManagerTasks_CollectionChanged(this, null);
        }

        void dataManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tasks")
            {
                this.dataManager.Tasks.CollectionChanged += DataManagerTasks_CollectionChanged;

                foreach (Task task in this.dataManager.Tasks)
                    task.PropertyChanged += new PropertyChangedEventHandler(task_PropertyChanged);
            }
        }

        private void DataManagerTasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null && e.NewItems != null)
            {
                foreach (Task task in e.NewItems)
                    task.PropertyChanged += new PropertyChangedEventHandler(task_PropertyChanged);
            }

            task_PropertyChanged(null, null);
        }

        private char GetAlphabetLetter(string text)
        {
            if (text.Length >= 1)
            {
                string alphabet = Localization.Strings.AlphabeticalOrder;
                char firstChar = text.ToLower()[0];
                if (alphabet.Contains(firstChar))
                    return firstChar;
            }
            return '#';
        }

        private void task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tasks = this.dataManager.Tasks.Where(p => dataManager.GetTaskIsDone(p))
                .Select(p => new TaskPreviewViewModel(this.dataManager, p));

            //Alphabetische gruppierung
            this.GroupedTasks = tasks.OrderBy(p => p.Description).GroupBy(p => GetAlphabetLetter(p.Description).ToString())
                    .Select(p => new PublicGrouping<string, TaskPreviewViewModel>(p));

            NotifyPropertyChanged("GroupedTasks");
        }

    }
}
