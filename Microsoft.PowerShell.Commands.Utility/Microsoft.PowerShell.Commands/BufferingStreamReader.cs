namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;

    internal class BufferingStreamReader : Stream
    {
        private Stream baseStream;
        private byte[] copyBuffer;
        private long length;
        private MemoryStream streamBuffer;

        internal BufferingStreamReader(Stream baseStream)
        {
            this.baseStream = baseStream;
            this.streamBuffer = new MemoryStream();
            this.length = 0x7fffffffffffffffL;
            this.copyBuffer = new byte[0x1000];
        }

        public override void Flush()
        {
            this.streamBuffer.SetLength(0L);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long position = this.Position;
            bool flag = false;
            int num2 = count;
            while (!flag && ((this.Position + num2) > this.streamBuffer.Length))
            {
                int num3 = Math.Min(num2, this.copyBuffer.Length);
                int num4 = this.baseStream.Read(this.copyBuffer, 0, num3);
                if (this.streamBuffer.Position < this.streamBuffer.Length)
                {
                    this.streamBuffer.Position = this.streamBuffer.Length;
                }
                this.streamBuffer.Write(this.copyBuffer, 0, num4);
                num2 -= num4;
                if (num4 < num3)
                {
                    flag = true;
                }
            }
            this.streamBuffer.Seek(position, SeekOrigin.Begin);
            int num5 = this.streamBuffer.Read(buffer, offset, count);
            if (num5 < count)
            {
                this.SetLength(this.Position);
            }
            return num5;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.streamBuffer.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.length = value;
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
                return true;
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
                return this.length;
            }
        }

        public override long Position
        {
            get
            {
                return this.streamBuffer.Position;
            }
            set
            {
                this.streamBuffer.Position = value;
            }
        }
    }
}

