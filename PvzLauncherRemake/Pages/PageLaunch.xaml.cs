using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Controls.Icons;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using static PvzLauncherRemake.Utils.Configuration.LocalizeManager;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageLaunch.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunch : ModernWpf.Controls.Page
    {
        private JsonGameInfo.Index currentGameInfo = null!;
        private JsonTrainerInfo.Index currentTrainerInfo = null!;

        private List<string> echoCaveTemp = new List<string>();

        /*private async Task RefreshEchoCave()
        {
            if (AppGlobals.Config.Settings.LauncherConfig.OfflineMode)
                return;

            try
            {
                if (AppGlobals.Indexes.EchoCaveIndex == null)
                {
                    using (var client = new HttpClient())
                    {
                        AppGlobals.Indexes.EchoCaveIndex = Json.ReadJson<JsonEchoCave.Index>(await client.GetStringAsync(AppGlobals.Urls.EchoCaveIndexUrl));
                    }

                    foreach (var echoCave in AppGlobals.Indexes.EchoCaveIndex.Data)
                        echoCaveTemp.Add(echoCave);
                }

                if (echoCaveTemp.Count == 0)
                {
                    foreach (var echoCave in AppGlobals.Indexes.EchoCaveIndex.Data)
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

                button_EchoCave.Content = echoCaveTemp[AppGlobals.Random.Next(0, echoCaveTemp.Count - 1)];
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

            switch (AppGlobals.Config.Settings.LauncherConfig.TitleImage)//切换语言
            {
                case "EN":
                    viewBox_Icon.Child = new TitleImageEn(); break;
                case "ZH":
                    viewBox_Icon.Child = new TitleImageZh(); break;
            }

            viewBox_Icon.Margin = new Thickness(0, -10 - viewBox_Icon.ActualHeight, 0, 0);
            stackPanel_LaunchButtons.Margin = new Thickness(0, 0, -50 - button_Launch.ActualWidth, 0);

            await Task.Delay(200);//等待Frame动画播放完毕

            var animation = new ThicknessAnimation
            {
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = new BackEase { Amplitude = 0.2, EasingMode = EasingMode.EaseOut }
            };
            viewBox_Icon.BeginAnimation(MarginProperty, animation);
            stackPanel_LaunchButtons.BeginAnimation(MarginProperty, animation);
        }

        public async Task StartLaunchAnimation()
        {
            //初始化状态
            var elli1Trans = elli_ani_1.RenderTransform as ScaleTransform;
            var elli2Trans = elli_ani_2.RenderTransform as ScaleTransform;
            var elli3Trans = elli_ani_3.RenderTransform as ScaleTransform;
            var contentTrans = grid_ani_content.RenderTransform as ScaleTransform;

            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);


            rect_ani_back.Opacity = 0;
            elli_ani_1.Opacity = 0; elli_ani_2.Opacity = 0; elli_ani_3.Opacity = 0; grid_ani_content.Opacity = 0;

            elli1Trans?.ScaleX = 1.5; elli1Trans?.ScaleY = 1.5;
            elli2Trans?.ScaleX = 2; elli2Trans?.ScaleY = 2;
            elli3Trans?.ScaleX = 2.5; elli3Trans?.ScaleY = 2.5;
            contentTrans?.ScaleX = 1.5; contentTrans?.ScaleY = 1.5;
            grid_Animation.Visibility = Visibility.Visible;


            rect_ani_back.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            var elliScaleAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var elliOpacityAnimation = new DoubleAnimation
            {
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);

            elli_ani_1.BeginAnimation(OpacityProperty, elliOpacityAnimation);
            elli_ani_2.BeginAnimation(OpacityProperty, elliOpacityAnimation);
            elli_ani_3.BeginAnimation(OpacityProperty, elliOpacityAnimation);

            await Task.Delay(100);

            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);
            grid_ani_content.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            //退出动画

            await Task.Delay(800);

            var opacityAniOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var scaleAniOut = new DoubleAnimation
            {
                To = 3,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            rect_ani_back.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_1.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_2.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_3.BeginAnimation(OpacityProperty, opacityAniOut);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);

            grid_ani_content.BeginAnimation(OpacityProperty, opacityAniOut);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);

            return;
        }
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {


                if (!string.IsNullOrEmpty(AppGlobals.Config.CurrentGame))
                {


                    //查找选择游戏信息
                    foreach (var game in AppGlobals.Indexes.GameList)
                        if (game.GameInfo.Name == AppGlobals.Config.CurrentGame)
                            currentGameInfo = game;

                    //设置按钮文本
                    textBlock_LaunchVersion.Text = AppGlobals.Config.CurrentGame;

                }
                else
                {
                    button_Launch.IsEnabled = false;
                    textBlock_LaunchVersion.Text = "请选择一个游戏";
                }

                if (!string.IsNullOrEmpty(AppGlobals.Config.CurrentTrainer))
                {

                    foreach (var trainer in AppGlobals.Indexes.TrainerList)
                        if (trainer.Name == AppGlobals.Config.CurrentTrainer)
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
                if (AppGlobals.Config.Settings.LauncherConfig.BackgroundMode == "custom" && !string.IsNullOrEmpty(AppGlobals.Config.Settings.LauncherConfig.Background))
                    image.Source = new BitmapImage(new Uri(AppGlobals.Config.Settings.LauncherConfig.Background));

                /*//回声洞
                if (AppGlobals.Config.Settings.LauncherConfig.EchoCaveEnabled)
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
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Loaded += ((sender, e) => Initialize());
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

                    if (AppGlobals.Config.Settings.LauncherConfig.LaunchAnimationEnabled)
                        await StartLaunchAnimation();

                    //切换存档
                    if (AppGlobals.Config.Settings.SaveConfig.EnableSaveIsolation && Directory.Exists(Path.Combine(AppGlobals.Directories.GameDirectory, AppGlobals.Config.CurrentGame, ".save")))
                    {

                        await GameManager.SwitchGameSave(currentGameInfo);

                    }

                    //启动游戏
                    GameManager.LaunchGame(currentGameInfo, (async () =>
                    {
                        SnackbarManager.Show(new SnackbarContent
                        {
                            Title = "提示",
                            Content = $"游戏进程退出, 退出代码: {GameManager.GameProcess.ExitCode}",
                            Type = SnackbarType.Warn
                        });

                        textBlock_LaunchText.Text = GetLoc("I18N.PageLaunch", "LaunchGame");

                        //保存存档
                        if (AppGlobals.Config.Settings.SaveConfig.EnableSaveIsolation && Directory.Exists(AppGlobals.Directories.SaveDirectory))
                        {

                            await GameManager.SaveGameSave(currentGameInfo);

                        }
                    }));

                    //启动修改器(如果有)
                    if (AppGlobals.Config.Settings.LauncherConfig.LaunchWithTrainer && !string.IsNullOrEmpty(AppGlobals.Config.CurrentTrainer))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(AppGlobals.Directories.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                            UseShellExecute = true
                        });
                    }

                    /*//启动提示
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"{AppGlobals.Config.CurrentGame} 启动成功!",
                        Type = SnackbarType.Success
                    });*/
                }
                //运行就结束
                else if (GameManager.IsGameRuning == true)
                {

                    textBlock_LaunchText.Text = GetLoc("I18N.PageLaunch", "LaunchGame");

                    await GameManager.KillGame((() =>
                    {
                        /*SnackbarManager.Show(new SnackbarContent
                        {
                            Title = "结束游戏",
                            Content = "成功结束游戏",
                            Type = SnackbarType.Success
                        });*/
                    }), (() =>
                    {
                        SnackbarManager.Show(new SnackbarContent
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
                    FileName = System.IO.Path.Combine(AppGlobals.Directories.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                    UseShellExecute = true,
                    WorkingDirectory = Path.Combine(AppGlobals.Directories.TrainerDirectory, currentTrainerInfo.Name)
                });
                SnackbarManager.Show(new SnackbarContent
                {
                    Title = "提示",
                    Content = $"{AppGlobals.Config.CurrentTrainer} 启动成功!",
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
