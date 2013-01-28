using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADPrincipalGroupMembershipParameterSet : ADParameterSet
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="ADPrincipalGMIdentityHM")]
		[ValidateNotNullOrEmptyADEntity]
		[ValidateNotNullOrEmpty]
		public ADPrincipal Identity
		{
			get
			{
				return base["Identity"] as ADPrincipal;
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
		public string ResourceContextPartition
		{
			get
			{
				return base["ResourceContextPartition"] as string;
			}
			set
			{
				base["ResourceContextPartition"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string ResourceContextServer
		{
			get
			{
				return base["ResourceContextServer"] as string;
			}
			set
			{
				base["ResourceContextServer"] = value;
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

		public GetADPrincipalGroupMembershipParameterSet()
		{
		}
	}
}