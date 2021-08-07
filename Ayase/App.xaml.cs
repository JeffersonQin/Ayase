using Ayase.AccessibilityBridge;
using Ayase.HotkeyLib;
using Ayase.ScreenLib;
using Ayase.UI;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Ayase
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SettingsWindow settingsWindow = new SettingsWindow();
        private int accessibilityHotKeyId;

        protected override void OnStartup(StartupEventArgs e)
        {
            #region base functions
            base.OnStartup(e);
            #endregion

            #region start up messages
            new ToastContentBuilder()
                .AddText("Ayase Initializing...")
                .Show();
            #endregion

            #region show settings window
            settingsWindow.Show();
            #endregion

            #region Hotkey registry
            IntPtr hwnd = new WindowInteropHelper(settingsWindow).Handle;
            while (true)
            {
                accessibilityHotKeyId = new Random().Next();
                new ToastContentBuilder()
                    .AddText("Trying to register accessibility hotkey...")
                    .AddText("HotKey ID: " + accessibilityHotKeyId)
                    .Show();
                int hr = HotkeyLib.GHotKey.RegisterHotKey(hwnd, accessibilityHotKeyId,
                    HotkeyLib.GHotKey.MOD_ALT | HotkeyLib.GHotKey.MOD_NOREPEAT,
                    (uint)HotkeyLib.VirtualKey.VK_CAPITAL);
                if (hr == 0)
                {
                    new ToastContentBuilder()
                        .AddText("Hotkey binding failed, trying other id...")
                        .Show();
                }
                else break;
            }
            HwndSource hwndSource = PresentationSource.FromVisual(settingsWindow) as HwndSource;
            hwndSource.AddHook(WndProc);
            #endregion

            #region automation registry
            GNativeIUIAutomationManager.InitializeUIAutomation();
            #endregion

            #region initialize window manager
            WindowManager.InitializeComponents();
            #endregion
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            if (wParam.ToInt64() > Int32.MaxValue) return IntPtr.Zero;
            if (wParam.ToInt32() == accessibilityHotKeyId)
            {
                WindowManager.CurrentThread = new Thread(() =>
                {
                    #region toggle captial light (offset the negative effect of hotkey)
                    GKeybdEvent.keybd_event((byte)VirtualKey.VK_MENU, 0x45, 3, 0);
                    new Thread(() =>
                    {
                        Thread.Sleep(200);
                        GKeybdEvent.keybd_event((byte)VirtualKey.VK_CAPITAL, 0x45,
                            GKeybdEvent.KEYEVENTF_EXTENDEDKEY | 0, 0);
                        GKeybdEvent.keybd_event((byte)VirtualKey.VK_CAPITAL, 0x45,
                            GKeybdEvent.KEYEVENTF_EXTENDEDKEY | GKeybdEvent.KEYEVENTF_KEYUP, 0);
                    }).Start();
                    #endregion

                    WindowManager.PresentScreenMask();
                });
                WindowManager.CurrentThread.Start();
            }
            return IntPtr.Zero;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            #region base functions
            base.OnExit(e);
            #endregion

            #region destroy hotkey
            IntPtr hwnd = new WindowInteropHelper(settingsWindow).Handle;
            HotkeyLib.GHotKey.UnregisterHotKey(hwnd, accessibilityHotKeyId);
            #endregion
        }
    }
}
