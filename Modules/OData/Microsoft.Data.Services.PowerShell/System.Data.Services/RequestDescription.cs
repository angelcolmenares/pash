namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("RequestDescription={TargetSource} '{ContainerName}' -> {TargetKind} '{TargetResourceType}'")]
    internal class RequestDescription
    {
		
		internal static readonly Version Version1Dot0 = new Version(1, 0);
		internal static readonly Version Version2Dot0 = new Version(2, 0);
		internal static readonly Version Version3Dot0 = new Version(3, 0);
        private Version actualResponseVersion;
        private readonly string containerName;
        private RequestQueryCountOption countOption;
        private long countValue;
		internal static readonly Version DataServiceDefaultResponseVersion = Version3Dot0;
        internal static readonly Version[] KnownDataServiceVersions = new Version[] { Version1Dot0, Version2Dot0, Version3Dot0 };
        private readonly string mimeType;
        private Version requestMaxVersion;
        private Version requestVersion;
        private Version responseVersion;
        private readonly Uri resultUri;
        private readonly System.Data.Services.Providers.RootProjectionNode rootProjectionNode;
        private readonly SegmentInfo[] segmentInfos;

        internal RequestDescription(SegmentInfo[] segmentInfos, Uri resultUri)
        {
            this.segmentInfos = segmentInfos;
            this.resultUri = resultUri;
            this.responseVersion = DataServiceDefaultResponseVersion;
            this.actualResponseVersion = this.ResponseVersion;
            this.PreferenceApplied = System.Data.Services.PreferenceApplied.None;
            this.containerName = (((this.LastSegmentInfo.TargetKind != RequestTargetKind.PrimitiveValue) && (this.LastSegmentInfo.TargetKind != RequestTargetKind.OpenPropertyValue)) && (this.LastSegmentInfo.TargetKind != RequestTargetKind.MediaResource)) ? this.LastSegmentInfo.Identifier : ((this.LastSegmentInfo.TargetSource == RequestTargetSource.ServiceOperation) ? null : segmentInfos[segmentInfos.Length - 2].Identifier);
            this.mimeType = ((this.TargetSource == RequestTargetSource.Property) && (this.Property != null)) ? this.Property.MimeType : ((this.TargetSource == RequestTargetSource.ServiceOperation) ? this.LastSegmentInfo.Operation.MimeType : null);
        }

        internal RequestDescription(RequestDescription other, Expression resultExpression, System.Data.Services.Providers.RootProjectionNode rootProjectionNode)
        {
            this.containerName = other.containerName;
            this.mimeType = other.mimeType;
            this.resultUri = other.resultUri;
            this.segmentInfos = other.SegmentInfos;
            this.rootProjectionNode = rootProjectionNode;
            this.countOption = other.countOption;
            this.SkipTokenExpressionCount = other.SkipTokenExpressionCount;
            this.SkipTokenProperties = other.SkipTokenProperties;
            this.countValue = other.countValue;
            this.responseVersion = other.ResponseVersion;
            this.actualResponseVersion = other.ActualResponseVersion;
            this.PreferenceApplied = other.PreferenceApplied;
            if (resultExpression == null)
            {
                this.segmentInfos = other.SegmentInfos;
            }
            else
            {
                int index = other.SegmentInfos.Length - 1;
                SegmentInfo info = other.SegmentInfos[index];
                info.RequestExpression = resultExpression;
            }
        }

        internal RequestDescription(RequestTargetKind targetKind, RequestTargetSource targetSource, Uri resultUri)
        {
            SegmentInfo info = new SegmentInfo {
                TargetKind = targetKind,
                TargetSource = targetSource,
                SingleResult = true
            };
            this.segmentInfos = new SegmentInfo[] { info };
            this.resultUri = resultUri;
            this.responseVersion = DataServiceDefaultResponseVersion;
            this.actualResponseVersion = this.ResponseVersion;
            this.PreferenceApplied = System.Data.Services.PreferenceApplied.None;
        }

        internal void AnalyzeClientPreference(IDataService service)
        {
            if ((!this.LinkUri && (this.SegmentInfos[0].TargetSource != RequestTargetSource.ServiceOperation)) && ((service.OperationContext.Host.RequestVersion >= Version3Dot0) && (service.Configuration.DataServiceBehavior.MaxProtocolVersion >= DataServiceProtocolVersion.V3)))
            {
                HttpVerbs httpVerb = service.OperationContext.Host.HttpVerb;
                if ((httpVerb == HttpVerbs.POST) || ((((httpVerb == HttpVerbs.PUT) || (httpVerb == HttpVerbs.MERGE)) || (httpVerb == HttpVerbs.PATCH)) && ((this.TargetKind == RequestTargetKind.Resource) || this.IsRequestForNonEntityProperty)))
                {
                    bool? returnContentPreference = service.OperationContext.Host.ReturnContentPreference;
                    if (returnContentPreference == true)
                    {
                        this.PreferenceApplied = System.Data.Services.PreferenceApplied.Content;
                    }
                    else if (returnContentPreference == false)
                    {
                        this.PreferenceApplied = System.Data.Services.PreferenceApplied.NoContent;
                    }
                }
            }
        }

        internal void ApplyRequestMinVersion(IDataService service)
        {
            this.VerifyAndRaiseResponseVersion(service.OperationContext.Host.RequestMinVersion, service);
        }

        internal static void CheckNullDirectReference(object result, SegmentInfo segmentInfo)
        {
            if (segmentInfo.IsDirectReference && (result == null))
            {
                throw DataServiceException.CreateResourceNotFound(segmentInfo.Identifier);
            }
        }

        internal static RequestDescription CreateSingleResultRequestDescription(RequestDescription description, object entity)
        {
            SegmentInfo info = new SegmentInfo {
                RequestExpression = Expression.Constant(entity),
                RequestEnumerable = new object[] { entity },
                TargetKind = description.TargetKind,
                TargetSource = description.TargetSource,
                SingleResult = true,
                ProjectedProperty = description.Property,
                TargetResourceType = description.TargetResourceType,
                TargetContainer = description.LastSegmentInfo.TargetContainer,
                Identifier = description.LastSegmentInfo.Identifier
            };
            SegmentInfo[] segmentInfos = description.SegmentInfos;
            segmentInfos[segmentInfos.Length - 1] = info;
            return new RequestDescription(segmentInfos, description.ResultUri) { responseVersion = description.ResponseVersion, actualResponseVersion = description.ActualResponseVersion, PreferenceApplied = description.PreferenceApplied };
        }

        internal int GetIndexOfTargetEntityResource()
        {
            if (this.LinkUri || (this.CountOption == RequestQueryCountOption.ValueOnly))
            {
                return (this.SegmentInfos.Length - 1);
            }
            for (int i = this.SegmentInfos.Length - 1; i >= 0; i--)
            {
                if ((this.segmentInfos[i].TargetKind == RequestTargetKind.Resource) || this.segmentInfos[i].HasKeyValues)
                {
                    return i;
                }
            }
            return -1;
        }

        private void GetLinkedResourceSets(out ResourceSetWrapper leftSet, out ResourceSetWrapper rightSet)
        {
            int index = 0;
            while (index < this.segmentInfos.Length)
            {
                if (this.segmentInfos[index].TargetKind == RequestTargetKind.Link)
                {
                    break;
                }
                index++;
            }
            leftSet = this.segmentInfos[index - 1].TargetContainer;
            rightSet = this.segmentInfos[index + 1].TargetContainer;
        }

        internal static ResourceProperty GetStreamProperty(RequestDescription description)
        {
            ResourceProperty property = null;
            if (IsNamedStream(description))
            {
                property = description.TargetResourceType.TryResolvePropertyName(description.LastSegmentInfo.Identifier);
            }
            return property;
        }

        private void InitializeVersion(DataServiceOperationContext operationContext)
        {
            if (this.requestVersion == null)
            {
                this.requestVersion = operationContext.Host.RequestVersion;
            }
            if (this.requestMaxVersion == null)
            {
                this.requestMaxVersion = operationContext.Host.RequestMaxVersion;
            }
        }

        internal static bool IsETagHeaderAllowed(RequestDescription description)
        {
            if ((!description.IsSingleResult || (description.CountOption == RequestQueryCountOption.ValueOnly)) || ((description.RootProjectionNode != null) && description.RootProjectionNode.ExpansionsSpecified))
            {
                return false;
            }
            return !description.LinkUri;
        }

        internal static bool IsKnownRequestVersion(Version requestVersion)
        {
            return KnownDataServiceVersions.Contains<Version>(requestVersion);
        }

        internal static bool IsNamedStream(RequestDescription description)
        {
            return (description.LastSegmentInfo.Identifier != "$value");
        }

        internal RequestDescription UpdateAndCheckEpmFeatureVersion(IDataService service)
        {
            if (this.LinkUri)
            {
                ResourceSetWrapper wrapper;
                ResourceSetWrapper wrapper2;
                this.GetLinkedResourceSets(out wrapper, out wrapper2);
                this.UpdateAndCheckEpmFeatureVersion(wrapper, service);
                this.UpdateAndCheckEpmFeatureVersion(wrapper2, service);
            }
            else
            {
                int indexOfTargetEntityResource = this.GetIndexOfTargetEntityResource();
                if (indexOfTargetEntityResource != -1)
                {
                    ResourceSetWrapper targetContainer = this.SegmentInfos[indexOfTargetEntityResource].TargetContainer;
                    this.UpdateAndCheckEpmFeatureVersion(targetContainer, service);
                }
            }
            return this;
        }

        internal RequestDescription UpdateAndCheckEpmFeatureVersion(ResourceSetWrapper resourceSet, IDataService service)
        {
            return this;
        }

        internal static void UpdateMetadataVersion(DataServiceProviderWrapper provider, DataServiceOperationContext operationContext, out Version metadataVersion, out MetadataEdmSchemaVersion edmSchemaVersion)
        {
            metadataVersion = Version1Dot0;
            edmSchemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
            if (!provider.IsV1Provider)
            {
                edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(edmSchemaVersion, MetadataEdmSchemaVersion.Version1Dot1);
            }
            foreach (ResourceType type in provider.GetVisibleTypes(operationContext))
            {
                UpdateMetadataVersionForResourceType(type, ref metadataVersion, ref edmSchemaVersion);
            }
            if (provider.HasAnnotations(operationContext))
            {
                edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(edmSchemaVersion, MetadataEdmSchemaVersion.Version3Dot0);
            }
            foreach (OperationWrapper wrapper in provider.GetVisibleOperations(operationContext))
            {
                if (wrapper.Kind == OperationKind.Action)
                {
                    edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(edmSchemaVersion, MetadataEdmSchemaVersion.Version3Dot0);
                    metadataVersion = WebUtil.RaiseVersion(metadataVersion, Version3Dot0);
                    break;
                }
                if (((wrapper.ResultKind == ServiceOperationResultKind.Void) || (wrapper.ResultKind == ServiceOperationResultKind.QueryWithSingleResult)) || (((wrapper.ResultKind == ServiceOperationResultKind.DirectValue) || (wrapper.ResultType.ResourceTypeKind == ResourceTypeKind.ComplexType)) || (wrapper.ResultType.ResourceTypeKind == ResourceTypeKind.Primitive)))
                {
                    edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(edmSchemaVersion, MetadataEdmSchemaVersion.Version1Dot1);
                    break;
                }
            }
        }

        private static void UpdateMetadataVersionForResourceType(ResourceType resourceType, ref Version metadataVersion, ref MetadataEdmSchemaVersion edmSchemaVersion)
        {
            if (resourceType.IsOpenType)
            {
                edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(edmSchemaVersion, MetadataEdmSchemaVersion.Version1Dot2);
            }
            if (resourceType.EpmMinimumDataServiceProtocolVersion.ToVersion() > metadataVersion)
            {
                metadataVersion = WebUtil.RaiseVersion(metadataVersion, resourceType.EpmMinimumDataServiceProtocolVersion.ToVersion());
            }
            metadataVersion = WebUtil.RaiseVersion(metadataVersion, resourceType.MetadataVersion);
            edmSchemaVersion = WebUtil.RaiseVersion(edmSchemaVersion, resourceType.SchemaVersion);
        }

        internal RequestDescription UpdateResponseVersionForPostMR(ResourceType resourceType, IDataService dataService)
        {
            if (this.PreferenceApplied != System.Data.Services.PreferenceApplied.NoContent)
            {
                this.InitializeVersion(dataService.OperationContext);
                bool considerEpmInVersion = WebUtil.IsAtomResponseFormat(dataService.OperationContext.Host.RequestAccept, RequestTargetKind.Resource, dataService.Configuration.DataServiceBehavior.MaxProtocolVersion, this.requestMaxVersion);
                ResourceSetWrapper targetContainer = this.LastSegmentInfo.TargetContainer;
                Version version = resourceType.GetMinimumResponseVersion(dataService, targetContainer, considerEpmInVersion);
                if (considerEpmInVersion && (version <= Version2Dot0))
                {
                    this.VerifyAndRaiseActualResponseVersion(version, dataService);
                    version = resourceType.GetMinimumResponseVersion(dataService, targetContainer, false);
                }
                this.VerifyAndRaiseResponseVersion(version, dataService);
            }
            return this;
        }

        internal RequestDescription UpdateVersions(string acceptTypesText, IDataService service)
        {
            return this.UpdateVersions(acceptTypesText, this.LastSegmentInfo.TargetContainer, service);
        }

        internal RequestDescription UpdateVersions(string acceptTypesText, ResourceSetWrapper resourceSet, IDataService service)
        {
            DataServiceHostWrapper host = service.OperationContext.Host;
            if ((host.HttpVerb == HttpVerbs.GET) || ((host.HttpVerb == HttpVerbs.POST) && (this.TargetSource == RequestTargetSource.ServiceOperation)))
            {
                if (this.TargetKind == RequestTargetKind.Resource)
                {
                    if (!this.LinkUri)
                    {
                        this.InitializeVersion(service.OperationContext);
                        bool considerEpmInVersion = WebUtil.IsAtomResponseFormat(acceptTypesText, this.TargetKind, service.Configuration.DataServiceBehavior.MaxProtocolVersion, this.requestMaxVersion);
                        Version version = resourceSet.MinimumResponsePayloadVersion(service, considerEpmInVersion);
                        this.VerifyAndRaiseResponseVersion(version, service);
                    }
                }
                else if (((this.TargetResourceType != null) && (this.CountOption != RequestQueryCountOption.ValueOnly)) && (this.TargetKind != RequestTargetKind.MediaResource))
                {
                    this.VerifyAndRaiseResponseVersion(this.TargetResourceType.MetadataVersion, service);
                }
                else if (this.TargetKind == RequestTargetKind.OpenProperty)
                {
                    this.InitializeVersion(service.OperationContext);
                    Version effectiveMaxResponseVersion = WebUtil.GetEffectiveMaxResponseVersion(service.Configuration.DataServiceBehavior.MaxProtocolVersion, this.requestMaxVersion);
                    this.VerifyAndRaiseResponseVersion(effectiveMaxResponseVersion, service);
                }
            }
            else if (host.HttpVerb == HttpVerbs.PATCH)
            {
                this.VerifyProtocolVersion(Version3Dot0, service);
                this.VerifyRequestVersion(Version3Dot0, service);
            }
            return this;
        }

        internal void VerifyAndRaiseActualResponseVersion(Version version, IDataService service)
        {
            this.InitializeVersion(service.OperationContext);
            this.actualResponseVersion = WebUtil.RaiseVersion(this.ActualResponseVersion, version);
        }

        internal void VerifyAndRaiseResponseVersion(Version version, IDataService service)
        {
            this.InitializeVersion(service.OperationContext);
            this.VerifyAndRaiseActualResponseVersion(version, service);
            this.responseVersion = WebUtil.RaiseVersion(this.ResponseVersion, version);
            if (this.requestMaxVersion < this.ResponseVersion)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_MaxDSVTooLow(this.requestMaxVersion.ToString(2), this.ResponseVersion.Major, this.ResponseVersion.Minor));
            }
        }

        internal void VerifyProtocolVersion(Version featureVersion, IDataService service)
        {
            this.InitializeVersion(service.OperationContext);
            WebUtil.CheckMaxProtocolVersion(featureVersion, service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion());
        }

        internal void VerifyRequestVersion(Version requiredVersion, IDataService service)
        {
            this.InitializeVersion(service.OperationContext);
            WebUtil.CheckRequestVersion(requiredVersion, this.requestVersion);
        }

        internal Version ActualResponseVersion
        {
            get
            {
                return this.actualResponseVersion;
            }
        }

        internal string ContainerName
        {
            [DebuggerStepThrough]
            get
            {
                return this.containerName;
            }
        }

        internal RequestQueryCountOption CountOption
        {
            get
            {
                return this.countOption;
            }
            set
            {
                this.countOption = value;
            }
        }

        internal long CountValue
        {
            get
            {
                return this.countValue;
            }
            set
            {
                this.countValue = value;
            }
        }

        internal bool IsRequestForEnumServiceOperation
        {
            get
            {
                return (((this.TargetSource == RequestTargetSource.ServiceOperation) && (this.SegmentInfos[0].Operation != null)) && (this.SegmentInfos[0].Operation.ResultKind == ServiceOperationResultKind.Enumeration));
            }
        }

        internal bool IsRequestForNonEntityProperty
        {
            get
            {
                if ((((this.TargetKind != RequestTargetKind.Collection) && (this.TargetKind != RequestTargetKind.ComplexObject)) && ((this.TargetKind != RequestTargetKind.OpenProperty) && (this.TargetKind != RequestTargetKind.OpenPropertyValue))) && (this.TargetKind != RequestTargetKind.Primitive))
                {
                    return (this.TargetKind == RequestTargetKind.PrimitiveValue);
                }
                return true;
            }
        }

        internal bool IsSingleResult
        {
            get
            {
                return this.LastSegmentInfo.SingleResult;
            }
        }

        internal SegmentInfo LastSegmentInfo
        {
            get
            {
                return this.segmentInfos[this.segmentInfos.Length - 1];
            }
        }

        internal bool LinkUri
        {
            get
            {
                return ((this.segmentInfos.Length >= 3) && (this.segmentInfos[this.segmentInfos.Length - 2].TargetKind == RequestTargetKind.Link));
            }
        }

        internal string MimeType
        {
            [DebuggerStepThrough]
            get
            {
                return this.mimeType;
            }
        }

        internal System.Data.Services.PreferenceApplied PreferenceApplied { get; private set; }

        internal ResourceProperty Property
        {
            get
            {
                return this.LastSegmentInfo.ProjectedProperty;
            }
        }

        internal Expression RequestExpression
        {
            get
            {
                return this.LastSegmentInfo.RequestExpression;
            }
        }

        internal Version RequestVersion
        {
            get
            {
                return this.requestVersion;
            }
        }

        internal Version ResponseVersion
        {
            get
            {
                return this.responseVersion;
            }
        }

        internal Uri ResultUri
        {
            [DebuggerStepThrough]
            get
            {
                return this.resultUri;
            }
        }

        internal System.Data.Services.Providers.RootProjectionNode RootProjectionNode
        {
            [DebuggerStepThrough]
            get
            {
                return this.rootProjectionNode;
            }
        }

        internal SegmentInfo[] SegmentInfos
        {
            [DebuggerStepThrough]
            get
            {
                return this.segmentInfos;
            }
        }

        internal bool ShouldWriteResponseBody { get; set; }

        internal int SkipTokenExpressionCount { get; set; }

        internal ICollection<ResourceProperty> SkipTokenProperties { get; set; }

        internal RequestTargetKind TargetKind
        {
            get
            {
                return this.LastSegmentInfo.TargetKind;
            }
        }

        internal ResourceSetWrapper TargetResourceSet
        {
            get
            {
                return this.LastSegmentInfo.TargetContainer;
            }
        }

        internal ResourceType TargetResourceType
        {
            get
            {
                return this.LastSegmentInfo.TargetResourceType;
            }
        }

        internal RequestTargetSource TargetSource
        {
            get
            {
                return this.LastSegmentInfo.TargetSource;
            }
        }
    }
}

