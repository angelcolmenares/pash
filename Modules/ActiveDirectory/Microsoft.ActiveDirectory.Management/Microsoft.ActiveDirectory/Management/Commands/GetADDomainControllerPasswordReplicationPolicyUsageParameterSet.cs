using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADDomainControllerPasswordReplicationPolicyUsageParameterSet : ADParameterSet
	{
		[Parameter(Mandatory=true, ParameterSetName="AuthenticatedAccounts")]
		[ValidateNotNull]
		public SwitchParameter AuthenticatedAccounts
		{
			get
			{
				return base.GetSwitchParameter("AuthenticatedAccounts");
			}
			set
			{
				base["AuthenticatedAccounts"] = value;
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="ADDCPRPUIdentityHM")]
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

		[Parameter(ParameterSetName="RevealedAccounts")]
		[ValidateNotNull]
		public SwitchParameter RevealedAccounts
		{
			get
			{
				return base.GetSwitchParameter("RevealedAccounts");
			}
			set
			{
				base["RevealedAccounts"] = value;
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

		public GetADDomainControllerPasswordReplicationPolicyUsageParameterSet()
		{
		}
	}
}