using Ayase.ThreadLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using UIAutomationClient;

namespace Ayase.AutomationLib
{
    public static class GIUIAutomationManager
    {
        public static CUIAutomation automation = new CUIAutomation();

        private static IUIAutomationCondition ConditionNotOffScreen = 
            automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsOffscreenPropertyId, false);
        private static IUIAutomationCondition ConditionControl =
            automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsControlElementPropertyId, true);
        private static IUIAutomationCondition ConditionOverall =
            automation.CreateAndCondition(ConditionNotOffScreen, ConditionControl);
        private static IUIAutomationTreeWalker ControlWalker = automation.CreateTreeWalker(ConditionOverall);


        public static IUIAutomationElement GetDesktop()
        {
            return automation.GetRootElement();
        }


        public static IUIAutomationElement GetWindowByProcessID(int processID)
        {
            return GetDesktop().FindFirst(TreeScope.TreeScope_Children,
                automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, processID));
        }


        public static List<IUIAutomationElement> GetLeafElements(IUIAutomationElement element)
        {
            IUIAutomationElementArray descendants = element.FindAll(TreeScope.TreeScope_Descendants, ConditionOverall);
            List<IUIAutomationElement> res = new List<IUIAutomationElement>();
            for (int i = 0; i < descendants.Length; i ++)
            {
                IUIAutomationElement e = descendants.GetElement(i);
                if (e.FindFirst(TreeScope.TreeScope_Children, ConditionOverall) == null)
                    res.Add(e);
            }
            return res;
        }


        public static List<IUIAutomationElement> GetLeafElementsDFS(IUIAutomationElement element)
        {
            List<IUIAutomationElement> res = new List<IUIAutomationElement>();
            try
            {
                Rect bounds = GetBoundingRectangle(element);
                if (!JudgeBounding(bounds)) return res;
                GetLeafElementsDFS(element, res, bounds);
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }

        public static ConcurrentQueue<IUIAutomationElement> GetLeafElementsByParallel(IUIAutomationElement element, int concurrentThread)
        {
            return GetLeafElementsByParallel(element, new GThreadPool<ThreadStart>(concurrentThread, new Action<ThreadStart>((threadStart) =>
            {
                threadStart.Invoke();
            })));
        }


        public static ConcurrentQueue<IUIAutomationElement> GetLeafElementsByParallel(IUIAutomationElement element, GThreadPool<ThreadStart> threadPool)
        {
            ConcurrentQueue<IUIAutomationElement> res = new ConcurrentQueue<IUIAutomationElement>();
            try
            {
                Rect bounds = GetBoundingRectangle(element);
                if (!JudgeBounding(bounds)) return res;
                GetLeafElementsDFS(element, res, threadPool, bounds);
                threadPool.Join();
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }


        public static Rect GetBoundingRectangle(IUIAutomationElement element)
        {
            double[] bounds = (double[])element.GetCurrentPropertyValue(UIA_PropertyIds.UIA_BoundingRectanglePropertyId);
            if (bounds == null) { return new Rect(0, 0, 0, 0); }
            return new Rect(bounds[0], bounds[1], bounds[2], bounds[3]);
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
                if (double.IsInfinity(bounds.Top) ||
                    double.IsInfinity(bounds.Left) ||
                    double.IsInfinity(bounds.Width) ||
                    double.IsInfinity(bounds.Height))
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static void GetLeafElementsDFS(IUIAutomationElement element, List<IUIAutomationElement> res, Rect bounds)
        {
            IUIAutomationElement e = ControlWalker.GetFirstChildElement(element);
            if (e == null)
            {
                res.Add(element);
                return;
            }
            while (e != null)
            {
                try
                {
                    Rect rect = GetBoundingRectangle(e);
                    if (!JudgeBounding(rect)) goto FindNextSibling;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) goto FindNextSibling;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) goto FindNextSibling;
                    GetLeafElementsDFS(e, res, rect);
                }
                catch (Exception) {  }
            FindNextSibling:
                e = ControlWalker.GetNextSiblingElement(e);
            }
        }

        private static void GetLeafElementsDFS(IUIAutomationElement element, ConcurrentQueue<IUIAutomationElement> res, GThreadPool<ThreadStart> threadPool, Rect bounds)
        {
            IUIAutomationElement e = ControlWalker.GetFirstChildElement(element);
            if (e == null)
            {
                res.Enqueue(element);
                return;
            }
            while (e != null)
            {
                try
                {
                    Rect rect = GetBoundingRectangle(e);
                    if (!JudgeBounding(rect)) goto FindNextSibling;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) goto FindNextSibling;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) goto FindNextSibling;
                    if (threadPool.IdleWorkerCount > 0)
                    {
                        threadPool.EnqueueTask(() =>
                        {
                            GetLeafElementsDFS(e, res, threadPool, rect);
                        });
                    }
                    else
                    {
                        GetLeafElementsDFS(e, res, threadPool, rect);
                    }
                }
                catch (Exception) { }
            FindNextSibling:
                e = ControlWalker.GetNextSiblingElement(e);
            }
        }
    }
}
