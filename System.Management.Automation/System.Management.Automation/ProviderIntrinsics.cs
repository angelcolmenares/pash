namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    public sealed class ProviderIntrinsics
    {
        private ChildItemCmdletProviderIntrinsics childItem;
        private InternalCommand cmdlet;
        private ContentCmdletProviderIntrinsics content;
        private ItemCmdletProviderIntrinsics item;
        private PropertyCmdletProviderIntrinsics property;
        private SecurityDescriptorCmdletProviderIntrinsics securityDescriptor;

        private ProviderIntrinsics()
        {
        }

        internal ProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.item = new ItemCmdletProviderIntrinsics(cmdlet);
            this.childItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
            this.content = new ContentCmdletProviderIntrinsics(cmdlet);
            this.property = new PropertyCmdletProviderIntrinsics(cmdlet);
            this.securityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(cmdlet);
        }

        internal ProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.item = new ItemCmdletProviderIntrinsics(sessionState);
            this.childItem = new ChildItemCmdletProviderIntrinsics(sessionState);
            this.content = new ContentCmdletProviderIntrinsics(sessionState);
            this.property = new PropertyCmdletProviderIntrinsics(sessionState);
            this.securityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(sessionState);
        }

        public ChildItemCmdletProviderIntrinsics ChildItem
        {
            get
            {
                return this.childItem;
            }
        }

        public ContentCmdletProviderIntrinsics Content
        {
            get
            {
                return this.content;
            }
        }

        public ItemCmdletProviderIntrinsics Item
        {
            get
            {
                return this.item;
            }
        }

        public PropertyCmdletProviderIntrinsics Property
        {
            get
            {
                return this.property;
            }
        }

        public SecurityDescriptorCmdletProviderIntrinsics SecurityDescriptor
        {
            get
            {
                return this.securityDescriptor;
            }
        }
    }
}

