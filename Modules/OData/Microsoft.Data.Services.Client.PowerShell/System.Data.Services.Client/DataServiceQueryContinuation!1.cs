namespace System.Data.Services.Client
{
    using System;

    internal sealed class DataServiceQueryContinuation<T> : DataServiceQueryContinuation
    {
        internal DataServiceQueryContinuation(Uri nextLinkUri, ProjectionPlan plan) : base(nextLinkUri, plan)
        {
        }

        internal override Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }
    }
}

