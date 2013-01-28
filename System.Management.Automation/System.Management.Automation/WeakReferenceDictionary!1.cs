namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class WeakReferenceDictionary<T> : IDictionary<object, T>, ICollection<KeyValuePair<object, T>>, IEnumerable<KeyValuePair<object, T>>, IEnumerable
    {
        private int cleanupTriggerSize;
        private Dictionary<WeakReference, T> dictionary;
        private const int initialCleanupTriggerSize = 0x3e8;
        private readonly IEqualityComparer<WeakReference> weakEqualityComparer;

        public WeakReferenceDictionary()
        {
            this.cleanupTriggerSize = 0x3e8;
            this.weakEqualityComparer = new WeakReferenceEqualityComparer();
            this.dictionary = new Dictionary<WeakReference, T>(this.weakEqualityComparer);
        }

        public void Add(KeyValuePair<object, T> item)
        {
            this.WeakCollection.Add(WeakReferenceDictionary<T>.WeakKeyValuePair(item));
            this.CleanUp();
        }

        public void Add(object key, T value)
        {
            this.dictionary.Add(new WeakReference(key), value);
            this.CleanUp();
        }

        private void CleanUp()
        {
            if (this.Count > this.cleanupTriggerSize)
            {
                Dictionary<WeakReference, T> dictionary = new Dictionary<WeakReference, T>(this.weakEqualityComparer);
                foreach (KeyValuePair<WeakReference, T> pair in this.dictionary)
                {
                    if (pair.Key.Target != null)
                    {
                        dictionary.Add(pair.Key, pair.Value);
                    }
                }
                this.dictionary = dictionary;
                this.cleanupTriggerSize = 0x3e8 + (this.Count * 2);
            }
        }

        public void Clear()
        {
            this.WeakCollection.Clear();
        }

        public bool Contains(KeyValuePair<object, T> item)
        {
            return this.WeakCollection.Contains(WeakReferenceDictionary<T>.WeakKeyValuePair(item));
        }

        public bool ContainsKey(object key)
        {
            return this.dictionary.ContainsKey(new WeakReference(key));
        }

        public void CopyTo(KeyValuePair<object, T>[] array, int arrayIndex)
        {
            List<KeyValuePair<object, T>> list = new List<KeyValuePair<object, T>>(this.WeakCollection.Count);
            foreach (KeyValuePair<object, T> pair in this)
            {
                list.Add(pair);
            }
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<object, T>> GetEnumerator()
        {
            foreach (KeyValuePair<WeakReference, T> iteratorVariable0 in this.WeakCollection)
            {
                object target = iteratorVariable0.Key.Target;
                if (target != null)
                {
                    yield return new KeyValuePair<object, T>(target, iteratorVariable0.Value);
                }
            }
        }

        public bool Remove(KeyValuePair<object, T> item)
        {
            return this.WeakCollection.Remove(WeakReferenceDictionary<T>.WeakKeyValuePair(item));
        }

        public bool Remove(object key)
        {
            return this.dictionary.Remove(new WeakReference(key));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<KeyValuePair<object, T>> enumerable = this;
            return enumerable.GetEnumerator();
        }

        public bool TryGetValue(object key, out T value)
        {
            WeakReference reference = new WeakReference(key);
            return this.dictionary.TryGetValue(reference, out value);
        }

        private static KeyValuePair<WeakReference, T> WeakKeyValuePair(KeyValuePair<object, T> publicKeyValuePair)
        {
            return new KeyValuePair<WeakReference, T>(new WeakReference(publicKeyValuePair.Key), publicKeyValuePair.Value);
        }

        public int Count
        {
            get
            {
                return this.WeakCollection.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.WeakCollection.IsReadOnly;
            }
        }

        public T this[object key]
        {
            get
            {
                return this.dictionary[new WeakReference(key)];
            }
            set
            {
                this.dictionary[new WeakReference(key)] = value;
                this.CleanUp();
            }
        }

        public ICollection<object> Keys
        {
            get
            {
                List<object> list = new List<object>(this.dictionary.Keys.Count);
                foreach (WeakReference reference in this.dictionary.Keys)
                {
                    object target = reference.Target;
                    if (target != null)
                    {
                        list.Add(target);
                    }
                }
                return list;
            }
        }

        public ICollection<T> Values
        {
            get
            {
                return (ICollection<T>) this.dictionary.Values;
            }
        }

        private ICollection<KeyValuePair<WeakReference, T>> WeakCollection
        {
            get
            {
                return this.dictionary;
            }
        }

        

        private class WeakReferenceEqualityComparer : IEqualityComparer<WeakReference>
        {
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
        }
    }
}

