using Ayase.AccessibilityBridge;
using Ayase.HotkeyLib;
using Ayase.ScreenLib;
using Ayase.ThreadLib;
using Ayase.UI;
using Microsoft.Toolkit.Uwp.Notifications;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ToolGood.Words;

namespace Ayase
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SettingsWindow settingsWindow = new SettingsWindow();

        private GThreadPool<ThreadStart> threadPool = new GThreadPool<ThreadStart>(10, new Action<ThreadStart>((threadStart) =>
        {
            threadStart.Invoke();
        }));

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
            new ToastContentBuilder()
                .AddText("Initializing automation library...")
                .Show(); 
            int result = GNativeIUIAutomationManager.InitializeUIAutomation();
            if (result < 0)
            {
                new ToastContentBuilder()
                    .AddText("Initializing automation library failed")
                    .AddText("Exiting Program...")
                    .Show();
                Shutdown();
            }
            #endregion

            #region initialize window manager
            WindowManager.InitializeComponents();
            #endregion

            #region load pinyin
            _ = WordsHelper.GetPinyin("中文测试");
            #endregion
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            if (wParam.ToInt64() > Int32.MaxValue) 
                return IntPtr.Zero;
            if (wParam.ToInt32() == accessibilityHotKeyId)
            {
                #region toggle captial light (offset the negative effect of hotkey)
                threadPool.EnqueueTask(() =>
                {
                    GKeybdEvent.keybd_event((byte)VirtualKey.VK_MENU, 0x45, 3, 0);
                    Thread.Sleep(200);
                    GKeybdEvent.keybd_event((byte)VirtualKey.VK_CAPITAL, 0x45,
                        GKeybdEvent.KEYEVENTF_EXTENDEDKEY | 0, 0);
                    GKeybdEvent.keybd_event((byte)VirtualKey.VK_CAPITAL, 0x45,
                        GKeybdEvent.KEYEVENTF_EXTENDEDKEY | GKeybdEvent.KEYEVENTF_KEYUP, 0);
                });
                #endregion

                #region avoid multiple running instance
                if (WindowManager.ProcessStopFlag != 0)
                {
                    WindowManager.notificationManager.Show(
                        new NotificationContent
                        {
                            Title = "Warning",
                            Message = "Please wait until present process ends.",
                            Type = NotificationType.Warning
                        }, areaName: "FormNotificationArea"
                    );
                    return IntPtr.Zero;
                }
                WindowManager.StartProcess();
                #endregion

                #region get foreground window
                GNativeIUIAutomationManager.GetForegroundWindowElement(out IntPtr foregroundWindow);
                #endregion

                #region present masks
                GNativeIUIAutomationManager.GetElementBounds(foregroundWindow, out double x, out double y, out double w, out double h);

                WindowManager.PresentScreenMask();
                WindowManager.PresentFormMask(
                    x * 96.0 / PrimaryScreen.DpiX,
                    y * 96.0 / PrimaryScreen.DpiY,
                    w * 96.0 / PrimaryScreen.DpiX,
                    h * 96.0 / PrimaryScreen.DpiY);
                WindowManager.PresentSearchWindow();
                #endregion

                #region start time costing thread
                settingsWindow.Dispatcher.Invoke(async () =>
                {
                    await Task.Run(() =>
                    {
                        #region get elements
                        int hr;
                        DateTime ts_start, ts_end;
                        ts_start = DateTime.UtcNow;
                        hr = GNativeIUIAutomationManager.GetLeafElementsFromWindow(foregroundWindow, out IntPtr leafElements, out int elementCount, ref WindowManager.ProcessStopFlag);
                        ts_end = DateTime.UtcNow;

                        if (hr < 0)
                        {
                            new ToastContentBuilder()
                                .AddText("Getting leaf elements from window failed, exitting...")
                                .AddText("Fail Code: " + hr).Show();
                            WindowManager.FinishRender();
                            WindowManager.EndProcess();
                            return;
                        }

                        if (elementCount == 0)
                        {
                            new ToastContentBuilder().AddText("No UI Element found, exitting...").Show();
                            WindowManager.FinishRender();
                            WindowManager.EndProcess();
                            return;
                        }

                        WindowManager.UIElements = leafElements;
                        WindowManager.UIElementCount = elementCount;
                        WindowManager.formMaskWindow.FormNotificationArea.Dispatcher.Invoke(() =>
                        {
                            WindowManager.notificationManager.Show(
                            new NotificationContent
                            {
                                Title = "UI Analyzation Completed",
                                Message = "Time Cost: " + (ts_end - ts_start).TotalSeconds.ToString() + "s\n" + elementCount + " items found",
                                Type = NotificationType.Success
                            }, areaName: "FormNotificationArea"
                        );
                        }, DispatcherPriority.Render);
                        
                        if (WindowManager.ProcessStopFlag > 0)
                        {
                            WindowManager.FinishRender();
                            return;
                        }
                        #endregion

                        #region render
                        for (int i = 0; i < elementCount; i ++)
                        {
                            int index = i;
                            threadPool.EnqueueTask(() =>
                            {
                                hr = GNativeIUIAutomationManager.GetGUIElement(leafElements, index, out IntPtr element);
                                if (element == IntPtr.Zero || hr < 0) return;
                                hr = GNativeUIElement.GetBoundingRectangle(element, out double ex, out double ey, out double ew, out double eh);
                                if (hr < 0) return;
                                hr = GNativeUIElement.GetName(element, out IntPtr namePointer);
                                if (hr < 0) return;
                                string name = Marshal.PtrToStringUni(namePointer);
                                lock (WindowManager.MatchStringsLocker)
                                {
                                    WindowManager.MatchStrings.Add(index, new List<string>());
                                    WindowManager.MatchStrings[index].Add(name.ToLower());
                                    if (WordsHelper.HasChinese(name))
                                    {
                                        WindowManager.MatchStrings[index].Add(WordsHelper.GetPinyin(name).ToLower());
                                        WindowManager.MatchStrings[index].Add(WordsHelper.GetFirstPinyin(name).ToLower());
                                    }
                                }
                                object locker = new object();
                                WindowManager.AddNotationLabel(
                                    (ex - x) * 96.0 / PrimaryScreen.DpiX,
                                    (ey - y) * 96.0 / PrimaryScreen.DpiY,
                                    ew * 96.0 / PrimaryScreen.DpiX,
                                    eh * 96.0 / PrimaryScreen.DpiY, 
                                    name, index, ref locker);
                                _ = locker;
                            });
                        }
                        threadPool.Join();
                        lock (WindowManager.candidateIndexesLocker)
                        {
                            WindowManager.candidateIndexes.Sort();
                        }
                        WindowManager.searchWindow.Dispatcher.Invoke(() =>
                        {
                            WindowManager.searchWindow.QueryTextBox_TextChanged(this, null);
                        }, DispatcherPriority.Render);
                        WindowManager.FinishRender();
                        #endregion
                    });
                }, DispatcherPriority.Loaded);
                #endregion
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
