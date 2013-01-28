using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADDomain : ADPartition
	{
		public ADPropertyValueCollection AllowedDNSSuffixes
		{
			get
			{
				return base["AllowedDNSSuffixes"];
			}
			set
			{
				base.SetValue("AllowedDNSSuffixes", value);
			}
		}

		public ADPropertyValueCollection ChildDomains
		{
			get
			{
				return base["ChildDomains"];
			}
		}

		public string ComputersContainer
		{
			get
			{
				return base.GetValue("ComputersContainer") as string;
			}
		}

		public string DomainControllersContainer
		{
			get
			{
				return base.GetValue("DomainControllersContainer") as string;
			}
		}

		public ADDomainMode? DomainMode
		{
			get
			{
				object mode = base.GetValue("DomainMode");
				if (mode == null) mode = (int)5;
				return new ADDomainMode?((ADDomainMode)((int)mode));
			}
			set
			{
				throw new NotSupportedException(StringResources.UseSetADDomainMode);
			}
		}

		public SecurityIdentifier DomainSID
		{
			get
			{
				return (SecurityIdentifier)base.GetValue("DomainSID");
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public string ForeignSecurityPrincipalsContainer
		{
			get
			{
				return base.GetValue("ForeignSecurityPrincipalsContainer") as string;
			}
		}

		public string Forest
		{
			get
			{
				return base.GetValue("Forest") as string;
			}
		}

		public string InfrastructureMaster
		{
			get
			{
				return base.GetValue("InfrastructureMaster") as string;
			}
		}

		public TimeSpan? LastLogonReplicationInterval
		{
			get
			{
				return (TimeSpan?)base.GetValue("LastLogonReplicationInterval");
			}
			set
			{
				base.SetValue("LastLogonReplicationInterval", value);
			}
		}

		public ADPropertyValueCollection LinkedGroupPolicyObjects
		{
			get
			{
				return base["LinkedGroupPolicyObjects"];
			}
		}

		public string ManagedBy
		{
			get
			{
				return base.GetValue("ManagedBy") as string;
			}
			set
			{
				base.SetValue("ManagedBy", value);
			}
		}

		public string NetBIOSName
		{
			get
			{
				return base.GetValue("NetBIOSName") as string;
			}
		}

		public string ParentDomain
		{
			get
			{
				return base.GetValue("ParentDomain") as string;
			}
		}

		public string PDCEmulator
		{
			get
			{
				return base.GetValue("PDCEmulator") as string;
			}
		}

		public string RIDMaster
		{
			get
			{
				return base.GetValue("RIDMaster") as string;
			}
		}

		public string SystemsContainer
		{
			get
			{
				return base.GetValue("SystemsContainer") as string;
			}
		}

		public string UsersContainer
		{
			get
			{
				return base.GetValue("UsersContainer") as string;
			}
		}

		static ADDomain()
		{
			ADEntity.RegisterMappingTable(typeof(ADDomain), ADDomainFactory<ADDomain>.AttributeTable);
		}

		public ADDomain()
		{
		}

		public ADDomain(string identity) : base(identity)
		{
		}

		public ADDomain(Guid guid) : base(guid)
		{
		}

		public ADDomain(SecurityIdentifier sid)
		{
			base.Identity = sid;
		}

		public ADDomain(ADObject adobject)
		{
			if (adobject != null)
			{
				base.Identity = adobject;
				if (adobject.IsSearchResult)
				{
					base.SessionInfo = adobject.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("adobject");
			}
		}
	}
}