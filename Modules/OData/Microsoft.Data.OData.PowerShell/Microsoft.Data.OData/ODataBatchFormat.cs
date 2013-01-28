namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class ODataBatchFormat : ODataFormat
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
            if (((HttpUtils.CompareMediaTypeNames("multipart", contentType.TypeName) && HttpUtils.CompareMediaTypeNames("mixed", contentType.SubTypeName)) && (contentType.Parameters != null)) && contentType.Parameters.Any<KeyValuePair<string, string>>(kvp => HttpUtils.CompareMediaTypeParameterNames("boundary", kvp.Key)))
            {
                return new ODataPayloadKind[] { ODataPayloadKind.Batch };
            }
            return Enumerable.Empty<ODataPayloadKind>();
        }

        public override string ToString()
        {
            return "Batch";
        }
    }
}

