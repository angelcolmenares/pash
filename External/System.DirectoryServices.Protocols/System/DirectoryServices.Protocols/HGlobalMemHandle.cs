using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.Protocols
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class HGlobalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal HGlobalMemHandle(IntPtr value) : base(true)
		{
			base.SetHandle(value);
		}

		protected override bool ReleaseHandle()
		{
			Marshal.FreeHGlobal(this.handle);
			return true;
		}
	}
}