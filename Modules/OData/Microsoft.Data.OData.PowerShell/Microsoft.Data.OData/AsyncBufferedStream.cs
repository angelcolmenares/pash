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

    internal sealed class AsyncBufferedStream : Stream
    {
        private Queue<DataBuffer> bufferQueue;
        private DataBuffer bufferToAppendTo;
        private readonly Stream innerStream;

        internal AsyncBufferedStream(Stream stream)
        {
            this.innerStream = stream;
            this.bufferQueue = new Queue<DataBuffer>();
        }

        internal void Clear()
        {
            this.bufferQueue.Clear();
            this.bufferToAppendTo = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.bufferQueue.Count > 0))
            {
                throw new ODataException(Strings.AsyncBufferedStream_WriterDisposedWithoutFlush);
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }

        internal Task FlushAsync()
        {
            return this.FlushAsyncInternal();
        }

        internal Task FlushAsyncInternal()
        {
            Queue<DataBuffer> buffers = this.PrepareFlushBuffers();
            if (buffers == null)
            {
                return TaskUtils.CompletedTask;
            }
            return Task.Factory.Iterate(this.FlushBuffersAsync(buffers));
        }

        private IEnumerable<Task> FlushBuffersAsync(Queue<DataBuffer> buffers)
        {
            while (true)
            {
                if (buffers.Count <= 0)
                {
                    yield break;
                }
                DataBuffer iteratorVariable0 = buffers.Dequeue();
                yield return iteratorVariable0.WriteToStreamAsync(this.innerStream);
            }
        }

        internal void FlushSync()
        {
            Queue<DataBuffer> queue = this.PrepareFlushBuffers();
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    queue.Dequeue().WriteToStream(this.innerStream);
                }
            }
        }

        private Queue<DataBuffer> PrepareFlushBuffers()
        {
            if (this.bufferQueue.Count == 0)
            {
                return null;
            }
            this.bufferToAppendTo = null;
            Queue<DataBuffer> bufferQueue = this.bufferQueue;
            this.bufferQueue = new Queue<DataBuffer>();
            return bufferQueue;
        }

        private void QueueNewBuffer()
        {
            this.bufferToAppendTo = new DataBuffer();
            this.bufferQueue.Enqueue(this.bufferToAppendTo);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            if (count > 0)
            {
                if (this.bufferToAppendTo == null)
                {
                    this.QueueNewBuffer();
                }
                while (count > 0)
                {
                    int num = this.bufferToAppendTo.Write(buffer, offset, count);
                    if (num < count)
                    {
                        this.QueueNewBuffer();
                    }
                    count -= num;
                    offset += num;
                }
            }
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
            private readonly byte[] buffer = new byte[0x13c00];
            private const int BufferSize = 0x13c00;
            private int storedCount = 0;

            public int Write(byte[] data, int index, int count)
            {
                int length = count;
                if (length > (this.buffer.Length - this.storedCount))
                {
                    length = this.buffer.Length - this.storedCount;
                }
                if (length > 0)
                {
                    Array.Copy(data, index, this.buffer, this.storedCount, length);
                    this.storedCount += length;
                }
                return length;
            }

            public void WriteToStream(Stream stream)
            {
                stream.Write(this.buffer, 0, this.storedCount);
            }

            public Task WriteToStreamAsync(Stream stream)
            {
                return Task.Factory.FromAsync((Func<AsyncCallback, object, IAsyncResult>) ((callback, state) => stream.BeginWrite(this.buffer, 0, this.storedCount, callback, state)), new Action<IAsyncResult>(stream.EndWrite), null);
            }
        }
    }
}

