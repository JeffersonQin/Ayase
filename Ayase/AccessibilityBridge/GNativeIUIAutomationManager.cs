using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.AccessibilityBridge
{
    public static class GNativeIUIAutomationManager
    {
        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int InitializeUIAutomation();

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetForegroundWindowElement(out IntPtr foundWindow);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetElementName(IntPtr element, out IntPtr name);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetElementBounds(IntPtr element, out double x, out double y, out double w, out double h);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetLeafElementsFromWindow(IntPtr element, out IntPtr leafElements, out int elementCount, ref int stopFlag);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int DeleteLeafElements(IntPtr leafElements);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetGUIElement(IntPtr leafElements, int index, out IntPtr element);
    }
}
