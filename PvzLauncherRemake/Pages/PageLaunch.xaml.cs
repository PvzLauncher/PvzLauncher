using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageLaunch.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunch : ModernWpf.Controls.Page
    {
        private JsonGameInfo.Index currentGameInfo = null!;
        private NotificationManager notifi = new NotificationManager();

        #region Animation
        public void StartTitleAnimation(double gridHeight, double timeMs = 500)
        {
            var animation = new ThicknessAnimationUsingKeyFrames();
            animation.Duration = TimeSpan.FromMilliseconds(timeMs);
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(
                new Thickness(0, -10 - gridHeight, 0, 0),
                KeyTime.FromTimeSpan(TimeSpan.Zero)));

            var easingKeyFrame = new EasingThicknessKeyFrame(
                new Thickness(0, 0, 0, 0),
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timeMs)))
            {

                // 1. 快速滑入并轻微回弹
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 }

                // 2. 平滑强减速
                // EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }

                // 3. 弹性弹跳效果
                // EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 8 }

                // 4. 先慢后快再减速
                // EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }

                // 5. 经典 Power 缓动
                // EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 4 }
            };

            animation.KeyFrames.Add(easingKeyFrame);
            animation.FillBehavior = FillBehavior.HoldEnd;
            grid_Title.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }
        #endregion

        #region Init
        public void Initialize() { }
        public async void InitializeLoaded()
        {
            try
            {
                if (!string.IsNullOrEmpty(AppInfo.Config.CurrentGame))
                {
                    logger.Info($"当前选择游戏: {AppInfo.Config.CurrentGame}");
                    //查找选择游戏信息
                    foreach (var game in AppInfo.GameList)
                        if (game.GameInfo.Name == AppInfo.Config.CurrentGame)
                            currentGameInfo = game;

                    //设置按钮文本
                    textBlock_LaunchVersion.Text = AppInfo.Config.CurrentGame;

                }
                else
                {
                    logger.Info("没有检测到选择游戏，禁用按钮");
                    button_Launch.IsEnabled = false;
                    textBlock_LaunchVersion.Text = "请选择一个游戏";
                }

                //判断游戏是否运行
                try
                {
                    if (AppProcess.Process != null && AppProcess.Process.Id != 0 && !AppProcess.Process.HasExited)
                    {
                        logger.Info("检测到游戏进程仍在运行...");
                        textBlock_LaunchText.Text = "结束进程";
                        
                    }
                }
                catch (InvalidOperationException) { }


                //播放动画
                grid_Title.Margin = new Thickness(0, -10 - grid_Title.Height, 0, 0);
                await Task.Delay(200);//等待Frame动画播放完毕
                StartTitleAnimation(grid_Title.Height,500);

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageLaunch 发生错误", ex);
            }
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Initialize();
            Loaded += ((sender, e) => InitializeLoaded());
        }

        private async void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //没运行就启动
                if (textBlock_LaunchText.Text == "启动游戏")
                {
                    textBlock_LaunchText.Text = "结束进程";

                    logger.Info("游戏开始启动...");
                    logger.Info($"当前游戏: {AppInfo.Config.CurrentGame}");
                    //游戏exe路径
                    string gameExePath = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name, currentGameInfo.GameInfo.ExecuteName);

                    logger.Info($"游戏exe路径: {gameExePath}");

                    //定义Process
                    AppProcess.Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = gameExePath,
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name)
                        }
                    };

                    //启动
                    AppProcess.Process.Start();
                    logger.Info($"进程启动完毕");

                    //启动提示
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"{AppInfo.Config.CurrentGame} 启动成功!",
                        Type = NotificationType.Information
                    });

                    //等待结束
                    logger.Info("等待进程退出...");

                    await AppProcess.Process.WaitForExitAsync();
                    logger.Info($"进程退出, ExitCode: {AppProcess.Process.ExitCode}");
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"游戏进程退出, 退出代码: {AppProcess.Process.ExitCode}",
                        Type = NotificationType.Warning
                    });

                    textBlock_LaunchText.Text = "启动游戏";
                }
                //运行就结束
                else if (textBlock_LaunchText.Text == "结束进程")
                {
                    logger.Info($"用户手动结束进程中...");
                    textBlock_LaunchText.Text = "启动游戏";

                    //尝试使程序自行退出
                    if (!AppProcess.Process.HasExited)
                    {
                        notifi.Show(new NotificationContent
                        {
                            Title = "提示",
                            Message = "正在尝试关闭游戏...",
                            Type = NotificationType.Information
                        });
                        AppProcess.Process.CloseMainWindow();
                        //等待自己关闭
                        await Task.Delay(1000);

                        //强制关
                        if (!AppProcess.Process.HasExited)
                        {
                            AppProcess.Process.Kill();
                            //等待完全关闭
                            await Task.Delay(1000);
                        }

                        if (!AppProcess.Process.HasExited)
                        {
                            //都Kill()了不能再关不上吧
                            notifi.Show(new NotificationContent
                            {
                                Title = "失败",
                                Message = "我们无法终止您的游戏，请您自行退出",
                                Type = NotificationType.Error
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "启动游戏时发生错误", ex);
            }
        }
    }
}
