using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Reset", "ADServiceAccountPassword", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216403", SupportsShouldProcess=true)]
	public class ResetADServiceAccountPassword : ADServiceAccountCmdletBase<ResetADServiceAccountPasswordParameterSet>
	{
		public ResetADServiceAccountPassword() : base("Reset")
		{
		}

		protected internal override void PerformOperation(ADObject serviceAccount)
		{
			bool flag = false;
			string name = serviceAccount.Name;
			string value = (string)serviceAccount["sAMAccountName"].Value;
			value = base.TrimServiceAccountSamAccountName(value);
			base.ValidateServiceAccountSamAccountNameLength(value);
			int num = UnsafeNativeMethods.NetIsServiceAccount(null, value, ref flag);
			if (num != 0)
			{
				string ntStatusMessage = Utils.GetNtStatusMessage(num);
				object[] objArray = new object[1];
				objArray[0] = ntStatusMessage;
				base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.CannotResetPasswordOfServiceAccount, objArray)), "InstallADServiceAccount:PerformOperation:NetIsServiceAccountFailed", ErrorCategory.ReadError, name));
			}
			else
			{
				if (!flag)
				{
					base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ServiceAccountIsNotInstalled, new object[0])), "InstallADServiceAccount:PerformOperation:ServiceAccountNotInstalled", ErrorCategory.ReadError, name));
					return;
				}
				else
				{
					num = UnsafeNativeMethods.NetAddServiceAccount(null, value, null, 1);
					if (num != 0)
					{
						string str = Utils.GetNtStatusMessage(num);
						object[] objArray1 = new object[1];
						objArray1[0] = str;
						base.WriteError(new ErrorRecord(new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NetAddServiceAccountFailed, objArray1)), "InstallADServiceAccount:PerformOperation:NetAddServiceAccountFailed", ErrorCategory.WriteError, name));
						return;
					}
				}
			}
		}
	}
}