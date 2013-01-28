using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADCentralAccessRule", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219593", SupportsShouldProcess=true)]
	public class NewADCentralAccessRule : ADNewCmdletBase<NewADCentralAccessRuleParameterSet, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		public NewADCentralAccessRule()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			IntPtr zero = IntPtr.Zero;
			try
			{
				int defaultCAPESecurityDescriptor = UnsafeNativeMethods.GetDefaultCAPESecurityDescriptor(out zero);
				if (defaultCAPESecurityDescriptor == 0)
				{
					string stringAuto = Marshal.PtrToStringAuto(zero);
					this._defaultParamSet["CurrentAcl"] = stringAuto;
				}
			}
			finally
			{
				UnsafeNativeMethods.LocalFree(zero);
			}
		}
	}
}