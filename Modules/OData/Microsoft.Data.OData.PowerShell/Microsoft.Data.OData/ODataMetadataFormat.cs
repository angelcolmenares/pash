namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Atom;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;

    internal sealed class ODataMetadataFormat : ODataFormat
    {
        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataRequestMessage requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessage>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return Enumerable.Empty<ODataPayloadKind>();
        }

        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(IODataResponseMessage responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessage>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return DetectPayloadKindImplementation(((ODataMessage) responseMessage).GetStream(), detectionInfo);
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataRequestMessageAsync requestMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessageAsync>(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return TaskUtils.GetCompletedTask<IEnumerable<ODataPayloadKind>>(Enumerable.Empty<ODataPayloadKind>());
        }

        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(IODataResponseMessageAsync responseMessage, ODataPayloadKindDetectionInfo detectionInfo)
        {
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessageAsync>(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull<ODataPayloadKindDetectionInfo>(detectionInfo, "detectionInfo");
            return ((ODataMessage) responseMessage).GetStreamAsync().FollowOnSuccessWith<Stream, IEnumerable<ODataPayloadKind>>(streamTask => DetectPayloadKindImplementation(streamTask.Result, detectionInfo));
        }

        private static IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(Stream messageStream, ODataPayloadKindDetectionInfo detectionInfo)
        {
            try
            {
                using (XmlReader reader = ODataAtomReaderUtils.CreateXmlReader(messageStream, detectionInfo.GetEncoding(), detectionInfo.MessageReaderSettings))
                {
                    string str;
                    if (((reader.TryReadToNextElement() && (string.CompareOrdinal("Edmx", reader.LocalName) == 0)) && ((str = reader.NamespaceURI) != null)) && (((str == "http://schemas.microsoft.com/ado/2007/06/edmx") || (str == "http://schemas.microsoft.com/ado/2008/10/edmx")) || (str == "http://schemas.microsoft.com/ado/2009/11/edmx")))
                    {
                        return new ODataPayloadKind[] { ODataPayloadKind.MetadataDocument };
                    }
                }
            }
            catch (XmlException)
            {
            }
            return Enumerable.Empty<ODataPayloadKind>();
        }

        public override string ToString()
        {
            return "Metadata";
        }
    }
}

