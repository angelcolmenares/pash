namespace System.Management.Automation.Remoting
{
    using System;
    using System.Security.Principal;

    public sealed class PSPrincipal : IPrincipal
    {
        private PSIdentity psIdentity;
        private System.Security.Principal.WindowsIdentity windowsIdentity;

        public PSPrincipal(PSIdentity identity, System.Security.Principal.WindowsIdentity windowsIdentity)
        {
            this.psIdentity = identity;
            this.windowsIdentity = windowsIdentity;
        }

        internal bool IsInRole(WindowsBuiltInRole role)
        {
            if (this.windowsIdentity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(this.windowsIdentity);
                return principal.IsInRole(role);
            }
            return false;
        }

        public bool IsInRole(string role)
        {
            if (this.windowsIdentity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(this.windowsIdentity);
                return principal.IsInRole(role);
            }
            return false;
        }

        public PSIdentity Identity
        {
            get
            {
                return this.psIdentity;
            }
        }

        IIdentity IPrincipal.Identity
        {
            get
            {
                return this.Identity;
            }
        }

        public System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                return this.windowsIdentity;
            }
        }
    }
}

