using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADReplicationAttributeMetadataParameterSet : ADParameterSet
	{
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

		[Parameter]
		[TransformFilter]
		[ValidateNotNullOrEmpty]
		public string Filter
		{
			get
			{
				return base["Filter"] as string;
			}
			set
			{
				base["Filter"] = value;
			}
		}

		[Parameter]
		public SwitchParameter IncludeDeletedObjects
		{
			get
			{
				return base.GetSwitchParameter("IncludeDeletedObjects");
			}
			set
			{
				base["IncludeDeletedObjects"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
		public ADObject Object
		{
			get
			{
				return base["Object"] as ADObject;
			}
			set
			{
				base["Object"] = value;
			}
		}

		[Alias(new string[] { "Property", "Attribute", "Attributes" })]
		[Parameter(Position=2)]
		[ValidateNotNullOrEmpty]
		public string[] Properties
		{
			get
			{
				return base["Properties"] as string[];
			}
			set
			{
				base["Properties"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=1)]
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

		[Parameter]
		public SwitchParameter ShowAllLinkedValues
		{
			get
			{
				return base.GetSwitchParameter("ShowAllLinkedValues");
			}
			set
			{
				base["ShowAllLinkedValues"] = value;
			}
		}

		public GetADReplicationAttributeMetadataParameterSet()
		{
		}
	}
}