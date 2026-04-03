using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace PvzLauncherRemake.Utils.Services
{
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    public static class Win32APIHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

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

        /// <summary>
        /// 获取Window的坐标及大小
        /// </summary>
        /// <param name="hWnd">句柄</param>
        /// <returns>坐标及大小，如空则获取失败</returns>
        public static RECT GetWindowArea(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return new RECT { Left = 0, Right = 0, Top = 0, Bottom = 0 };

            var result = new RECT();

            if (GetWindowRect(hWnd, ref result))
                return result;
            else
                return new RECT { Left = 0, Right = 0, Top = 0, Bottom = 0 };
        }

        /// <summary>
        /// 获取当前活动窗口的句柄，即用户正在操作的窗口
        /// </summary>
        /// <returns>窗口句柄</returns>
        public static IntPtr GetActiveWindowHandle() => GetForegroundWindow();
    }
}
