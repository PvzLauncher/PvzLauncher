using ModernWpf.Controls;
using PvzLauncherRemake.Windows;
using System.Windows;
using static PvzLauncherRemake.Classes.AppLogger;

namespace PvzLauncherRemake.Utils.UI
{
    public enum DialogDisplayArea
    {
        Main,
        Overlay
    }

    public static class DialogManager
    {
        private static readonly SemaphoreSlim DialogSemaphore = new(1, 1);

        /// <summary>
        /// 将对话框添加到显示序列等待并显示
        /// </summary>
        /// <param name="dialog">对话框</param>
        /// <param name="primaryCallback">主要按钮点击回调</param>
        /// <param name="secondaryCallback">次要按钮点击回调</param>
        /// <param name="closeCallback">关闭按钮点击回调</param>
        /// <param name="displayArea">显示区域，如目标窗口未创建则回滚至主窗口</param>
        /// <returns>对话框结果</returns>
        public static async Task<ContentDialogResult> ShowDialogAsync(
            ContentDialog dialog,
            Action? primaryCallback = null,
            Action? secondaryCallback = null,
            Action? closeCallback = null,
            DialogDisplayArea displayArea = DialogDisplayArea.Main)
        {
            logger.Info($"[对话框管理器] 请求显示对话框 → {dialog.Title}");

            switch (displayArea)
            {
                //主窗口
                case DialogDisplayArea.Main:
                    dialog.Owner = Application.Current.MainWindow;
                    break;
                //游戏内覆盖窗口
                case DialogDisplayArea.Overlay:
                    dialog.Owner = Application.Current.Windows
                        .OfType<WindowOverlay>()
                        .FirstOrDefault() ?? Application.Current.MainWindow;
                    //如未创建则回滚至主窗口
                    break;
            }

            await DialogSemaphore.WaitAsync();
            try
            {
                var result = await dialog.ShowAsync();

                logger.Info($"[对话框管理器] 对话框关闭 ← {dialog.Title}，用户选择：{result}");

                // 执行回调
                switch (result)
                {
                    case ContentDialogResult.Primary: primaryCallback?.Invoke(); break;
                    case ContentDialogResult.Secondary: secondaryCallback?.Invoke(); break;
                    default: closeCallback?.Invoke(); break;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"[对话框管理器] 显示对话框异常: {ex}");
                return ContentDialogResult.None;
            }
            finally
            {
                DialogSemaphore.Release();
            }
        }
    }
}