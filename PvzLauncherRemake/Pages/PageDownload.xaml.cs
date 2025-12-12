using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static PvzLauncherRemake.Class.AppLogger;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownload.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownload : ModernWpf.Controls.Page
    {
        private JsonDownloadIndex.Index DownloadIndex = null!;

        #region AddCard
        private void AddGameCard(ListBox listBox, JsonDownloadIndex.GameInfo[] gameInfos)
        {
            foreach (var gameInfo in gameInfos)
            {
                var card = new UserCard
                {
                    Title = gameInfo.Name,
                    Description = gameInfo.Description,
                    Icon =
                    gameInfo.Version.StartsWith("β") ? "Beta" :
                    gameInfo.Version.StartsWith("TAT", StringComparison.OrdinalIgnoreCase) ? "Tat" : "Origin",
                    Version = gameInfo.Version,
                    Size = gameInfo.Size.ToString(),
                    isNew = gameInfo.IsNew,
                    isRecommend = gameInfo.IsRecommend,
                    Tag = gameInfo
                };
                logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                listBox.Items.Add(card);
            }
        }
        private void AddTrainerCard(ListBox listBox, JsonDownloadIndex.TrainerInfo[] trainerInfos)
        {
            foreach (var trainerInfo in trainerInfos)
            {
                var card = new UserCard
                {
                    Title = trainerInfo.Name,
                    Description = trainerInfo.Description,
                    Icon = "Origin",
                    Version = trainerInfo.Version,
                    Size = trainerInfo.Size.ToString(),
                    SupportVersion = trainerInfo.SupportVersion,
                    isNew = trainerInfo.IsNew,
                    isRecommend = trainerInfo.IsRecommend,
                    Tag = trainerInfo
                };
                logger.Info($"[下载] 添加修改器卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                listBox.Items.Add(card);
            }
        }
        #endregion

        #region Load
        public void StartLoad(bool showProgressBar = false)
        {
            tabControl_Main.IsEnabled = false;
            tabControl_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Loading.Visibility = Visibility.Visible;
            if (showProgressBar)
                progressBar_Loading.Visibility = Visibility.Visible;
        }
        public void EndLoad()
        {
            tabControl_Main.IsEnabled = true;
            tabControl_Main.Effect = null;
            grid_Loading.Visibility = Visibility.Hidden;
            progressBar_Loading.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {
                logger.Info($"[下载] 开始初始化...");
                StartLoad();

                using (var client = new HttpClient())
                {
                    string indexString = await client.GetStringAsync(AppInfo.DownloadIndexUrl);
                    logger.Info($"[下载] 获取下载索引: {indexString}");
                    DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(indexString);
                }

                //中文原版
                logger.Info($"[下载] 开始加载中文原版游戏列表");
                listBox_zhOrigin.Items.Clear();
                listBox_zhRevision.Items.Clear();
                listBox_enOrigin.Items.Clear();
                listBox_trainer.Items.Clear();

                AddGameCard(listBox_zhOrigin, DownloadIndex.ZhOrigin);
                AddGameCard(listBox_zhRevision, DownloadIndex.ZhRevision);
                AddGameCard(listBox_enOrigin, DownloadIndex.EnOrigin);
                AddTrainerCard(listBox_trainer, DownloadIndex.Trainer);

                EndLoad();
                logger.Info($"[下载] 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageDownload 发生错误", ex);
            }
        }
        #endregion


        //Tab动画
        private void tabControl_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                if (e.OriginalSource != sender)
                    return;

                var selectItem = ((TabControl)sender).SelectedContent;
                ListBox animControl = null!;

                if (selectItem is ListBox)
                {
                    animControl = (ListBox)selectItem;
                }
                else if (selectItem is TabControl tabcontrol && tabcontrol.SelectedContent is ListBox)
                {
                    animControl = (ListBox)tabcontrol.SelectedContent;
                }
                else
                {
                    return;
                }

                animControl.BeginAnimation(MarginProperty, null);
                animControl.BeginAnimation(OpacityProperty, null);

                animControl.Margin = new Thickness(0, 25, 0, 0);
                animControl.Opacity = 0;

                var margniAnim = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var opacAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                animControl.BeginAnimation(MarginProperty, margniAnim);
                animControl.BeginAnimation(OpacityProperty, opacAnim);
            }
        }

        public PageDownload() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e) => Initialize();

        private async void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lisbox || lisbox.SelectedItem is not UserCard card) return;

            bool isTrainer = lisbox.Tag?.ToString() == "trainer";
            var info = isTrainer ? (JsonDownloadIndex.TrainerInfo)card.Tag : (JsonDownloadIndex.GameInfo)card.Tag;
            string baseDirectory =
                isTrainer ? AppInfo.TrainerDirectory :
                AppInfo.GameDirectory;

            //确认下载
            bool confirm = false;
            await DialogManager.ShowDialogAsync(new ContentDialog
            {
                Title = "下载确认",
                Content = $"是否下载 \"{info.Name}\"",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => confirm = true));
            if (!confirm) return;

            //处理同名
            string savePath = await GameManager.ResolveSameName(info.Name, baseDirectory);

            //判断Downloader是否占用
            if (AppDownloader.downloader != null)
            {
                new NotificationManager().Show(new NotificationContent
                {
                    Title = "启动下载任务失败",
                    Message = "已经有一个下载任务在执行，请等待任务完成",
                    Type = NotificationType.Error
                });
                return;
            }


            //开始下载
            await StartDownloadAsync(info, savePath, isTrainer);
        }

        private async Task StartDownloadAsync(dynamic info, string savePath, bool isTrainer)
        {
            StartLoad(true);

            string tempPath = Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERDOWNLOADCACHE");

            try
            {
                //清除残留
                if (File.Exists(tempPath))
                    await Task.Run(() => File.Delete(tempPath));

                bool? isDownloadComplete = null;
                string errorMessage = "";
                //定义下载器
                AppDownloader.downloader = new Downloader
                {
                    Url = info.Url,
                    SavePath = tempPath,
                    Progress = ((p, s) =>
                    {
                        progressBar_Loading.Value = p;
                        textBlock_Loading.Text = $"下载中 {Math.Round(p, 2)}% ({Math.Round(s / 1024, 2)}MB/s)";
                    }),
                    Completed = ((s, e) =>
                    {
                        if (s)
                            isDownloadComplete = true;
                        else
                        {
                            isDownloadComplete = false;
                            errorMessage = e!;
                        }

                    })
                };
                AppDownloader.downloader.StartDownload();

                //等待下载完毕
                while (isDownloadComplete == null)
                    await Task.Delay(1000);

                AppDownloader.downloader = null;

                //失败抛错误
                if (isDownloadComplete == false)
                    throw new Exception(errorMessage);

                textBlock_Loading.Text = $"解压中...";

                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                //解压
                await Task.Run(() =>
                {
                    ArchiveFactory.WriteToDirectory(tempPath, savePath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                });


                string configName = Path.GetFileName(savePath);
                if (!isTrainer)
                {
                    var cfg = new JsonGameInfo.Index
                    {
                        GameInfo = new JsonGameInfo.GameInfo
                        {
                            ExecuteName = info.ExecuteName,
                            Version = info.Version,
                            VersionType = info.VersionType,
                            Name = configName
                        },
                        Record = new JsonGameInfo.Record
                        {
                            FirstPlay = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            PlayCount = 0,
                            PlayTime = 0
                        }
                    };
                    Json.WriteJson(Path.Combine(savePath, ".pvzl.json"), cfg);
                    AppInfo.Config.CurrentGame = configName;
                }
                else
                {
                    var cfg = new JsonTrainerInfo.Index
                    {
                        ExecuteName = info.ExecuteName,
                        Version = info.Version,
                        Name = configName
                    };
                    Json.WriteJson(Path.Combine(savePath, ".pvzl.json"), cfg);
                    AppInfo.Config.CurrentTrainer = configName;
                }

                new NotificationManager().Show(new NotificationContent
                {
                    Title = "下载完成",
                    Message = $"\"{configName}\" 已添加进您的库内",
                    Type = NotificationType.Success
                });


                NavigationController.Navigate(this, "Manage");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }

            EndLoad();
        }
    }
}
