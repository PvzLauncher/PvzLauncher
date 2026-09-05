using PvzLauncherRemake.Classes;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public static class Cleaner
    {
        private static ILogger logger = Log.ForContext(typeof(Cleaner));

        public static async Task<CleanerFiles> Scan()
        {
            logger.Information("开始扫描垃圾文件...");

            //TEMP FILES
            string[] allTempFiles = { };
            List<string> pvzlTempFiles = new List<string>();
            long totalSize = 0;

            logger.Information($"开始获得总临时文件...");

            await Task.Run(() => allTempFiles = Directory.GetFiles(Globals.Directories.TempDirectory));

            logger.Information($"已获得总临时文件 {allTempFiles.Length}");

            pvzlTempFiles.Clear();
            foreach (var file in allTempFiles)
            {
                if (Path.GetFileName(file).StartsWith("PvzLauncher", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Information($"已找到: {file}");
                    pvzlTempFiles.Add(file);
                    totalSize += new FileInfo(file).Length;
                }
            }

            //LOG FILES
            string[] allLogFiles = { };
            List<string> oldLogFiles = new List<string>();

            logger.Information("开始获得总日志文件");

            await Task.Run(() => allLogFiles = Directory.GetFiles(Globals.Directories.LogDirectory));

            logger.Information($"已获得总日志文件 {allLogFiles.Length}");

            oldLogFiles.Clear();
            foreach (var file in allLogFiles)
            {
                var timeStampStr = Regex.Replace(Path.GetFileName(file), @"[^0-9]", "");
                long timeStamp = 0;
                if (!long.TryParse(timeStampStr, out var result))
                {
                    logger.Warning($"日志文件 `{Path.GetFileName(file)}` 不包含时间戳数字");
                    continue;
                }
                else
                    timeStamp = result;

                if (timeStamp <= DateTimeOffset.Now.AddDays(-10).ToUnixTimeMilliseconds())
                {
                    logger.Information($"已找到: {file}");
                    oldLogFiles.Add(file);
                    totalSize += new FileInfo(file).Length;
                }
            }

            logger.Information("垃圾文件扫描完成");
            //RETURN
            return new CleanerFiles
            {
                TotalSize = totalSize,
                TempFiles = pvzlTempFiles.ToArray(),
                OldLogs = oldLogFiles.ToArray()
            };
        }

        public static void Clean(CleanerFiles files, Action<string>? progressCallback = null)
        {
            logger.Information("开始清理垃圾文件...");

            void cleanFiles(string[] fs)
            {
                foreach (var file in fs)
                {
                    File.Delete(file);
                    progressCallback?.Invoke(file);
                    logger.Information($"清理: {file}");
                }
            }

            cleanFiles(files.TempFiles);
            cleanFiles(files.OldLogs);

            logger.Information("清理完成");
        }
    }

    public class CleanerFiles
    {
        public long TotalSize;

        public string[] TempFiles;
        public string[] OldLogs;
    }
}
