namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal static class ODataUtils
    {
        private const string Version1NumberString = "1.0";
        private const string Version2NumberString = "2.0";
        private const string Version3NumberString = "3.0";

        public static string GetHttpMethod(this IEdmModel model, IEdmElement annotatable)
        {
            string str;
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmElement>(annotatable, "annotatable");
            if (!model.TryGetODataAnnotation(annotatable, "HttpMethod", out str))
            {
                return null;
            }
            if (str == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_NullValueForHttpMethodAnnotation);
            }
            return str;
        }

        public static string GetMimeType(this IEdmModel model, IEdmElement annotatable)
        {
            string str;
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmElement>(annotatable, "annotatable");
            if (!model.TryGetODataAnnotation(annotatable, "MimeType", out str))
            {
                return null;
            }
            if (str == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_NullValueForMimeTypeAnnotation);
            }
            return str;
        }

        public static ODataFormat GetReadFormat(ODataMessageReader messageReader)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReader>(messageReader, "messageReader");
            return messageReader.GetFormat();
        }

        public static bool HasDefaultStream(this IEdmModel model, IEdmEntityType entityType)
        {
            bool flag;
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityType>(entityType, "entityType");
            return (TryGetBooleanAnnotation(model, entityType, "HasStream", true, out flag) && flag);
        }

        public static bool IsAlwaysBindable(this IEdmModel model, IEdmFunctionImport functionImport)
        {
            bool flag;
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmFunctionImport>(functionImport, "functionImport");
            if (!TryGetBooleanAnnotation(model, functionImport, "IsAlwaysBindable", out flag))
            {
                return false;
            }
            if (!functionImport.IsBindable && flag)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_UnexpectedIsAlwaysBindableAnnotationInANonBindableFunctionImport);
            }
            return flag;
        }

        public static bool IsDefaultEntityContainer(this IEdmModel model, IEdmEntityContainer entityContainer)
        {
            bool flag;
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityContainer>(entityContainer, "entityContainer");
            return (TryGetBooleanAnnotation(model, entityContainer, "IsDefaultEntityContainer", out flag) && flag);
        }

        public static void LoadODataAnnotations(this IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            model.LoadODataAnnotations(100);
        }

        public static void LoadODataAnnotations(this IEdmModel model, IEdmEntityType entityType)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityType>(entityType, "entityType");
            model.LoadODataAnnotations(entityType, 100);
        }

        public static void LoadODataAnnotations(this IEdmModel model, int maxEntityPropertyMappingsPerType)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            foreach (IEdmEntityType type in model.EntityTypes())
            {
                model.LoadODataAnnotations(type, maxEntityPropertyMappingsPerType);
            }
        }

        public static void LoadODataAnnotations(this IEdmModel model, IEdmEntityType entityType, int maxEntityPropertyMappingsPerType)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityType>(entityType, "entityType");
            model.ClearInMemoryEpmAnnotations(entityType);
            model.EnsureEpmCache(entityType, maxEntityPropertyMappingsPerType);
        }

        public static ODataNullValueBehaviorKind NullValueReadBehaviorKind(this IEdmModel model, IEdmProperty property)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmProperty>(property, "property");
            ODataEdmPropertyAnnotation annotationValue = model.GetAnnotationValue<ODataEdmPropertyAnnotation>(property);
            if (annotationValue != null)
            {
                return annotationValue.NullValueReadBehaviorKind;
            }
            return ODataNullValueBehaviorKind.Default;
        }

        public static string ODataVersionToString(ODataVersion version)
        {
            switch (version)
            {
                case ODataVersion.V1:
                    return "1.0";

                case ODataVersion.V2:
                    return "2.0";

                case ODataVersion.V3:
                    return "3.0";
            }
            throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_UnsupportedVersionNumber);
        }

        public static void SaveODataAnnotations(this IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            if (!model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_CannotSaveAnnotationsToBuiltInModel);
            }
            foreach (IEdmEntityType type in model.EntityTypes())
            {
                SaveODataAnnotationsImplementation(model, type);
            }
        }

        public static void SaveODataAnnotations(this IEdmModel model, IEdmEntityType entityType)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityType>(entityType, "entityType");
            SaveODataAnnotationsImplementation(model, entityType);
        }

        private static void SaveODataAnnotationsImplementation(IEdmModel model, IEdmEntityType entityType)
        {
            ODataEntityPropertyMappingCache epmCache = model.EnsureEpmCache(entityType, 0x7fffffff);
            if (epmCache != null)
            {
                model.SaveEpmAnnotations(entityType, epmCache.MappingsForInheritedProperties, false, false);
                IEnumerable<IEdmProperty> declaredProperties = entityType.DeclaredProperties;
                if (declaredProperties != null)
                {
                    foreach (IEdmProperty property in declaredProperties)
                    {
                        if (property.Type.IsODataPrimitiveTypeKind() || property.Type.IsODataComplexTypeKind())
                        {
                            model.SaveEpmAnnotationsForProperty(property, epmCache);
                        }
                        else if (property.Type.IsNonEntityODataCollectionTypeKind())
                        {
                            model.SaveEpmAnnotationsForProperty(property, epmCache);
                        }
                    }
                }
            }
        }

        private static void SetBooleanAnnotation(IEdmModel model, IEdmElement annotatable, string annotationLocalName, bool boolValue)
        {
            model.SetODataAnnotation(annotatable, annotationLocalName, boolValue ? "true" : null);
        }

        public static void SetHasDefaultStream(this IEdmModel model, IEdmEntityType entityType, bool hasStream)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityType>(entityType, "entityType");
            SetBooleanAnnotation(model, entityType, "HasStream", hasStream);
        }

        public static ODataFormat SetHeadersForPayload(ODataMessageWriter messageWriter, ODataPayloadKind payloadKind)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataMessageWriter>(messageWriter, "messageWriter");
            if (payloadKind == ODataPayloadKind.Unsupported)
            {
                throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotSetHeadersWithInvalidPayloadKind(payloadKind), "payloadKind");
            }
            return messageWriter.SetHeaders(payloadKind);
        }

        public static void SetHttpMethod(this IEdmModel model, IEdmElement annotatable, string httpMethod)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmElement>(annotatable, "annotatable");
            model.SetODataAnnotation(annotatable, "HttpMethod", httpMethod);
        }

        public static void SetIsAlwaysBindable(this IEdmModel model, IEdmFunctionImport functionImport, bool isAlwaysBindable)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmFunctionImport>(functionImport, "functionImport");
            if (!functionImport.IsBindable && isAlwaysBindable)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_IsAlwaysBindableAnnotationSetForANonBindableFunctionImport);
            }
            SetBooleanAnnotation(model, functionImport, "IsAlwaysBindable", isAlwaysBindable);
        }

        public static void SetIsDefaultEntityContainer(this IEdmModel model, IEdmEntityContainer entityContainer, bool isDefaultContainer)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmEntityContainer>(entityContainer, "entityContainer");
            SetBooleanAnnotation(model, entityContainer, "IsDefaultEntityContainer", isDefaultContainer);
        }

        public static void SetMimeType(this IEdmModel model, IEdmElement annotatable, string mimeType)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmElement>(annotatable, "annotatable");
            model.SetODataAnnotation(annotatable, "MimeType", mimeType);
        }

        public static void SetNullValueReaderBehavior(this IEdmModel model, IEdmProperty property, ODataNullValueBehaviorKind nullValueReadBehaviorKind)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmProperty>(property, "property");
            ODataEdmPropertyAnnotation annotationValue = model.GetAnnotationValue<ODataEdmPropertyAnnotation>(property);
            if (annotationValue == null)
            {
                if (nullValueReadBehaviorKind != ODataNullValueBehaviorKind.Default)
                {
                    annotationValue = new ODataEdmPropertyAnnotation {
                        NullValueReadBehaviorKind = nullValueReadBehaviorKind
                    };
                    model.SetAnnotationValue<ODataEdmPropertyAnnotation>(property, annotationValue);
                }
            }
            else
            {
                annotationValue.NullValueReadBehaviorKind = nullValueReadBehaviorKind;
            }
        }

        public static ODataVersion StringToODataVersion(string version)
        {
            string str = version;
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(version, "version");
            int index = str.IndexOf(';');
            if (index >= 0)
            {
                str = str.Substring(0, index);
            }
            switch (str.Trim())
            {
                case "1.0":
                    return ODataVersion.V1;

                case "2.0":
                    return ODataVersion.V2;

                case "3.0":
                    return ODataVersion.V3;
            }
            throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_UnsupportedVersionHeader(version));
        }

        private static bool TryGetBooleanAnnotation(IEdmModel model, IEdmElement annotatable, string annotationLocalName, out bool boolValue)
        {
            string str;
            if (model.TryGetODataAnnotation(annotatable, annotationLocalName, out str))
            {
                boolValue = XmlConvert.ToBoolean(str);
                return true;
            }
            boolValue = false;
            return false;
        }

        private static bool TryGetBooleanAnnotation(IEdmModel model, IEdmStructuredType structuredType, string annotationLocalName, bool recursive, out bool boolValue)
        {
            string str = null;
            bool flag;
            do
            {
                flag = model.TryGetODataAnnotation(structuredType, annotationLocalName, out str);
                if (flag)
                {
                    break;
                }
                structuredType = structuredType.BaseType;
            }
            while (recursive && (structuredType != null));
            if (!flag)
            {
                boolValue = false;
                return false;
            }
            boolValue = XmlConvert.ToBoolean(str);
            return true;
        }
    }
}

