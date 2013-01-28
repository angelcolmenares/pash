namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataRawInputContext : ODataInputContext
    {
        private const int BufferSize = 0x1000;
        private readonly Encoding encoding;
        private readonly ODataPayloadKind readerPayloadKind;
        private System.IO.Stream stream;
        private TextReader textReader;

        private ODataRawInputContext(ODataFormat format, System.IO.Stream messageStream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver, ODataPayloadKind readerPayloadKind) : base(format, messageReaderSettings, version, readingResponse, synchronous, model, urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "messageReaderSettings");
            try
            {
                this.stream = messageStream;
                this.encoding = encoding;
                this.readerPayloadKind = readerPayloadKind;
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception) && (messageStream != null))
                {
                    messageStream.Dispose();
                }
                throw;
            }
        }

        internal static ODataInputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver, ODataPayloadKind readerPayloadKind)
        {
            return new ODataRawInputContext(format, message.GetStream(), encoding, messageReaderSettings, version, readingResponse, true, model, urlResolver, readerPayloadKind);
        }

        internal static Task<ODataInputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver, ODataPayloadKind readerPayloadKind)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<System.IO.Stream, ODataInputContext>(streamTask => new ODataRawInputContext(format, streamTask.Result, encoding, messageReaderSettings, version, readingResponse, false, model, urlResolver, readerPayloadKind));
        }

        internal override ODataBatchReader CreateBatchReader(string batchBoundary)
        {
            return this.CreateBatchReaderImplementation(batchBoundary, true);
        }

        internal override Task<ODataBatchReader> CreateBatchReaderAsync(string batchBoundary)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchReader>(() => this.CreateBatchReaderImplementation(batchBoundary, false));
        }

        private ODataBatchReader CreateBatchReaderImplementation(string batchBoundary, bool synchronous)
        {
            return new ODataBatchReader(this, batchBoundary, this.encoding, synchronous);
        }

        protected override void DisposeImplementation()
        {
            try
            {
                if (this.textReader != null)
                {
                    this.textReader.Dispose();
                }
                else if (this.stream != null)
                {
                    this.stream.Dispose();
                }
            }
            finally
            {
                this.textReader = null;
                this.stream = null;
            }
        }

        private byte[] ReadBinaryValue()
        {
            byte[] buffer;
            int num2;
            long num = 0L;
            List<byte[]> list = new List<byte[]>();
            do
            {
                buffer = new byte[0x1000];
                num2 = this.stream.Read(buffer, 0, buffer.Length);
                num += num2;
                list.Add(buffer);
            }
            while (num2 == buffer.Length);
            buffer = new byte[num];
            for (int i = 0; i < (list.Count - 1); i++)
            {
                Buffer.BlockCopy(list[i], 0, buffer, i * 0x1000, 0x1000);
            }
            Buffer.BlockCopy(list[list.Count - 1], 0, buffer, (list.Count - 1) * 0x1000, num2);
            return buffer;
        }

        private object ReadRawValue(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            string text = this.textReader.ReadToEnd();
            if ((expectedPrimitiveTypeReference != null) && !base.MessageReaderSettings.DisablePrimitiveTypeConversion)
            {
                return AtomValueUtils.ConvertStringToPrimitive(text, expectedPrimitiveTypeReference);
            }
            return text;
        }

        internal override object ReadValue(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            return this.ReadValueImplementation(expectedPrimitiveTypeReference);
        }

        internal override Task<object> ReadValueAsync(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            return TaskUtils.GetTaskForSynchronousOperation<object>(() => this.ReadValueImplementation(expectedPrimitiveTypeReference));
        }

        private object ReadValueImplementation(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            bool flag;
            if (expectedPrimitiveTypeReference == null)
            {
                flag = this.readerPayloadKind == ODataPayloadKind.BinaryValue;
            }
            else if (expectedPrimitiveTypeReference.PrimitiveKind() == EdmPrimitiveTypeKind.Binary)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                return this.ReadBinaryValue();
            }
            this.textReader = (this.encoding == null) ? new StreamReader(this.stream) : new StreamReader(this.stream, this.encoding);
            return this.ReadRawValue(expectedPrimitiveTypeReference);
        }

        internal System.IO.Stream Stream
        {
            get
            {
                return this.stream;
            }
        }
    }
}

