using System;
using System.Runtime.InteropServices;

namespace Microsoft.ActiveDirectory.Management
{
	internal class UnsafeNativeMethods
	{
		internal const int LOGON32_LOGON_INTERACTIVE = 2;

		internal const int LOGON32_PROVIDER_DEFAULT = 0;

		internal const int ERROR_SUCCESS = 0;

		internal const int ERROR_NO_SUCH_DOMAIN = 0x54b;

		internal const int ERROR_INVALID_FLAGS = 0x3ec;

		internal const int SERVICE_ACCOUNT_FLAG_LINK_TO_HOST_ONLY = 1;

		internal const int SERVICE_ACCOUNT_FLAG_ADD_AGAINST_RODC = 2;

		internal const int SERVICE_ACCOUNT_FLAG_UNLINK_FROM_HOST_ONLY = 1;

		internal const int SERVICE_ACCOUNT_FLAG_REMOVE_OFFLINE = 2;

		internal const int SERVICE_ACCOUNT_DONT_DELETE = 1;

		internal const int SERVICE_ACCOUNT_UNLINK_LOCALLY = 2;

		internal const int SERVICE_ACCOUNT_DONT_CREATE = 1;

		internal const int SERVICE_ACCOUNT_DONT_CREATE_USE_PASSWORD = 2;

		internal const uint STATUS_NO_SUCH_DOMAIN = 0xc00000df;

		internal const int FORMAT_MESSAGE_ALLOCATE_BUFFER_FLAG = 0x100;

		internal const int FORMAT_MESSAGE_FROM_HMODULE_FLAG = 0x800;

		internal const int FORMAT_MESSAGE_FROM_SYSTEM_FLAG = 0x1000;

		public UnsafeNativeMethods()
		{

		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string stringSecurityDescriptor, uint StringSDRevision, out IntPtr SecurityDescriptor, IntPtr SecurityDescriptorSize);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int DsGetDcName(string ComputerName, string DomainName, int DomainGuid, string SiteName, uint Flags, out IntPtr pDOMAIN_CONTROLLER_INFO);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int DsGetSiteName(string ComputerName, out IntPtr pSiteNameBuffer);

		[DllImport("dsrolesrv.dll", CharSet=CharSet.Auto)]
		public static extern int DsRoleGetCustomAllowListPathRank(string pwszCustomerAllowPath, out int piCustomAllowListPathRank, out string ppwszHighestRankedPath, out int piHighestRankedPathRank);

		[DllImport("dsrolesrv.dll", CharSet=CharSet.Auto)]
		public static extern int DsRoleIsPassedAllowedList(bool fUseCustomerAllowList, out bool pfIsAllowed, out string pbstrFailedList, out string ppwszCustomerAllowListFileName, out int piCustomAllowListFileNameRank);

		[DllImport("dsrolesrv.dll", CharSet=CharSet.Auto)]
		public static extern int DsRolepCheckExistingCloneConfigFile(out bool pfFileExisting, string pwszStringNames, int dwBufferLength);

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		internal static extern int FormatMessage(uint dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, out IntPtr lpBuffer, uint nSize, IntPtr va_list_arguments);

		[DllImport("Kernel32.dll", CharSet=CharSet.None)]
		public static extern bool FreeLibrary(IntPtr moduleHandle);

		[DllImport("authz.dll", CharSet=CharSet.None)]
		internal static extern int GenerateNewCAPID(out IntPtr SID);

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		internal static extern int GetDefaultCAPESecurityDescriptor(out IntPtr pCAPESDDL);

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr LoadLibrary(string libraryName);

		[DllImport("Kernel32.dll", CharSet=CharSet.None)]
		public static extern IntPtr LocalFree(IntPtr hMem);

		[DllImport("logoncli.dll", CharSet=CharSet.Auto)]
		internal static extern int NetAddServiceAccount(string ServerName, string AccountName, string Reserved, int Flags);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		internal static extern int NetApiBufferFree(IntPtr Buffer);

		[DllImport("logoncli.dll", CharSet=CharSet.Auto)]
		internal static extern int NetIsServiceAccount(string ServerName, string AccountName, ref bool IsService);

		[DllImport("logoncli.dll", CharSet=CharSet.Auto)]
		internal static extern int NetQueryServiceAccount(string ServerName, string AccountName, uint InfoLevel, out IntPtr Buffer);

		[DllImport("logoncli.dll", CharSet=CharSet.Auto)]
		internal static extern int NetRemoveServiceAccount(string ServerName, string AccountName, int Flags);
	}
}