namespace Microsoft.Data.OData
{
    using System;
    using System.IO;

    internal sealed class NonDisposingStream : Stream
    {
        private readonly Stream innerStream;

        internal NonDisposingStream(Stream innerStream)
        {
            this.innerStream = innerStream;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.innerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.innerStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return this.innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.innerStream.Position;
            }
            set
            {
                this.innerStream.Position = value;
            }
        }
    }
}

