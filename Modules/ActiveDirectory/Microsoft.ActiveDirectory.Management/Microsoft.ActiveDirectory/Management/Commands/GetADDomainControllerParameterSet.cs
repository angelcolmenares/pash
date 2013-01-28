using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADDomainControllerParameterSet : ADParameterSet
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

		[Parameter(ParameterSetName="DiscoverByService")]
		public SwitchParameter AvoidSelf
		{
			get
			{
				return base.GetSwitchParameter("AvoidSelf");
			}
			set
			{
				base["AvoidSelf"] = value;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="Filter")]
		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(Mandatory=true, ParameterSetName="DiscoverByService")]
		public SwitchParameter Discover
		{
			get
			{
				return base.GetSwitchParameter("Discover");
			}
			set
			{
				base["Discover"] = value;
			}
		}

		[Parameter(ParameterSetName="DiscoverByService")]
		public string DomainName
		{
			get
			{
				return base["DomainName"] as string;
			}
			set
			{
				base["DomainName"] = value;
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

		[Parameter(ParameterSetName="DiscoverByService")]
		public SwitchParameter ForceDiscover
		{
			get
			{
				return base.GetSwitchParameter("ForceDiscover");
			}
			set
			{
				base["ForceDiscover"] = value;
			}
		}

		[Parameter(Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
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

		[Parameter(ParameterSetName="DiscoverByService")]
		public ADMinimumDirectoryServiceVersion MinimumDirectoryServiceVersion
		{
			get
			{
				return (ADMinimumDirectoryServiceVersion)base["MinimumDirectoryServiceVersion"];
			}
			set
			{
				base["MinimumDirectoryServiceVersion"] = value;
			}
		}

		[Parameter(ParameterSetName="DiscoverByService")]
		public SwitchParameter NextClosestSite
		{
			get
			{
				return base.GetSwitchParameter("NextClosestSite");
			}
			set
			{
				base["NextClosestSite"] = value;
			}
		}

		[Parameter(ParameterSetName="Filter")]
		[Parameter(ParameterSetName="Identity")]
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

		[Parameter(ParameterSetName="DiscoverByService")]
		public ADDiscoverableService[] Service
		{
			get
			{
				return base["Service"] as ADDiscoverableService[];
			}
			set
			{
				base["Service"] = value;
			}
		}

		[Parameter(ParameterSetName="DiscoverByService")]
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

		[Parameter(ParameterSetName="DiscoverByService")]
		public SwitchParameter Writable
		{
			get
			{
				return base.GetSwitchParameter("Writable");
			}
			set
			{
				base["Writable"] = value;
			}
		}

		public GetADDomainControllerParameterSet()
		{
		}
	}
}