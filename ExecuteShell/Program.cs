using System.Diagnostics;
using System.Reflection;

namespace ExecuteShell
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                string executePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
                string binPath = $"{Path.Combine(executePath, "bin")}";
                string exePath = $"{Path.Combine(binPath, "PvzLauncherRemake.exe")}";

                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Arguments = $"-shell"
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"在执行主程序时发生错误\n\n------------------------------\n\n{ex}", $"发生错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}