namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class BaseSaveResult : BaseAsyncResult
    {
        protected HttpWebResponse batchResponse;
        protected byte[] buildBatchBuffer;
        protected readonly List<Descriptor> ChangedEntries;
        protected int entryIndex;
        protected Stream mediaResourceRequestStream;
        protected readonly SaveChangesOptions Options;
        protected readonly System.Data.Services.Client.RequestInfo RequestInfo;
        protected readonly Serializer SerializerInstance;
        protected StreamRequestKind streamRequestKind;

        internal BaseSaveResult(DataServiceContext context, string method, DataServiceRequest[] queries, SaveChangesOptions options, AsyncCallback callback, object state) : base(context, method, callback, state)
        {
            this.entryIndex = -1;
            this.RequestInfo = new System.Data.Services.Client.RequestInfo(context);
            this.Options = options;
            this.SerializerInstance = new Serializer(this.RequestInfo);
            if (queries == null)
            {
                this.ChangedEntries = (from o in context.EntityTracker.Entities.Cast<Descriptor>().Union<Descriptor>(context.EntityTracker.Links.Cast<Descriptor>()).Union<Descriptor>((from e in context.EntityTracker.Entities select e.StreamDescriptors).Cast<Descriptor>())
                    where o.IsModified && (o.ChangeOrder != uint.MaxValue)
                    orderby o.ChangeOrder
                    select o).ToList<Descriptor>();
                foreach (Descriptor descriptor in this.ChangedEntries)
                {
                    descriptor.ContentGeneratedForSave = false;
                    descriptor.SaveResultWasProcessed = 0;
                    descriptor.SaveError = null;
                    if (descriptor.DescriptorKind == DescriptorKind.Link)
                    {
                        object target = ((LinkDescriptor) descriptor).Target;
                        if (target != null)
                        {
                            Descriptor entityDescriptor = context.EntityTracker.GetEntityDescriptor(target);
                            if (EntityStates.Unchanged == entityDescriptor.State)
                            {
                                entityDescriptor.ContentGeneratedForSave = false;
                                entityDescriptor.SaveResultWasProcessed = 0;
                                entityDescriptor.SaveError = null;
                            }
                        }
                    }
                }
            }
            else
            {
                this.ChangedEntries = new List<Descriptor>();
            }
        }

        protected static void ApplyPreferences(ODataRequestMessageWrapper requestMessage, string method, DataServiceResponsePreference responsePreference, ref Version requestVersion)
        {
            if (((string.CompareOrdinal("POST", method) == 0) || (string.CompareOrdinal("PUT", method) == 0)) || ((string.CompareOrdinal("MERGE", method) == 0) || (string.CompareOrdinal("PATCH", method) == 0)))
            {
                string preferHeaderAndRequestVersion = WebUtil.GetPreferHeaderAndRequestVersion(responsePreference, ref requestVersion);
                if (preferHeaderAndRequestVersion != null)
                {
                    requestMessage.SetHeader("Prefer", preferHeaderAndRequestVersion);
                }
            }
        }

        protected override void AsyncEndGetResponse(IAsyncResult asyncResult)
        {
            BaseAsyncResult.AsyncStateBag asyncState = asyncResult.AsyncState as BaseAsyncResult.AsyncStateBag;
            BaseAsyncResult.PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            DataServiceContext context = (asyncState == null) ? null : asyncState.Context;
            try
            {
                this.CompleteCheck(request, InternalError.InvalidEndGetResponseCompleted);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                BaseAsyncResult.EqualRefCheck(base.perRequest, request, InternalError.InvalidEndGetResponse);
                ODataRequestMessageWrapper wrapper = Util.NullCheck<ODataRequestMessageWrapper>(request.Request, InternalError.InvalidEndGetResponseRequest);
                HttpWebResponse response = null;
                response = WebUtil.EndGetResponse(wrapper, asyncResult, context);
                request.HttpWebResponse = Util.NullCheck<HttpWebResponse>(response, InternalError.InvalidEndGetResponseResponse);
                if (!this.IsBatch)
                {
                    this.HandleOperationResponse(response);
                    this.HandleOperationResponseHeaders(response.StatusCode, WebUtil.WrapResponseHeaders(response));
                }
                Stream responseStream = WebUtil.GetResponseStream(response, context);
                request.ResponseStream = responseStream;
                if ((responseStream != null) && responseStream.CanRead)
                {
                    if (this.buildBatchBuffer == null)
                    {
                        this.buildBatchBuffer = new byte[0x1f40];
                    }
                    do
                    {
                        asyncResult = BaseAsyncResult.InvokeAsync(new BaseAsyncResult.AsyncAction(responseStream.BeginRead), this.buildBatchBuffer, 0, this.buildBatchBuffer.Length, new AsyncCallback(this.AsyncEndRead), new AsyncReadState(request));
                        request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                    }
                    while (((asyncResult.CompletedSynchronously && !request.RequestCompleted) && !base.IsCompletedInternally) && responseStream.CanRead);
                }
                else
                {
                    request.SetComplete();
                    if (!base.IsCompletedInternally && !request.RequestCompletedSynchronously)
                    {
                        this.FinishCurrentChange(request);
                    }
                }
            }
            catch (Exception exception)
            {
                if (base.HandleFailure(request, exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        private void AsyncEndRead(IAsyncResult asyncResult)
        {
            AsyncReadState asyncState = (AsyncReadState) asyncResult.AsyncState;
            BaseAsyncResult.PerRequest pereq = asyncState.Pereq;
            int count = 0;
            try
            {
                this.CompleteCheck(pereq, InternalError.InvalidEndReadCompleted);
                pereq.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                BaseAsyncResult.EqualRefCheck(base.perRequest, pereq, InternalError.InvalidEndRead);
                Stream stream = Util.NullCheck<Stream>(pereq.ResponseStream, InternalError.InvalidEndReadStream);
                count = stream.EndRead(asyncResult);
                if (0 < count)
                {
                    Util.NullCheck<Stream>(this.ResponseStream, InternalError.InvalidEndReadCopy).Write(this.buildBatchBuffer, 0, count);
                    asyncState.TotalByteCopied += count;
                    if (!asyncResult.CompletedSynchronously && stream.CanRead)
                    {
                        do
                        {
                            asyncResult = BaseAsyncResult.InvokeAsync(new BaseAsyncResult.AsyncAction(stream.BeginRead), this.buildBatchBuffer, 0, this.buildBatchBuffer.Length, new AsyncCallback(this.AsyncEndRead), asyncState);
                            pereq.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                            if ((!asyncResult.CompletedSynchronously || pereq.RequestCompleted) || base.IsCompletedInternally)
                            {
                                return;
                            }
                        }
                        while (stream.CanRead);
                    }
                }
                else
                {
                    pereq.SetComplete();
                    if (!base.IsCompletedInternally && !pereq.RequestCompletedSynchronously)
                    {
                        this.FinishCurrentChange(pereq);
                    }
                }
            }
            catch (Exception exception)
            {
                if (base.HandleFailure(pereq, exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(pereq);
            }
        }

        private static bool CanHandleResponseVersion(string responseVersion, out Version parsedResponseVersion)
        {
            parsedResponseVersion = null;
            if (!string.IsNullOrEmpty(responseVersion))
            {
                KeyValuePair<Version, string> pair;
                if (!HttpProcessUtility.TryReadVersion(responseVersion, out pair))
                {
                    return false;
                }
                if (!Util.SupportedResponseVersions.Contains<Version>(pair.Key))
                {
                    return false;
                }
                parsedResponseVersion = pair.Key;
            }
            return true;
        }

        protected override void CompletedRequest()
        {
            this.buildBatchBuffer = null;
        }

        protected bool CreateChangeData(int index, ODataRequestMessageWrapper requestMessage)
        {
            Descriptor descriptor = this.ChangedEntries[index];
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                EntityDescriptor entityDescriptor = (EntityDescriptor) descriptor;
                descriptor.ContentGeneratedForSave = true;
                return this.CreateRequestData(entityDescriptor, requestMessage);
            }
            descriptor.ContentGeneratedForSave = true;
            LinkDescriptor binding = (LinkDescriptor) descriptor;
            if ((EntityStates.Added != binding.State) && ((EntityStates.Modified != binding.State) || (binding.Target == null)))
            {
                return false;
            }
            this.CreateRequestData(binding, requestMessage);
            return true;
        }

        protected ODataRequestMessageWrapper CreateRequest(EntityDescriptor entityDescriptor)
        {
            EntityStates state = entityDescriptor.State;
            Uri resourceUri = entityDescriptor.GetResourceUri(this.RequestInfo.BaseUriResolver, false);
            ClientEdmModel model = ClientEdmModel.GetModel(this.RequestInfo.MaxProtocolVersion);
            Version requestVersion = DetermineRequestVersion(model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityDescriptor.Entity.GetType())), state);
            string httpMethod = this.GetHttpMethod(state, ref requestVersion);
            ODataRequestMessageWrapper requestMessage = this.CreateRequestMessage(resourceUri, httpMethod);
            if (this.IsBatch)
            {
                requestMessage.SetHeader("Content-ID", entityDescriptor.ChangeOrder.ToString(CultureInfo.InvariantCulture));
            }
            if (EntityStates.Deleted != entityDescriptor.State)
            {
                requestMessage.SetHeader("Content-Type", "application/atom+xml");
            }
            if ((EntityStates.Deleted == state) || (EntityStates.Modified == state))
            {
                string latestETag = entityDescriptor.GetLatestETag();
                if (latestETag != null)
                {
                    requestMessage.SetHeader("If-Match", latestETag);
                    if (!this.IsBatch)
                    {
                        requestMessage.AddHeadersToReset("If-Match");
                    }
                }
            }
            ApplyPreferences(requestMessage, httpMethod, this.RequestInfo.AddAndUpdateResponsePreference, ref requestVersion);
            WebUtil.SetOperationVersionHeaders(requestMessage, requestVersion, this.RequestInfo.MaxProtocolVersionAsVersion);
            return requestMessage;
        }

        protected ODataRequestMessageWrapper CreateRequest(LinkDescriptor binding)
        {
            if (binding.ContentGeneratedForSave)
            {
                return null;
            }
            EntityDescriptor entityDescriptor = this.RequestInfo.EntityTracker.GetEntityDescriptor(binding.Source);
            EntityDescriptor descriptor2 = (binding.Target != null) ? this.RequestInfo.EntityTracker.GetEntityDescriptor(binding.Target) : null;
            Uri requestUri = null;
            if (entityDescriptor.GetLatestIdentity() == null)
            {
                if (!this.IsBatch)
                {
                    binding.ContentGeneratedForSave = true;
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_LinkResourceInsertFailure, entityDescriptor.SaveError);
                }
                Uri uri = this.CreateRequestRelativeUri(binding);
                requestUri = Util.CreateUri("$" + entityDescriptor.ChangeOrder.ToString(CultureInfo.InvariantCulture) + "/" + CommonUtil.UriToString(uri), UriKind.Relative);
            }
            else
            {
                if ((!this.IsBatch && (descriptor2 != null)) && (descriptor2.GetLatestIdentity() == null))
                {
                    binding.ContentGeneratedForSave = true;
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_LinkResourceInsertFailure, descriptor2.SaveError);
                }
                requestUri = this.CreateRequestUri(entityDescriptor, binding);
            }
            ODataRequestMessageWrapper requestMessage = this.CreateRequestMessage(requestUri, GetLinkHttpMethod(binding));
            if (this.IsBatch)
            {
                requestMessage.SetHeader("Content-ID", binding.ChangeOrder.ToString(CultureInfo.InvariantCulture));
            }
            if ((EntityStates.Added == binding.State) || ((EntityStates.Modified == binding.State) && (binding.Target != null)))
            {
                requestMessage.SetHeader("Content-Type", "application/xml");
            }
            WebUtil.SetOperationVersionHeaders(requestMessage, Util.DataServiceVersion1, this.RequestInfo.MaxProtocolVersionAsVersion);
            return requestMessage;
        }

        private bool CreateRequestData(EntityDescriptor entityDescriptor, ODataRequestMessageWrapper requestMessage)
        {
            bool flag = false;
            EntityStates state = entityDescriptor.State;
            if (state != EntityStates.Added)
            {
                if (state == EntityStates.Deleted)
                {
                    goto Label_0020;
                }
                if (state != EntityStates.Modified)
                {
                    System.Data.Services.Client.Error.ThrowInternalError(InternalError.UnvalidatedEntityState);
                    goto Label_0020;
                }
            }
            flag = true;
        Label_0020:
            if (flag)
            {
                this.SerializerInstance.WriteEntry(entityDescriptor, this.RelatedLinks(entityDescriptor), requestMessage);
            }
            return flag;
        }

        private void CreateRequestData(LinkDescriptor binding, ODataRequestMessageWrapper requestMessage)
        {
            this.SerializerInstance.WriteEntityReferenceLink(binding, requestMessage);
        }

        protected abstract ODataRequestMessageWrapper CreateRequestMessage(Uri requestUri, string method);
        protected Uri CreateRequestRelativeUri(LinkDescriptor binding)
        {
            if (binding.IsSourcePropertyCollection && (EntityStates.Added != binding.State))
            {
                EntityDescriptor entityDescriptor = this.RequestInfo.EntityTracker.GetEntityDescriptor(binding.Target);
                Uri uri = DataServiceContext.GenerateEditLinkRelativeUri(binding.SourceProperty, entityDescriptor.Entity, this.RequestInfo.MaxProtocolVersion);
                return Util.CreateUri("$links" + '/' + CommonUtil.UriToString(uri), UriKind.Relative);
            }
            return Util.CreateUri("$links" + '/' + binding.SourceProperty, UriKind.Relative);
        }

        protected Uri CreateRequestUri(EntityDescriptor sourceResource, LinkDescriptor binding)
        {
            Uri uri;
            LinkInfo linkInfo = null;
            if (sourceResource.TryGetLinkInfo(binding.SourceProperty, out linkInfo) && ((uri = linkInfo.AssociationLink) != null))
            {
                if (binding.IsSourcePropertyCollection && (EntityStates.Deleted == binding.State))
                {
                    EntityDescriptor entityDescriptor = this.RequestInfo.EntityTracker.GetEntityDescriptor(binding.Target);
                    uri = DataServiceContext.AppendKeysToUri(uri.AbsoluteUri, entityDescriptor.Entity, UriKind.Absolute, this.RequestInfo.MaxProtocolVersion);
                }
                return uri;
            }
            return Util.CreateUri(sourceResource.GetResourceUri(this.RequestInfo.BaseUriResolver, false), this.CreateRequestRelativeUri(binding));
        }

        protected ResponseInfo CreateResponseInfo(EntityDescriptor entityDescriptor)
        {
            MergeOption overwriteChanges = MergeOption.OverwriteChanges;
            if (entityDescriptor.StreamState == EntityStates.Added)
            {
                overwriteChanges = MergeOption.PreserveChanges;
            }
            return this.RequestInfo.GetDeserializationInfo(new MergeOption?(overwriteChanges));
        }

        internal static BaseSaveResult CreateSaveResult(DataServiceContext context, string method, DataServiceRequest[] queries, SaveChangesOptions options, AsyncCallback callback, object state)
        {
            if ((options & SaveChangesOptions.Batch) != SaveChangesOptions.Batch)
            {
                return new SaveResult(context, method, options, callback, state);
            }
            return new BatchSaveResult(context, method, queries, options, callback, state);
        }

        private static Version DetermineRequestVersion(ClientTypeAnnotation clientType, EntityStates state)
        {
            if (state == EntityStates.Deleted)
            {
                return Util.DataServiceVersion1;
            }
            Version version = Util.DataServiceVersion1;
            WebUtil.RaiseVersion(ref version, clientType.GetMetadataVersion());
            WebUtil.RaiseVersion(ref version, clientType.EpmMinimumDataServiceProtocolVersion.ToVersion());
            return version;
        }

        internal DataServiceResponse EndRequest()
        {
            foreach (Descriptor descriptor in this.ChangedEntries)
            {
                descriptor.ClearChanges();
            }
            return this.HandleResponse();
        }

        protected string GetHttpMethod(EntityStates state, ref Version requestVersion)
        {
            EntityStates states = state;
            if (states == EntityStates.Added)
            {
                return "POST";
            }
            if (states != EntityStates.Deleted)
            {
                if (states != EntityStates.Modified)
                {
                    throw System.Data.Services.Client.Error.InternalError(InternalError.UnvalidatedEntityState);
                }
            }
            else
            {
                return "DELETE";
            }
            if (Util.IsFlagSet(this.Options, SaveChangesOptions.ReplaceOnUpdate))
            {
                return "PUT";
            }
            if (Util.IsFlagSet(this.Options, SaveChangesOptions.PatchOnUpdate))
            {
                WebUtil.RaiseVersion(ref requestVersion, Util.DataServiceVersion3);
                return "PATCH";
            }
            return "MERGE";
        }

        protected static string GetLinkHttpMethod(LinkDescriptor link)
        {
            if (!link.IsSourcePropertyCollection)
            {
                if (link.Target == null)
                {
                    return "DELETE";
                }
                return "PUT";
            }
            if (EntityStates.Deleted == link.State)
            {
                return "DELETE";
            }
            return "POST";
        }

        protected abstract MaterializeAtom GetMaterializer(EntityDescriptor entityDescriptor, ResponseInfo responseInfo);
        internal static DataServiceClientException GetResponseText(Func<Stream> getResponseStream, HttpStatusCode statusCode)
        {
            string str = null;
            using (Stream stream = getResponseStream())
            {
                if ((stream != null) && stream.CanRead)
                {
                    str = new StreamReader(stream).ReadToEnd();
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = statusCode.ToString();
            }
            return new DataServiceClientException(str, (int) statusCode);
        }

        protected override void HandleCompleted(BaseAsyncResult.PerRequest pereq)
        {
            if (pereq != null)
            {
                base.SetCompletedSynchronously(pereq.RequestCompletedSynchronously);
                if (pereq.RequestCompleted)
                {
                    Interlocked.CompareExchange<BaseAsyncResult.PerRequest>(ref this.perRequest, null, pereq);
                    if (this.IsBatch)
                    {
                        Interlocked.CompareExchange<HttpWebResponse>(ref this.batchResponse, pereq.HttpWebResponse, null);
                        pereq.HttpWebResponse = null;
                    }
                    pereq.Dispose();
                }
            }
            base.HandleCompleted();
        }

        protected abstract void HandleOperationResponse(HttpWebResponse response);
        protected void HandleOperationResponse(Descriptor descriptor, Dictionary<string, string> contentHeaders)
        {
            EntityStates unchanged = EntityStates.Unchanged;
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                EntityDescriptor descriptor2 = (EntityDescriptor) descriptor;
                unchanged = descriptor2.StreamState;
            }
            if ((unchanged == EntityStates.Added) || (descriptor.State == EntityStates.Added))
            {
                this.HandleResponsePost(descriptor, contentHeaders);
            }
            else if ((unchanged == EntityStates.Modified) || (descriptor.State == EntityStates.Modified))
            {
                this.HandleResponsePut(descriptor, contentHeaders);
            }
            else if (descriptor.State == EntityStates.Deleted)
            {
                this.HandleResponseDelete(descriptor);
            }
        }

        protected void HandleOperationResponseHeaders(HttpStatusCode statusCode, Dictionary<string, string> headers)
        {
            Descriptor descriptor = this.ChangedEntries[this.entryIndex];
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                EntityDescriptor descriptor2 = (EntityDescriptor) descriptor;
                if ((((descriptor.State == EntityStates.Added) || (this.streamRequestKind == StreamRequestKind.PostMediaResource)) || Util.IsFlagSet(this.Options, SaveChangesOptions.PatchOnUpdate)) && WebUtil.SuccessStatusCode(statusCode))
                {
                    string str;
                    string str2;
                    Uri editLink = null;
                    headers.TryGetValue("Location", out str);
                    headers.TryGetValue("DataServiceId", out str2);
                    if (str != null)
                    {
                        editLink = WebUtil.ValidateLocationHeader(str);
                    }
                    else if ((descriptor.State == EntityStates.Added) || (this.streamRequestKind == StreamRequestKind.PostMediaResource))
                    {
                        throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Deserialize_NoLocationHeader);
                    }
                    if (str2 != null)
                    {
                        if (str == null)
                        {
                            throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Context_BothLocationAndIdMustBeSpecified);
                        }
                        WebUtil.ValidateIdentityValue(str2);
                    }
                    else
                    {
                        str2 = str;
                    }
                    if (null != editLink)
                    {
                        this.RequestInfo.EntityTracker.AttachLocation(descriptor2.Entity, str2, editLink);
                    }
                }
                if (this.streamRequestKind != StreamRequestKind.None)
                {
                    if (!WebUtil.SuccessStatusCode(statusCode))
                    {
                        if (this.streamRequestKind == StreamRequestKind.PostMediaResource)
                        {
                            descriptor.State = EntityStates.Added;
                        }
                        this.streamRequestKind = StreamRequestKind.None;
                        descriptor.ContentGeneratedForSave = true;
                    }
                    else
                    {
                        string str3;
                        if ((this.streamRequestKind == StreamRequestKind.PostMediaResource) && headers.TryGetValue("ETag", out str3))
                        {
                            descriptor2.ETag = str3;
                        }
                    }
                }
            }
        }

        protected abstract DataServiceResponse HandleResponse();
        internal static InvalidOperationException HandleResponse(System.Data.Services.Client.RequestInfo requestInfo, HttpStatusCode statusCode, string responseVersion, Func<Stream> getResponseStream, bool throwOnFailure, out Version parsedResponseVersion)
        {
            InvalidOperationException responseText = null;
            if (!CanHandleResponseVersion(responseVersion, out parsedResponseVersion))
            {
                responseText = System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_VersionNotSupported(responseVersion, Serializer.SerializeSupportedVersions()));
            }
            if (responseText == null)
            {
                responseText = requestInfo.ValidateResponseVersion(parsedResponseVersion);
            }
            if ((responseText == null) && !WebUtil.SuccessStatusCode(statusCode))
            {
                responseText = GetResponseText(getResponseStream, statusCode);
            }
            if ((responseText != null) && throwOnFailure)
            {
                throw responseText;
            }
            return responseText;
        }

        private void HandleResponseDelete(Descriptor descriptor)
        {
            if (EntityStates.Deleted != descriptor.State)
            {
                System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.EntityNotDeleted);
            }
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                EntityDescriptor resource = (EntityDescriptor) descriptor;
                this.RequestInfo.EntityTracker.DetachResource(resource);
            }
            else
            {
                this.RequestInfo.EntityTracker.DetachExistingLink((LinkDescriptor) descriptor, false);
            }
        }

        private static void HandleResponsePost(LinkDescriptor linkDescriptor)
        {
            if ((EntityStates.Added != linkDescriptor.State) && ((EntityStates.Modified != linkDescriptor.State) || (linkDescriptor.Target == null)))
            {
                System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.LinkNotAddedState);
            }
            linkDescriptor.State = EntityStates.Unchanged;
        }

        private void HandleResponsePost(Descriptor descriptor, Dictionary<string, string> contentHeaders)
        {
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                string str;
                contentHeaders.TryGetValue("ETag", out str);
                this.HandleResponsePost((EntityDescriptor) descriptor, str);
            }
            else
            {
                HandleResponsePost((LinkDescriptor) descriptor);
            }
        }

        private void HandleResponsePost(EntityDescriptor entityDescriptor, string etag)
        {
            try
            {
                if ((EntityStates.Added != entityDescriptor.State) && (EntityStates.Added != entityDescriptor.StreamState))
                {
                    System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.EntityNotAddedState);
                }
                if (this.ProcessResponsePayload)
                {
                    this.MaterializeResponse(entityDescriptor, this.CreateResponseInfo(entityDescriptor), etag);
                }
                else
                {
                    entityDescriptor.ETag = etag;
                    entityDescriptor.State = EntityStates.Unchanged;
                }
                if (entityDescriptor.StreamState != EntityStates.Added)
                {
                    foreach (LinkDescriptor descriptor in this.RelatedLinks(entityDescriptor))
                    {
                        if (Util.IncludeLinkState(descriptor.SaveResultWasProcessed) || (descriptor.SaveResultWasProcessed == EntityStates.Added))
                        {
                            HandleResponsePost(descriptor);
                        }
                    }
                }
            }
            finally
            {
                if (entityDescriptor.StreamState == EntityStates.Added)
                {
                    entityDescriptor.State = EntityStates.Modified;
                    entityDescriptor.StreamState = EntityStates.Unchanged;
                }
            }
        }

        private void HandleResponsePut(Descriptor descriptor, Dictionary<string, string> responseHeaders)
        {
            if (descriptor.DescriptorKind == DescriptorKind.Entity)
            {
                string str;
                responseHeaders.TryGetValue("ETag", out str);
                EntityDescriptor entityDescriptor = (EntityDescriptor) descriptor;
                if (this.ProcessResponsePayload)
                {
                    this.MaterializeResponse(entityDescriptor, this.CreateResponseInfo(entityDescriptor), str);
                }
                else
                {
                    if ((EntityStates.Modified != entityDescriptor.State) && (EntityStates.Modified != entityDescriptor.StreamState))
                    {
                        System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.EntryNotModified);
                    }
                    if (entityDescriptor.StreamState == EntityStates.Modified)
                    {
                        entityDescriptor.StreamETag = str;
                        entityDescriptor.StreamState = EntityStates.Unchanged;
                    }
                    else
                    {
                        entityDescriptor.ETag = str;
                        entityDescriptor.State = EntityStates.Unchanged;
                    }
                }
            }
            else if (descriptor.DescriptorKind == DescriptorKind.Link)
            {
                if ((EntityStates.Added == descriptor.State) || (EntityStates.Modified == descriptor.State))
                {
                    descriptor.State = EntityStates.Unchanged;
                }
                else if (EntityStates.Detached != descriptor.State)
                {
                    System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.LinkBadState);
                }
            }
            else
            {
                string str2;
                descriptor.State = EntityStates.Unchanged;
                StreamDescriptor descriptor3 = (StreamDescriptor) descriptor;
                responseHeaders.TryGetValue("ETag", out str2);
                descriptor3.ETag = str2;
            }
        }

        private void MaterializeResponse(EntityDescriptor entityDescriptor, ResponseInfo responseInfo, string etag)
        {
            using (MaterializeAtom atom = this.GetMaterializer(entityDescriptor, responseInfo))
            {
                atom.SetInsertingObject(entityDescriptor.Entity);
                object obj2 = null;
                foreach (object obj3 in atom)
                {
                    if (obj2 != null)
                    {
                        System.Data.Services.Client.Error.ThrowInternalError(InternalError.MaterializerReturningMoreThanOneEntity);
                    }
                    obj2 = obj3;
                }
                if (entityDescriptor.GetLatestETag() == null)
                {
                    entityDescriptor.ETag = etag;
                }
            }
        }

        protected IEnumerable<LinkDescriptor> RelatedLinks(EntityDescriptor entityDescriptor)
        {
            foreach (LinkDescriptor iteratorVariable0 in this.RequestInfo.EntityTracker.Links)
            {
                if ((iteratorVariable0.Source == entityDescriptor.Entity) && (iteratorVariable0.Target != null))
                {
                    EntityDescriptor iteratorVariable1 = this.RequestInfo.EntityTracker.GetEntityDescriptor(iteratorVariable0.Target);
                    if ((Util.IncludeLinkState(iteratorVariable1.SaveResultWasProcessed) || ((iteratorVariable1.SaveResultWasProcessed == 0) && Util.IncludeLinkState(iteratorVariable1.State))) || (((iteratorVariable1.Identity != null) && (iteratorVariable1.ChangeOrder < entityDescriptor.ChangeOrder)) && (((iteratorVariable1.SaveResultWasProcessed == 0) && (EntityStates.Added == iteratorVariable1.State)) || (EntityStates.Added == iteratorVariable1.SaveResultWasProcessed))))
                    {
                        yield return iteratorVariable0;
                    }
                }
            }
        }

        protected int SaveResultProcessed(Descriptor descriptor)
        {
            descriptor.SaveResultWasProcessed = descriptor.State;
            int num = 0;
            if ((descriptor.DescriptorKind == DescriptorKind.Entity) && (EntityStates.Added == descriptor.State))
            {
                foreach (LinkDescriptor descriptor2 in this.RelatedLinks((EntityDescriptor) descriptor))
                {
                    if (descriptor2.ContentGeneratedForSave)
                    {
                        descriptor2.SaveResultWasProcessed = descriptor2.State;
                        num++;
                    }
                }
            }
            return num;
        }

        internal abstract bool IsBatch { get; }

        protected abstract bool ProcessResponsePayload { get; }

        protected abstract Stream ResponseStream { get; }

        
        [StructLayout(LayoutKind.Sequential)]
        private struct AsyncReadState
        {
            internal readonly BaseAsyncResult.PerRequest Pereq;
            private int totalByteCopied;
            internal AsyncReadState(BaseAsyncResult.PerRequest pereq)
            {
                this.Pereq = pereq;
                this.totalByteCopied = 0;
            }

            internal int TotalByteCopied
            {
                get
                {
                    return this.totalByteCopied;
                }
                set
                {
                    this.totalByteCopied = value;
                }
            }
        }

        protected enum StreamRequestKind
        {
            None,
            PostMediaResource,
            PutMediaResource
        }
    }
}

