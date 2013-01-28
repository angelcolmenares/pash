using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class AddADDomainControllerPasswordReplicationPolicyParameterSet : ADParameterSet
	{
		[Parameter(Mandatory=true, ParameterSetName="AllowedPRP")]
		[ValidateCount(1, 0x7fffffff)]
		[ValidateNotNull]
		public ADPrincipal[] AllowedList
		{
			get
			{
				return base["AllowedList"] as ADPrincipal[];
			}
			set
			{
				base["AllowedList"] = value;
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
		[ValidateCount(1, 0x7fffffff)]
		[ValidateNotNull]
		public ADPrincipal[] DeniedList
		{
			get
			{
				return base["DeniedList"] as ADPrincipal[];
			}
			set
			{
				base["DeniedList"] = value;
			}
		}

		[Parameter(Position=0)]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
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

		public AddADDomainControllerPasswordReplicationPolicyParameterSet()
		{
		}
	}
}