namespace Microsoft.Data.OData
{
    using System;
    using System.IO;

    internal sealed class ODataBatchOperationWriteStream : ODataBatchOperationStream
    {
        private Stream batchStream;

        internal ODataBatchOperationWriteStream(Stream batchStream, IODataBatchOperationListener listener) : base(listener)
        {
            this.batchStream = batchStream;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            base.ValidateNotDisposed();
            return this.batchStream.BeginWrite(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.batchStream = null;
            }
            base.Dispose(disposing);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.ValidateNotDisposed();
            this.batchStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            base.ValidateNotDisposed();
            this.batchStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            base.ValidateNotDisposed();
            this.batchStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.ValidateNotDisposed();
            this.batchStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                base.ValidateNotDisposed();
                return this.batchStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                base.ValidateNotDisposed();
                return this.batchStream.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

