namespace System.Data.Services.Client
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class StreamDescriptor : Descriptor
    {
        private System.Data.Services.Client.EntityDescriptor entityDescriptor;
        private DataServiceStreamLink streamLink;
        private StreamDescriptor transientNamedStreamInfo;

        internal StreamDescriptor(System.Data.Services.Client.EntityDescriptor entityDescriptor) : base(EntityStates.Unchanged)
        {
            this.streamLink = new DataServiceStreamLink(null);
            this.entityDescriptor = entityDescriptor;
        }

        internal StreamDescriptor(string name, System.Data.Services.Client.EntityDescriptor entityDescriptor) : base(EntityStates.Unchanged)
        {
            this.streamLink = new DataServiceStreamLink(name);
            this.entityDescriptor = entityDescriptor;
        }

        internal override void ClearChanges()
        {
            this.transientNamedStreamInfo = null;
            this.CloseSaveStream();
        }

        internal void CloseSaveStream()
        {
            if (this.SaveStream != null)
            {
                DataServiceSaveStream saveStream = this.SaveStream;
                this.SaveStream = null;
                saveStream.Close();
            }
        }

        internal Uri GetLatestEditLink()
        {
            if ((this.transientNamedStreamInfo != null) && (this.transientNamedStreamInfo.EditLink != null))
            {
                return this.transientNamedStreamInfo.EditLink;
            }
            return this.EditLink;
        }

        internal string GetLatestETag()
        {
            if ((this.transientNamedStreamInfo != null) && (this.transientNamedStreamInfo.ETag != null))
            {
                return this.transientNamedStreamInfo.ETag;
            }
            return this.ETag;
        }

        internal static void MergeStreamDescriptor(StreamDescriptor existingStreamDescriptor, StreamDescriptor newStreamDescriptor)
        {
            if (newStreamDescriptor.SelfLink != null)
            {
                existingStreamDescriptor.SelfLink = newStreamDescriptor.SelfLink;
            }
            if (newStreamDescriptor.EditLink != null)
            {
                existingStreamDescriptor.EditLink = newStreamDescriptor.EditLink;
            }
            if (newStreamDescriptor.ContentType != null)
            {
                existingStreamDescriptor.ContentType = newStreamDescriptor.ContentType;
            }
            if (newStreamDescriptor.ETag != null)
            {
                existingStreamDescriptor.ETag = newStreamDescriptor.ETag;
            }
        }

        internal string ContentType
        {
            get
            {
                return this.streamLink.ContentType;
            }
            set
            {
                this.streamLink.ContentType = value;
            }
        }

        internal override System.Data.Services.Client.DescriptorKind DescriptorKind
        {
            get
            {
                return System.Data.Services.Client.DescriptorKind.NamedStream;
            }
        }

        internal Uri EditLink
        {
            get
            {
                return this.streamLink.EditLink;
            }
            set
            {
                this.streamLink.EditLink = value;
            }
        }

        public System.Data.Services.Client.EntityDescriptor EntityDescriptor
        {
            get
            {
                return this.entityDescriptor;
            }
            set
            {
                this.entityDescriptor = value;
            }
        }

        internal string ETag
        {
            get
            {
                return this.streamLink.ETag;
            }
            set
            {
                this.streamLink.ETag = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.streamLink.Name;
            }
        }

        internal DataServiceSaveStream SaveStream { get; set; }

        internal Uri SelfLink
        {
            get
            {
                return this.streamLink.SelfLink;
            }
            set
            {
                this.streamLink.SelfLink = value;
            }
        }

        public DataServiceStreamLink StreamLink
        {
            get
            {
                return this.streamLink;
            }
        }

        internal StreamDescriptor TransientNamedStreamInfo
        {
            set
            {
                if (this.transientNamedStreamInfo == null)
                {
                    this.transientNamedStreamInfo = value;
                }
                else
                {
                    MergeStreamDescriptor(this.transientNamedStreamInfo, value);
                }
            }
        }
    }
}

