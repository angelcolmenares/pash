namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services;

    internal sealed class EntityReferenceLinkDeserializer : ODataMessageReaderDeserializer
    {
        internal EntityReferenceLinkDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription, true)
        {
        }

        protected override ContentFormat GetContentFormat()
        {
            ODataFormat readFormat = ODataUtils.GetReadFormat(base.MessageReader);
            if (readFormat == ODataFormat.Atom)
            {
                return ContentFormat.PlainXml;
            }
            if (readFormat != ODataFormat.VerboseJson)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceException_GeneralError);
            }
            return ContentFormat.VerboseJson;
        }

        protected override object Read(System.Data.Services.SegmentInfo segmentInfo)
        {
            Uri url = base.MessageReader.ReadEntityReferenceLink().Url;
            if (string.IsNullOrEmpty(CommonUtil.UriToString(url)))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_MissingUriForLinkOperation);
            }
			return RequestUriProcessor.GetAbsoluteUriFromReference(url, base.Service.OperationContext.AbsoluteServiceUri, base.RequestDescription.RequestVersion);
        }
    }
}

