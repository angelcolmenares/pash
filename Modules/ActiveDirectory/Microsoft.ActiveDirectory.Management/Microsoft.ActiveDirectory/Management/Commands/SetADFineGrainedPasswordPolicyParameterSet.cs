using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADFineGrainedPasswordPolicyParameterSet : ADParameterSet
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

		[Parameter(ParameterSetName="Identity")]
		public bool? ComplexityEnabled
		{
			get
			{
				return (bool?)(base["ComplexityEnabled"] as bool?);
			}
			set
			{
				base["ComplexityEnabled"] = value;
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

		[Parameter(ParameterSetName="Identity")]
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
		public string DisplayName
		{
			get
			{
				return base["DisplayName"] as string;
			}
			set
			{
				base["DisplayName"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ADFineGrainedPasswordPolicy Identity
		{
			get
			{
				return base["Identity"] as ADFineGrainedPasswordPolicy;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
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

		[Parameter(ParameterSetName="Identity")]
		public TimeSpan? LockoutDuration
		{
			get
			{
				return (TimeSpan?)(base["LockoutDuration"] as TimeSpan?);
			}
			set
			{
				base["LockoutDuration"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public TimeSpan? LockoutObservationWindow
		{
			get
			{
				return (TimeSpan?)(base["LockoutObservationWindow"] as TimeSpan?);
			}
			set
			{
				base["LockoutObservationWindow"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public int? LockoutThreshold
		{
			get
			{
				return (int?)(base["LockoutThreshold"] as int?);
			}
			set
			{
				base["LockoutThreshold"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public TimeSpan? MaxPasswordAge
		{
			get
			{
				return (TimeSpan?)(base["MaxPasswordAge"] as TimeSpan?);
			}
			set
			{
				base["MaxPasswordAge"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public TimeSpan? MinPasswordAge
		{
			get
			{
				return (TimeSpan?)(base["MinPasswordAge"] as TimeSpan?);
			}
			set
			{
				base["MinPasswordAge"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public int? MinPasswordLength
		{
			get
			{
				return (int?)(base["MinPasswordLength"] as int?);
			}
			set
			{
				base["MinPasswordLength"] = value;
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
		public int? PasswordHistoryCount
		{
			get
			{
				return (int?)(base["PasswordHistoryCount"] as int?);
			}
			set
			{
				base["PasswordHistoryCount"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public int? Precedence
		{
			get
			{
				return (int?)(base["Precedence"] as int?);
			}
			set
			{
				base["Precedence"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ParameterSetName="Identity")]
		public bool? ReversibleEncryptionEnabled
		{
			get
			{
				return (bool?)(base["ReversibleEncryptionEnabled"] as bool?);
			}
			set
			{
				base["ReversibleEncryptionEnabled"] = value;
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

		public SetADFineGrainedPasswordPolicyParameterSet()
		{
		}
	}
}