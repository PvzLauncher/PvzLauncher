using HuaZi.Library.Json;
using MdXaml;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Wpf.Ui;


namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private Dictionary<string, Type> PageMap = new Dictionary<string, Type>();//Page预加载
        private NavigationTransitionInfo FrameAnimation = new DrillInNavigationTransitionInfo();//Frame切换动画
        public ISnackbarService _snackbarService;

        #region Init
        public async void Initialize()
        {
            try
            {
                //应用配置
                this.Title = Globals.Config.Settings.LauncherConfig.WindowTitle;
                this.Width = Globals.Config.WindowSize.Width;
                this.Height = Globals.Config.WindowSize.Height;
                switch (Globals.Config.Settings.LauncherConfig.NavigationViewAlign)
                {
                    case "Left":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact; break;
                    case "Top":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top; break;
                }

                //注册事件

                this.SizeChanged += ((sender, e) =>
                {

                    Globals.Config.WindowSize = new JsonConfig.WindowSize { Width = this.Width, Height = this.Height };
                    ConfigManager.SaveConfig();
                });

                //预加载Page
                void AddType(Type t)
                {
                    PageMap.Add($"{t.Name}", t);

                }
                AddType(typeof(PageLaunch));
                AddType(typeof(PageManage));
                AddType(typeof(PageDownload));

                AddType(typeof(PageTask));
                AddType(typeof(PageSettings));
                AddType(typeof(PageAbout));

                //选择默认页
                navView.SelectedItem = navViewItem_Launch;


                //禁用联网
                if (Globals.Config.Settings.LauncherConfig.OfflineMode)
                {
                    navViewItem_Download.IsEnabled = false;
                    navViewItem_Task.IsEnabled = false;
                }

                _snackbarService = new SnackbarService();
                _snackbarService.SetSnackbarPresenter(snackbarPersenter);


            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
        #endregion

        public WindowMain()
        {
            InitializeComponent();
            Initialize();
            Loaded += ((s, e) =>
            {
                Dispatcher.BeginInvoke((async () =>
                {
                    try
                    {




                        //是否CI构建
#if CI
                Globals.Arguments.isCIBuild = true;
#endif
                        //是否Debug构建
#if DEBUG
                        Globals.Arguments.isDebugBuild = true;
#endif


                        //处理启动参数
                        string[] args = Environment.GetCommandLineArgs();
                        foreach (var arg in args)
                        {

                            switch (arg)
                            {
                                //外壳启动
                                case "-shell":
                                    Globals.Arguments.isShell = true; break;
                                //更新启动，显示更新完毕对话框
                                case "-update":
                                    Globals.Arguments.isUpdate = true; break;
                            }
                        }










                        //参数检测
                        if (!Globals.Arguments.isShell && !Debugger.IsAttached)//是否外壳启动
                        {
                            await DialogManager.ShowDialogAsync(new ContentDialog
                            {
                                Title = "警告",
                                Content = "检测到程序非外壳启动, 此启动方式可能会导致某些意外的事情发生",
                                PrimaryButtonText = "改用外壳启动",
                                CloseButtonText = "忽略",
                                DefaultButton = ContentDialogButton.Primary
                            }, (() =>
                            {
                                //Primary=>改用外壳启动
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = System.IO.Path.Combine(Globals.Directories.RootDirectory, "PvzLauncher.exe"),
                                    UseShellExecute = true
                                });
                                Environment.Exit(0);
                            }));
                        }
                        if (Globals.Arguments.isUpdate)//更新启动
                        {
                            await DialogManager.ShowDialogAsync(new ContentDialog
                            {
                                Title = "更新完毕",
                                Content = $"您已更新到最新版 {Globals.Version} , 尽情享受吧！",
                                PrimaryButtonText = "确定",
                                DefaultButton = ContentDialogButton.Primary
                            });
                        }


                        //构建检测
                        if (Globals.Arguments.isCIBuild)//CI
                        {
                            SnackbarManager.Show(new SnackbarContent
                            {
                                Content = $"您使用的是基于 {Globals.Version} 构建的CI版本\nCI构建是每个提交自动生成的，稳定性无法得到保证，因此仅用于测试使用\n\n如果使用CI版本出现了BUG请不要反馈给开发者!",
                                Title = "警告",
                                Type = SnackbarType.Warn
                            });
                        }
                        else if (Globals.Arguments.isDebugBuild)//DEBUG
                        {
                            SnackbarManager.Show(new SnackbarContent
                            {
                                Content = $"您使用的是您自行构建的版本，此版本的稳定性与安全性无法得到保证，如果你自己改动代码导致了BUG，请不要反馈给开发者!",
                                Title = "警告",
                                Type = SnackbarType.Warn
                            });
                        }




                        //EULA检测
                        if (!Globals.Config.Eula)
                        {
                            string eulaPath = Path.Combine(Globals.Directories.ExecuteDirectory, "Resources", "Documents", "EULA.md");
                            string eulaText = $"无法加载{eulaPath}";
                            eulaText = await File.ReadAllTextAsync(eulaPath);

                            var docViewer = new FlowDocumentScrollViewer
                            {
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                            };
                            docViewer.Document = new Markdown().Transform(eulaText);
                            docViewer.Document.FontFamily = new FontFamily("Microsoft YaHei UI");

                            await DialogManager.ShowDialogAsync(new ContentDialog
                            {
                                Title = "请阅读并同意《Plants Vs. Zombies Launcher - 最终用户许可协议》",
                                Content = docViewer,
                                PrimaryButtonText = "同意",
                                CloseButtonText = "拒绝",
                                DefaultButton = ContentDialogButton.Primary
                            }, (() => Globals.Config.Eula = true), null, (() => Environment.Exit(0)));
                            ConfigManager.SaveConfig();
                        }





                        //检查更新
                        if (Globals.Config.Settings.LauncherConfig.StartUpCheckUpdate)
                        {

                            await Updater.CheckUpdate(null!, true);
                        }





                        //公告获取
                        if (Globals.Config.Settings.LauncherConfig.NoticeEnabled && !Globals.Config.Settings.LauncherConfig.OfflineMode)
                        {
                            JsonNoticeIndex.Root noticeIndex;
                            using (var client = new HttpClient())
                                noticeIndex = Json.ReadJson<JsonNoticeIndex.Root>(await client.GetStringAsync(Globals.Urls.NoticeIndexUrl));

                            foreach (var notice in noticeIndex.Notices)
                            {
                                string content = "";
                                foreach (var contentL in notice.Contents)
                                {
                                    content = $"{content}{contentL}\n";
                                }

                                var chkBox = new CheckBox { Content = "不再显示此公告", IsChecked = false };
                                if (!Globals.Config.Settings.LauncherConfig.HiddenNotices.Contains(notice.Title))
                                    await DialogManager.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = notice.Title,
                                        Content = new StackPanel
                                        {
                                            Children =
                                {
                                    new TextBlock{Text = content,TextWrapping=TextWrapping.Wrap},
                                    chkBox
                                }
                                        },
                                        PrimaryButtonText = notice.PrimaryButton,
                                        SecondaryButtonText = notice.SecondaryButton,
                                        CloseButtonText = "关闭",
                                        DefaultButton = ContentDialogButton.Primary
                                    }, (() => handleButtonActions(notice.PrimaryActions)
                                    ), (() => handleButtonActions(notice.SecondaryActions)));

                                void handleButtonActions(JsonNoticeIndex.ButtonActionInfo[] actions)
                                {
                                    foreach (var action in actions)
                                    {
                                        switch (action.Type)
                                        {
                                            case "to-url":
                                                Process.Start(new ProcessStartInfo
                                                {
                                                    FileName = action.Url,
                                                    UseShellExecute = true
                                                });
                                                break;
                                            case "to-page":
                                                if (Enum.TryParse<NavigaionPages>(action.Url, true, out NavigaionPages result))
                                                    NavigationController.Navigate(result);
                                                else
                                                    throw new Exception($"目标页: \"{action.Url}\" 不存在，这是开发者编写失误引起的，请联系开发者");
                                                break;
                                            default:
                                                throw new Exception($"未知的操作类型: \"{action.Type}\"。这一般是编写失误或当前启动器版本过低导致的");
                                        }
                                    }
                                }


                                if (chkBox.IsChecked == true)
                                    Globals.Config.Settings.LauncherConfig.HiddenNotices.Add(notice.Title);

                                ConfigManager.SaveConfig();
                            }
                        }



                    }
                    catch (Exception ex)
                    {
                        ErrorReportDialog.Show(ex);
                    }
                }), System.Windows.Threading.DispatcherPriority.Normal);
            });
        }

        private void navView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (navView.SelectedItem is NavigationViewItem item)
                {


                    frame.Navigate(PageMap[$"Page{item.Tag}"], null, FrameAnimation);
                }
                else
                    throw new Exception($"非法的项: {navView.SelectedItem}");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                //判断是否显示返回箭头
                if (frame.Content is ModernWpf.Controls.Page page && page.Tag != null && page.Tag.ToString() == "sub")
                {
                    navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
                    navView.IsBackEnabled = true;
                }
                else
                {
                    navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
                    navView.IsBackEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            frame.GoBack();
        }
    }
}