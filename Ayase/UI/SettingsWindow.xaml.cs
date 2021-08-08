using Ayase.AccessibilityBridge;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ayase.UI
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : ReuseWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            GNativeIUIAutomationManager.InitializeUIAutomation();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                GNativeIUIAutomationManager.GetForegroundWindowElement(out IntPtr foregroundWindow);
                DateTime ts_start, ts_end;
                ts_start = DateTime.UtcNow;
                GNativeIUIAutomationManager.GetLeafElementsFromWindow
                    (foregroundWindow, out IntPtr leafElements, out int elementCount, ref WindowManager.ProcessStopFlag);
                WindowManager.UIElements = leafElements;
                ts_end = DateTime.UtcNow;
                if (WindowManager.ProcessStopFlag > 0)
                {
                    WindowManager.FinishRender();
                    return;
                }
                //MessageBox.Show("Time Spent: " + (ts_end - ts_start).TotalSeconds.ToString());
                Debug.WriteLine("element count: " + elementCount);
                for (int i = 0; i < elementCount; i++)
                {
                    Debug.WriteLine("i: " + i);
                    if (WindowManager.ProcessStopFlag > 0)
                    {
                        WindowManager.FinishRender();
                        return;
                    }
                    GNativeIUIAutomationManager.GetGUIElement(leafElements, i, out IntPtr element);
                    GNativeUIElement.GetName(element, out IntPtr namePtr);
                    GNativeUIElement.GetBoundingRectangle(element, out double x, out double y, out double w, out double h);
                    Debug.WriteLine("Name: " + Marshal.PtrToStringUni(namePtr));
                    Debug.WriteLine("x: " + x + "; y: " + y + "; w: " + w + "; h: " + h);
                }

                WindowManager.FinishRender();
            });
        }
    }
}
