using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADResourceProperty", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216381", SupportsShouldProcess=true)]
	public class NewADResourceProperty : ADNewCmdletBase<NewADResourcePropertyParameterSet, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public NewADResourceProperty()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.NewADResourcePropertyCmdletValidationCSRoutine));
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
		}

		private bool NewADResourcePropertyCmdletValidationCSRoutine()
		{
			if (string.IsNullOrEmpty(this._cmdletParameters.DisplayName) || !ADCBACUtil.IsAttributeValueUsed<ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>("displayName", this._cmdletParameters.DisplayName, this.GetCmdletSessionInfo(), this.GetDefaultCreationPathBase()))
			{
				return true;
			}
			else
			{
				object[] displayName = new object[1];
				displayName[0] = this._cmdletParameters.DisplayName;
				base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.DisplayNameNotUniqueError, displayName)), "NewADResourceProperty:NewADResourcePropertyCmdletValidationCSRoutine", ErrorCategory.InvalidData, null));
				return false;
			}
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			this._defaultParamSet["Enabled"] = true;
			this._defaultParamSet["IsSecured"] = true;
		}
	}
}