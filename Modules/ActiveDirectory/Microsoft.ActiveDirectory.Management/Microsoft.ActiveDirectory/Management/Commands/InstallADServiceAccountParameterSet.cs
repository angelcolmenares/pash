using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class InstallADServiceAccountParameterSet : ADParameterSet
	{
		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SecureString AccountPassword
		{
			get
			{
				return base["AccountPassword"] as SecureString;
			}
			set
			{
				base["AccountPassword"] = value;
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
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Force
		{
			get
			{
				return base.GetSwitchParameter("Force");
			}
			set
			{
				base["Force"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
		public ADServiceAccount Identity
		{
			get
			{
				return base["Identity"] as ADServiceAccount;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter PromptForPassword
		{
			get
			{
				return base.GetSwitchParameter("PromptForPassword");
			}
			set
			{
				base["PromptForPassword"] = value;
			}
		}

		public InstallADServiceAccountParameterSet()
		{
		}
	}
}