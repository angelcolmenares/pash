using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADReplicationSiteParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? AutomaticInterSiteTopologyGenerationEnabled
		{
			get
			{
				return (bool?)(base["AutomaticInterSiteTopologyGenerationEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AutomaticInterSiteTopologyGenerationEnabled");
					return;
				}
				else
				{
					base["AutomaticInterSiteTopologyGenerationEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? AutomaticTopologyGenerationEnabled
		{
			get
			{
				return (bool?)(base["AutomaticTopologyGenerationEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AutomaticTopologyGenerationEnabled");
					return;
				}
				else
				{
					base["AutomaticTopologyGenerationEnabled"] = value;
					return;
				}
			}
		}

		[Credential]
		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public string Description
		{
			get
			{
				return base["Description"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Description");
					return;
				}
				else
				{
					base["Description"] = value;
					return;
				}
			}
		}

		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public ADDirectoryServer InterSiteTopologyGenerator
		{
			get
			{
				return base["InterSiteTopologyGenerator"] as ADDirectoryServer;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("InterSiteTopologyGenerator");
					return;
				}
				else
				{
					base["InterSiteTopologyGenerator"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public ADPrincipal ManagedBy
		{
			get
			{
				return base["ManagedBy"] as ADPrincipal;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ManagedBy");
					return;
				}
				else
				{
					base["ManagedBy"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get
			{
				return base["Name"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Name");
					return;
				}
				else
				{
					base["Name"] = value;
					return;
				}
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable OtherAttributes
		{
			get
			{
				return base["OtherAttributes"] as Hashtable;
			}
			set
			{
				base["OtherAttributes"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? ProtectedFromAccidentalDeletion
		{
			get
			{
				return (bool?)(base["ProtectedFromAccidentalDeletion"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ProtectedFromAccidentalDeletion");
					return;
				}
				else
				{
					base["ProtectedFromAccidentalDeletion"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? RedundantServerTopologyEnabled
		{
			get
			{
				return (bool?)(base["RedundantServerTopologyEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("RedundantServerTopologyEnabled");
					return;
				}
				else
				{
					base["RedundantServerTopologyEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ActiveDirectorySchedule ReplicationSchedule
		{
			get
			{
				return base["ReplicationSchedule"] as ActiveDirectorySchedule;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ReplicationSchedule");
					return;
				}
				else
				{
					base["ReplicationSchedule"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? ScheduleHashingEnabled
		{
			get
			{
				return (bool?)(base["ScheduleHashingEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ScheduleHashingEnabled");
					return;
				}
				else
				{
					base["ScheduleHashingEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? TopologyCleanupEnabled
		{
			get
			{
				return (bool?)(base["TopologyCleanupEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("TopologyCleanupEnabled");
					return;
				}
				else
				{
					base["TopologyCleanupEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? TopologyDetectStaleEnabled
		{
			get
			{
				return (bool?)(base["TopologyDetectStaleEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("TopologyDetectStaleEnabled");
					return;
				}
				else
				{
					base["TopologyDetectStaleEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? TopologyMinimumHopsEnabled
		{
			get
			{
				return (bool?)(base["TopologyMinimumHopsEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("TopologyMinimumHopsEnabled");
					return;
				}
				else
				{
					base["TopologyMinimumHopsEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? UniversalGroupCachingEnabled
		{
			get
			{
				return (bool?)(base["UniversalGroupCachingEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("UniversalGroupCachingEnabled");
					return;
				}
				else
				{
					base["UniversalGroupCachingEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public ADReplicationSite UniversalGroupCachingRefreshSite
		{
			get
			{
				return base["UniversalGroupCachingRefreshSite"] as ADReplicationSite;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("UniversalGroupCachingRefreshSite");
					return;
				}
				else
				{
					base["UniversalGroupCachingRefreshSite"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? WindowsServer2000BridgeheadSelectionMethodEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2000BridgeheadSelectionMethodEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("WindowsServer2000BridgeheadSelectionMethodEnabled");
					return;
				}
				else
				{
					base["WindowsServer2000BridgeheadSelectionMethodEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? WindowsServer2000KCCISTGSelectionBehaviorEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2000KCCISTGSelectionBehaviorEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("WindowsServer2000KCCISTGSelectionBehaviorEnabled");
					return;
				}
				else
				{
					base["WindowsServer2000KCCISTGSelectionBehaviorEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCBehaviorEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCBehaviorEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("WindowsServer2003KCCBehaviorEnabled");
					return;
				}
				else
				{
					base["WindowsServer2003KCCBehaviorEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCIgnoreScheduleEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCIgnoreScheduleEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("WindowsServer2003KCCIgnoreScheduleEnabled");
					return;
				}
				else
				{
					base["WindowsServer2003KCCIgnoreScheduleEnabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public bool? WindowsServer2003KCCSiteLinkBridgingEnabled
		{
			get
			{
				return (bool?)(base["WindowsServer2003KCCSiteLinkBridgingEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("WindowsServer2003KCCSiteLinkBridgingEnabled");
					return;
				}
				else
				{
					base["WindowsServer2003KCCSiteLinkBridgingEnabled"] = value;
					return;
				}
			}
		}

		public NewADReplicationSiteParameterSet()
		{
		}
	}
}