using ModernWpf.Controls;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils.UI
{
    public static class ErrorReportDialog
    {
        public static async void Show(Exception ex, bool isUnHandleException = false)
        {
            logger.Error(
                $"{new string('=', 10)}ERROR{new string('=', 10)}\n" +
                $"{ex}\n" +
                $"{new string('=', 10)}ERROR{new string('=', 10)}");

            var dialog = new TaskDialog
            {
                WindowTitle = isUnHandleException ? "发生未捕获的错误" : "发生错误",
                MainIcon = TaskDialogIcon.Error,
                MainInstruction = isUnHandleException ? "程序在运行时发生了未捕获的错误" : "程序在运行时发生错误",
                Content = ex.ToString(),
                AllowDialogCancellation = false,
                ButtonStyle = TaskDialogButtonStyle.CommandLinks
            };


            var btnEnd = new TaskDialogButton
            {
                Text = "终止程序（推荐）",
                CommandLinkNote = "丢弃现在状态并退出程序"
            };
            var btnCopyAndEnd = new TaskDialogButton
            {
                Text = "复制错误信息并退出",
                CommandLinkNote = "复制错误信息并退出程序"
            };
            var btnCopyAndReportAndEnd = new TaskDialogButton
            {
                Text = "报告问题并退出",
                CommandLinkNote = "打开Github反馈页面并复制错误信息。随后程序退出"
            };
            var btnContinue = new TaskDialogButton
            {
                Text = "继续运行（不推荐）",
                CommandLinkNote = "忽略这个错误并继续执行程序（出现错误后程序一般无法正常运行）"
            };

            var btnClose = new TaskDialogButton
            {
                Text = "关闭",
                ButtonType = ButtonType.Close
            };

            dialog.Buttons.Add(btnEnd);
            dialog.Buttons.Add(btnCopyAndEnd);

            if (!isUnHandleException)
            {
                dialog.Buttons.Add(btnCopyAndReportAndEnd);
                dialog.Buttons.Add(btnContinue);
            }

            dialog.Buttons.Add(btnClose);


            var result = dialog.ShowDialog();


            if (result == btnEnd)
            {
                Environment.Exit(1);
            }
            else if (result == btnCopyAndEnd)
            {
                Clipboard.SetText($"{new string('=', 5)}[PvzLauncher 错误信息]{new string('=', 5)}\n\n{ex}\n\n{new string('=', 25)}");
                Environment.Exit(1);
            }
            else if (result == btnCopyAndReportAndEnd)
            {
                Clipboard.SetText($"{new string('=', 5)}[PvzLauncher 错误信息]{new string('=', 5)}\n\n{ex}\n\n{new string('=', 25)}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/PvzLauncher/PvzLauncher/issues/new?template=bug.yml",
                    UseShellExecute = true
                });
                Environment.Exit(1);
            }
        }

        /*public static async void Show(string title, string message, Exception ex)
        {
            logger.Error(
                $"{new string('=', 10)}ERROR{new string('=', 10)}\n" +
                $"{title}\n" +
                $"{message}\n" +
                $"{ex}\n" +
                $"{new string('=', 10)}ERROR{new string('=', 10)}");

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var exceptionTextBox = new TextBox
            {
                Text = ex.ToString(),
                IsReadOnly = true,
                FontSize = 12,
                Padding = new Thickness(8),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };

            scrollViewer.Content = exceptionTextBox;

            var content = new StackPanel
            {
                Children =
                {
                    new System.Windows.Controls.ProgressBar
                    {
                        Value = 100,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 50, 50)),
                        Margin = new Thickness(0,0,0,10)
                    },
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0,0,0,10)
                    },
                    scrollViewer,

                    new TextBlock
                    {
                        Text = "您可以全选并复制上方内容反馈给开发者",
                    }
                }
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "继续运行",
                CloseButtonText = "终止程序",
                DefaultButton = ContentDialogButton.Primary
            };

            await DialogManager.ShowDialogAsync(dialog, closeCallback: (() =>
            {
                Environment.Exit(1);
            }));
        }*/
    }
}