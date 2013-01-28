using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal class SecurityHandler
	{
		private bool needToReset;

		private IntPtr handle;

		private ManagementScope scope;

		internal SecurityHandler(ManagementScope theScope)
		{
			this.scope = theScope;
			if (this.scope != null && this.scope.Options.EnablePrivileges)
			{
				WmiNetUtilsHelper.SetSecurity_f(out this.needToReset, out this.handle);
			}
		}

		internal void Reset()
		{
			if (this.needToReset)
			{
				this.needToReset = false;
				if (this.scope != null)
				{
					WmiNetUtilsHelper.ResetSecurity_f(this.handle);
				}
			}
		}

		internal void Secure(IWbemServices services)
		{
			if (this.scope != null)
			{
				IntPtr password = this.scope.Options.GetPassword();
				int num = WmiNetUtilsHelper.BlessIWbemServices_f(services, this.scope.Options.Username, password, this.scope.Options.Authority, (int)this.scope.Options.Impersonation, (int)this.scope.Options.Authentication);
				Marshal.ZeroFreeBSTR(password);
				if (num < 0)
				{
					Marshal.ThrowExceptionForHR(num);
				}
			}
		}

		internal void SecureIUnknown(object unknown)
		{
			if (this.scope != null)
			{
				IntPtr password = this.scope.Options.GetPassword();
				int num = WmiNetUtilsHelper.BlessIWbemServicesObject_f(unknown, this.scope.Options.Username, password, this.scope.Options.Authority, (int)this.scope.Options.Impersonation, (int)this.scope.Options.Authentication);
				Marshal.ZeroFreeBSTR(password);
				if (num < 0)
				{
					Marshal.ThrowExceptionForHR(num);
				}
			}
		}
	}
}