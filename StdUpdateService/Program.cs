using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace StdUpdateService
{
    internal class Program
    {
        private static string? BinPack;
        private static string? ShellPack;
        private static string? BinPath;
        private static string? ShellPath;
        private static string? ExePath;
        private static bool isSelfUpdate = false;
        private static bool isTest = false;


        static async Task Main(string[] args)
        {
            try
            {
                string thisExePaht = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StdUpdateService.exe");
                string tempPath = Path.GetTempPath();

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-binpack":
                            if (args.Length >= i + 1)
                                BinPack = args[i + 1];
                            break;
                        case "-shellpack":
                            if (args.Length >= i + 1)
                                ShellPack = args[i + 1];
                            break;
                        case "-binpath":
                            if (args.Length >= i + 1)
                                BinPath = args[i + 1];
                            break;
                        case "-shellpath":
                            if (args.Length >= i + 1)
                                ShellPath = args[i + 1];
                            break;
                        case "-exepath":
                            if (args.Length >= i + 1)
                                ExePath = args[i + 1];
                            break;
                        case "-selfupdate":
                            isSelfUpdate = true;
                            break;
                        case "-test":
                            isTest = true;
                            break;
                    }
                }
                //测试
                if (isTest)
                {
                    Console.Write("done");
                    return;
                }



                if (string.IsNullOrEmpty(BinPack) ||
                    string.IsNullOrEmpty(ShellPack) ||
                    string.IsNullOrEmpty(BinPath) ||
                    string.IsNullOrEmpty(ExePath) ||
                    string.IsNullOrEmpty(ShellPath))
                    throw new Exception($"未传入必要参数: -binpack -shellpack -binpath -shellpath -exepath");

                if (isSelfUpdate)
                {
                    Console.WriteLine("正在执行自更新...");
                    string tempExePath = Path.Combine(tempPath, "StdUpdateService.exe");
                    File.Copy(thisExePaht, tempExePath, true);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempExePath,
                        UseShellExecute = true,
                        Arguments = $"-binpack \"{BinPack}\" -shellpack \"{ShellPack}\" -binpath \"{BinPath}\" -shellpath \"{ShellPath}\" -exepath \"{ExePath}\""
                    });
                    return;
                }

                Console.WriteLine($"""
                    BinPack: "{BinPack}"
                    ShellPack: "{ShellPack}"
                    BinPath: "{BinPath}"
                    ShellPath: "{ShellPath}"
                    ExePath: "{ExePath}"
                    SelfUpdate: {(isSelfUpdate ? "True" : "False")}
                    """);

                Console.WriteLine("等待主程序完全退出...");

                if (!WaitForProcessExit("PvzLauncherRemake", 30))
                    throw new Exception("主程序未能在30秒内退出，请确保主程序已完全关闭后再尝试更新");

                Thread.Sleep(1000);

                Console.WriteLine("正在解压基础包...");
                ZipFile.ExtractToDirectory(BinPack, BinPath, true);
                Console.WriteLine("完成！");
                Console.WriteLine("正在解压外壳包...");
                ZipFile.ExtractToDirectory(ShellPack, ShellPath, true);
                Console.WriteLine("完成！");

                Console.WriteLine("更新完成，将在三秒内启动主程序");

                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine($"{3 - i}");
                    Thread.Sleep(1000);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = ExePath,
                    UseShellExecute = true,
                    Arguments = $"-shell -update"
                });

                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"发生错误: {ex.Message}\n  详细信息: {ex}");
                Console.ResetColor();
                Console.Write("程序异常终止，更新失败，按任意键退出...");
                Console.ReadKey();
            }
        }

        static bool WaitForProcessExit(string processName, int timeoutSeconds = 30)
        {
            var processes = Process.GetProcessesByName(processName);
            var stopwatch = Stopwatch.StartNew();

            while (processes.Length > 0 && stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
            {
                Thread.Sleep(500);
                processes = Process.GetProcessesByName(processName);
            }

            return processes.Length == 0;
        }
    }
}
