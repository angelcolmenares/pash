namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal sealed class ODataJsonEntryAndFeedDeserializer : ODataJsonPropertyAndValueDeserializer
    {
        internal ODataJsonEntryAndFeedDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
        }

        private static void AddEntryProperty(IODataJsonReaderEntryState entryState, string propertyName, object propertyValue)
        {
            ODataProperty property = new ODataProperty {
                Name = propertyName,
                Value = propertyValue
            };
            entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
            ReaderUtils.AddPropertyToPropertiesList(entryState.Entry.Properties, property);
        }

        internal bool IsDeferredLink(bool navigationLinkFound)
        {
            bool flag;
            switch (base.JsonReader.NodeType)
            {
                case JsonNodeType.PrimitiveValue:
                    if ((base.JsonReader.Value != null) && navigationLinkFound)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotReadNavigationPropertyValue);
                    }
                    return false;

                case JsonNodeType.StartArray:
                    return false;
            }
            base.JsonReader.StartBuffering();
            try
            {
                base.JsonReader.ReadStartObject();
                if (base.JsonReader.NodeType == JsonNodeType.EndObject)
                {
                    return false;
                }
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("__deferred", strB) != 0)
                {
                    return false;
                }
                base.JsonReader.SkipValue();
                flag = base.JsonReader.NodeType == JsonNodeType.EndObject;
            }
            finally
            {
                base.JsonReader.StopBuffering();
            }
            return flag;
        }

        internal bool IsEntityReferenceLink()
        {
            bool flag2;
            if (base.JsonReader.NodeType != JsonNodeType.StartObject)
            {
                return false;
            }
            base.JsonReader.StartBuffering();
            try
            {
                base.JsonReader.ReadStartObject();
                if (base.JsonReader.NodeType == JsonNodeType.EndObject)
                {
                    return false;
                }
                bool flag = false;
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = base.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("__metadata", strB) != 0)
                    {
                        return false;
                    }
                    if (base.JsonReader.NodeType != JsonNodeType.StartObject)
                    {
                        return false;
                    }
                    base.JsonReader.ReadStartObject();
                    while (base.JsonReader.NodeType == JsonNodeType.Property)
                    {
                        string str2 = base.JsonReader.ReadPropertyName();
                        if (string.CompareOrdinal("uri", str2) == 0)
                        {
                            flag = true;
                        }
                        base.JsonReader.SkipValue();
                    }
                    base.JsonReader.ReadEndObject();
                }
                flag2 = flag;
            }
            finally
            {
                base.JsonReader.StopBuffering();
            }
            return flag2;
        }

        private void ReadActionsMetadataProperty(ODataEntry entry, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            if ((base.MessageReaderSettings.MaxProtocolVersion >= ODataVersion.V3) && base.ReadingResponse)
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.Actions, "actions");
                this.ReadOperationsMetadata(entry, true);
            }
            else
            {
                base.JsonReader.SkipValue();
            }
        }

        private void ReadContentTypeMetadataProperty(ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField, ref ODataStreamReferenceValue mediaResource)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.ContentType, "content_type");
                ODataJsonReaderUtils.EnsureInstance<ODataStreamReferenceValue>(ref mediaResource);
                string propertyValue = base.JsonReader.ReadStringValue("content_type");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "content_type");
                mediaResource.ContentType = propertyValue;
            }
        }

        internal void ReadDeferredNavigationLink(ODataNavigationLink navigationLink)
        {
            base.JsonReader.ReadStartObject();
            base.JsonReader.ReadPropertyName();
            base.JsonReader.ReadStartObject();
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("uri", strB) == 0)
                {
                    if (navigationLink.Url != null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleUriPropertiesInDeferredLink);
                    }
                    string uriFromPayload = base.JsonReader.ReadStringValue("uri");
                    if (uriFromPayload == null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_DeferredLinkUriCannotBeNull);
                    }
                    navigationLink.Url = base.ProcessUriFromPayload(uriFromPayload);
                }
                else
                {
                    base.JsonReader.SkipValue();
                }
            }
            if (navigationLink.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_DeferredLinkMissingUri);
            }
            base.JsonReader.ReadEndObject();
            base.JsonReader.ReadEndObject();
        }

        private void ReadEditMediaMetadataProperty(ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField, ref ODataStreamReferenceValue mediaResource)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.EditMedia, "edit_media");
                ODataJsonReaderUtils.EnsureInstance<ODataStreamReferenceValue>(ref mediaResource);
                string propertyValue = base.JsonReader.ReadStringValue("edit_media");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "edit_media");
                mediaResource.EditLink = base.ProcessUriFromPayload(propertyValue);
            }
        }

        internal ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            base.JsonReader.ReadStartObject();
            base.JsonReader.ReadPropertyName();
            base.JsonReader.ReadStartObject();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink();
            ODataJsonReaderUtils.MetadataPropertyBitMask none = ODataJsonReaderUtils.MetadataPropertyBitMask.None;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("uri", strB) == 0)
                {
                    ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref none, ODataJsonReaderUtils.MetadataPropertyBitMask.Uri, "uri");
                    string propertyValue = base.JsonReader.ReadStringValue("uri");
                    ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "uri");
                    link.Url = base.ProcessUriFromPayload(propertyValue);
                }
                else
                {
                    base.JsonReader.SkipValue();
                }
            }
            base.JsonReader.ReadEndObject();
            base.JsonReader.ReadEndObject();
            return link;
        }

        internal ODataNavigationLink ReadEntryContent(IODataJsonReaderEntryState entryState, out IEdmNavigationProperty navigationProperty)
        {
            ODataNavigationLink navigationLink = null;
            navigationProperty = null;
            IEdmEntityType entityType = entryState.EntityType;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("__metadata", strB) == 0)
                {
                    if (entryState.MetadataPropertyFound)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesInEntryValue);
                    }
                    entryState.MetadataPropertyFound = true;
                    base.JsonReader.SkipValue();
                }
                else
                {
                    if (!ValidationUtils.IsValidPropertyName(strB))
                    {
                        base.JsonReader.SkipValue();
                        continue;
                    }
                    IEdmProperty edmProperty = ReaderValidationUtils.FindDefinedProperty(strB, entityType, base.MessageReaderSettings);
                    if (edmProperty != null)
                    {
                        navigationProperty = edmProperty as IEdmNavigationProperty;
                        if (navigationProperty != null)
                        {
                            if (this.ShouldEntryPropertyBeSkipped())
                            {
                                base.JsonReader.SkipValue();
                            }
                            else
                            {
                                bool flag = navigationProperty.Type.IsCollection();
                                navigationLink = new ODataNavigationLink {
                                    Name = strB,
                                    IsCollection = new bool?(flag)
                                };
                                this.ValidateNavigationLinkPropertyValue(flag);
                                entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNamesOnNavigationLinkStart(navigationLink);
                            }
                        }
                        else
                        {
                            this.ReadEntryProperty(entryState, edmProperty);
                        }
                    }
                    else if (entityType.IsOpen)
                    {
                        if (this.ShouldEntryPropertyBeSkipped())
                        {
                            base.JsonReader.SkipValue();
                        }
                        else
                        {
                            this.ReadOpenProperty(entryState, strB);
                        }
                    }
                    else
                    {
                        navigationLink = this.ReadUndeclaredProperty(entryState, strB);
                    }
                }
                if (navigationLink != null)
                {
                    return navigationLink;
                }
            }
            return navigationLink;
        }

        internal void ReadEntryMetadataPropertyValue(IODataJsonReaderEntryState entryState)
        {
            ODataEntry entry = entryState.Entry;
            base.JsonReader.ReadStartObject();
            ODataStreamReferenceValue mediaResource = null;
            ODataJsonReaderUtils.MetadataPropertyBitMask none = ODataJsonReaderUtils.MetadataPropertyBitMask.None;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                switch (base.JsonReader.ReadPropertyName())
                {
                    case "uri":
                    {
                        this.ReadUriMetadataProperty(entry, ref none);
                        continue;
                    }
                    case "id":
                    {
                        this.ReadIdMetadataProperty(entry, ref none);
                        continue;
                    }
                    case "etag":
                    {
                        this.ReadETagMetadataProperty(entry, ref none);
                        continue;
                    }
                    case "type":
                    {
                        base.JsonReader.SkipValue();
                        continue;
                    }
                    case "media_src":
                    {
                        this.ReadMediaSourceMetadataProperty(ref none, ref mediaResource);
                        continue;
                    }
                    case "edit_media":
                    {
                        this.ReadEditMediaMetadataProperty(ref none, ref mediaResource);
                        continue;
                    }
                    case "content_type":
                    {
                        this.ReadContentTypeMetadataProperty(ref none, ref mediaResource);
                        continue;
                    }
                    case "media_etag":
                    {
                        this.ReadMediaETagMetadataProperty(ref none, ref mediaResource);
                        continue;
                    }
                    case "actions":
                    {
                        this.ReadActionsMetadataProperty(entry, ref none);
                        continue;
                    }
                    case "functions":
                    {
                        this.ReadFunctionsMetadataProperty(entry, ref none);
                        continue;
                    }
                    case "properties":
                    {
                        this.ReadPropertiesMetadataProperty(entryState, ref none);
                        continue;
                    }
                }
                base.JsonReader.SkipValue();
            }
            entry.MediaResource = mediaResource;
        }

        private void ReadEntryProperty(IODataJsonReaderEntryState entryState, IEdmProperty edmProperty)
        {
            ODataNullValueBehaviorKind kind = base.ReadingResponse ? ODataNullValueBehaviorKind.Default : base.Model.NullValueReadBehaviorKind(edmProperty);
            IEdmTypeReference type = edmProperty.Type;
            object propertyValue = type.IsStream() ? this.ReadStreamPropertyValue() : base.ReadNonEntityValue(type, null, null, kind == ODataNullValueBehaviorKind.Default);
            if ((kind != ODataNullValueBehaviorKind.IgnoreValue) || (propertyValue != null))
            {
                AddEntryProperty(entryState, edmProperty.Name, propertyValue);
            }
        }

        internal void ReadEntryStart()
        {
            if (base.JsonReader.NodeType != JsonNodeType.StartObject)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadEntryStart(base.JsonReader.NodeType));
            }
            base.JsonReader.ReadNext();
        }

        private void ReadETagMetadataProperty(ODataEntry entry, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.ETag, "etag");
                string propertyValue = base.JsonReader.ReadStringValue("etag");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "etag");
                entry.ETag = propertyValue;
            }
        }

        internal void ReadFeedEnd(ODataFeed feed, bool readResultsWrapper, bool isExpandedLinkContent)
        {
            if (readResultsWrapper)
            {
                base.JsonReader.ReadEndArray();
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string propertyName = base.JsonReader.ReadPropertyName();
                    this.ReadFeedProperty(feed, propertyName, isExpandedLinkContent);
                }
            }
        }

        private void ReadFeedProperty(ODataFeed feed, string propertyName, bool isExpandedLinkContent)
        {
            switch (ODataJsonReaderUtils.DetermineFeedPropertyKind(propertyName))
            {
                case ODataJsonReaderUtils.FeedPropertyKind.Unsupported:
                    base.JsonReader.SkipValue();
                    return;

                case ODataJsonReaderUtils.FeedPropertyKind.Count:
                {
                    if ((!base.ReadingResponse || (base.Version < ODataVersion.V2)) || isExpandedLinkContent)
                    {
                        base.JsonReader.SkipValue();
                        return;
                    }
                    string propertyValue = base.JsonReader.ReadStringValue("__count");
                    ODataJsonReaderUtils.ValidateFeedProperty(propertyValue, "__count");
                    long num = (long) ODataJsonReaderUtils.ConvertValue(propertyValue, EdmCoreModel.Instance.GetInt64(false), base.MessageReaderSettings, base.Version, true);
                    feed.Count = new long?(num);
                    return;
                }
                case ODataJsonReaderUtils.FeedPropertyKind.Results:
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleFeedResultsPropertiesFound);

                case ODataJsonReaderUtils.FeedPropertyKind.NextPageLink:
                {
                    if (!base.ReadingResponse || (base.Version < ODataVersion.V2))
                    {
                        base.JsonReader.SkipValue();
                        return;
                    }
                    string str2 = base.JsonReader.ReadStringValue("__next");
                    ODataJsonReaderUtils.ValidateFeedProperty(str2, "__next");
                    feed.NextPageLink = base.ProcessUriFromPayload(str2);
                    return;
                }
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataJsonEntryAndFeedDeserializer_ReadFeedProperty));
        }

        internal void ReadFeedStart(ODataFeed feed, bool isResultsWrapperExpected, bool isExpandedLinkContent)
        {
            if (!isResultsWrapperExpected)
            {
                goto Label_004C;
            }
            base.JsonReader.ReadNext();
        Label_000F:
            if (base.JsonReader.NodeType != JsonNodeType.Property)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_ExpectedFeedResultsPropertyNotFound);
            }
            string strB = base.JsonReader.ReadPropertyName();
            if (string.CompareOrdinal("results", strB) != 0)
            {
                this.ReadFeedProperty(feed, strB, isExpandedLinkContent);
                goto Label_000F;
            }
        Label_004C:
            if (base.JsonReader.NodeType != JsonNodeType.StartArray)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotReadFeedContentStart(base.JsonReader.NodeType));
            }
            base.JsonReader.ReadStartArray();
        }

        private void ReadFunctionsMetadataProperty(ODataEntry entry, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            if ((base.MessageReaderSettings.MaxProtocolVersion >= ODataVersion.V3) && base.ReadingResponse)
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.Functions, "functions");
                this.ReadOperationsMetadata(entry, false);
            }
            else
            {
                base.JsonReader.SkipValue();
            }
        }

        private void ReadIdMetadataProperty(ODataEntry entry, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.Id, "id");
                string propertyValue = base.JsonReader.ReadStringValue("id");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "id");
                entry.Id = propertyValue;
            }
        }

        private void ReadMediaETagMetadataProperty(ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField, ref ODataStreamReferenceValue mediaResource)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.MediaETag, "media_etag");
                ODataJsonReaderUtils.EnsureInstance<ODataStreamReferenceValue>(ref mediaResource);
                string propertyValue = base.JsonReader.ReadStringValue("media_etag");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "media_etag");
                mediaResource.ETag = propertyValue;
            }
        }

        private void ReadMediaSourceMetadataProperty(ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField, ref ODataStreamReferenceValue mediaResource)
        {
            if (base.UseServerFormatBehavior)
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.MediaUri, "media_src");
                ODataJsonReaderUtils.EnsureInstance<ODataStreamReferenceValue>(ref mediaResource);
                string propertyValue = base.JsonReader.ReadStringValue("media_src");
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "media_src");
                mediaResource.ReadLink = base.ProcessUriFromPayload(propertyValue);
            }
        }

        private void ReadOpenProperty(IODataJsonReaderEntryState entryState, string propertyName)
        {
            object obj2 = base.ReadNonEntityValue(null, null, null, true);
            ValidationUtils.ValidateOpenPropertyValue(propertyName, obj2);
            AddEntryProperty(entryState, propertyName, obj2);
        }

        private void ReadOperationsMetadata(ODataEntry entry, bool isActions)
        {
            string str = isActions ? "actions" : "functions";
            if (base.JsonReader.NodeType != JsonNodeType.StartObject)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_PropertyInEntryMustHaveObjectValue(str, base.JsonReader.NodeType));
            }
            base.JsonReader.ReadStartObject();
            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                ODataOperation operation;
                string item = base.JsonReader.ReadPropertyName();
                if (set.Contains(item))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_RepeatMetadataValue(str, item));
                }
                set.Add(item);
                if (base.JsonReader.NodeType != JsonNodeType.StartArray)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MetadataMustHaveArrayValue(str, base.JsonReader.NodeType));
                }
                base.JsonReader.ReadStartArray();
                if (base.JsonReader.NodeType == JsonNodeType.StartObject)
                {
                    goto Label_0227;
                }
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_OperationMetadataArrayExpectedAnObject(str, base.JsonReader.NodeType));
            Label_00E1:
                base.JsonReader.ReadStartObject();
                if (isActions)
                {
                    operation = new ODataAction();
                    ReaderUtils.AddActionToEntry(entry, (ODataAction) operation);
                }
                else
                {
                    operation = new ODataFunction();
                    ReaderUtils.AddFunctionToEntry(entry, (ODataFunction) operation);
                }
                operation.Metadata = base.ResolveUri(item);
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string str3 = base.JsonReader.ReadPropertyName();
                    string str6 = str3;
                    if (str6 == null)
                    {
                        goto Label_01E5;
                    }
                    if (!(str6 == "title"))
                    {
                        if (str6 == "target")
                        {
                            goto Label_019D;
                        }
                        goto Label_01E5;
                    }
                    if (operation.Title != null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleOptionalPropertiesInOperation(str3, item, str));
                    }
                    string propertyValue = base.JsonReader.ReadStringValue("title");
                    ODataJsonReaderUtils.ValidateOperationJsonProperty(propertyValue, str3, item, str);
                    operation.Title = propertyValue;
                    continue;
                Label_019D:
                    if (operation.Target != null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleTargetPropertiesInOperation(item, str));
                    }
                    string str5 = base.JsonReader.ReadStringValue("target");
                    ODataJsonReaderUtils.ValidateOperationJsonProperty(str5, str3, item, str);
                    operation.Target = base.ProcessUriFromPayload(str5);
                    continue;
                Label_01E5:
                    base.JsonReader.SkipValue();
                }
                if (operation.Target == null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_OperationMissingTargetProperty(item, str));
                }
                base.JsonReader.ReadEndObject();
            Label_0227:
                if (base.JsonReader.NodeType == JsonNodeType.StartObject)
                {
                    goto Label_00E1;
                }
                base.JsonReader.ReadEndArray();
            }
            base.JsonReader.ReadEndObject();
        }

        private void ReadPropertiesMetadataProperty(IODataJsonReaderEntryState entryState, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            if (!base.ReadingResponse || (base.MessageReaderSettings.MaxProtocolVersion < ODataVersion.V3))
            {
                base.JsonReader.SkipValue();
            }
            else
            {
                ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.Properties, "properties");
                if (base.JsonReader.NodeType != JsonNodeType.StartObject)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_PropertyInEntryMustHaveObjectValue("properties", base.JsonReader.NodeType));
                }
                base.JsonReader.ReadStartObject();
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string associationLinkName = base.JsonReader.ReadPropertyName();
                    ValidationUtils.ValidateAssociationLinkName(associationLinkName);
                    ReaderValidationUtils.ValidateNavigationPropertyDefined(associationLinkName, entryState.EntityType, base.MessageReaderSettings);
                    base.JsonReader.ReadStartObject();
                    while (base.JsonReader.NodeType == JsonNodeType.Property)
                    {
                        if (string.CompareOrdinal(base.JsonReader.ReadPropertyName(), "associationuri") == 0)
                        {
                            string propertyValue = base.JsonReader.ReadStringValue("associationuri");
                            ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "associationuri");
                            ODataAssociationLink associationLink = new ODataAssociationLink {
                                Name = associationLinkName,
                                Url = base.ProcessUriFromPayload(propertyValue)
                            };
                            ValidationUtils.ValidateAssociationLink(associationLink);
                            entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(associationLink);
                            ReaderUtils.AddAssociationLinkToEntry(entryState.Entry, associationLink);
                        }
                        else
                        {
                            base.JsonReader.SkipValue();
                        }
                    }
                    base.JsonReader.ReadEndObject();
                }
                base.JsonReader.ReadEndObject();
            }
        }

        private ODataStreamReferenceValue ReadStreamPropertyValue()
        {
            if (!base.ReadingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_StreamPropertyInRequest);
            }
            ODataVersionChecker.CheckStreamReferenceProperty(base.Version);
            base.JsonReader.ReadStartObject();
            ODataStreamReferenceValue value2 = null;
            if (base.JsonReader.NodeType != JsonNodeType.Property)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotParseStreamReference);
            }
            string strB = base.JsonReader.ReadPropertyName();
            if (string.CompareOrdinal("__mediaresource", strB) != 0)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotParseStreamReference);
            }
            value2 = this.ReadStreamReferenceValue();
            if (base.JsonReader.NodeType != JsonNodeType.EndObject)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotParseStreamReference);
            }
            base.JsonReader.Read();
            return value2;
        }

        private ODataStreamReferenceValue ReadStreamReferenceValue()
        {
            base.JsonReader.ReadStartObject();
            ODataStreamReferenceValue value2 = new ODataStreamReferenceValue();
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string str6 = base.JsonReader.ReadPropertyName();
                if (str6 == null)
                {
                    goto Label_0186;
                }
                if (!(str6 == "edit_media"))
                {
                    if (str6 == "media_src")
                    {
                        goto Label_00BA;
                    }
                    if (str6 == "content_type")
                    {
                        goto Label_0106;
                    }
                    if (str6 == "media_etag")
                    {
                        goto Label_0146;
                    }
                    goto Label_0186;
                }
                if (value2.EditLink != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty("edit_media"));
                }
                string propertyValue = base.JsonReader.ReadStringValue("edit_media");
                ODataJsonReaderUtils.ValidateMediaResourceStringProperty(propertyValue, "edit_media");
                value2.EditLink = base.ProcessUriFromPayload(propertyValue);
                continue;
            Label_00BA:
                if (value2.ReadLink != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty("media_src"));
                }
                string str3 = base.JsonReader.ReadStringValue("media_src");
                ODataJsonReaderUtils.ValidateMediaResourceStringProperty(str3, "media_src");
                value2.ReadLink = base.ProcessUriFromPayload(str3);
                continue;
            Label_0106:
                if (value2.ContentType != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty("content_type"));
                }
                string str4 = base.JsonReader.ReadStringValue("content_type");
                ODataJsonReaderUtils.ValidateMediaResourceStringProperty(str4, "content_type");
                value2.ContentType = str4;
                continue;
            Label_0146:
                if (value2.ETag != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty("media_etag"));
                }
                string str5 = base.JsonReader.ReadStringValue("media_etag");
                ODataJsonReaderUtils.ValidateMediaResourceStringProperty(str5, "media_etag");
                value2.ETag = str5;
                continue;
            Label_0186:
                base.JsonReader.SkipValue();
            }
            base.JsonReader.ReadEndObject();
            return value2;
        }

        private ODataNavigationLink ReadUndeclaredProperty(IODataJsonReaderEntryState entryState, string propertyName)
        {
            bool flag = false;
            bool flag2 = false;
            if (base.JsonReader.NodeType == JsonNodeType.StartObject)
            {
                base.JsonReader.StartBuffering();
                base.JsonReader.Read();
                if (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strA = base.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal(strA, "__deferred") == 0)
                    {
                        flag2 = true;
                    }
                    else if (string.CompareOrdinal(strA, "__mediaresource") == 0)
                    {
                        flag = true;
                    }
                    base.JsonReader.SkipValue();
                    if (base.JsonReader.NodeType != JsonNodeType.EndObject)
                    {
                        flag = false;
                        flag2 = false;
                    }
                }
                base.JsonReader.StopBuffering();
            }
            if (flag || flag2)
            {
                if (!base.MessageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.ReportUndeclaredLinkProperty))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, entryState.EntityType.ODataFullName()));
                }
            }
            else if (!base.MessageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, entryState.EntityType.ODataFullName()));
            }
            if (flag2)
            {
                ODataNavigationLink navigationLink = new ODataNavigationLink {
                    Name = propertyName,
                    IsCollection = null
                };
                entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNamesOnNavigationLinkStart(navigationLink);
                return navigationLink;
            }
            if (flag)
            {
                object propertyValue = this.ReadStreamPropertyValue();
                AddEntryProperty(entryState, propertyName, propertyValue);
                return null;
            }
            base.JsonReader.SkipValue();
            return null;
        }

        private void ReadUriMetadataProperty(ODataEntry entry, ref ODataJsonReaderUtils.MetadataPropertyBitMask metadataPropertiesFoundBitField)
        {
            ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref metadataPropertiesFoundBitField, ODataJsonReaderUtils.MetadataPropertyBitMask.Uri, "uri");
            string propertyValue = base.JsonReader.ReadStringValue("uri");
            if (propertyValue != null)
            {
                ODataJsonReaderUtils.ValidateMetadataStringProperty(propertyValue, "uri");
                entry.EditLink = base.ProcessUriFromPayload(propertyValue);
            }
        }

        private bool ShouldEntryPropertyBeSkipped()
        {
            return ((!base.ReadingResponse && base.UseServerFormatBehavior) && this.IsDeferredLink(false));
        }

        internal void ValidateEntryMetadata(IODataJsonReaderEntryState entryState)
        {
            ODataEntry entry = entryState.Entry;
            IEdmEntityType entityType = entryState.EntityType;
            if (base.Model.HasDefaultStream(entityType) && (entry.MediaResource == null))
            {
                ODataStreamReferenceValue instance = null;
                ODataJsonReaderUtils.EnsureInstance<ODataStreamReferenceValue>(ref instance);
                entry.MediaResource = instance;
            }
            bool useDefaultFormatBehavior = base.UseDefaultFormatBehavior;
            ValidationUtils.ValidateEntryMetadata(entry, entityType, base.Model, useDefaultFormatBehavior);
        }

        private void ValidateNavigationLinkPropertyValue(bool isCollection)
        {
            JsonNodeType nodeType = base.JsonReader.NodeType;
            if (nodeType == JsonNodeType.StartArray)
            {
                if (!isCollection)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotReadSingletonNavigationPropertyValue(nodeType));
                }
            }
            else if ((nodeType == JsonNodeType.PrimitiveValue) && (base.JsonReader.Value == null))
            {
                if (isCollection)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotReadCollectionNavigationPropertyValue(nodeType));
                }
            }
            else if (nodeType != JsonNodeType.StartObject)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonEntryAndFeedDeserializer_CannotReadNavigationPropertyValue);
            }
        }
    }
}

