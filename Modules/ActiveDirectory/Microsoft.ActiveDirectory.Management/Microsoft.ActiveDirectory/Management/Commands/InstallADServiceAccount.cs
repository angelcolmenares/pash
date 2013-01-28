using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Install", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219319", SupportsShouldProcess=true)]
	public class InstallADServiceAccount : ADServiceAccountCmdletBase<InstallADServiceAccountParameterSet>
	{
		public InstallADServiceAccount() : base("Install")
		{
		}

		protected internal override ICollection<string> GetExtendedPropertiesToFetch()
		{
			List<string> strs = new List<string>();
			strs.Add("msDS-HostServiceAccountBL");
			return strs;
		}

		protected internal override void PerformOperation(ADObject serviceAccount)
		{
			int num;
			string name = serviceAccount.Name;
			string value = (string)serviceAccount["sAMAccountName"].Value;
			if (string.CompareOrdinal("msDS-ManagedServiceAccount", serviceAccount["objectClass"].Value as string) == 0)
			{
				object obj = serviceAccount["msDS-HostServiceAccountBL"].Value;
				string[] strArrays = obj as string[];
				if (strArrays == null)
				{
					string str = (string)serviceAccount["msDS-HostServiceAccountBL"].Value;
					if (str != null)
					{
						string[] strArrays1 = new string[1];
						strArrays1[0] = str;
						strArrays = strArrays1;
					}
				}
				if (strArrays != null)
				{
					CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
					ADComputerFactory<ADComputer> aDComputerFactory = new ADComputerFactory<ADComputer>();
					aDComputerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADObject directoryObjectFromIdentity = aDComputerFactory.GetDirectoryObjectFromIdentity(new ADComputer(Environment.MachineName), cmdletSessionInfo.DefaultPartitionPath);
					string distinguishedName = directoryObjectFromIdentity.DistinguishedName;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						if (!strArrays[i].Equals(distinguishedName) && !this._cmdletParameters.GetSwitchParameterBooleanValue("Force"))
						{
							object[] objArray = new object[2];
							objArray[0] = serviceAccount.Name;
							objArray[1] = strArrays[i];
							if (!base.ShouldContinue(string.Format(CultureInfo.CurrentCulture, StringResources.OtherBackLinkDescription, objArray), StringResources.OtherBackLinkCaption))
							{
								return;
							}
						}
					}
				}
			}
			value = base.TrimServiceAccountSamAccountName(value);
			base.ValidateServiceAccountSamAccountNameLength(value);
			SecureString item = null;
			if (this._cmdletParameters.Contains("AccountPassword"))
			{
				item = (SecureString)this._cmdletParameters["AccountPassword"];
			}
			if (this._cmdletParameters.GetSwitchParameterBooleanValue("PromptForPassword") && item == null)
			{
				ADPasswordUtil aDPasswordUtil = new ADPasswordUtil(base.InvokeCommand);
				item = aDPasswordUtil.PromptOldPassword(serviceAccount);
			}
			if (item == null)
			{
				num = UnsafeNativeMethods.NetAddServiceAccount(null, value, null, 1);
			}
			else
			{
				IntPtr bSTR = Marshal.SecureStringToBSTR(item);
				string stringUni = Marshal.PtrToStringUni(bSTR);
				num = UnsafeNativeMethods.NetAddServiceAccount(null, name, stringUni, 2);
				Marshal.ZeroFreeBSTR(bSTR);
			}
			if (num != 0)
			{
				string ntStatusMessage = Utils.GetNtStatusMessage(num);
				object[] objArray1 = new object[1];
				objArray1[0] = ntStatusMessage;
				base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.CannotInstallServiceAccount, objArray1)), "InstallADServiceAccount:PerformOperation:InstallServiceAcccountFailure", ErrorCategory.WriteError, name));
			}
		}
	}
}