using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADAccountControlParameterSet : ADParameterSet
	{
		[Parameter]
		public bool AccountNotDelegated
		{
			get
			{
				return (bool)base["AccountNotDelegated"];
			}
			set
			{
				base["AccountNotDelegated"] = value;
			}
		}

		[Parameter]
		public bool AllowReversiblePasswordEncryption
		{
			get
			{
				return (bool)base["AllowReversiblePasswordEncryption"];
			}
			set
			{
				base["AllowReversiblePasswordEncryption"] = value;
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

		[Parameter]
		public bool CannotChangePassword
		{
			get
			{
				return (bool)base["CannotChangePassword"];
			}
			set
			{
				base["CannotChangePassword"] = value;
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
		public bool DoesNotRequirePreAuth
		{
			get
			{
				return (bool)base["DoesNotRequirePreAuth"];
			}
			set
			{
				base["DoesNotRequirePreAuth"] = value;
			}
		}

		[Parameter]
		public bool Enabled
		{
			get
			{
				return (bool)base["Enabled"];
			}
			set
			{
				base["Enabled"] = value;
			}
		}

		[Parameter]
		public bool HomedirRequired
		{
			get
			{
				return (bool)base["HomedirRequired"];
			}
			set
			{
				base["HomedirRequired"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
		[ValidateNotNull]
		public ADAccount Identity
		{
			get
			{
				return base["Identity"] as ADAccount;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter]
		public bool MNSLogonAccount
		{
			get
			{
				return (bool)base["MNSLogonAccount"];
			}
			set
			{
				base["MNSLogonAccount"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Partition
		{
			get
			{
				return base["Partition"] as string;
			}
			set
			{
				base["Partition"] = value;
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
		public bool PasswordNeverExpires
		{
			get
			{
				return (bool)base["PasswordNeverExpires"];
			}
			set
			{
				base["PasswordNeverExpires"] = value;
			}
		}

		[Parameter]
		public bool PasswordNotRequired
		{
			get
			{
				return (bool)base["PasswordNotRequired"];
			}
			set
			{
				base["PasswordNotRequired"] = value;
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

		[Parameter]
		public bool TrustedForDelegation
		{
			get
			{
				return (bool)base["TrustedForDelegation"];
			}
			set
			{
				base["TrustedForDelegation"] = value;
			}
		}

		[Parameter]
		public bool TrustedToAuthForDelegation
		{
			get
			{
				return (bool)base["TrustedToAuthForDelegation"];
			}
			set
			{
				base["TrustedToAuthForDelegation"] = value;
			}
		}

		[Parameter]
		public bool UseDESKeyOnly
		{
			get
			{
				return (bool)base["UseDESKeyOnly"];
			}
			set
			{
				base["UseDESKeyOnly"] = value;
			}
		}

		public SetADAccountControlParameterSet()
		{
		}
	}
}