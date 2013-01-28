namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class ODataRawValueFormat : ODataFormat
    {
        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataRequestMessage requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessage>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return DetectPayloadKindImplementation(detectionInfo.ContentType);
        }

        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataResponseMessage responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessage>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return DetectPayloadKindImplementation(detectionInfo.ContentType);
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataRequestMessageAsync requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessageAsync>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return TaskUtils.GetTaskForSynchronousOperation<IEnumerable<ODataPayloadKind>>(() => DetectPayloadKindImplementation(detectionInfo.ContentType));
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataResponseMessageAsync responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessageAsync>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return TaskUtils.GetTaskForSynchronousOperation<IEnumerable<ODataPayloadKind>>(() => DetectPayloadKindImplementation(detectionInfo.ContentType));
        }

        private static IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(MediaType contentType)
        {
            if (HttpUtils.CompareMediaTypeNames("text", contentType.TypeName) && HttpUtils.CompareMediaTypeNames("text/plain", contentType.SubTypeName))
            {
                return new ODataPayloadKind[] { ODataPayloadKind.Value };
            }
            return new ODataPayloadKind[] { ODataPayloadKind.BinaryValue };
        }

        public override string ToString()
        {
            return "RawValue";
        }
    }
}

