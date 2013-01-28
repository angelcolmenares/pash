namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal abstract class ODataFormat
    {
        private static ODataAtomFormat atomFormat = new ODataAtomFormat();
        private static ODataBatchFormat batchFormat = new ODataBatchFormat();
        private static ODataMetadataFormat metadataFormat = new ODataMetadataFormat();
        private static ODataRawValueFormat rawValueFormat = new ODataRawValueFormat();
        private static ODataVerboseJsonFormat verboseJsonFormat = new ODataVerboseJsonFormat();

        protected ODataFormat()
        {
        }

        internal abstract IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataRequestMessage requestMessage, ODataPayloadKindDetectionInfo detectionInfo);
        internal abstract IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataResponseMessage responseMessage, ODataPayloadKindDetectionInfo detectionInfo);
        internal abstract Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataRequestMessageAsync requestMessage, ODataPayloadKindDetectionInfo detectionInfo);
        internal abstract Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataResponseMessageAsync responseMessage, ODataPayloadKindDetectionInfo detectionInfo);

        public static ODataFormat Atom
        {
            get
            {
                return atomFormat;
            }
        }

        public static ODataFormat Batch
        {
            get
            {
                return batchFormat;
            }
        }

        public static ODataFormat Metadata
        {
            get
            {
                return metadataFormat;
            }
        }

        public static ODataFormat RawValue
        {
            get
            {
                return rawValueFormat;
            }
        }

        public static ODataFormat VerboseJson
        {
            get
            {
                return verboseJsonFormat;
            }
        }
    }
}

