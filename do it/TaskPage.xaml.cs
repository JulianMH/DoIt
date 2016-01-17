using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DoIt.ViewModels;
using DoIt.Data.Queries;
using Microsoft.Phone.Shell;

namespace DoIt
{
    public partial class TaskPage : PhoneApplicationPage
    {
        public TaskPage()
        {
            InitializeComponent();

            if (App.IsAdvertismentVisible)
            {
                var adControl = AdControlHelper.CreateAdControl("10167809");
                adControl.Margin = new Thickness(-12, 6, -12, 0);
                Grid.SetRow(adControl, 3);
                this.TaskGrid.Children.Add(adControl);
            }

            new LongListSelectorAnimator(this.Dispatcher, FriendsLongListSelector);
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.DataContext == null)
            {
                string taskIdString = "";
                if (NavigationContext.QueryString.TryGetValue("id", out taskIdString))
                {
                    this.IsEnabled = false;
                    //Lade bekannten Task.
                    SystemTrayHelper.ShowProgress(this, Localization.Strings.MainPageProgressIndicatorLoggingIn, 1);
                    await App.DataManager.LogInTask;
                    SystemTrayHelper.ShowProgress(this, Localization.Strings.TaskPageProgressIndicatorLoadingTask, 1);
                    var task = await App.DataManager.GetTask(taskIdString);
                    SystemTrayHelper.HideProgress(this);
                    if (task != null)
                    {
                        this.DataContext = new TaskPageViewModel(App.DataManager, task);
                        this.IsEnabled = true;
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached) //Fehler, selbst online existiert der Task nicht
                            System.Diagnostics.Debugger.Break();
                        Controls.SafeMessageBox.Show(Localization.Strings.TaskPageNotFoundMessage, "");
                        isRemoved = true;
                        this.NavigationService.GoBack();
                        return;
                    }
                }
                else //Neuer Task wurde erstellt.
                {
                    if (NavigationContext.QueryString.TryGetValue("copy", out taskIdString))
                        this.DataContext = TaskPageViewModel.CreateTask(App.DataManager, taskIdString);
                    else
                        this.DataContext = TaskPageViewModel.CreateTask(App.DataManager);
                }
            }

            SetupAppBar();
        }

        /// <summary>
        /// Erstellt die ApplicationBar.
        /// </summary>
        private void SetupAppBar()
        {
            this.ApplicationBar = new ApplicationBar();
            var button = new ApplicationBarIconButton(new Uri("/Icons/AppBar/delete.png", UriKind.Relative))
                {
                    Text = Localization.Strings.ApplicationBarRemove
                };
            var viewModel = ((TaskPageViewModel)this.DataContext);
            if (!viewModel.IsDoneChangeable)
                this.ApplicationBar.Buttons.Add(ApplicationBarHelpers.CreateButton("copy", "Icons/AppBar/add.png", this.NavigationService,
                    "/TaskPage.xaml?copy=" + viewModel.GetTaskId()));

            button.Click += new EventHandler(button_Click);
            this.ApplicationBar.Buttons.Add(button);
        }

        private bool isRemoved = false;
        void button_Click(object sender, EventArgs e)
        {
            if (Controls.SafeMessageBox.Show(Localization.Strings.TaskPageRemoveMessage, Localization.Strings.TaskPageRemoveMessageTitle) == MessageBoxResult.OK)
            {
                isRemoved = true;
                this.NavigationService.GoBack();
                ((TaskPageViewModel)this.DataContext).Remove();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            //Bei den beiden Pickern soll nicht gespeichert werden, ansonsten schon.
            if (!isRemoved && this.DataContext != null && !e.Uri.ToString().Contains("Picker"))
                ((TaskPageViewModel)this.DataContext).Apply();
            base.OnNavigatedFrom(e);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Um das Virtuelle Keyboard zu verstecken und die Eingabe abzuschließen
            //oder die nächste TextBox auswählen
            //nur wenn Enter gedrückt wurde.
            if (e.Key == Key.Enter)
                Focus();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = ((TaskPageViewModel)this.DataContext);
            if (viewModel.IsDoneChangeable != false)
            {
                string bonusString = Localization.Strings.TaskPageBonusScoreOnTime;
                int score = viewModel.GetTaskFinishedScore(ref bonusString);
                TaskScoreLabel.ScorePayedAnimation(score, bonusString);
            }
        }

        private void TextBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var viewModel = ((TaskPageViewModel)this.DataContext);
            if (viewModel.Description == Localization.Strings.TaskNewDescription)
                viewModel.Description = "";
        }
    }
}