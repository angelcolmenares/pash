using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace System.DirectoryServices.ActiveDirectory
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class LoadLibrarySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private LoadLibrarySafeHandle() : base(true)
		{
		}

		internal LoadLibrarySafeHandle(IntPtr value) : base(true)
		{
			base.SetHandle(value);
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.FreeLibrary(this.handle) != 0;
		}
	}
}