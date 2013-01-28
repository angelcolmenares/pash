using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Internal
{
    internal class ObjectReader : ObjectReaderBase<object>
    {
        public ObjectReader(ObjectStream stream)
            : base(stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._stream.Close();
            }
        }

        public override Collection<object> NonBlockingRead()
        {
            return this._stream.NonBlockingRead(0x7fffffff);
        }

        public override Collection<object> NonBlockingRead(int maxRequested)
        {
            return this._stream.NonBlockingRead(maxRequested);
        }

        public override object Peek()
        {
            return this._stream.Peek();
        }

        public override Collection<object> Read(int count)
        {
            return this._stream.Read(count);
        }

        public override object Read()
        {
            return this._stream.Read();
        }

        public override Collection<object> ReadToEnd()
        {
            return this._stream.ReadToEnd();
        }
    }
}