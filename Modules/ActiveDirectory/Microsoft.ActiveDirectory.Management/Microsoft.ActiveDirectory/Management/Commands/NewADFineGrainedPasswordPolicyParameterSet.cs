using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADFineGrainedPasswordPolicyParameterSet : ADParameterSet
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? ComplexityEnabled
		{
			get
			{
				return (bool?)(base["ComplexityEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ComplexityEnabled");
					return;
				}
				else
				{
					base["ComplexityEnabled"] = value;
					return;
				}
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string DisplayName
		{
			get
			{
				return base["DisplayName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("DisplayName");
					return;
				}
				else
				{
					base["DisplayName"] = value;
					return;
				}
			}
		}

		[Parameter]
		[ValidateNotNull]
		public ADFineGrainedPasswordPolicy Instance
		{
			get
			{
				return base["Instance"] as ADFineGrainedPasswordPolicy;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public TimeSpan? LockoutDuration
		{
			get
			{
				return (TimeSpan?)(base["LockoutDuration"] as TimeSpan?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("LockoutDuration");
					return;
				}
				else
				{
					base["LockoutDuration"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public TimeSpan? LockoutObservationWindow
		{
			get
			{
				return (TimeSpan?)(base["LockoutObservationWindow"] as TimeSpan?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("LockoutObservationWindow");
					return;
				}
				else
				{
					base["LockoutObservationWindow"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public int? LockoutThreshold
		{
			get
			{
				return (int?)(base["LockoutThreshold"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("LockoutThreshold");
					return;
				}
				else
				{
					base["LockoutThreshold"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public TimeSpan? MaxPasswordAge
		{
			get
			{
				return (TimeSpan?)(base["MaxPasswordAge"] as TimeSpan?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("MaxPasswordAge");
					return;
				}
				else
				{
					base["MaxPasswordAge"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public TimeSpan? MinPasswordAge
		{
			get
			{
				return (TimeSpan?)(base["MinPasswordAge"] as TimeSpan?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("MinPasswordAge");
					return;
				}
				else
				{
					base["MinPasswordAge"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public int? MinPasswordLength
		{
			get
			{
				return (int?)(base["MinPasswordLength"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("MinPasswordLength");
					return;
				}
				else
				{
					base["MinPasswordLength"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true)]
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
		public int? PasswordHistoryCount
		{
			get
			{
				return (int?)(base["PasswordHistoryCount"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PasswordHistoryCount");
					return;
				}
				else
				{
					base["PasswordHistoryCount"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
		public int? Precedence
		{
			get
			{
				return (int?)(base["Precedence"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Precedence");
					return;
				}
				else
				{
					base["Precedence"] = value;
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
		public bool? ReversibleEncryptionEnabled
		{
			get
			{
				return (bool?)(base["ReversibleEncryptionEnabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ReversibleEncryptionEnabled");
					return;
				}
				else
				{
					base["ReversibleEncryptionEnabled"] = value;
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

		public NewADFineGrainedPasswordPolicyParameterSet()
		{
		}
	}
}