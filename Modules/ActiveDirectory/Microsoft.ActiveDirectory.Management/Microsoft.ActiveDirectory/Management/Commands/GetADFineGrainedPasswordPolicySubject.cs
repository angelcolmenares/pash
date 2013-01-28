using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADFineGrainedPasswordPolicySubject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219310")]
	public class GetADFineGrainedPasswordPolicySubject : ADGetPropertiesCmdletBase<GetADFineGrainedPasswordPolicySubjectParameterSet, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy, ADPrincipalFactory<ADPrincipal>, ADPrincipal>
	{
		private string _sourceProperty;

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
				return this._sourceProperty;
			}
		}

		internal override SourcePropertyType SourcePropertyType
		{
			get
			{
				return SourcePropertyType.LinkedDN;
			}
		}

		public GetADFineGrainedPasswordPolicySubject()
		{
			this._sourceProperty = "msDS-PSOAppliesTo";
		}
	}
}