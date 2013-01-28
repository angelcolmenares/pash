using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADComputerServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219315")]
	public class GetADComputerServiceAccount : ADGetPropertiesCmdletBase<GetADComputerServiceAccountParameterSet, ADComputerFactory<ADComputer>, ADComputer, ADServiceAccountFactory<ADServiceAccount>, ADServiceAccount>
	{
		internal override bool AutoRangeRetrieve
		{
			get
			{
				return false;
			}
		}

		internal override IdentityLookupMode IdentityLookupMode
		{
			get
			{
				return IdentityLookupMode.DirectoryMode;
			}
		}

		internal override string SourceProperty
		{
			get
			{
				return "msDS-HostServiceAccount";
			}
		}

		internal override SourcePropertyType SourcePropertyType
		{
			get
			{
				return SourcePropertyType.LinkedDN;
			}
		}

		public GetADComputerServiceAccount()
		{
		}
	}
}