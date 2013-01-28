using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADReplicationSiteLinkParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Add
		{
			get
			{
				return base["Add"] as Hashtable;
			}
			set
			{
				base["Add"] = value;
			}
		}

		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return (ADAuthType)base["AuthType"];
			}
			set
			{
				base["AuthType"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] Clear
		{
			get
			{
				return base["Clear"] as string[];
			}
			set
			{
				base["Clear"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public int? Cost
		{
			get
			{
				return (int?)(base["Cost"] as int?);
			}
			set
			{
				base["Cost"] = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return base["Credential"] as PSCredential;
			}
			set
			{
				base["Credential"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Description
		{
			get
			{
				return base["Description"] as string;
			}
			set
			{
				base["Description"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
		public ADReplicationSiteLink Identity
		{
			get
			{
				return base["Identity"] as ADReplicationSiteLink;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADReplicationSiteLink Instance
		{
			get
			{
				return base["Instance"] as ADReplicationSiteLink;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter PassThru
		{
			get
			{
				return base.GetSwitchParameter("PassThru");
			}
			set
			{
				base["PassThru"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Remove
		{
			get
			{
				return base["Remove"] as Hashtable;
			}
			set
			{
				base["Remove"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Replace
		{
			get
			{
				return base["Replace"] as Hashtable;
			}
			set
			{
				base["Replace"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public int ReplicationFrequencyInMinutes
		{
			get
			{
				return (int)base["ReplicationFrequencyInMinutes"];
			}
			set
			{
				base["ReplicationFrequencyInMinutes"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ActiveDirectorySchedule ReplicationSchedule
		{
			get
			{
				return base["ReplicationSchedule"] as ActiveDirectorySchedule;
			}
			set
			{
				base["ReplicationSchedule"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return base["Server"] as string;
			}
			set
			{
				base["Server"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable SitesIncluded
		{
			get
			{
				return base["SitesIncluded"] as Hashtable;
			}
			set
			{
				base["SitesIncluded"] = new ADMultivalueHashtableParameter<string>(value);
			}
		}

		public SetADReplicationSiteLinkParameterSet()
		{
		}
	}
}