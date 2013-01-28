namespace System.Data.Services.Client
{
    using System;

    internal abstract class OperationDescriptor : Descriptor
    {
        private Uri metadata;
        private Uri target;
        private string title;

        internal OperationDescriptor() : base(EntityStates.Unchanged)
        {
        }

        internal override void ClearChanges()
        {
        }

        internal override System.Data.Services.Client.DescriptorKind DescriptorKind
        {
            get
            {
                return System.Data.Services.Client.DescriptorKind.OperationDescriptor;
            }
        }

        public Uri Metadata
        {
            get
            {
                return this.metadata;
            }
            internal set
            {
                this.metadata = value;
            }
        }

        public Uri Target
        {
            get
            {
                return this.target;
            }
            internal set
            {
                this.target = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            internal set
            {
                this.title = value;
            }
        }
    }
}

