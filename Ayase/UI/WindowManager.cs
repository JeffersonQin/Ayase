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

        public static Dictionary<int, NotationLabel> notationLabels;

        public static List<int> candidateIndexes;

        public static int focusIndex;

        public static Dictionary<int, List<string>> MatchStrings;

        public static object MatchStringsLocker = new object();

        public static void InitializeComponents()
        {
            screenMaskWindow = new ScreenMaskWindow();
            formMaskWindow = new FormMaskWindow();
            searchWindow = new SearchWindow();
            notificationManager = new NotificationManager();
            notationLabels = new Dictionary<int, NotationLabel>();
            candidateIndexes = new List<int>();
            MatchStrings = new Dictionary<int, List<string>>();
        }

        #region base function: hide & present
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
        #endregion

        #region wrapped hide & present
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
                // Magic number 3: the border of formMaskWindow
                formMaskWindow.Top = y - 3;
                formMaskWindow.Left = x - 3;
                formMaskWindow.Width = w + 3 * 2;
                formMaskWindow.Height = h + 3 * 2;
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
        #endregion

        #region start & end process
        public static void StartProcess()
        {
            ProcessStopFlag = -1;
            RenderFinishedFlag = 0;
            focusIndex = -1;
        }

        public static void EndProcess()
        {
            if (ProcessStopFlag == 1) return;
            else ProcessStopFlag = 1;

            formMaskWindow.Dispatcher.Invoke(() =>
            {
                formMaskWindow.Canvas.Children.Clear();
            }, DispatcherPriority.Render);
            HideFormMask();
            HideScreenMask();
            HideSearchWindow();

            while (RenderFinishedFlag == 0) Thread.Sleep(10);

            if (UIElements != IntPtr.Zero) GNativeIUIAutomationManager.DeleteLeafElements(UIElements);
            UIElements = IntPtr.Zero;
            UIElementCount = 0;

            notationLabels.Clear();
            candidateIndexes.Clear();
            focusIndex = -1;
            MatchStrings.Clear();

            ProcessStopFlag = 0;
        }
        #endregion

        public static void FinishRender()
        {
            RenderFinishedFlag = 1;
        }

        public static NotationLabel AddNotationLabel(double x, double y, double w, double h, String Name, int index)
        {
            NotationLabel label = null;
            formMaskWindow.Dispatcher.Invoke(() => {
                label = new NotationLabel(x, y, w, h, Name);
                candidateIndexes.Add(index);
                notationLabels[index] = label;
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
            if (focusIndex >= 0)
            {
                if (candidateIndexes.Contains(focusIndex))
                    notationLabels[focusIndex].SetStatus(NotationLabelStatus.Candidate);
                else
                    notationLabels[focusIndex].SetStatus(NotationLabelStatus.Other);
            }
            focusIndex = index;
            if (index >= 0)
                notationLabels[index].SetStatus(NotationLabelStatus.Focus);
        }

        public static void SetFocusNext()
        {
            if (UIElementCount > 0 && focusIndex >= 0)
                SetFocus(candidateIndexes[(candidateIndexes.IndexOf(focusIndex) + 1) % candidateIndexes.Count]);
        }

        public static void SetFocusPrevious()
        {
            if (UIElementCount > 0 && focusIndex >= 0)
            {
                int candidateIndex = candidateIndexes.IndexOf(focusIndex);
                if (candidateIndex > 0)
                    SetFocus(candidateIndexes[candidateIndex - 1]);
                else
                    SetFocus(candidateIndexes[^1]);
            }
        }

        public static void FocusSearch()
        {
            PresentSearchWindow();
        }
    }
}
