using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Interop
{
	[SecurityCritical]
	internal sealed class SafeEventLogWriteHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[SecurityCritical]
		private SafeEventLogWriteHandle() : base(true)
		{
		}

		[DllImport("advapi32", CharSet=CharSet.None)]
		private static extern bool DeregisterEventSource(IntPtr hEventLog);

		[SecurityCritical]
		public static SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName)
		{
			SafeEventLogWriteHandle safeEventLogWriteHandle = UnsafeNativeMethods.RegisterEventSource(uncServerName, sourceName);
			Marshal.GetLastWin32Error();
			//TODO: REVIEW: safeEventLogWriteHandle.IsInvalid;
			return safeEventLogWriteHandle;
		}

		[SecurityCritical]
		protected override bool ReleaseHandle()
		{
			return SafeEventLogWriteHandle.DeregisterEventSource(this.handle);
		}
	}
}