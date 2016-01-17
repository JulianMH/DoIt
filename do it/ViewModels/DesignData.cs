using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using DoIt.Data;

namespace DoIt.ViewModels
{
    /// <summary>
    /// Stellt Designdaten für den XAML-Designer bereit.
    /// </summary>
    public class DesignData
    {
        public MainPageViewModel MainPageViewModel { get { return new MainPageViewModel(Data.DataManager.SampleData); } }
        public FinishedTasksViewModel FinishedTasksViewModel { get { return new FinishedTasksViewModel(Data.DataManager.SampleData); } }
        public PersonViewModel FriendPreviewViewModel { get { return new PersonViewModel(
            new Person(6, "MicrosoftAccount:8c0c7b2c1608d99d2b8eeba2628e84e7", "10e3df3749fcae2c", "Mustermann", "Julian", "", "de-DE", 2340, App.VersionNumber, DateTime.Today)); } }
        public TaskPageViewModel TaskPageViewModel { get { return new TaskPageViewModel(Data.DataManager.SampleData, Data.DataManager.SampleData.Tasks[3]); } }


        public Dictionary<char, List<PersonViewModel>> AlphabeticalFriends
        {
            get
            {
                var people = Data.DataManager.SampleData.GetFriends().Result.OrderBy(p => p.FirstName);
                string groups = "#abcdefghijklmnopqrstuvwxyz";

                Dictionary<char, List<PersonViewModel>> groupsDictionary = new Dictionary<char, List<PersonViewModel>>();

                foreach (char c in groups)
                {
                    groupsDictionary[c] = new List<PersonViewModel>();
                }

                foreach (Person person in people)
                {
                    if (person.FirstName == null)
                        continue;
                    char c = person.FirstName.ToLower()[0];
                    if (groupsDictionary.ContainsKey(c))
                        groupsDictionary[c].Add(new PersonViewModel(person));
                    else
                        groupsDictionary['#'].Add(new PersonViewModel(person));
                }

                return groupsDictionary;
            }
        }
    }
}
