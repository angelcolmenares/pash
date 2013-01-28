namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal abstract class Deserializer : IDisposable
    {
        private System.Data.Services.RequestDescription description;
        private int objectCount;
        private int recursionDepth;
        protected const int RecursionLimit = 100;
        private readonly IDataService service;
        private readonly UpdateTracker tracker;
        private readonly bool update;

        internal Deserializer(Deserializer parent)
        {
            this.recursionDepth = parent.recursionDepth;
            this.service = parent.service;
            this.tracker = parent.tracker;
            this.update = parent.update;
            this.description = parent.description;
        }

        internal Deserializer(bool update, IDataService dataService, UpdateTracker tracker, System.Data.Services.RequestDescription requestDescription)
        {
            this.service = dataService;
            this.tracker = tracker;
            this.update = update;
            this.description = requestDescription;
        }

        protected void CheckAndIncrementObjectCount()
        {
            if (!this.Update)
            {
                this.objectCount++;
                if (this.objectCount > this.Service.Configuration.MaxObjectCountOnInsert)
                {
                    throw new DataServiceException(0x19d, System.Data.Services.Strings.BadRequest_ExceedsMaxObjectCountOnInsert(this.Service.Configuration.MaxObjectCountOnInsert));
                }
            }
        }

        protected static void CheckForBindingInPutOperations(HttpVerbs requestVerb)
        {
            if (requestVerb == HttpVerbs.PUT)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_CannotUpdateRelatedEntitiesInPut);
            }
        }

        internal static Deserializer CreateDeserializer(System.Data.Services.RequestDescription description, IDataService dataService, bool update, UpdateTracker tracker)
        {
            string str;
            Encoding encoding;
            HttpProcessUtility.ReadContentType(dataService.OperationContext.Host.RequestContentType, out str, out encoding);
            if (DataServiceActionProviderWrapper.IsServiceActionRequest(description))
            {
                return new ParameterDeserializer(update, dataService, tracker, description);
            }
            if ((description.TargetKind == RequestTargetKind.OpenPropertyValue) || (description.TargetKind == RequestTargetKind.PrimitiveValue))
            {
                return new RawValueDeserializer(update, dataService, tracker, description);
            }
            if (description.TargetKind == RequestTargetKind.MediaResource)
            {
                return new MediaResourceDeserializer(update, dataService, tracker, description);
            }
            if (((description.TargetKind == RequestTargetKind.Primitive) || (description.TargetKind == RequestTargetKind.ComplexObject)) || ((description.TargetKind == RequestTargetKind.Collection) || (description.TargetKind == RequestTargetKind.OpenProperty)))
            {
                return new PropertyDeserializer(update, dataService, tracker, description);
            }
            if (description.LinkUri)
            {
                return new EntityReferenceLinkDeserializer(update, dataService, tracker, description);
            }
            if (description.TargetKind != RequestTargetKind.Resource)
            {
				throw new DataServiceException(0x19f, System.Data.Services.Strings.RequestUriProcessor_MethodNotAllowed);
            }
            return new EntityDeserializer(update, dataService, tracker, description);
        }

        internal static object CreateMediaLinkEntry(string fullTypeName, Stream requestStream, IDataService service, System.Data.Services.RequestDescription description, UpdateTracker tracker)
        {
            object target = service.Updatable.CreateResource(description.LastSegmentInfo.TargetContainer.Name, fullTypeName);
            tracker.TrackAction(target, description.LastSegmentInfo.TargetContainer, UpdateOperations.Add);
            SetStreamPropertyValue(target, requestStream, service, description);
            return target;
        }

        internal static IList CreateNewCollection()
        {
            return new List<object>();
        }

        protected static System.Data.Services.SegmentInfo CreateSegment(ResourceProperty property, string propertyName, ResourceSetWrapper propertySet, bool singleResult)
        {
            System.Data.Services.SegmentInfo info = new System.Data.Services.SegmentInfo {
                TargetSource = RequestTargetSource.Property,
                SingleResult = singleResult,
                Identifier = propertyName
            };
            if (property == null)
            {
                info.TargetKind = RequestTargetKind.OpenProperty;
                return info;
            }
            info.TargetKind = RequestTargetKind.Resource;
            info.Identifier = propertyName;
            info.ProjectedProperty = property;
            info.TargetResourceType = property.ResourceType;
            info.TargetContainer = propertySet;
            return info;
        }

        protected abstract object Deserialize(System.Data.Services.SegmentInfo segmentInfo);
        protected virtual void Dispose(bool disposing)
        {
        }

        protected CollectionResourceType GetCollectionTypeFromName(string typeName, string propertyName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidTypeName(typeName));
            }
            string collectionItemTypeName = CommonUtil.GetCollectionItemTypeName(typeName, false);
            if (collectionItemTypeName == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_CollectionTypeExpected(typeName, propertyName));
            }
            ResourceType itemType = WebUtil.TryResolveResourceType(this.Service.Provider, collectionItemTypeName);
            if (itemType == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidTypeName(typeName));
            }
            return ResourceType.GetCollectionResourceType(itemType);
        }

        internal static object GetCrossReferencedResource(System.Data.Services.SegmentInfo segmentInfo)
        {
            object[] requestEnumerable = (object[]) segmentInfo.RequestEnumerable;
            return requestEnumerable[0];
        }

        internal static object GetEntityResourceToModify(System.Data.Services.RequestDescription description, IDataService service, bool allowCrossReferencing, out ResourceSetWrapper entityContainer)
        {
            int num;
            return GetEntityResourceToModify(description, service, allowCrossReferencing, out entityContainer, out num);
        }

        private static object GetEntityResourceToModify(System.Data.Services.RequestDescription description, IDataService service, bool allowCrossReferencing, out ResourceSetWrapper entityContainer, out int entityResourceIndex)
        {
            if (!allowCrossReferencing && (description.RequestExpression == null))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ResourceCanBeCrossReferencedOnlyForBindOperation);
            }
            entityResourceIndex = GetIndexOfEntityResourceToModify(description);
            entityContainer = description.SegmentInfos[entityResourceIndex].TargetContainer;
            DataServiceHostWrapper host = service.OperationContext.Host;
            if (host.HttpVerb == HttpVerbs.PUT)
            {
                DataServiceConfiguration.CheckResourceRights(entityContainer, EntitySetRights.WriteReplace);
            }
            else if ((host.HttpVerb == HttpVerbs.MERGE) || (host.HttpVerb == HttpVerbs.PATCH))
            {
                DataServiceConfiguration.CheckResourceRights(entityContainer, EntitySetRights.WriteMerge);
            }
            else
            {
                DataServiceConfiguration.CheckResourceRights(entityContainer, EntitySetRights.WriteMerge | EntitySetRights.WriteReplace);
            }
            object obj2 = service.GetResource(description, entityResourceIndex, null);
            if (obj2 == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_DereferencingNullPropertyValue(description.SegmentInfos[entityResourceIndex].Identifier));
            }
            return obj2;
        }

        internal static int GetIndexOfEntityResourceToModify(System.Data.Services.RequestDescription description)
        {
            if (description.LinkUri)
            {
                return (description.SegmentInfos.Length - 3);
            }
            for (int i = description.SegmentInfos.Length - 1; i >= 0; i--)
            {
                if ((description.SegmentInfos[i].TargetKind == RequestTargetKind.Resource) && description.SegmentInfos[i].SingleResult)
                {
                    return i;
                }
            }
            return -1;
        }

        protected object GetObjectFromSegmentInfo(ResourceType resourceType, System.Data.Services.SegmentInfo segmentInfo, bool verifyETag, bool checkForNull, bool replaceResource)
        {
            object crossReferencedResource;
            if (segmentInfo.RequestExpression == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ResourceCanBeCrossReferencedOnlyForBindOperation);
            }
            if (IsCrossReferencedSegment(segmentInfo, this.service))
            {
                crossReferencedResource = GetCrossReferencedResource(segmentInfo);
            }
            else
            {
                crossReferencedResource = GetResource(segmentInfo, (resourceType != null) ? resourceType.FullName : null, this.Service, checkForNull);
                if (verifyETag)
                {
                    this.service.Updatable.SetETagValues(crossReferencedResource, segmentInfo.TargetContainer);
                }
            }
            if (replaceResource)
            {
                crossReferencedResource = this.Updatable.ResetResource(crossReferencedResource);
                WebUtil.CheckResourceExists(crossReferencedResource != null, segmentInfo.Identifier);
            }
            return crossReferencedResource;
        }

        internal static object GetReadOnlyCollection(IList collection)
        {
            return new CollectionPropertyValueEnumerable(collection);
        }

        internal static object GetResource(System.Data.Services.SegmentInfo segmentInfo, string fullTypeName, IDataService service, bool checkForNull)
        {
            if (segmentInfo.TargetContainer != null)
            {
                DataServiceConfiguration.CheckResourceRights(segmentInfo.TargetContainer, EntitySetRights.ReadSingle);
            }
            segmentInfo.RequestEnumerable = (IEnumerable) service.ExecutionProvider.Execute(segmentInfo.RequestExpression);
            object resource = service.Updatable.GetResource((IQueryable) segmentInfo.RequestEnumerable, fullTypeName);
            if ((resource == null) && (segmentInfo.HasKeyValues || checkForNull))
            {
                throw DataServiceException.CreateResourceNotFound(segmentInfo.Identifier);
            }
            return resource;
        }

        internal static object GetResourceToModify(System.Data.Services.RequestDescription description, IDataService service, bool allowCrossReferencing, out object entityResource, out ResourceSetWrapper entityContainer, bool checkETag)
        {
            int num;
            int num2;
            if ((description.TargetKind == RequestTargetKind.OpenPropertyValue) || (description.TargetKind == RequestTargetKind.PrimitiveValue))
            {
                num = description.SegmentInfos.Length - 3;
            }
            else
            {
                num = description.SegmentInfos.Length - 2;
            }
            entityResource = GetEntityResourceToModify(description, service, allowCrossReferencing, out entityContainer, out num2);
            object targetResource = entityResource;
            for (int i = num2 + 1; i <= num; i++)
            {
                if (!description.SegmentInfos[i].IsTypeIdentifierSegment)
                {
                    targetResource = service.Updatable.GetValue(targetResource, description.SegmentInfos[i].Identifier);
                }
            }
            if ((checkETag && !IsCrossReferencedSegment(description.SegmentInfos[num], service)) && (description.TargetKind != RequestTargetKind.MediaResource))
            {
                service.Updatable.SetETagValues(entityResource, entityContainer);
            }
            return targetResource;
        }

        protected object GetTargetResourceToBind(string uri, bool checkNull)
        {
			Uri absoluteUriFromReference = RequestUriProcessor.GetAbsoluteUriFromReference(uri, this.Service.OperationContext);
            return this.GetTargetResourceToBind(absoluteUriFromReference, checkNull);
        }

        protected object GetTargetResourceToBind(Uri referencedUri, bool checkNull)
        {
			System.Data.Services.RequestDescription description = RequestUriProcessor.ProcessRequestUri(referencedUri, this.Service, true);
            object obj2 = this.Service.GetResource(description, description.SegmentInfos.Length - 1, null);
            if (checkNull)
            {
                WebUtil.CheckResourceExists(obj2 != null, description.LastSegmentInfo.Identifier);
            }
            return obj2;
        }

        internal static object HandleBindOperation(System.Data.Services.RequestDescription description, object linkResource, IDataService service, UpdateTracker tracker)
        {
            ResourceSetWrapper wrapper;
            object target = GetEntityResourceToModify(description, service, true, out wrapper);
            description.UpdateAndCheckEpmFeatureVersion(wrapper, service);
            tracker.TrackAction(target, wrapper, UpdateOperations.Change);
            if (description.IsSingleResult)
            {
                service.Updatable.SetReference(target, description.Property.Name, linkResource);
                return target;
            }
            service.Updatable.AddReferenceToCollection(target, description.Property.Name, linkResource);
            return target;
        }

        internal object HandlePostRequest()
        {
            object targetResourceToBind;
            System.Data.Services.RequestDescription requestDescription = this.RequestDescription;
            if (requestDescription.LinkUri)
            {
                Uri referencedUri = (Uri) this.Deserialize(null);
                targetResourceToBind = this.GetTargetResourceToBind(referencedUri, true);
                HandleBindOperation(requestDescription, targetResourceToBind, this.Service, this.Tracker);
                return targetResourceToBind;
            }
            if (requestDescription.LastSegmentInfo.TargetContainer != null)
            {
                DataServiceConfiguration.CheckResourceRights(requestDescription.LastSegmentInfo.TargetContainer, EntitySetRights.WriteAppend);
            }
            targetResourceToBind = this.ReadEntity();
            if (requestDescription.TargetSource == RequestTargetSource.Property)
            {
                HandleBindOperation(requestDescription, targetResourceToBind, this.Service, this.Tracker);
                return targetResourceToBind;
            }
            this.Tracker.TrackAction(targetResourceToBind, requestDescription.LastSegmentInfo.TargetContainer, UpdateOperations.Add);
            return targetResourceToBind;
        }

        internal object HandlePutRequest()
        {
            object requestValue = null;
            object entityResource = null;
            ResourceSetWrapper entityContainer = null;
            string str;
            Encoding encoding;
            HttpProcessUtility.ReadContentType(this.Service.OperationContext.Host.RequestContentType, out str, out encoding);
            System.Data.Services.RequestDescription requestDescription = this.RequestDescription;
            if (((requestDescription.TargetKind == RequestTargetKind.MediaResource) || (requestDescription.TargetKind == RequestTargetKind.OpenPropertyValue)) || (requestDescription.TargetKind == RequestTargetKind.PrimitiveValue))
            {
                requestValue = this.Deserialize(requestDescription.LastSegmentInfo);
            }
            else if (requestDescription.LinkUri)
            {
                Uri referencedUri = (Uri) this.Deserialize(null);
                object targetResourceToBind = this.GetTargetResourceToBind(referencedUri, true);
                entityResource = HandleBindOperation(requestDescription, targetResourceToBind, this.Service, this.Tracker);
                entityContainer = requestDescription.LastSegmentInfo.TargetContainer;
            }
            else
            {
                requestValue = this.ReadEntity();
                if (((requestValue == null) && requestDescription.LastSegmentInfo.HasKeyValues) && (requestDescription.TargetSource == RequestTargetSource.EntitySet))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_CannotSetTopLevelResourceToNull(requestDescription.ResultUri.AbsoluteUri));
                }
            }
            if (!requestDescription.LinkUri && IsQueryRequired(requestDescription, requestValue))
            {
                object resourceToBeModified = GetResourceToModify(requestDescription, this.Service, false, out entityResource, out entityContainer, true);
                this.Tracker.TrackAction(entityResource, entityContainer, UpdateOperations.Change);
                ModifyResource(requestDescription, resourceToBeModified, requestValue, this.Service);
            }
            return (entityResource ?? requestValue);
        }

        internal static bool IsCrossReferencedSegment(System.Data.Services.SegmentInfo segmentInfo, IDataService service)
        {
            return (segmentInfo.Identifier.StartsWith("$", StringComparison.Ordinal) && (service.GetSegmentForContentId(segmentInfo.Identifier) != null));
        }

        private static bool IsQueryRequired(System.Data.Services.RequestDescription requestDescription, object requestValue)
        {
            if ((((requestDescription.TargetKind == RequestTargetKind.PrimitiveValue) || (requestDescription.TargetKind == RequestTargetKind.Primitive)) || ((requestDescription.TargetKind == RequestTargetKind.OpenPropertyValue) || (requestDescription.TargetKind == RequestTargetKind.MediaResource))) || ((requestDescription.TargetKind == RequestTargetKind.ComplexObject) || (requestDescription.TargetKind == RequestTargetKind.Collection)))
            {
                return true;
            }
            if (requestDescription.TargetKind == RequestTargetKind.OpenProperty)
            {
                ResourceType targetResourceType = requestDescription.LastSegmentInfo.TargetResourceType;
                if ((requestValue == null) || (targetResourceType.ResourceTypeKind == ResourceTypeKind.Primitive))
                {
                    return true;
                }
                if (targetResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                {
                    return true;
                }
            }
            return false;
        }

        internal static void ModifyResource(System.Data.Services.RequestDescription description, object resourceToBeModified, object requestValue, IDataService service)
        {
            if ((description.TargetKind == RequestTargetKind.OpenProperty) || (description.TargetKind == RequestTargetKind.OpenPropertyValue))
            {
                SetOpenPropertyValue(resourceToBeModified, description.ContainerName, requestValue, service);
            }
            else if (description.TargetKind == RequestTargetKind.MediaResource)
            {
                SetStreamPropertyValue(resourceToBeModified, (Stream) requestValue, service, description);
            }
            else
            {
                SetPropertyValue(description.LastSegmentInfo.ProjectedProperty, resourceToBeModified, requestValue, service);
            }
        }

        internal object ReadEntity()
        {
            System.Data.Services.RequestDescription requestDescription = this.RequestDescription;
            if (requestDescription.TargetKind == RequestTargetKind.Resource)
            {
                this.RequestDescription.UpdateAndCheckEpmFeatureVersion(this.description.LastSegmentInfo.TargetContainer, this.Service);
            }
            System.Data.Services.SegmentInfo lastSegmentInfo = requestDescription.LastSegmentInfo;
            if (!this.Update)
            {
                lastSegmentInfo = new System.Data.Services.SegmentInfo {
                    TargetKind = lastSegmentInfo.TargetKind,
                    TargetSource = lastSegmentInfo.TargetSource,
                    SingleResult = true,
                    ProjectedProperty = lastSegmentInfo.ProjectedProperty,
                    TargetResourceType = lastSegmentInfo.TargetResourceType,
                    TargetContainer = lastSegmentInfo.TargetContainer,
                    Identifier = lastSegmentInfo.Identifier
                };
            }
            return this.Deserialize(lastSegmentInfo);
        }

        internal static Dictionary<string, object> ReadPayloadParameters(System.Data.Services.SegmentInfo actionSegment, IDataService dataService)
        {
			System.Data.Services.RequestDescription description = new System.Data.Services.RequestDescription(new System.Data.Services.SegmentInfo[] { actionSegment }, RequestUriProcessor.GetResultUri(dataService.OperationContext));
            description.VerifyRequestVersion(System.Data.Services.RequestDescription.Version3Dot0, dataService);
            using (Deserializer deserializer = CreateDeserializer(description, dataService, false, UpdateTracker.CreateUpdateTracker(dataService)))
            {
                return (Dictionary<string, object>) deserializer.Deserialize(actionSegment);
            }
        }

        protected static string ReadStringFromStream(StreamReader streamReader)
        {
            return streamReader.ReadToEnd();
        }

        protected void RecurseEnter()
        {
            WebUtil.RecurseEnter(100, ref this.recursionDepth);
        }

        protected void RecurseLeave()
        {
            WebUtil.RecurseLeave(ref this.recursionDepth);
        }

        protected static void SetOpenPropertyValue(object declaringResource, string propertyName, object propertyValue, IDataService service)
        {
            service.Updatable.SetValue(declaringResource, propertyName, propertyValue);
        }

        protected static void SetPropertyValue(ResourceProperty resourceProperty, object declaringResource, object propertyValue, IDataService service)
        {
            service.Updatable.SetValue(declaringResource, resourceProperty.Name, propertyValue);
        }

        internal static void SetStreamPropertyValue(object resourceToBeModified, Stream requestStream, IDataService service, System.Data.Services.RequestDescription description)
        {
            resourceToBeModified = service.Updatable.ResolveResource(resourceToBeModified);
            ResourceType resourceType = service.Provider.GetResourceType(resourceToBeModified);
            if (((description.TargetKind == RequestTargetKind.MediaResource) && !System.Data.Services.RequestDescription.IsNamedStream(description)) && !resourceType.IsMediaLinkEntry)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidUriForMediaResource(service.OperationContext.AbsoluteRequestUri));
            }
            if (service.OperationContext.Host.HttpVerb == HttpVerbs.MERGE)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.BadRequest_InvalidUriForMergeOperation(service.OperationContext.AbsoluteRequestUri), DataServiceConfiguration.GetAllowedMethods(service.Configuration, description));
            }
            if (service.OperationContext.Host.HttpVerb == HttpVerbs.PATCH)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.BadRequest_InvalidUriForPatchOperation(service.OperationContext.AbsoluteRequestUri), DataServiceConfiguration.GetAllowedMethods(service.Configuration, description));
            }
            ResourceProperty streamProperty = null;
            if (description.TargetKind == RequestTargetKind.MediaResource)
            {
                streamProperty = System.Data.Services.RequestDescription.GetStreamProperty(description);
            }
            using (Stream stream = service.StreamProvider.GetWriteStream(resourceToBeModified, streamProperty, service.OperationContext))
            {
                WebUtil.CopyStream(requestStream, stream, service.StreamProvider.StreamBufferSize);
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void UpdateAndCheckRequestResponseDSV(ResourceType resourceType, bool topLevel)
        {
            bool flag = this.ContentFormat == System.Data.Services.ContentFormat.Atom;
            if (flag)
            {
                Version requiredVersion = resourceType.EpmMinimumDataServiceProtocolVersion.ToVersion();
                this.RequestDescription.VerifyRequestVersion(requiredVersion, this.service);
            }
            if (topLevel && this.ResponseWillBeSent)
            {
                Version version2;
                bool considerEpmInVersion = WebUtil.IsAtomResponseFormat(this.Service.OperationContext.Host.RequestAccept, this.RequestDescription.TargetKind, this.Service.Configuration.DataServiceBehavior.MaxProtocolVersion, this.Service.OperationContext.Host.RequestMaxVersion);
                ResourceSetWrapper targetContainer = this.RequestDescription.LastSegmentInfo.TargetContainer;
                if (flag)
                {
                    version2 = resourceType.GetMinimumResponseVersion(this.Service, targetContainer, considerEpmInVersion);
                }
                else
                {
                    version2 = resourceType.GetMinimumResponseVersion(this.Service, targetContainer, considerEpmInVersion);
                    if (considerEpmInVersion && (version2 <= System.Data.Services.RequestDescription.Version2Dot0))
                    {
                        this.RequestDescription.VerifyAndRaiseActualResponseVersion(version2, this.service);
                        version2 = resourceType.GetMinimumResponseVersion(this.Service, targetContainer, false);
                    }
                }
                this.RequestDescription.VerifyAndRaiseResponseVersion(version2, this.Service);
            }
        }

        internal void UpdateObjectCount(int value)
        {
            this.objectCount = value;
        }

        internal static bool XmlHasNullAttributeWithTrueValue(XmlReader reader)
        {
            string attribute = reader.GetAttribute("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            if ((attribute == null) || !XmlConvert.ToBoolean(attribute))
            {
                return false;
            }
            string localName = reader.LocalName;
            if (!CommonUtil.ReadEmptyElement(reader))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_CannotSpecifyValueOrChildElementsForNullElement(localName));
            }
            return true;
        }

        protected abstract System.Data.Services.ContentFormat ContentFormat { get; }

        protected int MaxObjectCount
        {
            get
            {
                return this.objectCount;
            }
        }

        protected System.Data.Services.RequestDescription RequestDescription
        {
            get
            {
                return this.description;
            }
        }

        private bool ResponseWillBeSent
        {
            get
            {
                return ((!this.Update && (this.description.PreferenceApplied != PreferenceApplied.NoContent)) || (this.Update && (this.description.PreferenceApplied == PreferenceApplied.Content)));
            }
        }

        protected IDataService Service
        {
            [DebuggerStepThrough]
            get
            {
                return this.service;
            }
        }

        internal UpdateTracker Tracker
        {
            [DebuggerStepThrough]
            get
            {
                return this.tracker;
            }
        }

        protected UpdatableWrapper Updatable
        {
            get
            {
                return this.Service.Updatable;
            }
        }

        protected bool Update
        {
            [DebuggerStepThrough]
            get
            {
                return this.update;
            }
        }
    }
}

