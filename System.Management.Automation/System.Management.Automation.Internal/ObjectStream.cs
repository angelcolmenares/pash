namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal sealed class ObjectStream : ObjectStreamBase, IDisposable
    {
        private int _capacity;
        private bool _disposed;
        private bool _isOpen;
        private object _monitorObject;
        private PipelineReader<PSObject> _mshreader;
        private ArrayList _objects;
        private ManualResetEvent _readClosedHandle;
        private PipelineReader<object> _reader;
        private AutoResetEvent _readHandle;
        private ManualResetEvent _readWaitHandle;
        private ManualResetEvent _writeClosedHandle;
        private AutoResetEvent _writeHandle;
        private PipelineWriter _writer;
        private ManualResetEvent _writeWaitHandle;

        internal ObjectStream() : this(0x7fffffff)
        {
        }

        internal ObjectStream(int capacity)
        {
            this._capacity = 0x7fffffff;
            this._monitorObject = new object();
            if ((capacity <= 0) || (capacity > 0x7fffffff))
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("capacity", capacity);
            }
            this._capacity = capacity;
            this._readHandle = new AutoResetEvent(false);
            this._writeHandle = new AutoResetEvent(true);
            this._readClosedHandle = new ManualResetEvent(false);
            this._writeClosedHandle = new ManualResetEvent(false);
            this._objects = new ArrayList();
            this._isOpen = true;
        }

        internal override void Close()
        {
            bool flag = false;
            try
            {
                lock (this._monitorObject)
                {
                    if (this._isOpen)
                    {
                        flag = true;
                        this._isOpen = false;
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    try
                    {
                        this._writeClosedHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    this.RaiseEvents();
                }
            }
        }

        private void DFT_AddHandler_OnDataReady(EventHandler eventHandler)
        {
            base.DataReady += eventHandler;
        }

        private void DFT_RemoveHandler_OnDataReady(EventHandler eventHandler)
        {
            base.DataReady -= eventHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                lock (this._monitorObject)
                {
                    if (this._disposed)
                    {
                        return;
                    }
                    this._disposed = true;
                }
                if (disposing)
                {
                    this._readHandle.Close();
                    this._writeHandle.Close();
                    this._writeClosedHandle.Close();
                    this._readClosedHandle.Close();
                    if (this._readWaitHandle != null)
                    {
                        this._readWaitHandle.Close();
                    }
                    if (this._writeWaitHandle != null)
                    {
                        this._writeWaitHandle.Close();
                    }
                    if (this._reader != null)
                    {
                        this._reader.Close();
                        this._reader.WaitHandle.Close();
                    }
                    if (this._writer != null)
                    {
                        this._writer.Close();
                        this._writer.WaitHandle.Close();
                    }
                }
            }
        }

        internal override void Flush()
        {
            bool flag = false;
            try
            {
                lock (this._monitorObject)
                {
                    if (this._objects.Count > 0)
                    {
                        flag = true;
                        this._objects.Clear();
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.RaiseEvents();
                }
            }
        }

        internal override Collection<object> NonBlockingRead(int maxRequested)
        {
            Collection<object> collection = null;
            bool flag = false;
            if (maxRequested == 0)
            {
                return new Collection<object>();
            }
            if (maxRequested < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("maxRequested", maxRequested);
            }
            try
            {
                lock (this._monitorObject)
                {
                    int count = this._objects.Count;
                    if (count > maxRequested)
                    {
                        count = maxRequested;
                    }
                    if (count > 0)
                    {
                        collection = new Collection<object>();
                        for (int i = 0; i < count; i++)
                        {
                            collection.Add(this._objects[i]);
                        }
                        flag = true;
                        this._objects.RemoveRange(0, count);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.RaiseEvents();
                }
            }
            if (collection == null)
            {
                collection = new Collection<object>();
            }
            return collection;
        }

        internal override object Peek()
        {
            lock (this._monitorObject)
            {
                if (this.EndOfPipeline || (this._objects.Count == 0))
                {
                    return AutomationNull.Value;
                }
                return this._objects[0];
            }
        }

        private void RaiseEvents()
        {
            bool flag = true;
            bool flag2 = true;
            bool flag3 = false;
            try
            {
                lock (this._monitorObject)
                {
                    flag = !this._isOpen || (this._objects.Count > 0);
                    flag2 = !this._isOpen || (this._objects.Count < this._capacity);
                    flag3 = !this._isOpen && (this._objects.Count == 0);
                    if (this._readWaitHandle != null)
                    {
                        try
                        {
                            if (flag)
                            {
                                this._readWaitHandle.Set();
                            }
                            else
                            {
                                this._readWaitHandle.Reset();
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                    if (this._writeWaitHandle != null)
                    {
                        try
                        {
                            if (flag2)
                            {
                                this._writeWaitHandle.Set();
                            }
                            else
                            {
                                this._writeWaitHandle.Reset();
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    try
                    {
                        this._readHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (flag2)
                {
                    try
                    {
                        this._writeHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (flag3)
                {
                    try
                    {
                        this._readClosedHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
            if (flag)
            {
                base.FireDataReadyEvent(this, new EventArgs());
            }
        }

        internal override object Read()
        {
            Collection<object> collection = this.Read(1);
            if (collection.Count == 1)
            {
                return collection[0];
            }
            return AutomationNull.Value;
        }

        internal override Collection<object> Read(int count)
        {
            if (count < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("count", count);
            }
            if (count == 0)
            {
                return new Collection<object>();
            }
            Collection<object> collection = new Collection<object>();
            bool flag = false;
            while ((count > 0) && this.WaitRead())
            {
                try
                {
                    lock (this._monitorObject)
                    {
                        if (this._objects.Count == 0)
                        {
                            continue;
                        }
                        flag = true;
                        int num = 0;
                        foreach (object obj2 in this._objects)
                        {
                            collection.Add(obj2);
                            num++;
                            if (--count <= 0)
                            {
                                break;
                            }
                        }
                        this._objects.RemoveRange(0, num);
                    }
                    continue;
                }
                finally
                {
                    if (flag)
                    {
                        this.RaiseEvents();
                    }
                }
            }
            return collection;
        }

        internal override Collection<object> ReadToEnd()
        {
            return this.Read(0x7fffffff);
        }

        private bool WaitRead()
        {
            if (!this.EndOfPipeline)
            {
                try
                {
                    WaitHandle.WaitAny(new WaitHandle[] { this._readHandle, this._readClosedHandle });
                }
                catch (ObjectDisposedException)
                {
                }
            }
            return !this.EndOfPipeline;
        }

        private bool WaitWrite()
        {
            if (this.IsOpen)
            {
                try
                {
                    WaitHandle.WaitAny(new WaitHandle[] { this._writeHandle, this._writeClosedHandle });
                }
                catch (ObjectDisposedException)
                {
                }
            }
            return this.IsOpen;
        }

        internal override int Write(object obj, bool enumerateCollection)
        {
            if (obj == AutomationNull.Value)
            {
                return 0;
            }
            if (!this.IsOpen)
            {
                Exception exception = new PipelineClosedException(PipelineStrings.WriteToClosedPipeline);
                throw exception;
            }
            ArrayList c = new ArrayList();
            IEnumerable enumerable = null;
            if (enumerateCollection)
            {
                enumerable = LanguagePrimitives.GetEnumerable(obj);
            }
            if (enumerable == null)
            {
                c.Add(obj);
            }
            else
            {
                foreach (object obj2 in enumerable)
                {
                    if (AutomationNull.Value != obj2)
                    {
                        c.Add(obj2);
                    }
                }
            }
            int index = 0;
            int count = c.Count;
            while (count > 0)
            {
                bool flag = false;
                if (!this.WaitWrite())
                {
                    return index;
                }
                try
                {
                    lock (this._monitorObject)
                    {
                        if (!this.IsOpen)
                        {
                            return index;
                        }
                        int num3 = this._capacity - this._objects.Count;
                        if (0 < num3)
                        {
                            int num4 = count;
                            if (num4 > num3)
                            {
                                num4 = num3;
                            }
                            try
                            {
                                if (num4 == c.Count)
                                {
                                    this._objects.AddRange(c);
                                    index += num4;
                                    count -= num4;
                                }
                                else
                                {
                                    ArrayList range = c.GetRange(index, num4);
                                    this._objects.AddRange(range);
                                    index += num4;
                                    count -= num4;
                                }
                            }
                            finally
                            {
                                flag = true;
                            }
                        }
                    }
                    continue;
                }
                finally
                {
                    if (flag)
                    {
                        this.RaiseEvents();
                    }
                }
            }
            return index;
        }

        internal override int Count
        {
            get
            {
                lock (this._monitorObject)
                {
                    return this._objects.Count;
                }
            }
        }

        internal override bool EndOfPipeline
        {
            get
            {
                lock (this._monitorObject)
                {
                    return ((this._objects.Count == 0) && !this._isOpen);
                }
            }
        }

        internal override bool IsOpen
        {
            get
            {
                lock (this._monitorObject)
                {
                    return this._isOpen;
                }
            }
        }

        internal override int MaxCapacity
        {
            get
            {
                return this._capacity;
            }
        }

        internal override PipelineReader<object> ObjectReader
        {
            get
            {
                lock (this._monitorObject)
                {
                    if (this._reader == null)
                    {
                        this._reader = new System.Management.Automation.Internal.ObjectReader(this);
                    }
                    return this._reader;
                }
            }
        }

        internal override PipelineWriter ObjectWriter
        {
            get
            {
                lock (this._monitorObject)
                {
                    if (this._writer == null)
                    {
                        this._writer = new System.Management.Automation.Internal.ObjectWriter(this);
                    }
                    return this._writer;
                }
            }
        }

        internal override PipelineReader<PSObject> PSObjectReader
        {
            get
            {
                lock (this._monitorObject)
                {
                    if (this._mshreader == null)
                    {
                        this._mshreader = new System.Management.Automation.Internal.PSObjectReader(this);
                    }
                    return this._mshreader;
                }
            }
        }

        internal override WaitHandle ReadHandle
        {
            get
            {
                lock (this._monitorObject)
                {
                    if (this._readWaitHandle == null)
                    {
                        this._readWaitHandle = new ManualResetEvent((this._objects.Count > 0) || !this._isOpen);
                    }
                    return this._readWaitHandle;
                }
            }
        }

        internal override WaitHandle WriteHandle
        {
            get
            {
                lock (this._monitorObject)
                {
                    if (this._writeWaitHandle == null)
                    {
                        this._writeWaitHandle = new ManualResetEvent((this._objects.Count < this._capacity) || !this._isOpen);
                    }
                    return this._writeWaitHandle;
                }
            }
        }
    }
}

