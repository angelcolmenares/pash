namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Services.Caching;
    using System.Data.Services.Providers;
    using System.Data.Services.Serializers;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall), AspNetCompatibilityRequirements(RequirementsMode=AspNetCompatibilityRequirementsMode.Allowed)]
    internal class DataService<T> : IRequestHandler, IDataService
    {
        private DataServiceActionProviderWrapper actionProvider;
        private static Func<T> cachedConstructor;
        private DataServiceConfiguration configuration;
        private DataServiceExecutionProviderWrapper executionProvider;
        private DataServiceOperationContext operationContext;
        private DataServicePagingProviderWrapper pagingProvider;
        private readonly DataServiceProcessingPipeline processingPipeline;
        private DataServiceProviderWrapper provider;
        private Action<IQueryable> requestQueryableConstructed;
        private DataServiceStreamProviderWrapper streamProvider;
        private UpdatableWrapper updatable;

        public DataService()
        {
            this.processingPipeline = new DataServiceProcessingPipeline();
        }

        public void AttachHost(IDataServiceHost host)
        {
            WebUtil.CheckArgumentNull<IDataServiceHost>(host, "host");
            this.operationContext = new DataServiceOperationContext(host);
        }

        private static void CheckETagValues(DataServiceHostWrapper host, RequestDescription description)
        {
            bool allowStrongEtag = description.TargetKind == RequestTargetKind.MediaResource;
            if (!WebUtil.IsETagValueValid(host.RequestIfMatch, allowStrongEtag))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagValueNotValid(host.RequestIfMatch));
            }
            if (!WebUtil.IsETagValueValid(host.RequestIfNoneMatch, allowStrongEtag))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagValueNotValid(host.RequestIfNoneMatch));
            }
        }

        private static Action<Stream> CompareETagAndWriteResponse(RequestDescription description, IDataService dataService, IODataResponseMessage responseMessage)
        {
            Action<Stream> emptyStreamWriter;
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            IEnumerator o = null;
            try
            {
                ResourceSetWrapper wrapper4;
                if (host.HttpVerb == HttpVerbs.GET)
                {
                    bool writeResponseForGetMethods = true;
                    int indexOfTargetEntityResource = description.GetIndexOfTargetEntityResource();
                    System.Data.Services.SegmentInfo segmentInfo = description.SegmentInfos[indexOfTargetEntityResource];
                    o = DataServiceExecutionProviderWrapper.GetSingleResultFromRequest(segmentInfo);
                    object current = o.Current;
                    string str = null;
                    if (description.LinkUri)
                    {
                        if (current == null)
                        {
                            throw DataServiceException.CreateResourceNotFound(description.LastSegmentInfo.Identifier);
                        }
                    }
                    else if (RequestDescription.IsETagHeaderAllowed(description) && (description.TargetKind != RequestTargetKind.MediaResource))
                    {
                        ResourceSetWrapper targetContainer = segmentInfo.TargetContainer;
                        str = WebUtil.CompareAndGetETag(current, current, targetContainer, dataService, out writeResponseForGetMethods);
                    }
                    if ((current == null) && (description.TargetKind == RequestTargetKind.Resource))
                    {
                        WebUtil.Dispose(o);
                        o = null;
                        host.ResponseStatusCode = 0xcc;
                        return WebUtil.GetEmptyStreamWriter();
                    }
                    if (writeResponseForGetMethods)
                    {
                        return DataService<T>.WriteSingleElementResponse(description, o, indexOfTargetEntityResource, str, dataService, responseMessage);
                    }
                    WebUtil.Dispose(o);
                    o = null;
                    WebUtil.WriteETagValueInResponseHeader(description, str, host);
                    host.ResponseStatusCode = 0x130;
                    return WebUtil.GetEmptyStreamWriter();
                }
                if (((host.HttpVerb == HttpVerbs.PUT) || (host.HttpVerb == HttpVerbs.MERGE)) || (host.HttpVerb == HttpVerbs.PATCH))
                {
                    ResourceSetWrapper wrapper3;
                    string str2;
                    object entity = DataService<T>.GetContainerAndActualEntityInstance(dataService, description, out wrapper3);
                    if (description.TargetKind == RequestTargetKind.MediaResource)
                    {
                        str2 = dataService.StreamProvider.GetStreamETag(entity, RequestDescription.GetStreamProperty(description), dataService.OperationContext);
                    }
                    else
                    {
                        ResourceType resourceType = WebUtil.GetNonPrimitiveResourceType(dataService.Provider, entity);
                        str2 = WebUtil.GetETagValue(dataService, entity, resourceType, wrapper3);
                    }
                    if (description.PreferenceApplied == PreferenceApplied.Content)
                    {
                        o = DataServiceExecutionProviderWrapper.GetSingleResultFromRequest(description.LastSegmentInfo);
                        return DataService<T>.WriteSingleElementResponse(description, o, description.GetIndexOfTargetEntityResource(), str2, dataService, responseMessage);
                    }
                    WebUtil.WriteETagValueInResponseHeader(description, str2, host);
                    return WebUtil.GetEmptyStreamWriter();
                }
                object obj4 = DataService<T>.GetContainerAndActualEntityInstance(dataService, description, out wrapper4);
                ResourceType nonPrimitiveResourceType = WebUtil.GetNonPrimitiveResourceType(dataService.Provider, obj4);
                string absoluteUri = Serializer.GetEditLink(obj4, nonPrimitiveResourceType, dataService.Provider, wrapper4, host.AbsoluteServiceUri).AbsoluteUri;
                host.ResponseLocation = absoluteUri;
                string etagValue = WebUtil.GetETagValue(dataService, obj4, nonPrimitiveResourceType, wrapper4);
                if (description.PreferenceApplied != PreferenceApplied.NoContent)
                {
                    o = DataServiceExecutionProviderWrapper.GetSingleResultFromRequest(description.LastSegmentInfo);
                    return DataService<T>.WriteSingleElementResponse(description, o, description.SegmentInfos.Length - 1, etagValue, dataService, responseMessage);
                }
                host.ResponseHeaders.Add("DataServiceId", absoluteUri);
                WebUtil.WriteETagValueInResponseHeader(description, etagValue, host);
                emptyStreamWriter = WebUtil.GetEmptyStreamWriter();
            }
            catch
            {
                WebUtil.Dispose(o);
                throw;
            }
            return emptyStreamWriter;
        }

        private static DataServiceConfiguration CreateConfiguration(System.Type dataServiceType, IDataServiceMetadataProvider provider)
        {
            DataServiceConfiguration configuration = new DataServiceConfiguration(provider);
            configuration.Initialize(dataServiceType);
            if (!(provider is BaseServiceProvider) && configuration.GetKnownTypes().Any<System.Type>())
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_RegisterKnownTypeNotAllowedForIDSP);
            }
            configuration.Seal();
            return configuration;
        }

        protected virtual T CreateDataSource()
        {
            if (DataService<T>.cachedConstructor == null)
            {
                System.Type type = typeof(T);
                if (type.IsAbstract)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataService_ContextTypeIsAbstract(type, base.GetType()));
                }
                DataService<T>.cachedConstructor = (Func<T>) WebUtil.CreateNewInstanceConstructor(type, null, type);
            }
            return DataService<T>.cachedConstructor();
        }

        private object CreateDataSourceInstance()
        {
            object obj2 = this.CreateDataSource();
            if (obj2 == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_CreateDataSourceNull);
            }
            return obj2;
        }

        private static Message CreateMessage(MessageVersion version, string action, string contentType, Action<Stream> writer, IDataService service)
        {
            DelegateBodyWriter writer2 = new DelegateBodyWriter(writer, service);
            Message message = Message.CreateMessage(version, action, (BodyWriter) writer2);
            message.Properties.Add("WebBodyFormatMessageProperty", new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            HttpResponseMessageProperty property = new HttpResponseMessageProperty();
            property.Headers[HttpResponseHeader.ContentType] = contentType;
            message.Properties.Add(HttpResponseMessageProperty.Name, property);
            return message;
        }

        private void CreateMetadataAndQueryProviders(out IDataServiceMetadataProvider metadataProviderInstance, out IDataServiceQueryProvider queryProviderInstance, out BaseServiceProvider builtInProvider, out object dataSourceInstance)
        {
            queryProviderInstance = null;
            metadataProviderInstance = WebUtil.GetService<IDataServiceMetadataProvider>(this);
            if (metadataProviderInstance != null)
            {
                queryProviderInstance = WebUtil.GetService<IDataServiceQueryProvider>(this);
                if (queryProviderInstance == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataService_IDataServiceQueryProviderNull);
                }
            }
            builtInProvider = null;
            if (metadataProviderInstance != null)
            {
                dataSourceInstance = queryProviderInstance.CurrentDataSource;
                if (dataSourceInstance == null)
                {
                    dataSourceInstance = this.CreateDataSourceInstance();
                    queryProviderInstance.CurrentDataSource = dataSourceInstance;
                }
                System.Type type = typeof(T);
                if (!type.IsAssignableFrom(dataSourceInstance.GetType()))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_DataSourceTypeMustBeAssignableToContextType);
                }
            }
            else
            {
                dataSourceInstance = this.CreateDataSourceInstance();
                metadataProviderInstance = dataSourceInstance as IDataServiceMetadataProvider;
                if (metadataProviderInstance != null)
                {
                    queryProviderInstance = dataSourceInstance as IDataServiceQueryProvider;
                    if (queryProviderInstance == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataService_IDataServiceQueryProviderNull);
                    }
                    queryProviderInstance.CurrentDataSource = dataSourceInstance;
                }
                else
                {
                    System.Type c = dataSourceInstance.GetType();
                    if (typeof(ObjectContext).IsAssignableFrom(c) || DbContextHelper.IsDbContextType(c))
                    {
                        builtInProvider = new ObjectContextServiceProvider(this, dataSourceInstance);
                    }
                    else
                    {
                        builtInProvider = new ReflectionServiceProvider(this, dataSourceInstance);
                    }
                    builtInProvider.LoadMetadata();
                    metadataProviderInstance = builtInProvider;
                    queryProviderInstance = builtInProvider;
                }
            }
        }

        private void CreateProvider()
        {
            IDataServiceMetadataProvider provider;
            IDataServiceQueryProvider provider2;
            BaseServiceProvider provider3;
            object obj2;
            System.Type serviceType = base.GetType();
            this.CreateMetadataAndQueryProviders(out provider, out provider2, out provider3, out obj2);
            DataServiceCacheItem item = MetadataCache<DataServiceCacheItem>.TryLookup(serviceType, obj2);
            bool flag = item == null;
            if (flag)
            {
                item = new DataServiceCacheItem(DataService<T>.CreateConfiguration(serviceType, provider));
            }
            this.streamProvider = new DataServiceStreamProviderWrapper(this);
            if (provider3 == null)
            {
                this.provider = new DataServiceProviderWrapper(new DataServiceCacheItem(item.Configuration), provider, provider2, this);
                if (flag)
                {
                    item = MetadataCache<DataServiceCacheItem>.AddCacheItem(serviceType, obj2, item);
                }
            }
            else
            {
                this.provider = new DataServiceProviderWrapper(item, provider, provider2, this);
                provider3.ApplyConfiguration(item.Configuration);
                provider3.MakeMetadataReadonly();
                if (flag)
                {
                    this.provider.PopulateMetadataCacheItemForBuiltInProvider();
                    DataServiceCacheItem objA = MetadataCache<DataServiceCacheItem>.AddCacheItem(serviceType, obj2, item);
                    if (!object.ReferenceEquals(objA, item))
                    {
                        item = objA;
                        this.provider = new DataServiceProviderWrapper(item, provider, provider2, this);
                    }
                }
            }
            this.configuration = item.Configuration;
            this.configuration.ValidateServerOptions();
        }

        private static ResponseBodyWriter CreateResponseBodyWriter(bool hasMoved, RequestDescription requestDescription, IDataService service, IEnumerator queryResults, IODataResponseMessage responseMessage)
        {
            ODataPayloadKind unsupported = ODataPayloadKind.Unsupported;
            if (requestDescription.TargetKind == RequestTargetKind.MediaResource)
            {
                object current = queryResults.Current;
                ResourceType resourceType = WebUtil.GetResourceType(service.Provider, current);
                if (!RequestDescription.IsNamedStream(requestDescription) && !resourceType.IsMediaLinkEntry)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidUriForMediaResource(service.OperationContext.AbsoluteRequestUri));
                }
                unsupported = ODataPayloadKind.BinaryValue;
                string str = DataService<T>.SelectMediaResourceContentType(current, service.OperationContext.Host.RequestAccept, service, requestDescription);
                if (!string.IsNullOrEmpty(str))
                {
                    responseMessage.SetHeader("Content-Type", str);
                }
            }
            else if ((requestDescription.TargetKind == RequestTargetKind.OpenPropertyValue) || (requestDescription.TargetKind == RequestTargetKind.PrimitiveValue))
            {
                string str2;
                object obj3 = queryResults.Current;
                ResourceType type2 = WebUtil.GetResourceType(service.Provider, obj3);
                if (type2.ResourceTypeKind != ResourceTypeKind.Primitive)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ValuesCanBeReturnedForPrimitiveTypesOnly);
                }
                if (WebUtil.IsBinaryResourceType(type2))
                {
                    unsupported = ODataPayloadKind.BinaryValue;
                    str2 = requestDescription.MimeType ?? "application/octet-stream";
                }
                else
                {
                    unsupported = ODataPayloadKind.Value;
                    str2 = requestDescription.MimeType ?? "text/plain";
                }
                string headerValue = HttpProcessUtility.SelectRequiredMimeType(service.OperationContext.Host.RequestAccept, new string[] { str2 }, str2);
                responseMessage.SetHeader("Content-Type", headerValue);
            }
            if (unsupported == ODataPayloadKind.Unsupported)
            {
                unsupported = DataService<T>.GetPayloadKind(requestDescription);
            }
            return new ResponseBodyWriter(hasMoved, service, queryResults, requestDescription, responseMessage, unsupported);
        }

        private void EnsureProviderAndConfigForRequest()
        {
            if (this.provider == null)
            {
                this.CreateProvider();
            }
        }

        private static object GetContainerAndActualEntityInstance(IDataService service, RequestDescription description, out ResourceSetWrapper container)
        {
            object[] requestEnumerable = (object[]) description.LastSegmentInfo.RequestEnumerable;
            requestEnumerable[0] = service.Updatable.ResolveResource(requestEnumerable[0]);
            container = description.SegmentInfos[description.GetIndexOfTargetEntityResource()].TargetContainer;
            if (container == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(description.LastSegmentInfo.Identifier));
            }
            return requestEnumerable[0];
        }

        private static ODataPayloadKind GetPayloadKind(RequestDescription description)
        {
            if (description.LinkUri)
            {
                if (!description.IsSingleResult)
                {
                    return ODataPayloadKind.EntityReferenceLinks;
                }
                return ODataPayloadKind.EntityReferenceLink;
            }
            if (description.TargetKind == RequestTargetKind.Resource)
            {
                if (description.TargetResourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
                {
                    return ODataPayloadKind.Feed;
                }
                if (!description.IsSingleResult)
                {
                    return ODataPayloadKind.Feed;
                }
                return ODataPayloadKind.Entry;
            }
            if ((description.TargetKind == RequestTargetKind.OpenPropertyValue) || (description.TargetKind == RequestTargetKind.PrimitiveValue))
            {
                return ODataPayloadKind.Value;
            }
            if (description.TargetSource == RequestTargetSource.Property)
            {
                return ODataPayloadKind.Property;
            }
            if (((description.TargetKind == RequestTargetKind.Primitive) || (description.TargetKind == RequestTargetKind.ComplexObject)) || (description.TargetKind == RequestTargetKind.Collection))
            {
                if (description.IsSingleResult)
                {
                    return ODataPayloadKind.Property;
                }
                return ODataPayloadKind.Collection;
            }
            if (description.TargetKind == RequestTargetKind.ServiceDirectory)
            {
                return ODataPayloadKind.ServiceDocument;
            }
            if (description.TargetKind == RequestTargetKind.Metadata)
            {
                return ODataPayloadKind.MetadataDocument;
            }
            return ODataPayloadKind.Unsupported;
        }

        private Action<Stream> HandleBatchRequest()
        {
            ODataMessageReader reader;
            ODataBatchReader reader2;
            DataServiceHostWrapper host = this.operationContext.Host;
            if (host.HttpVerb != HttpVerbs.POST)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.DataService_BatchResourceOnlySupportsPost, "POST");
            }
            try
            {
                System.Data.Services.ODataRequestMessage requestMessage = new System.Data.Services.ODataRequestMessage(host);
                reader = new ODataMessageReader(requestMessage, WebUtil.CreateMessageReaderSettings(this, true));
                reader2 = reader.CreateODataBatchReader();
            }
            catch (ODataException exception)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceException_GeneralError, exception);
            }
            host.ResponseStatusCode = 0xca;
            host.ResponseCacheControl = "no-cache";
            System.Data.Services.ODataResponseMessage responseMessage = new System.Data.Services.ODataResponseMessage(host);
            ODataMessageWriter messageWriter = ResponseBodyWriter.CreateMessageWriter(this.operationContext.AbsoluteServiceUri, this, this.operationContext.Host.RequestMinVersion, responseMessage, "*/*", null);
            ODataUtils.SetHeadersForPayload(messageWriter, ODataPayloadKind.Batch);
            BatchDataService service = new BatchDataService(this, reader, reader2, responseMessage, messageWriter);
            return new Action<Stream>(service.HandleBatchContent);
        }

        private static void HandleDeleteOperation(RequestDescription description, IDataService dataService)
        {
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            if (description.IsSingleResult && description.LinkUri)
            {
                DataService<T>.HandleUnbindOperation(description, dataService);
            }
            else if (description.IsSingleResult && (description.TargetKind == RequestTargetKind.Resource))
            {
                if (description.RequestExpression == null)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ResourceCanBeCrossReferencedOnlyForBindOperation);
                }
                if (!string.IsNullOrEmpty(host.RequestIfNoneMatch))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_IfNoneMatchHeaderNotSupportedInDelete);
                }
                object resourceCookie = Deserializer.GetResource(description.LastSegmentInfo, null, dataService, true);
                ResourceSetWrapper targetContainer = description.LastSegmentInfo.TargetContainer;
                dataService.Updatable.SetETagValues(resourceCookie, targetContainer);
                object instance = dataService.Updatable.ResolveResource(resourceCookie);
                ResourceType resourceType = dataService.Provider.GetResourceType(instance);
                if (description.Property != null)
                {
                    DataServiceConfiguration.CheckResourceRights(targetContainer, EntitySetRights.WriteDelete);
                }
                dataService.Updatable.DeleteResource(resourceCookie);
                if ((resourceType != null) && (resourceType.IsMediaLinkEntry || resourceType.HasNamedStreams))
                {
                    dataService.StreamProvider.DeleteStream(instance, dataService.OperationContext);
                }
                UpdateTracker.FireNotification(dataService, instance, targetContainer, UpdateOperations.Delete);
            }
            else if (description.TargetKind == RequestTargetKind.PrimitiveValue)
            {
                object obj4;
                ResourceSetWrapper wrapper3;
                if (!string.IsNullOrEmpty(host.RequestIfNoneMatch))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_IfNoneMatchHeaderNotSupportedInDelete);
                }
                if ((description.Property != null) && description.Property.IsOfKind(ResourcePropertyKind.Key))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_CannotUpdateKeyProperties(description.Property.Name));
                }
                if (!WebUtil.IsNullableType(description.Property.Type) && description.Property.Type.IsValueType)
                {
                    throw new DataServiceException(0x193, System.Data.Services.Strings.BadRequest_CannotNullifyValueTypeProperty);
                }
                object resourceToBeModified = Deserializer.GetResourceToModify(description, dataService, false, out obj4, out wrapper3, true);
                object target = dataService.Updatable.ResolveResource(obj4);
                Deserializer.ModifyResource(description, resourceToBeModified, null, dataService);
                UpdateTracker.FireNotification(dataService, target, wrapper3, UpdateOperations.Change);
            }
            else
            {
                object obj7;
                ResourceSetWrapper wrapper4;
                if (description.TargetKind == RequestTargetKind.OpenProperty)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(description.LastSegmentInfo.Identifier));
                }
                if (description.TargetKind != RequestTargetKind.OpenPropertyValue)
                {
                    throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.BadRequest_InvalidUriForDeleteOperation(host.AbsoluteRequestUri), DataServiceConfiguration.GetAllowedMethods(dataService.Configuration, description));
                }
                if (!string.IsNullOrEmpty(host.RequestIfNoneMatch))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_IfNoneMatchHeaderNotSupportedInDelete);
                }
                object obj8 = Deserializer.GetResourceToModify(description, dataService, false, out obj7, out wrapper4, true);
                object obj9 = dataService.Updatable.ResolveResource(obj7);
                Deserializer.ModifyResource(description, obj8, null, dataService);
                UpdateTracker.FireNotification(dataService, obj9, wrapper4, UpdateOperations.Change);
            }
        }

        protected virtual void HandleException(HandleExceptionArgs args)
        {
            WebUtil.CheckArgumentNull<HandleExceptionArgs>(args, "arg");
        }

        private static Action<Stream> HandleInternalResources(RequestDescription description, IDataService dataService, IODataResponseMessage responseMessage)
        {
            if ((description.TargetKind != RequestTargetKind.Metadata) && (description.TargetKind != RequestTargetKind.ServiceDirectory))
            {
                return null;
            }
            return new Action<Stream>(new ResponseBodyWriter(false, dataService, null, description, responseMessage, DataService<T>.GetPayloadKind(description)).Write);
        }

        private Action<Stream> HandleNonBatchRequest(RequestDescription description)
        {
            bool flag = description.TargetSource == RequestTargetSource.ServiceOperation;
            bool flag2 = DataServiceActionProviderWrapper.IsServiceActionRequest(description);
            description = DataService<T>.ProcessIncomingRequest(description, this);
            if (this.operationContext.Host.HttpVerb != HttpVerbs.GET)
            {
                if (flag)
                {
                    if (this.provider.IsV1ProviderAndImplementsUpdatable() || flag2)
                    {
                        ((IDataService) this).Updatable.SaveChanges();
                    }
                }
                else
                {
                    ((IDataService) this).Updatable.SaveChanges();
                }
                this.processingPipeline.InvokeProcessedChangeset(this, new EventArgs());
            }
            if (!description.ShouldWriteResponseBody)
            {
                return WebUtil.GetEmptyStreamWriter();
            }
            System.Data.Services.ODataResponseMessage responseMessage = new System.Data.Services.ODataResponseMessage(this.operationContext.Host);
            return DataService<T>.SerializeResponseBody(description, this, responseMessage);
        }

        private static RequestDescription HandlePostOperation(RequestDescription description, IDataService dataService)
        {
            object obj2;
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            if (!string.IsNullOrEmpty(host.RequestIfMatch) || !string.IsNullOrEmpty(host.RequestIfNoneMatch))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagSpecifiedForPost);
            }
            if (description.IsSingleResult)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.BadRequest_InvalidUriForPostOperation(host.AbsoluteRequestUri), DataServiceConfiguration.GetAllowedMethods(dataService.Configuration, description));
            }
            Stream requestStream = host.RequestStream;
            ResourceType targetResourceType = description.TargetResourceType;
            if ((!description.LinkUri && dataService.Provider.HasDerivedTypes(targetResourceType)) && WebUtil.HasMediaLinkEntryInHierarchy(targetResourceType, dataService.Provider))
            {
                ResourceSetWrapper targetContainer = description.LastSegmentInfo.TargetContainer;
                targetResourceType = dataService.StreamProvider.ResolveType(targetContainer.Name, dataService);
            }
            UpdateTracker tracker = UpdateTracker.CreateUpdateTracker(dataService);
            if (!description.LinkUri && targetResourceType.IsMediaLinkEntry)
            {
                DataServiceConfiguration.CheckResourceRights(description.LastSegmentInfo.TargetContainer, EntitySetRights.WriteAppend);
                description.UpdateResponseVersionForPostMR(targetResourceType, dataService);
                obj2 = Deserializer.CreateMediaLinkEntry(targetResourceType.FullName, requestStream, dataService, description, tracker);
                if (description.TargetSource == RequestTargetSource.Property)
                {
                    Deserializer.HandleBindOperation(description, obj2, dataService, tracker);
                }
            }
            else
            {
                using (Deserializer deserializer = Deserializer.CreateDeserializer(description, dataService, false, tracker))
                {
                    obj2 = deserializer.HandlePostRequest();
                }
            }
            tracker.FireNotifications();
            return RequestDescription.CreateSingleResultRequestDescription(description, obj2);
        }

        private static RequestDescription HandlePutOperation(RequestDescription description, IDataService dataService)
        {
            object obj2;
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            if (!description.IsSingleResult)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.BadRequest_InvalidUriForPutOperation(host.AbsoluteRequestUri), DataServiceConfiguration.GetAllowedMethods(dataService.Configuration, description));
            }
            if (description.LinkUri && (description.Property.Kind != ResourcePropertyKind.ResourceReference))
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.DataService_CannotUpdateSetReferenceLinks, "DELETE");
            }
            if (!string.IsNullOrEmpty(host.RequestIfNoneMatch) && (description.TargetKind != RequestTargetKind.MediaResource))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_IfNoneMatchHeaderNotSupportedInPut);
            }
            if (!RequestDescription.IsETagHeaderAllowed(description) && !string.IsNullOrEmpty(host.RequestIfMatch))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagCannotBeSpecified(host.AbsoluteRequestUri));
            }
            if ((description.Property != null) && description.Property.IsOfKind(ResourcePropertyKind.Key))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_CannotUpdateKeyProperties(description.Property.Name));
            }
            UpdateTracker tracker = UpdateTracker.CreateUpdateTracker(dataService);
            using (Deserializer deserializer = Deserializer.CreateDeserializer(description, dataService, true, tracker))
            {
                obj2 = deserializer.HandlePutRequest();
            }
            tracker.FireNotifications();
            return RequestDescription.CreateSingleResultRequestDescription(description, obj2);
        }

        private Action<Stream> HandleRequest()
        {
            this.operationContext.InitializeAndCacheHeaders(this);
            try
            {
                this.EnsureProviderAndConfigForRequest();
            }
            catch (Exception exception)
            {
                int statusCode = 500;
                if (!CommonUtil.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                DataServiceException exception2 = exception as DataServiceException;
                if (exception2 != null)
                {
                    statusCode = exception2.StatusCode;
                }
                DataServiceHostWrapper host = this.operationContext.Host;
                host.ResponseStatusCode = statusCode;
                host.ResponseVersion = "1.0;";
                throw;
            }
            try
            {
                RequestDescription description = this.ProcessIncomingRequestUri();
                if (description.TargetKind != RequestTargetKind.Batch)
                {
                    Action<Stream> action = this.HandleNonBatchRequest(description);
                    DataServiceProcessingPipelineEventArgs e = new DataServiceProcessingPipelineEventArgs(this.operationContext);
                    this.processingPipeline.InvokeProcessedRequest(this, e);
                    return action;
                }
                return this.HandleBatchRequest();
            }
            catch (Exception exception3)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception3))
                {
                    throw;
                }
                return ErrorHandler.HandleBeforeWritingException(exception3, this);
            }
        }

        private static void HandleServiceAction(RequestDescription description, IDataService dataService)
        {
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            if (!string.IsNullOrEmpty(host.RequestIfMatch) || !string.IsNullOrEmpty(host.RequestIfNoneMatch))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagSpecifiedForServiceAction);
            }
            IDataServiceInvokable invokable = DataServiceActionProviderWrapper.CreateInvokableFromSegment(description.LastSegmentInfo);
            dataService.Updatable.ScheduleInvokable(invokable);
        }

        private static void HandleUnbindOperation(RequestDescription description, IDataService dataService)
        {
            ResourceSetWrapper wrapper;
            if (!string.IsNullOrEmpty(dataService.OperationContext.Host.RequestIfMatch) || !string.IsNullOrEmpty(dataService.OperationContext.Host.RequestIfNoneMatch))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagNotSupportedInUnbind);
            }
            object targetResource = Deserializer.GetEntityResourceToModify(description, dataService, false, out wrapper);
            if (description.Property.Kind == ResourcePropertyKind.ResourceReference)
            {
                dataService.Updatable.SetReference(targetResource, description.Property.Name, null);
            }
            else
            {
                object resourceToBeRemoved = Deserializer.GetResource(description.LastSegmentInfo, null, dataService, true);
                dataService.Updatable.RemoveReferenceFromCollection(targetResource, description.Property.Name, resourceToBeRemoved);
            }
            if (dataService.Configuration.DataServiceBehavior.InvokeInterceptorsOnLinkDelete)
            {
                object target = dataService.Updatable.ResolveResource(targetResource);
                UpdateTracker.FireNotification(dataService, target, wrapper, UpdateOperations.Change);
            }
        }

        protected virtual void OnStartProcessingRequest(ProcessRequestArgs args)
        {
        }

        private static RequestDescription ProcessIncomingRequest(RequestDescription description, IDataService dataService)
        {
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            if (description.TargetKind == RequestTargetKind.Metadata)
            {
                description.VerifyAndRaiseResponseVersion(dataService.Provider.GetMetadataVersion(dataService.OperationContext), dataService);
            }
            DataService<T>.CheckETagValues(host, description);
            ResourceSetWrapper targetContainer = description.LastSegmentInfo.TargetContainer;
            if (host.HttpVerb == HttpVerbs.GET)
            {
                if (((description.LastSegmentInfo.Operation == null) && (targetContainer != null)) && (description.LastSegmentInfo.Identifier != "$count"))
                {
                    DataServiceConfiguration.CheckResourceRightsForRead(targetContainer, description.IsSingleResult);
                }
            }
            else if (description.TargetKind == RequestTargetKind.ServiceDirectory)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.DataService_OnlyGetOperationSupportedOnServiceUrl, "GET");
            }
            int statusCode = 200;
            bool flag = true;
            RequestDescription description2 = description;
            if (description.TargetSource != RequestTargetSource.ServiceOperation)
            {
                if (host.HttpVerb == HttpVerbs.POST)
                {
                    flag = !description.LinkUri;
                    description2 = DataService<T>.HandlePostOperation(description, dataService);
                    if (flag && (description2.PreferenceApplied != PreferenceApplied.NoContent))
                    {
                        statusCode = 0xc9;
                    }
                    else
                    {
                        statusCode = 0xcc;
                    }
                }
                else if (((host.HttpVerb == HttpVerbs.PUT) || (host.HttpVerb == HttpVerbs.MERGE)) || (host.HttpVerb == HttpVerbs.PATCH))
                {
                    if ((targetContainer != null) && !description.LinkUri)
                    {
                        if (host.HttpVerb == HttpVerbs.PUT)
                        {
                            DataServiceConfiguration.CheckResourceRights(targetContainer, EntitySetRights.WriteReplace);
                        }
                        else
                        {
                            DataServiceConfiguration.CheckResourceRights(targetContainer, EntitySetRights.WriteMerge);
                        }
                    }
                    flag = !description.LinkUri;
                    description2 = DataService<T>.HandlePutOperation(description, dataService);
                    if (description2.PreferenceApplied == PreferenceApplied.Content)
                    {
                        statusCode = 200;
                    }
                    else
                    {
                        statusCode = 0xcc;
                    }
                }
                else if (host.HttpVerb == HttpVerbs.DELETE)
                {
                    if ((targetContainer != null) && !description.LinkUri)
                    {
                        DataServiceConfiguration.CheckResourceRights(targetContainer, EntitySetRights.WriteDelete);
                    }
                    DataService<T>.HandleDeleteOperation(description, dataService);
                    statusCode = 0xcc;
                    flag = false;
                }
            }
            else
            {
                if (DataServiceActionProviderWrapper.IsServiceActionRequest(description))
                {
                    DataService<T>.HandleServiceAction(description, dataService);
                }
                if (description.TargetKind == RequestTargetKind.VoidOperation)
                {
                    statusCode = 0xcc;
                    flag = false;
                }
            }
            DataService<T>.SetResponseHeaders(description2, host, statusCode);
            description2.ShouldWriteResponseBody = flag;
            return description2;
        }

        private RequestDescription ProcessIncomingRequestUri()
        {
            DataServiceHostWrapper host = this.operationContext.Host;
            host.VerifyQueryParameters();
            DataService<T>.ValidateRequest(this.operationContext);
            DataServiceProcessingPipelineEventArgs e = new DataServiceProcessingPipelineEventArgs(this.operationContext);
            this.processingPipeline.InvokeProcessingRequest(this, e);
            ((IDataService) this).InternalOnStartProcessingRequest(new ProcessRequestArgs(host.AbsoluteRequestUri, false, this.operationContext));
            if ((host.HttpVerb != HttpVerbs.GET) && !this.operationContext.IsBatchRequest)
            {
                this.processingPipeline.InvokeProcessingChangeset(this, new EventArgs());
            }
            DataService<T>.VerifyAndInitializeRequest(this);
            this.updatable = new UpdatableWrapper(this);
            return RequestUriProcessor.ProcessRequestUri(host.AbsoluteRequestUri, this, false);
        }

        public void ProcessRequest()
        {
            if (this.operationContext == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_HostNotAttached);
            }
            try
            {
                Action<Stream> action = this.HandleRequest();
                if (action != null)
                {
                    action(this.operationContext.Host.ResponseStream);
                }
            }
            finally
            {
                ((IDataService) this).DisposeDataSource();
            }
        }

        public Message ProcessRequestForMessage(Stream messageBody)
        {
            Message message2;
            WebUtil.CheckArgumentNull<Stream>(messageBody, "messageBody");
            if (WebOperationContext.Current == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.HttpContextServiceHost_WebOperationContextCurrentMissing);
            }
            if (this.operationContext == null)
            {
                HttpContextServiceHost host = new HttpContextServiceHost(messageBody);
                this.AttachHost(host);
            }
            bool flag = true;
            try
            {
                Action<Stream> writer = this.HandleRequest();
                Message message = DataService<T>.CreateMessage(MessageVersion.None, string.Empty, this.operationContext.Host.ResponseContentType, writer, this);
                if (!WebOperationContext.Current.OutgoingResponse.SuppressEntityBody)
                {
                    flag = false;
                }
                message2 = message;
            }
            finally
            {
                if (flag)
                {
                    ((IDataService) this).DisposeDataSource();
                }
            }
            return message2;
        }

        private static string SelectMediaResourceContentType(object mediaLinkEntry, string acceptTypesText, IDataService service, RequestDescription description)
        {
            ResourceProperty streamProperty = RequestDescription.GetStreamProperty(description);
            string str = service.StreamProvider.GetStreamContentType(mediaLinkEntry, streamProperty, service.OperationContext);
            string str2 = null;
            if (!string.IsNullOrEmpty(str))
            {
                str2 = HttpProcessUtility.SelectRequiredMimeType(acceptTypesText, new string[] { str }, str);
            }
            return str2;
        }

        private static Action<Stream> SerializeResponseBody(RequestDescription description, IDataService dataService, IODataResponseMessage responseMessage)
        {
            DataServiceHostWrapper host = dataService.OperationContext.Host;
            Action<Stream> action = DataService<T>.HandleInternalResources(description, dataService, responseMessage);
            if (action != null)
            {
                return action;
            }
            if (!RequestDescription.IsETagHeaderAllowed(description) && (!string.IsNullOrEmpty(host.RequestIfMatch) || !string.IsNullOrEmpty(host.RequestIfNoneMatch)))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ETagCannotBeSpecified(host.AbsoluteRequestUri));
            }
            if (((description.TargetSource == RequestTargetSource.ServiceOperation) || (description.TargetSource == RequestTargetSource.None)) || !description.IsSingleResult)
            {
                System.Data.Services.SegmentInfo lastSegmentInfo = description.LastSegmentInfo;
                if (DataServiceActionProviderWrapper.IsServiceActionSegment(lastSegmentInfo))
                {
                    DataServiceActionProviderWrapper.ResolveActionResult(lastSegmentInfo);
                    if (lastSegmentInfo.RequestEnumerable == null)
                    {
                        host.ResponseStatusCode = 0xcc;
                        return WebUtil.GetEmptyStreamWriter();
                    }
                }
                IEnumerator requestEnumerator = WebUtil.GetRequestEnumerator(lastSegmentInfo.RequestEnumerable);
                try
                {
                    bool hasMoved = requestEnumerator.MoveNext();
                    if (description.IsSingleResult)
                    {
                        if (!hasMoved || (requestEnumerator.Current == null))
                        {
                            throw DataServiceException.CreateResourceNotFound(lastSegmentInfo.Identifier);
                        }
                        if (((description.TargetSource == RequestTargetSource.ServiceOperation) && (description.TargetKind == RequestTargetKind.Resource)) && RequestDescription.IsETagHeaderAllowed(description))
                        {
                            object current = requestEnumerator.Current;
                            ResourceType nonPrimitiveResourceType = WebUtil.GetNonPrimitiveResourceType(dataService.Provider, current);
                            string etagValue = WebUtil.GetETagValue(dataService, current, nonPrimitiveResourceType, lastSegmentInfo.TargetContainer);
                            WebUtil.WriteETagValueInResponseHeader(description, etagValue, host);
                        }
                    }
                    return new Action<Stream>(DataService<T>.CreateResponseBodyWriter(hasMoved, description, dataService, requestEnumerator, responseMessage).Write);
                }
                catch
                {
                    WebUtil.Dispose(requestEnumerator);
                    throw;
                }
            }
            return DataService<T>.CompareETagAndWriteResponse(description, dataService, responseMessage);
        }

        private static void SetResponseHeaders(RequestDescription description, DataServiceHostWrapper host, int statusCode)
        {
            host.ResponseCacheControl = "no-cache";
            if (description.PreferenceApplied != PreferenceApplied.None)
            {
                host.ResponseHeaders.Add("Preference-Applied", (description.PreferenceApplied == PreferenceApplied.Content) ? "return-content" : "return-no-content");
            }
            host.ResponseVersion = description.ResponseVersion.ToString() + ";";
            host.ResponseStatusCode = statusCode;
        }

        void IDataService.DisposeDataSource()
        {
            if (this.updatable != null)
            {
                this.updatable.DisposeProvider();
                this.updatable = null;
            }
            if (this.streamProvider != null)
            {
                this.streamProvider.DisposeProvider();
                this.streamProvider = null;
            }
            if (this.pagingProvider != null)
            {
                this.pagingProvider.DisposeProvider();
                this.pagingProvider = null;
            }
            if (this.provider != null)
            {
                this.provider.DisposeDataSource();
                this.provider = null;	
            }
        }

        object IDataService.GetResource(RequestDescription description, int segmentIndex, string typeFullName)
        {
            return Deserializer.GetResource(description.SegmentInfos[segmentIndex], typeFullName, this, false);
        }

        System.Data.Services.SegmentInfo IDataService.GetSegmentForContentId(string contentId)
        {
            return null;
        }

        void IDataService.InternalApplyingExpansions(Expression queryExpression, ICollection<ExpandSegmentCollection> expandPaths)
        {
            int num = 0;
            int num2 = 0;
            foreach (ExpandSegmentCollection segments in expandPaths)
            {
                int count = segments.Count;
                if (count > num)
                {
                    num = count;
                }
                num2 += count;
            }
            if (this.configuration.MaxExpandDepth < num)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ExpandDepthExceeded(num, this.configuration.MaxExpandDepth));
            }
            if (this.configuration.MaxExpandCount < num2)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ExpandCountExceeded(num2, this.configuration.MaxExpandCount));
            }
        }

        void IDataService.InternalHandleException(HandleExceptionArgs args)
        {
            try
            {
                this.HandleException(args);
            }
            catch (Exception exception)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                args.Exception = exception;
            }
        }

        void IDataService.InternalOnRequestQueryConstructed(IQueryable query)
        {
            if (this.requestQueryableConstructed != null)
            {
                this.requestQueryableConstructed(query);
            }
        }

        void IDataService.InternalOnStartProcessingRequest(ProcessRequestArgs args)
        {
            this.OnStartProcessingRequest(args);
        }

        private static void ValidateRequest(DataServiceOperationContext operationContext)
        {
            if (!string.IsNullOrEmpty(operationContext.Host.RequestIfMatch) && !string.IsNullOrEmpty(operationContext.Host.RequestIfNoneMatch))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_BothIfMatchAndIfNoneMatchHeaderSpecified);
            }
        }

        private static void ValidateSingleResultValue(object singleResult, System.Data.Services.SegmentInfo segmentInfo, DataServiceProviderWrapper provider)
        {
            RequestDescription.CheckNullDirectReference(singleResult, segmentInfo);
            if ((segmentInfo.TargetKind == RequestTargetKind.OpenProperty) && (singleResult != null))
            {
                WebUtil.CheckResourceNotCollectionForOpenProperty(WebUtil.GetResourceType(provider, singleResult), segmentInfo.Identifier);
            }
        }

        private static void VerifyAndInitializeRequest(IDataService service)
        {
            service.OperationContext.Host.InitializeRequestVersionHeaders(service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion());
        }

        private static Action<Stream> WriteSingleElementResponse(RequestDescription description, IEnumerator queryResults, int parentResourceIndex, string etagValue, IDataService dataService, IODataResponseMessage responseMessage)
        {
            Action<Stream> action;
            try
            {
                if (description.SegmentInfos[parentResourceIndex].RequestExpression != description.RequestExpression)
                {
                    object current = queryResults.Current;
                    for (int i = parentResourceIndex + 1; i < description.SegmentInfos.Length; i++)
                    {
                        Func<ResourceProperty, bool> predicate = null;
                        System.Data.Services.SegmentInfo info = description.SegmentInfos[i - 1];
                        System.Data.Services.SegmentInfo currentSegment = description.SegmentInfos[i];
                        WebUtil.CheckResourceExists(current != null, info.Identifier);
                        if ((currentSegment.TargetKind == RequestTargetKind.PrimitiveValue) || (currentSegment.TargetKind == RequestTargetKind.OpenPropertyValue))
                        {
                            break;
                        }
                        if (currentSegment.TargetKind == RequestTargetKind.OpenProperty)
                        {
                            ResourceType resourceType = WebUtil.GetResourceType(dataService.Provider, current);
                            if (resourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                            {
                                if (predicate == null)
                                {
                                    predicate = p => p.Name == currentSegment.Identifier;
                                }
                                ResourceProperty resourceProperty = resourceType.Properties.First<ResourceProperty>(predicate);
                                current = WebUtil.GetPropertyValue(dataService.Provider, current, resourceType, resourceProperty, null);
                            }
                            else
                            {
                                current = WebUtil.GetPropertyValue(dataService.Provider, current, resourceType, null, currentSegment.Identifier);
                            }
                        }
                        else
                        {
                            current = WebUtil.GetPropertyValue(dataService.Provider, current, info.TargetResourceType, currentSegment.ProjectedProperty, null);
                        }
                    }
                    DataService<T>.ValidateSingleResultValue(current, description.LastSegmentInfo, dataService.Provider);
                    queryResults = new QueryResultsWrapper(new object[] { current }.GetEnumerator(), queryResults);
                    queryResults.MoveNext();
                }
                DataServiceHostWrapper host = dataService.OperationContext.Host;
                WebUtil.WriteETagValueInResponseHeader(description, etagValue, host);
                action = new Action<Stream>(DataService<T>.CreateResponseBodyWriter(true, description, dataService, queryResults, responseMessage).Write);
            }
            catch
            {
                WebUtil.Dispose(queryResults);
                throw;
            }
            return action;
        }

        protected T CurrentDataSource
        {
            get
            {
                return (T) this.provider.CurrentDataSource;
            }
        }

        public DataServiceProcessingPipeline ProcessingPipeline
        {
            [DebuggerStepThrough]
            get
            {
                return this.processingPipeline;
            }
        }

        DataServiceActionProviderWrapper IDataService.ActionProvider
        {
            [DebuggerStepThrough]
            get
            {
                return (this.actionProvider ?? (this.actionProvider = new DataServiceActionProviderWrapper(this)));
            }
        }

        DataServiceConfiguration IDataService.Configuration
        {
            [DebuggerStepThrough]
            get
            {
                return this.configuration;
            }
        }

        DataServiceExecutionProviderWrapper IDataService.ExecutionProvider
        {
            [DebuggerStepThrough]
            get
            {
                return (this.executionProvider ?? (this.executionProvider = new DataServiceExecutionProviderWrapper(this)));
            }
        }

        object IDataService.Instance
        {
            [DebuggerStepThrough]
            get
            {
                return this;
            }
        }

        DataServiceOperationContext IDataService.OperationContext
        {
            [DebuggerStepThrough]
            get
            {
                return this.operationContext;
            }
        }

        DataServicePagingProviderWrapper IDataService.PagingProvider
        {
            [DebuggerStepThrough]
            get
            {
                return (this.pagingProvider ?? (this.pagingProvider = new DataServicePagingProviderWrapper(this)));
            }
        }

        DataServiceProcessingPipeline IDataService.ProcessingPipeline
        {
            [DebuggerStepThrough]
            get
            {
                return this.processingPipeline;
            }
        }

        DataServiceProviderWrapper IDataService.Provider
        {
            [DebuggerStepThrough]
            get
            {
                return this.provider;
            }
        }

        DataServiceStreamProviderWrapper IDataService.StreamProvider
        {
            [DebuggerStepThrough]
            get
            {
                return this.streamProvider;
            }
        }

        UpdatableWrapper IDataService.Updatable
        {
            [DebuggerStepThrough]
            get
            {
                return this.updatable;
            }
        }

        private class BatchDataService : IDataService
        {
            private DataServiceActionProviderWrapper actionProvider;
            private int batchElementCount;
            private bool batchLimitExceeded;
            private readonly List<DataServiceOperationContext> batchOperationContexts;
            private readonly ODataBatchReader batchReader;
            private readonly List<RequestDescription> batchRequestDescription;
            private readonly System.Data.Services.ODataResponseMessage batchResponseMessage;
            private int changeSetElementCount;
            private readonly HashSet<int> contentIds;
            private readonly Dictionary<string, System.Data.Services.SegmentInfo> contentIdsToSegmentInfoMapping;
            private readonly IDataService dataService;
            private DataServiceExecutionProviderWrapper executionProvider;
            private readonly ODataMessageReader messageReader;
            private readonly ODataMessageWriter messageWriter;
            private DataServiceOperationContext operationContext;
            private DataServicePagingProviderWrapper pagingProvider;
            private DataServiceStreamProviderWrapper streamProvider;
            private UpdatableWrapper updatable;

            internal BatchDataService(IDataService dataService, ODataMessageReader messageReader, ODataBatchReader batchReader, System.Data.Services.ODataResponseMessage batchResponseMessage, ODataMessageWriter messageWriter)
            {
                this.contentIds = new HashSet<int>(new Int32EqualityComparer());
                this.contentIdsToSegmentInfoMapping = new Dictionary<string, System.Data.Services.SegmentInfo>(StringComparer.Ordinal);
                this.batchRequestDescription = new List<RequestDescription>();
                this.batchOperationContexts = new List<DataServiceOperationContext>();
                this.dataService = dataService;
                this.messageReader = messageReader;
                this.batchReader = batchReader;
                this.batchResponseMessage = batchResponseMessage;
                this.messageWriter = messageWriter;
            }

            private static BatchServiceHost CreateBatchServiceHostFromOperationMessage(IDataService dataService, IODataRequestMessage operationMessage, HashSet<int> contentIds, ODataBatchWriter writer)
            {
                Uri absoluteServiceUri = dataService.OperationContext.AbsoluteServiceUri;
                Version requestMaxVersion = dataService.OperationContext.Host.RequestMaxVersion;
                Version requestMinVersion = dataService.OperationContext.Host.RequestMinVersion;
                Version requestVersion = dataService.OperationContext.Host.RequestVersion;
                string header = operationMessage.GetHeader("Content-ID");
                if (!string.IsNullOrEmpty(header))
                {
                    int num;
                    if (!int.TryParse(header, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ContentIdMustBeAnInteger(header));
                    }
                    if (!contentIds.Add(num))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_ContentIdMustBeUniqueInBatch(num));
                    }
                }
                return new BatchServiceHost(absoluteServiceUri, operationMessage, header, writer, requestMaxVersion, requestMinVersion, requestVersion);
            }

            private static DataServiceOperationContext CreateOperationContextFromBatchServiceHost(IDataService dataService, BatchServiceHost operationHost)
            {
                DataServiceOperationContext context = new DataServiceOperationContext(true, operationHost);
                context.InitializeAndCacheHeaders(dataService);
                return context;
            }

            public void DisposeDataSource()
            {
                if (this.updatable != null)
                {
                    this.updatable.DisposeProvider();
                    this.updatable = null;
                }
                if (this.pagingProvider != null)
                {
                    this.pagingProvider.DisposeProvider();
                    this.pagingProvider = null;
                }
                if (this.streamProvider != null)
                {
                    this.streamProvider.DisposeProvider();
                    this.streamProvider = null;
                }
                this.dataService.DisposeDataSource();
            }

            public object GetResource(RequestDescription description, int segmentIndex, string typeFullName)
            {
                if (!Deserializer.IsCrossReferencedSegment(description.SegmentInfos[0], this))
                {
                    return Deserializer.GetResource(description.SegmentInfos[segmentIndex], typeFullName, this, false);
                }
                if (description.SegmentInfos[segmentIndex].RequestEnumerable != null)
                {
                    return Deserializer.GetCrossReferencedResource(description.SegmentInfos[segmentIndex]);
                }
                object crossReferencedResource = Deserializer.GetCrossReferencedResource(description.SegmentInfos[0]);
                for (int i = 1; i <= segmentIndex; i++)
                {
                    crossReferencedResource = this.Updatable.GetValue(crossReferencedResource, description.SegmentInfos[i].Identifier);
                    if (crossReferencedResource == null)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_DereferencingNullPropertyValue(description.SegmentInfos[i].Identifier));
                    }
                    description.SegmentInfos[i].RequestEnumerable = new object[] { crossReferencedResource };
                }
                return crossReferencedResource;
            }

            public System.Data.Services.SegmentInfo GetSegmentForContentId(string contentId)
            {
                System.Data.Services.SegmentInfo info;
                if (!contentId.StartsWith("$", StringComparison.Ordinal))
                {
                    return null;
                }
                this.contentIdsToSegmentInfoMapping.TryGetValue(contentId.Substring(1), out info);
                if (info.TargetContainer.HasNavigationPropertyOrNamedStreamsOnDerivedTypes(this.Provider))
                {
                    object crossReferencedResource = Deserializer.GetCrossReferencedResource(info);
                    object instance = this.Updatable.ResolveResource(crossReferencedResource);
                    info.TargetResourceType = this.Provider.GetResourceType(instance);
                }
                return info;
            }

            internal void HandleBatchContent(Stream responseStream)
            {
                Exception exception = null;
                bool serviceOperationRequests = true;
                bool flag2 = false;
                this.batchResponseMessage.SetStream(responseStream);
                ODataBatchWriter batchWriter = this.messageWriter.CreateODataBatchWriter();
                batchWriter.WriteStartBatch();
                try
                {
                    while (!this.batchLimitExceeded && (this.batchReader.State != ODataBatchReaderState.Completed))
                    {
                        IODataRequestMessage message;
                        this.operationContext = null;
                        try
                        {
                            this.batchReader.Read();
                            message = (this.batchReader.State == ODataBatchReaderState.Operation) ? this.batchReader.CreateOperationRequestMessage() : null;
                        }
                        catch (Exception exception2)
                        {
                            if (!CommonUtil.IsCatchableExceptionType(exception2))
                            {
                                throw;
                            }
                            if (exception2 is ODataException)
                            {
                                exception2 = DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceException_GeneralError, exception2);
                            }
                            ErrorHandler.HandleBatchOperationError(this, null, exception2, batchWriter, responseStream, this.dataService.OperationContext.Host.RequestMinVersion);
                            break;
                        }
                        try
                        {
                            try
                            {
                                DataServiceProcessingPipelineEventArgs args;
                                switch (this.batchReader.State)
                                {
                                    case ODataBatchReaderState.Operation:
                                    {
                                        this.HandleBatchOperation(message, batchWriter, exception != null, ref serviceOperationRequests);
                                        continue;
                                    }
                                    case ODataBatchReaderState.ChangesetStart:
                                    {
                                        this.IncreaseBatchCount();
                                        batchWriter.WriteStartChangeset();
                                        flag2 = true;
                                        this.dataService.ProcessingPipeline.InvokeProcessingChangeset(this.dataService, new EventArgs());
                                        continue;
                                    }
                                    case ODataBatchReaderState.ChangesetEnd:
                                        this.changeSetElementCount = 0;
                                        this.contentIdsToSegmentInfoMapping.Clear();
                                        if (exception != null)
                                        {
                                            throw exception;
                                        }
                                        break;

                                    default:
                                        goto Label_0206;
                                }
                                if (this.batchRequestDescription.Count > 0)
                                {
                                    bool flag3 = this.batchRequestDescription.Any<RequestDescription>(d => DataServiceActionProviderWrapper.IsServiceActionRequest(d));
                                    if (serviceOperationRequests)
                                    {
                                        if (this.Provider.IsV1ProviderAndImplementsUpdatable() || flag3)
                                        {
                                            this.Updatable.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        this.Updatable.SaveChanges();
                                    }
                                }
                                this.dataService.ProcessingPipeline.InvokeProcessedChangeset(this.dataService, new EventArgs());
                                for (int i = 0; i < this.batchRequestDescription.Count; i++)
                                {
                                    this.operationContext = this.batchOperationContexts[i];
                                    this.WriteRequest(this.batchRequestDescription[i], this.batchOperationContexts[i].Host.BatchServiceHost);
                                }
                                batchWriter.WriteEndChangeset();
                                flag2 = false;
                                continue;
                            Label_0206:
                                args = new DataServiceProcessingPipelineEventArgs(this.dataService.OperationContext);
                                this.dataService.ProcessingPipeline.InvokeProcessedRequest(this.dataService, args);
                            }
                            catch (Exception exception3)
                            {
                                if (!CommonUtil.IsCatchableExceptionType(exception3))
                                {
                                    throw;
                                }
                                if (this.batchReader.State == ODataBatchReaderState.ChangesetEnd)
                                {
                                    this.HandleChangesetException(exception3, this.batchOperationContexts, batchWriter, responseStream, this.dataService.OperationContext.Host.RequestMinVersion);
                                    flag2 = false;
                                }
                                else if (flag2)
                                {
                                    exception = exception3;
                                }
                                else
                                {
									BatchServiceHost host = null;
                                    //using (BatchServiceHost host = null)
                                    {
                                        DataServiceHostWrapper wrapper = (this.operationContext == null) ? null : this.operationContext.Host;
                                        if (wrapper == null)
                                        {
                                            host = new BatchServiceHost(batchWriter);
                                            wrapper = new DataServiceHostWrapper(host);
                                        }
                                        ErrorHandler.HandleBatchOperationError(this, wrapper, exception3, batchWriter, responseStream, this.dataService.OperationContext.Host.RequestMinVersion);
                                    }
									if (host != null) host.Dispose ();
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            if (this.batchReader.State == ODataBatchReaderState.ChangesetEnd)
                            {
                                exception = null;
                                this.batchRequestDescription.Clear();
                                this.batchOperationContexts.Clear();
                            }
                        }
                    }
                    if (flag2)
                    {
                        batchWriter.WriteEndChangeset();
                    }
                    batchWriter.WriteEndBatch();
                    batchWriter.Flush();
                }
                catch (Exception exception4)
                {
                    if (!CommonUtil.IsCatchableExceptionType(exception4))
                    {
                        throw;
                    }
                    ErrorHandler.HandleBatchInStreamError(this, exception4, batchWriter, responseStream);
                }
                finally
                {
                    this.messageReader.Dispose();
                    this.messageWriter.Dispose();
                    this.DisposeDataSource();
                }
            }

            private void HandleBatchOperation(IODataRequestMessage operationMessage, ODataBatchWriter batchWriter, bool ignoreCUDOperations, ref bool serviceOperationRequests)
            {
                using (BatchServiceHost host = DataService<T>.BatchDataService.CreateBatchServiceHostFromOperationMessage(this.dataService, operationMessage, this.contentIds, batchWriter))
                {
                    DataServiceOperationContext item = DataService<T>.BatchDataService.CreateOperationContextFromBatchServiceHost(this.dataService, host);
                    string method = operationMessage.Method;
                    if (string.CompareOrdinal("GET", method) == 0)
                    {
                        this.IncreaseBatchCount();
                        this.operationContext = item;
                        this.dataService.InternalOnStartProcessingRequest(new ProcessRequestArgs(this.operationContext.AbsoluteRequestUri, true, this.operationContext));
                        DataService<T>.VerifyAndInitializeRequest(this);
                        RequestDescription description = DataService<T>.ProcessIncomingRequest(RequestUriProcessor.ProcessRequestUri(this.operationContext.AbsoluteRequestUri, this, false), this);
                        this.WriteRequest(description, item.Host.BatchServiceHost);
                    }
                    else if (((string.CompareOrdinal("POST", method) == 0) || (string.CompareOrdinal("PUT", method) == 0)) || (((string.CompareOrdinal("DELETE", method) == 0) || (string.CompareOrdinal("MERGE", method) == 0)) || (string.CompareOrdinal("PATCH", method) == 0)))
                    {
                        this.IncreaseChangeSetCount();
                        if (!ignoreCUDOperations)
                        {
                            this.batchOperationContexts.Add(item);
                            this.operationContext = item;
                            this.dataService.InternalOnStartProcessingRequest(new ProcessRequestArgs(this.operationContext.AbsoluteRequestUri, true, this.operationContext));
                            DataService<T>.VerifyAndInitializeRequest(this);
                            RequestDescription description2 = RequestUriProcessor.ProcessRequestUri(this.operationContext.AbsoluteRequestUri, this, false);
                            serviceOperationRequests &= description2.TargetSource == RequestTargetSource.ServiceOperation;
                            description2 = DataService<T>.ProcessIncomingRequest(description2, this);
                            this.batchRequestDescription.Add(description2);
                            if (description2.ShouldWriteResponseBody)
                            {
                                if (string.CompareOrdinal("POST", method) == 0)
                                {
                                    string contentId = item.Host.BatchServiceHost.ContentId;
                                    if (contentId != null)
                                    {
                                        this.contentIdsToSegmentInfoMapping.Add(contentId, description2.LastSegmentInfo);
                                    }
                                }
                                else if (string.CompareOrdinal("PUT", method) == 0)
                                {
                                    this.UpdateRequestEnumerableForPut(description2);
                                }
                            }
                        }
                    }
                }
            }

            private void HandleChangesetException(Exception exception, List<DataServiceOperationContext> changesetOperationContexts, ODataBatchWriter batchWriter, Stream responseStream, Version defaultResponseVersion)
            {
                BatchServiceHost host = null;
                try
                {
                    DataServiceHostWrapper wrapper;
                    DataServiceOperationContext context = (changesetOperationContexts.Count == 0) ? null : changesetOperationContexts[changesetOperationContexts.Count - 1];
                    if ((context == null) || (context.Host == null))
                    {
                        host = new BatchServiceHost(batchWriter);
                        wrapper = new DataServiceHostWrapper(host);
                    }
                    else
                    {
                        wrapper = context.Host;
                    }
                    ErrorHandler.HandleBatchOperationError(this, wrapper, exception, batchWriter, responseStream, defaultResponseVersion);
                    batchWriter.WriteEndChangeset();
                    this.Updatable.ClearChanges();
                }
                finally
                {
                    if (host != null)
                    {
                        host.Dispose();
                    }
                }
            }

            private void IncreaseBatchCount()
            {
                this.batchElementCount++;
                if (this.batchElementCount > this.dataService.Configuration.MaxBatchCount)
                {
                    this.batchLimitExceeded = true;
                    throw new DataServiceException(400, System.Data.Services.Strings.DataService_BatchExceedMaxBatchCount(this.dataService.Configuration.MaxBatchCount));
                }
            }

            private void IncreaseChangeSetCount()
            {
                this.changeSetElementCount++;
                if (this.changeSetElementCount > this.dataService.Configuration.MaxChangesetCount)
                {
                    throw new DataServiceException(400, System.Data.Services.Strings.DataService_BatchExceedMaxChangeSetCount(this.dataService.Configuration.MaxChangesetCount));
                }
            }

            public void InternalApplyingExpansions(Expression queryExpression, ICollection<ExpandSegmentCollection> expandPaths)
            {
                this.dataService.InternalApplyingExpansions(queryExpression, expandPaths);
            }

            public void InternalHandleException(HandleExceptionArgs args)
            {
                this.dataService.InternalHandleException(args);
            }

            public void InternalOnRequestQueryConstructed(IQueryable query)
            {
            }

            public void InternalOnStartProcessingRequest(ProcessRequestArgs args)
            {
                this.dataService.InternalOnStartProcessingRequest(args);
            }

            private void UpdateRequestEnumerableForPut(RequestDescription requestDescription)
            {
                string identifier = requestDescription.SegmentInfos[0].Identifier;
                if (identifier.StartsWith("$", StringComparison.Ordinal))
                {
                    string str2 = identifier.Substring(1);
                    for (int i = 0; i < (this.batchOperationContexts.Count - 1); i++)
                    {
                        DataServiceOperationContext context = this.batchOperationContexts[i];
                        BatchServiceHost batchServiceHost = context.Host.BatchServiceHost;
                        RequestDescription description = this.batchRequestDescription[i];
                        if ((context.Host.HttpVerb == HttpVerbs.POST) && (batchServiceHost.ContentId == str2))
                        {
                            object crossReferencedResource = Deserializer.GetCrossReferencedResource(requestDescription.LastSegmentInfo);
                            description.LastSegmentInfo.RequestEnumerable = new object[] { crossReferencedResource };
                            return;
                        }
                    }
                }
            }

            private void WriteRequest(RequestDescription description, BatchServiceHost batchHost)
            {
                ODataBatchOperationResponseMessage operationResponseMessage = batchHost.GetOperationResponseMessage();
                if (!description.ShouldWriteResponseBody)
                {
                    WebUtil.SetResponseHeadersForBatchRequests(operationResponseMessage, batchHost);
                }
                else
                {
                    Action<Stream> action = DataService<T>.SerializeResponseBody(description, this, operationResponseMessage);
                    WebUtil.SetResponseHeadersForBatchRequests(operationResponseMessage, batchHost);
                    if (action != null)
                    {
                        action(null);
                    }
                }
            }

            public DataServiceActionProviderWrapper ActionProvider
            {
                get
                {
                    return (this.actionProvider ?? (this.actionProvider = new DataServiceActionProviderWrapper(this)));
                }
            }

            public DataServiceConfiguration Configuration
            {
                get
                {
                    return this.dataService.Configuration;
                }
            }

            public DataServiceExecutionProviderWrapper ExecutionProvider
            {
                get
                {
                    return (this.executionProvider ?? (this.executionProvider = new DataServiceExecutionProviderWrapper(this)));
                }
            }

            public object Instance
            {
                get
                {
                    return this.dataService.Instance;
                }
            }

            public DataServiceOperationContext OperationContext
            {
                get
                {
                    return this.operationContext;
                }
            }

            public DataServicePagingProviderWrapper PagingProvider
            {
                get
                {
                    return (this.pagingProvider ?? (this.pagingProvider = new DataServicePagingProviderWrapper(this)));
                }
            }

            public DataServiceProcessingPipeline ProcessingPipeline
            {
                get
                {
                    return this.dataService.ProcessingPipeline;
                }
            }

            public DataServiceProviderWrapper Provider
            {
                get
                {
                    return this.dataService.Provider;
                }
            }

            public DataServiceStreamProviderWrapper StreamProvider
            {
                get
                {
                    return (this.streamProvider ?? (this.streamProvider = new DataServiceStreamProviderWrapper(this)));
                }
            }

            public UpdatableWrapper Updatable
            {
                get
                {
                    return (this.updatable ?? (this.updatable = new UpdatableWrapper(this)));
                }
            }
        }

        private class QueryResultsWrapper : IEnumerator, IDisposable
        {
            private readonly IEnumerator enumerator;
            private readonly IEnumerator query;

            public QueryResultsWrapper(IEnumerator enumerator, IEnumerator query)
            {
                this.enumerator = enumerator;
                this.query = query;
            }

            bool IEnumerator.MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                this.enumerator.Reset();
            }

            void IDisposable.Dispose()
            {
                WebUtil.Dispose(this.query);
                GC.SuppressFinalize(this);
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }
        }
    }
}

