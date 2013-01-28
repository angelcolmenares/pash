using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADClaimTransformPolicyParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="DenyAllExcept")]
		[Parameter(ParameterSetName="AllowAllExcept")]
		[Parameter(ParameterSetName="AllowAll")]
		[Parameter(ParameterSetName="DenyAll")]
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

		[Parameter(Mandatory=true, ParameterSetName="AllowAll")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "true" })]
		public SwitchParameter AllowAll
		{
			get
			{
				return base.GetSwitchParameter("AllowAll");
			}
			set
			{
				base["AllowAll"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="AllowAllExcept")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADClaimType[] AllowAllExcept
		{
			get
			{
				return base["AllowAllExcept"] as ADClaimType[];
			}
			set
			{
				base["AllowAllExcept"] = value;
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
		[Parameter(ParameterSetName="AllowAll")]
		[Parameter(ParameterSetName="AllowAllExcept")]
		[Parameter(ParameterSetName="DenyAll")]
		[Parameter(ParameterSetName="DenyAllExcept")]
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

		[Parameter(Mandatory=true, ParameterSetName="DenyAll")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "true" })]
		public SwitchParameter DenyAll
		{
			get
			{
				return base.GetSwitchParameter("DenyAll");
			}
			set
			{
				base["DenyAll"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="DenyAllExcept")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADClaimType[] DenyAllExcept
		{
			get
			{
				return base["DenyAllExcept"] as ADClaimType[];
			}
			set
			{
				base["DenyAllExcept"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="AllowAll")]
		[Parameter(ParameterSetName="AllowAllExcept")]
		[Parameter(ParameterSetName="DenyAll")]
		[Parameter(ParameterSetName="DenyAllExcept")]
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
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="AllowAllExcept")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="DenyAll")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="DenyAllExcept")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="AllowAll")]
		[ValidateNotNull]
		public ADClaimTransformPolicy Identity
		{
			get
			{
				return base["Identity"] as ADClaimTransformPolicy;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADClaimTransformPolicy Instance
		{
			get
			{
				return base["Instance"] as ADClaimTransformPolicy;
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
		[Parameter(ParameterSetName="DenyAll")]
		[Parameter(ParameterSetName="AllowAllExcept")]
		[Parameter(ParameterSetName="DenyAllExcept")]
		[Parameter(ParameterSetName="AllowAll")]
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

		[Parameter(ParameterSetName="AllowAllExcept")]
		[Parameter(ParameterSetName="AllowAll")]
		[Parameter(ParameterSetName="DenyAll")]
		[Parameter(ParameterSetName="DenyAllExcept")]
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
		[Parameter(ParameterSetName="DenyAllExcept")]
		[Parameter(ParameterSetName="DenyAll")]
		[Parameter(ParameterSetName="AllowAll")]
		[Parameter(ParameterSetName="AllowAllExcept")]
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
		public string Rule
		{
			get
			{
				return base["Rule"] as string;
			}
			set
			{
				base["Rule"] = value;
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

		public SetADClaimTransformPolicyParameterSet()
		{
		}
	}
}