namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal sealed class PSDataCollectionStream<T> : ObjectStreamBase
    {
        private bool _disposed;
        private PipelineReader<object> _objectReader;
        private PipelineReader<object> _objectReaderForPipeline;
        private PSDataCollection<T> _objects;
        private PipelineReader<PSObject> _psobjectReader;
        private PipelineReader<PSObject> _psobjectReaderForPipeline;
        private object _syncObject;
        private PipelineWriter _writer;
        private bool isOpen;
        private Guid psInstanceId;

        internal PSDataCollectionStream(Guid psInstanceId, PSDataCollection<T> storeToUse)
        {
            this._syncObject = new object();
            if (storeToUse == null)
            {
                throw PSTraceSource.NewArgumentNullException("storeToUse");
            }
            this._objects = storeToUse;
            this.psInstanceId = psInstanceId;
            this.isOpen = true;
            storeToUse.AddRef();
            storeToUse.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDataAdded);
            storeToUse.Completed += new EventHandler(this.HandleClosed);
        }

        internal override void Close()
        {
            bool flag = false;
            lock (this._syncObject)
            {
                if (this.isOpen)
                {
                    this._objects.DecrementRef();
                    this._objects.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleDataAdded);
                    this._objects.Completed -= new EventHandler(this.HandleClosed);
                    flag = true;
                    this.isOpen = false;
                }
            }
            if (flag)
            {
                base.FireDataReadyEvent(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                lock (this._syncObject)
                {
                    if (this._disposed)
                    {
                        return;
                    }
                    this._disposed = true;
                }
                if (disposing)
                {
                    this._objects.Dispose();
                    this.Close();
                    if (this._objectReaderForPipeline != null)
                    {
                        ((PSDataCollectionPipelineReader<T, object>) this._objectReaderForPipeline).Dispose();
                    }
                    if (this._psobjectReaderForPipeline != null)
                    {
                        ((PSDataCollectionPipelineReader<T, PSObject>) this._psobjectReaderForPipeline).Dispose();
                    }
                }
            }
        }

        internal PipelineReader<object> GetObjectReaderForPipeline(string computerName, Guid runspaceId)
        {
            if (this._objectReaderForPipeline == null)
            {
                lock (this._syncObject)
                {
                    if (this._objectReaderForPipeline == null)
                    {
                        this._objectReaderForPipeline = new PSDataCollectionPipelineReader<T, object>((PSDataCollectionStream<T>) this, computerName, runspaceId);
                    }
                }
            }
            return this._objectReaderForPipeline;
        }

        internal PipelineReader<PSObject> GetPSObjectReaderForPipeline(string computerName, Guid runspaceId)
        {
            if (this._psobjectReaderForPipeline == null)
            {
                lock (this._syncObject)
                {
                    if (this._psobjectReaderForPipeline == null)
                    {
                        this._psobjectReaderForPipeline = new PSDataCollectionPipelineReader<T, PSObject>((PSDataCollectionStream<T>) this, computerName, runspaceId);
                    }
                }
            }
            return this._psobjectReaderForPipeline;
        }

        private void HandleClosed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void HandleDataAdded(object sender, DataAddedEventArgs e)
        {
            base.FireDataReadyEvent(this, EventArgs.Empty);
        }

        internal override int Write(object obj, bool enumerateCollection)
        {
            if (obj == AutomationNull.Value)
            {
                return 0;
            }
            if (!this.IsOpen)
            {
                Exception exception = new PipelineClosedException(PSDataBufferStrings.WriteToClosedBuffer);
                throw exception;
            }
            Collection<T> collection = new Collection<T>();
            IEnumerable enumerable = null;
            if (enumerateCollection)
            {
                enumerable = LanguagePrimitives.GetEnumerable(obj);
            }
            if (enumerable == null)
            {
                collection.Add((T) LanguagePrimitives.ConvertTo(obj, typeof(T), CultureInfo.InvariantCulture));
            }
            else
            {
                foreach (object obj2 in enumerable)
                {
                    if (AutomationNull.Value != obj2)
                    {
                        collection.Add((T) LanguagePrimitives.ConvertTo(obj, typeof(T), CultureInfo.InvariantCulture));
                    }
                }
            }
            this._objects.InternalAddRange(this.psInstanceId, collection);
            return collection.Count;
        }

        internal override int Count
        {
            get
            {
                return this._objects.Count;
            }
        }

        internal override bool EndOfPipeline
        {
            get
            {
                lock (this._syncObject)
                {
                    return ((this._objects.Count == 0) && !this.isOpen);
                }
            }
        }

        internal override bool IsOpen
        {
            get
            {
                return (this.isOpen && this._objects.IsOpen);
            }
        }

        internal override int MaxCapacity
        {
            get
            {
                throw PSTraceSource.NewNotSupportedException();
            }
        }

        internal override PipelineReader<object> ObjectReader
        {
            get
            {
                if (this._objectReader == null)
                {
                    lock (this._syncObject)
                    {
                        if (this._objectReader == null)
                        {
                            this._objectReader = new PSDataCollectionReader<T, object>((PSDataCollectionStream<T>) this);
                        }
                    }
                }
                return this._objectReader;
            }
        }

        internal PSDataCollection<T> ObjectStore
        {
            get
            {
                return this._objects;
            }
        }

        internal override PipelineWriter ObjectWriter
        {
            get
            {
                if (this._writer == null)
                {
                    lock (this._syncObject)
                    {
                        if (this._writer == null)
                        {
                            this._writer = new PSDataCollectionWriter<T>((PSDataCollectionStream<T>) this);
                        }
                    }
                }
                return this._writer;
            }
        }

        internal override PipelineReader<PSObject> PSObjectReader
        {
            get
            {
                if (this._psobjectReader == null)
                {
                    lock (this._syncObject)
                    {
                        if (this._psobjectReader == null)
                        {
                            this._psobjectReader = new PSDataCollectionReader<T, PSObject>((PSDataCollectionStream<T>) this);
                        }
                    }
                }
                return this._psobjectReader;
            }
        }

        internal override WaitHandle ReadHandle
        {
            get
            {
                return this._objects.WaitHandle;
            }
        }
    }
}

