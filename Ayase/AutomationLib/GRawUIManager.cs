using Ayase.Wrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace Ayase.AutomationLib
{
    public static class GRawUIManager
    {

        public static String GetElementName(IntPtr hWnd)
        {
            int length = Win32API.GetWindowTextLength(hWnd);
            StringBuilder name = new StringBuilder(length);
            Win32API.GetWindowText(hWnd, name, name.Capacity);
            return name.ToString();
        }


        public static Rect GetElementBounds(IntPtr hWnd)
        {
            Win32API.GetWindowRect(hWnd, out Win32API.RECT rect);
            return new Rect(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom));
        }


        public static GUIElement GetUIElement(IntPtr hWnd)
        {
            return new GUIElement(GetElementBounds(hWnd), GetElementName(hWnd));
        }


        public static List<GUIElement> GetLeafElements(IntPtr hWnd)
        {
            List<GUIElement> res = new List<GUIElement>();
            try
            {
                Rect bounds = GetElementBounds(hWnd);
                if (!JudgeBounding(bounds)) return res;
                GetLeafElementsDFS(hWnd, res, bounds);
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }


        private static bool JudgeBounding(Rect bounds)
        {
            try
            {
                if (bounds == null)
                    return false;
                if (bounds.Width == 0 || bounds.Height == 0)
                    return false;
                if (bounds.IsEmpty)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static void GetLeafElementsDFS(IntPtr hWnd, List<GUIElement> res, Rect bounds)
        {
            IntPtr childHWnd = IntPtr.Zero;
            int child_cnt = 0;
            while (true)
            {
                childHWnd = Win32API.FindWindowEx(hWnd, childHWnd, null, null);
                Debug.WriteLine(childHWnd);
                if (childHWnd == IntPtr.Zero) break;
                child_cnt ++;
                Rect rect = GetElementBounds(childHWnd);
                if (!JudgeBounding(rect)) continue;
                if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                GetLeafElementsDFS(childHWnd, res, rect);
            }
            if (child_cnt == 0) res.Add(GetUIElement(hWnd));
        }
    }
}
