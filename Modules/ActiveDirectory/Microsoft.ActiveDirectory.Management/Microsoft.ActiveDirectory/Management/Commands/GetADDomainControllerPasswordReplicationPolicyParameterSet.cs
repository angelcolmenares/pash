using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADDomainControllerPasswordReplicationPolicyParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="AllowedPRP")]
		[ValidateNotNull]
		public SwitchParameter Allowed
		{
			get
			{
				return base.GetSwitchParameter("Allowed");
			}
			set
			{
				base["Allowed"] = value;
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

		[Parameter(Mandatory=true, ParameterSetName="DeniedPRP")]
		[ValidateNotNull]
		public SwitchParameter Denied
		{
			get
			{
				return base.GetSwitchParameter("Denied");
			}
			set
			{
				base["Denied"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="ADDCPRPIdentityHM")]
		[ValidateNotNullOrEmpty]
		public ADDomainController Identity
		{
			get
			{
				return base["Identity"] as ADDomainController;
			}
			set
			{
				base["Identity"] = value;
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

		public GetADDomainControllerPasswordReplicationPolicyParameterSet()
		{
		}
	}
}