

namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
	using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Linq;

    internal abstract class ODataMaterializer : IDisposable
    {
        internal static readonly ODataNavigationLink[] EmptyLinks = new ODataNavigationLink[0];
        protected static readonly ODataProperty[] EmptyProperties = new ODataProperty[0];
        protected readonly Type ExpectedType;
        protected Dictionary<IEnumerable, DataServiceQueryContinuation> nextLinkTable;

        protected ODataMaterializer(System.Data.Services.Client.ResponseInfo responseInfo, Type expectedType)
        {
            this.ResponseInfo = responseInfo;
            this.ExpectedType = expectedType;
            this.nextLinkTable = new Dictionary<IEnumerable, DataServiceQueryContinuation>(System.Data.Services.Client.ReferenceEqualityComparer<IEnumerable>.Instance);
        }

        protected static void ApplyCollectionDataValues(ODataProperty collectionProperty, bool ignoreMissingProperties, System.Data.Services.Client.ResponseInfo responseInfo, object collectionInstance, Type collectionItemType, Action<object, object> AddValueToBackingICollectionInstance)
        {
            ODataCollectionValue value2 = collectionProperty.Value as ODataCollectionValue;
            if (value2.Items != null)
            {
                bool flag = PrimitiveType.IsKnownNullableType(collectionItemType);
                ClientEdmModel model = ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion);
                foreach (object obj2 in value2.Items)
                {
                    if (obj2 == null)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_NullCollectionItemsNotSupported);
                    }
                    if (flag)
                    {
                        object obj3;
                        if ((obj2 is ODataComplexValue) || (obj2 is ODataCollectionValue))
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_ComplexTypesInCollectionOfPrimitiveTypesNotAllowed);
                        }
                        MaterializePrimitiveDataValue(collectionItemType, value2.TypeName, obj2, responseInfo, () => System.Data.Services.Client.Strings.Collection_NullCollectionItemsNotSupported, out obj3);
                        AddValueToBackingICollectionInstance(collectionInstance, ConvertPrimitiveValue(obj2, collectionItemType));
                    }
                    else
                    {
                        ODataComplexValue value3 = obj2 as ODataComplexValue;
                        if (value3 == null)
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_PrimitiveTypesInCollectionOfComplexTypesNotAllowed);
                        }
                        ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(collectionItemType));
                        object instance = clientTypeAnnotation.CreateInstance();
                        ApplyDataValues(clientTypeAnnotation, value3.Properties, ignoreMissingProperties, responseInfo, instance);
                        AddValueToBackingICollectionInstance(collectionInstance, instance);
                    }
                }
            }
            collectionProperty.SetMaterializedValue(collectionInstance);
        }

        protected static void ApplyDataValue(ClientTypeAnnotation type, ODataProperty property, bool ignoreMissingProperties, System.Data.Services.Client.ResponseInfo responseInfo, object instance)
        {
            ClientPropertyAnnotation annotation = type.GetProperty(property.Name, ignoreMissingProperties);
            if (annotation != null)
            {
                if (annotation.IsPrimitiveOrComplexCollection)
                {
                    if (property.Value == null)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_NullCollectionNotSupported(property.Name));
                    }
                    if (property.Value is string)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MixedTextWithComment);
                    }
                    if (property.Value is ODataComplexValue)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidCollectionItem(property.Name));
                    }
                    object obj2 = annotation.GetValue(instance);
                    if (obj2 == null)
                    {
                        obj2 = CreateCollectionInstance(property, annotation.PropertyType, responseInfo);
                        annotation.SetValue(instance, obj2, property.Name, false);
                    }
                    else
                    {
                        annotation.ClearBackingICollectionInstance(obj2);
                    }
                    ApplyCollectionDataValues(property, ignoreMissingProperties, responseInfo, obj2, annotation.PrimitiveOrComplexCollectionItemType, new Action<object, object>(annotation.AddValueToBackingICollectionInstance));
                }
                else
                {
                    object obj3 = property.Value;
                    ODataComplexValue value2 = obj3 as ODataComplexValue;
                    if ((obj3 != null) && (value2 != null))
                    {
                        if (!annotation.EdmProperty.Type.IsComplex())
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_ExpectingSimpleValue);
                        }
                        bool flag = false;
                        ClientEdmModel model = ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion);
                        ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(annotation.PropertyType));
                        object obj4 = annotation.GetValue(instance);
                        if (obj4 == null)
                        {
                            obj4 = clientTypeAnnotation.CreateInstance();
                            flag = true;
                        }
                        MaterializeDataValues(clientTypeAnnotation, value2.Properties, ignoreMissingProperties);
                        ApplyDataValues(clientTypeAnnotation, value2.Properties, ignoreMissingProperties, responseInfo, obj4);
                        if (flag)
                        {
                            annotation.SetValue(instance, obj4, property.Name, true);
                        }
                    }
                    else
                    {
                        MaterializePrimitiveDataValue(annotation.NullablePropertyType, property);
                        annotation.SetValue(instance, property.GetMaterializedValue(), property.Name, true);
                    }
                }
            }
        }

        protected static void ApplyDataValues(ClientTypeAnnotation type, IEnumerable<ODataProperty> properties, bool ignoreMissingProperties, System.Data.Services.Client.ResponseInfo responseInfo, object instance)
        {
            foreach (ODataProperty property in properties)
            {
                ApplyDataValue(type, property, ignoreMissingProperties, responseInfo, instance);
            }
        }

        internal abstract void ApplyLogToContext();
        internal abstract void ClearLog();
        protected static object ConvertPrimitiveValue(object value, Type propertyType)
        {
            if ((propertyType != null) && (value != null))
            {
                Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                TypeCode typeCode = System.Data.Services.Client.PlatformHelper.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.String:
                        return value;
                }
                if ((((typeCode == TypeCode.Char) || (typeCode == TypeCode.UInt16)) || ((typeCode == TypeCode.UInt32) || (typeCode == TypeCode.UInt64))) || (((type == typeof(char[])) || (type == typeof(Type))) || (((type == typeof(Uri)) || (type == typeof(XDocument))) || (type == typeof(XElement)))))
                {
                    PrimitiveType type2;
                    PrimitiveType.TryGetPrimitiveType(propertyType, out type2);
                    return type2.TypeConverter.Parse((string) value);
                }
                if (propertyType == BinaryTypeConverter.BinaryType)
                {
                    byte[] buffer = value as byte[];
                    value = Activator.CreateInstance(BinaryTypeConverter.BinaryType, new object[] { buffer });
                }
            }
            return value;
        }

        protected static object CreateCollectionInstance(ODataProperty collectionProperty, Type userCollectionType, System.Data.Services.Client.ResponseInfo responseInfo)
        {
            object obj2;
            ODataCollectionValue value2 = collectionProperty.Value as ODataCollectionValue;
            ClientTypeAnnotation annotation = responseInfo.TypeResolver.ResolveEdmTypeName(userCollectionType, value2.TypeName);
            if (IsDataServiceCollection(annotation.ElementType))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_DataServiceCollectionNotSupportedForNonEntities);
            }
            try
            {
                obj2 = annotation.CreateInstance();
            }
            catch (MissingMethodException exception)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_NoParameterlessCtorForCollectionProperty(collectionProperty.Name, annotation.ElementTypeName), exception);
            }
            return obj2;
        }

        public static ODataMaterializer CreateMaterializerForMessage(IODataResponseMessage responseMessage, System.Data.Services.Client.ResponseInfo responseInfo, Type materializerType, QueryComponents queryComponents, ProjectionPlan plan, ODataPayloadKind payloadKind)
        {
            ODataMaterializer materializer2;
            bool projectionQuery = (plan != null) || (queryComponents.Projection != null);
            ODataMessageReader messageReader = CreateODataMessageReader(responseMessage, responseInfo, projectionQuery, ref payloadKind);
            IEdmType expectedType = null;
            try
            {
                ODataMaterializer materializer;
                if (materializerType != typeof(object))
                {
                    expectedType = ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion).GetOrCreateEdmType(materializerType);
                }
                if ((payloadKind == ODataPayloadKind.Entry) || (payloadKind == ODataPayloadKind.Feed))
                {
                    if ((expectedType != null) && (expectedType.TypeKind != EdmTypeKind.Entity))
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidNonEntityType(materializerType.FullName));
                    }
                    ODataReader reader = CreateODataReader(messageReader, payloadKind, expectedType, responseInfo.MaxProtocolVersion);
                    materializer = new ODataReaderEntityMaterializer(messageReader, reader, responseInfo, queryComponents, materializerType, plan);
                }
                else
                {
                    switch (payloadKind)
                    {
                        case ODataPayloadKind.Property:
                            if ((expectedType != null) && (expectedType.TypeKind == EdmTypeKind.Entity))
                            {
                                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidEntityType(materializerType.FullName));
                            }
                            break;

                        case ODataPayloadKind.EntityReferenceLink:
                        case ODataPayloadKind.EntityReferenceLinks:
                            materializer = new ODataLinksMaterializer(messageReader, responseInfo, materializerType, queryComponents.SingleResult);
                            goto Label_013A;

                        case ODataPayloadKind.Value:
                            materializer = new ODataValueMaterializer(messageReader, responseInfo, materializerType, queryComponents.SingleResult);
                            goto Label_013A;

                        case ODataPayloadKind.BinaryValue:
                        case ODataPayloadKind.Collection:
                        case ODataPayloadKind.ServiceDocument:
                        case ODataPayloadKind.MetadataDocument:
                            goto Label_0129;

                        case ODataPayloadKind.Error:
                        {
                            ODataError error = messageReader.ReadError();
                            throw new ODataErrorException(error.Message, error);
                        }
                        default:
                            goto Label_0129;
                    }
                    materializer = new ODataPropertyMaterializer(messageReader, responseInfo, materializerType, queryComponents.SingleResult);
                }
                goto Label_013A;
            Label_0129:
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidResponsePayload(responseInfo.DataNamespace));
            Label_013A:
                materializer2 = materializer;
            }
            catch (Exception exception)
            {
                if (CommonUtil.IsCatchableExceptionType(exception))
                {
                    messageReader.Dispose();
                }
                throw;
            }
            return materializer2;
        }

        protected static ODataMessageReader CreateODataMessageReader(IODataResponseMessage responseMessage, System.Data.Services.Client.ResponseInfo responseInfo, bool projectionQuery, ref ODataPayloadKind payloadKind)
        {
            Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizer = null;
            if (responseInfo.HasReadingEntityHandlers)
            {
                entryXmlCustomizer = new Func<ODataEntry, XmlReader, Uri, XmlReader>(ODataMaterializer.EntryXmlCustomizer);
            }
            ODataMessageReaderSettings settings = WebUtil.CreateODataMessageReaderSettings(responseInfo, entryXmlCustomizer, projectionQuery);
            ODataMessageReader reader = new ODataMessageReader(responseMessage, settings, ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion));
            if (payloadKind == ODataPayloadKind.Unsupported)
            {
                List<ODataPayloadKindDetectionResult> source = reader.DetectPayloadKind().ToList<ODataPayloadKindDetectionResult>();
                if (source.Count == 0)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidResponsePayload(responseInfo.DataNamespace));
                }
                ODataPayloadKindDetectionResult result = source.FirstOrDefault<ODataPayloadKindDetectionResult>(delegate (ODataPayloadKindDetectionResult k) {
                    if (k.PayloadKind != ODataPayloadKind.EntityReferenceLink)
                    {
                        return k.PayloadKind == ODataPayloadKind.EntityReferenceLinks;
                    }
                    return true;
                });
                if (result == null)
                {
                    result = source.First<ODataPayloadKindDetectionResult>();
                }
                if ((result.Format != ODataFormat.Atom) && (result.Format != ODataFormat.RawValue))
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidContentTypeEncountered(responseMessage.GetHeader("Content-Type")));
                }
                payloadKind = result.PayloadKind;
            }
            return reader;
        }

        protected static ODataReader CreateODataReader(ODataMessageReader messageReader, ODataPayloadKind messageType, IEdmType expectedType, DataServiceProtocolVersion protocolVersion)
        {
            if (expectedType != null)
            {
                ClientEdmModel.GetModel(protocolVersion).GetClientTypeAnnotation(expectedType).EnsureEPMLoaded();
            }
            IEdmEntityType expectedEntityType = expectedType as IEdmEntityType;
            if (messageType == ODataPayloadKind.Entry)
            {
                return messageReader.CreateODataEntryReader(expectedEntityType);
            }
            return messageReader.CreateODataFeedReader(expectedEntityType);
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        private static XmlReader EntryXmlCustomizer(ODataEntry entry, XmlReader entryReader, Uri baseUri)
        {
            XElement payload = XElement.Load(entryReader.ReadSubtree(), LoadOptions.None);
            entryReader.Read();
            entry.SetAnnotation<ReadingEntityInfo>(new ReadingEntityInfo(payload, baseUri));
            XmlReader reader = payload.CreateReader();
            reader.Read();
            return reader;
        }

        protected static Action<object, object> GetAddToCollectionDelegate(Type listType)
        {
            Type type;
            ParameterExpression expression;
            ParameterExpression expression2;
            MethodInfo addToCollectionMethod = ClientTypeUtil.GetAddToCollectionMethod(listType, out type);
            return (Action<object, object>) Expression.Lambda(Expression.Call(Expression.Convert(expression = Expression.Parameter(typeof(object), "list"), listType), addToCollectionMethod, new Expression[] { Expression.Convert(expression2 = Expression.Parameter(typeof(object), "element"), type) }), new ParameterExpression[] { expression, expression2 }).Compile();
        }

        protected static bool IsDataServiceCollection(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericTypeEx() && WebUtil.IsDataServiceCollectionType(type.GetGenericTypeDefinition()))
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        protected static void MaterializeComplexTypeProperty(Type propertyType, ODataComplexValue complexValue, bool ignoreMissingProperties, System.Data.Services.Client.ResponseInfo responseInfo)
        {
            object instance = null;
            if ((complexValue != null) && !complexValue.HasMaterializedValue())
            {
                ClientTypeAnnotation actualType = null;
                if (WebUtil.IsWireTypeCollection(complexValue.TypeName))
                {
                    actualType = responseInfo.TypeResolver.ResolveEdmTypeName(propertyType, complexValue.TypeName);
                }
                else
                {
                    ClientEdmModel model = ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion);
                    actualType = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(propertyType));
                }
                instance = Util.ActivatorCreateInstance(propertyType, new object[0]);
                MaterializeDataValues(actualType, complexValue.Properties, ignoreMissingProperties);
                ApplyDataValues(actualType, complexValue.Properties, ignoreMissingProperties, responseInfo, instance);
                complexValue.SetMaterializedValue(instance);
            }
        }

        protected static void MaterializeDataValues(ClientTypeAnnotation actualType, IEnumerable<ODataProperty> values, bool ignoreMissingProperties)
        {
            foreach (ODataProperty property in values)
            {
                if (!(property.Value is ODataStreamReferenceValue))
                {
                    string name = property.Name;
                    ClientPropertyAnnotation annotation = actualType.GetProperty(name, ignoreMissingProperties);
                    if (annotation != null)
                    {
                        if (ClientTypeUtil.TypeOrElementTypeIsEntity(annotation.PropertyType))
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidEntityType(annotation.EntityCollectionItemType ?? annotation.PropertyType));
                        }
                        if (annotation.IsKnownType)
                        {
                            MaterializePrimitiveDataValue(annotation.NullablePropertyType, property);
                        }
                    }
                }
            }
        }

        protected static void MaterializePrimitiveDataValue(Type type, ODataProperty property)
        {
            if (!property.HasMaterializedValue())
            {
                object materializedValue = ConvertPrimitiveValue(property.Value, type);
                property.SetMaterializedValue(materializedValue);
            }
        }

        protected static bool MaterializePrimitiveDataValue(Type type, string wireTypeName, object value, System.Data.Services.Client.ResponseInfo responseInfo, Func<string> throwOnNullMessage, out object materializedValue)
        {
            PrimitiveType type3;
            Type clrType = Nullable.GetUnderlyingType(type) ?? type;
            bool flag = PrimitiveType.TryGetPrimitiveType(clrType, out type3);
            if (!flag)
            {
                flag = PrimitiveType.TryGetPrimitiveType(responseInfo.TypeResolver.ResolveEdmTypeName(type, wireTypeName).ElementType, out type3);
            }
            if (flag)
            {
                if (value == null)
                {
                    if (!ClientTypeUtil.CanAssignNull(type))
                    {
                        throw new InvalidOperationException(throwOnNullMessage());
                    }
                    materializedValue = null;
                }
                else
                {
                    materializedValue = ConvertPrimitiveValue(value, clrType);
                }
                return true;
            }
            materializedValue = null;
            return false;
        }

        protected abstract void OnDispose();
        public bool Read()
        {
            this.VerifyNotDisposed();
            return this.ReadImplementation();
        }

        protected abstract bool ReadImplementation();
        protected void VerifyNotDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(typeof(ODataEntityMaterializer).FullName);
            }
        }

        internal abstract long CountValue { get; }

        internal abstract ODataEntry CurrentEntry { get; }

        internal abstract ODataFeed CurrentFeed { get; }

        internal abstract object CurrentValue { get; }

        internal virtual bool IsCountable
        {
            get
            {
                return false;
            }
        }

        protected abstract bool IsDisposed { get; }

        internal abstract bool IsEndOfStream { get; }

        internal abstract ProjectionPlan MaterializeEntryPlan { get; }

        internal Dictionary<IEnumerable, DataServiceQueryContinuation> NextLinkTable
        {
            get
            {
                return this.nextLinkTable;
            }
        }

        internal System.Data.Services.Client.ResponseInfo ResponseInfo { get; set; }

        protected sealed class ReadingEntityInfo
        {
            internal readonly Uri BaseUri;
            internal readonly XElement EntryPayload;

            internal ReadingEntityInfo(XElement payload, Uri uri)
            {
                this.EntryPayload = payload;
                this.BaseUri = uri;
            }
        }
    }
}

