using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADReplicationConnectionParameterSet : ADParameterSet
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

		[Parameter]
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ADReplicationConnection Identity
		{
			get
			{
				return base["Identity"] as ADReplicationConnection;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADReplicationConnection Instance
		{
			get
			{
				return base["Instance"] as ADReplicationConnection;
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
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADDirectoryServer ReplicateFromDirectoryServer
		{
			get
			{
				return base["ReplicateFromDirectoryServer"] as ADDirectoryServer;
			}
			set
			{
				base["ReplicateFromDirectoryServer"] = value;
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

		public SetADReplicationConnectionParameterSet()
		{
		}
	}
}