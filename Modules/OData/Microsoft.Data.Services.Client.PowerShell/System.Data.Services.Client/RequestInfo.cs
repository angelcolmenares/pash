namespace System.Data.Services.Client
{
    using System;
    using System.CodeDom.Compiler;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    internal class RequestInfo
    {
        private readonly DataServiceContext context;

        internal RequestInfo(DataServiceContext context)
        {
            this.context = context;
        }

        internal void FireSendingRequest(SendingRequestEventArgs eventArgs)
        {
            this.context.FireSendingRequest(eventArgs);
        }

        internal void FireSendingRequest2(SendingRequest2EventArgs eventArgs)
        {
            this.context.FireSendingRequest2(eventArgs);
        }

        internal void FireWritingEntityEvent(object element, XElement data, Uri baseUri)
        {
            this.context.FireWritingEntityEvent(element, data, baseUri);
        }

        internal ResponseInfo GetDeserializationInfo(MergeOption? mergeOption)
        {
            return new ResponseInfo(this.context, mergeOption.HasValue ? mergeOption.Value : this.context.MergeOption, this.context.IgnoreMissingProperties);
        }

        internal string GetServerTypeName(EntityDescriptor descriptor)
        {
            if (this.HasResolveName)
            {
                Type type = descriptor.Entity.GetType();
                if (this.IsUserSuppliedResolver)
                {
                    return (this.ResolveNameFromType(type) ?? descriptor.GetLatestServerTypeName());
                }
                return (descriptor.GetLatestServerTypeName() ?? this.ResolveNameFromType(type));
            }
            return descriptor.GetLatestServerTypeName();
        }

        internal string GetServerTypeName(ClientTypeAnnotation clientTypeAnnotation)
        {
            return this.ResolveNameFromType(clientTypeAnnotation.ElementType);
        }

        internal void InternalSendRequest(HttpWebRequest request)
        {
            this.context.InternalSendRequest(request);
        }

        internal string ResolveNameFromType(Type type)
        {
            return this.context.ResolveNameFromType(type);
        }

        internal InvalidOperationException ValidateResponseVersion(Version responseVersion)
        {
            if ((responseVersion != null) && (responseVersion > this.context.MaxProtocolVersionAsVersion))
            {
                return System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_ResponseVersionIsBiggerThanProtocolVersion(responseVersion.ToString(), this.context.MaxProtocolVersion.ToString()));
            }
            return null;
        }

        internal DataServiceResponsePreference AddAndUpdateResponsePreference
        {
            get
            {
                return this.context.AddAndUpdateResponsePreference;
            }
        }

        internal UriResolver BaseUriResolver
        {
            get
            {
                return this.context.BaseUriResolver;
            }
        }

        internal ICredentials Credentials
        {
            get
            {
                return this.context.Credentials;
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

        internal bool HasResolveName
        {
            get
            {
                return (this.context.ResolveName != null);
            }
        }

        internal bool HasSendingRequest2EventHandlers
        {
            get
            {
                return this.context.HasSendingRequest2EventHandlers;
            }
        }

        internal bool HasSendingRequestEventHandlers
        {
            get
            {
                return this.context.HasSendingRequestEventHandlers;
            }
        }

        internal bool HasWritingEventHandlers
        {
            get
            {
                return this.context.HasWritingEntityHandlers;
            }
        }

        internal bool IgnoreResourceNotFoundException
        {
            get
            {
                return this.context.IgnoreResourceNotFoundException;
            }
        }

        internal bool IsUserSuppliedResolver
        {
            get
            {
                GeneratedCodeAttribute attribute = this.context.ResolveName.Method.GetCustomAttributes(false).OfType<GeneratedCodeAttribute>().FirstOrDefault<GeneratedCodeAttribute>();
                if (attribute != null)
                {
                    return (attribute.Tool != "System.Data.Services.Design");
                }
                return true;
            }
        }

        internal DataServiceProtocolVersion MaxProtocolVersion
        {
            get
            {
                return this.context.MaxProtocolVersion;
            }
        }

        internal Version MaxProtocolVersionAsVersion
        {
            get
            {
                return this.context.MaxProtocolVersionAsVersion;
            }
        }

        internal int Timeout
        {
            get
            {
                return this.context.Timeout;
            }
        }

        internal Uri TypeScheme
        {
            get
            {
                return this.context.TypeScheme;
            }
        }

        internal bool UsePostTunneling
        {
            get
            {
                return this.context.UsePostTunneling;
            }
        }
    }
}

