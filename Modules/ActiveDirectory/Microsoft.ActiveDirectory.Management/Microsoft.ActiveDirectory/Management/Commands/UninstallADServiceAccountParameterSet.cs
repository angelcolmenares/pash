using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class UninstallADServiceAccountParameterSet : ADParameterSet
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
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter ForceRemoveLocal
		{
			get
			{
				return base.GetSwitchParameter("ForceRemoveLocal");
			}
			set
			{
				base["ForceRemoveLocal"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
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

		public UninstallADServiceAccountParameterSet()
		{
		}
	}
}