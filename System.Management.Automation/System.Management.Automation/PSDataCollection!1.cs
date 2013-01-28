namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    public class PSDataCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, IDisposable, ISerializable
    {
        private bool _blockingEnumerator;
        private int _countNewData;
        private int _dataAddedFrequency;
        private int _lastIndex;
        private Guid _lastPsInstanceId;
        private bool _refCountIncrementedForBlockingEnumerator;
        private Guid _sourceGuid;
        private IList<T> data;
        private bool isDisposed;
        private bool isEnumerated;
        private bool isOpen;
        private ManualResetEvent readWaitHandle;
        private int refCount;
        private bool releaseOnEnumeration;
        private static string resBaseName;
        private bool serializeInput;
        private object syncObject;

        public event EventHandler Completed;

        public event EventHandler<DataAddedEventArgs> DataAdded;

        public event EventHandler<DataAddingEventArgs> DataAdding;

        internal event EventHandler<EventArgs> IdleEvent;

        static PSDataCollection()
        {
            PSDataCollection<T>.resBaseName = "PSDataBufferStrings";
        }

        public PSDataCollection() : this((IList<T>) new List<T>())
        {
        }

        public PSDataCollection(IEnumerable<T> items) : this((IList<T>) new List<T>(items))
        {
            this.Complete();
        }

        internal PSDataCollection(IList<T> listToUse)
        {
            this.isOpen = true;
            this.syncObject = new object();
            this._dataAddedFrequency = 1;
            this._sourceGuid = Guid.Empty;
            this.data = listToUse;
        }

        public PSDataCollection(int capacity) : this((IList<T>) new List<T>(capacity))
        {
        }

        protected PSDataCollection(SerializationInfo info, StreamingContext context)
        {
            this.isOpen = true;
            this.syncObject = new object();
            this._dataAddedFrequency = 1;
            this._sourceGuid = Guid.Empty;
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            IList<T> list = info.GetValue("Data", typeof(IList<T>)) as IList<T>;
            if (list == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            this.data = list;
            this._blockingEnumerator = info.GetBoolean("BlockingEnumerator");
            this._dataAddedFrequency = info.GetInt32("DataAddedCount");
            this.EnumeratorNeverBlocks = info.GetBoolean("EnumeratorNeverBlocks");
            this.isOpen = info.GetBoolean("IsOpen");
        }

        public void Add(T item)
        {
            this.InternalAdd(Guid.Empty, item);
        }

        internal void AddRef()
        {
            lock (this.syncObject)
            {
                this.refCount++;
            }
        }

        public void Clear()
        {
            lock (this.syncObject)
            {
                if (this.data != null)
                {
                    this.data.Clear();
                }
            }
        }

        public void Complete()
        {
            bool flag = false;
            bool flag2 = false;
            try
            {
                lock (this.syncObject)
                {
                    if (this.isOpen)
                    {
                        this.isOpen = false;
                        flag = true;
                        Monitor.PulseAll(this.syncObject);
                        if (this._countNewData > 0)
                        {
                            flag2 = true;
                            this._countNewData = 0;
                        }
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    if (this.readWaitHandle != null)
                    {
                        this.readWaitHandle.Set();
                    }
                    EventHandler completed = this.Completed;
                    if (completed != null)
                    {
                        completed(this, EventArgs.Empty);
                    }
                }
                if (flag2)
                {
                    this.RaiseDataAddedEvent(this._lastPsInstanceId, this._lastIndex);
                }
            }
        }

        public bool Contains(T item)
        {
            lock (this.syncObject)
            {
                if (this.serializeInput)
                {
                    item = (T) Convert.ChangeType(this.GetSerializedObject(item), typeof(T));
                }
                return this.data.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this.syncObject)
            {
                this.data.CopyTo(array, arrayIndex);
            }
        }

        private static PSDataCollection<T> CreateAndInitializeFromExplicitValue(object valueToConvert)
        {
            PSDataCollection<T> datas = new PSDataCollection<T> {
                LanguagePrimitives.ConvertTo<T>(valueToConvert)
            };
            datas.Complete();
            return datas;
        }

        internal void DecrementRef()
        {
            lock (this.syncObject)
            {
                this.refCount--;
                if ((this.refCount == 0) || (this._blockingEnumerator && (this.refCount == 1)))
                {
                    if (this.readWaitHandle != null)
                    {
                        this.readWaitHandle.Set();
                    }
                    Monitor.PulseAll(this.syncObject);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                lock (this.syncObject)
                {
                    if (this.isDisposed)
                    {
                        return;
                    }
                    this.isDisposed = true;
                }
                this.Complete();
                lock (this.syncObject)
                {
                    if (this.readWaitHandle != null)
                    {
                        this.readWaitHandle.Close();
                        this.readWaitHandle = null;
                    }
                    if (this.data != null)
                    {
                        this.data.Clear();
                    }
                }
            }
        }

        internal void FireIdleEvent()
        {
            this.IdleEvent.SafeInvoke<EventArgs>(this, null);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PSDataCollectionEnumerator<T>((PSDataCollection<T>) this, this.EnumeratorNeverBlocks);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            info.AddValue("Data", this.data);
            info.AddValue("BlockingEnumerator", this._blockingEnumerator);
            info.AddValue("DataAddedCount", this._dataAddedFrequency);
            info.AddValue("EnumeratorNeverBlocks", this.EnumeratorNeverBlocks);
            info.AddValue("IsOpen", this.isOpen);
        }

        private PSObject GetSerializedObject(object value)
        {
            PSObject result = value as PSObject;
            if (this.SerializationWouldHaveNoEffect(result))
            {
                return result;
            }
            object obj3 = PSSerializer.Deserialize(PSSerializer.Serialize(value));
            if (obj3 == null)
            {
                return (PSObject) obj3;
            }
            return PSObject.AsPSObject(obj3);
        }

        public int IndexOf(T item)
        {
            lock (this.syncObject)
            {
                return this.InternalIndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (this.syncObject)
            {
                this.InternalInsertItem(Guid.Empty, index, item);
            }
            this.RaiseEvents(Guid.Empty, index);
        }

        protected virtual void InsertItem(Guid psInstanceId, int index, T item)
        {
            this.RaiseDataAddingEvent(psInstanceId, item);
            if (this.serializeInput)
            {
                item = (T) Convert.ChangeType(this.GetSerializedObject(item), typeof(T));
            }
            this.data.Insert(index, item);
        }

        internal void InternalAdd(Guid psInstanceId, T item)
        {
            int index = -1;
            lock (this.syncObject)
            {
                index = this.data.Count;
                this.InternalInsertItem(psInstanceId, index, item);
            }
            if (index > -1)
            {
                this.RaiseEvents(psInstanceId, index);
            }
        }

        internal void InternalAddRange(Guid psInstanceId, ICollection collection)
        {
            if (collection == null)
            {
                throw PSTraceSource.NewArgumentNullException("collection");
            }
            int index = -1;
            bool flag = false;
            lock (this.syncObject)
            {
                if (!this.isOpen)
                {
                    throw PSTraceSource.NewInvalidOperationException(PSDataCollection<T>.resBaseName, "WriteToClosedBuffer", new object[0]);
                }
                index = this.data.Count;
                foreach (object obj2 in collection)
                {
                    this.InsertItem(psInstanceId, this.data.Count, (T) obj2);
                    flag = true;
                }
            }
            if (flag)
            {
                this.RaiseEvents(psInstanceId, index);
            }
        }

        private int InternalIndexOf(T item)
        {
            if (this.serializeInput)
            {
                item = (T) Convert.ChangeType(this.GetSerializedObject(item), typeof(T));
            }
            int count = this.data.Count;
            for (int i = 0; i < count; i++)
            {
                if (object.Equals(this.data[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        private void InternalInsertItem(Guid psInstanceId, int index, T item)
        {
            if (!this.isOpen)
            {
                throw PSTraceSource.NewInvalidOperationException(PSDataCollection<T>.resBaseName, "WriteToClosedBuffer", new object[0]);
            }
            this.InsertItem(psInstanceId, index, item);
        }

        public static implicit operator PSDataCollection<T>(T valueToConvert)
        {
            PSDataCollection<T> datas = new PSDataCollection<T> {
                LanguagePrimitives.ConvertTo<T>(valueToConvert)
            };
            datas.Complete();
            return datas;
        }

        public static implicit operator PSDataCollection<T>(bool valueToConvert)
        {
            return PSDataCollection<T>.CreateAndInitializeFromExplicitValue(valueToConvert);
        }

        public static implicit operator PSDataCollection<T>(byte valueToConvert)
        {
            return PSDataCollection<T>.CreateAndInitializeFromExplicitValue(valueToConvert);
        }

        public static implicit operator PSDataCollection<T>(Hashtable valueToConvert)
        {
            PSDataCollection<T> datas = new PSDataCollection<T> {
                LanguagePrimitives.ConvertTo<T>(valueToConvert)
            };
            datas.Complete();
            return datas;
        }

        public static implicit operator PSDataCollection<T>(int valueToConvert)
        {
            return PSDataCollection<T>.CreateAndInitializeFromExplicitValue(valueToConvert);
        }

        public static implicit operator PSDataCollection<T>(string valueToConvert)
        {
            return PSDataCollection<T>.CreateAndInitializeFromExplicitValue(valueToConvert);
        }

        public static implicit operator PSDataCollection<T>(object[] arrayToConvert)
        {
            PSDataCollection<T> datas = new PSDataCollection<T>();
            if (arrayToConvert != null)
            {
                foreach (object obj2 in arrayToConvert)
                {
                    datas.Add(LanguagePrimitives.ConvertTo<T>(obj2));
                }
            }
            datas.Complete();
            return datas;
        }

        internal void Pulse()
        {
            lock (this.syncObject)
            {
                Monitor.PulseAll(this.syncObject);
            }
        }

        private void RaiseDataAddedEvent(Guid psInstanceId, int index)
        {
            EventHandler<DataAddedEventArgs> dataAdded = this.DataAdded;
            if (dataAdded != null)
            {
                dataAdded(this, new DataAddedEventArgs(psInstanceId, index));
            }
        }

        private void RaiseDataAddingEvent(Guid psInstanceId, object itemAdded)
        {
            EventHandler<DataAddingEventArgs> dataAdding = this.DataAdding;
            if (dataAdding != null)
            {
                dataAdding(this, new DataAddingEventArgs(psInstanceId, itemAdded));
            }
        }

        private void RaiseEvents(Guid psInstanceId, int index)
        {
            bool flag = false;
            lock (this.syncObject)
            {
                if (this.readWaitHandle != null)
                {
                    if ((this.data.Count > 0) || !this.isOpen)
                    {
                        this.readWaitHandle.Set();
                    }
                    else
                    {
                        this.readWaitHandle.Reset();
                    }
                }
                Monitor.PulseAll(this.syncObject);
                this._countNewData++;
                if ((this._countNewData >= this._dataAddedFrequency) || ((this._countNewData > 0) && !this.isOpen))
                {
                    flag = true;
                    this._countNewData = 0;
                }
                else
                {
                    this._lastPsInstanceId = psInstanceId;
                    this._lastIndex = index;
                }
            }
            if (flag)
            {
                this.RaiseDataAddedEvent(psInstanceId, index);
            }
        }

        public Collection<T> ReadAll()
        {
            return this.ReadAndRemove(0);
        }

        internal Collection<T> ReadAndRemove(int readCount)
        {
            int num = (readCount > 0) ? readCount : 0x7fffffff;
            lock (this.syncObject)
            {
                Collection<T> collection = new Collection<T>();
                for (int i = 0; i < num; i++)
                {
                    if (this.data.Count <= 0)
                    {
                        break;
                    }
                    collection.Add(this.data[0]);
                    this.data.RemoveAt(0);
                }
                if (this.readWaitHandle != null)
                {
                    if ((this.data.Count > 0) || !this.isOpen)
                    {
                        this.readWaitHandle.Set();
                    }
                    else
                    {
                        this.readWaitHandle.Reset();
                    }
                }
                return collection;
            }
        }

        internal T ReadAndRemoveAt0()
        {
            T local = default(T);
            lock (this.syncObject)
            {
                if ((this.data != null) && (this.data.Count > 0))
                {
                    local = this.data[0];
                    this.data.RemoveAt(0);
                }
            }
            return local;
        }

        public bool Remove(T item)
        {
            lock (this.syncObject)
            {
                int index = this.InternalIndexOf(item);
                if (index < 0)
                {
                    return false;
                }
                this.RemoveItem(index);
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (this.syncObject)
            {
                if ((index < 0) || (index >= this.data.Count))
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("index", index, PSDataCollection<T>.resBaseName, "IndexOutOfRange", new object[] { 0, this.data.Count - 1 });
                }
                this.RemoveItem(index);
            }
        }

        protected virtual void RemoveItem(int index)
        {
            this.data.RemoveAt(index);
        }

        private bool SerializationWouldHaveNoEffect(PSObject result)
        {
            if (result == null)
            {
                return true;
            }
            object obj2 = PSObject.Base(result);
            return ((obj2 == null) || (InternalSerializer.IsPrimitiveKnownType(obj2.GetType()) || ((obj2 is CimInstance) || result.TypeNames[0].StartsWith("Deserialized", StringComparison.OrdinalIgnoreCase))));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (this.syncObject)
            {
                this.data.CopyTo((T[]) array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PSDataCollectionEnumerator<T>((PSDataCollection<T>) this, this.EnumeratorNeverBlocks);
        }

        int IList.Add(object value)
        {
            PSDataCollection<T>.VerifyValueType(value);
            int count = this.data.Count;
            this.InternalAdd(Guid.Empty, (T) value);
            this.RaiseEvents(Guid.Empty, count);
            return count;
        }

        bool IList.Contains(object value)
        {
            PSDataCollection<T>.VerifyValueType(value);
            return this.Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            PSDataCollection<T>.VerifyValueType(value);
            return this.IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            PSDataCollection<T>.VerifyValueType(value);
            this.Insert(index, (T) value);
        }

        void IList.Remove(object value)
        {
            PSDataCollection<T>.VerifyValueType(value);
            this.Remove((T) value);
        }

        private static void VerifyCollectionType(ICollection value)
        {
            Type[] genericArguments = value.GetType().GetGenericArguments();
            if (1 != genericArguments.Length)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            if (genericArguments[0].Equals(typeof(T)))
            {
                throw PSTraceSource.NewArgumentException("value", PSDataCollection<T>.resBaseName, "CannotConvertToGenericType", new object[] { genericArguments[0].FullName, typeof(T).FullName });
            }
        }

        private static void VerifyValueType(object value)
        {
            if (value == null)
            {
                if (typeof(T).IsValueType)
                {
                    throw PSTraceSource.NewArgumentNullException("value", PSDataCollection<T>.resBaseName, "ValueNullReference", new object[0]);
                }
            }
            else if (!(value is T))
            {
                throw PSTraceSource.NewArgumentException("value", PSDataCollection<T>.resBaseName, "CannotConvertToGenericType", new object[] { value.GetType().FullName, typeof(T).FullName });
            }
        }

        public bool BlockingEnumerator
        {
            get
            {
                lock (this.syncObject)
                {
                    return this._blockingEnumerator;
                }
            }
            set
            {
                lock (this.syncObject)
                {
                    this._blockingEnumerator = value;
                    if (this._blockingEnumerator)
                    {
                        if (!this._refCountIncrementedForBlockingEnumerator)
                        {
                            this._refCountIncrementedForBlockingEnumerator = true;
                            this.AddRef();
                        }
                    }
                    else if (this._refCountIncrementedForBlockingEnumerator)
                    {
                        this._refCountIncrementedForBlockingEnumerator = false;
                        this.DecrementRef();
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.data == null)
                    {
                        return 0;
                    }
                    return this.data.Count;
                }
            }
        }

        public int DataAddedCount
        {
            get
            {
                return this._dataAddedFrequency;
            }
            set
            {
                bool flag = false;
                lock (this.syncObject)
                {
                    this._dataAddedFrequency = value;
                    if (this._countNewData >= this._dataAddedFrequency)
                    {
                        flag = true;
                        this._countNewData = 0;
                    }
                }
                if (flag)
                {
                    this.RaiseDataAddedEvent(this._lastPsInstanceId, this._lastIndex);
                }
            }
        }

        public bool EnumeratorNeverBlocks { get; set; }

        public bool IsAutoGenerated { get; set; }

        internal bool IsEnumerated
        {
            get
            {
                lock (this.syncObject)
                {
                    return this.isEnumerated;
                }
            }
            set
            {
                lock (this.syncObject)
                {
                    this.isEnumerated = value;
                }
            }
        }

        public bool IsOpen
        {
            get
            {
                lock (this.syncObject)
                {
                    return this.isOpen;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this.syncObject)
                {
                    return this.data[index];
                }
            }
            set
            {
                lock (this.syncObject)
                {
                    if ((index < 0) || (index >= this.data.Count))
                    {
                        throw PSTraceSource.NewArgumentOutOfRangeException("index", index, PSDataCollection<T>.resBaseName, "IndexOutOfRange", new object[] { 0, this.data.Count - 1 });
                    }
                    if (this.serializeInput)
                    {
                        value = (T) Convert.ChangeType(this.GetSerializedObject(value), typeof(T));
                    }
                    this.data[index] = value;
                }
            }
        }

        internal bool PulseIdleEvent
        {
            get
            {
                return (this.IdleEvent != null);
            }
        }

        internal int RefCount
        {
            get
            {
                return this.refCount;
            }
            set
            {
                lock (this.syncObject)
                {
                    this.refCount = value;
                }
            }
        }

        internal bool ReleaseOnEnumeration
        {
            get
            {
                lock (this.syncObject)
                {
                    return this.releaseOnEnumeration;
                }
            }
            set
            {
                lock (this.syncObject)
                {
                    this.releaseOnEnumeration = value;
                }
            }
        }

        public bool SerializeInput
        {
            get
            {
                return this.serializeInput;
            }
            set
            {
                if (typeof(T) != typeof(PSObject))
                {
                    throw new NotSupportedException(PSDataBufferStrings.SerializationNotSupported);
                }
                this.serializeInput = value;
            }
        }

        internal Guid SourceId
        {
            get
            {
                lock (this.syncObject)
                {
                    return this._sourceGuid;
                }
            }
            set
            {
                lock (this.syncObject)
                {
                    this._sourceGuid = value;
                }
            }
        }

        internal object SyncObject
        {
            get
            {
                return this.syncObject;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.syncObject;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                PSDataCollection<T>.VerifyValueType(value);
                this[index] = (T) value;
            }
        }

        internal System.Threading.WaitHandle WaitHandle
        {
            get
            {
                if (this.readWaitHandle == null)
                {
                    lock (this.syncObject)
                    {
                        if (this.readWaitHandle == null)
                        {
                            this.readWaitHandle = new ManualResetEvent((this.data.Count > 0) || !this.isOpen);
                        }
                    }
                }
                return this.readWaitHandle;
            }
        }
    }
}

