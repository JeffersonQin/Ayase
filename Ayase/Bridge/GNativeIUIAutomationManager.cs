using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.Bridge
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

        [DllImport("../Ayase.Accessibility.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindowName();

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern IntPtr test();
    }
}
