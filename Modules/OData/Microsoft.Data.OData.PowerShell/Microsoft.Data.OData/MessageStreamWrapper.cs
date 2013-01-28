namespace Microsoft.Data.OData
{
    using System;
    using System.IO;

    internal static class MessageStreamWrapper
    {
        internal static Stream CreateNonDisposingStream(Stream innerStream)
        {
            return new MessageStreamWrappingStream(innerStream, true, -1L);
        }

        internal static Stream CreateNonDisposingStreamWithMaxSize(Stream innerStream, long maxBytesToBeRead)
        {
            return new MessageStreamWrappingStream(innerStream, true, maxBytesToBeRead);
        }

        internal static Stream CreateStreamWithMaxSize(Stream innerStream, long maxBytesToBeRead)
        {
            return new MessageStreamWrappingStream(innerStream, false, maxBytesToBeRead);
        }

        internal static bool IsNonDisposingStream(Stream stream)
        {
            MessageStreamWrappingStream stream2 = stream as MessageStreamWrappingStream;
            return ((stream2 != null) && stream2.IgnoreDispose);
        }

        private sealed class MessageStreamWrappingStream : Stream
        {
            private readonly bool ignoreDispose;
            private Stream innerStream;
            private readonly long maxBytesToBeRead;
            private long totalBytesRead;

            internal MessageStreamWrappingStream(Stream innerStream, bool ignoreDispose, long maxBytesToBeRead)
            {
                this.innerStream = innerStream;
                this.ignoreDispose = ignoreDispose;
                this.maxBytesToBeRead = maxBytesToBeRead;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return this.innerStream.BeginRead(buffer, offset, count, callback, state);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
            }

            protected override void Dispose(bool disposing)
            {
                if ((disposing && !this.ignoreDispose) && (this.innerStream != null))
                {
                    this.innerStream.Dispose();
                    this.innerStream = null;
                }
                base.Dispose(disposing);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                int bytesRead = this.innerStream.EndRead(asyncResult);
                this.IncreaseTotalBytesRead(bytesRead);
                return bytesRead;
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                this.innerStream.EndWrite(asyncResult);
            }

            public override void Flush()
            {
                this.innerStream.Flush();
            }

            private void IncreaseTotalBytesRead(int bytesRead)
            {
                if (this.maxBytesToBeRead > 0L)
                {
                    this.totalBytesRead += (bytesRead < 0) ? ((long) 0) : ((long) bytesRead);
                    if (this.totalBytesRead > this.maxBytesToBeRead)
                    {
                        throw new ODataException(Strings.MessageStreamWrappingStream_ByteLimitExceeded(this.totalBytesRead, this.maxBytesToBeRead));
                    }
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int bytesRead = this.innerStream.Read(buffer, offset, count);
                this.IncreaseTotalBytesRead(bytesRead);
                return bytesRead;
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

            internal bool IgnoreDispose
            {
                get
                {
                    return this.ignoreDispose;
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
}

