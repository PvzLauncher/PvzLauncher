using MdXaml;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Game;
using PvzLauncherRemake.Utils.Network;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using Wpf.Ui;

namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        public Dictionary<string, Type> PageMap = new Dictionary<string, Type>();//Page预加载
        private NavigationTransitionInfo FrameAnimation = new DrillInNavigationTransitionInfo();//Frame切换动画
        public ISnackbarService _snackbarService;

        #region Init
        public async void Initialize()
        {
            try
            {
                //加载列表
                await GameManager.LoadGameListAsync();
                await GameManager.LoadTrainerListAsync();

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

                //初始化教程Host
                TutorialManager.Initialize(tuhost);

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

                _snackbarService = new Wpf.Ui.SnackbarService();
                _snackbarService.SetSnackbarPresenter(snackbarPersenter);

                //特殊日期
                var now = DateTimeOffset.Now;
                if (now.Month == 1 && now.Day == 1)//元旦
                    ThemeManager.Current.AccentColor = Color.FromRgb(255, 150, 150);
                if (now.Month == 4 && now.Day == 1)//愚人节
                    ThemeManager.Current.AccentColor = Color.FromRgb(150, 255, 150);

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
            Loaded += (async (s, e) =>
            {
                try
                {
                    //参数
                    if (Globals.Arguments.isUpdate)//更新启动
                    {
                        await DialogService.ShowDialogAsync(new ContentDialog
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
                        textBlock_buildWaterMark.Visibility = Visibility.Visible;
                        textBlock_buildWaterMark.Text = "此版本由 CI 自动构建，仅供测试使用，非正式发布版。(点击此关闭水印)";
                    }
                    else if (Globals.Arguments.isDebugBuild)//DEBUG
                    {
                        textBlock_buildWaterMark.Visibility = Visibility.Visible;
                        textBlock_buildWaterMark.Text = "此版本为 调试 版本，仅测试使用，非正式发布版。(点击此关闭水印)";
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

                        await DialogService.ShowDialogAsync(new ContentDialog
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
                    NoticeService.FetchNotices();
                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
            });

            this.SizeChanged += ((sender, e) =>
            {
                Globals.Config.WindowSize = new JsonConfig.WindowSize { Width = this.Width, Height = this.Height };
                ConfigManager.SaveConfig();
            });

            textBlock_buildWaterMark.MouseUp += (s, e) => textBlock_buildWaterMark.Visibility = Visibility.Collapsed;


            bool _isClose = false;

            Closing += async (s, e) =>
            {
                if (_isClose)
                    return;

                if (GameManager.IsGameRuning && Globals.Config.Settings.SaveConfig.EnableSaveIsolation)
                {
                    e.Cancel = true;
                    await DialogService.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = "游戏运行时关闭启动器会导致隔离存档与当前存档不同步，在下次启动游戏时会丢失进度。确定退出？",
                        PrimaryButtonText = "留在启动器",
                        CloseButtonText = "仍然退出",
                        DefaultButton = ContentDialogButton.Primary
                    }, null, null, () =>
                    {
                        _isClose = true;
                        this.Close();
                    });
                }
            };

            PreviewKeyUp += (s, e) =>
            {
                if (navView.IsBackEnabled != true || navView.IsBackButtonVisible != NavigationViewBackButtonVisible.Visible)
                    return;
                if (e.Key != System.Windows.Input.Key.Escape)
                    return;

                navView_BackRequested(navView, null!);
            };

            //拖放支持
            DragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                    //仅允许文件夹拖放
                    if (paths.Length == 1 && Directory.Exists(paths[0]))
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }

                e.Effects = DragDropEffects.None;
                e.Handled = true;
            };
            Drop += async (s, e) =>
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return;

                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (paths.Length != 1 || !Directory.Exists(paths[0]))
                    return;

                string folderPath = paths[0];

                SetLoadState(true, "导入中...");
                await GameManager.ImportGameOrTrainer((s) => SetLoadText($"正在导入: {s}"), folderPath);
                SetLoadState(false);
            };
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

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) =>
            frame.GoBack();


        public void SetLoadState(bool state, string tipText = "加载中...")
        {
            navView.IsEnabled = !state;
            grid_loadScreen.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            textBlock_loadScreen.Text = tipText;

            navView.Effect = state ? new BlurEffect { Radius = 10 } : null;
        }

        public void SetLoadText(string tipText) => textBlock_loadScreen.Text = tipText;



    }
}