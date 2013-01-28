using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADCentralAccessRuleParameterSet : ADParameterSet
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string CurrentAcl
		{
			get
			{
				return base["CurrentAcl"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("CurrentAcl");
					return;
				}
				else
				{
					base["CurrentAcl"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		[ValidateNotNull]
		public ADCentralAccessRule Instance
		{
			get
			{
				return base["Instance"] as ADCentralAccessRule;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string ProposedAcl
		{
			get
			{
				return base["ProposedAcl"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ProposedAcl");
					return;
				}
				else
				{
					base["ProposedAcl"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string ResourceCondition
		{
			get
			{
				return base["ResourceCondition"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ResourceCondition");
					return;
				}
				else
				{
					base["ResourceCondition"] = value;
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

		public NewADCentralAccessRuleParameterSet()
		{
		}
	}
}