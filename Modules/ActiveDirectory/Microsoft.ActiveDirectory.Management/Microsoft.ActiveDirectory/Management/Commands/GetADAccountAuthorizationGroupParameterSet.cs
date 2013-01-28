using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADAccountAuthorizationGroupParameterSet : ADParameterSet
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="ADAccountAuthGroupIdentityHM")]
		[ValidateNotNullOrEmptyADEntity]
		[ValidateNotNullOrEmpty]
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
		[ValidateNotNull]
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

		public GetADAccountAuthorizationGroupParameterSet()
		{
		}
	}
}