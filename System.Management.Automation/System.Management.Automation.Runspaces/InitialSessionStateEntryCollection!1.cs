namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public sealed class InitialSessionStateEntryCollection<T> : IEnumerable<T>, IEnumerable where T: InitialSessionStateEntry
    {
        private Collection<T> _internalCollection;
        private object _syncObject;

        public InitialSessionStateEntryCollection()
        {
            this._syncObject = new object();
            this._internalCollection = new Collection<T>();
        }

        public InitialSessionStateEntryCollection(IEnumerable<T> items)
        {
            this._syncObject = new object();
            this._internalCollection = new Collection<T>();
            foreach (T local in items)
            {
                this._internalCollection.Add(local);
            }
        }

        public void Add(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            lock (this._syncObject)
            {
                foreach (T local in items)
                {
                    this._internalCollection.Add(local);
                }
            }
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            lock (this._syncObject)
            {
                this._internalCollection.Add(item);
            }
        }

        public void Clear()
        {
            lock (this._syncObject)
            {
                this._internalCollection.Clear();
            }
        }

        public InitialSessionStateEntryCollection<T> Clone()
        {
            InitialSessionStateEntryCollection<T> entrys;
            lock (this._syncObject)
            {
                entrys = new InitialSessionStateEntryCollection<T>();
                foreach (T local in this._internalCollection)
                {
                    entrys.Add((T) local.Clone());
                }
            }
            return entrys;
        }

        public void Remove(string name, object type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            lock (this._syncObject)
            {
                Type type2 = null;
                if (type != null)
                {
                    type2 = type as Type;
                    if (type2 == null)
                    {
                        type2 = type.GetType();
                    }
                }
                for (int i = this._internalCollection.Count - 1; i >= 0; i--)
                {
                    T local = this._internalCollection[i];
                    if (((local != null) && ((type2 == null) || (local.GetType() == type2))) && string.Equals(local.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        this._internalCollection.RemoveAt(i);
                    }
                }
            }
        }

        public void RemoveItem(int index)
        {
            lock (this._syncObject)
            {
                this._internalCollection.RemoveAt(index);
            }
        }

        public void RemoveItem(int index, int count)
        {
            lock (this._syncObject)
            {
                while (count-- > 0)
                {
                    this._internalCollection.RemoveAt(index);
                }
            }
        }

        public void Reset()
        {
            lock (this._syncObject)
            {
                this._internalCollection.Clear();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this._internalCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._internalCollection.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._internalCollection.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this._syncObject)
                {
                    return this._internalCollection[index];
                }
            }
        }

        public Collection<T> this[string name]
        {
            get
            {
                Collection<T> collection = new Collection<T>();
                lock (this._syncObject)
                {
                    foreach (T local in this._internalCollection)
                    {
                        if (local.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            collection.Add(local);
                        }
                    }
                }
                return collection;
            }
        }
    }
}

