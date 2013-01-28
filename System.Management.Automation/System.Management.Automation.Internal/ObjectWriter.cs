namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ObjectWriter : PipelineWriter
    {
        private ObjectStreamBase _stream;

        public ObjectWriter([In, Out] ObjectStreamBase stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this._stream = stream;
        }

        public override void Close()
        {
            this._stream.Close();
        }

        public override void Flush()
        {
            this._stream.Flush();
        }

        public override int Write(object obj)
        {
            return this._stream.Write(obj);
        }

        public override int Write(object obj, bool enumerateCollection)
        {
            return this._stream.Write(obj, enumerateCollection);
        }

        public override int Count
        {
            get
            {
                return this._stream.Count;
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
                return this._stream.WriteHandle;
            }
        }
    }
}

