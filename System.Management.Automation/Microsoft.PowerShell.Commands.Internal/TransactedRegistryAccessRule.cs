namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class TransactedRegistryAccessRule : AccessRule
    {
        internal TransactedRegistryAccessRule(IdentityReference identity, System.Security.AccessControl.RegistryRights registryRights, AccessControlType type) : this(identity, (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal TransactedRegistryAccessRule(string identity, System.Security.AccessControl.RegistryRights registryRights, AccessControlType type) : this(new NTAccount(identity), (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public TransactedRegistryAccessRule(IdentityReference identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        internal TransactedRegistryAccessRule(string identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        internal TransactedRegistryAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        public System.Security.AccessControl.RegistryRights RegistryRights
        {
            get
            {
                return (System.Security.AccessControl.RegistryRights) base.AccessMask;
            }
        }
    }
}

