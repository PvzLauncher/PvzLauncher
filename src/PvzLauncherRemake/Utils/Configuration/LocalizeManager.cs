using System.Globalization;
using System.Reflection;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace PvzLauncherRemake.Utils.Configuration
{
    public class LocalizeManager
    {
        public string Directory { get; set; }

        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;


        public string GetLoc(string key)
        {
            string fullKey = $"{AssemblyName}:{Directory}:{key}";
            string value = LocExtension.GetLocalizedValue<string>(fullKey);
            return value ?? fullKey;
        }

        public static string GetLoc(string directory, string key)
        {
            string fullKey = $"{AssemblyName}:{directory}:{key}";
            string value = LocExtension.GetLocalizedValue<string>(fullKey);
            return value ?? fullKey;
        }

        public static void SwitchLanguage(string languageCode = "zh-CN")
        {
            var newCulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;

            LocalizeDictionary.Instance.Culture = newCulture;
        }
    }
}
