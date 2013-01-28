using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADUserResultantPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219313", DefaultParameterSetName="Identity")]
	public class GetADUserResultantPasswordPolicy : ADGetPropertiesCmdletBase<GetADUserResultantPasswordPolicyParameterSet, ADUserFactory<ADUser>, ADUser, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	{
		private string _sourceProperty;

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
				return this._sourceProperty;
			}
		}

		internal override SourcePropertyType SourcePropertyType
		{
			get
			{
				return SourcePropertyType.IdentityInfo;
			}
		}

		public GetADUserResultantPasswordPolicy()
		{
			this._sourceProperty = "msDS-ResultantPSO";
		}
	}
}