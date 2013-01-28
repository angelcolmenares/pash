namespace System.Data.Services.Serializers
{
    using System;
    using System.Data.Linq;
    using System.Data.Services;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BinarySerializer : IExceptionWriter
    {
        private readonly Stream outputStream;
        internal BinarySerializer(Stream output)
        {
            this.outputStream = output;
        }

        public void WriteException(HandleExceptionArgs args)
        {
            using (XmlWriter writer = XmlWriter.Create(this.outputStream))
            {
                ErrorHandler.SerializeXmlError(args, writer);
            }
        }

        internal void WriteRequest(object content)
        {
            byte[] buffer = content as byte[];
            if (buffer == null)
            {
                buffer = ((Binary) content).ToArray();
            }
            this.outputStream.Write(buffer, 0, buffer.Length);
        }

        internal void WriteRequest(Stream inputStream, int bufferSize)
        {
            WebUtil.CopyStream(inputStream, this.outputStream, bufferSize);
        }
    }
}

