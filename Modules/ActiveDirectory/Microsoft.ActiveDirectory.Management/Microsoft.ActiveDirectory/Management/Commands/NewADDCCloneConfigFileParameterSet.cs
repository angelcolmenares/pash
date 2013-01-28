using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADDCCloneConfigFileParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="IPv4StaticSettings")]
		[Parameter(ParameterSetName="OfflineExecution")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string AlternateWINSServer
		{
			get
			{
				return base["AlternateWINSServer"] as string;
			}
			set
			{
				base["AlternateWINSServer"] = value;
			}
		}

		[Alias(new string[] { "cn" })]
		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string CloneComputerName
		{
			get
			{
				return base["CloneComputerName"] as string;
			}
			set
			{
				base["CloneComputerName"] = value;
			}
		}

		[Parameter(ParameterSetName="OfflineExecution")]
		[Parameter(Mandatory=true, ParameterSetName="IPv4StaticSettings")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string IPv4Address
		{
			get
			{
				return base["IPv4Address"] as string;
			}
			set
			{
				base["IPv4Address"] = value;
			}
		}

		[Parameter(ParameterSetName="OfflineExecution")]
		[Parameter(ParameterSetName="IPv4StaticSettings")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string IPv4DefaultGateway
		{
			get
			{
				return base["IPv4DefaultGateway"] as string;
			}
			set
			{
				base["IPv4DefaultGateway"] = value;
			}
		}

		[Parameter(ParameterSetName="OfflineExecution")]
		[Parameter(ParameterSetName="IPv4DynamicSettings")]
		[Parameter(Mandatory=true, ParameterSetName="IPv4StaticSettings")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] IPv4DNSResolver
		{
			get
			{
				return base["IPv4DNSResolver"] as string[];
			}
			set
			{
				base["IPv4DNSResolver"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="IPv4StaticSettings")]
		[Parameter(ParameterSetName="OfflineExecution")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string IPv4SubnetMask
		{
			get
			{
				return base["IPv4SubnetMask"] as string;
			}
			set
			{
				base["IPv4SubnetMask"] = value;
			}
		}

		[Parameter(ParameterSetName="IPv6DynamicSettings")]
		[Parameter(Mandatory=true, ParameterSetName="IPv6StaticSettings")]
		[Parameter(ParameterSetName="OfflineExecution")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] IPv6DNSResolver
		{
			get
			{
				return base["IPv6DNSResolver"] as string[];
			}
			set
			{
				base["IPv6DNSResolver"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="OfflineExecution")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Offline
		{
			get
			{
				return base.GetSwitchParameter("Offline");
			}
			set
			{
				base["Offline"] = value;
			}
		}

		[Parameter(ParameterSetName="IPv4StaticSettings")]
		[Parameter(ParameterSetName="IPv6StaticSettings")]
		[Parameter(Mandatory=true, ParameterSetName="OfflineExecution")]
		[Parameter(ParameterSetName="IPv6DynamicSettings")]
		[Parameter(ParameterSetName="IPv4DynamicSettings")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get
			{
				return base["Path"] as string;
			}
			set
			{
				base["Path"] = value;
			}
		}

		[Parameter(ParameterSetName="IPv4StaticSettings")]
		[Parameter(ParameterSetName="OfflineExecution")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string PreferredWINSServer
		{
			get
			{
				return base["PreferredWINSServer"] as string;
			}
			set
			{
				base["PreferredWINSServer"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string SiteName
		{
			get
			{
				return base["SiteName"] as string;
			}
			set
			{
				base["SiteName"] = value;
			}
		}

		[Parameter(ParameterSetName="OfflineExecution")]
		[Parameter(Mandatory=true, ParameterSetName="IPv6StaticSettings")]
		[Parameter(Mandatory=true, ParameterSetName="IPv4StaticSettings")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Static
		{
			get
			{
				return base.GetSwitchParameter("Static");
			}
			set
			{
				base["Static"] = value;
			}
		}

		public NewADDCCloneConfigFileParameterSet()
		{
		}
	}
}