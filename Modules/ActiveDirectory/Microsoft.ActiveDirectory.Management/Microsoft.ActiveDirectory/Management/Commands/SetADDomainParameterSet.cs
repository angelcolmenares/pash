using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADDomainParameterSet : ADParameterSet
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
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable AllowedDNSSuffixes
		{
			get
			{
				return base["AllowedDNSSuffixes"] as Hashtable;
			}
			set
			{
				base["AllowedDNSSuffixes"] = new ADMultivalueHashtableParameter<string>(value);
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
		public ADDomain Identity
		{
			get
			{
				return base["Identity"] as ADDomain;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADDomain Instance
		{
			get
			{
				return base["Instance"] as ADDomain;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter]
		public TimeSpan? LastLogonReplicationInterval
		{
			get
			{
				return (TimeSpan?)(base["LastLogonReplicationInterval"] as TimeSpan?);
			}
			set
			{
				base["LastLogonReplicationInterval"] = value;
			}
		}

		[Parameter]
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

		public SetADDomainParameterSet()
		{
		}
	}
}