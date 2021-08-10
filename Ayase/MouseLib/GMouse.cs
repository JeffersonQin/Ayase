using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.MouseLib
{
    public class GMouse
    {
        public static uint MOUSEEVENTF_MOVE = 0x0001;
        public static uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public static uint MOUSEEVENTF_LEFTUP = 0x0004;
        public static uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public static uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public static uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public static uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public static uint MOUSEEVENTF_XDOWN = 0x0080;
        public static uint MOUSEEVENTF_XUP = 0x0100;
        public static uint MOUSEEVENTF_WHEEL = 0x0800;
        public static uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public static uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
    }
}
