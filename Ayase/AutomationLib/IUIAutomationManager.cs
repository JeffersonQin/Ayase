using Ayase.ThreadLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using UIAutomationClient;

namespace Ayase.IUIAutomationLib
{
    class IUIAutomationManager
    {
        public static CUIAutomation automation = new CUIAutomation();
        private static IUIAutomationCondition ConditionNotOffScreen = 
            automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsOffscreenPropertyId, false);

        public static IUIAutomationElement GetDesktop()
        {
            return automation.GetRootElement();
        }


        public static IUIAutomationElement GetWindowByProcessID(int processID)
        {
            return GetDesktop().FindFirst(TreeScope.TreeScope_Children,
                automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, processID));
        }


        public static List<IUIAutomationElement> GetEndElements(IUIAutomationElement element)
        {
            List<IUIAutomationElement> res = new List<IUIAutomationElement>();
            try
            {
                Rect bounds = GetBoundingRectangle(element);
                if (!JudgeBounding(bounds)) return res;
                GetEndElementsDFS(element, res, bounds);
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }

        public static ConcurrentQueue<IUIAutomationElement> GetEndElementsByParallel(IUIAutomationElement element, int concurrentThread)
        {
            ConcurrentQueue<IUIAutomationElement> res = new ConcurrentQueue<IUIAutomationElement>();
            try
            {
                Rect bounds = GetBoundingRectangle(element);
                if (!JudgeBounding(bounds)) return res;
                IThreadPool<ThreadStart> threadPool = new IThreadPool<ThreadStart>(concurrentThread, new Action<ThreadStart>((threadStart) =>
                {
                    threadStart.Invoke();
                }));
                GetEndElementsDFS(element, res, threadPool, bounds);
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


        private static void GetEndElementsDFS(IUIAutomationElement element, List<IUIAutomationElement> res, Rect bounds)
        {
            IUIAutomationElementArray children = element.FindAll(TreeScope.TreeScope_Children, ConditionNotOffScreen);
            for (int i = 0; i < children.Length; i ++)
            {
                IUIAutomationElement e = children.GetElement(i);
                try
                {
                    Rect rect = GetBoundingRectangle(e);
                    if (!JudgeBounding(rect)) continue;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                    GetEndElementsDFS(e, res, rect);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (children.Length == 0) res.Add(element);
        }

        private static void GetEndElementsDFS(IUIAutomationElement element, ConcurrentQueue<IUIAutomationElement> res, IThreadPool<ThreadStart> threadPool, Rect bounds)
        {
            IUIAutomationElementArray children = element.FindAll(TreeScope.TreeScope_Children, ConditionNotOffScreen);
            for (int i = 0; i < children.Length; i ++)
            {
                IUIAutomationElement e = children.GetElement(i);
                try
                {
                    Rect rect = GetBoundingRectangle(e);
                    if (!JudgeBounding(rect)) continue;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                    if (threadPool.IdleWorkerCount > 0)
                    {
                        threadPool.EnqueueTask(() =>
                        {
                            GetEndElementsDFS(e, res, threadPool, rect);
                        });
                    }
                    else
                    {
                        GetEndElementsDFS(e, res, threadPool, rect);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (children.Length == 0) res.Enqueue(element);
        }
    }
}
