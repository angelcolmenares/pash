namespace Microsoft.Data.OData
{
    using System;

    internal sealed class ODataPayloadKindDetectionResult
    {
        private readonly ODataFormat format;
        private readonly ODataPayloadKind payloadKind;

        internal ODataPayloadKindDetectionResult(ODataPayloadKind payloadKind, ODataFormat format)
        {
            this.payloadKind = payloadKind;
            this.format = format;
        }

        public ODataFormat Format
        {
            get
            {
                return this.format;
            }
        }

        public ODataPayloadKind PayloadKind
        {
            get
            {
                return this.payloadKind;
            }
        }
    }
}

