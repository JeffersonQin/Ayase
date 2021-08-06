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
        public static extern int GetLeafElementsFromForegroundWindow(out IntPtr leafElements, out int elementCount);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int DeleteLeafElements(IntPtr leafElements);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetGUIElement(IntPtr leafElements, int index, out IntPtr element);
    }
}
