using ModernWpf.Controls;
using PvzLauncherRemake.JsonModel.Network;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.UI;
using Serilog;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace PvzLauncherRemake.Utils.Network
{
    public static class NoticeService
    {
        private static readonly ILogger logger = Log.ForContext(typeof(NoticeService));

        public static async void FetchNotices()
        {
            logger.Information("尝试拉取公告...");
            if (!Globals.Config.Settings.LauncherConfig.NoticeEnabled || Globals.Config.Settings.LauncherConfig.OfflineMode)
            {
                logger.Warning("公告未启用或离线已开启，公告获取取消");
                return;
            }

            //GET
            JsonNotice.Root noticeIndex;
            using (var client = new HttpClient())
                noticeIndex = JsonHelper.ReadJson<JsonNotice.Root>(await client.GetStringAsync(Globals.Urls.NoticeIndexUrl));
            logger.Information($"获取到 {noticeIndex.Notices.Length} 个公告");


            //SHOW
            foreach (var notice in noticeIndex.Notices)
            {
                var now = DateTimeOffset.Now;
                var isNoticeHit =
                    !Globals.Config.Settings.LauncherConfig.HiddenNotices.Contains(notice.Title) &&
                    now <= notice.End && now >= notice.Start;

                if (!isNoticeHit)
                    continue;

                string content = string.Join("\n", notice.Contents);
                var chkBox = new CheckBox { Content = "不再显示此公告", IsChecked = false };

                logger.Information($"公告命中 {noticeIndex.Notices.Length} 中的 {noticeIndex.Notices.IndexOf(notice)}");
                logger.Information($"""

                    =====[{notice.Title}]=====
                    {content}
                    -----START={notice.Start}
                    -----END={notice.End}
                    -----hit?={isNoticeHit}

                    -----PrimaryButton={notice.PrimaryButton}
                    -----SecondaryButton={notice.SecondaryButton}
                    ====={new string('=', notice.Title.Length)}=====
                    """);
                await DialogService.ShowDialogAsync(new ContentDialog
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
                }, (() => HandleButtonActions(notice.PrimaryActions)
                ), (() => HandleButtonActions(notice.SecondaryActions)));


                if (chkBox.IsChecked == true)
                {
                    Globals.Config.Settings.LauncherConfig.HiddenNotices.Add(notice.Title);
                    logger.Information($"用户标记公告 \"{notice.Title}\" 为不再显示");
                }

                ConfigManager.SaveConfig();

            }
            //SHOW END


            logger.Information("公告流程结束");
        }

        private static void HandleButtonActions(JsonNotice.ButtonActionInfo[] actions)
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
    }
}
