using ExecuteShell.Windows;
using System.Windows;

namespace ExecuteShell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var window = new WindowMain();
        this.MainWindow = window;
        window.Show();
    }
}

