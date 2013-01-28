namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ODataUtilsInternal
    {
        internal static ODataVersion GetDataServiceVersion(ODataMessage message, ODataVersion defaultVersion)
        {
            string header = message.GetHeader("DataServiceVersion");
            if (!string.IsNullOrEmpty(header))
            {
                return ODataUtils.StringToODataVersion(header);
            }
            return defaultVersion;
        }

        internal static bool IsPayloadKindSupported(ODataPayloadKind payloadKind, bool inRequest)
        {
            switch (payloadKind)
            {
                case ODataPayloadKind.Feed:
                case ODataPayloadKind.EntityReferenceLinks:
                case ODataPayloadKind.Collection:
                case ODataPayloadKind.ServiceDocument:
                case ODataPayloadKind.MetadataDocument:
                case ODataPayloadKind.Error:
                    return !inRequest;

                case ODataPayloadKind.Entry:
                case ODataPayloadKind.Property:
                case ODataPayloadKind.EntityReferenceLink:
                case ODataPayloadKind.Value:
                case ODataPayloadKind.BinaryValue:
                case ODataPayloadKind.Batch:
                    return true;

                case ODataPayloadKind.Parameter:
                    return inRequest;
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataUtilsInternal_IsPayloadKindSupported_UnreachableCodePath));
        }

        internal static void SetDataServiceVersion(ODataMessage message, ODataMessageWriterSettings settings)
        {
            string headerValue = ODataUtils.ODataVersionToString(settings.Version.Value) + ";";
            message.SetHeader("DataServiceVersion", headerValue);
        }

        internal static Version ToDataServiceVersion(this ODataVersion version)
        {
            switch (version)
            {
                case ODataVersion.V1:
                    return new Version(1, 0);

                case ODataVersion.V2:
                    return new Version(2, 0);

                case ODataVersion.V3:
                    return new Version(3, 0);
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataUtilsInternal_ToDataServiceVersion_UnreachableCodePath));
        }
    }
}

