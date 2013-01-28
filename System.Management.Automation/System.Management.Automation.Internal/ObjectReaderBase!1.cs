namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class ObjectReaderBase<T> : PipelineReader<T>, IDisposable
    {
        private object _monitorObject;
        protected ObjectStreamBase _stream;

        public override event EventHandler DataReady
        {
            add
            {
                lock (this._monitorObject)
                {
                    bool flag = null == this.InternalDataReady;
                    this.InternalDataReady += value;
                    if (flag)
                    {
                        this._stream.DataReady += new EventHandler(this.OnDataReady);
                    }
                }
            }
            remove
            {
                lock (this._monitorObject)
                {
                    this.InternalDataReady -= value;
                    if (this.InternalDataReady == null)
                    {
                        this._stream.DataReady -= new EventHandler(this.OnDataReady);
                    }
                }
            }
        }

        public event EventHandler InternalDataReady;

        public ObjectReaderBase([In, Out] ObjectStreamBase stream)
        {
            this._monitorObject = new object();
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream may not be null");
            }
            this._stream = stream;
        }

        public override void Close()
        {
            this._stream.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        private void OnDataReady(object sender, EventArgs args)
        {
            this.InternalDataReady.SafeInvoke(this, args);
        }

        public override int Count
        {
            get
            {
                return this._stream.Count;
            }
        }

        public override bool EndOfPipeline
        {
            get
            {
                return this._stream.EndOfPipeline;
            }
        }

        public override bool IsOpen
        {
            get
            {
                return this._stream.IsOpen;
            }
        }

        public override int MaxCapacity
        {
            get
            {
                return this._stream.MaxCapacity;
            }
        }

        public override System.Threading.WaitHandle WaitHandle
        {
            get
            {
                return this._stream.ReadHandle;
            }
        }
    }
}

