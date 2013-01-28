namespace Microsoft.Management.Infrastructure.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class WeakReferenceHashSet<T> where T : class
    {
        private int _cleanupTriggerSize;
        private ConcurrentDictionary<WeakReference, object> _underlyingCollection;
        private const int InitialCleanupTriggerSize = 0x3e8;

        public WeakReferenceHashSet()
        {
            this._cleanupTriggerSize = 0x3e8;
            this._underlyingCollection = new ConcurrentDictionary<WeakReference, object>(WeakReferenceEqualityComparer.Singleton);
        }

        public void Add(T o)
        {
            WeakReference key = new WeakReference(o);
            this._underlyingCollection.TryAdd(key, null);
            this.CleanUp();
        }

        private void CleanUp()
        {
            if (this._underlyingCollection.Count > this._cleanupTriggerSize)
            {
                ConcurrentDictionary<WeakReference, object> dictionary = new ConcurrentDictionary<WeakReference, object>(from t in this.GetSnapshotOfLiveObjects() select new KeyValuePair<WeakReference, object>(new WeakReference(t), null), WeakReferenceEqualityComparer.Singleton);
                this._underlyingCollection = dictionary;
                this._cleanupTriggerSize = 0x3e8 + (this._underlyingCollection.Count * 2);
            }
        }

        public IEnumerable<T> GetSnapshotOfLiveObjects()
        {
            return (from t in
                        (from w in this._underlyingCollection.Keys select w.Target).OfType<T>()
                    where t != null
                    select t).ToList<T>();
        }

        public void Remove(T o)
        {
            object obj2;
            WeakReference key = new WeakReference(o);
            this._underlyingCollection.TryRemove(key, out obj2);
        }

        private class WeakReferenceEqualityComparer : IEqualityComparer<WeakReference>
        {
            private static readonly Lazy<WeakReferenceHashSet<T>.WeakReferenceEqualityComparer> LazySingleton;

            static WeakReferenceEqualityComparer()
            {
                WeakReferenceHashSet<T>.WeakReferenceEqualityComparer.LazySingleton = new Lazy<WeakReferenceHashSet<T>.WeakReferenceEqualityComparer>(() => new WeakReferenceHashSet<T>.WeakReferenceEqualityComparer());
            }

            public bool Equals(WeakReference x, WeakReference y)
            {
                object target = x.Target;
                if (target == null)
                {
                    return false;
                }
                object objB = y.Target;
                if (objB == null)
                {
                    return false;
                }
                return object.ReferenceEquals(target, objB);
            }

            public int GetHashCode(WeakReference obj)
            {
                object target = obj.Target;
                if (target == null)
                {
                    return RuntimeHelpers.GetHashCode(obj);
                }
                return RuntimeHelpers.GetHashCode(target);
            }

            public static WeakReferenceHashSet<T>.WeakReferenceEqualityComparer Singleton
            {
                get
                {
                    return WeakReferenceHashSet<T>.WeakReferenceEqualityComparer.LazySingleton.Value;
                }
            }
        }
    }
}

