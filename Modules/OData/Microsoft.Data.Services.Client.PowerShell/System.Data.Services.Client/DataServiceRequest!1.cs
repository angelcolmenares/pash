namespace System.Data.Services.Client
{
    using System;
    using System.Data.Services.Common;

    internal sealed class DataServiceRequest<TElement> : DataServiceRequest
    {
        private readonly ProjectionPlan plan;
        private System.Data.Services.Client.QueryComponents queryComponents;
        private Uri requestUri;

        public DataServiceRequest(Uri requestUri)
        {
            Util.CheckArgumentNull<Uri>(requestUri, "requestUri");
            this.requestUri = requestUri;
        }

        internal DataServiceRequest(System.Data.Services.Client.QueryComponents queryComponents, ProjectionPlan plan)
        {
            this.queryComponents = queryComponents;
            this.requestUri = queryComponents.Uri;
            this.plan = plan;
        }

        internal override System.Data.Services.Client.QueryComponents QueryComponents(DataServiceProtocolVersion maxProtocolVersion)
        {
            if (this.queryComponents == null)
            {
                Type type = typeof(TElement);
                type = (PrimitiveType.IsKnownType(type) || WebUtil.IsCLRTypeCollection(type, maxProtocolVersion)) ? type : TypeSystem.GetElementType(type);
                this.queryComponents = new System.Data.Services.Client.QueryComponents(this.requestUri, Util.DataServiceVersionEmpty, type, null, null);
            }
            return this.queryComponents;
        }

        public override string ToString()
        {
            return this.requestUri.ToString();
        }

        public override Type ElementType
        {
            get
            {
                return typeof(TElement);
            }
        }

        internal override ProjectionPlan Plan
        {
            get
            {
                return this.plan;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.requestUri;
            }
            internal set
            {
                this.requestUri = value;
            }
        }
    }
}

