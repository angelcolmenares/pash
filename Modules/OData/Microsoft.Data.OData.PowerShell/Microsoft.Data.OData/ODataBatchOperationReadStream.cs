namespace Microsoft.Data.OData
{
    using System;

    internal abstract class ODataBatchOperationReadStream : ODataBatchOperationStream
    {
        protected ODataBatchReaderStream batchReaderStream;

        internal ODataBatchOperationReadStream(ODataBatchReaderStream batchReaderStream, IODataBatchOperationListener listener) : base(listener)
        {
            this.batchReaderStream = batchReaderStream;
        }

        internal static ODataBatchOperationReadStream Create(ODataBatchReaderStream batchReaderStream, IODataBatchOperationListener listener)
        {
            return new ODataBatchOperationReadStreamWithDelimiter(batchReaderStream, listener);
        }

        internal static ODataBatchOperationReadStream Create(ODataBatchReaderStream batchReaderStream, IODataBatchOperationListener listener, int length)
        {
            return new ODataBatchOperationReadStreamWithLength(batchReaderStream, listener, length);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private sealed class ODataBatchOperationReadStreamWithDelimiter : ODataBatchOperationReadStream
        {
            private bool exhausted;

            internal ODataBatchOperationReadStreamWithDelimiter(ODataBatchReaderStream batchReaderStream, IODataBatchOperationListener listener) : base(batchReaderStream, listener)
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                ExceptionUtils.CheckArgumentNotNull<byte[]>(buffer, "buffer");
                ExceptionUtils.CheckIntegerNotNegative(offset, "offset");
                ExceptionUtils.CheckIntegerNotNegative(count, "count");
                base.ValidateNotDisposed();
                if (this.exhausted)
                {
                    return 0;
                }
                int num = base.batchReaderStream.ReadWithDelimiter(buffer, offset, count);
                if (num < count)
                {
                    this.exhausted = true;
                }
                return num;
            }
        }

        private sealed class ODataBatchOperationReadStreamWithLength : ODataBatchOperationReadStream
        {
            private int length;

            internal ODataBatchOperationReadStreamWithLength(ODataBatchReaderStream batchReaderStream, IODataBatchOperationListener listener, int length) : base(batchReaderStream, listener)
            {
                ExceptionUtils.CheckIntegerNotNegative(length, "length");
                this.length = length;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                ExceptionUtils.CheckArgumentNotNull<byte[]>(buffer, "buffer");
                ExceptionUtils.CheckIntegerNotNegative(offset, "offset");
                ExceptionUtils.CheckIntegerNotNegative(count, "count");
                base.ValidateNotDisposed();
                if (this.length == 0)
                {
                    return 0;
                }
                int num = base.batchReaderStream.ReadWithLength(buffer, offset, Math.Min(count, this.length));
                this.length -= num;
                return num;
            }
        }
    }
}

