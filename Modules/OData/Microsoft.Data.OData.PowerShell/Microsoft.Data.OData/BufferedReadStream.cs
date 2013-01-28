namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class BufferedReadStream : Stream
    {
        private readonly List<DataBuffer> buffers = new List<DataBuffer>();
        private int currentBufferIndex;
        private int currentBufferReadCount;
        private Stream inputStream;

        private BufferedReadStream(Stream inputStream)
        {
            this.inputStream = inputStream;
            this.currentBufferIndex = -1;
        }

        private DataBuffer AddNewBuffer()
        {
            DataBuffer item = new DataBuffer();
            this.buffers.Add(item);
            this.currentBufferIndex = this.buffers.Count - 1;
            return item;
        }

        private IEnumerable<Task> BufferInputStream()
        {
            while (true)
            {
                if (this.inputStream == null)
                {
                    yield break;
                }
                DataBuffer currentBuffer = (this.currentBufferIndex == -1) ? null : this.buffers[this.currentBufferIndex];
                if ((currentBuffer != null) && (currentBuffer.FreeBytes < 0x400))
                {
                    currentBuffer = null;
                }
                if (currentBuffer == null)
                {
                    currentBuffer = this.AddNewBuffer();
                }
                yield return Task.Factory.FromAsync((Func<AsyncCallback, object, IAsyncResult>) ((asyncCallback, asyncState) => this.inputStream.BeginRead(currentBuffer.Buffer, currentBuffer.OffsetToWriteTo, currentBuffer.FreeBytes, asyncCallback, asyncState)), delegate (IAsyncResult asyncResult) {
                    try
                    {
                        int count = this.inputStream.EndRead(asyncResult);
                        if (count == 0)
                        {
                            this.inputStream = null;
                        }
                        else
                        {
                            currentBuffer.MarkBytesAsWritten(count);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!ExceptionUtils.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        this.inputStream = null;
                        throw;
                    }
                }, null);
            }
        }

        internal static Task<BufferedReadStream> BufferStreamAsync(Stream inputStream)
        {
            BufferedReadStream bufferedReadStream = new BufferedReadStream(inputStream);
            return Task.Factory.Iterate(bufferedReadStream.BufferInputStream()).FollowAlwaysWith(delegate (Task task) {
                inputStream.Dispose();
            }).FollowOnSuccessWith<BufferedReadStream>(delegate (Task task) {
                bufferedReadStream.ResetForReading();
                return bufferedReadStream;
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ExceptionUtils.CheckArgumentNotNull<byte[]>(buffer, "buffer");
            if (this.currentBufferIndex == -1)
            {
                return 0;
            }
            DataBuffer buffer2 = this.buffers[this.currentBufferIndex];
            while (this.currentBufferReadCount >= buffer2.StoredCount)
            {
                this.currentBufferIndex++;
                if (this.currentBufferIndex >= this.buffers.Count)
                {
                    this.currentBufferIndex = -1;
                    return 0;
                }
                buffer2 = this.buffers[this.currentBufferIndex];
                this.currentBufferReadCount = 0;
            }
            int length = count;
            if (count > (buffer2.StoredCount - this.currentBufferReadCount))
            {
                length = buffer2.StoredCount - this.currentBufferReadCount;
            }
            Array.Copy(buffer2.Buffer, this.currentBufferReadCount, buffer, offset, length);
            this.currentBufferReadCount += length;
            return length;
        }

        internal void ResetForReading()
        {
            this.currentBufferIndex = (this.buffers.Count == 0) ? -1 : 0;
            this.currentBufferReadCount = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
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

        

        private sealed class DataBuffer
        {
            private readonly byte[] buffer = new byte[0x10000];
            private const int BufferSize = 0x10000;
            internal const int MinReadBufferSize = 0x400;

            public DataBuffer()
            {
                this.StoredCount = 0;
            }

            public void MarkBytesAsWritten(int count)
            {
                this.StoredCount += count;
            }

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public int FreeBytes
            {
                get
                {
                    return (this.buffer.Length - this.StoredCount);
                }
            }

            public int OffsetToWriteTo
            {
                get
                {
                    return this.StoredCount;
                }
            }

            public int StoredCount { get; private set; }
        }
    }
}

