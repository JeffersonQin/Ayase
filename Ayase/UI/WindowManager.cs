using Ayase.AccessibilityBridge;
using Microsoft.Toolkit.Uwp.Notifications;
using Notifications.Wpf;
using Notifications.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ayase.UI
{
    public static class WindowManager
    {
        public static ScreenMaskWindow screenMaskWindow;

        public static FormMaskWindow formMaskWindow;

        public static SearchWindow searchWindow;

        public static NotificationManager notificationManager;

        /// <summary>
        /// 1: Stopping<br/>
        /// 0: Idle<br/>
        /// -1: Running<br/>
        /// </summary>
        public static int ProcessStopFlag = 0;

        public static int RenderFinishedFlag = 1;

        public static IntPtr UIElements = IntPtr.Zero;

        public static int UIElementCount = 0;

        public static List<Label> notationLabels;

        public static void InitializeComponents()
        {
            screenMaskWindow = new ScreenMaskWindow();
            formMaskWindow = new FormMaskWindow();
            searchWindow = new SearchWindow();
            notificationManager = new NotificationManager();
            notationLabels = new List<Label>();
        }

        public static void HideWindow(Window window)
        {
            window.Dispatcher.InvokeAsync(() =>
            {
                window.Hide();
            }, DispatcherPriority.Render);
        }

        public static void PresentWindow(Window window)
        {
            window.Dispatcher.InvokeAsync(() =>
            {
                window.Show();
                window.Activate();
            }, DispatcherPriority.Render);
        }

        public static void HideScreenMask()
        {
            HideWindow(screenMaskWindow);
        }

        public static void HideFormMask()
        {
            HideWindow(formMaskWindow);
        }

        public static void HideSearchWindow()
        {
            HideWindow(searchWindow);
        }

        public static void PresentScreenMask()
        {
            screenMaskWindow.Dispatcher.InvokeAsync(() =>
            {
                screenMaskWindow.Top = SystemParameters.VirtualScreenTop;
                screenMaskWindow.Left = SystemParameters.VirtualScreenLeft;
                screenMaskWindow.Width = SystemParameters.VirtualScreenWidth;
                screenMaskWindow.Height = SystemParameters.VirtualScreenHeight;
            }, DispatcherPriority.Render);
            PresentWindow(screenMaskWindow);
        }

        public static void PresentFormMask(double x, double y, double w, double h)
        {
            formMaskWindow.Dispatcher.InvokeAsync(() =>
            {
                formMaskWindow.Top = y;
                formMaskWindow.Left = x;
                formMaskWindow.Width = w;
                formMaskWindow.Height = h;
                formMaskWindow.FormNotificationArea.Position = NotificationPosition.BottomRight;
                formMaskWindow.FormNotificationArea.MaxItems = 10;
                formMaskWindow.Canvas.SetValue(Canvas.LeftProperty, 0.0);
                formMaskWindow.Canvas.SetValue(Canvas.TopProperty, 0.0);
                formMaskWindow.Canvas.SetValue(Canvas.WidthProperty, w);
                formMaskWindow.Canvas.SetValue(Canvas.HeightProperty, h);
                formMaskWindow.FormNotificationArea.SetValue(Canvas.LeftProperty, 0.0);
                formMaskWindow.FormNotificationArea.SetValue(Canvas.TopProperty, 0.0);
                formMaskWindow.FormNotificationArea.SetValue(Canvas.WidthProperty, w);
                formMaskWindow.FormNotificationArea.SetValue(Canvas.HeightProperty, h);
            }, DispatcherPriority.Render);
            PresentWindow(formMaskWindow);
        }

        public static void PresentSearchWindow()
        {
            PresentWindow(searchWindow);
        }

        public static void StartProcess()
        {
            ProcessStopFlag = -1;
            RenderFinishedFlag = 0;
        }

        public static void EndProcess()
        {
            formMaskWindow.Dispatcher.Invoke(() =>
            {
                formMaskWindow.Canvas.Children.Clear();
            }, DispatcherPriority.Render);
            HideFormMask();
            HideScreenMask();
            HideSearchWindow();

            ProcessStopFlag = 1;
            while (RenderFinishedFlag == 0) Thread.Sleep(10);

            if (UIElements != IntPtr.Zero) GNativeIUIAutomationManager.DeleteLeafElements(UIElements);
            UIElements = IntPtr.Zero;
            UIElementCount = 0;

            notationLabels.Clear();

            ProcessStopFlag = 0;
        }

        public static Label AddNotationLabel(double x, double y, double w, double h, String Name)
        {
            Label label = null;
            formMaskWindow.Dispatcher.Invoke(() => {
                label = new Label();
                label.SetValue(Canvas.LeftProperty, x);
                label.SetValue(Canvas.TopProperty, y);
                label.Foreground = new SolidColorBrush(Colors.White);
                label.Background = new SolidColorBrush(Color.FromArgb(100, 69, 179, 232));
                label.BorderBrush = new SolidColorBrush(Colors.Red);
                label.VerticalAlignment = VerticalAlignment.Center;
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.Padding = new Thickness(0);
                label.Margin = new Thickness(0);
                label.BorderThickness = new Thickness(1);
                label.Width = w;
                label.Height = h;
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Text = Name;
                label.Content = tb;
                notationLabels.Add(label);
                formMaskWindow.Canvas.Dispatcher.Invoke(() =>
                {
                    formMaskWindow.Canvas.Children.Add(label);
                }, DispatcherPriority.Render);
            }, DispatcherPriority.Render);
            return label;
        }

        public static void FinishRender()
        {
            RenderFinishedFlag = 1;
        }

        public static void FocusSearch()
        {
            
        }
    }
}
