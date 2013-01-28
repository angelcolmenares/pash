namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class TransactedRegistryAuditRule : AuditRule
    {
        internal TransactedRegistryAuditRule(IdentityReference identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        internal TransactedRegistryAuditRule(string identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        internal TransactedRegistryAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
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

