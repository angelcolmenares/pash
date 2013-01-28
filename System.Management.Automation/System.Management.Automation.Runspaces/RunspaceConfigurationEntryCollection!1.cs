namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Reflection;
    using System.Threading;

    public sealed class RunspaceConfigurationEntryCollection<T> : IEnumerable<T>, IEnumerable where T: RunspaceConfigurationEntry
    {
        private int _builtInEnd;
        private Collection<T> _data;
        private object _syncObject;
        private Collection<T> _updateList;

        internal event RunspaceConfigurationEntryUpdateEventHandler OnUpdate;

        public RunspaceConfigurationEntryCollection()
        {
            this._data = new Collection<T>();
            this._updateList = new Collection<T>();
            this._syncObject = new object();
        }

        public RunspaceConfigurationEntryCollection(IEnumerable<T> items)
        {
            this._data = new Collection<T>();
            this._updateList = new Collection<T>();
            this._syncObject = new object();
            if (items == null)
            {
                throw PSTraceSource.NewArgumentNullException("item");
            }
            this.AddBuiltInItem(items);
        }

        internal void AddBuiltInItem(T item)
        {
            lock (this._syncObject)
            {
                item._builtIn = true;
                this.RecordAdd(item);
                this._data.Insert(this._builtInEnd, item);
                this._builtInEnd++;
            }
        }

        internal void AddBuiltInItem(IEnumerable<T> items)
        {
            lock (this._syncObject)
            {
                foreach (T local in items)
                {
                    local._builtIn = true;
                    this.RecordAdd(local);
                    this._data.Insert(this._builtInEnd, local);
                    this._builtInEnd++;
                }
            }
        }

        public void Append(IEnumerable<T> items)
        {
            lock (this._syncObject)
            {
                foreach (T local in items)
                {
                    this.RecordAdd(local);
                    local._builtIn = false;
                    this._data.Add(local);
                }
            }
        }

        public void Append(T item)
        {
            lock (this._syncObject)
            {
                this.RecordAdd(item);
                item._builtIn = false;
                this._data.Add(item);
            }
        }

        public void Prepend(IEnumerable<T> items)
        {
            lock (this._syncObject)
            {
                int num = 0;
                foreach (T local in items)
                {
                    this.RecordAdd(local);
                    local._builtIn = false;
                    this._data.Insert(num++, local);
                    this._builtInEnd++;
                }
            }
        }

        public void Prepend(T item)
        {
            lock (this._syncObject)
            {
                this.RecordAdd(item);
                item._builtIn = false;
                this._data.Insert(0, item);
                this._builtInEnd++;
            }
        }

        private void RecordAdd(T t)
        {
            if (t.Action == UpdateAction.Remove)
            {
                t._action = UpdateAction.None;
                this._updateList.Remove(t);
            }
            else
            {
                t._action = UpdateAction.Add;
                this._updateList.Add(t);
            }
        }

        private void RecordRemove(T t)
        {
            if (t.Action == UpdateAction.Add)
            {
                t._action = UpdateAction.None;
                this._updateList.Remove(t);
            }
            else
            {
                t._action = UpdateAction.Remove;
                this._updateList.Add(t);
            }
        }

        internal void Remove(T item)
        {
            lock (this._syncObject)
            {
                int index = this._data.IndexOf(item);
                if ((index < 0) || (index >= this._data.Count))
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("index", index);
                }
                this.RecordRemove(item);
                this._data.Remove(item);
                if (index < this._builtInEnd)
                {
                    this._builtInEnd--;
                }
            }
        }

        public void RemoveItem(int index)
        {
            lock (this._syncObject)
            {
                if ((index < 0) || (index >= this._data.Count))
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("index", index);
                }
                this.RecordRemove(this._data[index]);
                this._data.RemoveAt(index);
                if (index < this._builtInEnd)
                {
                    this._builtInEnd--;
                }
            }
        }

        public void RemoveItem(int index, int count)
        {
            lock (this._syncObject)
            {
                if ((index < 0) || ((index + count) > this._data.Count))
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("index", index);
                }
                for (int i = (index + count) - 1; i >= index; i--)
                {
                    this.RecordRemove(this._data[i]);
                    this._data.RemoveAt(i);
                }
                int num2 = Math.Min(count, this._builtInEnd - index);
                if (num2 > 0)
                {
                    this._builtInEnd -= num2;
                }
            }
        }

        internal void RemovePSSnapIn(string PSSnapinName)
        {
            lock (this._syncObject)
            {
                for (int i = this._data.Count - 1; i >= 0; i--)
                {
                    T local = this._data[i];
                    if (local.PSSnapIn != null)
                    {
                        T local2 = this._data[i];
                        if (local2.PSSnapIn.Name.Equals(PSSnapinName, StringComparison.Ordinal))
                        {
                            this.RecordRemove(this._data[i]);
                            this._data.RemoveAt(i);
                            if (i < this._builtInEnd)
                            {
                                this._builtInEnd--;
                            }
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            lock (this._syncObject)
            {
                for (int i = this._data.Count - 1; i >= 0; i--)
                {
                    T local = this._data[i];
                    if (!local.BuiltIn)
                    {
                        this.RecordRemove(this._data[i]);
                        this._data.RemoveAt(i);
                    }
                }
                this._builtInEnd = this._data.Count;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this._data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._data.GetEnumerator();
        }

        public void Update()
        {
            this.Update(false);
        }

        internal void Update(bool force)
        {
            lock (this._syncObject)
            {
                if ((this.OnUpdate != null) && (force || (this._updateList.Count > 0)))
                {
                    this.OnUpdate();
                    foreach (T local in this._updateList)
                    {
                        local._action = UpdateAction.None;
                    }
                    this._updateList.Clear();
                }
            }
        }

        public int Count
        {
            get
            {
                lock (this._syncObject)
                {
                    return this._data.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this._syncObject)
                {
                    return this._data[index];
                }
            }
        }

        internal Collection<T> UpdateList
        {
            get
            {
                return this._updateList;
            }
        }
    }
}

