using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryDomain", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ActiveDirectoryDomain : ActiveDirectoryPartition
	{
		private string[] AllowedDNSSuffixesField;

		private string[] AppliedGroupPoliciesField;

		private string[] ChildDomainsField;

		private string ComputersContainerField;

		private string DomainControllersContainerField;

		private int DomainModeField;

		private byte[] DomainSIDField;

		private string ForeignSecurityPrincipalsContainerField;

		private string ForestField;

		private string InfrastructureMasterField;

		private TimeSpan? LastLogonReplicationIntervalField;

		private string ManagedByField;

		private string NetBIOSNameField;

		private string PDCEmulatorField;

		private string ParentDomainField;

		private string RIDMasterField;

		private string SystemsContainerField;

		private string UsersContainerField;

		[DataMember(IsRequired=true)]
		internal string[] AllowedDNSSuffixes
		{
			get
			{
				return this.AllowedDNSSuffixesField;
			}
			set
			{
				this.AllowedDNSSuffixesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] AppliedGroupPolicies
		{
			get
			{
				return this.AppliedGroupPoliciesField;
			}
			set
			{
				this.AppliedGroupPoliciesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] ChildDomains
		{
			get
			{
				return this.ChildDomainsField;
			}
			set
			{
				this.ChildDomainsField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ComputersContainer
		{
			get
			{
				return this.ComputersContainerField;
			}
			set
			{
				this.ComputersContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string DomainControllersContainer
		{
			get
			{
				return this.DomainControllersContainerField;
			}
			set
			{
				this.DomainControllersContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal int DomainMode
		{
			get
			{
				return this.DomainModeField;
			}
			set
			{
				this.DomainModeField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal byte[] DomainSID
		{
			get
			{
				return this.DomainSIDField;
			}
			set
			{
				this.DomainSIDField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ForeignSecurityPrincipalsContainer
		{
			get
			{
				return this.ForeignSecurityPrincipalsContainerField;
			}
			set
			{
				this.ForeignSecurityPrincipalsContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string Forest
		{
			get
			{
				return this.ForestField;
			}
			set
			{
				this.ForestField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string InfrastructureMaster
		{
			get
			{
				return this.InfrastructureMasterField;
			}
			set
			{
				this.InfrastructureMasterField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal TimeSpan? LastLogonReplicationInterval
		{
			get
			{
				return this.LastLogonReplicationIntervalField;
			}
			set
			{
				this.LastLogonReplicationIntervalField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ManagedBy
		{
			get
			{
				return this.ManagedByField;
			}
			set
			{
				this.ManagedByField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string NetBIOSName
		{
			get
			{
				return this.NetBIOSNameField;
			}
			set
			{
				this.NetBIOSNameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ParentDomain
		{
			get
			{
				return this.ParentDomainField;
			}
			set
			{
				this.ParentDomainField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string PDCEmulator
		{
			get
			{
				return this.PDCEmulatorField;
			}
			set
			{
				this.PDCEmulatorField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string RIDMaster
		{
			get
			{
				return this.RIDMasterField;
			}
			set
			{
				this.RIDMasterField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string SystemsContainer
		{
			get
			{
				return this.SystemsContainerField;
			}
			set
			{
				this.SystemsContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string UsersContainer
		{
			get
			{
				return this.UsersContainerField;
			}
			set
			{
				this.UsersContainerField = value;
			}
		}

		public ActiveDirectoryDomain()
		{
		}
	}
}