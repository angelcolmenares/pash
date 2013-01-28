using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace System.DirectoryServices.ActiveDirectory
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class PolicySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal PolicySafeHandle(IntPtr value) : base(true)
		{
			base.SetHandle(value);
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.LsaClose(this.handle) == 0;
		}
	}
}