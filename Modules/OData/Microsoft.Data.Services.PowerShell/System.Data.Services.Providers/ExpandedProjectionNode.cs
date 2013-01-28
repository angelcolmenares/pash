namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    [DebuggerDisplay("ExpandedProjectionNode {PropertyName}")]
    internal class ExpandedProjectionNode : ProjectionNode
    {
        private readonly Expression filter;
        private bool hasExpandedPropertyOnDerivedType;
        private readonly int? maxResultsExpected;
        private List<ProjectionNode> nodes;
        private List<OperationWrapper> operations;
        private readonly System.Data.Services.Providers.OrderingInfo orderingInfo;
        private bool projectAllImmediateOperations;
        private bool projectAllImmediateProperties;
        private bool projectionFound;
        private bool projectSubtree;
        private readonly System.Data.Services.Providers.ResourceSetWrapper resourceSetWrapper;
        private readonly int? skipCount;
        private readonly int? takeCount;

        internal ExpandedProjectionNode(string propertyName, ResourceProperty property, System.Data.Services.Providers.ResourceType targetResourceType, System.Data.Services.Providers.ResourceSetWrapper resourceSetWrapper, System.Data.Services.Providers.OrderingInfo orderingInfo, Expression filter, int? skipCount, int? takeCount, int? maxResultsExpected) : base(propertyName, property, targetResourceType)
        {
            this.resourceSetWrapper = resourceSetWrapper;
            this.orderingInfo = orderingInfo;
            this.filter = filter;
            this.skipCount = skipCount;
            this.takeCount = takeCount;
            this.maxResultsExpected = maxResultsExpected;
            this.nodes = new List<ProjectionNode>();
            this.operations = new List<OperationWrapper>();
            this.hasExpandedPropertyOnDerivedType = false;
        }

        internal ExpandedProjectionNode AddExpandedNode(ExpandSegment segment)
        {
            ExpandedProjectionNode node = (ExpandedProjectionNode) this.FindNode(segment.Name);
            if ((node != null) && (node.Property == segment.ExpandedProperty))
            {
                if (segment.TargetResourceType.IsAssignableFrom(node.TargetResourceType))
                {
                    node.TargetResourceType = segment.TargetResourceType;
                }
                return node;
            }
            node = new ExpandedProjectionNode(segment.Name, segment.ExpandedProperty, segment.TargetResourceType, segment.Container, segment.OrderingInfo, segment.Filter, null, (segment.Container.PageSize != 0) ? new int?(segment.Container.PageSize) : null, (segment.MaxResultsExpected != 0x7fffffff) ? new int?(segment.MaxResultsExpected) : null);
            this.AddNode(node);
            return node;
        }

        private void AddNode(ProjectionNode node)
        {
            this.nodes.Add(node);
            if (node is ExpandedProjectionNode)
            {
                this.hasExpandedPropertyOnDerivedType = this.hasExpandedPropertyOnDerivedType || !System.Data.Services.Providers.ResourceType.CompareReferences(this.ResourceType, node.TargetResourceType);
            }
        }

        internal void AddOperation(OperationWrapper operation)
        {
            if (this.FindOperation(operation.Name) == null)
            {
                this.operations.Add(operation);
            }
        }

        internal ExpandedProjectionNode AddProjectionNode(string propertyName, ResourceProperty property, System.Data.Services.Providers.ResourceType targetResourceType, bool lastPathSegment)
        {
            ProjectionNode existingNode = this.FindNode(propertyName);
            if ((existingNode == null) || !ApplyPropertyToExistingNode(existingNode, property, targetResourceType))
            {
                if (!lastPathSegment)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_ProjectedPropertyWithoutMatchingExpand(base.PropertyName));
                }
                existingNode = new ProjectionNode(propertyName, property, targetResourceType);
                this.AddNode(existingNode);
            }
            return (existingNode as ExpandedProjectionNode);
        }

        private static bool ApplyPropertyToExistingNode(ProjectionNode existingNode, ResourceProperty property, System.Data.Services.Providers.ResourceType targetResourceType)
        {
            if (((property == null) || (existingNode.Property == null)) || (property == existingNode.Property))
            {
                ExpandedProjectionNode node = existingNode as ExpandedProjectionNode;
                if (targetResourceType.IsAssignableFrom(existingNode.TargetResourceType))
                {
                    VerifyPropertyMismatchAndExpandSelectMismatchScenario(existingNode, property, targetResourceType, node != null);
                    existingNode.TargetResourceType = targetResourceType;
                    return true;
                }
                if (existingNode.TargetResourceType.IsAssignableFrom(targetResourceType))
                {
                    VerifyPropertyMismatchAndExpandSelectMismatchScenario(existingNode, property, targetResourceType, node != null);
                    return true;
                }
            }
            return false;
        }

        internal void ApplyWildcardsAndSort(DataServiceProviderWrapper provider)
        {
            Func<ProjectionNode, bool> predicate = null;
            if (this.projectSubtree)
            {
                for (int i = this.nodes.Count - 1; i >= 0; i--)
                {
                    ExpandedProjectionNode node = this.nodes[i] as ExpandedProjectionNode;
                    if (node != null)
                    {
                        node.projectSubtree = true;
                        node.ApplyWildcardsAndSort(provider);
                    }
                    else
                    {
                        this.nodes.RemoveAt(i);
                    }
                }
                this.projectAllImmediateProperties = false;
                this.projectAllImmediateOperations = false;
            }
            else
            {
                for (int j = this.nodes.Count - 1; j >= 0; j--)
                {
                    ExpandedProjectionNode node2 = this.nodes[j] as ExpandedProjectionNode;
                    if (this.ProjectAllImmediateProperties && (node2 == null))
                    {
                        this.nodes.RemoveAt(j);
                    }
                    else if (node2 != null)
                    {
                        node2.ApplyWildcardsAndSort(provider);
                    }
                }
                if (this.nodes.Count > 0)
                {
                    List<System.Data.Services.Providers.ResourceType> resourceTypesInMetadataOrder = new List<System.Data.Services.Providers.ResourceType> {
                        this.ResourceType
                    };
                    if (predicate == null)
                    {
                        predicate = n => !System.Data.Services.Providers.ResourceType.CompareReferences(n.TargetResourceType, this.ResourceType);
                    }
                    List<ProjectionNode> source = this.nodes.Where<ProjectionNode>(predicate).ToList<ProjectionNode>();
                    if (source.Count > 0)
                    {
                        using (IEnumerator<System.Data.Services.Providers.ResourceType> enumerator = provider.GetDerivedTypes(this.ResourceType).GetEnumerator())
                        {
                            Func<ProjectionNode, bool> func = null;
                            System.Data.Services.Providers.ResourceType rt;
                            while (enumerator.MoveNext())
                            {
                                rt = enumerator.Current;
                                if (func == null)
                                {
                                    func = node => node.TargetResourceType == rt;
                                }
                                if (source.FirstOrDefault<ProjectionNode>(func) != null)
                                {
                                    resourceTypesInMetadataOrder.Add(rt);
                                }
                            }
                        }
                    }
                    this.nodes = SortNodes(this.nodes, resourceTypesInMetadataOrder);
                }
            }
        }

        internal ExpandedProjectionNode FindExpandedNode(ResourceProperty property, System.Data.Services.Providers.ResourceType targetResourceType)
        {
            return (this.nodes.FirstOrDefault<ProjectionNode>(node => ((node.Property == property) && node.TargetResourceType.IsAssignableFrom(targetResourceType))) as ExpandedProjectionNode);
        }

        private ProjectionNode FindNode(string propertyName)
        {
            return this.nodes.FirstOrDefault<ProjectionNode>(projectionNode => string.Equals(projectionNode.PropertyName, propertyName, StringComparison.Ordinal));
        }

        internal OperationWrapper FindOperation(string operationName)
        {
            return this.operations.FirstOrDefault<OperationWrapper>(o => o.Name.Equals(operationName, StringComparison.Ordinal));
        }

        internal void MarkSubtreeAsProjected()
        {
            this.projectSubtree = true;
            this.projectAllImmediateProperties = false;
            foreach (ProjectionNode node in this.nodes)
            {
                ExpandedProjectionNode node2 = node as ExpandedProjectionNode;
                if (node2 != null)
                {
                    node2.MarkSubtreeAsProjected();
                }
            }
        }

        internal void RemoveNonProjectedNodes()
        {
            for (int i = this.nodes.Count - 1; i >= 0; i--)
            {
                ExpandedProjectionNode node = this.nodes[i] as ExpandedProjectionNode;
                if (node != null)
                {
                    if (!this.projectSubtree && !node.ProjectionFound)
                    {
                        this.nodes.RemoveAt(i);
                    }
                    else
                    {
                        node.RemoveNonProjectedNodes();
                    }
                }
            }
        }

        private static List<ProjectionNode> SortNodes(List<ProjectionNode> existingNodes, List<System.Data.Services.Providers.ResourceType> resourceTypesInMetadataOrder)
        {
            List<ProjectionNode> list = new List<ProjectionNode>(existingNodes.Count);
            using (List<System.Data.Services.Providers.ResourceType>.Enumerator enumerator = resourceTypesInMetadataOrder.GetEnumerator())
            {
                System.Data.Services.Providers.ResourceType resourceType;
                while (enumerator.MoveNext())
                {
                    resourceType = enumerator.Current;
                    using (IEnumerator<ResourceProperty> enumerator2 = resourceType.Properties.GetEnumerator())
                    {
                        Func<ProjectionNode, bool> predicate = null;
                        ResourceProperty property;
                        while (enumerator2.MoveNext())
                        {
                            property = enumerator2.Current;
                            if (predicate == null)
                            {
                                predicate = node => (node.Property == property) && (node.TargetResourceType == resourceType);
                            }
                            ProjectionNode item = existingNodes.FirstOrDefault<ProjectionNode>(predicate);
                            if (item != null)
                            {
                                list.Add(item);
                                existingNodes.Remove(item);
                            }
                        }
                        continue;
                    }
                }
            }
            List<ProjectionNode> collection = (from node in existingNodes
                where node.Property == null
                select node).ToList<ProjectionNode>();
            collection.Sort((Comparison<ProjectionNode>) ((x, y) => string.Compare(x.PropertyName, y.PropertyName, StringComparison.Ordinal)));
            list.AddRange(collection);
            return list;
        }

        private static void VerifyPropertyMismatchAndExpandSelectMismatchScenario(ProjectionNode existingNode, ResourceProperty property, System.Data.Services.Providers.ResourceType targetResourceType, bool expandNode)
        {
            if (property != existingNode.Property)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_CannotSpecifyOpenPropertyAndDeclaredPropertyAtTheSameTime(existingNode.PropertyName, (property == null) ? targetResourceType.FullName : existingNode.TargetResourceType.FullName, (property == null) ? existingNode.TargetResourceType.FullName : targetResourceType.FullName));
            }
            if (!System.Data.Services.Providers.ResourceType.CompareReferences(targetResourceType, existingNode.TargetResourceType) && expandNode)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_SelectAndExpandCannotBeSpecifiedTogether(existingNode.PropertyName));
            }
        }

        public Expression Filter
        {
            get
            {
                return this.filter;
            }
        }

        internal bool HasExpandedPropertyOnDerivedType
        {
            get
            {
                return this.hasExpandedPropertyOnDerivedType;
            }
        }

        internal bool HasFilterOrMaxResults
        {
            get
            {
                if (this.Filter == null)
                {
                    return this.MaxResultsExpected.HasValue;
                }
                return true;
            }
        }

        public int? MaxResultsExpected
        {
            get
            {
                return this.maxResultsExpected;
            }
        }

        public IEnumerable<ProjectionNode> Nodes
        {
            get
            {
                return this.nodes;
            }
        }

        public IEnumerable<OperationWrapper> Operations
        {
            get
            {
                return this.operations;
            }
        }

        public System.Data.Services.Providers.OrderingInfo OrderingInfo
        {
            get
            {
                return this.orderingInfo;
            }
        }

        internal bool ProjectAllImmediateOperations
        {
            get
            {
                return this.projectAllImmediateOperations;
            }
            set
            {
                this.projectAllImmediateOperations = value;
            }
        }

        internal bool ProjectAllImmediateProperties
        {
            get
            {
                return this.projectAllImmediateProperties;
            }
            set
            {
                this.projectAllImmediateProperties = value;
            }
        }

        public bool ProjectAllOperations
        {
            get
            {
                if (!this.projectSubtree)
                {
                    return this.ProjectAllImmediateOperations;
                }
                return true;
            }
        }

        public bool ProjectAllProperties
        {
            get
            {
                if (!this.projectSubtree)
                {
                    return this.ProjectAllImmediateProperties;
                }
                return true;
            }
        }

        internal bool ProjectionFound
        {
            get
            {
                return this.projectionFound;
            }
            set
            {
                this.projectionFound = value;
            }
        }

        internal System.Data.Services.Providers.ResourceSetWrapper ResourceSetWrapper
        {
            get
            {
                return this.resourceSetWrapper;
            }
        }

        internal virtual System.Data.Services.Providers.ResourceType ResourceType
        {
            get
            {
                return base.Property.ResourceType;
            }
        }

        public int? SkipCount
        {
            get
            {
                return this.skipCount;
            }
        }

        public int? TakeCount
        {
            get
            {
                return this.takeCount;
            }
        }
    }
}

