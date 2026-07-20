using System.Drawing;
using System.Runtime.InteropServices;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();



        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="newTitle">新标题</param>
        /// <returns>是否成功</returns>
        public static bool SetWindowTitle(IntPtr hWnd, string newTitle)
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
        /// 取鼠标坐标
        /// </summary>
        /// <returns>坐标,X & Y</returns>
        public static POINT GetCursorPos()
        {
            if (!GetCursorPos(out var result))
                return new POINT { X = -1, Y = -1 };
            return result;
        }

        /// <summary>
        /// 设置鼠标坐标
        /// </summary>
        /// <param name="pos">目标坐标</param>
        /// <returns>是否成功</returns>
        public static bool SetCursorPos(Point pos) => SetCursorPos(pos.X, pos.Y);
    }
}
