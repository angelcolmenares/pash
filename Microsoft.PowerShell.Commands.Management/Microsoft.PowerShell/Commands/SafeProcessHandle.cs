using Microsoft.Win32.SafeHandles;
using System;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands
{
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeProcessHandle() : base(true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return SafeNativeMethods.CloseHandle(this.handle);
		}
	}
}