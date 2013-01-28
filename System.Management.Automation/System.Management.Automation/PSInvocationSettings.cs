namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Host;
    using System.Security.Principal;
    using System.Threading;

    public sealed class PSInvocationSettings
    {
        private bool addToHistory = false;
        private System.Threading.ApartmentState apartmentState = System.Threading.ApartmentState.Unknown;
        private ActionPreference? errorActionPreference = null;
        private bool flowImpersonationPolicy;
        private PSHost host = null;
        private bool invokeAndDisconnect;
        private System.Management.Automation.RemoteStreamOptions remoteStreamOptions = 0;
        private WindowsIdentity windowsIdentityToImpersonate;

        public bool AddToHistory
        {
            get
            {
                return this.addToHistory;
            }
            set
            {
                this.addToHistory = value;
            }
        }

        public System.Threading.ApartmentState ApartmentState
        {
            get
            {
                return this.apartmentState;
            }
            set
            {
                this.apartmentState = value;
            }
        }

        public ActionPreference? ErrorActionPreference
        {
            get
            {
                return this.errorActionPreference;
            }
            set
            {
                this.errorActionPreference = value;
            }
        }

        public bool FlowImpersonationPolicy
        {
            get
            {
                return this.flowImpersonationPolicy;
            }
            set
            {
                this.flowImpersonationPolicy = value;
            }
        }

        public PSHost Host
        {
            get
            {
                return this.host;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Host");
                }
                this.host = value;
            }
        }

        internal bool InvokeAndDisconnect
        {
            get
            {
                return this.invokeAndDisconnect;
            }
            set
            {
                this.invokeAndDisconnect = value;
            }
        }

        public System.Management.Automation.RemoteStreamOptions RemoteStreamOptions
        {
            get
            {
                return this.remoteStreamOptions;
            }
            set
            {
                this.remoteStreamOptions = value;
            }
        }

        internal WindowsIdentity WindowsIdentityToImpersonate
        {
            get
            {
                return this.windowsIdentityToImpersonate;
            }
            set
            {
                this.windowsIdentityToImpersonate = value;
            }
        }
    }
}

