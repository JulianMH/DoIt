using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DoIt.ViewModels;

namespace DoIt
{
    public partial class FinishedTaskPage : PhoneApplicationPage
    {
        public FinishedTaskPage()
        {
            InitializeComponent();
            new LongListSelectorAnimator(this.Dispatcher, TasksLongListSelecetor);

            //ViewModel festlegen
            DataContext = new ViewModels.FinishedTasksViewModel(App.DataManager);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TaskPreviewViewModel task = e.AddedItems[0] as TaskPreviewViewModel;

                if (task != null)
                    this.NavigationService.Navigate(new Uri("/TaskPage.xaml?id=" + task.GetTaskID(), UriKind.Relative));

                TasksLongListSelecetor.SelectedItem = null;
            }
        }
    }
}