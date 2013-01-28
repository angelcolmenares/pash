namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;

    internal class DataServiceContext
    {
        private DataServiceResponsePreference addAndUpdateResponsePreference;
        private bool applyingChanges;
        private UriResolver baseUriResolver;
        private ICredentials credentials;
        private string dataNamespace;
        private readonly System.Data.Services.Client.EntityTracker entityTracker;
        private Func<Stream, Stream> getRequestWrappingStream;
        private Func<Stream, Stream> getResponseWrappingStream;
        private bool ignoreMissingProperties;
        private bool ignoreResourceNotFoundException;
        private readonly DataServiceProtocolVersion maxProtocolVersion;
        internal readonly Version MaxProtocolVersionAsVersion;
        private System.Data.Services.Client.MergeOption mergeOption;
        private bool postTunneling;
        private Func<Type, string> resolveName;
        private Func<string, Type> resolveType;
        private SaveChangesOptions saveChangesDefaultOptions;
        private Action<object> sendRequest;
        private Action<object> sendResponse;
        private int timeout;
        private Uri typeScheme;

        internal event EventHandler<SaveChangesEventArgs> ChangesSaved;

        public event EventHandler<ReadingWritingEntityEventArgs> ReadingEntity;

        public event EventHandler<SendingRequestEventArgs> SendingRequest;

        public event EventHandler<SendingRequest2EventArgs> SendingRequest2;

        public event EventHandler<ReadingWritingEntityEventArgs> WritingEntity;

        public DataServiceContext() : this(null)
        {
        }

        public DataServiceContext(Uri serviceRoot) : this(serviceRoot, DataServiceProtocolVersion.V2)
        {
        }

        public DataServiceContext(Uri serviceRoot, DataServiceProtocolVersion maxProtocolVersion)
        {
            this.baseUriResolver = UriResolver.CreateFromBaseUri(serviceRoot, "serviceRoot");
            this.maxProtocolVersion = Util.CheckEnumerationValue(maxProtocolVersion, "maxProtocolVersion");
            this.mergeOption = System.Data.Services.Client.MergeOption.AppendOnly;
            this.dataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            this.entityTracker = new System.Data.Services.Client.EntityTracker(maxProtocolVersion);
            this.typeScheme = new Uri("http://schemas.microsoft.com/ado/2007/08/dataservices/scheme");
            this.MaxProtocolVersionAsVersion = Util.GetVersionFromMaxProtocolVersion(maxProtocolVersion);
            this.UsePostTunneling = false;
        }

        public void AddLink(object source, string sourceProperty, object target)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Added);
            LinkDescriptor linkDescriptor = new LinkDescriptor(source, sourceProperty, target, this.MaxProtocolVersion);
            this.entityTracker.AddLink(linkDescriptor);
            linkDescriptor.State = EntityStates.Added;
            this.EntityTracker.IncrementChange(linkDescriptor);
        }

        public void AddObject(string entitySetName, object entity)
        {
            ValidateEntitySetName(ref entitySetName);
            ValidateEntityType(entity, this.MaxProtocolVersion);
            EntityDescriptor descriptor = new EntityDescriptor(null, null, null, this.BaseUriResolver.GetEntitySetUri(entitySetName), entity, null, null, null, EntityStates.Added, this.MaxProtocolVersion);
            this.EntityTracker.AddEntityDescriptor(descriptor);
            this.EntityTracker.IncrementChange(descriptor);
        }

        public void AddRelatedObject(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull<object>(source, "source");
            Util.CheckArgumentNullAndEmpty(sourceProperty, "sourceProperty");
            Util.CheckArgumentNull<object>(target, "target");
            ValidateEntityType(source, this.MaxProtocolVersion);
            EntityDescriptor entityDescriptor = this.EntityTracker.GetEntityDescriptor(source);
            if (entityDescriptor.State == EntityStates.Deleted)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_AddRelatedObjectSourceDeleted);
            }
            ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
            ClientPropertyAnnotation property = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(source.GetType())).GetProperty(sourceProperty, false);
            if (property.IsKnownType || !property.IsEntityCollection)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_AddRelatedObjectCollectionOnly);
            }
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(target.GetType()));
            ValidateEntityType(target, this.MaxProtocolVersion);
            if (!model.GetClientTypeAnnotation(model.GetOrCreateEdmType(property.EntityCollectionItemType)).ElementType.IsAssignableFrom(clientTypeAnnotation.ElementType))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_RelationNotRefOrCollection, "target");
            }
            EntityDescriptor descriptor = new EntityDescriptor(null, null, null, null, target, entityDescriptor, sourceProperty, null, EntityStates.Added, this.MaxProtocolVersion);
            this.EntityTracker.AddEntityDescriptor(descriptor);
            LinkDescriptor relatedEnd = descriptor.GetRelatedEnd(this.MaxProtocolVersion);
            relatedEnd.State = EntityStates.Added;
            this.EntityTracker.AddLink(relatedEnd);
            this.EntityTracker.IncrementChange(descriptor);
        }

        internal static Uri AppendKeysToUri(string uri, object entity, UriKind kind, DataServiceProtocolVersion maxProtocolVersion)
        {
            ValidateEntityTypeHasKeys(entity, maxProtocolVersion);
            StringBuilder builder = new StringBuilder();
            builder.Append(uri);
            builder.Append("(");
            string str = string.Empty;
            ClientEdmModel model = ClientEdmModel.GetModel(maxProtocolVersion);
            IEdmEntityType orCreateEdmType = model.GetOrCreateEdmType(entity.GetType()) as IEdmEntityType;
            ClientPropertyAnnotation[] annotationArray = (orCreateEdmType != null) ? (from k in orCreateEdmType.Key() select model.GetClientPropertyAnnotation(k)).ToArray<ClientPropertyAnnotation>() : new ClientPropertyAnnotation[0];
            foreach (ClientPropertyAnnotation annotation in annotationArray)
            {
                string str2;
                builder.Append(str);
                if (1 < annotationArray.Length)
                {
                    builder.Append(annotation.PropertyName).Append("=");
                }
                object obj2 = annotation.GetValue(entity);
                if (obj2 == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NullKeysAreNotSupported(annotation.PropertyName));
                }
                if (!ClientConvert.TryKeyPrimitiveToString(obj2, out str2))
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_CannotConvertKey(obj2));
                }
                builder.Append(str2);
                str = ",";
            }
            builder.Append(")");
            return Util.CreateUri(builder.ToString(), kind);
        }

        public void AttachLink(object source, string sourceProperty, object target)
        {
            this.AttachLink(source, sourceProperty, target, System.Data.Services.Client.MergeOption.NoTracking);
        }

        internal void AttachLink(object source, string sourceProperty, object target, System.Data.Services.Client.MergeOption linkMerge)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Unchanged);
            this.EntityTracker.AttachLink(source, sourceProperty, target, linkMerge);
        }

        public void AttachTo(string entitySetName, object entity)
        {
            this.AttachTo(entitySetName, entity, null);
        }

        public void AttachTo(string entitySetName, object entity, string etag)
        {
            ValidateEntitySetName(ref entitySetName);
            Uri editLink = this.GenerateEditLinkUri(entitySetName, entity);
            EntityDescriptor entityDescriptorFromMaterializer = new EntityDescriptor(editLink.AbsoluteUri, null, editLink, null, entity, null, null, etag, EntityStates.Unchanged, this.MaxProtocolVersion);
            this.entityTracker.InternalAttachEntityDescriptor(entityDescriptorFromMaterializer, true);
        }

        public IAsyncResult BeginExecute<T>(DataServiceQueryContinuation<T> continuation, AsyncCallback callback, object state)
        {
            Util.CheckArgumentNull<DataServiceQueryContinuation<T>>(continuation, "continuation");
            return new DataServiceRequest<T>(continuation.CreateQueryComponents(), continuation.Plan).BeginExecute(this, this, callback, state, "Execute");
        }

        public IAsyncResult BeginExecute<TElement>(Uri requestUri, AsyncCallback callback, object state)
        {
            return this.InnerBeginExecute<TElement>(requestUri, callback, state, "GET", "Execute", null, new OperationParameter[0]);
        }

        public IAsyncResult BeginExecute(Uri requestUri, AsyncCallback callback, object state, string httpMethod, params OperationParameter[] operationParameters)
        {
            return this.InnerBeginExecute<object>(requestUri, callback, state, httpMethod, "ExecuteVoid", false, operationParameters);
        }

        public IAsyncResult BeginExecute<TElement>(Uri requestUri, AsyncCallback callback, object state, string httpMethod, bool singleResult, params OperationParameter[] operationParameters)
        {
            return this.InnerBeginExecute<TElement>(requestUri, callback, state, httpMethod, "Execute", new bool?(singleResult), operationParameters);
        }

        public IAsyncResult BeginExecuteBatch(AsyncCallback callback, object state, params DataServiceRequest[] queries)
        {
            Util.CheckArgumentNotEmpty<DataServiceRequest>(queries, "queries");
            BatchSaveResult result = new BatchSaveResult(this, "ExecuteBatch", queries, SaveChangesOptions.Batch, callback, state);
            result.BatchBeginRequest();
            return result;
        }

        public IAsyncResult BeginGetReadStream(object entity, DataServiceRequestArgs args, AsyncCallback callback, object state)
        {
            GetReadStreamResult result = this.CreateGetReadStreamResult(entity, args, callback, state, null);
            result.Begin();
            return result;
        }

        public IAsyncResult BeginGetReadStream(object entity, string name, DataServiceRequestArgs args, AsyncCallback callback, object state)
        {
            Util.CheckArgumentNullAndEmpty(name, "name");
            this.EnsureMinimumProtocolVersionV3();
            GetReadStreamResult result = this.CreateGetReadStreamResult(entity, args, callback, state, name);
            result.Begin();
            return result;
        }

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, AsyncCallback callback, object state)
        {
            return this.BeginLoadProperty(entity, propertyName, (Uri) null, callback, state);
        }

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, DataServiceQueryContinuation continuation, AsyncCallback callback, object state)
        {
            Util.CheckArgumentNull<DataServiceQueryContinuation>(continuation, "continuation");
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, callback, state, null, continuation);
            result.BeginExecuteQuery(this);
            return result;
        }

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, Uri nextLinkUri, AsyncCallback callback, object state)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, callback, state, nextLinkUri, null);
            result.BeginExecuteQuery(this);
            return result;
        }

        public IAsyncResult BeginSaveChanges(AsyncCallback callback, object state)
        {
            return this.BeginSaveChanges(this.SaveChangesDefaultOptions, callback, state);
        }

        public IAsyncResult BeginSaveChanges(SaveChangesOptions options, AsyncCallback callback, object state)
        {
            this.ValidateSaveChangesOptions(options);
            BaseSaveResult result = BaseSaveResult.CreateSaveResult(this, "SaveChanges", null, options, callback, state);
            if (result.IsBatch)
            {
                ((BatchSaveResult) result).BatchBeginRequest();
                return result;
            }
            ((SaveResult) result).BeginCreateNextChange();
            return result;
        }

        public void CancelRequest(IAsyncResult asyncResult)
        {
            Util.CheckArgumentNull<IAsyncResult>(asyncResult, "asyncResult");
            BaseAsyncResult result = asyncResult as BaseAsyncResult;
            if ((result == null) || (this != result.Source))
            {
                object context = null;
                DataServiceQuery source = null;
                if (result != null)
                {
                    source = result.Source as DataServiceQuery;
                    if (source != null)
                    {
                        DataServiceQueryProvider provider = source.Provider as DataServiceQueryProvider;
                        if (provider != null)
                        {
                            context = provider.Context;
                        }
                    }
                }
                if (this != context)
                {
                    throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_DidNotOriginateAsync, "asyncResult");
                }
            }
            if (!result.IsCompletedInternally)
            {
                result.SetAborted();
                ODataRequestMessageWrapper abortable = result.Abortable;
                if (abortable != null)
                {
                    abortable.Abort();
                }
            }
        }

        private GetReadStreamResult CreateGetReadStreamResult(object entity, DataServiceRequestArgs args, AsyncCallback callback, object state, string name)
        {
            StreamDescriptor defaultStreamDescriptor;
            Uri readStreamUri;
            Version version;
            Util.CheckArgumentNull<object>(entity, "entity");
            Util.CheckArgumentNull<DataServiceRequestArgs>(args, "args");
            EntityDescriptor entityDescriptor = this.EntityTracker.GetEntityDescriptor(entity);
            if (name == null)
            {
                version = null;
                readStreamUri = entityDescriptor.ReadStreamUri;
                if (readStreamUri == null)
                {
                    throw new ArgumentException(System.Data.Services.Client.Strings.Context_EntityNotMediaLinkEntry, "entity");
                }
                defaultStreamDescriptor = entityDescriptor.DefaultStreamDescriptor;
            }
            else
            {
                version = Util.DataServiceVersion3;
                if (!entityDescriptor.TryGetNamedStreamInfo(name, out defaultStreamDescriptor))
                {
                    throw new ArgumentException(System.Data.Services.Client.Strings.Context_EntityDoesNotContainNamedStream(name), "name");
                }
                readStreamUri = defaultStreamDescriptor.SelfLink ?? defaultStreamDescriptor.EditLink;
                if (readStreamUri == null)
                {
                    throw new ArgumentException(System.Data.Services.Client.Strings.Context_MissingSelfAndEditLinkForNamedStream(name), "name");
                }
            }
            return new GetReadStreamResult(this, "GetReadStream", this.CreateODataRequestMessage("GET", readStreamUri, true, version, args.Headers, args.Headers.Keys), callback, state, defaultStreamDescriptor);
        }

        private LoadPropertyResult CreateLoadPropertyRequest(object entity, string propertyName, AsyncCallback callback, object state, Uri requestUri, DataServiceQueryContinuation continuation)
        {
            ProjectionPlan plan;
            Version version;
            Util.CheckArgumentNull<object>(entity, "entity");
            EntityDescriptor entityDescriptor = this.entityTracker.GetEntityDescriptor(entity);
            Util.CheckArgumentNullAndEmpty(propertyName, "propertyName");
            ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entity.GetType()));
            if (EntityStates.Added == entityDescriptor.State)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoLoadWithInsertEnd);
            }
            ClientPropertyAnnotation property = clientTypeAnnotation.GetProperty(propertyName, false);
            if (continuation == null)
            {
                plan = null;
            }
            else
            {
                plan = continuation.Plan;
                requestUri = continuation.NextLinkUri;
            }
            bool allowAnyType = (clientTypeAnnotation.MediaDataMember != null) && (propertyName == clientTypeAnnotation.MediaDataMember.PropertyName);
            if (requestUri == null)
            {
                if (allowAnyType)
                {
                    Uri uri = Util.CreateUri("$value", UriKind.Relative);
                    requestUri = Util.CreateUri(entityDescriptor.GetResourceUri(this.BaseUriResolver, true), uri);
                }
                else
                {
                    requestUri = entityDescriptor.GetNavigationLink(this.baseUriResolver, property);
                }
                version = Util.DataServiceVersion1;
            }
            else
            {
                version = Util.DataServiceVersion2;
            }
            ODataRequestMessageWrapper wrapper = this.CreateODataRequestMessage("GET", requestUri, allowAnyType, version, null, null);
            DataServiceRequest instance = DataServiceRequest.GetInstance(property.PropertyType, requestUri);
            instance.PayloadKind = ODataPayloadKind.Property;
            return new LoadPropertyResult(entity, propertyName, this, wrapper, callback, state, instance, plan);
        }

        internal ODataRequestMessageWrapper CreateODataRequestMessage(string httpMethod, Uri requestUri, bool allowAnyType, Version requestVersion, IEnumerable<KeyValuePair<string, string>> headers, IEnumerable<string> headersToReset)
        {
            ODataRequestMessageWrapper requestMessage = new ODataRequestMessageWrapper(httpMethod, requestUri, new RequestInfo(this));
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    requestMessage.SetHeader(pair.Key, pair.Value);
                }
            }
            WebUtil.SetOperationVersionHeaders(requestMessage, requestVersion, this.MaxProtocolVersionAsVersion);
            if (headersToReset != null)
            {
                requestMessage.AddHeadersToReset(headersToReset);
            }
            requestMessage.AllowAnyAcceptType = allowAnyType;
            requestMessage.FireSendingRequest2(null);
            return requestMessage;
        }

        public DataServiceQuery<T> CreateQuery<T>(string entitySetName)
        {
            Util.CheckArgumentNullAndEmpty(entitySetName, "entitySetName");
            ValidateEntitySetName(ref entitySetName);
            return new DataServiceQuery<T>.DataServiceOrderedQuery(new ResourceSetExpression(typeof(IOrderedQueryable<T>), null, Expression.Constant(entitySetName), typeof(T), null, CountOption.None, null, null, null, null), new DataServiceQueryProvider(this));
        }

        public void DeleteLink(object source, string sourceProperty, object target)
        {
            bool flag = this.EnsureRelatable(source, sourceProperty, target, EntityStates.Deleted);
            LinkDescriptor existingLink = this.entityTracker.TryGetLinkDescriptor(source, sourceProperty, target);
            if ((existingLink != null) && (EntityStates.Added == existingLink.State))
            {
                this.entityTracker.DetachExistingLink(existingLink, false);
            }
            else
            {
                if (flag)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoRelationWithInsertEnd);
                }
                if (existingLink == null)
                {
                    LinkDescriptor linkDescriptor = new LinkDescriptor(source, sourceProperty, target, this.MaxProtocolVersion);
                    this.entityTracker.AddLink(linkDescriptor);
                    existingLink = linkDescriptor;
                }
                if (EntityStates.Deleted != existingLink.State)
                {
                    existingLink.State = EntityStates.Deleted;
                    this.EntityTracker.IncrementChange(existingLink);
                }
            }
        }

        public void DeleteObject(object entity)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            EntityDescriptor entityDescriptor = this.EntityTracker.GetEntityDescriptor(entity);
            EntityStates state = entityDescriptor.State;
            if (EntityStates.Added == state)
            {
                this.entityTracker.DetachResource(entityDescriptor);
            }
            else if (EntityStates.Deleted != state)
            {
                entityDescriptor.State = EntityStates.Deleted;
                this.EntityTracker.IncrementChange(entityDescriptor);
            }
        }

        public bool Detach(object entity)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            EntityDescriptor resource = this.entityTracker.TryGetEntityDescriptor(entity);
            return ((resource != null) && this.entityTracker.DetachResource(resource));
        }

        public bool DetachLink(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull<object>(source, "source");
            Util.CheckArgumentNullAndEmpty(sourceProperty, "sourceProperty");
            LinkDescriptor existingLink = this.entityTracker.TryGetLinkDescriptor(source, sourceProperty, target);
            if (existingLink == null)
            {
                return false;
            }
            this.entityTracker.DetachExistingLink(existingLink, false);
            return true;
        }

        public IEnumerable<TElement> EndExecute<TElement>(IAsyncResult asyncResult)
        {
            Util.CheckArgumentNull<IAsyncResult>(asyncResult, "asyncResult");
            return DataServiceRequest.EndExecute<TElement>(this, this, "Execute", asyncResult);
        }

        public OperationResponse EndExecute(IAsyncResult asyncResult)
        {
            Util.CheckArgumentNull<IAsyncResult>(asyncResult, "asyncResult");
            QueryOperationResponse<object> source = (QueryOperationResponse<object>) DataServiceRequest.EndExecute<object>(this, this, "ExecuteVoid", asyncResult);
            if (source.Any<object>())
            {
                throw new DataServiceClientException(System.Data.Services.Client.Strings.Context_EndExecuteExpectedVoidResponse);
            }
            return source;
        }

        public DataServiceResponse EndExecuteBatch(IAsyncResult asyncResult)
        {
            return BaseAsyncResult.EndExecute<BatchSaveResult>(this, "ExecuteBatch", asyncResult).EndRequest();
        }

        public DataServiceStreamResponse EndGetReadStream(IAsyncResult asyncResult)
        {
            return BaseAsyncResult.EndExecute<GetReadStreamResult>(this, "GetReadStream", asyncResult).End();
        }

        public QueryOperationResponse EndLoadProperty(IAsyncResult asyncResult)
        {
            return BaseAsyncResult.EndExecute<LoadPropertyResult>(this, "LoadProperty", asyncResult).LoadProperty();
        }

        public DataServiceResponse EndSaveChanges(IAsyncResult asyncResult)
        {
            DataServiceResponse response = BaseAsyncResult.EndExecute<BaseSaveResult>(this, "SaveChanges", asyncResult).EndRequest();
            if (this.ChangesSaved != null)
            {
                this.ChangesSaved(this, new SaveChangesEventArgs(response));
            }
            return response;
        }

        private void EnsureMaximumProtocolVersionForProperty(string propertyName, Version maxAllowedVersion)
        {
            if (this.MaxProtocolVersionAsVersion > maxAllowedVersion)
            {
                throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Context_PropertyNotSupportedForMaxDataServiceVersionGreaterThanX(propertyName, maxAllowedVersion));
            }
        }

        private void EnsureMinimumProtocolVersionV3()
        {
            if (this.MaxProtocolVersionAsVersion < Util.DataServiceVersion3)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_RequestVersionIsBiggerThanProtocolVersion(Util.DataServiceVersion3, this.MaxProtocolVersionAsVersion));
            }
        }

        private bool EnsureRelatable(object source, string sourceProperty, object target, EntityStates state)
        {
            Util.CheckArgumentNull<object>(source, "source");
            EntityDescriptor entityDescriptor = this.entityTracker.GetEntityDescriptor(source);
            EntityDescriptor descriptor2 = null;
            if ((target != null) || ((EntityStates.Modified != state) && (EntityStates.Unchanged != state)))
            {
                Util.CheckArgumentNull<object>(target, "target");
                descriptor2 = this.entityTracker.GetEntityDescriptor(target);
            }
            Util.CheckArgumentNullAndEmpty(sourceProperty, "sourceProperty");
            ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
            ClientPropertyAnnotation property = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(source.GetType())).GetProperty(sourceProperty, false);
            if (property.IsKnownType)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_RelationNotRefOrCollection);
            }
            if (((EntityStates.Unchanged == state) && (target == null)) && property.IsEntityCollection)
            {
                Util.CheckArgumentNull<object>(target, "target");
                descriptor2 = this.entityTracker.GetEntityDescriptor(target);
            }
            if (((EntityStates.Added == state) || (EntityStates.Deleted == state)) && !property.IsEntityCollection)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_AddLinkCollectionOnly);
            }
            if ((EntityStates.Modified == state) && property.IsEntityCollection)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_SetLinkReferenceOnly);
            }
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(property.EntityCollectionItemType ?? property.PropertyType));
            if ((target != null) && !clientTypeAnnotation.ElementType.IsInstanceOfType(target))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_RelationNotRefOrCollection, "target");
            }
            if (((EntityStates.Added == state) || (EntityStates.Unchanged == state)) && ((entityDescriptor.State == EntityStates.Deleted) || ((descriptor2 != null) && (descriptor2.State == EntityStates.Deleted))))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoRelationWithDeleteEnd);
            }
            if (((EntityStates.Deleted == state) || (EntityStates.Unchanged == state)) && ((entityDescriptor.State == EntityStates.Added) || ((descriptor2 != null) && (descriptor2.State == EntityStates.Added))))
            {
                if (EntityStates.Deleted != state)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoRelationWithInsertEnd);
                }
                return true;
            }
            return false;
        }

        public QueryOperationResponse<T> Execute<T>(DataServiceQueryContinuation<T> continuation)
        {
            Util.CheckArgumentNull<DataServiceQueryContinuation<T>>(continuation, "continuation");
            QueryComponents queryComponents = continuation.CreateQueryComponents();
            DataServiceRequest request = new DataServiceRequest<T>(queryComponents, continuation.Plan);
            return request.Execute<T>(this, queryComponents);
        }

        public IEnumerable<TElement> Execute<TElement>(Uri requestUri)
        {
            return this.InnerSynchExecute<TElement>(requestUri, "GET", null, new OperationParameter[0]);
        }

        public OperationResponse Execute(Uri requestUri, string httpMethod, params OperationParameter[] operationParameters)
        {
            QueryOperationResponse<object> source = (QueryOperationResponse<object>) this.Execute<object>(requestUri, httpMethod, false, operationParameters);
            if (source.Any<object>())
            {
                throw new DataServiceClientException(System.Data.Services.Client.Strings.Context_ExecuteExpectedVoidResponse);
            }
            return source;
        }

        public IEnumerable<TElement> Execute<TElement>(Uri requestUri, string httpMethod, bool singleResult, params OperationParameter[] operationParameters)
        {
            return this.InnerSynchExecute<TElement>(requestUri, httpMethod, new bool?(singleResult), operationParameters);
        }

        public DataServiceResponse ExecuteBatch(params DataServiceRequest[] queries)
        {
            Util.CheckArgumentNotEmpty<DataServiceRequest>(queries, "queries");
            BatchSaveResult result = new BatchSaveResult(this, "ExecuteBatch", queries, SaveChangesOptions.Batch, null, null);
            result.BatchRequest();
            return result.EndRequest();
        }

        internal void FireReadingEntityEvent(object entity, XElement data, Uri baseUri)
        {
            ReadingWritingEntityEventArgs e = new ReadingWritingEntityEventArgs(entity, data, baseUri);
            this.ReadingEntity(this, e);
        }

        internal void FireSendingRequest(SendingRequestEventArgs eventArgs)
        {
            this.SendingRequest(this, eventArgs);
        }

        internal void FireSendingRequest2(SendingRequest2EventArgs eventArgs)
        {
            this.SendingRequest2(this, eventArgs);
        }

        internal void FireWritingEntityEvent(object entity, XElement data, Uri baseUri)
        {
            ReadingWritingEntityEventArgs e = new ReadingWritingEntityEventArgs(entity, data, baseUri);
            this.WritingEntity(this, e);
        }

        internal static Uri GenerateEditLinkRelativeUri(string entitySetName, object entity, DataServiceProtocolVersion maxProtocolVersion)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(entitySetName);
            return AppendKeysToUri(builder.ToString(), entity, UriKind.Relative, maxProtocolVersion);
        }

        internal Uri GenerateEditLinkUri(string entitySetName, object entity)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CommonUtil.UriToString(this.BaseUriResolver.GetEntitySetUri(entitySetName)));
            return AppendKeysToUri(builder.ToString(), entity, UriKind.Absolute, this.MaxProtocolVersion);
        }

        public EntityDescriptor GetEntityDescriptor(object entity)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            return this.entityTracker.TryGetEntityDescriptor(entity);
        }

        public LinkDescriptor GetLinkDescriptor(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull<object>(source, "source");
            Util.CheckArgumentNullAndEmpty(sourceProperty, "sourceProperty");
            Util.CheckArgumentNull<object>(target, "target");
            return this.entityTracker.TryGetLinkDescriptor(source, sourceProperty, target);
        }

        public Uri GetMetadataUri()
        {
            return Util.CreateUri(CommonUtil.UriToString(this.BaseUriResolver.GetBaseUriWithSlash()) + "$metadata", UriKind.Absolute);
        }

        private Uri GetOrCreateAbsoluteUri(Uri requestUri)
        {
            Util.CheckArgumentNull<Uri>(requestUri, "requestUri");
            if (requestUri.IsAbsoluteUri)
            {
                return requestUri;
            }
            return Util.CreateUri(this.BaseUriResolver.GetBaseUriWithSlash(() => System.Data.Services.Client.Strings.Context_RequestUriIsRelativeBaseUriRequired), requestUri);
        }

        public DataServiceStreamResponse GetReadStream(object entity)
        {
            DataServiceRequestArgs args = new DataServiceRequestArgs();
            return this.GetReadStream(entity, args);
        }

        public DataServiceStreamResponse GetReadStream(object entity, DataServiceRequestArgs args)
        {
            return this.CreateGetReadStreamResult(entity, args, null, null, null).Execute();
        }

        public DataServiceStreamResponse GetReadStream(object entity, string acceptContentType)
        {
            Util.CheckArgumentNullAndEmpty(acceptContentType, "acceptContentType");
            DataServiceRequestArgs args = new DataServiceRequestArgs {
                AcceptContentType = acceptContentType
            };
            return this.GetReadStream(entity, args);
        }

        public DataServiceStreamResponse GetReadStream(object entity, string name, DataServiceRequestArgs args)
        {
            Util.CheckArgumentNullAndEmpty(name, "name");
            this.EnsureMinimumProtocolVersionV3();
            return this.CreateGetReadStreamResult(entity, args, null, null, name).Execute();
        }

        public Uri GetReadStreamUri(object entity)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            return this.EntityTracker.GetEntityDescriptor(entity).ReadStreamUri;
        }

        public Uri GetReadStreamUri(object entity, string name)
        {
            StreamDescriptor descriptor2;
            Util.CheckArgumentNull<object>(entity, "entity");
            Util.CheckArgumentNullAndEmpty(name, "name");
            this.EnsureMinimumProtocolVersionV3();
            if (!this.EntityTracker.GetEntityDescriptor(entity).TryGetNamedStreamInfo(name, out descriptor2))
            {
                return null;
            }
            return (descriptor2.SelfLink ?? descriptor2.EditLink);
        }

        internal IAsyncResult InnerBeginExecute<TElement>(Uri requestUri, AsyncCallback callback, object state, string httpMethod, string method, bool? singleResult, params OperationParameter[] operationParameters)
        {
            List<UriOperationParameter> uriOperationParameters = null;
            List<BodyOperationParameter> bodyOperationParameters = null;
            this.ValidateExecuteParameters<TElement>(ref requestUri, httpMethod, ref singleResult, out bodyOperationParameters, out uriOperationParameters, operationParameters);
            QueryComponents queryComponents = new QueryComponents(requestUri, Util.DataServiceVersionEmpty, typeof(TElement), null, null, httpMethod, singleResult, bodyOperationParameters, uriOperationParameters);
            return new DataServiceRequest<TElement>(queryComponents, null).BeginExecute(this, this, callback, state, method);
        }

        internal QueryOperationResponse<TElement> InnerSynchExecute<TElement>(Uri requestUri, string httpMethod, bool? singleResult, params OperationParameter[] operationParameters)
        {
            List<UriOperationParameter> uriOperationParameters = null;
            List<BodyOperationParameter> bodyOperationParameters = null;
            this.ValidateExecuteParameters<TElement>(ref requestUri, httpMethod, ref singleResult, out bodyOperationParameters, out uriOperationParameters, operationParameters);
            QueryComponents queryComponents = new QueryComponents(requestUri, Util.DataServiceVersionEmpty, typeof(TElement), null, null, httpMethod, singleResult, bodyOperationParameters, uriOperationParameters);
            DataServiceRequest request = new DataServiceRequest<TElement>(queryComponents, null);
            return request.Execute<TElement>(this, queryComponents);
        }

        internal Stream InternalGetRequestWrappingStream(Stream requestStream)
        {
            if (this.getRequestWrappingStream == null)
            {
                return requestStream;
            }
            return this.getRequestWrappingStream(requestStream);
        }

        internal Stream InternalGetResponseWrappingStream(Stream responseStream)
        {
            if (this.getResponseWrappingStream == null)
            {
                return responseStream;
            }
            return this.getResponseWrappingStream(responseStream);
        }

        internal void InternalSendRequest(HttpWebRequest request)
        {
            if (this.sendRequest != null)
            {
                this.sendRequest(request);
            }
        }

        internal void InternalSendResponse(HttpWebResponse response)
        {
            if (this.sendResponse != null)
            {
                this.sendResponse(response);
            }
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName)
        {
            return this.LoadProperty(entity, propertyName, (Uri) null);
        }

        public QueryOperationResponse<T> LoadProperty<T>(object entity, string propertyName, DataServiceQueryContinuation<T> continuation)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, null, continuation);
            result.ExecuteQuery(this);
            return (QueryOperationResponse<T>) result.LoadProperty();
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName, DataServiceQueryContinuation continuation)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, null, continuation);
            result.ExecuteQuery(this);
            return result.LoadProperty();
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName, Uri nextLinkUri)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, nextLinkUri, null);
            result.ExecuteQuery(this);
            return result.LoadProperty();
        }

        internal string ResolveNameFromType(Type type)
        {
            Func<Type, string> resolveName = this.ResolveName;
            if (resolveName == null)
            {
                return null;
            }
            return resolveName(type);
        }

        internal Type ResolveTypeFromName(string wireName)
        {
            Func<string, Type> resolveType = this.ResolveType;
            if (resolveType != null)
            {
                return resolveType(wireName);
            }
            return null;
        }

        public DataServiceResponse SaveChanges()
        {
            return this.SaveChanges(this.SaveChangesDefaultOptions);
        }

        public DataServiceResponse SaveChanges(SaveChangesOptions options)
        {
            DataServiceResponse response = null;
            this.ValidateSaveChangesOptions(options);
            BaseSaveResult result = BaseSaveResult.CreateSaveResult(this, "SaveChanges", null, options, null, null);
            if (result.IsBatch)
            {
                ((BatchSaveResult) result).BatchRequest();
            }
            else
            {
                ((SaveResult) result).CreateNextChange();
            }
            response = result.EndRequest();
            if (this.ChangesSaved != null)
            {
                this.ChangesSaved(this, new SaveChangesEventArgs(response));
            }
            return response;
        }

        public void SetLink(object source, string sourceProperty, object target)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Modified);
            LinkDescriptor linkDescriptor = this.EntityTracker.DetachReferenceLink(source, sourceProperty, target, System.Data.Services.Client.MergeOption.NoTracking);
            if (linkDescriptor == null)
            {
                linkDescriptor = new LinkDescriptor(source, sourceProperty, target, this.MaxProtocolVersion);
                this.entityTracker.AddLink(linkDescriptor);
            }
            if (EntityStates.Modified != linkDescriptor.State)
            {
                linkDescriptor.State = EntityStates.Modified;
                this.EntityTracker.IncrementChange(linkDescriptor);
            }
        }

        public void SetSaveStream(object entity, Stream stream, bool closeStream, DataServiceRequestArgs args)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            Util.CheckArgumentNull<Stream>(stream, "stream");
            Util.CheckArgumentNull<DataServiceRequestArgs>(args, "args");
            EntityDescriptor entityDescriptor = this.entityTracker.GetEntityDescriptor(entity);
            ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entity.GetType()));
            if (clientTypeAnnotation.MediaDataMember != null)
            {
                throw new ArgumentException(System.Data.Services.Client.Strings.Context_SetSaveStreamOnMediaEntryProperty(clientTypeAnnotation.ElementTypeName), "entity");
            }
            entityDescriptor.SaveStream = new DataServiceSaveStream(stream, closeStream, args);
            switch (entityDescriptor.State)
            {
                case EntityStates.Unchanged:
                case EntityStates.Modified:
                    entityDescriptor.StreamState = EntityStates.Modified;
                    return;

                case EntityStates.Added:
                    entityDescriptor.StreamState = EntityStates.Added;
                    return;
            }
            throw new DataServiceClientException(System.Data.Services.Client.Strings.Context_SetSaveStreamOnInvalidEntityState(System.Enum.GetName(typeof(EntityStates), entityDescriptor.State)));
        }

        public void SetSaveStream(object entity, Stream stream, bool closeStream, string contentType, string slug)
        {
            Util.CheckArgumentNull<string>(contentType, "contentType");
            Util.CheckArgumentNull<string>(slug, "slug");
            DataServiceRequestArgs args = new DataServiceRequestArgs {
                ContentType = contentType,
                Slug = slug
            };
            this.SetSaveStream(entity, stream, closeStream, args);
        }

        public void SetSaveStream(object entity, string name, Stream stream, bool closeStream, DataServiceRequestArgs args)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            Util.CheckArgumentNullAndEmpty(name, "name");
            Util.CheckArgumentNull<Stream>(stream, "stream");
            Util.CheckArgumentNull<DataServiceRequestArgs>(args, "args");
            this.EnsureMinimumProtocolVersionV3();
            if (string.IsNullOrEmpty(args.ContentType))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_ContentTypeRequiredForNamedStream, "args");
            }
            EntityDescriptor entityDescriptor = this.entityTracker.GetEntityDescriptor(entity);
            if (entityDescriptor.State == EntityStates.Deleted)
            {
                throw new DataServiceClientException(System.Data.Services.Client.Strings.Context_SetSaveStreamOnInvalidEntityState(System.Enum.GetName(typeof(EntityStates), entityDescriptor.State)));
            }
            StreamDescriptor descriptor = entityDescriptor.AddStreamInfoIfNotPresent(name);
            descriptor.SaveStream = new DataServiceSaveStream(stream, closeStream, args);
            descriptor.State = EntityStates.Modified;
            this.EntityTracker.IncrementChange(descriptor);
        }

        public void SetSaveStream(object entity, string name, Stream stream, bool closeStream, string contentType)
        {
            Util.CheckArgumentNullAndEmpty(contentType, "contentType");
            DataServiceRequestArgs args = new DataServiceRequestArgs {
                ContentType = contentType
            };
            this.SetSaveStream(entity, name, stream, closeStream, args);
        }

        public bool TryGetEntity<TEntity>(Uri identity, out TEntity entity) where TEntity: class
        {
            EntityStates states;
            entity = default(TEntity);
            Util.CheckArgumentNull<Uri>(identity, "relativeUri");
            entity = (TEntity) this.EntityTracker.TryGetEntity(CommonUtil.UriToString(identity), out states);
            return (null != ((TEntity) entity));
        }

        public bool TryGetUri(object entity, out Uri identity)
        {
            identity = null;
            Util.CheckArgumentNull<object>(entity, "entity");
            EntityDescriptor objA = this.entityTracker.TryGetEntityDescriptor(entity);
            if (((objA != null) && (objA.Identity != null)) && object.ReferenceEquals(objA, this.entityTracker.TryGetEntityDescriptor(objA.Identity)))
            {
                string str = objA.Identity;
                identity = Util.CreateUri(str, UriKind.Absolute);
            }
            return (null != identity);
        }

        public void UpdateObject(object entity)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            EntityDescriptor descriptor = this.EntityTracker.TryGetEntityDescriptor(entity);
            if (descriptor == null)
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_EntityNotContained, "entity");
            }
            if (EntityStates.Unchanged == descriptor.State)
            {
                descriptor.State = EntityStates.Modified;
                this.EntityTracker.IncrementChange(descriptor);
            }
        }

        private static void ValidateEntitySetName(ref string entitySetName)
        {
            Util.CheckArgumentNullAndEmpty(entitySetName, "entitySetName");
            entitySetName = entitySetName.Trim(Util.ForwardSlash);
            Util.CheckArgumentNullAndEmpty(entitySetName, "entitySetName");
            Uri requestUri = Util.CreateUri(entitySetName, UriKind.RelativeOrAbsolute);
            if (requestUri.IsAbsoluteUri || !string.IsNullOrEmpty(Util.CreateUri(new Uri("http://ConstBaseUri/ConstService.svc/"), requestUri).GetComponents(UriComponents.Fragment | UriComponents.Query, UriFormat.SafeUnescaped)))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Context_EntitySetName, "entitySetName");
            }
        }

        private static void ValidateEntityType(object entity, DataServiceProtocolVersion maxProtocolVersion)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            if (!ClientTypeUtil.TypeIsEntity(entity.GetType(), maxProtocolVersion))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Content_EntityIsNotEntityType, "entity");
            }
        }

        private static void ValidateEntityTypeHasKeys(object entity, DataServiceProtocolVersion maxProtocolVersion)
        {
            Util.CheckArgumentNull<object>(entity, "entity");
            IEdmType orCreateEdmType = ClientEdmModel.GetModel(maxProtocolVersion).GetOrCreateEdmType(entity.GetType());
            if ((orCreateEdmType.TypeKind != EdmTypeKind.Entity) || !((IEdmEntityType) orCreateEdmType).Key().Any<IEdmStructuralProperty>())
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.Content_EntityWithoutKey, "entity");
            }
        }

        private void ValidateExecuteParameters<TElement>(ref Uri requestUri, string httpMethod, ref bool? singleResult, out List<BodyOperationParameter> bodyOperationParameters, out List<UriOperationParameter> uriOperationParameters, params OperationParameter[] operationParameters)
        {
            if ((string.CompareOrdinal("GET", httpMethod) != 0) && (string.CompareOrdinal("POST", httpMethod) != 0))
            {
                throw new ArgumentException(System.Data.Services.Client.Strings.Context_ExecuteExpectsGetOrPost, "httpMethod");
            }
            if (ClientTypeUtil.TypeOrElementTypeIsEntity(typeof(TElement)))
            {
                singleResult = false;
            }
            if (operationParameters != null)
            {
                ValidateOperationParameters(httpMethod, operationParameters, out bodyOperationParameters, out uriOperationParameters);
            }
            else
            {
                uriOperationParameters = null;
                bodyOperationParameters = null;
            }
            requestUri = this.GetOrCreateAbsoluteUri(requestUri);
        }

        private static void ValidateOperationParameters(string httpMethod, OperationParameter[] parameters, out List<BodyOperationParameter> bodyOperationParameters, out List<UriOperationParameter> uriOperationParameters)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> set2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<UriOperationParameter> source = new List<UriOperationParameter>();
            List<BodyOperationParameter> list2 = new List<BodyOperationParameter>();
            foreach (OperationParameter parameter in parameters)
            {
                if (parameter == null)
                {
                    throw new ArgumentException(System.Data.Services.Client.Strings.Context_NullElementInOperationParameterArray);
                }
                if (string.IsNullOrEmpty(parameter.Name))
                {
                    throw new ArgumentException(System.Data.Services.Client.Strings.Context_MissingOperationParameterName);
                }
                string item = parameter.Name.Trim();
                BodyOperationParameter parameter2 = parameter as BodyOperationParameter;
                if (parameter2 != null)
                {
                    if (string.CompareOrdinal("GET", httpMethod) == 0)
                    {
                        throw new ArgumentException(System.Data.Services.Client.Strings.Context_BodyOperationParametersNotAllowedWithGet);
                    }
                    if (!set2.Add(item))
                    {
                        throw new ArgumentException(System.Data.Services.Client.Strings.Context_DuplicateBodyOperationParameterName);
                    }
                    list2.Add(parameter2);
                }
                else
                {
                    UriOperationParameter parameter3 = parameter as UriOperationParameter;
                    if (parameter3 != null)
                    {
                        if (!set.Add(item))
                        {
                            throw new ArgumentException(System.Data.Services.Client.Strings.Context_DuplicateUriOperationParameterName);
                        }
                        source.Add(parameter3);
                    }
                }
            }
            uriOperationParameters = source.Any<UriOperationParameter>() ? source : null;
            bodyOperationParameters = list2.Any<BodyOperationParameter>() ? list2 : null;
        }

        private void ValidateSaveChangesOptions(SaveChangesOptions options)
        {
            if ((options | (SaveChangesOptions.PatchOnUpdate | SaveChangesOptions.ReplaceOnUpdate | SaveChangesOptions.ContinueOnError | SaveChangesOptions.Batch)) != (SaveChangesOptions.PatchOnUpdate | SaveChangesOptions.ReplaceOnUpdate | SaveChangesOptions.ContinueOnError | SaveChangesOptions.Batch))
            {
                throw System.Data.Services.Client.Error.ArgumentOutOfRange("options");
            }
            if (Util.IsFlagSet(options, SaveChangesOptions.ContinueOnError | SaveChangesOptions.Batch))
            {
                throw System.Data.Services.Client.Error.ArgumentOutOfRange("options");
            }
            if (Util.IsFlagSet(options, SaveChangesOptions.PatchOnUpdate | SaveChangesOptions.ReplaceOnUpdate))
            {
                throw System.Data.Services.Client.Error.ArgumentOutOfRange("options");
            }
            if (Util.IsFlagSet(options, SaveChangesOptions.PatchOnUpdate))
            {
                this.EnsureMinimumProtocolVersionV3();
            }
        }

        public DataServiceResponsePreference AddAndUpdateResponsePreference
        {
            get
            {
                return this.addAndUpdateResponsePreference;
            }
            set
            {
                if (value != DataServiceResponsePreference.None)
                {
                    this.EnsureMinimumProtocolVersionV3();
                }
                this.addAndUpdateResponsePreference = value;
            }
        }

        public bool ApplyingChanges
        {
            get
            {
                return this.applyingChanges;
            }
            internal set
            {
                this.applyingChanges = value;
            }
        }

        public Uri BaseUri
        {
            get
            {
                return this.baseUriResolver.GetRawBaseUriValue();
            }
            set
            {
                this.baseUriResolver = this.baseUriResolver.CloneWithOverrideValue(value, null);
            }
        }

        internal UriResolver BaseUriResolver
        {
            get
            {
                return this.baseUriResolver;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return this.credentials;
            }
            set
            {
                this.credentials = value;
            }
        }

        [Obsolete("You cannot change the default data namespace for an OData service that supports version 3 of the OData protocol, or a later version.", false)]
        public string DataNamespace
        {
            get
            {
                return this.dataNamespace;
            }
            set
            {
                Util.CheckArgumentNull<string>(value, "value");
                if (!string.Equals(value, "http://schemas.microsoft.com/ado/2007/08/dataservices", StringComparison.Ordinal))
                {
                    this.EnsureMaximumProtocolVersionForProperty("DataNamespace", Util.DataServiceVersion2);
                }
                this.dataNamespace = value;
            }
        }

        public ReadOnlyCollection<EntityDescriptor> Entities
        {
            get
            {
                return (from d in this.entityTracker.Entities
                    orderby d.ChangeOrder
                    select d).ToList<EntityDescriptor>().AsReadOnly();
            }
        }

        internal System.Data.Services.Client.EntityTracker EntityTracker
        {
            get
            {
                return this.entityTracker;
            }
        }

        internal bool HasReadingEntityHandlers
        {
            [DebuggerStepThrough]
            get
            {
                return (this.ReadingEntity != null);
            }
        }

        internal bool HasSendingRequest2EventHandlers
        {
            [DebuggerStepThrough]
            get
            {
                return (this.SendingRequest2 != null);
            }
        }

        internal bool HasSendingRequestEventHandlers
        {
            [DebuggerStepThrough]
            get
            {
                return ((this.SendingRequest2 == null) && (this.SendingRequest != null));
            }
        }

        internal bool HasWritingEntityHandlers
        {
            [DebuggerStepThrough]
            get
            {
                return (this.WritingEntity != null);
            }
        }

        public bool IgnoreMissingProperties
        {
            get
            {
                return this.ignoreMissingProperties;
            }
            set
            {
                this.ignoreMissingProperties = value;
            }
        }

        public bool IgnoreResourceNotFoundException
        {
            get
            {
                return this.ignoreResourceNotFoundException;
            }
            set
            {
                this.ignoreResourceNotFoundException = value;
            }
        }

        public ReadOnlyCollection<LinkDescriptor> Links
        {
            get
            {
                return (from l in this.entityTracker.Links
                    orderby l.ChangeOrder
                    select l).ToList<LinkDescriptor>().AsReadOnly();
            }
        }

        public DataServiceProtocolVersion MaxProtocolVersion
        {
            get
            {
                return this.maxProtocolVersion;
            }
        }

        public System.Data.Services.Client.MergeOption MergeOption
        {
            get
            {
                return this.mergeOption;
            }
            set
            {
                this.mergeOption = Util.CheckEnumerationValue(value, "MergeOption");
            }
        }

        public Func<string, Uri> ResolveEntitySet
        {
            get
            {
                return this.baseUriResolver.ResolveEntitySet;
            }
            set
            {
                this.baseUriResolver = this.baseUriResolver.CloneWithOverrideValue(value);
            }
        }

        public Func<Type, string> ResolveName
        {
            get
            {
                return this.resolveName;
            }
            set
            {
                this.resolveName = value;
            }
        }

        public Func<string, Type> ResolveType
        {
            get
            {
                return this.resolveType;
            }
            set
            {
                this.resolveType = value;
            }
        }

        public SaveChangesOptions SaveChangesDefaultOptions
        {
            get
            {
                return this.saveChangesDefaultOptions;
            }
            set
            {
                this.ValidateSaveChangesOptions(value);
                this.saveChangesDefaultOptions = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                if (value < 0)
                {
                    throw System.Data.Services.Client.Error.ArgumentOutOfRange("Timeout");
                }
                this.timeout = value;
            }
        }

        [Obsolete("You cannot change the default type scheme for an OData service that supports version 3 of the OData protocol, or a later version.", false)]
        public Uri TypeScheme
        {
            get
            {
                return this.typeScheme;
            }
            set
            {
                Util.CheckArgumentNull<Uri>(value, "value");
                if (!string.Equals(value.AbsoluteUri, "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme", StringComparison.Ordinal))
                {
                    this.EnsureMaximumProtocolVersionForProperty("TypeScheme", Util.DataServiceVersion2);
                }
                this.typeScheme = value;
            }
        }

        public bool UsePostTunneling
        {
            get
            {
                return this.postTunneling;
            }
            set
            {
                this.postTunneling = value;
            }
        }
    }
}

