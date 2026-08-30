using GameLibraryEditor.JsonConfigs;
using GameLibraryEditor.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace GameLibraryEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!File.Exists(Globals.ConfigPath))
            {
                var dialog = new OpenFolderDialog
                {
                    Multiselect = false
                };
                if (dialog.ShowDialog() != true)
                    App.Current.Shutdown(0);


                Globals.Config = new JsonConfig.Root
                {
                    IndexDirectory = dialog.FolderName
                };

                File.WriteAllText(Globals.ConfigPath, JsonConvert.SerializeObject(Globals.Config));
            }

            Globals.Config = JsonConvert.DeserializeObject<JsonConfig.Root>(File.ReadAllText(Globals.ConfigPath))!;

            var win = new WindowMain();
            this.MainWindow = win;
            win.Show();
        }
    }

}
