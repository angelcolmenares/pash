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

    internal sealed class ODataMetadataInputContext : ODataInputContext
    {
        private XmlReader baseXmlReader;
        private BufferingXmlReader xmlReader;

        private ODataMetadataInputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageReaderSettings, version, readingResponse, synchronous, model, urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "messageReaderSettings");
            try
            {
                this.baseXmlReader = ODataAtomReaderUtils.CreateXmlReader(messageStream, encoding, messageReaderSettings);
                this.xmlReader = new BufferingXmlReader(this.baseXmlReader, null, messageReaderSettings.BaseUri, false, messageReaderSettings.MessageQuotas.MaxNestingDepth, messageReaderSettings.ReaderBehavior.ODataNamespace);
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

        internal static ODataInputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataMetadataInputContext(format, message.GetStream(), encoding, messageReaderSettings, version, readingResponse, true, model, urlResolver);
        }

        protected override void DisposeImplementation()
        {
            try
            {
                if (this.baseXmlReader != null)
                {
                    this.baseXmlReader.Dispose();
                }
            }
            finally
            {
                this.baseXmlReader = null;
                this.xmlReader = null;
            }
        }

        internal override IEdmModel ReadMetadataDocument()
        {
            return this.ReadMetadataDocumentImplementation();
        }

        private IEdmModel ReadMetadataDocumentImplementation()
        {
            IEdmModel model;
            IEnumerable<EdmError> enumerable;
            if (!EdmxReader.TryParse(this.xmlReader, out model, out enumerable))
            {
                StringBuilder builder = new StringBuilder();
                foreach (EdmError error in enumerable)
                {
                    builder.AppendLine(error.ToString());
                }
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMetadataInputContext_ErrorReadingMetadata(builder.ToString()));
            }
            model.LoadODataAnnotations(base.MessageReaderSettings.MessageQuotas.MaxEntityPropertyMappingsPerType);
            return model;
        }
    }
}

