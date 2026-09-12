using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PvzLauncherRemake.Utils
{
    public static class HotKeyHelper
    {
        /// <summary>
        /// 检查热键是否可用
        /// </summary>
        /// <param name="key">热键</param>
        /// <param name="modifierKeys">修饰键</param>
        /// <returns></returns>
        public static bool IsHotKeyAvaiable(Key key, ModifierKeys modifierKeys)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace("Test", key, modifierKeys, (_, _) => { });
                HotkeyManager.Current.Remove("Test");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
