using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADFineGrainedPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219327", SupportsShouldProcess=true)]
	public class NewADFineGrainedPasswordPolicy : ADNewCmdletBase<NewADFineGrainedPasswordPolicyParameterSet, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	{
		private const string _debugCategory = "NewADFineGrainedPasswordPolicy";

		public NewADFineGrainedPasswordPolicy()
		{
		}

		protected internal override string GetDefaultCreationPath()
		{
			string wellKnownGuidDN = Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.SystemsContainerGuid);
			if (wellKnownGuidDN == null)
			{
				return null;
			}
			else
			{
				return string.Concat("CN=Password Settings Container,", wellKnownGuidDN);
			}
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			this._defaultParamSet["ComplexityEnabled"] = true;
			this._defaultParamSet["ReversibleEncryptionEnabled"] = true;
			this._defaultParamSet["PasswordHistoryCount"] = 24;
			this._defaultParamSet["MinPasswordLength"] = 7;
			this._defaultParamSet["MinPasswordAge"] = new TimeSpan(1, 0, 0, 0);
			this._defaultParamSet["MaxPasswordAge"] = new TimeSpan(42, 0, 0, 0);
			this._defaultParamSet["LockoutThreshold"] = 0;
			this._defaultParamSet["LockoutObservationWindow"] = new TimeSpan(0, 30, 0);
			this._defaultParamSet["LockoutDuration"] = new TimeSpan(0, 30, 0);
		}
	}
}