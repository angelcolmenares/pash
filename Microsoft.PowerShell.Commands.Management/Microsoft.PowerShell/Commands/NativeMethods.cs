using System;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	internal static class NativeMethods
	{
		internal const int ERROR_SERVICE_ALREADY_RUNNING = 0x420;

		internal const int ERROR_SERVICE_NOT_ACTIVE = 0x426;

		internal const uint SC_MANAGER_CONNECT = 1;

		internal const uint SC_MANAGER_CREATE_SERVICE = 2;

		internal const uint SERVICE_QUERY_CONFIG = 1;

		internal const uint SERVICE_CHANGE_CONFIG = 2;

		internal const uint SERVICE_NO_CHANGE = 0xffffffff;

		internal const uint SERVICE_AUTO_START = 2;

		internal const uint SERVICE_DEMAND_START = 3;

		internal const uint SERVICE_DISABLED = 4;

		internal const uint SERVICE_CONFIG_DESCRIPTION = 1;

		internal const uint SERVICE_WIN32_OWN_PROCESS = 16;

		internal const uint SERVICE_ERROR_NORMAL = 1;

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool AssignProcessToJobObject(SafeHandle hJob, IntPtr hProcess);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool ChangeServiceConfig2W(IntPtr hService, uint dwInfoLevel, IntPtr lpInfo);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool ChangeServiceConfigW(IntPtr hService, uint dwServiceType, uint dwStartType, uint dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, IntPtr lpPassword, string lpDisplayName);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool CloseServiceHandle(IntPtr hSCManagerOrService);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		internal static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern IntPtr CreateServiceW(IntPtr hSCManager, string lpServiceName, string lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, string lpdwTagId, IntPtr lpDependencies, string lpServiceStartName, IntPtr lpPassword);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern IntPtr OpenSCManagerW(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern IntPtr OpenServiceW(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern bool QueryInformationJobObject(SafeHandle hJob, int JobObjectInfoClass, ref JOBOBJECT_BASIC_PROCESS_ID_LIST lpJobObjectInfo, int cbJobObjectLength, IntPtr lpReturnLength);

		internal struct SERVICE_DESCRIPTIONW
		{
			internal string lpDescription;

		}
	}
}