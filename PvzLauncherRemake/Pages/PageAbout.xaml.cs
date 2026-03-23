using HuaZi.Library.Json;
using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageAbout.xaml 的交互逻辑
    /// </summary>
    public partial class PageAbout : ModernWpf.Controls.Page
    {
        private long _eggCount = 0;

        private static readonly IReadOnlyList<(int clicks, string title, string message, SnackbarType type, Action? action)>
            EasterEggs =
        [
            (10,  "香蒲", "你真的很无聊...",                               SnackbarType.Info, null),
            (20,  "香蒲", "不是我说，你无聊的话可以去干其他事，能不能不要点我了", SnackbarType.Info, null),
            (40,  "香蒲", "不 要 再 点 我 了 ! ! !",                   SnackbarType.Warn,       null),
            (70,  "香蒲", "你可以去干一些有意义的事情，而不是在这里点一堆矢量路径！！！", SnackbarType.Error, null),
            (100, "香蒲", "好了，到此为止。作者只做了100次点击的判断，后面没有了",     SnackbarType.Success,      null),
            (130, "发生错误", "System.IndexOutOfRangeException: 索引超出了数组的边界。\r\n   在 Program.Main() 位置 C:\\Projects\\ArrayDemo\\Program.cs:第 11 行\r\n   在 System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)\r\n   在 System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr)",SnackbarType.Error, null),
            (150, "香蒲", "看来骗不到你",                               SnackbarType.Info, null),
            (200, "香蒲", "恭喜！200次点击",                           SnackbarType.Info, null),
            (250, "香蒲", "好了，这次是真的没了，最大值就是250了。快走吧。", SnackbarType.Info, null)
        ];

        public PageAbout()
        {
            InitializeComponent();

            textBlock_Version.Text = $"{AppGlobals.Version}{(AppGlobals.Arguments.isCIBuild ? " - CI" : AppGlobals.Arguments.isDebugBuild ? " - Debug" : null)}";



            //获取赞助名单
            GetSponsorList();
        }

        private async void GetSponsorList()
        {
            if (AppGlobals.Config.Settings.LauncherConfig.OfflineMode)
            {
                stackpanel_SponsorList.Children.Clear();
                stackpanel_SponsorList.Children.Add(new TextBlock
                {
                    Text = "离线模式已启用，无法获取赞助者列表",
                    Margin = new Thickness(0, 0, 0, 5)
                });
                return;
            }


            string sponsorIndexUrl = "https://gitee.com/huamouren110/PvzLauncher.Service/raw/main/sponsors/index.json";
            string[] sponsorList;
            using (var client = new HttpClient())
                sponsorList = Json.ReadJson<string[]>(await client.GetStringAsync(sponsorIndexUrl));

            stackpanel_SponsorList.Children.Clear();
            foreach (var sponsor in sponsorList)
            {
                var tb = new TextBlock
                {
                    Text = sponsor,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                stackpanel_SponsorList.Children.Add(tb);
            }
        }


        public void GoToUrl(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                logger.Info($"[关于] 跳转Url => {button.Tag}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = button.Tag.ToString(),
                    UseShellExecute = true
                });
            }
        }

        public void GoToHelpCenter(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                this.NavigationService.Navigate(new PageHelp());
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _eggCount++;
            logger.Info($"[关于] 触发彩蛋，当前点击次数: {_eggCount}");

            foreach (var (clicks, title, message, type, action) in EasterEggs)
            {
                if (_eggCount == clicks)
                {
                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = title,
                        Content = message,
                        Type = type
                    });
                    action?.Invoke();
                    return;
                }
            }
        }

        private async void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                logger.Info($"[关于] 用户尝试进入控制台");

                if (Debugger.IsAttached)
                {
                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = "开发者控制台",
                        Content = "检测到调试器附加，自动进入开发者控制台",
                        Type = SnackbarType.Success
                    });
                    NavigationService?.Navigate(new PageDeveloper());
                    return;
                }

                var textBox = new TextBox();
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "开发者控制台",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text="您正在进入开发者控制台，为避免意外，请输入Int32最大值与最小值的和",
                                Margin=new Thickness(0,0,0,5)
                            },
                            textBox
                        }
                    },
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() =>
                {
                    if (textBox.Text == (Int32.MaxValue + Int32.MinValue).ToString())
                    {
                        NavigationService?.Navigate(new PageDeveloper());
                    }
                    else
                    {
                        SnackbarManager.Show(new SnackbarContent
                        {
                            Title = "答案错误",
                            Content = $"您无法进入开发者控制台, \"{textBox.Text}\" 是错误的！",
                            Type = SnackbarType.Error
                        });
                    }
                }));

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        private async void button_Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button_Update.IsEnabled = false;

                await Updater.CheckUpdate();


            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
            finally
            {
                button_Update.IsEnabled = true;
            }
        }

        private async void button_Sponsor_Click(object sender, RoutedEventArgs e)
        {
            string qrcodePath = Path.Combine(AppGlobals.ExecuteDirectory, "Assets", "Images", "sponsor_qrcode.png");
            if (!File.Exists(qrcodePath))
                throw new FileNotFoundException("文件不存在", qrcodePath);
            var bitmap = new BitmapImage(new Uri(qrcodePath));
            var dialog = new ContentDialog
            {
                Title = "赞助",
                Content = new UserScrollViewer
                {
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock{Text="感谢您对 PvzLauncher 的赞助，您可以在赞助备注填写您的名称。我们会将您的名称列在程序的关于页内",TextWrapping=TextWrapping.Wrap},
                            new Image{Source=bitmap}
                        }
                    }
                },
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close
            };
            await DialogManager.ShowDialogAsync(dialog);
        }
    }
}
