namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class ODataPayloadKindDetectionInfo
    {
        private readonly MediaType contentType;
        private readonly ODataMessageReaderSettings messageReaderSettings;
        private readonly IEdmModel model;
        private readonly IEnumerable<ODataPayloadKind> possiblePayloadKinds;

        internal ODataPayloadKindDetectionInfo(MediaType contentType, ODataMessageReaderSettings messageReaderSettings, IEdmModel model, IEnumerable<ODataPayloadKind> possiblePayloadKinds)
        {
            ExceptionUtils.CheckArgumentNotNull<MediaType>(contentType, "contentType");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "readerSettings");
            ExceptionUtils.CheckArgumentNotNull<IEnumerable<ODataPayloadKind>>(possiblePayloadKinds, "possiblePayloadKinds");
            this.contentType = contentType;
            this.messageReaderSettings = messageReaderSettings;
            this.model = model;
            this.possiblePayloadKinds = possiblePayloadKinds;
        }

        public Encoding GetEncoding()
        {
            return this.contentType.SelectEncoding();
        }

        internal MediaType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public ODataMessageReaderSettings MessageReaderSettings
        {
            get
            {
                return this.messageReaderSettings;
            }
        }

        public IEdmModel Model
        {
            get
            {
                return this.model;
            }
        }

        public IEnumerable<ODataPayloadKind> PossiblePayloadKinds
        {
            get
            {
                return this.possiblePayloadKinds;
            }
        }
    }
}

