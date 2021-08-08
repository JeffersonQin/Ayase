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

        public static List<NotationLabel> notationLabels;

        public static List<int> candidateIndexes;

        public static int focusIndex;

        public static void InitializeComponents()
        {
            screenMaskWindow = new ScreenMaskWindow();
            formMaskWindow = new FormMaskWindow();
            searchWindow = new SearchWindow();
            notificationManager = new NotificationManager();
            notationLabels = new List<NotationLabel>();
            candidateIndexes = new List<int>();
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
            focusIndex = -1;
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
            candidateIndexes.Clear();
            focusIndex = -1;

            ProcessStopFlag = 0;
        }

        public static NotationLabel AddNotationLabel(double x, double y, double w, double h, String Name, int index)
        {
            NotationLabel label = null;
            formMaskWindow.Dispatcher.Invoke(() => {
                label = new NotationLabel(x, y, w, h, Name);
                notationLabels.Add(label);
                candidateIndexes.Add(index);
                if (index == 0) SetFocus(0);
                formMaskWindow.Canvas.Dispatcher.Invoke(() =>
                {
                    formMaskWindow.Canvas.Children.Add(label);
                }, DispatcherPriority.Render);
            }, DispatcherPriority.Render);
            return label;
        }

        public static void SetFocus(int index)
        {
            if (focusIndex == index) return;
            if (focusIndex >= 0)
            {
                if (candidateIndexes.Contains(focusIndex))
                    notationLabels[focusIndex].SetStatus(NotationLabelStatus.Candidate);
                else
                    notationLabels[focusIndex].SetStatus(NotationLabelStatus.Other);
            }
            notationLabels[index].SetStatus(NotationLabelStatus.Focus);
            focusIndex = index;
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
