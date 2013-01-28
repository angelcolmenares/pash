namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Provider;

    public sealed class CmdletProviderManagementIntrinsics
    {
        private SessionStateInternal sessionState;

        private CmdletProviderManagementIntrinsics()
        {
        }

        internal CmdletProviderManagementIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        internal static bool CheckProviderCapabilities(ProviderCapabilities capability, ProviderInfo provider)
        {
            return ((provider.Capabilities & capability) != ProviderCapabilities.None);
        }

        public Collection<ProviderInfo> Get(string name)
        {
            return this.sessionState.GetProvider(name);
        }

        public IEnumerable<ProviderInfo> GetAll()
        {
            return this.sessionState.ProviderList;
        }

        public ProviderInfo GetOne(string name)
        {
            return this.sessionState.GetSingleProvider(name);
        }

        internal int Count
        {
            get
            {
                return this.sessionState.ProviderCount;
            }
        }
    }
}

