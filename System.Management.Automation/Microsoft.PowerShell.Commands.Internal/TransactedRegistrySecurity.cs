namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;

    public sealed class TransactedRegistrySecurity : NativeObjectSecurity
    {
        private const string resBaseName = "RegistryProviderStrings";

        public TransactedRegistrySecurity() : base(true, ResourceType.RegistryKey)
        {
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal TransactedRegistrySecurity(SafeRegistryHandle hKey, string name, AccessControlSections includeSections) : base(true, ResourceType.RegistryKey, hKey, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(TransactedRegistrySecurity._HandleErrorCode), null)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.View, name).Demand();
        }

        private static Exception _HandleErrorCode(int errorCode, string name, SafeHandle handle, object context)
        {
            switch (errorCode)
            {
                case 2:
                    return new IOException(RegistryProviderStrings.Arg_RegKeyNotFound);

                case 6:
                    return new ArgumentException(RegistryProviderStrings.AccessControl_InvalidHandle);

                case 0x7b:
                    return new ArgumentException(RegistryProviderStrings.Arg_RegInvalidKeyName);
            }
            return null;
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new TransactedRegistryAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public void AddAccessRule(TransactedRegistryAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(TransactedRegistryAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new TransactedRegistryAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
        }

        internal AccessControlSections GetAccessControlSectionsFromChanges()
        {
            AccessControlSections none = AccessControlSections.None;
            if (base.AccessRulesModified)
            {
                none = AccessControlSections.Access;
            }
            if (base.AuditRulesModified)
            {
                none |= AccessControlSections.Audit;
            }
            if (base.OwnerModified)
            {
                none |= AccessControlSections.Owner;
            }
            if (base.GroupModified)
            {
                none |= AccessControlSections.Group;
            }
            return none;
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal void Persist(SafeRegistryHandle hKey, string keyName)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.Change, keyName).Demand();
            base.WriteLock();
            try
            {
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                if (accessControlSectionsFromChanges != AccessControlSections.None)
                {
                    bool flag;
                    bool flag2;
                    base.Persist(hKey, accessControlSectionsFromChanges);
                    base.AccessRulesModified = flag = false;
                    base.AuditRulesModified = flag2 = flag;
                    base.OwnerModified = base.GroupModified = flag2;
                }
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        public bool RemoveAccessRule(TransactedRegistryAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(TransactedRegistryAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(TransactedRegistryAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public bool RemoveAuditRule(TransactedRegistryAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(TransactedRegistryAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(TransactedRegistryAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(TransactedRegistryAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(TransactedRegistryAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(TransactedRegistryAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(RegistryRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(TransactedRegistryAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(TransactedRegistryAuditRule);
            }
        }
    }
}

