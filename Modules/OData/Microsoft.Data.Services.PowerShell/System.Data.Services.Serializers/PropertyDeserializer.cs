using System.Collections.Generic;

namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class PropertyDeserializer : ODataMessageReaderDeserializer
    {
        internal PropertyDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription, true)
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
            ResourceProperty projectedProperty;
            ResourceType resourceType;
            IEdmTypeReference typeReference;
            if (segmentInfo.TargetKind == RequestTargetKind.OpenProperty)
            {
                projectedProperty = null;
                resourceType = null;
                typeReference = null;
            }
            else
            {
                projectedProperty = segmentInfo.ProjectedProperty;
                resourceType = projectedProperty.ResourceType;
                typeReference = base.GetTypeReference(resourceType, projectedProperty.CustomAnnotations.ToList<KeyValuePair<string, object>>());
                if ((projectedProperty.Kind == ResourcePropertyKind.Primitive) && MetadataProviderUtils.ShouldDisablePrimitivePropertyNullValidation(projectedProperty, (IEdmPrimitiveTypeReference) typeReference))
                {
                    typeReference = base.GetSchemaType(resourceType).ToTypeReference(true);
                }
                if (((projectedProperty.Kind == ResourcePropertyKind.ComplexType) && base.Service.Provider.IsV1Provider) && !typeReference.IsNullable)
                {
                    typeReference = base.GetSchemaType(resourceType).ToTypeReference(true);
                }
            }
            ODataProperty property2 = base.MessageReader.ReadProperty(typeReference);
            if ((((this.ContentFormat != ContentFormat.PlainXml) || ((segmentInfo.TargetKind != RequestTargetKind.OpenProperty) && (property2.Value != null))) && ((this.ContentFormat != ContentFormat.VerboseJson) || (property2.Name.Length != 0))) && (string.CompareOrdinal(segmentInfo.Identifier, property2.Name) != 0))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.PlainXml_IncorrectElementName(segmentInfo.Identifier, property2.Name));
            }
            object odataValue = property2.Value;
            if ((segmentInfo.TargetKind == RequestTargetKind.OpenProperty) && (odataValue is ODataCollectionValue))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_OpenCollectionProperty(property2.Name));
            }
            object obj3 = base.ConvertValue(odataValue, ref resourceType);
            if (segmentInfo.TargetKind == RequestTargetKind.OpenProperty)
            {
                segmentInfo.TargetResourceType = resourceType;
            }
            return obj3;
        }
    }
}

