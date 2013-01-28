namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    internal sealed class FileStreamBackReader : StreamReader
    {
        private readonly byte[] _byteBuff;
        private int _byteCount;
        private readonly char[] _charBuff;
        private int _charCount;
        private readonly Encoding _currentEncoding;
        private long _currentPosition;
        private readonly Encoding _defaultAnsiEncoding;
        private readonly Encoding _oemEncoding;
        private bool? _singleByteCharSet;
        private readonly FileStream _stream;
        private const byte BothTopBitsSet = 0xc0;
        private const int BuffSize = 0x1000;
        private const byte TopBitUnset = 0x80;

        internal FileStreamBackReader(FileStream fileStream, Encoding encoding) : base(fileStream, encoding)
        {
            this._byteBuff = new byte[0x1000];
            this._charBuff = new char[0x1000];
            this._singleByteCharSet = null;
            this._stream = fileStream;
            if (this._stream.Length > 0L)
            {
                long position = this._stream.Position;
                this._stream.Seek(0L, SeekOrigin.Begin);
                base.Peek();
                this._stream.Position = position;
                this._currentEncoding = base.CurrentEncoding;
                this._currentPosition = this._stream.Position;
                this._oemEncoding = EncodingConversion.Convert(null, "oem");
                this._defaultAnsiEncoding = EncodingConversion.Convert(null, "default");
            }
        }

        internal void DiscardBufferedData()
        {
            base.DiscardBufferedData();
            this._currentPosition = this._stream.Position;
            this._charCount = 0;
            this._byteCount = 0;
        }

        internal int GetByteCount(string delimiter)
        {
            char[] chars = delimiter.ToCharArray();
            return this._currentEncoding.GetByteCount(chars, 0, chars.Length);
        }

        internal long GetCurrentPosition()
        {
            if (this._charCount == 0)
            {
                return this._currentPosition;
            }
            int num = this._currentEncoding.GetByteCount(this._charBuff, 0, this._charCount);
            return (this._currentPosition + num);
        }

        private bool IsSingleByteCharacterSet()
        {
            Win32Native.CPINFO cpinfo;
            if (this._singleByteCharSet.HasValue)
            {
                return this._singleByteCharSet.Value;
            }
            if ((this._currentEncoding.Equals(this._oemEncoding) || this._currentEncoding.Equals(this._defaultAnsiEncoding)) && (Win32Native.GetCPInfo((int) this._currentEncoding.CodePage, out cpinfo) && (cpinfo.MaxCharSize == 1)))
            {
                this._singleByteCharSet = true;
                return true;
            }
            this._singleByteCharSet = false;
            return false;
        }

        public override int Peek()
        {
            if ((this._charCount == 0) && (this.RefillCharBuffer() == -1))
            {
                return -1;
            }
            return this._charBuff[this._charCount - 1];
        }

        public override int Read()
        {
            if ((this._charCount == 0) && (this.RefillCharBuffer() == -1))
            {
                return -1;
            }
            this._charCount--;
            return this._charBuff[this._charCount];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int num = 0;
            do
            {
                if ((this._charCount == 0) && (this.RefillCharBuffer() == -1))
                {
                    return num;
                }
                int num2 = (this._charCount > count) ? count : this._charCount;
                while (num2 > 0)
                {
                    buffer[index++] = this._charBuff[--this._charCount];
                    num2--;
                    count--;
                    num++;
                }
            }
            while (count > 0);
            return num;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        public override string ReadLine()
        {
            if ((this._charCount == 0) && (this.RefillCharBuffer() == -1))
            {
                return null;
            }
            int length = 0;
            StringBuilder builder = new StringBuilder();
            if ((this._charBuff[this._charCount - 1] == '\r') || (this._charBuff[this._charCount - 1] == '\n'))
            {
                length++;
                builder.Insert(0, this._charBuff[--this._charCount]);
                if (this._charBuff[this._charCount] == '\n')
                {
                    if ((this._charCount == 0) && (this.RefillCharBuffer() == -1))
                    {
                        return string.Empty;
                    }
                    if ((this._charCount > 0) && (this._charBuff[this._charCount - 1] == '\r'))
                    {
                        length++;
                        builder.Insert(0, this._charBuff[--this._charCount]);
                    }
                }
            }
            do
            {
                while (this._charCount > 0)
                {
                    if ((this._charBuff[this._charCount - 1] == '\r') || (this._charBuff[this._charCount - 1] == '\n'))
                    {
                        builder.Remove(builder.Length - length, length);
                        return builder.ToString();
                    }
                    builder.Insert(0, this._charBuff[--this._charCount]);
                }
            }
            while (this.RefillCharBuffer() != -1);
            builder.Remove(builder.Length - length, length);
            return builder.ToString();
        }

        public override string ReadToEnd()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        private int RefillByteBuff()
        {
            long position = this._stream.Position;
            if (position == 0L)
            {
                return -1;
            }
            int count = (position > 0x1000L) ? 0x1000 : ((int) position);
            this._stream.Seek((long) -count, SeekOrigin.Current);
            if (!this._currentEncoding.Equals(Encoding.UTF8))
            {
                if ((!this._currentEncoding.Equals(Encoding.Unicode) && !this._currentEncoding.Equals(Encoding.BigEndianUnicode)) && ((!this._currentEncoding.Equals(Encoding.UTF32) && !this._currentEncoding.Equals(Encoding.ASCII)) && !this.IsSingleByteCharacterSet()))
                {
                    throw new BackReaderEncodingNotSupportedException(StringUtil.Format(FileSystemProviderStrings.ReadBackward_Encoding_NotSupport, this._currentEncoding.EncodingName), this._currentEncoding.EncodingName);
                }
                this._currentPosition = this._stream.Position;
                this._byteCount = this._stream.Read(this._byteBuff, 0, count);
                this._stream.Position = this._currentPosition;
            }
            else
            {
                do
                {
                    this._currentPosition = this._stream.Position;
                    byte num3 = (byte) this._stream.ReadByte();
                    if (((num3 & 0xc0) == 0xc0) || ((num3 & 0x80) == 0))
                    {
                        this._byteBuff[0] = num3;
                        this._byteCount = 1;
                        break;
                    }
                }
                while (position > this._stream.Position);
                if (position == this._stream.Position)
                {
                    this._stream.Seek((long) -count, SeekOrigin.Current);
                    this._byteCount = 0;
                }
                this._byteCount += this._stream.Read(this._byteBuff, this._byteCount, (int) (position - this._stream.Position));
                this._stream.Position = this._currentPosition;
            }
            return this._byteCount;
        }

        private int RefillCharBuffer()
        {
            if (this.RefillByteBuff() == -1)
            {
                return -1;
            }
            this._charCount = this._currentEncoding.GetChars(this._byteBuff, 0, this._byteCount, this._charBuff, 0);
            return this._charCount;
        }
    }
}

