using Ayase.AutomationLib;
using Ayase.IUIAutomationLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIAutomationClient;

namespace Ayase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*NativeAPI.GetWindowThreadProcessId(NativeAPI.GetForegroundWindow(), out int processID);
            AutomationElement window = IAutomationManager.GetWindowByProcessID(15204);
            DateTime ts_start = DateTime.UtcNow;
            ConcurrentQueue<AutomationElement> res = IAutomationManager.GetEndElementsByParallel(window, 2);
            DateTime ts_end = DateTime.UtcNow;
            MessageBox.Show("thread time spent: " + (ts_end - ts_start).TotalSeconds.ToString() + "sec");
            Debug.WriteLine("count: " + res.Count);
            foreach (AutomationElement res_element in res)
            {
                Debug.WriteLine("------------------------");
                Debug.WriteLine("Name: " + res_element.Current.Name);
                Debug.WriteLine("Rect: " + res_element.Current.BoundingRectangle.ToString());
                Debug.WriteLine("------------------------");
                Debug.WriteLine("");
            }*/

            DateTime ts_start = DateTime.UtcNow;
            IUIAutomationElement iui_window = IUIAutomationManager.GetWindowByProcessID(15204);
            ConcurrentQueue<IUIAutomationElement> res = IUIAutomationManager.GetEndElementsByParallel(iui_window, 24);
            DateTime ts_end = DateTime.UtcNow;
            MessageBox.Show("thread time spent: " + (ts_end - ts_start).TotalSeconds.ToString() + "sec");
            foreach (IUIAutomationElement res_element in res)
            {
                Debug.WriteLine("------------------------");
                Debug.WriteLine("Name: " + res_element.GetCurrentPropertyValue(UIA_PropertyIds.UIA_NamePropertyId));
                Debug.WriteLine("Rect: " + IUIAutomationManager.GetBoundingRectangle(res_element));
                Debug.WriteLine("------------------------");
                Debug.WriteLine("");
            }
        }
    }
}
