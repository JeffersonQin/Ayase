using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.HotkeyLib
{
    public static class GKeybdEvent
    {
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 2;
        
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
    }
}
