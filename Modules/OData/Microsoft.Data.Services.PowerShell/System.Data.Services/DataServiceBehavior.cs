namespace System.Data.Services
{
    using System;
    using System.Data.Services.Common;
    using System.Runtime.CompilerServices;

    internal sealed class DataServiceBehavior
    {
        internal DataServiceBehavior()
        {
            this.InvokeInterceptorsOnLinkDelete = true;
            this.AcceptCountRequests = true;
            this.AcceptProjectionRequests = true;
            this.AcceptAnyAllRequests = true;
            this.MaxProtocolVersion = DataServiceProtocolVersion.V1;
            this.IncludeAssociationLinksInResponse = false;
            this.UseMetadataKeyOrderForBuiltInProviders = false;
            this.AcceptSpatialLiteralsInQuery = true;
        }

        public bool AcceptAnyAllRequests { get; set; }

        public bool AcceptCountRequests { get; set; }

        public bool AcceptProjectionRequests { get; set; }

        public bool AcceptSpatialLiteralsInQuery { get; set; }

        public bool IncludeAssociationLinksInResponse { get; set; }

        public bool InvokeInterceptorsOnLinkDelete { get; set; }

        public DataServiceProtocolVersion MaxProtocolVersion { get; set; }

        internal bool ShouldIncludeAssociationLinksInResponse
        {
            get
            {
                return (this.IncludeAssociationLinksInResponse && (this.MaxProtocolVersion >= DataServiceProtocolVersion.V3));
            }
        }

        public bool UseMetadataKeyOrderForBuiltInProviders { get; set; }
    }
}

