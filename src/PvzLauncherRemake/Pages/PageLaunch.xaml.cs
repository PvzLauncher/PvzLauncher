using PvzLauncherRemake.Controls.Icons;
using PvzLauncherRemake.Models;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Game;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static PvzLauncherRemake.Utils.UI.LocalizeService;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageLaunch.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunch : ModernWpf.Controls.Page
    {
        private GameProfile.Root currentGameInfo = null!;
        private TrainerProfile.Root currentTrainerInfo = null!;

        private List<string> echoCaveTemp = new List<string>();

        /*private async Task RefreshEchoCave()
        {
            if (Globals.Config.Settings.LauncherConfig.OfflineMode)
                return;

            try
            {
                if (Globals.Caches.EchoCaveIndex == null)
                {
                    using (var client = new HttpClient())
                    {
                        Globals.Caches.EchoCaveIndex = JsonHelper.ReadJson<JsonEchoCave.Index>(await client.GetStringAsync(Globals.Urls.EchoCaveIndexUrl));
                    }

                    foreach (var echoCave in Globals.Caches.EchoCaveIndex.Data)
                        echoCaveTemp.Add(echoCave);
                }

                if (echoCaveTemp.Count == 0)
                {
                    foreach (var echoCave in Globals.Caches.EchoCaveIndex.Data)
                        echoCaveTemp.Add(echoCave);
                }

                button_EchoCave.IsEnabled = false;
                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                button_EchoCave.BeginAnimation(OpacityProperty, null);
                button_EchoCave.BeginAnimation(OpacityProperty, animation);

                await Task.Delay(500);

                button_EchoCave.Content = echoCaveTemp[Globals.Random.Next(0, echoCaveTemp.Count - 1)];
                echoCaveTemp.Remove((string)button_EchoCave.Content);

                animation.From = 0; animation.To = 1;

                button_EchoCave.BeginAnimation(OpacityProperty, null);
                button_EchoCave.BeginAnimation(OpacityProperty, animation);
                button_EchoCave.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }*/

        #region Animation
        public async void StartAnimation()
        {
            switch (Globals.Config.Settings.LauncherConfig.TitleImage)//切换语言
            {
                case "EN":
                    viewBox_Icon.Child = new TitleImageEn(); break;
                case "ZH":
                    viewBox_Icon.Child = new TitleImageZh(); break;
            }

            viewBox_Icon.RenderTransform = new TranslateTransform { Y = -10 - viewBox_Icon.ActualHeight };
            stackPanel_LaunchButtons.RenderTransform = new TranslateTransform { X = 50 + button_Launch.ActualWidth };

            await Task.Delay(200);//等待Frame动画播放完毕

            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = new BackEase { Amplitude = 0.2, EasingMode = EasingMode.EaseOut }
            };
            ((TranslateTransform)viewBox_Icon.RenderTransform).BeginAnimation(TranslateTransform.YProperty, animation);
            ((TranslateTransform)stackPanel_LaunchButtons.RenderTransform).BeginAnimation(TranslateTransform.XProperty, animation);
        }

        public void StartLaunchAnimation()
        {
            //已转移至UserLaunchAnimation
            return;
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Loaded += ((sender, e) =>
            {
                try
                {


                    if (!string.IsNullOrEmpty(Globals.Config.CurrentGame))
                    {


                        //查找选择游戏信息
                        foreach (var game in Globals.Caches.GameList)
                            if (game.GameInfo.Name == Globals.Config.CurrentGame)
                                currentGameInfo = game;

                        //设置按钮文本
                        textBlock_LaunchVersion.Text = Globals.Config.CurrentGame;

                    }
                    else
                    {
                        button_Launch.IsEnabled = false;
                        textBlock_LaunchVersion.Text = "请选择一个游戏";
                    }

                    if (!string.IsNullOrEmpty(Globals.Config.CurrentTrainer))
                    {

                        foreach (var trainer in Globals.Caches.TrainerList)
                            if (trainer.Name == Globals.Config.CurrentTrainer)
                                currentTrainerInfo = trainer;
                    }
                    else
                    {
                        button_LaunchTrainer.IsEnabled = false;
                    }

                    //判断游戏是否运行
                    try
                    {
                        if (GameManager.IsGameRuning == true)
                        {

                            textBlock_LaunchText.Text = I18N.PageLaunch.StopGame;
                        }
                    }
                    catch (InvalidOperationException) { }

                    //播放动画
                    StartAnimation();

                    //设置背景
                    if (File.Exists(Globals.Paths.BackgroundPath))
                    {
                        if (Globals.Caches.LauncherBackground == null)
                            Globals.Caches.LauncherBackground = BitmapHelper.LoadBitmapImageFromDisk(Globals.Paths.BackgroundPath);
                        if (Globals.Config.Settings.LauncherConfig.BackgroundMode == "custom")
                            image.Source = Globals.Caches.LauncherBackground;
                        else
                            image.Source = null;
                    }


                    /*//回声洞
                    if (Globals.Config.Settings.LauncherConfig.EchoCaveEnabled)
                    {
                        button_EchoCave.Visibility = Visibility.Visible;
                        await RefreshEchoCave();
                    }
                    else
                    {
                        button_EchoCave.Visibility = Visibility.Hidden;
                    }*/




                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
            });
            //button_EchoCave.Click += (async (s, e) => await RefreshEchoCave());
        }

        private async void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button_Launch.IsEnabled = false;

                //没运行就启动
                if (GameManager.IsGameRuning == false)
                {


                    textBlock_LaunchText.Text = GetLoc("I18N.PageLaunch", "StopGame");

                    if (Globals.Config.Settings.LauncherConfig.LaunchAnimationEnabled)
                        Globals.Windows.WindowMain.launchAnimation.PlayAnimation();
                    //再次加载游戏档案
                    currentGameInfo = JsonHelper.ReadJson<GameProfile.Root>(Path.Combine(Globals.Directories.GameDirectory, currentGameInfo.GameInfo.Name, ".pvzl.json"));
                    //启动游戏
                    GameManager.LaunchGame(currentGameInfo, (async () =>
                    {
                        SnackbarService.Show(new SnackbarContent
                        {
                            Title = "提示",
                            Content = $"游戏进程退出, 本次游玩时长: {Math.Round((DateTimeOffset.Now - GameManager.LatestGameLaunchTime!).Value.TotalMinutes),2} 分钟",
                            Type = SnackbarType.Warn
                        });

                        textBlock_LaunchText.Text = GetLoc("I18N.PageLaunch", "LaunchGame");
                    }));

                    //启动修改器(如果有)
                    if (Globals.Config.Settings.LauncherConfig.LaunchWithTrainer && !string.IsNullOrEmpty(Globals.Config.CurrentTrainer))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(Globals.Directories.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                            UseShellExecute = true
                        });
                    }

                    /*//启动提示
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"{Globals.Config.CurrentGame} 启动成功!",
                        Type = SnackbarType.Success
                    });*/
                }
                //运行就结束
                else if (GameManager.IsGameRuning == true)
                {

                    textBlock_LaunchText.Text = GetLoc("I18N.PageLaunch", "LaunchGame");

                    await GameManager.KillGame((() =>
                    {
                        /*SnackbarService.Show(new SnackbarContent
                        {
                            Title = "结束游戏",
                            Content = "成功结束游戏",
                            Type = SnackbarType.Success
                        });*/
                    }), (() =>
                    {
                        SnackbarService.Show(new SnackbarContent
                        {
                            Title = "结束游戏",
                            Content = "无法结束游戏，请手动关闭游戏",
                            Type = SnackbarType.Error
                        });
                    }));

                }

                button_Launch.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        private void button_LaunchTrainer_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                Process.Start(new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(Globals.Directories.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                    UseShellExecute = true,
                    WorkingDirectory = Path.Combine(Globals.Directories.TrainerDirectory, currentTrainerInfo.Name)
                });
                SnackbarService.Show(new SnackbarContent
                {
                    Title = "提示",
                    Content = $"{Globals.Config.CurrentTrainer} 启动成功!",
                    Type = SnackbarType.Success
                });

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
    }
}
