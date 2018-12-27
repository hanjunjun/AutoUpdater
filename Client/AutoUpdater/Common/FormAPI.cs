//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater.Common
{
    public class FormAPI
    {
        public const int HWND_TOP = 0;
        public const int HWND_BOTTOM = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOSIZE = 1;


        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);


        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out WindowRect lpRect);

        /// <summary>
        /// 设置窗体最前端
        /// </summary>
        /// <param name="hWnd"></param>
        public static void SetFormTop(IntPtr hWnd)
        {
            WindowRect rect = new WindowRect();
            GetWindowRect(hWnd, out rect);
            SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
        }

        /// <summary>
        /// 取消窗体最前端
        /// </summary>
        /// <param name="hWnd"></param>
        public static void SetFormNoTop(IntPtr hWnd)
        {
            WindowRect rect = new WindowRect();
            GetWindowRect(hWnd, out rect);
            SetWindowPos(hWnd, (IntPtr)HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
    }
    public struct WindowRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
