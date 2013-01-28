namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq;
	using System.Reflection;

    [DebuggerDisplay("{NextLinkUri}")]
    internal abstract class DataServiceQueryContinuation
    {
        private readonly Uri nextLinkUri;
        private readonly ProjectionPlan plan;

        internal DataServiceQueryContinuation(Uri nextLinkUri, ProjectionPlan plan)
        {
            this.nextLinkUri = nextLinkUri;
            this.plan = plan;
        }

        internal static DataServiceQueryContinuation Create(Uri nextLinkUri, ProjectionPlan plan)
        {
            if (nextLinkUri == null)
            {
                return null;
            }
            return (DataServiceQueryContinuation) Util.ConstructorInvoke(typeof(DataServiceQueryContinuation<>).MakeGenericType(new Type[] { plan.ProjectedType }).GetInstanceConstructors(false).Single<ConstructorInfo>(), new object[] { nextLinkUri, plan });
        }

        internal QueryComponents CreateQueryComponents()
        {
            return new QueryComponents(this.NextLinkUri, Util.DataServiceVersion2, this.Plan.LastSegmentType, null, null);
        }

        public override string ToString()
        {
            return this.NextLinkUri.ToString();
        }

        internal abstract Type ElementType { get; }

        public Uri NextLinkUri
        {
            get
            {
                return this.nextLinkUri;
            }
        }

        internal ProjectionPlan Plan
        {
            get
            {
                return this.plan;
            }
        }
    }
}

