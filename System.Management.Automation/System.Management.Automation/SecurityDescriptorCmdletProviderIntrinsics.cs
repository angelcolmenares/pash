namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Security.AccessControl;

    public sealed class SecurityDescriptorCmdletProviderIntrinsics
    {
        private Cmdlet cmdlet;
        private SessionStateInternal sessionState;

        private SecurityDescriptorCmdletProviderIntrinsics()
        {
        }

        internal SecurityDescriptorCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.sessionState = cmdlet.Context.EngineSessionState;
        }

        internal SecurityDescriptorCmdletProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public Collection<PSObject> Get(string path, AccessControlSections includeSections)
        {
            return this.sessionState.GetSecurityDescriptor(path, includeSections);
        }

        internal void Get(string path, AccessControlSections includeSections, CmdletProviderContext context)
        {
            this.sessionState.GetSecurityDescriptor(path, includeSections, context);
        }

        public ObjectSecurity NewFromPath(string path, AccessControlSections includeSections)
        {
            return this.sessionState.NewSecurityDescriptorFromPath(path, includeSections);
        }

        public ObjectSecurity NewOfType(string providerId, string type, AccessControlSections includeSections)
        {
            return this.sessionState.NewSecurityDescriptorOfType(providerId, type, includeSections);
        }

        public Collection<PSObject> Set(string path, ObjectSecurity sd)
        {
            return this.sessionState.SetSecurityDescriptor(path, sd);
        }

        internal void Set(string path, ObjectSecurity sd, CmdletProviderContext context)
        {
            this.sessionState.SetSecurityDescriptor(path, sd, context);
        }
    }
}

