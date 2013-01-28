using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADReplicationSiteLinkParameterSet : ADParameterSet
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
		public int? Cost
		{
			get
			{
				return (int?)(base["Cost"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Cost");
					return;
				}
				else
				{
					base["Cost"] = value;
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public ADInterSiteTransportProtocolType? InterSiteTransportProtocol
		{
			get
			{
				return (ADInterSiteTransportProtocolType?)(base["InterSiteTransportProtocol"] as ADInterSiteTransportProtocolType?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("InterSiteTransportProtocol");
					return;
				}
				else
				{
					base["InterSiteTransportProtocol"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
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
		public int? ReplicationFrequencyInMinutes
		{
			get
			{
				return (int?)(base["ReplicationFrequencyInMinutes"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ReplicationFrequencyInMinutes");
					return;
				}
				else
				{
					base["ReplicationFrequencyInMinutes"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
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

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		[ValidateNotNullOrEmpty]
		public ADReplicationSite[] SitesIncluded
		{
			get
			{
				return base["SitesIncluded"] as ADReplicationSite[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SitesIncluded");
					return;
				}
				else
				{
					base["SitesIncluded"] = value;
					return;
				}
			}
		}

		public NewADReplicationSiteLinkParameterSet()
		{
		}
	}
}