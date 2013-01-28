using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class RemoveADGroupMemberParameterSet : ADParameterSet
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
		public ADGroup Identity
		{
			get
			{
				return base["Identity"] as ADGroup;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=1)]
		[ValidateNotNullOrEmpty]
		public ADPrincipal[] Members
		{
			get
			{
				return base["Members"] as ADPrincipal[];
			}
			set
			{
				base["Members"] = value;
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

		public RemoveADGroupMemberParameterSet()
		{
		}
	}
}