using Ayase.ThreadLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;

namespace Ayase.AutomationLib
{
    public static class IAutomationElementManager
    {

        /// <summary>
        /// Get the AutomationElement of Desktop (Root)
        /// </summary>
        /// <returns></returns>
        public static AutomationElement GetDesktop()
        {
            return AutomationElement.RootElement;
        }

        
        /// <summary>
        /// Get the AutomationElement of a window by process ID (PID)
        /// </summary>
        /// <param name="processID">Process ID used to get window element</param>
        /// <returns></returns>
        public static AutomationElement GetWindowByProcessID(int processID)
        {
            return GetDesktop().FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ProcessIdProperty, processID));
        }


        /// <summary>
        /// Get the AutomationElementCollection of windows by process ID (PID)
        /// </summary>
        /// <param name="processID">Process ID used to get windows elements</param>
        /// <returns></returns>
        public static AutomationElementCollection GetWindowsByProcessID(int processID)
        {
            return GetDesktop().FindAll(TreeScope.Children, 
                new PropertyCondition(AutomationElement.ProcessIdProperty, processID));
        }


        /// <summary>
        /// Get the elements that don't have children of an element
        /// </summary>
        /// <param name="element">Input element</param>
        /// <returns></returns>
        public static List<AutomationElement> GetEndElements(AutomationElement element)
        {
            List<AutomationElement> res = new List<AutomationElement>();
            try
            {
                Rect bounds = element.Current.BoundingRectangle;
                if (!JudgeBounding(bounds)) return res;
                GetEndElementsDFS(element, res, bounds);
            }
            catch (Exception)
            {
                return res;
            }
            return res;
        }


        /// <summary>
        /// Get the elements that don't have children of a collection of elements which don't have common descendents
        /// </summary>
        /// <param name="collection">A collection of elements which don't have common descendents</param>
        /// <returns></returns>
        public static List<AutomationElement> GetEndElements(AutomationElementCollection collection)
        {
            List<AutomationElement> res = new List<AutomationElement>();
            foreach (AutomationElement element in collection)
            {
                res.AddRange(GetEndElements(element));
            }
            return res;
        }


        public static ConcurrentQueue<AutomationElement> GetEndElementsByParallel(AutomationElement element, int concurrentThread)
        {
            ConcurrentQueue<AutomationElement> res = new ConcurrentQueue<AutomationElement>();
            try
            {
                Rect bounds = element.Current.BoundingRectangle;
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


        private static void GetEndElementsDFS(AutomationElement element, List<AutomationElement> res, Rect bounds)
        {
            AutomationElementCollection children = element.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
            foreach (AutomationElement e in children)
            {
                try
                {
                    Rect rect = e.Current.BoundingRectangle;
                    if (!JudgeBounding(rect)) continue;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                    GetEndElementsDFS(e, res, rect);
                } catch (Exception)
                {
                    continue;
                }
            }
            if (children.Count == 0) res.Add(element);
        }


        private static void GetEndElementsDFS(AutomationElement element, ConcurrentQueue<AutomationElement> res, IThreadPool<ThreadStart> threadPool, Rect bounds)
        {
            AutomationElementCollection children = element.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
            foreach (AutomationElement e in children)
            {
                try
                {
                    Rect rect = e.Current.BoundingRectangle;
                    if (!JudgeBounding(rect)) continue;
                    if (rect.Bottom <= bounds.Top || rect.Right <= bounds.Left) continue;
                    if (rect.Top >= bounds.Bottom || rect.Left >= bounds.Right) continue;
                    if (threadPool.IdleWorkerCount > 0)
                    {
                        threadPool.EnqueueTask(() =>
                        {
                            GetEndElementsDFS(e, res, threadPool, rect);
                        });
                    } else
                    {
                        GetEndElementsDFS(e, res, threadPool, rect);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (children.Count == 0) res.Enqueue(element);
        }

    }
}
