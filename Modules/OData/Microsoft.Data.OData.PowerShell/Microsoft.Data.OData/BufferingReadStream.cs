namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class BufferingReadStream : Stream
    {
        private bool bufferingModeDisabled;
        private readonly LinkedList<byte[]> buffers;
        private LinkedListNode<byte[]> currentReadNode;
        private Stream innerStream;
        private int positionInCurrentBuffer;

        internal BufferingReadStream(Stream stream)
        {
            this.innerStream = stream;
            this.buffers = new LinkedList<byte[]>();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.bufferingModeDisabled)
            {
                if (disposing && (this.innerStream != null))
                {
                    this.innerStream.Dispose();
                    this.innerStream = null;
                }
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        private void MoveToNextBuffer()
        {
            if (this.bufferingModeDisabled)
            {
                this.buffers.RemoveFirst();
                this.currentReadNode = this.buffers.First;
            }
            else
            {
                this.currentReadNode = this.currentReadNode.Next;
            }
            this.positionInCurrentBuffer = 0;
        }

        public override int Read(byte[] userBuffer, int offset, int count)
        {
            ExceptionUtils.CheckArgumentNotNull<byte[]>(userBuffer, "userBuffer");
            ExceptionUtils.CheckIntegerNotNegative(offset, "offset");
            ExceptionUtils.CheckIntegerPositive(count, "count");
            int num = 0;
            while ((this.currentReadNode != null) && (count > 0))
            {
                byte[] src = this.currentReadNode.Value;
                int num2 = src.Length - this.positionInCurrentBuffer;
                if (num2 == count)
                {
                    Buffer.BlockCopy(src, this.positionInCurrentBuffer, userBuffer, offset, count);
                    num += count;
                    this.MoveToNextBuffer();
                    return num;
                }
                if (num2 > count)
                {
                    Buffer.BlockCopy(src, this.positionInCurrentBuffer, userBuffer, offset, count);
                    num += count;
                    this.positionInCurrentBuffer += count;
                    return num;
                }
                Buffer.BlockCopy(src, this.positionInCurrentBuffer, userBuffer, offset, num2);
                num += num2;
                offset += num2;
                count -= num2;
                this.MoveToNextBuffer();
            }
            int num3 = this.innerStream.Read(userBuffer, offset, count);
            if (!this.bufferingModeDisabled && (num3 > 0))
            {
                byte[] dst = new byte[num3];
                Buffer.BlockCopy(userBuffer, offset, dst, 0, num3);
                this.buffers.AddLast(dst);
            }
            return (num + num3);
        }

        internal void ResetStream()
        {
            this.currentReadNode = this.buffers.First;
            this.positionInCurrentBuffer = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        internal void StopBuffering()
        {
            this.ResetStream();
            this.bufferingModeDisabled = true;
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

        internal bool IsBuffering
        {
            get
            {
                return !this.bufferingModeDisabled;
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
    }
}

