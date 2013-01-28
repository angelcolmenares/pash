using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADServiceAccount", DefaultParameterSetName="Group", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219329", SupportsShouldProcess=true)]
	public class NewADServiceAccount : ADNewCmdletBase<NewADServiceAccountParameterSet, ADServiceAccountFactory<ADServiceAccount>, ADServiceAccount>
	{
		private const string _debugCategory = "NewADServiceAccount";

		public NewADServiceAccount()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.NewADServiceAccountBeginProcessCSRoutine));
		}

		protected internal override string GenerateObjectClass(ADServiceAccountFactory<ADServiceAccount> factory, ADParameterSet cmdletParameters, NewADServiceAccountParameterSet dynamicParameters)
		{
			if (!cmdletParameters.GetSwitchParameterBooleanValue("RestrictToSingleComputer"))
			{
				return "msDS-GroupManagedServiceAccount";
			}
			else
			{
				return "msDS-ManagedServiceAccount";
			}
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.MSAContainerGuid);
		}

		private bool NewADServiceAccountBeginProcessCSRoutine()
		{
			if (!this._cmdletParameters.GetSwitchParameterBooleanValue("RestrictToSingleComputer"))
			{
				this._defaultParamSet["ManagedPasswordIntervalInDays"] = 30;
				this._defaultParamSet["Enabled"] = true;
			}
			this._defaultParamSet["KerberosEncryptionType"] = ADKerberosEncryptionType.RC4 | ADKerberosEncryptionType.AES128 | ADKerberosEncryptionType.AES256;
			if (this._cmdletParameters.GetSwitchParameterBooleanValue("RestrictToSingleComputer"))
			{
				return true;
			}
			else
			{
				return base.GetADCmdletBaseExternalDelegates().AddSessionOptionWindows2012AndAboveRequiredCSRoutine();
			}
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ADRootDSE.ServerType == ADServerType.ADDS && string.IsNullOrEmpty((string)this._cmdletParameters["SamAccountName"]))
			{
				this._cmdletParameters["SamAccountName"] = this._cmdletParameters["Name"];
			}
		}
	}
}