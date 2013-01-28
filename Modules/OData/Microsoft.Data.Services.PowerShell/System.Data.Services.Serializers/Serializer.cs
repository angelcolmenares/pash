namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Services;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Linq;

    internal abstract class Serializer
    {
        private readonly Uri absoluteServiceUri;
        private HashSet<object> complexTypeCollection;
        private object[] currentSkipTokenForCustomPaging;
        private readonly string httpETagHeaderValue;
        private static readonly string[] NextPageQueryParametersToCopy = new string[] { "$filter", "$expand", "$orderby", "$inlinecount", "$select" };
        private int recursionDepth;
        private const int RecursionLimit = 100;
        private readonly System.Data.Services.RequestDescription requestDescription;
        private SegmentInfo segmentInfo;
        private readonly IDataService service;

        internal Serializer(System.Data.Services.RequestDescription requestDescription, Uri absoluteServiceUri, IDataService service, string httpETagHeaderValue)
        {
            this.requestDescription = requestDescription;
            this.absoluteServiceUri = absoluteServiceUri;
            this.service = service;
            this.httpETagHeaderValue = httpETagHeaderValue;
        }

        protected bool AddToComplexTypeCollection(object complexTypeInstance)
        {
            if (this.complexTypeCollection == null)
            {
                this.complexTypeCollection = new HashSet<object>(System.Data.Services.ReferenceEqualityComparer<object>.Instance);
            }
            return this.complexTypeCollection.Add(complexTypeInstance);
        }

        internal static Uri AppendEntryToUri(Uri currentUri, string entry)
        {
			return RequestUriProcessor.AppendUnescapedSegment(currentUri, entry);
        }

        private static void AppendProjectionOrExpansionPath(StringBuilder pathsBuilder, IEnumerable<string> parentPathSegments, string lastPathSegment)
        {
            if (pathsBuilder.Length != 0)
            {
                pathsBuilder.Append(',');
            }
            foreach (string str in parentPathSegments)
            {
                pathsBuilder.Append(str).Append('/');
            }
            pathsBuilder.Append(lastPathSegment);
        }

        private void BuildProjectionAndExpansionPathsForNode(List<string> parentPathSegments, StringBuilder projectionPaths, StringBuilder expansionPaths, ExpandedProjectionNode expandedNode, out bool foundProjections, out bool foundExpansions)
        {
            foundProjections = false;
            foundExpansions = false;
            List<ExpandedProjectionNode> list = new List<ExpandedProjectionNode>();
            foreach (ProjectionNode node in expandedNode.Nodes)
            {
                ExpandedProjectionNode node2 = node as ExpandedProjectionNode;
                if (node2 == null)
                {
                    AppendProjectionOrExpansionPath(projectionPaths, parentPathSegments, node.PropertyName);
                    foundProjections = true;
                }
                else
                {
                    bool flag;
                    bool flag2;
                    foundExpansions = true;
                    parentPathSegments.Add(node2.PropertyName);
                    this.BuildProjectionAndExpansionPathsForNode(parentPathSegments, projectionPaths, expansionPaths, node2, out flag2, out flag);
                    parentPathSegments.RemoveAt(parentPathSegments.Count - 1);
                    if (node2.ProjectAllProperties)
                    {
                        if (flag2)
                        {
                            AppendProjectionOrExpansionPath(projectionPaths, parentPathSegments, node.PropertyName + "/*");
                        }
                        else
                        {
                            list.Add(node2);
                        }
                    }
                    foundProjections |= flag2;
                    if (!flag)
                    {
                        AppendProjectionOrExpansionPath(expansionPaths, parentPathSegments, node.PropertyName);
                    }
                }
            }
            if (!expandedNode.ProjectAllProperties || foundProjections)
            {
                foreach (ExpandedProjectionNode node3 in list)
                {
                    AppendProjectionOrExpansionPath(projectionPaths, parentPathSegments, node3.PropertyName);
                    foundProjections = true;
                }
            }
        }

        internal abstract void Flush();
        protected ODataCollectionValue GetCollection(string propertyName, CollectionResourceType propertyResourceType, object propertyValue)
        {
            Func<object, ODataComplexValue> valueConverter = null;
            this.RecurseEnter();
            IEnumerable collectionEnumerable = GetCollectionEnumerable(propertyValue, propertyName);
            ODataCollectionValue value2 = new ODataCollectionValue {
                TypeName = propertyResourceType.FullName
            };
            if (propertyResourceType.ItemType.ResourceTypeKind == ResourceTypeKind.Primitive)
            {
                value2.Items = GetEnumerable<object>(collectionEnumerable, new Func<object, object>(Serializer.GetPrimitiveValue));
            }
            else
            {
                if (valueConverter == null)
                {
                    valueConverter = value => this.GetComplexValue(propertyName, value);
                }
                value2.Items = GetEnumerable<ODataComplexValue>(collectionEnumerable, valueConverter);
            }
            this.RecurseLeave();
            return value2;
        }

        internal static IEnumerable GetCollectionEnumerable(object collectionPropertyValue, string propertyName)
        {
            IEnumerable enumerable;
            if (WebUtil.IsNullValue(collectionPropertyValue))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.Serializer_CollectionCanNotBeNull(propertyName));
            }
            if (!WebUtil.IsElementIEnumerable(collectionPropertyValue, out enumerable))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.Serializer_CollectionPropertyValueMustImplementIEnumerable(propertyName));
            }
            return enumerable;
        }

        protected ODataComplexValue GetComplexValue(string propertyName, object propertyValue)
        {
            if (WebUtil.IsNullValue(propertyValue))
            {
                return null;
            }
            ODataComplexValue value2 = new ODataComplexValue();
            ResourceType nonPrimitiveResourceType = WebUtil.GetNonPrimitiveResourceType(this.Provider, propertyValue);
            value2.TypeName = nonPrimitiveResourceType.FullName;
            value2.Properties = this.GetPropertiesOfComplexType(propertyValue, nonPrimitiveResourceType, propertyName);
            return value2;
        }

        private ExpandedProjectionNode GetCurrentExpandedProjectionNode()
        {
            if (this.segmentInfo != null)
            {
                return this.segmentInfo.CurrentExpandedNode;
            }
            return null;
        }

        internal static Uri GetEditLink(object resource, ResourceType resourceType, DataServiceProviderWrapper provider, ResourceSetWrapper container, Uri absoluteServiceUri)
        {
            Uri uri;
            return GetIdAndEditLink(resource, resourceType, provider, container, absoluteServiceUri, out uri);
        }

        private static IEnumerable<T> GetEnumerable<T>(IEnumerable enumerable, Func<object, T> valueConverter)
        {
            List<T> list = new List<T>();
            foreach (object obj2 in enumerable)
            {
                list.Add(valueConverter(obj2));
            }
            return list;
        }

        protected string GetETagValue(object resource, ResourceType resourceType)
        {
            if (!string.IsNullOrEmpty(this.httpETagHeaderValue))
            {
                return this.httpETagHeaderValue;
            }
            return WebUtil.GetETagValue(this.service, resource, resourceType, this.CurrentContainer);
        }

        protected static object GetExpandedElement(IExpandedResult expanded)
        {
            return expanded.ExpandedElement;
        }

        protected object GetExpandedProperty(IExpandedResult expanded, object customObject, ResourceProperty property, ExpandedProjectionNode expandedNode)
        {
            if (expanded == null)
            {
                return WebUtil.GetPropertyValue(this.Provider, customObject, null, property, null);
            }
            string name = property.Name;
            if ((expandedNode != null) && (this.GetCurrentExpandedProjectionNode().ResourceType != expandedNode.TargetResourceType))
            {
                name = expandedNode.TargetResourceType.FullName + "/" + property.Name;
            }
            return expanded.GetExpandedPropertyValue(name);
        }

        internal static Uri GetIdAndEditLink(object resource, ResourceType resourceType, DataServiceProviderWrapper provider, ResourceSetWrapper container, Uri absoluteServiceUri, out Uri id)
        {
            string segmentIdentifier = GetObjectKey(resource, resourceType, provider, container.Name);
			Uri uri = RequestUriProcessor.AppendEscapedSegment(absoluteServiceUri, segmentIdentifier);
            id = uri;
            if (container.HasNavigationPropertyOrNamedStreamsOnDerivedTypes(provider))
            {
				uri = RequestUriProcessor.AppendUnescapedSegment(uri, resourceType.FullName);
            }
            return uri;
        }

        protected ResourcePropertyInfo GetNavigationPropertyInfo(IExpandedResult expanded, object customObject, ResourceType currentResourceType, ResourceProperty property)
        {
            ExpandedProjectionNode node;
            object obj2 = null;
            bool expand = this.ShouldExpandSegment(property, currentResourceType, out node);
            if (expand)
            {
                obj2 = this.GetExpandedProperty(expanded, customObject, property, node);
            }
            return ResourcePropertyInfo.CreateResourcePropertyInfo(property, obj2, node, expand);
        }

        protected Uri GetNextLinkUri(object lastObject, IExpandedResult skipTokenExpandedResult, Uri absoluteUri)
        {
            SkipTokenBuilder builder2;
            UriBuilder builder = new UriBuilder(absoluteUri);
            if (this.IsRootContainer)
            {
                if (!this.IsCustomPaged)
                {
                    if (skipTokenExpandedResult != null)
                    {
                        builder2 = new SkipTokenBuilderFromExpandedResult(skipTokenExpandedResult, this.RequestDescription.SkipTokenExpressionCount);
                    }
                    else
                    {
                        builder2 = new SkipTokenBuilderFromProperties(lastObject, this.Provider, this.RequestDescription.SkipTokenProperties);
                    }
                }
                else
                {
                    builder2 = new SkipTokenBuilderFromCustomPaging(this.currentSkipTokenForCustomPaging);
                }
                builder.Query = this.GetNextPageQueryParametersForRootContainer().Append(builder2.GetSkipToken()).ToString();
            }
            else
            {
                if (!this.IsCustomPaged)
                {
                    builder2 = new SkipTokenBuilderFromProperties(lastObject, this.Provider, this.CurrentContainer.ResourceType.KeyProperties);
                }
                else
                {
                    builder2 = new SkipTokenBuilderFromCustomPaging(this.currentSkipTokenForCustomPaging);
                }
                builder.Query = this.GetNextPageQueryParametersForExpandedContainer().Append(builder2.GetSkipToken()).ToString();
            }
            return builder.Uri;
        }

        private StringBuilder GetNextPageQueryParametersForExpandedContainer()
        {
            StringBuilder builder = new StringBuilder();
            ExpandedProjectionNode currentExpandedProjectionNode = this.GetCurrentExpandedProjectionNode();
            if (currentExpandedProjectionNode != null)
            {
                bool flag;
                bool flag2;
                List<string> parentPathSegments = new List<string>();
                StringBuilder projectionPaths = new StringBuilder();
                StringBuilder expansionPaths = new StringBuilder();
                this.BuildProjectionAndExpansionPathsForNode(parentPathSegments, projectionPaths, expansionPaths, currentExpandedProjectionNode, out flag2, out flag);
                if (flag2 && currentExpandedProjectionNode.ProjectAllProperties)
                {
                    AppendProjectionOrExpansionPath(projectionPaths, parentPathSegments, "*");
                }
                if (projectionPaths.Length > 0)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('&');
                    }
                    builder.Append("$select=").Append(projectionPaths.ToString());
                }
                if (expansionPaths.Length > 0)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('&');
                    }
                    builder.Append("$expand=").Append(expansionPaths.ToString());
                }
            }
            if (builder.Length > 0)
            {
                builder.Append('&');
            }
            return builder;
        }

        private StringBuilder GetNextPageQueryParametersForRootContainer()
        {
            StringBuilder builder = new StringBuilder();
            if (this.RequestDescription.SegmentInfos[0].TargetSource == RequestTargetSource.ServiceOperation)
            {
                foreach (OperationParameter parameter in this.RequestDescription.SegmentInfos[0].Operation.Parameters)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('&');
                    }
                    builder.Append(parameter.Name).Append('=');
                    string str = DataStringEscapeBuilder.EscapeDataString(this.service.OperationContext.Host.GetQueryStringItem(parameter.Name));
                    builder.Append(str);
                }
            }
            foreach (string str2 in NextPageQueryParametersToCopy)
            {
                string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem(str2);
                if (!string.IsNullOrEmpty(queryStringItem))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('&');
                    }
                    builder.Append(str2).Append('=').Append(DataStringEscapeBuilder.EscapeDataString(queryStringItem));
                }
            }
            int? topQueryParameter = this.GetTopQueryParameter();
            if (topQueryParameter.HasValue)
            {
                int num = topQueryParameter.Value;
                if (!this.IsCustomPaged)
                {
                    num = topQueryParameter.Value - this.CurrentContainer.PageSize;
                }
                else
                {
                    num = topQueryParameter.Value - this.segmentInfo.CurrentResultCount;
                }
                if (num > 0)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('&');
                    }
                    builder.Append("$top").Append('=').Append(num);
                }
            }
            if (builder.Length > 0)
            {
                builder.Append('&');
            }
            return builder;
        }

        private static string GetObjectKey(object resource, ResourceType resourceType, DataServiceProviderWrapper provider, string containerName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(containerName);
            builder.Append('(');
            IList<ResourceProperty> keyProperties = resourceType.KeyProperties;
            for (int i = 0; i < keyProperties.Count; i++)
            {
                string str;
                ResourceProperty resourceProperty = keyProperties[i];
                object obj2 = WebUtil.GetPropertyValue(provider, resource, resourceType, resourceProperty, null);
                if (obj2 == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.Serializer_NullKeysAreNotSupported(resourceProperty.Name));
                }
                if (i == 0)
                {
                    if (keyProperties.Count != 1)
                    {
                        builder.Append(resourceProperty.Name);
                        builder.Append('=');
                    }
                }
                else
                {
                    builder.Append(',');
                    builder.Append(resourceProperty.Name);
                    builder.Append('=');
                }
                if (!WebConvert.TryKeyPrimitiveToString(obj2, out str))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.Serializer_CannotConvertValue(obj2));
                }
                builder.Append(str);
            }
            builder.Append(')');
            return builder.ToString();
        }

        internal Uri GetODataOperationMetadata(Uri serviceBaseUri, string operationTitle)
        {
            string segmentIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}#{1}.{2}", new object[] { "$metadata", Uri.EscapeDataString(this.Service.Provider.ContainerName), Uri.EscapeDataString(operationTitle) });
			return RequestUriProcessor.AppendEscapedSegment(serviceBaseUri, segmentIdentifier);
        }

        protected IEnumerable<OperationWrapper> GetOperationProjections()
        {
            ExpandedProjectionNode currentExpandedProjectionNode = this.GetCurrentExpandedProjectionNode();
            if ((currentExpandedProjectionNode != null) && !currentExpandedProjectionNode.ProjectAllOperations)
            {
                return currentExpandedProjectionNode.Operations;
            }
            return null;
        }

        internal static object GetPrimitiveValue(object propertyValue)
        {
            if (WebUtil.IsNullValue(propertyValue))
            {
                return null;
            }
            Type type = propertyValue.GetType();
            if (type == typeof(XElement))
            {
                return ((XElement) propertyValue).ToString();
            }
            if (type == typeof(Binary))
            {
                return ((Binary) propertyValue).ToArray();
            }
            return propertyValue;
        }

        protected IEnumerable<ProjectionNode> GetProjections()
        {
            ExpandedProjectionNode currentExpandedProjectionNode = this.GetCurrentExpandedProjectionNode();
            if ((currentExpandedProjectionNode != null) && !currentExpandedProjectionNode.ProjectAllProperties)
            {
                return currentExpandedProjectionNode.Nodes;
            }
            return null;
        }

        private IEnumerable<ODataProperty> GetPropertiesOfComplexType(object resource, ResourceType resourceType, string propertyName)
        {
            List<ODataProperty> list = new List<ODataProperty>();
            if (resourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
            {
                this.RecurseEnter();
                if (!this.AddToComplexTypeCollection(resource))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.Serializer_LoopsNotAllowedInComplexTypes(propertyName));
                }
                foreach (ResourceProperty property in resourceType.Properties)
                {
                    object propertyValue = WebUtil.GetPropertyValue(this.Service.Provider, resource, resourceType, property, null);
                    ODataProperty item = new ODataProperty {
                        Name = property.Name,
                        Value = this.GetPropertyValue(property.Name, property.ResourceType, propertyValue, false)
                    };
                    list.Add(item);
                }
                this.RemoveFromComplexTypeCollection(resource);
                this.RecurseLeave();
            }
            return list;
        }

        protected object GetPropertyValue(string propertyName, ResourceType propertyResourceType, object propertyValue, bool openProperty)
        {
            if (propertyResourceType.ResourceTypeKind == ResourceTypeKind.Primitive)
            {
                return GetPrimitiveValue(propertyValue);
            }
            if (propertyResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
            {
                return this.GetComplexValue(propertyName, propertyValue);
            }
            if (!openProperty)
            {
                return this.GetCollection(propertyName, (CollectionResourceType) propertyResourceType, propertyValue);
            }
            WebUtil.CheckResourceNotCollectionForOpenProperty(propertyResourceType, propertyName);
            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(propertyName));
        }

        protected IExpandedResult GetSkipToken(IExpandedResult expanded)
        {
            if (((expanded != null) && !this.IsCustomPaged) && !this.RequestDescription.IsRequestForEnumServiceOperation)
            {
                return (expanded.GetExpandedPropertyValue("$skiptoken") as IExpandedResult);
            }
            return null;
        }

        private int? GetTopQueryParameter()
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$top");
            if (!string.IsNullOrEmpty(queryStringItem))
            {
                return new int?(int.Parse(queryStringItem, CultureInfo.InvariantCulture));
            }
            return null;
        }

        protected void IncrementSegmentResultCount()
        {
            if (this.segmentInfo != null)
            {
                int maxResultsPerCollection = this.service.Configuration.MaxResultsPerCollection;
                if ((!this.IsCustomPaged && (this.CurrentContainer != null)) && (this.CurrentContainer.PageSize != 0))
                {
                    maxResultsPerCollection = this.CurrentContainer.PageSize;
                }
                if ((maxResultsPerCollection != 0x7fffffff) || this.IsCustomPaged)
                {
                    int num2 = this.segmentInfo.CurrentResultCount + 1;
                    if ((num2 > maxResultsPerCollection) && !DataServiceActionProviderWrapper.IsServiceActionRequest(this.RequestDescription))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.Serializer_ResultsExceedMax(maxResultsPerCollection));
                    }
                    this.segmentInfo.CurrentResultCount = num2;
                }
            }
        }

        protected bool NeedNextPageLink(IEnumerator enumerator)
        {
            if (((this.CurrentContainer != null) && !DataServiceActionProviderWrapper.IsServiceActionRequest(this.RequestDescription)) && !this.RequestDescription.IsRequestForEnumServiceOperation)
            {
                if (this.IsCustomPaged)
                {
                    this.currentSkipTokenForCustomPaging = this.service.PagingProvider.PagingProviderInterface.GetContinuationToken(BasicExpandProvider.ExpandedEnumeratorEx.UnwrapEnumerator(enumerator));
                    return ((this.currentSkipTokenForCustomPaging != null) && (this.currentSkipTokenForCustomPaging.Length > 0));
                }
                int pageSize = this.CurrentContainer.PageSize;
                if ((pageSize != 0) && (this.RequestDescription.ResponseVersion != System.Data.Services.RequestDescription.DataServiceDefaultResponseVersion))
                {
                    if (this.segmentInfo.Count == 1)
                    {
                        int? topQueryParameter = this.GetTopQueryParameter();
                        if (topQueryParameter.HasValue && (topQueryParameter.Value <= pageSize))
                        {
                            return false;
                        }
                    }
                    return (this.segmentInfo.CurrentResultCount == pageSize);
                }
            }
            return false;
        }

        protected void PopSegmentName(bool needPop)
        {
            if ((this.segmentInfo != null) && needPop)
            {
                this.segmentInfo.PopSegment();
            }
        }

        private bool PushSegment(ResourceSetWrapper container, ExpandedProjectionNode expandedNode)
        {
            if (((this.service.Configuration.MaxResultsPerCollection == 0x7fffffff) && ((container == null) || (container.PageSize == 0))) && ((this.requestDescription.RootProjectionNode == null) && !this.IsCustomPaged))
            {
                return false;
            }
            if (this.segmentInfo == null)
            {
                this.segmentInfo = new SegmentInfo();
            }
            this.segmentInfo.PushSegment(container, expandedNode);
            return true;
        }

        protected bool PushSegmentForProperty(ResourceProperty property, ResourceType currentResourceType, ExpandedProjectionNode expandedProjectionNode)
        {
            ResourceSetWrapper currentContainer = this.CurrentContainer;
            if (currentContainer != null)
            {
                currentContainer = this.service.Provider.GetContainer(currentContainer, currentResourceType, property);
            }
            return this.PushSegment(currentContainer, expandedProjectionNode);
        }

        protected bool PushSegmentForRoot()
        {
            return this.PushSegment(this.CurrentContainer, this.RequestDescription.RootProjectionNode);
        }

        protected void RecurseEnter()
        {
            WebUtil.RecurseEnter(100, ref this.recursionDepth);
        }

        protected void RecurseLeave()
        {
            WebUtil.RecurseLeave(ref this.recursionDepth);
        }

        protected void RemoveFromComplexTypeCollection(object complexTypeInstance)
        {
            this.complexTypeCollection.Remove(complexTypeInstance);
        }

        protected bool ShouldExpandSegment(ResourceProperty property, ResourceType currentResourceType, out ExpandedProjectionNode expandedNode)
        {
            expandedNode = null;
            if ((this.segmentInfo == null) || (this.segmentInfo.CurrentExpandedNode == null))
            {
                return false;
            }
            if (this.requestDescription.RootProjectionNode.UseExpandPathsForSerialization && (this.requestDescription.RootProjectionNode.ExpandPaths != null))
            {
                for (int i = 0; i < this.requestDescription.RootProjectionNode.ExpandPaths.Count; i++)
                {
                    List<ExpandSegment> list = this.requestDescription.RootProjectionNode.ExpandPaths[i];
                    if (list.Count >= this.segmentInfo.Count)
                    {
                        bool flag = true;
                        for (int j = 1; j < this.segmentInfo.Count; j++)
                        {
                            if (list[j - 1].Name != this.segmentInfo.GetSegmentName(j))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag && (list[this.segmentInfo.Count - 1].Name == property.Name))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                expandedNode = this.segmentInfo.CurrentExpandedNode.FindExpandedNode(property, currentResourceType);
            }
            return (expandedNode != null);
        }

        internal void WriteRequest(IEnumerator queryResults, bool hasMoved)
        {
            IExpandedResult expanded = queryResults as IExpandedResult;
            if (this.requestDescription.IsSingleResult)
            {
                this.WriteTopLevelElement(expanded, queryResults.Current);
                if (queryResults.MoveNext())
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.SingleResourceExpected);
                }
            }
            else
            {
                this.WriteTopLevelElements(expanded, queryResults, hasMoved);
            }
        }

        protected abstract void WriteTopLevelElement(IExpandedResult expanded, object element);
        protected abstract void WriteTopLevelElements(IExpandedResult expanded, IEnumerator elements, bool hasMoved);

        protected Uri AbsoluteServiceUri
        {
            [DebuggerStepThrough]
            get
            {
                return this.absoluteServiceUri;
            }
        }

        protected ResourceSetWrapper CurrentContainer
        {
            get
            {
                if ((this.segmentInfo != null) && (this.segmentInfo.Count != 0))
                {
                    return this.segmentInfo.CurrentResourceSet;
                }
                return this.requestDescription.LastSegmentInfo.TargetContainer;
            }
        }

        protected bool IsCustomPaged
        {
            get
            {
                return this.service.PagingProvider.IsCustomPagedForSerialization;
            }
        }

        protected bool IsRootContainer
        {
            get
            {
                if (this.segmentInfo != null)
                {
                    return (this.segmentInfo.Count == 1);
                }
                return true;
            }
        }

        protected DataServiceProviderWrapper Provider
        {
            [DebuggerStepThrough]
            get
            {
                return this.service.Provider;
            }
        }

        protected System.Data.Services.RequestDescription RequestDescription
        {
            [DebuggerStepThrough]
            get
            {
                return this.requestDescription;
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct ResourcePropertyInfo
        {
            internal ResourceProperty Property { get; private set; }
            internal object Value { get; private set; }
            internal bool Expand { get; private set; }
            internal ExpandedProjectionNode ExpandedNode { get; private set; }
            internal static Serializer.ResourcePropertyInfo CreateResourcePropertyInfo(ResourceProperty resourceProperty, object value, ExpandedProjectionNode expandedNode, bool expand)
            {
                return new Serializer.ResourcePropertyInfo { Property = resourceProperty, Value = value, Expand = expand, ExpandedNode = expandedNode };
            }
        }

        private class SegmentInfo
        {
            private readonly List<ExpandedProjectionNode> projectionNodes = new List<ExpandedProjectionNode>();
            private readonly List<ResourceSetWrapper> segmentContainers = new List<ResourceSetWrapper>();
            private readonly List<int> segmentResultCounts = new List<int>();

            internal SegmentInfo()
            {

            }

            internal string GetSegmentName(int index)
            {
                return this.projectionNodes[index].PropertyName;
            }

            internal void PopSegment()
            {
                this.segmentContainers.RemoveAt(this.segmentContainers.Count - 1);
                this.segmentResultCounts.RemoveAt(this.segmentResultCounts.Count - 1);
                this.projectionNodes.RemoveAt(this.projectionNodes.Count - 1);
            }

            internal void PushSegment(ResourceSetWrapper set, ExpandedProjectionNode projectionNode)
            {
                this.segmentContainers.Add(set);
                this.segmentResultCounts.Add(0);
                this.projectionNodes.Add(projectionNode);
            }

            internal int Count
            {
                get
                {
                    return this.projectionNodes.Count;
                }
            }

            internal ExpandedProjectionNode CurrentExpandedNode
            {
                get
                {
                    return this.projectionNodes[this.projectionNodes.Count - 1];
                }
            }

            internal ResourceSetWrapper CurrentResourceSet
            {
                get
                {
                    return this.segmentContainers[this.segmentContainers.Count - 1];
                }
            }

            internal int CurrentResultCount
            {
                get
                {
                    return this.segmentResultCounts[this.segmentResultCounts.Count - 1];
                }
                set
                {
                    this.segmentResultCounts[this.segmentResultCounts.Count - 1] = value;
                }
            }
        }

        private abstract class SkipTokenBuilder
        {
            private readonly StringBuilder skipToken = new StringBuilder();

            protected SkipTokenBuilder()
            {
                this.skipToken.Append("$skiptoken").Append('=');
            }

            public StringBuilder GetSkipToken()
            {
                object[] skipTokenProperties = this.GetSkipTokenProperties();
                bool flag = true;
                for (int i = 0; i < skipTokenProperties.Length; i++)
                {
                    string str;
                    object obj2 = skipTokenProperties[i];
                    if (obj2 == null)
                    {
                        str = "null";
                    }
                    else if (!WebConvert.TryKeyPrimitiveToString(obj2, out str))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.Serializer_CannotConvertValue(obj2));
                    }
                    if (!flag)
                    {
                        this.skipToken.Append(',');
                    }
                    this.skipToken.Append(str);
                    flag = false;
                }
                return this.skipToken;
            }

            protected abstract object[] GetSkipTokenProperties();
        }

        private class SkipTokenBuilderFromCustomPaging : Serializer.SkipTokenBuilder
        {
            private readonly object[] lastTokenValue;

            public SkipTokenBuilderFromCustomPaging(object[] lastTokenValue)
            {
                this.lastTokenValue = lastTokenValue;
            }

            protected override object[] GetSkipTokenProperties()
            {
                return this.lastTokenValue;
            }
        }

        private class SkipTokenBuilderFromExpandedResult : Serializer.SkipTokenBuilder
        {
            private readonly IExpandedResult skipTokenExpandedResult;
            private readonly int skipTokenExpressionCount;

            public SkipTokenBuilderFromExpandedResult(IExpandedResult skipTokenExpandedResult, int skipTokenExpressionCount)
            {
                this.skipTokenExpandedResult = skipTokenExpandedResult;
                this.skipTokenExpressionCount = skipTokenExpressionCount;
            }

            protected override object[] GetSkipTokenProperties()
            {
                object[] objArray = new object[this.skipTokenExpressionCount];
                for (int i = 0; i < this.skipTokenExpressionCount; i++)
                {
                    string name = "SkipTokenProperty" + i.ToString(CultureInfo.InvariantCulture);
                    object expandedPropertyValue = this.skipTokenExpandedResult.GetExpandedPropertyValue(name);
                    if (WebUtil.IsNullValue(expandedPropertyValue))
                    {
                        expandedPropertyValue = null;
                    }
                    objArray[i] = expandedPropertyValue;
                }
                return objArray;
            }
        }

        private class SkipTokenBuilderFromProperties : Serializer.SkipTokenBuilder
        {
            private readonly object element;
            private readonly ICollection<ResourceProperty> properties;
            private readonly DataServiceProviderWrapper provider;

            public SkipTokenBuilderFromProperties(object element, DataServiceProviderWrapper provider, ICollection<ResourceProperty> properties)
            {
                this.element = element;
                this.provider = provider;
                this.properties = properties;
            }

            protected override object[] GetSkipTokenProperties()
            {
                object[] objArray = new object[this.properties.Count];
                int num = 0;
                foreach (ResourceProperty property in this.properties)
                {
                    object propertyValue = WebUtil.GetPropertyValue(this.provider, this.element, null, property, null);
                    if (WebUtil.IsNullValue(propertyValue))
                    {
                        propertyValue = null;
                    }
                    objArray[num++] = propertyValue;
                }
                return objArray;
            }
        }
    }
}

