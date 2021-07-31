using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.AutomationLib
{
    public class NativeAPI
    {
        /// <summary>
        /// Get the pointer of foreground window
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();


        /// <summary>
        /// Get the process ID of the window given
        /// </summary>
        /// <param name="hWnd">pointer of the window</param>
        /// <param name="ID">the process id variable to set</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ID);

    }
}
