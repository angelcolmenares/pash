namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("State = {state}, Uri = {editLink}, Element = {entity.GetType().ToString()}")]
    internal sealed class EntityDescriptor : Descriptor
    {
        private Uri addToUri;
        private StreamDescriptor defaultStreamDescriptor;
        private Uri editLink;
        private object entity;
        private string etag;
        private string identity;
        internal readonly DataServiceProtocolVersion MaxProtocolVersion;
        private List<OperationDescriptor> operationDescriptors;
        private EntityDescriptor parentDescriptor;
        private string parentProperty;
        private Dictionary<string, LinkInfo> relatedEntityLinks;
        private Uri selfLink;
        private string serverTypeName;
        private Dictionary<string, StreamDescriptor> streamDescriptors;
        private EntityDescriptor transientEntityDescriptor;

        internal EntityDescriptor(DataServiceProtocolVersion maxProtocolVersion) : base(EntityStates.Unchanged)
        {
            this.MaxProtocolVersion = maxProtocolVersion;
        }

        internal EntityDescriptor(string identity, Uri selfLink, Uri editLink, Uri addToUri, object entity, EntityDescriptor parentEntity, string parentProperty, string etag, EntityStates state, DataServiceProtocolVersion maxProtocolVersion) : base(state)
        {
            this.identity = identity;
            this.selfLink = selfLink;
            this.editLink = editLink;
            this.addToUri = addToUri;
            this.parentDescriptor = parentEntity;
            this.parentProperty = parentProperty;
            this.MaxProtocolVersion = maxProtocolVersion;
            this.Entity = entity;
            this.etag = etag;
        }

        internal void AddAssociationLink(string propertyName, Uri associationUri)
        {
            this.GetLinkInfo(propertyName).AssociationLink = associationUri;
        }

        internal void AddNavigationLink(string propertyName, Uri navigationUri)
        {
            this.GetLinkInfo(propertyName).NavigationLink = navigationUri;
        }

        internal void AddOperationDescriptor(OperationDescriptor operationDescriptor)
        {
            if (this.operationDescriptors == null)
            {
                this.operationDescriptors = new List<OperationDescriptor>();
            }
            this.operationDescriptors.Add(operationDescriptor);
        }

        internal StreamDescriptor AddStreamInfoIfNotPresent(string name)
        {
            StreamDescriptor descriptor;
            if (this.streamDescriptors == null)
            {
                this.streamDescriptors = new Dictionary<string, StreamDescriptor>(StringComparer.Ordinal);
            }
            if (!this.streamDescriptors.TryGetValue(name, out descriptor))
            {
                descriptor = new StreamDescriptor(name, this);
                this.streamDescriptors.Add(name, descriptor);
            }
            return descriptor;
        }

        internal override void ClearChanges()
        {
            this.transientEntityDescriptor = null;
            this.CloseSaveStream();
        }

        internal void CloseSaveStream()
        {
            if (this.defaultStreamDescriptor != null)
            {
                this.defaultStreamDescriptor.CloseSaveStream();
            }
        }

        private StreamDescriptor CreateDefaultStreamDescriptor()
        {
            if (this.defaultStreamDescriptor == null)
            {
                this.defaultStreamDescriptor = new StreamDescriptor(this);
            }
            return this.defaultStreamDescriptor;
        }

        internal Uri GetLatestEditLink()
        {
            if ((this.TransientEntityDescriptor != null) && (this.TransientEntityDescriptor.EditLink != null))
            {
                return this.TransientEntityDescriptor.EditLink;
            }
            return this.EditLink;
        }

        internal Uri GetLatestEditStreamUri()
        {
            if ((this.TransientEntityDescriptor != null) && (this.TransientEntityDescriptor.EditStreamUri != null))
            {
                return this.TransientEntityDescriptor.EditStreamUri;
            }
            return this.EditStreamUri;
        }

        internal string GetLatestETag()
        {
            if ((this.TransientEntityDescriptor != null) && !string.IsNullOrEmpty(this.TransientEntityDescriptor.ETag))
            {
                return this.TransientEntityDescriptor.ETag;
            }
            return this.ETag;
        }

        internal string GetLatestIdentity()
        {
            if ((this.TransientEntityDescriptor != null) && (this.TransientEntityDescriptor.Identity != null))
            {
                return this.TransientEntityDescriptor.Identity;
            }
            return this.Identity;
        }

        internal string GetLatestServerTypeName()
        {
            if ((this.TransientEntityDescriptor != null) && !string.IsNullOrEmpty(this.TransientEntityDescriptor.ServerTypeName))
            {
                return this.TransientEntityDescriptor.ServerTypeName;
            }
            return this.ServerTypeName;
        }

        internal string GetLatestStreamETag()
        {
            if ((this.TransientEntityDescriptor != null) && !string.IsNullOrEmpty(this.TransientEntityDescriptor.StreamETag))
            {
                return this.TransientEntityDescriptor.StreamETag;
            }
            return this.StreamETag;
        }

        private Uri GetLink(bool queryLink)
        {
            if (queryLink && (this.SelfLink != null))
            {
                return this.SelfLink;
            }
            Uri latestEditLink = this.GetLatestEditLink();
            if (latestEditLink != null)
            {
                return latestEditLink;
            }
            if (base.State != EntityStates.Added)
            {
                throw new ArgumentNullException(System.Data.Services.Client.Strings.EntityDescriptor_MissingSelfEditLink(this.identity));
            }
            if (this.addToUri != null)
            {
                return this.addToUri;
            }
            return Util.CreateUri(this.parentProperty, UriKind.Relative);
        }

        private LinkInfo GetLinkInfo(string propertyName)
        {
            if (this.relatedEntityLinks == null)
            {
                this.relatedEntityLinks = new Dictionary<string, LinkInfo>(StringComparer.Ordinal);
            }
            LinkInfo info = null;
            if (!this.relatedEntityLinks.TryGetValue(propertyName, out info))
            {
                info = new LinkInfo(propertyName);
                this.relatedEntityLinks[propertyName] = info;
            }
            return info;
        }

        internal Uri GetNavigationLink(UriResolver baseUriResolver, ClientPropertyAnnotation property)
        {
            LinkInfo linkInfo = null;
            Uri navigationLink = null;
            if (this.TryGetLinkInfo(property.PropertyName, out linkInfo))
            {
                navigationLink = linkInfo.NavigationLink;
            }
            if (navigationLink == null)
            {
                Uri requestUri = Util.CreateUri(property.PropertyName + (property.IsEntityCollection ? "()" : string.Empty), UriKind.Relative);
                navigationLink = Util.CreateUri(this.GetResourceUri(baseUriResolver, true), requestUri);
            }
            return navigationLink;
        }

        internal LinkDescriptor GetRelatedEnd(DataServiceProtocolVersion maxProtocolVersion)
        {
            return new LinkDescriptor(this.parentDescriptor.entity, this.parentProperty, this.entity, maxProtocolVersion);
        }

        internal Uri GetResourceUri(UriResolver baseUriResolver, bool queryLink)
        {
            LinkInfo info;
            if (this.parentDescriptor == null)
            {
                return this.GetLink(queryLink);
            }
            if (this.parentDescriptor.Identity == null)
            {
                return Util.CreateUri(Util.CreateUri(baseUriResolver.GetBaseUriWithSlash(), new Uri("$" + this.parentDescriptor.ChangeOrder.ToString(CultureInfo.InvariantCulture), UriKind.Relative)), Util.CreateUri(this.parentProperty, UriKind.Relative));
            }
            if (this.parentDescriptor.TryGetLinkInfo(this.parentProperty, out info) && (info.NavigationLink != null))
            {
                return info.NavigationLink;
            }
            return Util.CreateUri(this.parentDescriptor.GetLink(queryLink), this.GetLink(queryLink));
        }

        internal bool IsRelatedEntity(LinkDescriptor related)
        {
            if (this.entity != related.Source)
            {
                return (this.entity == related.Target);
            }
            return true;
        }

        internal void MergeLinkInfo(LinkInfo linkInfo)
        {
            if (this.relatedEntityLinks == null)
            {
                this.relatedEntityLinks = new Dictionary<string, LinkInfo>(StringComparer.Ordinal);
            }
            LinkInfo info = null;
            if (!this.relatedEntityLinks.TryGetValue(linkInfo.Name, out info))
            {
                this.relatedEntityLinks[linkInfo.Name] = linkInfo;
            }
            else
            {
                if (linkInfo.AssociationLink != null)
                {
                    info.AssociationLink = linkInfo.AssociationLink;
                }
                if (linkInfo.NavigationLink != null)
                {
                    info.NavigationLink = linkInfo.NavigationLink;
                }
            }
        }

        internal void MergeStreamDescriptor(StreamDescriptor materializedStreamDescriptor)
        {
            if (this.streamDescriptors == null)
            {
                this.streamDescriptors = new Dictionary<string, StreamDescriptor>(StringComparer.Ordinal);
            }
            StreamDescriptor descriptor = null;
            if (!this.streamDescriptors.TryGetValue(materializedStreamDescriptor.Name, out descriptor))
            {
                this.streamDescriptors[materializedStreamDescriptor.Name] = materializedStreamDescriptor;
                materializedStreamDescriptor.EntityDescriptor = this;
            }
            else
            {
                StreamDescriptor.MergeStreamDescriptor(descriptor, materializedStreamDescriptor);
            }
        }

        internal bool TryGetLinkInfo(string propertyName, out LinkInfo linkInfo)
        {
            Util.CheckArgumentNullAndEmpty(propertyName, "propertyName");
            linkInfo = null;
            return (((this.TransientEntityDescriptor != null) && this.TransientEntityDescriptor.TryGetLinkInfo(propertyName, out linkInfo)) || ((this.relatedEntityLinks != null) && this.relatedEntityLinks.TryGetValue(propertyName, out linkInfo)));
        }

        internal bool TryGetNamedStreamInfo(string name, out StreamDescriptor namedStreamInfo)
        {
            namedStreamInfo = null;
            return ((this.streamDescriptors != null) && this.streamDescriptors.TryGetValue(name, out namedStreamInfo));
        }

        internal StreamDescriptor DefaultStreamDescriptor
        {
            get
            {
                return this.defaultStreamDescriptor;
            }
        }

        internal override System.Data.Services.Client.DescriptorKind DescriptorKind
        {
            get
            {
                return System.Data.Services.Client.DescriptorKind.Entity;
            }
        }

        public Uri EditLink
        {
            get
            {
                return this.editLink;
            }
            internal set
            {
                this.editLink = value;
            }
        }

        public Uri EditStreamUri
        {
            get
            {
                if (this.defaultStreamDescriptor == null)
                {
                    return null;
                }
                return this.defaultStreamDescriptor.EditLink;
            }
            internal set
            {
                this.CreateDefaultStreamDescriptor().EditLink = value;
            }
        }

        public object Entity
        {
            get
            {
                return this.entity;
            }
            internal set
            {
                this.entity = value;
                ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
                if ((value != null) && model.GetClientTypeAnnotation(model.GetOrCreateEdmType(value.GetType())).IsMediaLinkEntry)
                {
                    this.CreateDefaultStreamDescriptor();
                }
            }
        }

        public string ETag
        {
            get
            {
                return this.etag;
            }
            internal set
            {
                this.etag = value;
            }
        }

        public string Identity
        {
            get
            {
                return this.identity;
            }
            internal set
            {
                Util.CheckArgumentNullAndEmpty(value, "Identity");
                this.identity = value;
                this.parentDescriptor = null;
                this.parentProperty = null;
                this.addToUri = null;
            }
        }

        internal bool IsDeepInsert
        {
            get
            {
                return (this.parentDescriptor != null);
            }
        }

        internal bool IsMediaLinkEntry
        {
            get
            {
                return (this.defaultStreamDescriptor != null);
            }
        }

        internal override bool IsModified
        {
            get
            {
                return (base.IsModified || ((this.defaultStreamDescriptor != null) && (this.defaultStreamDescriptor.SaveStream != null)));
            }
        }

        public ReadOnlyCollection<LinkInfo> LinkInfos
        {
            get
            {
                if (this.relatedEntityLinks != null)
                {
                    return this.relatedEntityLinks.Values.ToList<LinkInfo>().AsReadOnly();
                }
                return new List<LinkInfo>(0).AsReadOnly();
            }
        }

        public ReadOnlyCollection<OperationDescriptor> OperationDescriptors
        {
            get
            {
                if (this.operationDescriptors != null)
                {
                    return this.operationDescriptors.AsReadOnly();
                }
                return new List<OperationDescriptor>(0).AsReadOnly();
            }
        }

        internal object ParentEntity
        {
            get
            {
                if (this.parentDescriptor == null)
                {
                    return null;
                }
                return this.parentDescriptor.entity;
            }
        }

        public EntityDescriptor ParentForInsert
        {
            get
            {
                return this.parentDescriptor;
            }
        }

        public string ParentPropertyForInsert
        {
            get
            {
                return this.parentProperty;
            }
        }

        public Uri ReadStreamUri
        {
            get
            {
                if (this.defaultStreamDescriptor == null)
                {
                    return null;
                }
                return this.defaultStreamDescriptor.SelfLink;
            }
            internal set
            {
                this.CreateDefaultStreamDescriptor().SelfLink = value;
            }
        }

        internal DataServiceSaveStream SaveStream
        {
            get
            {
                if (this.defaultStreamDescriptor == null)
                {
                    return null;
                }
                return this.defaultStreamDescriptor.SaveStream;
            }
            set
            {
                this.CreateDefaultStreamDescriptor().SaveStream = value;
            }
        }

        public Uri SelfLink
        {
            get
            {
                return this.selfLink;
            }
            internal set
            {
                this.selfLink = value;
            }
        }

        public string ServerTypeName
        {
            get
            {
                return this.serverTypeName;
            }
            internal set
            {
                this.serverTypeName = value;
            }
        }

        public ReadOnlyCollection<StreamDescriptor> StreamDescriptors
        {
            get
            {
                if (this.streamDescriptors != null)
                {
                    return this.streamDescriptors.Values.ToList<StreamDescriptor>().AsReadOnly();
                }
                return new List<StreamDescriptor>(0).AsReadOnly();
            }
        }

        public string StreamETag
        {
            get
            {
                if (this.defaultStreamDescriptor == null)
                {
                    return null;
                }
                return this.defaultStreamDescriptor.ETag;
            }
            internal set
            {
                this.defaultStreamDescriptor.ETag = value;
            }
        }

        internal EntityStates StreamState
        {
            get
            {
                if (this.defaultStreamDescriptor == null)
                {
                    return EntityStates.Unchanged;
                }
                return this.defaultStreamDescriptor.State;
            }
            set
            {
                this.defaultStreamDescriptor.State = value;
            }
        }

        internal EntityDescriptor TransientEntityDescriptor
        {
            get
            {
                return this.transientEntityDescriptor;
            }
            set
            {
                if (this.transientEntityDescriptor == null)
                {
                    this.transientEntityDescriptor = value;
                }
                else
                {
                    AtomMaterializerLog.MergeEntityDescriptorInfo(this.transientEntityDescriptor, value, true, MergeOption.OverwriteChanges);
                }
                if ((value.streamDescriptors != null) && (this.streamDescriptors != null))
                {
                    foreach (StreamDescriptor descriptor in value.streamDescriptors.Values)
                    {
                        StreamDescriptor descriptor2;
                        if (this.streamDescriptors.TryGetValue(descriptor.Name, out descriptor2))
                        {
                            descriptor2.TransientNamedStreamInfo = descriptor;
                        }
                    }
                }
            }
        }
    }
}

