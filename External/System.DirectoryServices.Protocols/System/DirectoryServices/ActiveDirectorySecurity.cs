using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace System.DirectoryServices
{
	[Flags]
	internal enum SecurityMasks
	{
		None = 0,
		Owner = 1,
		Group = 2,
		Dacl = 4,
		Sacl = 8
	}

	public class ActiveDirectorySecurity : DirectoryObjectSecurity
	{
		private SecurityMasks securityMaskUsedInRetrieval;
		
		public override Type AccessRightType
		{
			get
			{
				return typeof(ActiveDirectoryRights);
			}
		}
		
		public override Type AccessRuleType
		{
			get
			{
				return typeof(ActiveDirectoryAccessRule);
			}
		}
		
		public override Type AuditRuleType
		{
			get
			{
				return typeof(ActiveDirectoryAuditRule);
			}
		}
		
		public ActiveDirectorySecurity()
		{
			this.securityMaskUsedInRetrieval = SecurityMasks.Owner | SecurityMasks.Group | SecurityMasks.Dacl | SecurityMasks.Sacl;
		}
		
		internal ActiveDirectorySecurity(byte[] sdBinaryForm, SecurityMasks securityMask) : base(new CommonSecurityDescriptor(true, true, sdBinaryForm, 0))
		{
			this.securityMaskUsedInRetrieval = SecurityMasks.Owner | SecurityMasks.Group | SecurityMasks.Dacl | SecurityMasks.Sacl;
			this.securityMaskUsedInRetrieval = securityMask;
		}
		
		public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			return new ActiveDirectoryAccessRule(identityReference, accessMask, type, Guid.Empty, isInherited, inheritanceFlags, propagationFlags, Guid.Empty);
		}
		
		public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectGuid, Guid inheritedObjectGuid)
		{
			return new ActiveDirectoryAccessRule(identityReference, accessMask, type, objectGuid, isInherited, inheritanceFlags, propagationFlags, inheritedObjectGuid);
		}
			
		public void AddAccessRule(ActiveDirectoryAccessRule rule)
		{
			if (this.DaclRetrieved())
			{
				base.AddAccessRule(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public void AddAuditRule(ActiveDirectoryAuditRule rule)
		{
			if (this.SaclRetrieved())
			{
				base.AddAuditRule(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new ActiveDirectoryAuditRule(identityReference, accessMask, flags, Guid.Empty, isInherited, inheritanceFlags, propagationFlags, Guid.Empty);
		}
		
		public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectGuid, Guid inheritedObjectGuid)
		{
			return new ActiveDirectoryAuditRule(identityReference, accessMask, flags, objectGuid, isInherited, inheritanceFlags, propagationFlags, inheritedObjectGuid);
		}
		
		private bool DaclRetrieved()
		{
			return (this.securityMaskUsedInRetrieval & SecurityMasks.Dacl) != SecurityMasks.None;
		}
		
		internal bool IsModified()
		{
			bool flag;
			bool auditRulesModified;
			base.ReadLock();
			try
			{
				if (base.OwnerModified || base.GroupModified || base.AccessRulesModified)
				{
					auditRulesModified = true;
				}
				else
				{
					auditRulesModified = base.AuditRulesModified;
				}
				flag = auditRulesModified;
			}
			finally
			{
				base.ReadUnlock();
			}
			return flag;
		}
		
		public override bool ModifyAccessRule(AccessControlModification modification, AccessRule rule, out bool modified)
		{
			if (this.DaclRetrieved())
			{
				return base.ModifyAccessRule(modification, rule, out modified);
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public override bool ModifyAuditRule(AccessControlModification modification, AuditRule rule, out bool modified)
		{
			if (this.SaclRetrieved())
			{
				return base.ModifyAuditRule(modification, rule, out modified);
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public void PurgeAccessRules(IdentityReference identity)
		{
			if (this.DaclRetrieved())
			{
				base.PurgeAccessRules(identity);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public void PurgeAuditRules(IdentityReference identity)
		{
			if (this.SaclRetrieved())
			{
				base.PurgeAuditRules(identity);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public void RemoveAccess(IdentityReference identity, AccessControlType type)
		{
			if (this.DaclRetrieved())
			{
				ActiveDirectoryAccessRule activeDirectoryAccessRule = new ActiveDirectoryAccessRule(identity, ActiveDirectoryRights.GenericRead, type, ActiveDirectorySecurityInheritance.None);
				base.RemoveAccessRuleAll(activeDirectoryAccessRule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public bool RemoveAccessRule(ActiveDirectoryAccessRule rule)
		{
			if (this.DaclRetrieved())
			{
				return base.RemoveAccessRule(rule);
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public void RemoveAccessRuleSpecific(ActiveDirectoryAccessRule rule)
		{
			if (this.DaclRetrieved())
			{
				base.RemoveAccessRuleSpecific(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public void RemoveAudit(IdentityReference identity)
		{
			if (this.SaclRetrieved())
			{
				ActiveDirectoryAuditRule activeDirectoryAuditRule = new ActiveDirectoryAuditRule(identity, ActiveDirectoryRights.GenericRead, AuditFlags.Success | AuditFlags.Failure, ActiveDirectorySecurityInheritance.None);
				base.RemoveAuditRuleAll(activeDirectoryAuditRule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public bool RemoveAuditRule(ActiveDirectoryAuditRule rule)
		{
			if (this.SaclRetrieved())
			{
				return base.RemoveAuditRule(rule);
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public void RemoveAuditRuleSpecific(ActiveDirectoryAuditRule rule)
		{
			if (this.SaclRetrieved())
			{
				base.RemoveAuditRuleSpecific(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
		
		public void ResetAccessRule(ActiveDirectoryAccessRule rule)
		{
			if (this.DaclRetrieved())
			{
				base.ResetAccessRule(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		private bool SaclRetrieved()
		{
			return (this.securityMaskUsedInRetrieval & SecurityMasks.Sacl) != SecurityMasks.None;
		}
		
		public void SetAccessRule(ActiveDirectoryAccessRule rule)
		{
			if (this.DaclRetrieved())
			{
				base.SetAccessRule(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifyDacl");
			}
		}
		
		public void SetAuditRule(ActiveDirectoryAuditRule rule)
		{
			if (this.SaclRetrieved())
			{
				base.SetAuditRule(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException("CannotModifySacl");
			}
		}
	}

}

