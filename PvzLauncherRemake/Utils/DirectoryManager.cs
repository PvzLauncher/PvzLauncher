using PvzLauncherRemake.Class;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class DirectoryManager
    {
        /// <summary>
        /// 简单的复制文件夹，没有安全保护，自己写去
        /// </summary>
        /// <param name="sourceDir">源文件夹</param>
        /// <param name="destDir">目标文件夹</param>
        /// <returns></returns>
        public static async Task CopyDirectoryAsync(string sourceDir, string destDir, Action<string> callBack = null!)
        {
            var sourceDirInfo = new DirectoryInfo(sourceDir);

            // 创建目标文件夹（包括多级）
            Directory.CreateDirectory(destDir);

            // 复制所有文件
            foreach (FileInfo file in sourceDirInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                await Task.Run(() =>
                {
                    if (callBack != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            callBack(file.Name);
                        });
                    }
                    logger.Info($"复制文件: {file.Name}");
                    file.CopyTo(targetFilePath);
                });
            }

            // 递归复制所有子文件夹
            foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
            {
                string targetSubDir = Path.Combine(destDir, subDir.Name);
                await CopyDirectoryAsync(subDir.FullName, targetSubDir);
            }
        }
    }
}