using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADAccountPassword", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219353", SupportsShouldProcess=true)]
	public class SetADAccountPassword : ADSetCmdletBase<SetADAccountPasswordParameterSet, ADAccountFactory<ADAccount>, ADAccount>
	{
		public SetADAccountPassword()
		{
			base.BeginProcessPipeline.Clear();
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADAccountPasswordProcessCSRoutine));
		}

		private bool SetADAccountPasswordProcessCSRoutine()
		{
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			ADPasswordUtil aDPasswordUtil = new ADPasswordUtil(base.InvokeCommand);
			bool switchParameterBooleanValue = this._cmdletParameters.GetSwitchParameterBooleanValue("Reset");
			SecureString item = this._cmdletParameters["OldPassword"] as SecureString;
			SecureString secureString = this._cmdletParameters["NewPassword"] as SecureString;
			base.ValidateParameters();
			ADAccount aDAccount = this._cmdletParameters["Identity"] as ADAccount;
			this.SetPipelinedSessionInfo(aDAccount.SessionInfo);
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(aDAccount, cmdletSessionInfo.DefaultPartitionPath);
			if (!switchParameterBooleanValue)
			{
				if (secureString == null || item != null)
				{
					aDPasswordUtil.ChangePassword(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity, item, secureString);
				}
				else
				{
					try
					{
						aDPasswordUtil.SetPassword(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity, secureString);
					}
					catch
					{
						aDPasswordUtil.ChangePassword(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity, item, secureString);
					}
				}
			}
			else
			{
				aDPasswordUtil.SetPassword(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity, secureString);
			}
			if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
			{
				ADAccount extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(directoryObjectFromIdentity.DistinguishedName);
				base.WriteObject(extendedObjectFromDN);
			}
			return true;
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			bool switchParameterBooleanValue = this._cmdletParameters.GetSwitchParameterBooleanValue("Reset");
			SecureString item = this._cmdletParameters["OldPassword"] as SecureString;
			if (!switchParameterBooleanValue || item == null)
			{
				return;
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = "OldPassword";
				objArray[1] = "Reset";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredOnlyOne, objArray));
			}
		}
	}
}