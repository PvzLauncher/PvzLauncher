using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace PvzLauncherRemake.Utils
{
    public class LocalizeManager
    {
        public static void SwitchLanguage(string languageCode = "zh-CN")
        {
            var newCulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;

            LocalizeDictionary.Instance.Culture = newCulture;
        }

        public static string GetLoc(string key)
        {
            return LocExtension.GetLocalizedValue<string>(key);
        }
    }
}
