using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PvzLauncherRemake.Utils.Services
{
    public static class Win32APIHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="newTitle">新标题</param>
        /// <returns>是否成功</returns>
        public static bool SetWindowTitle(IntPtr hWnd,string newTitle)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            return SetWindowText(hWnd, newTitle);
        }
    }
}
