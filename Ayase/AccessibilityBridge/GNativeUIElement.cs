using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ayase.AccessibilityBridge
{
    public static class GNativeUIElement
    {
        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetName(IntPtr element, out IntPtr name);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int GetBoundingRectangle(IntPtr element, out double x, out double y, out double w, out double h);

        [DllImport("../Ayase.Accessibility.dll")]
        public static extern int DeleteUIElement(IntPtr element);
    }
}
