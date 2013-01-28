using System;
using System.Threading;

namespace System.Management.Automation.Interpreter
{
    internal class ThreadLocal<T>
    {
        private ThreadLocal<T>.StorageInfo[] _stores;

        private readonly static ThreadLocal<T>.StorageInfo[] Updating;

        private readonly bool _refCounted;

        public T Value
        {
            get
            {
                return this.GetStorageInfo().Value;
            }
            set
            {
                this.GetStorageInfo().Value = value;
            }
        }

        static ThreadLocal()
        {
            ThreadLocal<T>.Updating = new ThreadLocal<T>.StorageInfo[0];
        }

        public ThreadLocal()
        {
        }

        public ThreadLocal(bool refCounted)
        {
            this._refCounted = refCounted;
        }

        private ThreadLocal<T>.StorageInfo CreateStorageInfo()
        {
            ThreadLocal<T>.StorageInfo storageInfo;
            Thread.BeginCriticalRegion();
            ThreadLocal<T>.StorageInfo[] updating = ThreadLocal<T>.Updating;
            try
            {
                int currentThreadId = ThreadLocal<T>.GetCurrentThreadId();
                ThreadLocal<T>.StorageInfo storageInfo1 = new ThreadLocal<T>.StorageInfo(Thread.CurrentThread);
                while (true)
                {
                    ThreadLocal<T>.StorageInfo[] storageInfoArray = Interlocked.Exchange<ThreadLocal<T>.StorageInfo[]>(ref this._stores, ThreadLocal<T>.Updating);
                    updating = storageInfoArray;
                    if (storageInfoArray != ThreadLocal<T>.Updating)
                    {
                        break;
                    }
                    Thread.Sleep(0);
                }
                if (updating != null)
                {
                    if ((int)updating.Length <= currentThreadId)
                    {
                        ThreadLocal<T>.StorageInfo[] storageInfoArray1 = new ThreadLocal<T>.StorageInfo[currentThreadId + 1];
                        for (int i = 0; i < (int)updating.Length; i++)
                        {
                            if (updating[i] != null && updating[i].Thread.IsAlive)
                            {
                                storageInfoArray1[i] = updating[i];
                            }
                        }
                        updating = storageInfoArray1;
                    }
                }
                else
                {
                    updating = new ThreadLocal<T>.StorageInfo[currentThreadId + 1];
                }
                ThreadLocal<T>.StorageInfo storageInfo2 = storageInfo1;
                ThreadLocal<T>.StorageInfo storageInfo3 = storageInfo2;
                updating[currentThreadId] = storageInfo2;
                storageInfo = storageInfo3;
            }
            finally
            {
                if (updating != ThreadLocal<T>.Updating)
                {
                    Interlocked.Exchange<ThreadLocal<T>.StorageInfo[]>(ref this._stores, updating);
                }
                Thread.EndCriticalRegion();
            }
            return storageInfo;
        }

        private static int GetCurrentThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public T GetOrCreate(Func<T> func)
        {
            ThreadLocal<T>.StorageInfo storageInfo = this.GetStorageInfo();
            T value = storageInfo.Value;
            if (value == null)
            {
                T t = func();
                value = t;
                storageInfo.Value = t;
            }
            return value;
        }

        public ThreadLocal<T>.StorageInfo GetStorageInfo()
        {
            return this.GetStorageInfo(this._stores);
        }

        private ThreadLocal<T>.StorageInfo GetStorageInfo(ThreadLocal<T>.StorageInfo[] curStorage)
        {
            int currentThreadId = ThreadLocal<T>.GetCurrentThreadId();
            if (curStorage != null && (int)curStorage.Length > currentThreadId)
            {
                ThreadLocal<T>.StorageInfo storageInfo = curStorage[currentThreadId];
                if (storageInfo != null && (this._refCounted || storageInfo.Thread == Thread.CurrentThread))
                {
                    return storageInfo;
                }
            }
            return this.RetryOrCreateStorageInfo(curStorage);
        }

        private ThreadLocal<T>.StorageInfo RetryOrCreateStorageInfo(ThreadLocal<T>.StorageInfo[] curStorage)
        {
            if (curStorage != ThreadLocal<T>.Updating)
            {
                return this.CreateStorageInfo();
            }
            else
            {
                while (true)
                {
                    ThreadLocal<T>.StorageInfo[] storageInfoArray = this._stores;
                    curStorage = storageInfoArray;
                    if (storageInfoArray != ThreadLocal<T>.Updating)
                    {
                        break;
                    }
                    Thread.Sleep(0);
                }
                return this.GetStorageInfo(curStorage);
            }
        }

        public T Update(Func<T, T> updater)
        {
            ThreadLocal<T>.StorageInfo storageInfo = this.GetStorageInfo();
            T value = updater(storageInfo.Value);
            T t = value;
            storageInfo.Value = value;
            return t;
        }

        public T Update(T newValue)
        {
            ThreadLocal<T>.StorageInfo storageInfo = this.GetStorageInfo();
            T value = storageInfo.Value;
            storageInfo.Value = newValue;
            return value;
        }

        internal sealed class StorageInfo
        {
            internal readonly Thread Thread;

            public T Value;

            internal StorageInfo(Thread curThread)
            {
                this.Thread = curThread;
            }
        }
    }
}