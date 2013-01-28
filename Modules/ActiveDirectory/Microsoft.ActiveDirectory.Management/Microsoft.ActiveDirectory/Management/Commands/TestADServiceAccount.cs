using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Test", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216559")]
	public class TestADServiceAccount : ADServiceAccountCmdletBase<TestADServiceAccountParameterSet>
	{
		public TestADServiceAccount() : base("Test")
		{
		}

		protected internal override void PerformOperation(ADObject serviceAccount)
		{
			string name = serviceAccount.Name;
			string value = (string)serviceAccount["sAMAccountName"].Value;
			value = base.TrimServiceAccountSamAccountName(value);
			base.ValidateServiceAccountSamAccountNameLength(value);
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = UnsafeNativeMethods.NetQueryServiceAccount(null, value, 0, out zero);
				if (num == 0)
				{
					MSA_INFO_0 structure = (MSA_INFO_0)Marshal.PtrToStructure(zero, typeof(MSA_INFO_0));
					if (structure.State != MSA_INFO_STATE.MsaInfoInstalled)
					{
						MSA_INFO_STATE state = structure.State;
						switch (state)
						{
							case MSA_INFO_STATE.MsaInfoNotExist:
							{
								object[] objArray = new object[1];
								objArray[0] = name;
								base.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.MsaDoesNotExist, objArray));
								break;
							}
							case MSA_INFO_STATE.MsaInfoNotService:
							{
								object[] objArray1 = new object[1];
								objArray1[0] = name;
								base.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.MsaNotServiceAccount, objArray1));
								break;
							}
							case MSA_INFO_STATE.MsaInfoCannotInstall:
							{
								object[] objArray2 = new object[1];
								objArray2[0] = name;
								base.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.MsaStandaloneLinkedToAlternateComputer, objArray2));
								break;
							}
							case MSA_INFO_STATE.MsaInfoCanInstall:
							{
								object[] objArray3 = new object[1];
								objArray3[0] = name;
								base.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.MsaStandloneNotLinked, objArray3));
								break;
							}
						}
						base.WriteObject(false);
					}
					else
					{
						base.WriteObject(true);
					}
				}
				else
				{
					string ntStatusMessage = Utils.GetNtStatusMessage(num);
					object[] objArray4 = new object[2];
					objArray4[0] = name;
					objArray4[1] = ntStatusMessage;
					base.WriteError(new ErrorRecord(new Win32Exception(ntStatusMessage), string.Format(CultureInfo.CurrentCulture, StringResources.CannotTestServiceAccount, objArray4), ErrorCategory.InvalidOperation, name));
				}
			}
			finally
			{
				UnsafeNativeMethods.NetApiBufferFree(zero);
			}
		}
	}
}