using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeThreadHandle() : base(true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return SafeNativeMethods.CloseHandle(this.handle);
		}
	}
}