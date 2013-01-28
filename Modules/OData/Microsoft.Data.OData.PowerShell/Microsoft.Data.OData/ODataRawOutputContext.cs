namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataRawOutputContext : ODataOutputContext
    {
        private AsyncBufferedStream asynchronousOutputStream;
        private Encoding encoding;
        private Stream messageOutputStream;
        private IODataOutputInStreamErrorListener outputInStreamErrorListener;
        private Stream outputStream;
        private System.IO.TextWriter textWriter;

        private ODataRawOutputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
        {
            try
            {
                this.messageOutputStream = messageStream;
                this.encoding = encoding;
                if (synchronous)
                {
                    this.outputStream = messageStream;
                }
                else
                {
                    this.asynchronousOutputStream = new AsyncBufferedStream(messageStream);
                    this.outputStream = this.asynchronousOutputStream;
                }
            }
            catch
            {
                messageStream.Dispose();
                throw;
            }
        }

        internal void CloseTextWriter()
        {
            this.textWriter.Dispose();
            this.textWriter = null;
        }

        internal static ODataOutputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataRawOutputContext(format, message.GetStream(), encoding, messageWriterSettings, writingResponse, true, model, urlResolver);
        }

        internal static Task<ODataOutputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<Stream, ODataOutputContext>(streamTask => new ODataRawOutputContext(format, streamTask.Result, encoding, messageWriterSettings, writingResponse, false, model, urlResolver));
        }

        internal override ODataBatchWriter CreateODataBatchWriter(string batchBoundary)
        {
            return this.CreateODataBatchWriterImplementation(batchBoundary);
        }

        internal override Task<ODataBatchWriter> CreateODataBatchWriterAsync(string batchBoundary)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchWriter>(() => this.CreateODataBatchWriterImplementation(batchBoundary));
        }

        private ODataBatchWriter CreateODataBatchWriterImplementation(string batchBoundary)
        {
            this.encoding = this.encoding ?? MediaTypeUtils.EncodingUtf8NoPreamble;
            ODataBatchWriter writer = new ODataBatchWriter(this, batchBoundary);
            this.outputInStreamErrorListener = writer;
            return writer;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (this.messageOutputStream != null)
                {
                    if (this.textWriter != null)
                    {
                        this.textWriter.Flush();
                    }
                    if (this.asynchronousOutputStream != null)
                    {
                        this.asynchronousOutputStream.FlushSync();
                        this.asynchronousOutputStream.Dispose();
                    }
                    this.messageOutputStream.Dispose();
                }
            }
            finally
            {
                this.messageOutputStream = null;
                this.asynchronousOutputStream = null;
                this.outputStream = null;
                this.textWriter = null;
            }
        }

        internal void Flush()
        {
            if (this.textWriter != null)
            {
                this.textWriter.Flush();
            }
        }

        internal Task FlushAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                if (this.textWriter != null)
                {
                    this.textWriter.Flush();
                }
                return this.asynchronousOutputStream.FlushAsync();
            }).FollowOnSuccessWithTask(asyncBufferedStreamFlushTask => this.messageOutputStream.FlushAsync());
        }

        internal void FlushBuffers()
        {
            if (this.asynchronousOutputStream != null)
            {
                this.asynchronousOutputStream.FlushSync();
            }
        }

        internal Task FlushBuffersAsync()
        {
            if (this.asynchronousOutputStream != null)
            {
                return this.asynchronousOutputStream.FlushAsync();
            }
            return TaskUtils.CompletedTask;
        }

        internal void InitializeTextWriter()
        {
            Stream outputStream;
            if (MessageStreamWrapper.IsNonDisposingStream(this.outputStream) || (this.outputStream is AsyncBufferedStream))
            {
                outputStream = this.outputStream;
            }
            else
            {
                outputStream = MessageStreamWrapper.CreateNonDisposingStream(this.outputStream);
            }
            this.textWriter = new StreamWriter(outputStream, this.encoding);
        }

        internal void VerifyNotDisposed()
        {
            if (this.messageOutputStream == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        private void WriteBinaryValue(byte[] bytes)
        {
            this.OutputStream.Write(bytes, 0, bytes.Length);
        }

        internal override void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            if (this.outputInStreamErrorListener != null)
            {
                this.outputInStreamErrorListener.OnInStreamError();
            }
            throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotWriteInStreamErrorForRawValues);
        }

        internal override Task WriteInStreamErrorAsync(ODataError error, bool includeDebugInformation)
        {
            if (this.outputInStreamErrorListener != null)
            {
                this.outputInStreamErrorListener.OnInStreamError();
            }
            throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotWriteInStreamErrorForRawValues);
        }

        private void WriteRawValue(object value)
        {
            string str;
            if (!AtomValueUtils.TryConvertPrimitiveToString(value, out str))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_CannotConvertValueToRawPrimitive(value.GetType().FullName));
            }
            this.InitializeTextWriter();
            this.TextWriter.Write(str);
        }

        internal override void WriteValue(object value)
        {
            this.WriteValueImplementation(value);
            this.Flush();
        }

        internal override Task WriteValueAsync(object value)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteValueImplementation(value);
                return this.FlushAsync();
            });
        }

        private void WriteValueImplementation(object value)
        {
            byte[] bytes = value as byte[];
            if (bytes != null)
            {
                this.WriteBinaryValue(bytes);
            }
            else
            {
                this.WriteRawValue(value);
            }
        }

        internal Stream OutputStream
        {
            get
            {
                return this.outputStream;
            }
        }

        internal System.IO.TextWriter TextWriter
        {
            get
            {
                return this.textWriter;
            }
        }
    }
}

