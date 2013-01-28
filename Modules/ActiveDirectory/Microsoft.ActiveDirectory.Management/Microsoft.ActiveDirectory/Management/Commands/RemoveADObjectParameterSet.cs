using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class RemoveADObjectParameterSet : ADParameterSet
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
		[ValidateNotNullOrEmptyADEntity]
		public ADObject Identity
		{
			get
			{
				return base["Identity"] as ADObject;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter]
		public SwitchParameter IncludeDeletedObjects
		{
			get
			{
				return base.GetSwitchParameter("IncludeDeletedObjects");
			}
			set
			{
				base["IncludeDeletedObjects"] = value;
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
		public SwitchParameter Recursive
		{
			get
			{
				return base.GetSwitchParameter("Recursive");
			}
			set
			{
				base["Recursive"] = value;
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

		public RemoveADObjectParameterSet()
		{
		}
	}
}