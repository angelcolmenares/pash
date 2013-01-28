namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.Edm.Validation;
    using Microsoft.Data.OData.Atom;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal sealed class ODataMetadataOutputContext : ODataOutputContext
    {
        private Stream messageOutputStream;
        private XmlWriter xmlWriter;

        private ODataMetadataOutputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
        {
            try
            {
                this.messageOutputStream = messageStream;
                this.xmlWriter = ODataAtomWriterUtils.CreateXmlWriter(messageStream, messageWriterSettings, encoding);
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

        internal static ODataOutputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataMetadataOutputContext(format, message.GetStream(), encoding, messageWriterSettings, writingResponse, true, model, urlResolver);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (this.xmlWriter != null)
                {
                    this.xmlWriter.Flush();
                    this.messageOutputStream.Dispose();
                }
            }
            finally
            {
                this.messageOutputStream = null;
                this.xmlWriter = null;
            }
        }

        internal void Flush()
        {
            this.xmlWriter.Flush();
        }

        internal override void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            ODataAtomWriterUtils.WriteError(this.xmlWriter, error, includeDebugInformation, base.MessageWriterSettings.MessageQuotas.MaxNestingDepth);
            this.Flush();
        }

        internal override void WriteMetadataDocument()
        {
            IEnumerable<EdmError> enumerable;
            base.Model.SaveODataAnnotations();
            if (!EdmxWriter.TryWriteEdmx(base.Model, this.xmlWriter, EdmxTarget.OData, out enumerable))
            {
                StringBuilder builder = new StringBuilder();
                foreach (EdmError error in enumerable)
                {
                    builder.AppendLine(error.ToString());
                }
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMetadataOutputContext_ErrorWritingMetadata(builder.ToString()));
            }
            this.Flush();
        }
    }
}

