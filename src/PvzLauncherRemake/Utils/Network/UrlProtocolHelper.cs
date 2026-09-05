using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvzLauncherRemake.Utils.Network
{
    public static class UrlProtocolHelper
    {
        public const string ProtocolName = "pvzl";

        /// <summary>
        /// 注册URL协议
        /// </summary>
        /// <param name="protocol">协议名(name://)</param>
        /// <param name="path">可执行文件路径</param>
        public static void Register(string protocol, string path)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{protocol}"))
            {
                key.SetValue("", $"URL:{protocol} Protocol");
                key.SetValue("URL Protocol", "");

                using (var command = key.CreateSubKey(@"shell\open\command"))
                {
                    command.SetValue("", $"\"{path}\" \"%1\"");
                }
            }
        }

        /// <summary>
        /// 卸载协议
        /// </summary>
        /// <param name="protocol">协议名</param>
        public static void Unregister(string protocol)
            => Registry.CurrentConfig.DeleteSubKeyTree(@$"Software\Classes\{protocol}", false);
    }
}
