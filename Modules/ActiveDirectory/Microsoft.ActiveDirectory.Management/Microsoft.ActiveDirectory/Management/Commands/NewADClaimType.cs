using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADClaimType", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216375", SupportsShouldProcess=true, DefaultParameterSetName="SourceAttribute")]
	public class NewADClaimType : ADNewCmdletBase<NewADClaimTypeParameterSet, ADClaimTypeFactory<ADClaimType>, ADClaimType>
	{
		public NewADClaimType()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.NewADClaimTypeCmdletValidationCSRoutine));
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
		}

		private bool NewADClaimTypeCmdletValidationCSRoutine()
		{
			if (string.IsNullOrEmpty(this._cmdletParameters.DisplayName) || !ADCBACUtil.IsAttributeValueUsed<ADClaimTypeFactory<ADClaimType>, ADClaimType>("displayName", this._cmdletParameters.DisplayName, this.GetCmdletSessionInfo(), this.GetDefaultCreationPathBase()))
			{
				return true;
			}
			else
			{
				object[] displayName = new object[1];
				displayName[0] = this._cmdletParameters.DisplayName;
				base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.DisplayNameNotUniqueError, displayName)), "NewADClaimType:NewADClaimTypeCmdletValidationCSRoutine", ErrorCategory.InvalidData, null));
				return false;
			}
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			this._defaultParamSet["Enabled"] = true;
			this._defaultParamSet["IsSingleValued"] = true;
			string[] strArrays = new string[1];
			strArrays[0] = "user";
			this._defaultParamSet["AppliesToClasses"] = strArrays;
		}
	}
}