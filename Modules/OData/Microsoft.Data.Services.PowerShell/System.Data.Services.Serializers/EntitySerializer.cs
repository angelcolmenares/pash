namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Atom;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class EntitySerializer : Serializer
    {
        private readonly ODataFormat contentFormat;
        private readonly ODataMessageWriter messageWriter;
        private ODataWriter odataWriter;

        internal EntitySerializer(RequestDescription requestDescription, Uri absoluteServiceUri, IDataService service, string httpETagHeaderValue, ODataMessageWriter messageWriter, ODataFormat contentFormat) : base(requestDescription, absoluteServiceUri, service, httpETagHeaderValue)
        {
            this.messageWriter = messageWriter;
            this.contentFormat = contentFormat;
        }

        internal override void Flush()
        {
            if (this.odataWriter != null)
            {
                this.odataWriter.Flush();
            }
        }

        private IEnumerable<ODataProperty> GetAllEntityProperties(object customObject, ResourceType currentResourceType, Uri relativeUri)
        {
            List<ODataProperty> list = new List<ODataProperty>(currentResourceType.Properties.Count);
            foreach (ResourceProperty property in base.Provider.GetResourceSerializableProperties(base.CurrentContainer, currentResourceType))
            {
                if (property.TypeKind != ResourceTypeKind.EntityType)
                {
                    list.Add(this.GetODataPropertyForEntityProperty(customObject, currentResourceType, relativeUri, property));
                }
            }
            if (currentResourceType.IsOpenType)
            {
                foreach (KeyValuePair<string, object> pair in base.Provider.GetOpenPropertyValues(customObject))
                {
                    string key = pair.Key;
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new DataServiceException(500, System.Data.Services.Strings.Syndication_InvalidOpenPropertyName(currentResourceType.FullName));
                    }
                    list.Add(this.GetODataPropertyForOpenProperty(key, pair.Value));
                }
                if (!currentResourceType.HasEntityPropertyMappings || (this.contentFormat == ODataFormat.VerboseJson))
                {
                    return list;
                }
                HashSet<string> propertiesLookup = new HashSet<string>(from p in list select p.Name);
                foreach (EpmSourcePathSegment segment in from p in currentResourceType.EpmSourceTree.Root.SubProperties
                    where !propertiesLookup.Contains(p.PropertyName)
                    select p)
                {
                    list.Add(this.GetODataPropertyForOpenProperty(segment.PropertyName, base.Provider.GetOpenPropertyValue(customObject, segment.PropertyName)));
                }
            }
            return list;
        }

        private static ODataAssociationLink GetAssociationLink(Uri relativeUri, ResourceProperty navigationProperty)
        {
			Uri uri = RequestUriProcessor.AppendUnescapedSegment(RequestUriProcessor.AppendEscapedSegment(relativeUri, "$links"), navigationProperty.Name);
            return new ODataAssociationLink { Name = navigationProperty.Name, Url = uri };
        }

        private IEnumerable<ODataAssociationLink> GetEntityAssociationLinks(ResourceType currentResourceType, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
        {
            if (!base.Service.Configuration.DataServiceBehavior.ShouldIncludeAssociationLinksInResponse)
            {
                return null;
            }
            List<ODataAssociationLink> list = new List<ODataAssociationLink>(currentResourceType.Properties.Count);
            if (projectionNodesForCurrentResourceType == null)
            {
                foreach (ResourceProperty property in base.Provider.GetResourceSerializableProperties(base.CurrentContainer, currentResourceType))
                {
                    if (property.TypeKind == ResourceTypeKind.EntityType)
                    {
                        list.Add(GetAssociationLink(relativeUri, property));
                    }
                }
                return list;
            }
            foreach (ProjectionNode node in projectionNodesForCurrentResourceType)
            {
                string propertyName = node.PropertyName;
                ResourceProperty navigationProperty = node.TargetResourceType.TryResolvePropertyName(propertyName);
                if (((navigationProperty != null) && (navigationProperty.TypeKind == ResourceTypeKind.EntityType)) && (list != null))
                {
                    list.Add(GetAssociationLink(relativeUri, navigationProperty));
                }
            }
            return list;
        }

        private IEnumerable<ODataProperty> GetEntityProperties(object customObject, ResourceType currentResourceType, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
        {
            IEnumerable<ODataProperty> enumerable;
            base.RecurseEnter();
            try
            {
                if (projectionNodesForCurrentResourceType == null)
                {
                    return this.GetAllEntityProperties(customObject, currentResourceType, relativeUri);
                }
                enumerable = this.GetProjectedEntityProperties(customObject, currentResourceType, relativeUri, projectionNodesForCurrentResourceType);
            }
            finally
            {
                base.RecurseLeave();
            }
            return enumerable;
        }

        private ODataStreamReferenceValue GetMediaResource(object element, ResourceType entityResourceType, string title, Uri relativeUri)
        {
            ODataStreamReferenceValue value2 = null;
            if (entityResourceType.IsMediaLinkEntry)
            {
                string str;
                Uri uri;
                string str2;
                base.Service.StreamProvider.GetStreamDescription(element, null, base.Service.OperationContext, out str, out uri, out str2);
				Uri uri2 = RequestUriProcessor.AppendEscapedSegment(relativeUri, "$value");
                value2 = new ODataStreamReferenceValue {
                    EditLink = uri2,
                    ContentType = str2,
                    ReadLink = uri ?? uri2
                };
                AtomStreamReferenceMetadata metadata2 = new AtomStreamReferenceMetadata();
                AtomLinkMetadata metadata3 = new AtomLinkMetadata {
                    Title = title
                };
                metadata2.EditLink = metadata3;
                AtomStreamReferenceMetadata annotation = metadata2;
                value2.SetAnnotation<AtomStreamReferenceMetadata>(annotation);
                if (!string.IsNullOrEmpty(str))
                {
                    value2.ETag = str;
                }
            }
            return value2;
        }

        private ODataStreamReferenceValue GetNamedStreamPropertyValue(object element, ResourceProperty namedStreamProperty, Uri relativeUri)
        {
            string str;
            Uri uri;
            string str2;
            base.Service.StreamProvider.GetStreamDescription(element, namedStreamProperty, base.Service.OperationContext, out str, out uri, out str2);
            ODataStreamReferenceValue value2 = new ODataStreamReferenceValue {
                ContentType = str2,
				EditLink = RequestUriProcessor.AppendUnescapedSegment(relativeUri, namedStreamProperty.Name)
            };
            if (!string.IsNullOrEmpty(str))
            {
                value2.ETag = str;
            }
            value2.ReadLink = uri;
            return value2;
        }

        private ODataProperty GetODataPropertyForEntityProperty(object customObject, ResourceType currentResourceType, Uri relativeUri, ResourceProperty property)
        {
            object obj2;
            if (property.IsOfKind(ResourcePropertyKind.Stream))
            {
                obj2 = this.GetNamedStreamPropertyValue(customObject, property, relativeUri);
            }
            else
            {
                object propertyValue = WebUtil.GetPropertyValue(base.Provider, customObject, currentResourceType, property, null);
                obj2 = base.GetPropertyValue(property.Name, property.ResourceType, propertyValue, property == null);
            }
            return new ODataProperty { Name = property.Name, Value = obj2 };
        }

        private ODataProperty GetODataPropertyForOpenProperty(string propertyName, object propertyValue)
        {
            ResourceType primitiveStringResourceType;
            if (WebUtil.IsNullValue(propertyValue))
            {
                primitiveStringResourceType = ResourceType.PrimitiveStringResourceType;
            }
            else
            {
                primitiveStringResourceType = WebUtil.GetResourceType(base.Provider, propertyValue);
                if (primitiveStringResourceType == null)
                {
                    throw new DataServiceException(500, System.Data.Services.Strings.Syndication_InvalidOpenPropertyType(propertyName));
                }
            }
            return new ODataProperty { Name = propertyName, Value = base.GetPropertyValue(propertyName, primitiveStringResourceType, propertyValue, true) };
        }

        private IEnumerable<ODataProperty> GetProjectedEntityProperties(object customObject, ResourceType currentResourceType, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
        {
            List<ODataProperty> source = new List<ODataProperty>(currentResourceType.Properties.Count);
            foreach (ProjectionNode node in projectionNodesForCurrentResourceType)
            {
                string str = node.PropertyName;
                ResourceProperty property = node.TargetResourceType.TryResolvePropertyName(str);
                if (property != null)
                {
                    if (property.TypeKind != ResourceTypeKind.EntityType)
                    {
                        source.Add(this.GetODataPropertyForEntityProperty(customObject, currentResourceType, relativeUri, property));
                    }
                }
                else
                {
                    object propertyValue = WebUtil.GetPropertyValue(base.Provider, customObject, currentResourceType, null, str);
                    source.Add(this.GetODataPropertyForOpenProperty(str, propertyValue));
                }
            }
            if (currentResourceType.HasEntityPropertyMappings)
            {
                foreach (EpmSourcePathSegment segment in currentResourceType.EpmSourceTree.Root.SubProperties)
                {
                    string propertyName = segment.PropertyName;
                    if (source.FirstOrDefault<ODataProperty>(p => (p.Name == propertyName)) == null)
                    {
                        ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                        ResourceProperty resourceProperty = currentResourceType.TryResolvePropertyName(propertyName, stream);
                        object obj3 = WebUtil.GetPropertyValue(base.Provider, customObject, currentResourceType, resourceProperty, (resourceProperty == null) ? propertyName : null);
                        if (resourceProperty != null)
                        {
                            ODataProperty item = new ODataProperty {
                                Name = propertyName,
                                Value = base.GetPropertyValue(propertyName, resourceProperty.ResourceType, obj3, resourceProperty == null)
                            };
                            source.Add(item);
                        }
                        else
                        {
                            source.Add(this.GetODataPropertyForOpenProperty(propertyName, obj3));
                        }
                    }
                }
            }
            return source;
        }

        private void PopulateODataOperations(object resourceInstance, bool resourceInstanceInFeed, ODataEntry entry, ResourceType actualResourceType)
        {
            Func<OperationWrapper, bool> predicate = null;
            ResourceType resourceType = base.CurrentContainer.ResourceType;
            IEnumerable<OperationWrapper> operationProjections = base.GetOperationProjections();
            if (operationProjections == null)
            {
                operationProjections = base.Service.ActionProvider.GetServiceActionsByBindingParameterType(base.Service.OperationContext, actualResourceType);
            }
            else
            {
                if (predicate == null)
                {
                    predicate = o => o.BindingParameter.ParameterType.IsAssignableFrom(actualResourceType);
                }
                operationProjections = operationProjections.Where<OperationWrapper>(predicate);
            }
            if (operationProjections.Any<OperationWrapper>())
            {
                List<ODataAction> source = new List<ODataAction>();
                string containerName = base.Service.Provider.ContainerName;
                foreach (OperationWrapper wrapper in operationProjections)
                {
                    string text = null;
                    string actionTitleSegmentByResourceType = wrapper.GetActionTitleSegmentByResourceType(actualResourceType, containerName);
                    ResourceType parameterType = wrapper.BindingParameter.ParameterType;
                    if ((parameterType != resourceType) && resourceType.IsAssignableFrom(parameterType))
                    {
                        text = wrapper.BindingParameter.ParameterType.FullName + "/" + actionTitleSegmentByResourceType;
                    }
                    else
                    {
                        text = actionTitleSegmentByResourceType;
                    }
                    Uri uri = new Uri(entry.Id, UriKind.RelativeOrAbsolute);
                    Uri oDataOperationMetadata = base.GetODataOperationMetadata(base.AbsoluteServiceUri, wrapper.Name);
					Uri uri3 = RequestUriProcessor.AppendUnescapedSegment(uri, text);
                    ODataAction item = new ODataAction {
                        Title = wrapper.Name,
                        Metadata = oDataOperationMetadata,
                        Target = uri3
                    };
                    if (wrapper.OperationParameterBindingKind == OperationParameterBindingKind.Always)
                    {
                        source.Add(item);
                    }
                    else if (base.Service.ActionProvider.AdvertiseServiceAction(base.Service.OperationContext, wrapper, resourceInstance, resourceInstanceInFeed, ref item))
                    {
                        if (item == null)
                        {
                            throw new DataServiceException(500, System.Data.Services.Strings.DataServiceActionProviderWrapper_AdvertiseServiceActionCannotReturnNullActionToSerialize);
                        }
                        source.Add(item);
                    }
                }
                if (source.Any<ODataAction>())
                {
                    entry.Actions = source;
                }
            }
        }

        private void WriteEntry(IExpandedResult expanded, object element, bool resourceInstanceInFeed, ResourceType expectedType)
        {
            Uri uri;
            Func<ProjectionNode, bool> predicate = null;
            base.IncrementSegmentResultCount();
            ODataEntry entry = new ODataEntry();
            AtomEntryMetadata annotation = new AtomEntryMetadata();
            entry.SetAnnotation<AtomEntryMetadata>(annotation);
            string name = expectedType.Name;
            ResourceType actualResourceType = WebUtil.GetNonPrimitiveResourceType(base.Provider, element);
            if (actualResourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.BadProvider_InconsistentEntityOrComplexTypeUsage(actualResourceType.FullName));
            }
            Uri absoluteUri = Serializer.GetIdAndEditLink(element, actualResourceType, base.Provider, base.CurrentContainer, base.AbsoluteServiceUri, out uri);
            Uri relativeUri = new Uri(absoluteUri.AbsoluteUri.Substring(base.AbsoluteServiceUri.AbsoluteUri.Length), UriKind.Relative);
            entry.MediaResource = this.GetMediaResource(element, actualResourceType, name, relativeUri);
            entry.TypeName = actualResourceType.FullName;
            entry.Id = uri.AbsoluteUri;
            entry.EditLink = relativeUri;
            AtomLinkMetadata metadata2 = new AtomLinkMetadata {
                Title = name
            };
            annotation.EditLink = metadata2;
            string eTagValue = base.GetETagValue(element, actualResourceType);
            if (eTagValue != null)
            {
                entry.ETag = eTagValue;
            }
            IEnumerable<ProjectionNode> projections = base.GetProjections();
            if (projections != null)
            {
                if (predicate == null)
                {
                    predicate = projectionNode => projectionNode.TargetResourceType.IsAssignableFrom(actualResourceType);
                }
                projections = projections.Where<ProjectionNode>(predicate);
                entry.SetAnnotation<ProjectedPropertiesAnnotation>(new ProjectedPropertiesAnnotation(from p in projections select p.PropertyName));
            }
            entry.AssociationLinks = this.GetEntityAssociationLinks(actualResourceType, relativeUri, projections);
            this.PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType);
            this.odataWriter.WriteStart(entry);
            this.WriteNavigationProperties(expanded, element, resourceInstanceInFeed, actualResourceType, absoluteUri, relativeUri, projections);
            entry.Properties = this.GetEntityProperties(element, actualResourceType, relativeUri, projections);
            this.odataWriter.WriteEnd();
        }

        private void WriteFeedElements(IExpandedResult expanded, IEnumerator elements, ResourceType expectedType, string title, Uri relativeUri, Uri absoluteUri, bool hasMoved, bool topLevel)
        {
            ODataFeed feed = new ODataFeed {
                Id = absoluteUri.AbsoluteUri
            };
            AtomFeedMetadata annotation = new AtomFeedMetadata();
            feed.SetAnnotation<AtomFeedMetadata>(annotation);
            AtomTextConstruct construct = new AtomTextConstruct {
                Text = title
            };
            annotation.Title = construct;
            AtomLinkMetadata metadata2 = new AtomLinkMetadata {
                Href = relativeUri,
                Title = title
            };
            annotation.SelfLink = metadata2;
            bool flag = false;
            if (topLevel && (base.RequestDescription.CountOption == RequestQueryCountOption.Inline))
            {
                flag = this.contentFormat == ODataFormat.VerboseJson;
                if (!flag)
                {
                    feed.Count = new long?(base.RequestDescription.CountValue);
                }
            }
            this.odataWriter.WriteStart(feed);
            try
            {
                object lastObject = null;
                IExpandedResult skipTokenExpandedResult = null;
                while (hasMoved)
                {
                    object current = elements.Current;
                    IExpandedResult skipToken = base.GetSkipToken(expanded);
                    if (current != null)
                    {
                        IExpandedResult result3 = current as IExpandedResult;
                        if (result3 != null)
                        {
                            expanded = result3;
                            current = Serializer.GetExpandedElement(expanded);
                            skipToken = base.GetSkipToken(expanded);
                        }
                        this.WriteEntry(expanded, current, true, expectedType);
                    }
                    hasMoved = elements.MoveNext();
                    lastObject = current;
                    skipTokenExpandedResult = skipToken;
                }
                if (flag)
                {
                    feed.Count = new long?(base.RequestDescription.CountValue);
                }
                if (base.NeedNextPageLink(elements))
                {
                    feed.NextPageLink = base.GetNextLinkUri(lastObject, skipTokenExpandedResult, absoluteUri);
                }
            }
            finally
            {
                if (!topLevel)
                {
                    WebUtil.Dispose(elements);
                }
            }
            this.odataWriter.WriteEnd();
        }

        private void WriteNavigationProperties(IExpandedResult expanded, object customObject, bool resourceInstanceInFeed, ResourceType currentResourceType, Uri absoluteUri, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
        {
			Func<ProjectionNode, bool> __CachedAnonymousMethodDelegate1b = null;
            if ((projectionNodesForCurrentResourceType != null))
            {
				__CachedAnonymousMethodDelegate1b = p => (p.Property != null) && (p.Property.TypeKind == ResourceTypeKind.EntityType);
            }
			IEnumerable<ResourceProperty> enumerable = (projectionNodesForCurrentResourceType == null) ? (from p in base.Provider.GetResourceSerializableProperties(base.CurrentContainer, currentResourceType)
                where p.TypeKind == ResourceTypeKind.EntityType
                select p) : projectionNodesForCurrentResourceType.Where<ProjectionNode>(__CachedAnonymousMethodDelegate1b).Select<ProjectionNode, ResourceProperty>(x => x.Property);
            foreach (ResourceProperty property in enumerable)
            {
                ODataNavigationLink link = null;
                Serializer.ResourcePropertyInfo info = base.GetNavigationPropertyInfo(expanded, customObject, currentResourceType, property);
                link = new ODataNavigationLink {
                    Name = info.Property.Name,
                    IsCollection = new bool?(info.Property.Kind == ResourcePropertyKind.ResourceSetReference),
                    Url = Serializer.AppendEntryToUri(relativeUri, link.Name)
                };
                this.odataWriter.WriteStart(link);
                if (!info.Expand)
                {
                    goto Label_0215;
                }
                object obj2 = info.Value;
                IExpandedResult result = obj2 as IExpandedResult;
                object element = (result != null) ? Serializer.GetExpandedElement(result) : obj2;
                bool needPop = base.PushSegmentForProperty(info.Property, currentResourceType, info.ExpandedNode);
                if (base.CurrentContainer != null)
                {
                    if (info.Property.Kind == ResourcePropertyKind.ResourceSetReference)
                    {
                        IEnumerable enumerable2;
                        WebUtil.IsElementIEnumerable(element, out enumerable2);
                        IEnumerator elements = enumerable2.GetEnumerator();
                        try
                        {
                            bool hasMoved = elements.MoveNext();
                            Uri uri = Serializer.AppendEntryToUri(absoluteUri, link.Name);
                            this.WriteFeedElements(result, elements, info.Property.ResourceType, link.Name, link.Url, uri, hasMoved, false);
                            goto Label_020D;
                        }
                        catch
                        {
                            WebUtil.Dispose(elements);
                            throw;
                        }
                    }
                    if (WebUtil.IsNullValue(element))
                    {
                        this.odataWriter.WriteStart((ODataEntry) null);
                        this.odataWriter.WriteEnd();
                    }
                    else
                    {
                        this.WriteEntry(result, element, resourceInstanceInFeed, info.Property.ResourceType);
                    }
                }
            Label_020D:
                base.PopSegmentName(needPop);
            Label_0215:
                this.odataWriter.WriteEnd();
            }
        }

        protected override void WriteTopLevelElement(IExpandedResult expanded, object element)
        {
            ResourceType targetResourceType;
            this.odataWriter = this.messageWriter.CreateODataEntryWriter();
            if ((base.RequestDescription.TargetSource == RequestTargetSource.EntitySet) || (base.RequestDescription.TargetSource == RequestTargetSource.ServiceOperation))
            {
                targetResourceType = base.RequestDescription.TargetResourceType;
            }
            else
            {
                targetResourceType = base.RequestDescription.Property.ResourceType;
            }
            bool needPop = base.PushSegmentForRoot();
            this.WriteEntry(expanded, element, false, targetResourceType);
            base.PopSegmentName(needPop);
        }

        protected override void WriteTopLevelElements(IExpandedResult expanded, IEnumerator elements, bool hasMoved)
        {
            string name;
            if ((base.RequestDescription.TargetKind != RequestTargetKind.OpenProperty) && (base.RequestDescription.TargetSource == RequestTargetSource.Property))
            {
                name = base.RequestDescription.Property.Name;
            }
            else
            {
                name = base.RequestDescription.ContainerName;
            }
            this.odataWriter = this.messageWriter.CreateODataFeedWriter();
            bool needPop = base.PushSegmentForRoot();
            this.WriteFeedElements(expanded, elements, base.RequestDescription.TargetResourceType.ElementType(), name, new Uri(base.RequestDescription.LastSegmentInfo.Identifier, UriKind.Relative), base.RequestDescription.ResultUri, hasMoved, true);
            base.PopSegmentName(needPop);
        }
    }
}

