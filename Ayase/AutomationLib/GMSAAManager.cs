using Accessibility;
using Ayase.AutomationLib.Wrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Ayase.AutomationLib
{
    public static class GMSAAManager
    {

        public static String GetElementName(IAccessible element)
        {
            try
            {
                return element.get_accName((int)ObjectIdentifiers.CHILDID_SELF);
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static Rect GetElementBounds(IAccessible element)
        {
            try
            {
                element.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, (int)ObjectIdentifiers.CHILDID_SELF);
                return new Rect(pxLeft, pyTop, pcxWidth, pcyHeight);
            }
            catch (Exception)
            {
                return new Rect(0, 0, 0, 0);
            }
        }


        public static IAccessible[] GetChildren(IAccessible element)
        {
            try
            {
                IAccessible[] children = new IAccessible[element.accChildCount];
                int obtained = 0;
                Win32API.AccessibleChildren(element, 0, element.accChildCount, children, ref obtained);
                if (obtained == 0) return null;
                return children;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static GUIElement GetElement(IAccessible element)
        {
            return new GUIElement(GetElementBounds(element), GetElementName(element));
        }


        public static GUIElement GetElement(IAccessible element, Rect bounds)
        {
            return new GUIElement(bounds, GetElementName(element));
        }
        
        public static List<GUIElement> GetLeafElements(IAccessible element)
        {
            List<GUIElement> res = new List<GUIElement>();
            try
            {
                Rect bounds = GetElementBounds(element);
                if (!JudgeBounding(bounds)) return res;
                GetLeafElementsDFS(element, res, bounds);
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }


        public static IAccessible GetWindow(IntPtr hWnd)
        {
            object obj = null;
            Win32API.AccessibleObjectFromWindow(hWnd, (uint)ObjectIdentifiers.OBJID_WINDOW, ref ReferenceIdentifiers.IID_IAccessible, ref obj);
            return (IAccessible)obj;
        }


        private static bool JudgeBounding(Rect bounds)
        {
            try
            {
                if (bounds.Width == 0 || bounds.Height == 0)
                    return false;
                return !bounds.IsEmpty;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static void GetLeafElementsDFS(IAccessible element, List<GUIElement> res, Rect bounds)
        {
            var children = GetChildren(element);
            if (children == null)
            {
                res.Add(GetElement(element, bounds));
                return;
            }
            if (children.Length == 0)
            {
                res.Add(GetElement(element, bounds));
                return;
            }
            foreach (var child in children)
            {
                if (child == null) continue;
                if (!child.GetType().IsCOMObject) continue;
                Rect rect = GetElementBounds(child);
                if (!JudgeBounding(rect)) continue;
                if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                GetLeafElementsDFS(child, res, rect);
            }
        }
        
    }
}
