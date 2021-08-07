using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

namespace Ayase.UI
{
    public static class WindowManager
    {
        public static ScreenMaskWindow screenMaskWindow;

        public static FormMaskWindow formMaskWindow;

        public static Thread CurrentThread;

        public static void InitializeComponents()
        {
            screenMaskWindow = new ScreenMaskWindow();
            formMaskWindow = new FormMaskWindow();
        }

        public static void HideWindow(Window window)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.Hide();
            }, System.Windows.Threading.DispatcherPriority.Send);
        }

        public static void PresentWindow(Window window)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.Show();
                window.Activate();
            }, System.Windows.Threading.DispatcherPriority.Send);
        }

        public static void HideScreenMask()
        {
            HideWindow(screenMaskWindow);
        }

        public static void HideFormMask()
        {
            HideWindow(formMaskWindow);
        }

        public static void FocusSearch()
        {

        }

        public static void PresentScreenMask()
        {
            screenMaskWindow.Dispatcher.Invoke(() =>
            {
                screenMaskWindow.Top = SystemParameters.VirtualScreenTop;
                screenMaskWindow.Left = SystemParameters.VirtualScreenLeft;
                screenMaskWindow.Width = SystemParameters.VirtualScreenWidth;
                screenMaskWindow.Height = SystemParameters.VirtualScreenHeight;
            }, System.Windows.Threading.DispatcherPriority.Send);
            PresentWindow(screenMaskWindow);
        }

        public static void PresentFormMask()
        {
            PresentWindow(formMaskWindow);
        }
    }
}
