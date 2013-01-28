using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADReplicationSiteLinkBridgeParameterSet : ADParameterSet
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
		public ADReplicationSiteLinkBridge Instance
		{
			get
			{
				return base["Instance"] as ADReplicationSiteLinkBridge;
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
		public ADReplicationSiteLink[] SiteLinksIncluded
		{
			get
			{
				return base["SiteLinksIncluded"] as ADReplicationSiteLink[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SiteLinksIncluded");
					return;
				}
				else
				{
					base["SiteLinksIncluded"] = value;
					return;
				}
			}
		}

		public NewADReplicationSiteLinkBridgeParameterSet()
		{
		}
	}
}