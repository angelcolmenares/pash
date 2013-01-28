namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    internal sealed class ODataAtomFormat : ODataFormat
    {
        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataRequestMessage requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessage>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            Stream messageStream = ((ODataMessage) requestMessage).GetStream();
            return this.DetectPayloadKindImplementation(messageStream, false, true, detectionInfo);
        }

        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataResponseMessage responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessage>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            Stream messageStream = ((ODataMessage) responseMessage).GetStream();
            return this.DetectPayloadKindImplementation(messageStream, true, true, detectionInfo);
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataRequestMessageAsync requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessageAsync>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return ((ODataMessage) requestMessage).GetStreamAsync().FollowOnSuccessWith<Stream, IEnumerable<ODataPayloadKind>>(streamTask => this.DetectPayloadKindImplementation(streamTask.Result, false, false, detectionInfo));
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataResponseMessageAsync responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessageAsync>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return ((ODataMessage) responseMessage).GetStreamAsync().FollowOnSuccessWith<Stream, IEnumerable<ODataPayloadKind>>(streamTask => this.DetectPayloadKindImplementation(streamTask.Result, true, false, detectionInfo));
        }

        private IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(Stream messageStream, bool readingResponse, bool synchronous, ODataPayloadKindDetectionInfo detectionInfo)
        {
            using (ODataAtomInputContext context = new ODataAtomInputContext(this, messageStream, detectionInfo.GetEncoding(), detectionInfo.MessageReaderSettings, ODataVersion.V3, readingResponse, synchronous, detectionInfo.Model, null))
            {
                return context.DetectPayloadKind(detectionInfo);
            }
        }

        public override string ToString()
        {
            return "Atom";
        }
    }
}

