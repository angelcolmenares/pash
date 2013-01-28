using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADReplicationUpToDatenessVectorTableParameterSet : ADParameterSet
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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string EnumerationServer
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

		[Parameter]
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

		[Alias(new string[] { "NC", "NamingContext" })]
		[Parameter(Position=2)]
		[ValidateNotNullOrEmpty]
		public string[] Partition
		{
			get
			{
				return base["PartitionFilter"] as string[];
			}
			set
			{
				base["PartitionFilter"] = value;
			}
		}

		[Alias(new string[] { "ReplicationSite" })]
		[Parameter(Mandatory=true, Position=1, ParameterSetName="Scope")]
		[ValidateNotNull]
		public ADScopeType Scope
		{
			get
			{
				return (ADScopeType)base["Scope"];
			}
			set
			{
				base["Scope"] = value;
			}
		}

		[Alias(new string[] { "Name", "HostName", "Site", "Domain", "Forest" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Target", HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="TargetParameterHM")]
		[Parameter(Position=0, ValueFromPipeline=true, ParameterSetName="Scope", HelpMessageBaseName="Microsoft.ActiveDirectory.Management", HelpMessageResourceId="TargetParameterHM")]
		[ValidateNotNull]
		public object[] Target
		{
			get
			{
				return base["Target"] as object[];
			}
			set
			{
				base["Target"] = value;
			}
		}

		public GetADReplicationUpToDatenessVectorTableParameterSet()
		{
		}
	}
}