namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.Xml;

    internal static class ODataJsonReaderUtils
    {
        private static object ConvertInt32Value(int intValue, Type targetType, IEdmPrimitiveTypeReference primitiveTypeReference, bool usesV1ProviderBehavior)
        {
            if (targetType == typeof(short))
            {
                return Convert.ToInt16(intValue);
            }
            if (targetType == typeof(byte))
            {
                return Convert.ToByte(intValue);
            }
            if (targetType == typeof(sbyte))
            {
                return Convert.ToSByte(intValue);
            }
            if (targetType == typeof(float))
            {
                return Convert.ToSingle(intValue);
            }
            if (targetType == typeof(double))
            {
                return Convert.ToDouble(intValue);
            }
            if ((targetType == typeof(decimal)) || (targetType == typeof(long)))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertInt64OrDecimal);
            }
            if (!IsV1PrimitiveType(targetType) || ((targetType != typeof(int)) && !usesV1ProviderBehavior))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertInt32(primitiveTypeReference.ODataFullName()));
            }
            return intValue;
        }

        private static object ConvertStringValue(string stringValue, Type targetType, ODataVersion version)
        {
            if (targetType == typeof(byte[]))
            {
                return Convert.FromBase64String(stringValue);
            }
            if (targetType == typeof(Guid))
            {
                return new Guid(stringValue);
            }
            if (targetType == typeof(TimeSpan))
            {
                return XmlConvert.ToTimeSpan(stringValue);
            }
            if (targetType == typeof(DateTimeOffset))
            {
                return XmlConvert.ToDateTimeOffset(stringValue);
            }
            if ((targetType == typeof(DateTime)) && (version >= ODataVersion.V3))
            {
                try
                {
                    return Microsoft.Data.Edm.PlatformHelper.ConvertStringToDateTime(stringValue);
                }
                catch (FormatException)
                {
                }
            }
            return Convert.ChangeType(stringValue, targetType, CultureInfo.InvariantCulture);
        }

        internal static object ConvertValue(object value, IEdmPrimitiveTypeReference primitiveTypeReference, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool validateNullValue)
        {
            if (value == null)
            {
                ReaderValidationUtils.ValidateNullValue(EdmCoreModel.Instance, primitiveTypeReference, messageReaderSettings, validateNullValue, version);
                return null;
            }
            try
            {
                Type primitiveClrType = EdmLibraryExtensions.GetPrimitiveClrType(primitiveTypeReference.PrimitiveDefinition(), false);
                ODataReaderBehavior readerBehavior = messageReaderSettings.ReaderBehavior;
                string stringValue = value as string;
                if (stringValue != null)
                {
                    return ConvertStringValue(stringValue, primitiveClrType, version);
                }
                if (value is int)
                {
                    return ConvertInt32Value((int) value, primitiveClrType, primitiveTypeReference, (readerBehavior != null) && readerBehavior.UseV1ProviderBehavior);
                }
                if (value is double)
                {
                    double num = (double) value;
                    if (primitiveClrType == typeof(float))
                    {
                        return Convert.ToSingle(num);
                    }
                    if (!IsV1PrimitiveType(primitiveClrType) || ((primitiveClrType != typeof(double)) && ((readerBehavior == null) || !readerBehavior.UseV1ProviderBehavior)))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertDouble(primitiveTypeReference.ODataFullName()));
                    }
                    return value;
                }
                if (value is bool)
                {
                    if ((primitiveClrType != typeof(bool)) && ((readerBehavior == null) || (readerBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesServer)))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertBoolean(primitiveTypeReference.ODataFullName()));
                    }
                    return value;
                }
                if (value is DateTime)
                {
                    if ((primitiveClrType != typeof(DateTime)) && ((readerBehavior == null) || (readerBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesServer)))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertDateTime(primitiveTypeReference.ODataFullName()));
                    }
                    return value;
                }
                if ((value is DateTimeOffset) && (primitiveClrType != typeof(DateTimeOffset)))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_CannotConvertDateTimeOffset(primitiveTypeReference.ODataFullName()));
                }
            }
            catch (Exception exception)
            {
                if (!ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ReaderValidationUtils.GetPrimitiveTypeConversionException(primitiveTypeReference, exception);
            }
            return value;
        }

        internal static FeedPropertyKind DetermineFeedPropertyKind(string propertyName)
        {
            if (string.CompareOrdinal("__count", propertyName) == 0)
            {
                return FeedPropertyKind.Count;
            }
            if (string.CompareOrdinal("__next", propertyName) == 0)
            {
                return FeedPropertyKind.NextPageLink;
            }
            if (string.CompareOrdinal("results", propertyName) == 0)
            {
                return FeedPropertyKind.Results;
            }
            return FeedPropertyKind.Unsupported;
        }

        internal static void EnsureInstance<T>(ref T instance) where T: class, new()
        {
            if (((T) instance) == null)
            {
                instance = Activator.CreateInstance<T>();
            }
        }

        internal static bool ErrorPropertyNotFound(ref ErrorPropertyBitMask propertiesFoundBitField, ErrorPropertyBitMask propertyFoundBitMask)
        {
            if ((propertiesFoundBitField & propertyFoundBitMask) == propertyFoundBitMask)
            {
                return false;
            }
            propertiesFoundBitField |= propertyFoundBitMask;
            return true;
        }

        internal static string GetPayloadTypeName(object payloadItem)
        {
            if (payloadItem == null)
            {
                return null;
            }
            TypeCode typeCode = Microsoft.Data.OData.PlatformHelper.GetTypeCode(payloadItem.GetType());
            if (typeCode != TypeCode.Boolean)
            {
                switch (typeCode)
                {
                    case TypeCode.Double:
                        return "Edm.Double";

                    case TypeCode.DateTime:
                        return "Edm.DateTime";

                    case TypeCode.String:
                        return "Edm.String";

                    case TypeCode.Int32:
                        return "Edm.Int32";
                }
            }
            else
            {
                return "Edm.Boolean";
            }
            ODataComplexValue value2 = payloadItem as ODataComplexValue;
            if (value2 != null)
            {
                return value2.TypeName;
            }
            ODataCollectionValue value3 = payloadItem as ODataCollectionValue;
            if (value3 != null)
            {
                return value3.TypeName;
            }
            ODataEntry entry = payloadItem as ODataEntry;
            if (entry == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataJsonReader_ReadEntryStart));
            }
            return entry.TypeName;
        }

        private static bool IsV1PrimitiveType(Type targetType)
        {
            return (!(targetType == typeof(DateTimeOffset)) && !(targetType == typeof(TimeSpan)));
        }

        internal static void ValidateCountPropertyInEntityReferenceLinks(long? propertyValue)
        {
            if (!propertyValue.HasValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_EntityReferenceLinksInlineCountWithNullValue("__count"));
            }
        }

        internal static void ValidateEntityReferenceLinksStringProperty(string propertyValue, string propertyName)
        {
            if (propertyValue == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_EntityReferenceLinksPropertyWithNullValue(propertyName));
            }
        }

        internal static void ValidateFeedProperty(object propertyValue, string propertyName)
        {
            if (propertyValue == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_FeedPropertyWithNullValue(propertyName));
            }
        }

        internal static void ValidateMediaResourceStringProperty(string propertyValue, string propertyName)
        {
            if (propertyValue == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_MediaResourcePropertyWithNullValue(propertyName));
            }
        }

        internal static void ValidateMetadataStringProperty(string propertyValue, string propertyName)
        {
            if (propertyValue == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_MetadataPropertyWithNullValue(propertyName));
            }
        }

        internal static void ValidateOperationJsonProperty(object propertyValue, string propertyName, string metadata, string operationsHeader)
        {
            if (propertyValue == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_OperationPropertyCannotBeNull(propertyName, metadata, operationsHeader));
            }
        }

        internal static void VerifyEntityReferenceLinksWrapperPropertyNotFound(ref EntityReferenceLinksWrapperPropertyBitMask propertiesFoundBitField, EntityReferenceLinksWrapperPropertyBitMask propertyFoundBitMask, string propertyName)
        {
            if ((propertiesFoundBitField & propertyFoundBitMask) == propertyFoundBitMask)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_MultipleEntityReferenceLinksWrapperPropertiesWithSameName(propertyName));
            }
            propertiesFoundBitField |= propertyFoundBitMask;
        }

        internal static void VerifyErrorPropertyNotFound(ref ErrorPropertyBitMask propertiesFoundBitField, ErrorPropertyBitMask propertyFoundBitMask, string propertyName)
        {
            if (!ErrorPropertyNotFound(ref propertiesFoundBitField, propertyFoundBitMask))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_MultipleErrorPropertiesWithSameName(propertyName));
            }
        }

        internal static void VerifyMetadataPropertyNotFound(ref MetadataPropertyBitMask propertiesFoundBitField, MetadataPropertyBitMask propertyFoundBitMask, string propertyName)
        {
            if ((propertiesFoundBitField & propertyFoundBitMask) != MetadataPropertyBitMask.None)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReaderUtils_MultipleMetadataPropertiesWithSameName(propertyName));
            }
            propertiesFoundBitField |= propertyFoundBitMask;
        }

        [Flags]
        internal enum EntityReferenceLinksWrapperPropertyBitMask
        {
            Count = 1,
            NextPageLink = 4,
            None = 0,
            Results = 2
        }

        [Flags]
        internal enum ErrorPropertyBitMask
        {
            Code = 2,
            Error = 1,
            InnerError = 0x20,
            Message = 4,
            MessageLanguage = 8,
            MessageValue = 0x10,
            None = 0,
            StackTrace = 0x80,
            TypeName = 0x40
        }

        internal enum FeedPropertyKind
        {
            Unsupported,
            Count,
            Results,
            NextPageLink
        }

        [Flags]
        internal enum MetadataPropertyBitMask
        {
            Actions = 0x200,
            ContentType = 0x20,
            EditMedia = 0x10,
            ETag = 4,
            Functions = 0x400,
            Id = 0x100,
            MediaETag = 0x40,
            MediaUri = 8,
            None = 0,
            Properties = 0x80,
            Type = 2,
            Uri = 1
        }
    }
}

