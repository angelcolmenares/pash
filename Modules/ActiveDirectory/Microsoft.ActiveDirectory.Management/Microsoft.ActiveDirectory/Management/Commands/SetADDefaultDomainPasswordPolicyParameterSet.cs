using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADDefaultDomainPasswordPolicyParameterSet : ADParameterSet
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

		[Parameter]
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
		[ValidateNotNull]
		public ADDefaultDomainPasswordPolicy Identity
		{
			get
			{
				return base["Identity"] as ADDefaultDomainPasswordPolicy;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		[Parameter]
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

		public SetADDefaultDomainPasswordPolicyParameterSet()
		{
		}
	}
}