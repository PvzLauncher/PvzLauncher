using HuaZi.Library.Json;
using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageAbout.xaml 的交互逻辑
    /// </summary>
    public partial class PageAbout : ModernWpf.Controls.Page
    {
        public PageAbout()
        {
            InitializeComponent();

            textBlock_Version.Text = $"{Globals.Version}{(Globals.Arguments.isCIBuild ? " - CI" : Globals.Arguments.isDebugBuild ? " - Debug" : null)}";



            //获取赞助名单
            GetSponsorList();




            //图标动画

            icon.MouseLeftButtonUp += async (s, e) =>
            {
                if (icon.RenderTransform is not RotateTransform rt)
                    return;

                //旋转
                var rtAni = new DoubleAnimation
                {
                    From = rt.Angle,
                    To = rt.Angle + new Random().Next(90, 360),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };

                rt.BeginAnimation(RotateTransform.AngleProperty, null);
                rt.BeginAnimation(RotateTransform.AngleProperty, rtAni);
            };

        }

        private async void GetSponsorList()
        {
            if (Globals.Config.Settings.LauncherConfig.OfflineMode)
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = button.Tag.ToString(),
                    UseShellExecute = true
                });
            }
        }

        private async void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
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
            string qrcodePath = Path.Combine(Globals.Directories.ExecuteDirectory, "Resources", "Images", "sponsor_qrcode.png");
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
