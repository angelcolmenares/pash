using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADTrustParameterSet : ADParameterSet
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

		[Parameter(Mandatory=true, ParameterSetName="Filter")]
		[TransformFilter]
		[ValidateNotNullOrEmpty]
		public string Filter
		{
			get
			{
				return base["Filter"] as string;
			}
			set
			{
				base["Filter"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
		public ADTrust Identity
		{
			get
			{
				return base["Identity"] as ADTrust;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="InputObject")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public object InputObject
		{
			get
			{
				return base["InputObject"];
			}
			set
			{
				base["InputObject"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="LdapFilter")]
		[ValidateNotNullOrEmpty]
		public string LDAPFilter
		{
			get
			{
				return base["LDAPFilter"] as string;
			}
			set
			{
				base["LDAPFilter"] = value;
			}
		}

		[Alias(new string[] { "Property" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string[] Properties
		{
			get
			{
				return base["Properties"] as string[];
			}
			set
			{
				base["Properties"] = value;
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

		public GetADTrustParameterSet()
		{
		}
	}
}