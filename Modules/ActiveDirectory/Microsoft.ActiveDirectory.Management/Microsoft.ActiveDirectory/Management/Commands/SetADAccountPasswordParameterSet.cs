using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADAccountPasswordParameterSet : ADParameterSet
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
		public SecureString NewPassword
		{
			get
			{
				return base["NewPassword"] as SecureString;
			}
			set
			{
				base["NewPassword"] = value;
			}
		}

		[Parameter]
		public SecureString OldPassword
		{
			get
			{
				return base["OldPassword"] as SecureString;
			}
			set
			{
				base["OldPassword"] = value;
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
		public SwitchParameter Reset
		{
			get
			{
				return base.GetSwitchParameter("Reset");
			}
			set
			{
				base["Reset"] = value;
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

		public SetADAccountPasswordParameterSet()
		{
		}
	}
}