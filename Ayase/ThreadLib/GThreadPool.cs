using System;
using System.Collections.Generic;
using System.Threading;

namespace Ayase.ThreadLib
{
    /// <summary>
    /// Custom Thread Pool Implementation.
    /// Modified from https://stackoverflow.com/questions/5826981/how-to-reuse-threads-in-net-3-5,
    /// by dashton@stackoverflow, ChrisWue@stackoverflow
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GThreadPool<T> : IDisposable where T : class
    {
        private readonly object _locker = new object();
        private readonly object _idleLocker = new object();
        private readonly List<Thread> _workers;
        private readonly Queue<T> _taskQueue = new Queue<T>();
        private readonly Action<T> _dequeueAction;
        public readonly long WorkerCount;

        private int _idleWorkerCount = 0;


        /// <summary>
        /// The count of idle worker
        /// </summary>
        public int IdleWorkerCount
        {
            get { return _idleWorkerCount; }
        }

        public bool Idle;

        /// <summary>
        /// Initializes a new instance of the <see cref="GThreadPool{T}"/> class.
        /// </summary>
        /// <param name="workerCount">The worker count.</param>
        /// <param name="dequeueAction">The dequeue action.</param>
        public GThreadPool(int workerCount, Action<T> dequeueAction)
        {
            Idle = true;
            WorkerCount = workerCount;
            _idleWorkerCount = workerCount;
            _dequeueAction = dequeueAction;
            _workers = new List<Thread>(workerCount);

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i ++)
            {
                Thread t = new Thread(Consume) { IsBackground = true, Name = string.Format("IThreadPool worker {0}", i) };
                _workers.Add(t);
                t.Start();
            }
        }


        /// <summary>
        /// Enqueues the task.
        /// </summary>
        /// <param name="task">The task.</param>
        public void EnqueueTask(T task)
        {
            lock (_locker)
            {
                _taskQueue.Enqueue(task);
                Idle = false;
                Monitor.PulseAll(_locker);
            }
        }


        /// <summary>
        /// Consumes this instance.
        /// </summary>
        private void Consume()
        {
            while (true)
            {
                T item;
                lock (_locker)
                {
                    while (_taskQueue.Count == 0) Monitor.Wait(_locker);
                    item = _taskQueue.Dequeue();
                }
                if (item == null) return;

                Interlocked.Decrement(ref _idleWorkerCount);
                // run actual method
                _dequeueAction(item);
                Interlocked.Increment(ref _idleWorkerCount);
                if (_idleWorkerCount == WorkerCount && _taskQueue.Count == 0)
                {
                    lock (_idleLocker)
                    {
                        Idle = true;
                        Monitor.PulseAll(_idleLocker);
                    }
                } else Idle = false;
            }
        }


        /// <summary>
        /// Waiting for everything to finish.
        /// </summary>
        public void Join()
        {
            lock (_idleLocker)
            {
                while (!Idle) Monitor.Wait(_idleLocker);
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Enqueue one null task per worker to make each exit.
            _workers.ForEach(thread => EnqueueTask(null));
            _workers.ForEach(thread => thread.Join());
        }
    }
}
