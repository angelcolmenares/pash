using System;
using System.Runtime.InteropServices;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal static class NativeMethods
	{
		public const int LOGON32_LOGON_NETWORK = 3;

		public const int LOGON32_PROVIDER_DEFAULT = 0;
		/*
		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
		*/

		public static bool CloseHandle(IntPtr handle)
		{
			return true;
		}

		public static bool LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken)
		{
			//if  (lpszUserName == "bruno") return true;
			return true;
		}


	}
}