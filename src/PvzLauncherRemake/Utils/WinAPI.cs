using Serilog;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PvzLauncherRemake.Utils
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

    public static class WinAPI
    {
        private static readonly ILogger logger = Log.ForContext(typeof(WinAPI));


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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BlockInput(bool fBlockIt);

        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_TRANSPARENT = 0x00000020; // 鼠标穿透
        public const int WS_EX_TOOLWINDOW = 0x00000080;  // 不显示在Alt+Tab
        public const int WS_EX_NOACTIVATE = 0x08000000;  // 不激活

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // 用于检测是否运行在Wine环境下

        [DllImport("ntdll.dll")]
        private static extern IntPtr wine_get_version();


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

        /// <summary>
        /// 是否运行在Wine环境下
        /// </summary>
        public static bool IsWine()
        {
            try
            {
                return wine_get_version() != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                logger.Debug($"尝试判断Wine环境时报错:\n {ex}");
                return false;
            }
        }

        /// <summary>
        /// 尝试获取Wine的版本
        /// </summary>
        /// <returns>理应为版本号字符串，若非wine环境则为null</returns>
        public static string? GetWineVersion()
        {
            try
            {
                var ptr = wine_get_version();
                return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
            }
            catch (Exception ex)
            {
                logger.Debug($"尝试获取Wine版本时报错: \n{ex}");
                return null;
            }
        }

    }
}
