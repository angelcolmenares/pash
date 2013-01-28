namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("RootProjectionNode {PropertyName}")]
    internal class RootProjectionNode : ExpandedProjectionNode
    {
        private readonly System.Data.Services.Providers.ResourceType baseResourceType;
        private readonly List<ExpandSegmentCollection> expandPaths;

        internal RootProjectionNode(ResourceSetWrapper resourceSetWrapper, OrderingInfo orderingInfo, Expression filter, int? skipCount, int? takeCount, int? maxResultsExpected, List<ExpandSegmentCollection> expandPaths, System.Data.Services.Providers.ResourceType baseResourceType) : base(string.Empty, null, null, resourceSetWrapper, orderingInfo, filter, skipCount, takeCount, maxResultsExpected)
        {
            this.expandPaths = expandPaths;
            this.baseResourceType = baseResourceType;
        }

        internal List<ExpandSegmentCollection> ExpandPaths
        {
            get
            {
                return this.expandPaths;
            }
        }

        internal bool ExpansionOnDerivedTypesSpecified { get; set; }

        internal bool ExpansionsSpecified { get; set; }

        internal bool ProjectionsSpecified { get; set; }

        internal override System.Data.Services.Providers.ResourceType ResourceType
        {
            get
            {
                return this.baseResourceType;
            }
        }

        internal bool UseExpandPathsForSerialization { get; set; }
    }
}

