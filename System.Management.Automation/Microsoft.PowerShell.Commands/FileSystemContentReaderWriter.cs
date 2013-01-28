namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Provider;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal class FileSystemContentReaderWriter : IContentReader, IContentWriter, IDisposable
    {
        private bool _alreadyDetectEncoding;
        private FileStreamBackReader _backReader;
        private FileAccess access;
        private string delimiter;
        private Encoding encoding;
        private long fileOffset;
        private bool haveOldAttributes;
        private bool isRawStream;
        private FileMode mode;
        private System.IO.FileAttributes oldAttributes;
        private string path;
        private CmdletProvider provider;
        private StreamReader reader;
        private FileShare share;
        private FileStream stream;
        private string streamName;
        [TraceSource("FileSystemContentStream", "The provider content reader and writer for the file system")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("FileSystemContentStream", "The provider content reader and writer for the file system");
        private bool usingByteEncoding;
        private bool usingDelimiter;
        private bool waitForChanges;
        private StreamWriter writer;

        public FileSystemContentReaderWriter(string path, FileMode mode, FileAccess access, FileShare share, string delimiter, Encoding encoding, bool waitForChanges, CmdletProvider provider, bool isRawStream) : this(path, null, mode, access, share, encoding, false, waitForChanges, provider, isRawStream)
        {
        }

        public FileSystemContentReaderWriter(string path, FileMode mode, FileAccess access, FileShare share, Encoding encoding, bool usingByteEncoding, bool waitForChanges, CmdletProvider provider, bool isRawStream) : this(path, null, mode, access, share, encoding, usingByteEncoding, waitForChanges, provider, isRawStream)
        {
        }

        public FileSystemContentReaderWriter(string path, string streamName, FileMode mode, FileAccess access, FileShare share, string delimiter, Encoding encoding, bool waitForChanges, CmdletProvider provider, bool isRawStream) : this(path, streamName, mode, access, share, encoding, false, waitForChanges, provider, isRawStream)
        {
            this.delimiter = delimiter;
            this.usingDelimiter = true;
        }

        public FileSystemContentReaderWriter(string path, string streamName, FileMode mode, FileAccess access, FileShare share, Encoding encoding, bool usingByteEncoding, bool waitForChanges, CmdletProvider provider, bool isRawStream)
        {
            this.delimiter = "\n";
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            tracer.WriteLine("path = {0}", new object[] { path });
            tracer.WriteLine("mode = {0}", new object[] { mode });
            tracer.WriteLine("access = {0}", new object[] { access });
            this.path = path;
            this.streamName = streamName;
            this.mode = mode;
            this.access = access;
            this.share = share;
            this.encoding = encoding;
            this.usingByteEncoding = usingByteEncoding;
            this.waitForChanges = waitForChanges;
            this.provider = provider;
            this.isRawStream = isRawStream;
            this.CreateStreams(path, streamName, mode, access, share, encoding);
        }

        public void Close()
        {
            bool flag = false;
            if (this.writer != null)
            {
                try
                {
                    this.writer.Flush();
                    this.writer.Close();
                }
                finally
                {
                    flag = true;
                }
            }
            if (this.reader != null)
            {
                this.reader.Close();
                flag = true;
            }
            if (this._backReader != null)
            {
                this._backReader.Close();
                flag = true;
            }
            if (!flag)
            {
                this.stream.Flush();
                this.stream.Close();
            }
            if (this.haveOldAttributes && (this.provider.Force != 0))
            {
                File.SetAttributes(this.path, this.oldAttributes);
            }
        }

        private void CreateStreams(string filePath, string streamName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding fileEncoding)
        {
            if (File.Exists(filePath) && (this.provider.Force != 0))
            {
                this.oldAttributes = File.GetAttributes(filePath);
                this.haveOldAttributes = true;
                System.IO.FileAttributes hidden = System.IO.FileAttributes.Hidden;
                if ((fileAccess & FileAccess.Write) != 0)
                {
                    hidden |= System.IO.FileAttributes.ReadOnly;
                }
                File.SetAttributes(this.path, File.GetAttributes(filePath) & ~hidden);
            }
            FileAccess access = fileAccess;
            if ((fileAccess & FileAccess.Write) != 0)
            {
                fileAccess = FileAccess.ReadWrite;
            }
            try
            {
                if (!string.IsNullOrEmpty(streamName))
                {
                    this.stream = AlternateDataStreamUtilities.CreateFileStream(filePath, streamName, fileMode, fileAccess, fileShare);
                }
                else
                {
                    this.stream = new FileStream(filePath, fileMode, fileAccess, fileShare);
                }
            }
            catch (IOException)
            {
                if (!string.IsNullOrEmpty(streamName))
                {
                    this.stream = AlternateDataStreamUtilities.CreateFileStream(filePath, streamName, fileMode, access, fileShare);
                }
                else
                {
                    this.stream = new FileStream(filePath, fileMode, access, fileShare);
                }
            }
            if (!this.usingByteEncoding)
            {
                if ((fileAccess & FileAccess.Read) != 0)
                {
                    this.reader = new StreamReader(this.stream, fileEncoding);
                    this._backReader = new FileStreamBackReader(this.stream, fileEncoding);
                }
                if ((fileAccess & FileAccess.Write) != 0)
                {
                    if ((this.reader != null) && ((fileAccess & FileAccess.Read) != 0))
                    {
                        this.reader.Peek();
                        fileEncoding = this.reader.CurrentEncoding;
                    }
                    this.writer = new StreamWriter(this.stream, fileEncoding);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.stream != null)
                {
                    this.stream.Close();
                }
                if (this.reader != null)
                {
                    this.reader.Close();
                }
                if (this._backReader != null)
                {
                    this._backReader.Close();
                }
                if (this.writer != null)
                {
                    this.writer.Close();
                }
            }
        }

        public IList Read(long readCount)
        {
            if (this.isRawStream && this.waitForChanges)
            {
                throw PSTraceSource.NewInvalidOperationException("FileSystemProviderStrings", "RawAndWaitCannotCoexist", new object[0]);
            }
            bool waitForChanges = this.waitForChanges;
            tracer.WriteLine("blocks requested = {0}", new object[] { readCount });
            ArrayList blocks = new ArrayList();
            bool flag2 = readCount <= 0L;
            if (this._alreadyDetectEncoding && (this.reader.BaseStream.Position == 0L))
            {
                Encoding currentEncoding = this.reader.CurrentEncoding;
                this.stream.Close();
                this.CreateStreams(this.path, null, this.mode, this.access, this.share, currentEncoding);
                this._alreadyDetectEncoding = false;
            }
            try
            {
                for (long i = 0L; (i < readCount) || flag2; i += 1L)
                {
                    if (waitForChanges && this.provider.Stopping)
                    {
                        waitForChanges = false;
                    }
                    if (this.usingByteEncoding)
                    {
                        if (this.ReadByteEncoded(waitForChanges, blocks, false))
                        {
                            continue;
                        }
                        break;
                    }
                    if (this.usingDelimiter || this.isRawStream)
                    {
                        if (this.ReadDelimited(waitForChanges, blocks, false, this.delimiter))
                        {
                            continue;
                        }
                        break;
                    }
                    if (!this.ReadByLine(waitForChanges, blocks, false))
                    {
                        break;
                    }
                }
                tracer.WriteLine("blocks read = {0}", new object[] { blocks.Count });
            }
            catch (Exception exception)
            {
                if ((!(exception is IOException) && !(exception is ArgumentException)) && ((!(exception is SecurityException) && !(exception is UnauthorizedAccessException)) && !(exception is ArgumentNullException)))
                {
                    throw;
                }
                this.provider.WriteError(new ErrorRecord(exception, "GetContentReaderIOError", ErrorCategory.ReadError, this.path));
                return null;
            }
            return blocks.ToArray();
        }

        private bool ReadByLine(bool waitChanges, ArrayList blocks, bool readBackward)
        {
            string str = readBackward ? this._backReader.ReadLine() : this.reader.ReadLine();
            if ((str == null) && waitChanges)
            {
                do
                {
                    this.WaitForChanges(this.path, this.mode, this.access, this.share, this.reader.CurrentEncoding);
                    str = this.reader.ReadLine();
                }
                while ((str == null) && !this.provider.Stopping);
            }
            if (str != null)
            {
                blocks.Add(str);
            }
            int num = readBackward ? this._backReader.Peek() : this.reader.Peek();
            if (num == -1)
            {
                return false;
            }
            return true;
        }

        private bool ReadByteEncoded(bool waitChanges, ArrayList blocks, bool readBack)
        {
            if (!this.isRawStream)
            {
                if (readBack)
                {
                    if (this.stream.Position == 0L)
                    {
                        return false;
                    }
                    this.stream.Position -= 1L;
                    blocks.Add((byte) this.stream.ReadByte());
                    this.stream.Position -= 1L;
                    return true;
                }
                int num4 = this.stream.ReadByte();
                if ((num4 == -1) && waitChanges)
                {
                    this.WaitForChanges(this.path, this.mode, this.access, this.share, Encoding.Default);
                    num4 = this.stream.ReadByte();
                }
                if (num4 != -1)
                {
                    blocks.Add((byte) num4);
                    return true;
                }
                return false;
            }
            byte[] buffer = new byte[this.stream.Length];
            int length = (int) this.stream.Length;
            int offset = 0;
            while (length > 0)
            {
                int num3 = this.stream.Read(buffer, offset, length);
                if (num3 == 0)
                {
                    break;
                }
                offset += num3;
                length -= num3;
            }
            if (offset == 0)
            {
                return false;
            }
            blocks.Add(buffer);
            return true;
        }

        private bool ReadDelimited(bool waitChanges, ArrayList blocks, bool readBackward, string actualDelimiter)
        {
            int charCount = 0;
            char[] buffer = new char[actualDelimiter.Length];
            int length = actualDelimiter.Length;
            StringBuilder builder = new StringBuilder();
            Dictionary<char, int> dictionary = new Dictionary<char, int>();
            foreach (char ch in actualDelimiter)
            {
                dictionary[ch] = (actualDelimiter.Length - actualDelimiter.LastIndexOf(ch)) - 1;
            }
            do
            {
                if (this.isRawStream)
                {
                    string str = this.reader.ReadToEnd();
                    charCount = str.Length;
                    builder.Append(str);
                }
                else
                {
                    buffer = new char[length];
                    charCount = readBackward ? this._backReader.Read(buffer, 0, length) : this.reader.Read(buffer, 0, length);
                    if ((charCount == 0) && waitChanges)
                    {
                        while ((charCount < length) && !this.provider.Stopping)
                        {
                            this.WaitForChanges(this.path, this.mode, this.access, this.share, this.reader.CurrentEncoding);
                            charCount += this.reader.Read(buffer, 0, length - charCount);
                        }
                    }
                    if (charCount > 0)
                    {
                        builder.Append(buffer, 0, charCount);
                        if (dictionary.ContainsKey(builder[builder.Length - 1]))
                        {
                            length = dictionary[builder[builder.Length - 1]];
                        }
                        else
                        {
                            length = actualDelimiter.Length;
                        }
                        if (length == 0)
                        {
                            length = 1;
                        }
                    }
                }
            }
            while ((this.isRawStream && (charCount != 0)) || ((builder.ToString().IndexOf(actualDelimiter, StringComparison.Ordinal) < 0) && (charCount != 0)));
            if (builder.Length > 0)
            {
                blocks.Add(builder.ToString());
            }
            int num3 = readBackward ? this._backReader.Peek() : this.reader.Peek();
            return ((num3 != -1) || (readBackward && (builder.Length > 0)));
        }

        internal IList ReadWithoutWaitingChanges(long readCount)
        {
            IList list;
            bool waitForChanges = this.waitForChanges;
            this.waitForChanges = false;
            try
            {
                list = this.Read(readCount);
            }
            finally
            {
                this.waitForChanges = waitForChanges;
            }
            return list;
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            if (this.writer != null)
            {
                this.writer.Flush();
            }
            this.stream.Seek(offset, origin);
            if (this.writer != null)
            {
                this.writer.Flush();
            }
            if (this.reader != null)
            {
                this.reader.DiscardBufferedData();
            }
            if (this._backReader != null)
            {
                this._backReader.DiscardBufferedData();
            }
        }

        internal void SeekItemsBackward(int backCount)
        {
            if (backCount < 0)
            {
                throw PSTraceSource.NewArgumentException("backCount");
            }
            if (this.isRawStream && this.waitForChanges)
            {
                throw PSTraceSource.NewInvalidOperationException("FileSystemProviderStrings", "RawAndWaitCannotCoexist", new object[0]);
            }
            tracer.WriteLine("blocks seek backwards = {0}", new object[] { backCount });
            ArrayList blocks = new ArrayList();
            if (this.reader != null)
            {
                this.Seek(0L, SeekOrigin.Begin);
                this.reader.Peek();
                this._alreadyDetectEncoding = true;
            }
            this.Seek(0L, SeekOrigin.End);
            if (backCount != 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (char ch in this.delimiter)
                {
                    builder.Insert(0, ch);
                }
                string actualDelimiter = builder.ToString();
                long num = 0L;
                string str2 = null;
                try
                {
                    if (!this.isRawStream)
                    {
                        goto Label_0151;
                    }
                    this.Seek(0L, SeekOrigin.Begin);
                    return;
                Label_00EA:
                    if (this.usingByteEncoding)
                    {
                        if (this.ReadByteEncoded(false, blocks, true))
                        {
                            goto Label_0144;
                        }
                        goto Label_0157;
                    }
                    if (this.usingDelimiter)
                    {
                        if (!this.ReadDelimited(false, blocks, true, actualDelimiter))
                        {
                            goto Label_0157;
                        }
                        str2 = (string) blocks[0];
                        if ((num == 0L) && str2.Equals(actualDelimiter, StringComparison.Ordinal))
                        {
                            backCount++;
                        }
                    }
                    else if (!this.ReadByLine(false, blocks, true))
                    {
                        goto Label_0157;
                    }
                Label_0144:
                    blocks.Clear();
                    num += 1L;
                Label_0151:
                    if (num < backCount)
                    {
                        goto Label_00EA;
                    }
                Label_0157:
                    if (!this.usingByteEncoding)
                    {
                        long currentPosition = this._backReader.GetCurrentPosition();
                        if ((this.usingDelimiter && (num == backCount)) && str2.EndsWith(actualDelimiter, StringComparison.Ordinal))
                        {
                            currentPosition += this._backReader.GetByteCount(this.delimiter);
                        }
                        this.Seek(currentPosition, SeekOrigin.Begin);
                    }
                    tracer.WriteLine("blocks seek position = {0}", new object[] { this.stream.Position });
                }
                catch (Exception exception)
                {
                    if ((!(exception is IOException) && !(exception is ArgumentException)) && ((!(exception is SecurityException) && !(exception is UnauthorizedAccessException)) && !(exception is ArgumentNullException)))
                    {
                        throw;
                    }
                    this.provider.WriteError(new ErrorRecord(exception, "GetContentReaderIOError", ErrorCategory.ReadError, this.path));
                }
            }
        }

        private void WaitForChanges(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding fileEncoding)
        {
            if (this.stream != null)
            {
                this.fileOffset = this.stream.Position;
                this.stream.Close();
            }
            FileInfo info = new FileInfo(filePath);
            using (FileSystemWatcher watcher = new FileSystemWatcher(info.DirectoryName, info.Name))
            {
                watcher.EnableRaisingEvents = true;
                while (!this.provider.Stopping)
                {
                    if (!watcher.WaitForChanged(WatcherChangeTypes.All, 500).TimedOut)
                    {
                        goto Label_0076;
                    }
                }
            }
        Label_0076:
            Thread.Sleep(100);
            this.CreateStreams(filePath, null, fileMode, fileAccess, fileShare, fileEncoding);
            if (this.fileOffset > this.stream.Length)
            {
                this.fileOffset = 0L;
            }
            this.stream.Seek(this.fileOffset, SeekOrigin.Begin);
            if (this.reader != null)
            {
                this.reader.DiscardBufferedData();
            }
            if (this._backReader != null)
            {
                this._backReader.DiscardBufferedData();
            }
        }

        public IList Write(IList content)
        {
            foreach (object obj2 in content)
            {
                object[] objArray = obj2 as object[];
                if (objArray != null)
                {
                    foreach (object obj3 in objArray)
                    {
                        this.WriteObject(obj3);
                    }
                }
                else
                {
                    this.WriteObject(obj2);
                }
            }
            return content;
        }

        private void WriteObject(object content)
        {
            if (content != null)
            {
                if (this.usingByteEncoding)
                {
                    try
                    {
                        byte num = (byte) content;
                        this.stream.WriteByte(num);
                        return;
                    }
                    catch (InvalidCastException)
                    {
                        throw PSTraceSource.NewArgumentException("content", "FileSystemProviderStrings", "ByteEncodingError", new object[0]);
                    }
                }
                this.writer.WriteLine(content.ToString());
            }
        }
    }
}

