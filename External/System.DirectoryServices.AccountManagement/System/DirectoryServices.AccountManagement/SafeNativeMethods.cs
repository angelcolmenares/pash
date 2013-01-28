using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[SuppressUnmanagedCodeSecurity]
	internal class SafeNativeMethods
	{
		private SafeNativeMethods()
		{
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern int GetCurrentThreadId();

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaNtStatusToWinError(int ntStatus);
	}
}