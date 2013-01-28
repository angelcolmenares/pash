namespace System.Data.Services.Client
{
    using System;
    using System.Data.Services.Common;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    internal class ResponseInfo
    {
        private readonly DataServiceContext context;
        private readonly bool ignoreMissingProperties;
        private readonly System.Data.Services.Client.MergeOption mergeOption;

        internal ResponseInfo(DataServiceContext context, System.Data.Services.Client.MergeOption mergeOption, bool ignoreMissingProperties)
        {
            this.context = context;
            this.mergeOption = mergeOption;
            this.ignoreMissingProperties = ignoreMissingProperties;
        }

        internal void FireReadingEntityEvent(object element, XElement data, Uri baseUri)
        {
            this.context.FireReadingEntityEvent(element, data, baseUri);
        }

        internal Type ResolveTypeFromName(string wireName)
        {
            return this.context.ResolveTypeFromName(wireName);
        }

        internal bool ApplyingChanges
        {
            get
            {
                return this.context.ApplyingChanges;
            }
            set
            {
                this.context.ApplyingChanges = value;
            }
        }

        internal UriResolver BaseUriResolver
        {
            get
            {
                return this.context.BaseUriResolver;
            }
        }

        internal DataServiceContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal string DataNamespace
        {
            get
            {
                return this.context.DataNamespace;
            }
        }

        internal System.Data.Services.Client.EntityTracker EntityTracker
        {
            get
            {
                return this.context.EntityTracker;
            }
        }

        internal bool HasReadingEntityHandlers
        {
            get
            {
                return this.context.HasReadingEntityHandlers;
            }
        }

        internal bool IgnoreMissingProperties
        {
            get
            {
                return this.ignoreMissingProperties;
            }
        }

        internal DataServiceProtocolVersion MaxProtocolVersion
        {
            get
            {
                return this.context.MaxProtocolVersion;
            }
        }

        internal System.Data.Services.Client.MergeOption MergeOption
        {
            get
            {
                return this.mergeOption;
            }
        }

        internal System.Data.Services.Client.TypeResolver TypeResolver { get; set; }

        internal Uri TypeScheme
        {
            get
            {
                return this.context.TypeScheme;
            }
        }
    }
}

