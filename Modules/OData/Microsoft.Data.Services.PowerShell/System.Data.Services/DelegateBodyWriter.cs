namespace System.Data.Services
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class DelegateBodyWriter : BodyWriter
    {
        private readonly IDataService service;
        private readonly Action<Stream> writerAction;

        internal DelegateBodyWriter(Action<Stream> writer, IDataService service) : base(false)
        {
            this.writerAction = writer;
            this.service = service;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WebUtil.CheckArgumentNull<XmlDictionaryWriter>(writer, "writer");
            try
            {
                writer.WriteStartElement("Binary");
                using (XmlWriterStream stream = new XmlWriterStream(writer))
                {
                    this.writerAction(stream);
                }
                writer.WriteEndElement();
            }
            finally
            {
                if (this.service != null)
                {
                    this.service.DisposeDataSource();
                    HttpContextServiceHost httpContextServiceHost = this.service.OperationContext.Host.HttpContextServiceHost;
                    if ((httpContextServiceHost != null) && httpContextServiceHost.ErrorFound)
                    {
                        OperationContext current = OperationContext.Current;
                        if (current != null)
                        {
                            current.Channel.Abort();
                        }
                    }
                }
            }
        }

        internal class XmlWriterStream : Stream
        {
            private readonly XmlDictionaryWriter innerWriter;

            internal XmlWriterStream(XmlDictionaryWriter xmlWriter)
            {
                this.innerWriter = xmlWriter;
            }

            public override void Flush()
            {
                this.innerWriter.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw Error.NotSupported();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw Error.NotSupported();
            }

            public override void SetLength(long value)
            {
                throw Error.NotSupported();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
				var xml = System.Text.Encoding.UTF8.GetString (buffer);
				System.Diagnostics.Debug.WriteLine ("\r\n\r\nXML: Writing output:\r\n\r\n" + xml + "\r\n\r\n");
				this.innerWriter.WriteBinHex (buffer, 0, count);
                //this.innerWriter.WriteBase64(buffer, offset, count);
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
                    throw Error.NotSupported();
                }
            }

            public override long Position
            {
                get
                {
                    throw Error.NotSupported();
                }
                set
                {
                    throw Error.NotSupported();
                }
            }
        }
    }
}

