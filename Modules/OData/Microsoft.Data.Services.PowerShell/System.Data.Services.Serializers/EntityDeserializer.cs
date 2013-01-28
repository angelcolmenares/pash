namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Runtime.CompilerServices;

    internal sealed class EntityDeserializer : ODataMessageReaderDeserializer
    {
        internal EntityDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription, true)
        {
        }

        private void ApplyEntityProperties(System.Data.Services.SegmentInfo segmentInfo, ODataEntry entry, ODataEntryAnnotation entryAnnotation)
        {
            object entityResource = entryAnnotation.EntityResource;
            ResourceType entityResourceType = entryAnnotation.EntityResourceType;
            this.ApplyValueProperties(entry, entityResourceType, entityResource);
            this.ApplyNavigationProperties(entryAnnotation, segmentInfo.TargetContainer, entityResourceType, entityResource);
            if (base.Update)
            {
                base.Tracker.TrackAction(entityResource, segmentInfo.TargetContainer, UpdateOperations.Change);
            }
            else
            {
                base.Tracker.TrackAction(entityResource, segmentInfo.TargetContainer, UpdateOperations.Add);
            }
        }

        private void ApplyEntityReferenceLinkInNavigationProperty(ResourceProperty navigationProperty, object entityResource, ODataEntityReferenceLink entityReferenceLink)
        {
            if (entityReferenceLink.Url != null)
            {
                string url = CommonUtil.UriToString(entityReferenceLink.Url);
                if ((this.ContentFormat == ContentFormat.Atom) && (url.Length == 0))
                {
                    this.SetResourceReferenceToNull(entityResource, navigationProperty);
                }
                else
                {
                    if (this.ContentFormat == ContentFormat.VerboseJson)
                    {
                        base.RecurseEnter();
                        base.RecurseLeave();
                    }
                    this.SetResourceReferenceToUrl(entityResource, navigationProperty, url);
                }
            }
        }

        private void ApplyEntryInNavigationProperty(ResourceProperty navigationProperty, ResourceSetWrapper targetResourceSet, object entityResource, ODataEntry entry)
        {
            if (((this.ContentFormat != ContentFormat.VerboseJson) || (entry != null)) && base.Update)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_DeepUpdateNotSupported);
            }
            if (entry == null)
            {
                this.SetResourceReferenceToNull(entityResource, navigationProperty);
            }
            else
            {
                System.Data.Services.SegmentInfo segmentInfo = Deserializer.CreateSegment(navigationProperty, navigationProperty.Name, targetResourceSet, true);
                object propertyValue = this.CreateNestedEntityAndApplyProperties(segmentInfo, entry);
                base.Updatable.SetReference(entityResource, navigationProperty.Name, propertyValue);
            }
        }

        private void ApplyFeedInNavigationProperty(ResourceProperty navigationProperty, ResourceSetWrapper targetResourceSet, object entityResource, ODataFeed feed)
        {
            ODataFeedAnnotation annotation = feed.GetAnnotation<ODataFeedAnnotation>();
            if (base.Update && ((this.ContentFormat != ContentFormat.VerboseJson) || (annotation.Count > 0)))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_DeepUpdateNotSupported);
            }
            System.Data.Services.SegmentInfo segmentInfo = Deserializer.CreateSegment(navigationProperty, navigationProperty.Name, targetResourceSet, false);
            foreach (ODataEntry entry in annotation)
            {
                object resourceToBeAdded = this.CreateNestedEntityAndApplyProperties(segmentInfo, entry);
                base.Updatable.AddReferenceToCollection(entityResource, navigationProperty.Name, resourceToBeAdded);
            }
        }

        private void ApplyNavigationProperties(ODataEntryAnnotation entryAnnotation, ResourceSetWrapper entityResourceSet, ResourceType entityResourceType, object entityResource)
        {
            foreach (ODataNavigationLink link in entryAnnotation)
            {
                ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                ResourceProperty navigationProperty = entityResourceType.TryResolvePropertyName(link.Name, stream);
                this.ApplyNavigationProperty(link, entityResourceSet, entityResourceType, navigationProperty, entityResource);
            }
        }

        private void ApplyNavigationProperty(ODataNavigationLink navigationLink, ResourceSetWrapper entityResourceSet, ResourceType entityResourceType, ResourceProperty navigationProperty, object entityResource)
        {
            ResourceSetWrapper targetResourceSet = null;
            targetResourceSet = this.GetNavigationPropertyTargetResourceSet(entityResourceSet, entityResourceType, navigationProperty);
            foreach (ODataItem item in navigationLink.GetAnnotation<ODataNavigationLinkAnnotation>())
            {
                ODataEntityReferenceLink entityReferenceLink = item as ODataEntityReferenceLink;
                if (entityReferenceLink != null)
                {
                    this.ApplyEntityReferenceLinkInNavigationProperty(navigationProperty, entityResource, entityReferenceLink);
                }
                else
                {
                    ODataFeed feed = item as ODataFeed;
                    if (feed != null)
                    {
                        this.ApplyFeedInNavigationProperty(navigationProperty, targetResourceSet, entityResource, feed);
                    }
                    else
                    {
                        ODataEntry entry = (ODataEntry) item;
                        this.ApplyEntryInNavigationProperty(navigationProperty, targetResourceSet, entityResource, entry);
                    }
                }
            }
        }

        private void ApplyValueProperties(ODataEntry entry, ResourceType entityResourceType, object entityResource)
        {
            foreach (ODataProperty property in entry.Properties)
            {
                base.ApplyProperty(property, entityResourceType, entityResource);
            }
        }

        private void CreateEntityResource(System.Data.Services.SegmentInfo segmentInfo, ODataEntry entry, ODataEntryAnnotation entryAnnotation, bool topLevel)
        {
            object obj2;
            base.CheckAndIncrementObjectCount();
            ResourceType entryResourceType = this.GetEntryResourceType(entry, segmentInfo.TargetResourceType);
            base.UpdateAndCheckRequestResponseDSV(entryResourceType, topLevel);
            if (segmentInfo.TargetKind == RequestTargetKind.OpenProperty)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(segmentInfo.Identifier));
            }
            if (base.Update)
            {
                obj2 = base.GetObjectFromSegmentInfo(entryResourceType, segmentInfo, true, true, base.Service.OperationContext.Host.HttpVerb == HttpVerbs.PUT);
            }
            else
            {
                DataServiceConfiguration.CheckResourceRights(segmentInfo.TargetContainer, EntitySetRights.WriteAppend);
                obj2 = base.Updatable.CreateResource(segmentInfo.TargetContainer.Name, entryResourceType.FullName);
            }
            entryAnnotation.EntityResource = obj2;
            entryAnnotation.EntityResourceType = entryResourceType;
        }

        private object CreateNestedEntityAndApplyProperties(System.Data.Services.SegmentInfo segmentInfo, ODataEntry entry)
        {
            ODataEntryAnnotation entryAnnotation = entry.GetAnnotation<ODataEntryAnnotation>();
            base.RecurseEnter();
            this.CreateEntityResource(segmentInfo, entry, entryAnnotation, false);
            this.ApplyEntityProperties(segmentInfo, entry, entryAnnotation);
            base.RecurseLeave();
            return entryAnnotation.EntityResource;
        }

        protected override ContentFormat GetContentFormat()
        {
            ODataFormat readFormat = ODataUtils.GetReadFormat(base.MessageReader);
            if (readFormat == ODataFormat.Atom)
            {
                return ContentFormat.Atom;
            }
            if (readFormat != ODataFormat.VerboseJson)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceException_GeneralError);
            }
            return ContentFormat.VerboseJson;
        }

        private ResourceType GetEntryResourceType(ODataEntry entry, ResourceType expectedType)
        {
            ResourceType type;
            string typeName = entry.TypeName;
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            if (annotation != null)
            {
                typeName = annotation.TypeName;
            }
            if (string.IsNullOrEmpty(typeName))
            {
                type = expectedType;
                if (base.Service.Provider.HasDerivedTypes(type))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_TypeInformationMustBeSpecifiedForInhertiance);
                }
                return type;
            }
            type = base.Service.Provider.TryResolveResourceType(typeName);
            if (type == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidTypeName(typeName));
            }
            if (!expectedType.IsAssignableFrom(type))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidTypeSpecified(typeName, expectedType.FullName));
            }
            return type;
        }

        private ResourceSetWrapper GetNavigationPropertyTargetResourceSet(ResourceSetWrapper parentResourceSet, ResourceType parentResourceType, ResourceProperty navigationProperty)
        {
            ResourceSetWrapper resourceSet = base.Service.Provider.GetContainer(parentResourceSet, parentResourceType, navigationProperty);
            if (resourceSet == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidPropertyNameSpecified(navigationProperty.Name, parentResourceType.FullName));
            }
            base.RequestDescription.UpdateAndCheckEpmFeatureVersion(resourceSet, base.Service);
            return resourceSet;
        }

        protected override object Read(System.Data.Services.SegmentInfo segmentInfo)
        {
            ODataEntry entry;
            ResourceType targetResourceType = segmentInfo.TargetResourceType;
            IEdmEntityType schemaType = (IEdmEntityType) base.GetSchemaType(targetResourceType);
            ODataReader odataReader = base.MessageReader.CreateODataEntryReader(schemaType);
            try
            {
                entry = this.ReadEntry(odataReader, segmentInfo);
            }
            catch (UriFormatException exception)
            {
                if (this.ContentFormat == ContentFormat.Atom)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.Syndication_ErrorReadingEntry(exception.Message), exception);
                }
                throw;
            }
            ODataEntryAnnotation entryAnnotation = entry.GetAnnotation<ODataEntryAnnotation>();
            base.RecurseEnter();
            this.ApplyEntityProperties(segmentInfo, entry, entryAnnotation);
            base.RecurseLeave();
            return entryAnnotation.EntityResource;
        }

        private ODataEntry ReadEntry(ODataReader odataReader, System.Data.Services.SegmentInfo topLevelSegmentInfo)
        {
            ODataEntry entry = null;
            Stack<ODataItem> stack = new Stack<ODataItem>();
            while (odataReader.Read())
            {
                if (stack.Count >= 100)
                {
                    throw DataServiceException.CreateDeepRecursion(100);
                }
                switch (odataReader.State)
                {
                    case ODataReaderState.FeedStart:
                    {
                        ODataFeed feed2 = (ODataFeed) odataReader.Item;
                        feed2.SetAnnotation<ODataFeedAnnotation>(new ODataFeedAnnotation());
                        ODataNavigationLink link3 = (ODataNavigationLink) stack.Peek();
                        link3.GetAnnotation<ODataNavigationLinkAnnotation>().Add(feed2);
                        stack.Push(feed2);
                        break;
                    }
                    case ODataReaderState.FeedEnd:
                        stack.Pop();
                        break;

                    case ODataReaderState.EntryStart:
                    {
                        ODataEntry entry2 = (ODataEntry) odataReader.Item;
                        ODataEntryAnnotation annotation = null;
                        if (entry2 != null)
                        {
                            annotation = new ODataEntryAnnotation();
                            entry2.SetAnnotation<ODataEntryAnnotation>(annotation);
                        }
                        if (stack.Count == 0)
                        {
                            entry = entry2;
                            this.CreateEntityResource(topLevelSegmentInfo, entry2, annotation, true);
                        }
                        else
                        {
                            ODataItem item = stack.Peek();
                            ODataFeed feed = item as ODataFeed;
                            if (feed != null)
                            {
                                feed.GetAnnotation<ODataFeedAnnotation>().Add(entry2);
                            }
                            else
                            {
                                ODataNavigationLink link = (ODataNavigationLink) item;
                                link.GetAnnotation<ODataNavigationLinkAnnotation>().Add(entry2);
                            }
                        }
                        stack.Push(entry2);
                        break;
                    }
                    case ODataReaderState.EntryEnd:
                        stack.Pop();
                        break;

                    case ODataReaderState.NavigationLinkStart:
                    {
                        ODataNavigationLink link2 = (ODataNavigationLink) odataReader.Item;
                        link2.SetAnnotation<ODataNavigationLinkAnnotation>(new ODataNavigationLinkAnnotation());
                        ODataEntry entry3 = (ODataEntry) stack.Peek();
                        entry3.GetAnnotation<ODataEntryAnnotation>().Add(link2);
                        stack.Push(link2);
                        break;
                    }
                    case ODataReaderState.NavigationLinkEnd:
                        stack.Pop();
                        break;

                    case ODataReaderState.EntityReferenceLink:
                    {
                        ODataEntityReferenceLink link4 = (ODataEntityReferenceLink) odataReader.Item;
                        ODataNavigationLink link5 = (ODataNavigationLink) stack.Peek();
                        link5.GetAnnotation<ODataNavigationLinkAnnotation>().Add(link4);
                        break;
                    }
                }
            }
            return entry;
        }

        private void SetResourceReferenceToNull(object entityResource, ResourceProperty navigationProperty)
        {
            base.CheckAndIncrementObjectCount();
            if (navigationProperty.Kind == ResourcePropertyKind.ResourceSetReference)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_CannotSetCollectionsToNull(navigationProperty.Name));
            }
            base.Updatable.SetReference(entityResource, navigationProperty.Name, null);
        }

        private void SetResourceReferenceToUrl(object entityResource, ResourceProperty navigationProperty, string url)
        {
            base.CheckAndIncrementObjectCount();
			RequestDescription description = RequestUriProcessor.ProcessRequestUri(RequestUriProcessor.GetAbsoluteUriFromReference(url, base.Service.OperationContext), base.Service, true);
            if ((this.ContentFormat == ContentFormat.Atom) && !description.IsSingleResult)
            {
                if ((navigationProperty != null) && (navigationProperty.Kind == ResourcePropertyKind.ResourceReference))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_LinkHrefMustReferToSingleResource(navigationProperty.Name));
                }
            }
            else
            {
                object propertyValue = base.Service.GetResource(description, description.SegmentInfos.Length - 1, null);
                if (navigationProperty.Kind == ResourcePropertyKind.ResourceReference)
                {
                    base.Updatable.SetReference(entityResource, navigationProperty.Name, propertyValue);
                }
                else
                {
                    WebUtil.CheckResourceExists(propertyValue != null, description.LastSegmentInfo.Identifier);
                    base.Updatable.AddReferenceToCollection(entityResource, navigationProperty.Name, propertyValue);
                }
            }
        }

        private sealed class ODataEntryAnnotation : List<ODataNavigationLink>
        {
            internal object EntityResource { get; set; }

            internal ResourceType EntityResourceType { get; set; }
        }

        private sealed class ODataFeedAnnotation : List<ODataEntry>
        {
        }

        private sealed class ODataNavigationLinkAnnotation : List<ODataItem>
        {
        }
    }
}

