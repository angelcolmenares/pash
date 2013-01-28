using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADClaimTransformPolicyParameterSet : ADParameterSet
	{
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="AllowAll")]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AllowAll");
					return;
				}
				else
				{
					base["AllowAll"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="AllowAllExcept")]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AllowAllExcept");
					return;
				}
				else
				{
					base["AllowAllExcept"] = value;
					return;
				}
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="DenyAll")]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("DenyAll");
					return;
				}
				else
				{
					base["DenyAll"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="DenyAllExcept")]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("DenyAllExcept");
					return;
				}
				else
				{
					base["DenyAllExcept"] = value;
					return;
				}
			}
		}

		[Parameter]
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

		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(Mandatory=true, Position=0)]
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
				base["Name"] = value;
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

		[Parameter]
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Identity")]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Rule");
					return;
				}
				else
				{
					base["Rule"] = value;
					return;
				}
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

		public NewADClaimTransformPolicyParameterSet()
		{
		}
	}
}