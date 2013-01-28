using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.ActiveDirectory
{
	[ComVisible(false)]
	[SuppressUnmanagedCodeSecurity]
	internal sealed class NativeMethods
	{
		internal const int VER_PLATFORM_WIN32_NT = 2;

		internal const int ERROR_INVALID_DOMAIN_NAME_FORMAT = 0x4bc;

		internal const int ERROR_NO_SUCH_DOMAIN = 0x54b;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_INVALID_FLAGS = 0x3ec;

		internal const int DS_NAME_NO_ERROR = 0;

		internal const int ERROR_NO_MORE_ITEMS = 0x103;

		internal const int ERROR_FILE_MARK_DETECTED = 0x44d;

		internal const int DNS_ERROR_RCODE_NAME_ERROR = 0x232b;

		internal const int ERROR_NO_SUCH_LOGON_SESSION = 0x520;

		internal const int DS_NAME_FLAG_SYNTACTICAL_ONLY = 1;

		internal const int DS_FQDN_1779_NAME = 1;

		internal const int DS_CANONICAL_NAME = 7;

		internal const int DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 6;

		internal const int STATUS_QUOTA_EXCEEDED = -1073741756;

		internal const int DsDomainControllerInfoLevel2 = 2;

		internal const int DsDomainControllerInfoLevel3 = 3;

		internal const int DsNameNoError = 0;

		internal const int DnsSrvData = 33;

		internal const int DnsQueryBypassCache = 8;

		internal const int NegGetCallerName = 1;

		private NativeMethods()
		{
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		internal static extern int CompareString(uint locale, uint dwCmpFlags, IntPtr lpString1, int cchCount1, IntPtr lpString2, int cchCount2);

		[DllImport("Dnsapi.dll", CharSet=CharSet.Unicode)]
		internal static extern int DnsQuery(string recordName, short recordType, int options, IntPtr servers, out IntPtr dnsResultList, IntPtr reserved);

		[DllImport("Dnsapi.dll", CharSet=CharSet.Unicode)]
		internal static extern void DnsRecordListFree(IntPtr dnsResultList, bool dnsFreeType);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern void DsGetDcClose(IntPtr getDcContextHandle);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int DsGetDcName(string computerName, string domainName, IntPtr domainGuid, string siteName, int flags, out IntPtr domainControllerInfo);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int DsGetDcNext(IntPtr getDcContextHandle, out IntPtr sockAddressCount, out IntPtr sockAdresses, out IntPtr dnsHostName);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int DsGetDcOpen(string dnsName, int optionFlags, string siteName, IntPtr domainGuid, string dnsForestName, int dcFlags, out IntPtr retGetDcContext);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		internal static extern int GetLastError();

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool GetVersionEx(OSVersionInfoEx ver);

		[DllImport("Secur32.dll", CharSet=CharSet.None)]
		internal static extern int LsaCallAuthenticationPackage(LsaLogonProcessSafeHandle lsaHandle, int authenticationPackage, NegotiateCallerNameRequest protocolSubmitBuffer, int submitBufferLength, out IntPtr protocolReturnBuffer, out int returnBufferLength, out int protocolStatus);

		[DllImport("Secur32.dll", CharSet=CharSet.None)]
		internal static extern int LsaConnectUntrusted(out LsaLogonProcessSafeHandle lsaHandle);

		[DllImport("Secur32.dll", CharSet=CharSet.None)]
		internal static extern int LsaDeregisterLogonProcess(IntPtr lsaHandle);

		[DllImport("Secur32.dll", CharSet=CharSet.None)]
		internal static extern uint LsaFreeReturnBuffer(IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int LsaNtStatusToWinError(int ntStatus);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		internal static extern int NetApiBufferFree(IntPtr buffer);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsBindWithCred(string domainController, string dnsDomainName, IntPtr authIdentity, out IntPtr handle);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsCrackNames(IntPtr hDS, int flags, int formatOffered, int formatDesired, int nameCount, IntPtr names, out IntPtr results);

		[SuppressUnmanagedCodeSecurity]
		internal delegate void DsFreeDomainControllerInfo(int infoLevel, int dcInfoListCount, IntPtr dcInfoList);

		[SuppressUnmanagedCodeSecurity]
		internal delegate void DsFreePasswordCredentials(IntPtr authIdentity);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsGetDomainControllerInfo(IntPtr handle, string domainName, int infoLevel, out int dcCount, out IntPtr dcInfo);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsListRoles(IntPtr dsHandle, out IntPtr roles);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsListSites(IntPtr dsHandle, out IntPtr sites);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsMakePasswordCredentials(string user, string domain, string password, out IntPtr authIdentity);

		[SuppressUnmanagedCodeSecurity]
		internal delegate int DsUnBind(ref IntPtr handle);
	}
}