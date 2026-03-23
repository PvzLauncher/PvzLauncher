using System.Windows;
using Wpf.Ui.Controls;

namespace PvzLauncherRemake.Utils.UI
{
    public static class SnackbarManager
    {
        public static void Show(SnackbarContent snackbar)
        {
            if (Application.Current.MainWindow is not WindowMain window)
                return;


            var appearance = ControlAppearance.Info;
            var symbolRegular = SymbolRegular.Info24;

            switch (snackbar.Type)
            {
                case SnackbarType.Info:
                    appearance = ControlAppearance.Info;
                    symbolRegular = SymbolRegular.Info24;
                    break;
                case SnackbarType.Warn:
                    appearance = ControlAppearance.Caution;
                    symbolRegular = SymbolRegular.Warning24;
                    break;
                case SnackbarType.Error:
                    appearance = ControlAppearance.Danger;
                    symbolRegular = SymbolRegular.ErrorCircle24;
                    break;
                case SnackbarType.Success:
                    appearance = ControlAppearance.Success;
                    symbolRegular = SymbolRegular.CheckmarkCircle24;
                    break;
            }



            window._snackbarService.Show(
                title: snackbar.Title,
                message: snackbar.Content,
                appearance: appearance,
                icon: new SymbolIcon { Symbol = symbolRegular },
                timeout: snackbar.TimeOut
                );

        }
    }

    public class SnackbarContent
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public SnackbarType Type { get; set; } = SnackbarType.Info;
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(2);
    }

    public enum SnackbarType
    {
        Info,
        Warn,
        Error,
        Success
    }
}
