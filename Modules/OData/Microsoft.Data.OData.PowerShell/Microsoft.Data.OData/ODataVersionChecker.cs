namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal static class ODataVersionChecker
    {
        internal static void CheckAssociationLinks(ODataVersion version)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_AssociationLinksNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckCollectionValue(ODataVersion version)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_CollectionNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckCollectionValueProperties(ODataVersion version, string propertyName)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_CollectionPropertiesNotSupported(propertyName, ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckCount(ODataVersion version)
        {
            if (version < ODataVersion.V2)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_InlineCountNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckCustomDataNamespace(ODataVersion version)
        {
            if (version > ODataVersion.V2)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX("DataNamespace", ODataUtils.ODataVersionToString(ODataVersion.V2)));
            }
        }

        internal static void CheckCustomTypeScheme(ODataVersion version)
        {
            if (version > ODataVersion.V2)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX("TypeScheme", ODataUtils.ODataVersionToString(ODataVersion.V2)));
            }
        }

        internal static void CheckEntityPropertyMapping(ODataVersion version, IEdmEntityType entityType, IEdmModel model)
        {
            ODataEntityPropertyMappingCache epmCache = model.GetEpmCache(entityType);
            if ((epmCache != null) && (version < epmCache.EpmTargetTree.MinimumODataProtocolVersion))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_EpmVersionNotSupported(entityType.ODataFullName(), ODataUtils.ODataVersionToString(epmCache.EpmTargetTree.MinimumODataProtocolVersion), ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckNextLink(ODataVersion version)
        {
            if (version < ODataVersion.V2)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_NextLinkNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckParameterPayload(ODataVersion version)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_ParameterPayloadNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckSpatialValue(ODataVersion version)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_GeographyAndGeometryNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckStreamReferenceProperty(ODataVersion version)
        {
            if (version < ODataVersion.V3)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_StreamPropertiesNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        internal static void CheckVersionSupported(ODataVersion version, ODataMessageReaderSettings messageReaderSettings)
        {
            if (version > messageReaderSettings.MaxProtocolVersion)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataVersionChecker_MaxProtocolVersionExceeded(ODataUtils.ODataVersionToString(version), ODataUtils.ODataVersionToString(messageReaderSettings.MaxProtocolVersion)));
            }
        }
    }
}

