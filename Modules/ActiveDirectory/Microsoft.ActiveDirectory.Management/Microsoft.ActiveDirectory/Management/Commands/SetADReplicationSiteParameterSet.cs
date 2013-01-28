using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADReplicationSiteParameterSet : ADParameterSet
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
		public bool? AutomaticInterSiteTopologyGenerationEnabled
		{
			get
			{
				return (bool?)(base["AutomaticInterSiteTopologyGenerationEnabled"] as bool?);
			}
			set
			{
				base["AutomaticInterSiteTopologyGenerationEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? AutomaticTopologyGenerationEnabled
		{
			get
			{
				return (bool?)(base["AutomaticTopologyGenerationEnabled"] as bool?);
			}
			set
			{
				base["AutomaticTopologyGenerationEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
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
		public ADReplicationSite Identity
		{
			get
			{
				return base["Identity"] as ADReplicationSite;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADReplicationSite Instance
		{
			get
			{
				return base["Instance"] as ADReplicationSite;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADDirectoryServer InterSiteTopologyGenerator
		{
			get
			{
				return base["InterSiteTopologyGenerator"] as ADDirectoryServer;
			}
			set
			{
				base["InterSiteTopologyGenerator"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADPrincipal ManagedBy
		{
			get
			{
				return base["ManagedBy"] as ADPrincipal;
			}
			set
			{
				base["ManagedBy"] = value;
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
		public bool? ProtectedFromAccidentalDeletion
		{
			get
			{
				return (bool?)(base["ProtectedFromAccidentalDeletion"] as bool?);
			}
			set
			{
				base["ProtectedFromAccidentalDeletion"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? RedundantServerTopologyEnabled
		{
			get
			{
				return (bool?)(base["RedundantServerTopologyEnabled"] as bool?);
			}
			set
			{
				base["RedundantServerTopologyEnabled"] = value;
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

		[Parameter(ParameterSetName="Identity")]
		public bool? ScheduleHashingEnabled
		{
			get
			{
				return (bool?)(base["ScheduleHashingEnabled"] as bool?);
			}
			set
			{
				base["ScheduleHashingEnabled"] = value;
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
		public bool? TopologyCleanupEnabled
		{
			get
			{
				return (bool?)(base["TopologyCleanupEnabled"] as bool?);
			}
			set
			{
				base["TopologyCleanupEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? TopologyDetectStaleEnabled
		{
			get
			{
				return (bool?)(base["TopologyDetectStaleEnabled"] as bool?);
			}
			set
			{
				base["TopologyDetectStaleEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? TopologyMinimumHopsEnabled
		{
			get
			{
				return (bool?)(base["TopologyMinimumHopsEnabled"] as bool?);
			}
			set
			{
				base["TopologyMinimumHopsEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? UniversalGroupCachingEnabled
		{
			get
			{
				return (bool?)(base["UniversalGroupCachingEnabled"] as bool?);
			}
			set
			{
				base["UniversalGroupCachingEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADReplicationSite UniversalGroupCachingRefreshSite
		{
			get
			{
				return base["UniversalGroupCachingRefreshSite"] as ADReplicationSite;
			}
			set
			{
				base["UniversalGroupCachingRefreshSite"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? WindowsServer2000BridgeheadSelectionMethodEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2000BridgeheadSelectionMethodEnabled"] as bool?);
			}
			set
			{
				base["WindowsServer2000BridgeheadSelectionMethodEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? WindowsServer2000KCCISTGSelectionBehaviorEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2000KCCISTGSelectionBehaviorEnabled"] as bool?);
			}
			set
			{
				base["WindowsServer2000KCCISTGSelectionBehaviorEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCBehaviorEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCBehaviorEnabled"] as bool?);
			}
			set
			{
				base["WindowsServer2003KCCBehaviorEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCIgnoreScheduleEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCIgnoreScheduleEnabled"] as bool?);
			}
			set
			{
				base["WindowsServer2003KCCIgnoreScheduleEnabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCSiteLinkBridgingEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCSiteLinkBridgingEnabled"] as bool?);
			}
			set
			{
				base["WindowsServer2003KCCSiteLinkBridgingEnabled"] = value;
			}
		}

		public SetADReplicationSiteParameterSet()
		{
		}
	}
}