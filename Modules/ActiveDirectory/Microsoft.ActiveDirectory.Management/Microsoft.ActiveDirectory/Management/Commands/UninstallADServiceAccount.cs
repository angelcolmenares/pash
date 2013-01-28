using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Uninstall", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219359", SupportsShouldProcess=true)]
	public class UninstallADServiceAccount : ADServiceAccountCmdletBase<UninstallADServiceAccountParameterSet>
	{
		public UninstallADServiceAccount() : base("Uninstall")
		{
		}

		protected internal override void PerformOperation(ADObject serviceAccount)
		{
			string name = serviceAccount.Name;
			string value = (string)serviceAccount["sAMAccountName"].Value;
			value = base.TrimServiceAccountSamAccountName(value);
			base.ValidateServiceAccountSamAccountNameLength(value);
			int num = UnsafeNativeMethods.NetRemoveServiceAccount(null, value, 1);
			if (-1073741601 == num)
			{
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("ForceRemoveLocal"))
				{
					num = UnsafeNativeMethods.NetRemoveServiceAccount(null, name, 2);
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(num);
					object[] message = new object[1];
					message[0] = win32Exception.Message;
					base.WriteError(new ErrorRecord(new Win32Exception(num), string.Format(CultureInfo.CurrentCulture, StringResources.CannotReachHostingDC, message), ErrorCategory.InvalidOperation, name));
					return;
				}
			}
			if (num != 0)
			{
				string ntStatusMessage = Utils.GetNtStatusMessage(num);
				object[] objArray = new object[1];
				objArray[0] = ntStatusMessage;
				base.WriteError(new ErrorRecord(new Win32Exception(ntStatusMessage), string.Format(CultureInfo.CurrentCulture, StringResources.CannotUninstallServiceAccount, objArray), ErrorCategory.InvalidOperation, name));
			}
		}
	}
}