using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADReplicationSubnetParameterSet : ADParameterSet
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
		public ADReplicationSubnet Instance
		{
			get
			{
				return base["Instance"] as ADReplicationSubnet;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
		public string Location
		{
			get
			{
				return base["Location"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Location");
					return;
				}
				else
				{
					base["Location"] = value;
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
		[ValidateNotNull]
		public ADReplicationSite Site
		{
			get
			{
				return base["Site"] as ADReplicationSite;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Site");
					return;
				}
				else
				{
					base["Site"] = value;
					return;
				}
			}
		}

		public NewADReplicationSubnetParameterSet()
		{
		}
	}
}