using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Network;
using PvzLauncherRemake.Utils.UI;
using PvzLauncherRemake.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static PvzLauncherRemake.Utils.UI.LocalizeService;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageTask.xaml 的交互逻辑
    /// </summary>
    public partial class PageTask : ModernWpf.Controls.Page
    {
        private DispatcherTimer _timer;

        private void RefreshTaskList()
        {
            stackPanel_Tasks.Children.Clear();

            foreach (var task in TaskManager.DownloadTaskList)
            {
                var card = new UserTaskCard
                {
                    Title = task.TaskName!,
                    Tag = task,
                    Icon = task.TaskIcon,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                card.button_Cancel.Click += (s, e) =>
                {
                    if (card.Tag != null)
                    {
                        TaskManager.StopTask((DownloadTaskInfo)card.Tag);
                    }
                };

                card.InitializeControl();
                card.UpdateControl();
                stackPanel_Tasks.Children.Add(card);
            }
        }

        public PageTask()
        {
            InitializeComponent();
            Loaded += ((s, e) =>
            {
                try
                {
                    RefreshTaskList();
                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
            });

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            TaskManager.TaskAdded += (e) =>
            {
                RefreshTaskList();
            };
            TaskManager.TaskRemoved += (e) =>
            {
                RefreshTaskList();

                if (e.IsComplete != true)
                    return;

                var card = new UserCard
                {
                    Title = e.TaskName!,
                    Icon = e.TaskIcon,
                    Version = e.Info.Version,
                    IsReadOnly = true,
                    Tag = e
                };
                var button = new Button
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new PathIcon
                            {
                                Data=Geometry.Parse("m226-559 78 33q14-28 29-54t33-52l-56-11-84 84Zm142 83 114 113q42-16 90-49t90-75q70-70 109.5-155.5T806-800q-72-5-158 34.5T492-656q-42 42-75 90t-49 90Zm155-121.5q0-33.5 23-56.5t57-23q34 0 57 23t23 56.5q0 33.5-23 56.5t-57 23q-34 0-57-23t-23-56.5ZM565-220l84-84-11-56q-26 18-52 32.5T532-299l33 79Zm313-653q19 121-23.5 235.5T708-419l20 99q4 20-2 39t-20 33L538-80l-84-197-171-171-197-84 167-168q14-14 33.5-20t39.5-2l99 20q104-104 218-147t235-24ZM157-321q35-35 85.5-35.5T328-322q35 35 34.5 85.5T327-151q-25 25-83.5 43T82-76q14-103 32-161.5t43-83.5Zm57 56q-10 10-20 36.5T180-175q27-4 53.5-13.5T270-208q12-12 13-29t-11-29q-12-12-29-11.5T214-265Z"),
                                Width=15,Height=15,
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="开始游戏"
                            }
                        }
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 15, 0),
                    Tag = e
                };
                button.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
                var grid = new Grid
                {
                    Children =
                    {
                        card,
                        button
                    },
                    Margin = new Thickness(0, 0, 0, 10)
                };

                if (e.Info is JsonDownloadIndex.TrainerInfo)
                    button.Visibility = Visibility.Hidden;

                stackPanel_Completed.Children.Add(grid);



                button.Click += async (s, e) =>
                {
                    if (s is not Button btn || btn.Tag is not DownloadTaskInfo dti)
                        return;

                    Globals.Config.CurrentGame = dti.Info.Name;
                    ConfigManager.SaveConfig();

                    NavigationController.Navigate(NavigaionPages.Launch);
                    await Task.Delay(500);

                    if (Application.Current.MainWindow is not WindowMain wm || wm.frame.Content is not PageLaunch pl)
                        return;

                    pl.button_Launch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                };
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                double progressSum = 0;
                double progressAverage = 0;
                double speedSum = 0;

                foreach (UserTaskCard card in stackPanel_Tasks.Children)
                {
                    if (card.Tag != null)
                    {
                        card.Progress = ((DownloadTaskInfo)card.Tag).Progress;
                        card.Speed = ((DownloadTaskInfo)card.Tag).Speed;
                        card.ProgressCompress = ((DownloadTaskInfo)card.Tag).ExtractProgress;
                        card.UpdateControl();
                    }
                }
                foreach (var task in TaskManager.DownloadTaskList)
                {
                    progressSum = progressSum + task.Progress + task.ExtractProgress;
                    speedSum = speedSum + task.Speed;
                }

                progressAverage = progressSum / (TaskManager.DownloadTaskList.Count * 2);

                textBlock_ProgressAverage.Text = $"{GetLoc("I18N.PageTask", "Total_Progress")}: {(double.IsNaN(progressAverage) ? "0" : Math.Round(progressAverage, 2))}%";
                textBlock_SpeedSum.Text = $"{GetLoc("I18N.PageTask", "Total_Speed")}: {(double.IsNaN(speedSum) ? "0" : Math.Round(speedSum, 2))}MB/s";
                progressBar_Average.Value = double.IsNaN(progressAverage) ? 0 : progressAverage;

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }

        }

        private void button_Download_Click(object sender, RoutedEventArgs e) => NavigationController.Navigate(NavigaionPages.Download);
    }
}
