using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class TestADServiceAccountParameterSet : ADParameterSet
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

		public TestADServiceAccountParameterSet()
		{
		}
	}
}