namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Query;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Globalization;
	using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    internal class Serializer
    {
        private readonly RequestInfo requestInfo;

        internal Serializer(RequestInfo requestInfo)
        {
            this.requestInfo = requestInfo;
        }

        private string ConvertToEscapedUriValue(string paramName, object value)
        {
            object obj2 = null;
            string str;
            bool flag = false;
            if (value == null)
            {
                flag = true;
            }
            else if (value.GetType() == typeof(ODataUriNullValue))
            {
                obj2 = value;
                flag = true;
            }
            else
            {
                ClientTypeAnnotation annotation3;
                ClientEdmModel model = ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion);
                IEdmType orCreateEdmType = model.GetOrCreateEdmType(value.GetType());
                switch (orCreateEdmType.TypeKind)
                {
                    case EdmTypeKind.Primitive:
                        obj2 = value;
                        flag = true;
                        goto Label_0155;

                    case EdmTypeKind.Complex:
                    {
                        ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(orCreateEdmType);
                        obj2 = this.CreateODataComplexValue(clientTypeAnnotation.ElementType, value, null, false, null);
                        SerializationTypeNameAnnotation annotation = ((ODataComplexValue) obj2).GetAnnotation<SerializationTypeNameAnnotation>();
                        if ((annotation == null) || string.IsNullOrEmpty(annotation.TypeName))
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.DataServiceException_GeneralError);
                        }
                        goto Label_0155;
                    }
                    case EdmTypeKind.Collection:
                    {
                        IEdmCollectionType type2 = orCreateEdmType as IEdmCollectionType;
                        IEdmTypeReference elementType = type2.ElementType;
                        annotation3 = model.GetClientTypeAnnotation(elementType.Definition);
                        switch (annotation3.EdmType.TypeKind)
                        {
                            case EdmTypeKind.Primitive:
                            case EdmTypeKind.Complex:
                                obj2 = this.CreateODataCollection(annotation3.ElementType, null, value, null);
                                goto Label_0155;
                        }
                        break;
                    }
                    default:
                        throw new NotSupportedException(System.Data.Services.Client.Strings.Serializer_InvalidParameterType(paramName, orCreateEdmType.TypeKind));
                }
                throw new NotSupportedException(System.Data.Services.Client.Strings.Serializer_InvalidCollectionParamterItemType(paramName, annotation3.EdmType.TypeKind));
            }
        Label_0155:
            str = ODataUriUtils.ConvertToUriLiteral(obj2, CommonUtil.ConvertToODataVersion(this.requestInfo.MaxProtocolVersionAsVersion));
            if (flag)
            {
                return DataStringEscapeBuilder.EscapeDataString(str);
            }
            return Uri.EscapeDataString(str);
        }

        internal static ODataMessageWriter CreateMessageWriter(ODataRequestMessageWrapper requestMessage, RequestInfo requestInfo)
        {
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings {
                CheckCharacters = false,
                Indent = false,
                DisableMessageStreamDisposal = !requestMessage.IsBatchPartRequest
            };
            if (requestInfo.HasWritingEventHandlers)
            {
                writerSettings.EnableWcfDataServicesClientBehavior(new Func<ODataEntry, XmlWriter, XmlWriter>(Serializer.StartEntryXmlCustomizer), new Action<ODataEntry, XmlWriter, XmlWriter>(Serializer.EndEntryXmlCustomizer), requestInfo.DataNamespace, requestInfo.TypeScheme.AbsoluteUri);
            }
            else
            {
                writerSettings.EnableWcfDataServicesClientBehavior(null, null, requestInfo.DataNamespace, requestInfo.TypeScheme.AbsoluteUri);
            }
            return requestMessage.CreateWriter(writerSettings);
        }

        private ODataCollectionValue CreateODataCollection(Type collectionItemType, string propertyName, object value, List<object> visitedComplexTypeObjects)
        {
            PrimitiveType type;
            string edmType;
            Func<object, object> valueConverter = null;
            Func<object, ODataComplexValue> func2 = null;
            WebUtil.ValidateCollection(collectionItemType, value, propertyName);
            bool flag = PrimitiveType.TryGetPrimitiveType(collectionItemType, out type);
            ODataCollectionValue value2 = new ODataCollectionValue();
            IEnumerable enumerable = (IEnumerable) value;
            if (flag)
            {
                edmType = ClientConvert.GetEdmType(Nullable.GetUnderlyingType(collectionItemType) ?? collectionItemType);
                if (valueConverter == null)
                {
                    valueConverter = delegate (object val) {
                        WebUtil.ValidateCollectionItem(val);
                        WebUtil.ValidatePrimitiveCollectionItem(val, propertyName, collectionItemType);
                        return GetPrimitiveValue(val, collectionItemType);
                    };
                }
                value2.Items = Util.GetEnumerable<object>(enumerable, valueConverter);
            }
            else
            {
                edmType = this.requestInfo.ResolveNameFromType(collectionItemType);
                if (func2 == null)
                {
                    func2 = delegate (object val) {
                        WebUtil.ValidateCollectionItem(val);
                        WebUtil.ValidateComplexCollectionItem(val, propertyName, collectionItemType);
                        return this.CreateODataComplexValue(collectionItemType, val, propertyName, true, visitedComplexTypeObjects);
                    };
                }
                value2.Items = Util.GetEnumerable<ODataComplexValue>(enumerable, func2);
            }
            string str2 = (edmType == null) ? null : string.Format(CultureInfo.InvariantCulture, "Collection({0})", new object[] { edmType });
            SerializationTypeNameAnnotation annotation = new SerializationTypeNameAnnotation {
                TypeName = str2
            };
            value2.SetAnnotation<SerializationTypeNameAnnotation>(annotation);
            return value2;
        }

        private ODataCollectionValue CreateODataCollectionPropertyValue(ClientPropertyAnnotation property, object propertyValue, List<object> visitedComplexTypeObjects)
        {
            return this.CreateODataCollection(property.PrimitiveOrComplexCollectionItemType, property.PropertyName, propertyValue, visitedComplexTypeObjects);
        }

        private ODataComplexValue CreateODataComplexPropertyValue(ClientPropertyAnnotation property, object propertyValue, List<object> visitedComplexTypeObjects)
        {
            Type complexType = property.IsPrimitiveOrComplexCollection ? property.PrimitiveOrComplexCollectionItemType : property.PropertyType;
            return this.CreateODataComplexValue(complexType, propertyValue, property.PropertyName, property.IsPrimitiveOrComplexCollection, visitedComplexTypeObjects);
        }

        private ODataComplexValue CreateODataComplexValue(Type complexType, object value, string propertyName, bool isCollectionItem, List<object> visitedComplexTypeObjects)
        {
            ClientTypeAnnotation clientTypeAnnotation = ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion).GetClientTypeAnnotation(complexType);
            if (visitedComplexTypeObjects == null)
            {
                visitedComplexTypeObjects = new List<object>();
            }
            else if (visitedComplexTypeObjects.Contains(value))
            {
                if (propertyName != null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Serializer_LoopsNotAllowedInComplexTypes(propertyName));
                }
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Serializer_LoopsNotAllowedInNonPropertyComplexTypes(clientTypeAnnotation.ElementTypeName));
            }
            if (value == null)
            {
                return null;
            }
            visitedComplexTypeObjects.Add(value);
            ODataComplexValue value2 = new ODataComplexValue();
            if (!isCollectionItem)
            {
                SerializationTypeNameAnnotation annotation = new SerializationTypeNameAnnotation {
                    TypeName = this.requestInfo.GetServerTypeName(clientTypeAnnotation)
                };
                value2.SetAnnotation<SerializationTypeNameAnnotation>(annotation);
            }
            value2.Properties = this.PopulateProperties(clientTypeAnnotation, value, visitedComplexTypeObjects);
            visitedComplexTypeObjects.Remove(value);
            return value2;
        }

        private static void EndEntryXmlCustomizer(ODataEntry entry, XmlWriter entryWriter, XmlWriter parentWriter)
        {
            WritingEntityInfo annotation = entry.GetAnnotation<WritingEntityInfo>();
            entryWriter.Close();
            annotation.RequestInfo.FireWritingEntityEvent(annotation.Entity, annotation.EntryPayload.Root, null);
            annotation.EntryPayload.Root.WriteTo(parentWriter);
        }

        private static object GetPrimitiveValue(object propertyValue, Type propertyType)
        {
            PrimitiveType type;
            if (propertyValue == null)
            {
                return null;
            }
            PrimitiveType.TryGetPrimitiveType(propertyType, out type);
            if ((((propertyType == typeof(char)) || (propertyType == typeof(char[]))) || ((propertyType == typeof(Type)) || (propertyType == typeof(Uri)))) || ((propertyType == typeof(XDocument)) || (propertyType == typeof(XElement))))
            {
                return type.TypeConverter.ToString(propertyValue);
            }
            if (propertyType.FullName == "System.Data.Linq.Binary")
            {
                return ((BinaryTypeConverter) type.TypeConverter).ToArray(propertyValue);
            }
            if (type.EdmTypeName == null)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CantCastToUnsupportedPrimitive(propertyType.Name));
            }
            return propertyValue;
        }

        internal IEnumerable<ODataProperty> PopulateProperties(ClientTypeAnnotation type, object resource, List<object> visitedComplexTypeObjects)
        {
            List<ODataProperty> list = new List<ODataProperty>();
            foreach (ClientPropertyAnnotation annotation in from p in type.Properties()
                orderby p.PropertyName
                select p)
            {
                if (((!annotation.IsDictionary && (annotation != type.MediaDataMember)) && !annotation.IsStreamLinkProperty) && ((type.MediaDataMember == null) || (type.MediaDataMember.MimeTypeProperty != annotation)))
                {
                    object propertyValue = annotation.GetValue(resource);
                    if (annotation.IsKnownType)
                    {
                        ODataProperty item = new ODataProperty {
                            Name = annotation.EdmProperty.Name,
                            Value = GetPrimitiveValue(propertyValue, annotation.PropertyType)
                        };
                        list.Add(item);
                    }
                    else if (annotation.IsPrimitiveOrComplexCollection)
                    {
                        ODataProperty property2 = new ODataProperty {
                            Name = annotation.EdmProperty.Name,
                            Value = this.CreateODataCollectionPropertyValue(annotation, propertyValue, visitedComplexTypeObjects)
                        };
                        list.Add(property2);
                    }
                    else if (!annotation.IsEntityCollection && !ClientTypeUtil.TypeIsEntity(annotation.PropertyType, this.requestInfo.MaxProtocolVersion))
                    {
                        ODataProperty property3 = new ODataProperty {
                            Name = annotation.EdmProperty.Name,
                            Value = this.CreateODataComplexPropertyValue(annotation, propertyValue, visitedComplexTypeObjects)
                        };
                        list.Add(property3);
                    }
                }
            }
            return list;
        }

        internal static string SerializeSupportedVersions()
        {
            StringBuilder builder = new StringBuilder("'").Append(Util.SupportedResponseVersions[0].ToString());
            for (int i = 1; i < Util.SupportedResponseVersions.Length; i++)
            {
                builder.Append("', '");
                builder.Append(Util.SupportedResponseVersions[i].ToString());
            }
            builder.Append("'");
            return builder.ToString();
        }

        private static XmlWriter StartEntryXmlCustomizer(ODataEntry entry, XmlWriter entryWriter)
        {
            return entry.GetAnnotation<WritingEntityInfo>().EntryPayload.CreateWriter();
        }

        internal void WriteBodyOperationParameters(List<BodyOperationParameter> operationParameters, ODataRequestMessageWrapper requestMessage)
        {
            using (ODataMessageWriter writer = CreateMessageWriter(requestMessage, this.requestInfo))
            {
                ODataParameterWriter writer2 = writer.CreateODataParameterWriter(null);
                writer2.WriteStart();
                foreach (OperationParameter parameter in operationParameters)
                {
                    IEnumerator enumerator;
                    ODataCollectionWriter writer3;
                    object obj2;
                    if (parameter.Value == null)
                    {
                        writer2.WriteValue(parameter.Name, parameter.Value);
                        continue;
                    }
                    ClientEdmModel model = ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion);
                    IEdmType orCreateEdmType = model.GetOrCreateEdmType(parameter.Value.GetType());
                    switch (orCreateEdmType.TypeKind)
                    {
                        case EdmTypeKind.Primitive:
                        {
                            writer2.WriteValue(parameter.Name, parameter.Value);
                            continue;
                        }
                        case EdmTypeKind.Complex:
                        {
                            ODataComplexValue parameterValue = this.CreateODataComplexValue(model.GetClientTypeAnnotation(orCreateEdmType).ElementType, parameter.Value, null, false, null);
                            writer2.WriteValue(parameter.Name, parameterValue);
                            continue;
                        }
                        case EdmTypeKind.Collection:
                        {
                            enumerator = ((ICollection) parameter.Value).GetEnumerator();
                            writer3 = writer2.CreateCollectionWriter(parameter.Name);
                            ODataCollectionStart collectionStart = new ODataCollectionStart();
                            writer3.WriteStart(collectionStart);
                            goto Label_016D;
                        }
                        default:
                            throw new NotSupportedException(System.Data.Services.Client.Strings.Serializer_InvalidParameterType(parameter.Name, orCreateEdmType.TypeKind));
                    }
                Label_00D3:
                    obj2 = enumerator.Current;
                    if (obj2 == null)
                    {
                        throw new NotSupportedException(System.Data.Services.Client.Strings.Serializer_NullCollectionParamterItemValue(parameter.Name));
                    }
                    IEdmType edmType = model.GetOrCreateEdmType(obj2.GetType());
                    switch (edmType.TypeKind)
                    {
                        case EdmTypeKind.Primitive:
                            writer3.WriteItem(obj2);
                            break;

                        case EdmTypeKind.Complex:
                        {
                            ODataComplexValue item = this.CreateODataComplexValue(model.GetClientTypeAnnotation(edmType).ElementType, obj2, null, false, null);
                            writer3.WriteItem(item);
                            break;
                        }
                        default:
                            throw new NotSupportedException(System.Data.Services.Client.Strings.Serializer_InvalidCollectionParamterItemType(parameter.Name, edmType.TypeKind));
                    }
                Label_016D:
                    if (enumerator.MoveNext())
                    {
                        goto Label_00D3;
                    }
                    writer3.WriteEnd();
                    writer3.Flush();
                }
                writer2.WriteEnd();
                writer2.Flush();
            }
        }

        internal void WriteEntityReferenceLink(LinkDescriptor binding, ODataRequestMessageWrapper requestMessage)
        {
            using (ODataMessageWriter writer = CreateMessageWriter(requestMessage, this.requestInfo))
            {
                Uri resourceUri;
                EntityDescriptor entityDescriptor = this.requestInfo.EntityTracker.GetEntityDescriptor(binding.Target);
                if (entityDescriptor.GetLatestIdentity() != null)
                {
                    resourceUri = entityDescriptor.GetResourceUri(this.requestInfo.BaseUriResolver, false);
                }
                else
                {
                    resourceUri = Util.CreateUri("$" + entityDescriptor.ChangeOrder.ToString(CultureInfo.InvariantCulture), UriKind.Relative);
                }
                ODataEntityReferenceLink link = new ODataEntityReferenceLink {
                    Url = resourceUri
                };
                writer.WriteEntityReferenceLink(link);
            }
        }

        internal void WriteEntry(EntityDescriptor entityDescriptor, IEnumerable<LinkDescriptor> relatedLinks, ODataRequestMessageWrapper requestMessage)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityDescriptor.Entity.GetType()));
            using (ODataMessageWriter writer = CreateMessageWriter(requestMessage, this.requestInfo))
            {
                ODataWriter odataWriter = writer.CreateODataEntryWriter();
                ODataEntry entry = new ODataEntry();
                if (this.requestInfo.HasWritingEventHandlers)
                {
                    entry.SetAnnotation<WritingEntityInfo>(new WritingEntityInfo(entityDescriptor.Entity, this.requestInfo));
                }
                string serverTypeName = this.requestInfo.GetServerTypeName(entityDescriptor);
                if (clientTypeAnnotation.ElementTypeName != serverTypeName)
                {
                    SerializationTypeNameAnnotation annotation = new SerializationTypeNameAnnotation {
                        TypeName = serverTypeName
                    };
                    entry.SetAnnotation<SerializationTypeNameAnnotation>(annotation);
                }
                entry.TypeName = clientTypeAnnotation.ElementTypeName;
                if (EntityStates.Modified == entityDescriptor.State)
                {
                    entry.Id = entityDescriptor.GetLatestIdentity();
                }
                if (entityDescriptor.IsMediaLinkEntry || clientTypeAnnotation.IsMediaLinkEntry)
                {
                    entry.MediaResource = new ODataStreamReferenceValue();
                }
                odataWriter.WriteStart(entry);
                if (EntityStates.Added == entityDescriptor.State)
                {
                    this.WriteNavigationLink(entityDescriptor, relatedLinks, odataWriter);
                }
                entry.Properties = this.PopulateProperties(clientTypeAnnotation, entityDescriptor.Entity, null);
                odataWriter.WriteEnd();
            }
        }

        internal void WriteNavigationLink(EntityDescriptor entityDescriptor, IEnumerable<LinkDescriptor> relatedLinks, ODataWriter odataWriter)
        {
            ClientTypeAnnotation clientTypeAnnotation = null;
            foreach (LinkDescriptor descriptor in relatedLinks)
            {
                descriptor.ContentGeneratedForSave = true;
                if (clientTypeAnnotation == null)
                {
                    ClientEdmModel model = ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion);
                    clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityDescriptor.Entity.GetType()));
                }
                ODataNavigationLink navigationLink = new ODataNavigationLink {
                    Url = this.requestInfo.EntityTracker.GetEntityDescriptor(descriptor.Target).GetLatestEditLink(),
                    IsCollection = new bool?(clientTypeAnnotation.GetProperty(descriptor.SourceProperty, false).IsEntityCollection),
                    Name = descriptor.SourceProperty
                };
                odataWriter.WriteStart(navigationLink);
                ODataEntityReferenceLink entityReferenceLink = new ODataEntityReferenceLink {
                    Url = navigationLink.Url
                };
                odataWriter.WriteEntityReferenceLink(entityReferenceLink);
                odataWriter.WriteEnd();
            }
        }

        internal Uri WriteUriOperationParametersToUri(Uri requestUri, List<UriOperationParameter> operationParameters)
        {
            UriBuilder builder = new UriBuilder(requestUri);
            StringBuilder builder2 = new StringBuilder();
            string str = CommonUtil.UriToString(builder.Uri);
            if (!string.IsNullOrEmpty(builder.Query))
            {
                builder2.Append(builder.Query.Substring(1));
                builder2.Append('&');
            }
            foreach (UriOperationParameter parameter in operationParameters)
            {
                string str2 = parameter.Name.Trim();
                if (str2.StartsWith(char.ToString('@'), StringComparison.OrdinalIgnoreCase) && !str.Contains(str2))
                {
                    throw new DataServiceRequestException(System.Data.Services.Client.Strings.Serializer_UriDoesNotContainParameterAlias(parameter.Name));
                }
                builder2.Append(str2);
                builder2.Append('=');
                builder2.Append(this.ConvertToEscapedUriValue(str2, parameter.Value));
                builder2.Append('&');
            }
            builder2.Remove(builder2.Length - 1, 1);
            builder.Query = builder2.ToString();
            return builder.Uri;
        }

        internal sealed class WritingEntityInfo
        {
            internal readonly object Entity;
            internal readonly XDocument EntryPayload;
            internal readonly System.Data.Services.Client.RequestInfo RequestInfo;

            internal WritingEntityInfo(object entity, System.Data.Services.Client.RequestInfo requestInfo)
            {
                this.Entity = entity;
                this.EntryPayload = new XDocument();
                this.RequestInfo = requestInfo;
            }
        }
    }
}

