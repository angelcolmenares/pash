namespace System.Data.Services.Serializers
{
    using System;
    using System.Data.Services;

    internal sealed class MediaResourceDeserializer : Deserializer
    {
        internal MediaResourceDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription)
        {
        }

        protected override object Deserialize(System.Data.Services.SegmentInfo segmentInfo)
        {
            return base.Service.OperationContext.Host.RequestStream;
        }

        protected override System.Data.Services.ContentFormat ContentFormat
        {
            get
            {
                return System.Data.Services.ContentFormat.Binary;
            }
        }
    }
}

