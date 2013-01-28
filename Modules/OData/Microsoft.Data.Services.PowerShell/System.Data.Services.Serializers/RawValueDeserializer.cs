using System.Collections.Generic;

namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using System;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class RawValueDeserializer : ODataMessageReaderDeserializer
    {
        internal RawValueDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription, true)
        {
        }

        protected override ContentFormat GetContentFormat()
        {
            return ContentFormat.Text;
        }

        protected override object Read(System.Data.Services.SegmentInfo segmentInfo)
        {
            ResourceType resourceType;
            IEdmTypeReference typeReference;
            ResourceProperty projectedProperty = segmentInfo.ProjectedProperty;
            if (projectedProperty == null)
            {
                resourceType = null;
                typeReference = null;
            }
            else
            {
                resourceType = projectedProperty.ResourceType;
                typeReference = base.GetTypeReference(resourceType, projectedProperty.CustomAnnotations.ToList<KeyValuePair<string, object>>());
            }
            object obj3 = ODataMessageReaderDeserializer.ConvertPrimitiveValue(base.MessageReader.ReadValue(typeReference), ref resourceType);
            if (projectedProperty == null)
            {
                segmentInfo.TargetResourceType = resourceType;
            }
            return obj3;
        }
    }
}

