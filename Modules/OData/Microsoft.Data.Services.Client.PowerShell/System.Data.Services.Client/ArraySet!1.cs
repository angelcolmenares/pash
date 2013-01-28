namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Count = {count}")]
    internal struct ArraySet<T> : IEnumerable<T>, IEnumerable where T: class
    {
        private T[] items;
        private int count;
        private int version;
        public ArraySet(int capacity)
        {
            this.items = new T[capacity];
            this.count = 0;
            this.version = 0;
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public T this[int index]
        {
            get
            {
                return this.items[index];
            }
        }
        public bool Add(T item, Func<T, T, bool> equalityComparer)
        {
            if ((equalityComparer != null) && this.Contains(item, equalityComparer))
            {
                return false;
            }
            int num = this.count++;
            if ((this.items == null) || (num == this.items.Length))
            {
                Array.Resize<T>(ref this.items, Math.Min(Math.Max(num, 0x10), 0x3fffffff) * 2);
            }
            this.items[num] = item;
            this.version++;
            return true;
        }

        public bool Contains(T item, Func<T, T, bool> equalityComparer)
        {
            return (0 <= this.IndexOf(item, equalityComparer));
        }

        public IEnumerator<T> GetEnumerator()
        {
            int index = 0;
            while (true)
            {
                if (index >= this.count)
                {
                    yield break;
                }
                yield return this.items[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(T item, Func<T, T, bool> comparer)
        {
            return this.IndexOf<T>(item, new Func<T, T>(ArraySet<T>.IdentitySelect), comparer);
        }

        public int IndexOf<K>(K item, Func<T, K> select, Func<K, K, bool> comparer)
        {
            T[] items = this.items;
            if (items != null)
            {
                int count = this.count;
                for (int i = 0; i < count; i++)
                {
                    if (comparer(item, select(items[i])))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public T Remove(T item, Func<T, T, bool> equalityComparer)
        {
            int index = this.IndexOf(item, equalityComparer);
            if (0 <= index)
            {
                item = this.items[index];
                this.RemoveAt(index);
                return item;
            }
            return default(T);
        }

        public void RemoveAt(int index)
        {
            T[] items = this.items;
            int num = --this.count;
            items[index] = items[num];
            items[num] = default(T);
            if ((num == 0) && (0x100 <= items.Length))
            {
                this.items = null;
            }
            else if ((0x100 < items.Length) && (num < (items.Length / 4)))
            {
                Array.Resize<T>(ref this.items, items.Length / 2);
            }
            this.version++;
        }

        public void Sort<K>(Func<T, K> selector, Func<K, K, int> comparer)
        {
            if (this.items != null)
            {
                SelectorComparer<K> comparer2;
                comparer2.Selector = selector;
                comparer2.Comparer = comparer;
                Array.Sort<T>(this.items, 0, this.count, comparer2);
            }
        }

        public void TrimToSize()
        {
            Array.Resize<T>(ref this.items, this.count);
        }

        private static T IdentitySelect(T arg)
        {
            return arg;
        }
        

        [StructLayout(LayoutKind.Sequential)]
        private struct SelectorComparer<K> : IComparer<T>
        {
            internal Func<T, K> Selector;
            internal Func<K, K, int> Comparer;
            int IComparer<T>.Compare(T x, T y)
            {
                return this.Comparer(this.Selector(x), this.Selector(y));
            }
        }
    }
}

