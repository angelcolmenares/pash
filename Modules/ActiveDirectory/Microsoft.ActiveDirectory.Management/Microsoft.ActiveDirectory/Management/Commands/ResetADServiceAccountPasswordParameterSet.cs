using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ResetADServiceAccountPasswordParameterSet : ADParameterSet
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

		[Parameter]
		[ValidateNotNull]
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

		public ResetADServiceAccountPasswordParameterSet()
		{
		}
	}
}