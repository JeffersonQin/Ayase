using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.Wrapper
{
    public class Win32API
    {

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Get the pointer of foreground window
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();


        /// <summary>
        /// Get the process ID of the window given
        /// </summary>
        /// <param name="hWnd">hWnd of the window</param>
        /// <param name="ID">the process id variable to set</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ID);


        /// <summary>
        /// Find the hWnd of the window by ClassName or WindowName
        /// </summary>
        /// <param name="lpClassName">ClassName of window</param>
        /// <param name="lpWindowName">WindowName of window</param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        /// <summary>
        /// Find the hWnd of the window
        /// </summary>
        /// <param name="hwndParent">Parent hWnd</param>
        /// <param name="hwndChildAfter">hWnd of last sibling</param>
        /// <param name="lpszClass">ClassName of Window</param>
        /// <param name="lpszWindow">WindowName of Window</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);



        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);


        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);


        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }
}
