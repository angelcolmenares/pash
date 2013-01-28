using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.DirectoryServices.ActiveDirectory
{
	[ComVisible(false)]
	[SuppressUnmanagedCodeSecurity]
	internal class UnsafeNativeMethods
	{
		public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;

		public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;

		public const int FORMAT_MESSAGE_FROM_STRING = 0x400;

		public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;

		public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

		public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;

		public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;

		public UnsafeNativeMethods()
		{
		}

		[DllImport("activeds.dll", CharSet=CharSet.Unicode)]
		public static extern int ADsEncodeBinaryData(byte[] data, int length, ref IntPtr result);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern int CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int ConvertSidToStringSidW(IntPtr pSid, ref IntPtr stringSid);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int ConvertStringSidToSidW(IntPtr stringSid, ref IntPtr pSid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool CopySid(int destinationLength, IntPtr pSidDestination, IntPtr pSidSource);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsEnumerateDomainTrustsW(string serverName, int flags, out IntPtr domains, out int count);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsGetSiteName(string dcName, ref IntPtr ptr);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		public static extern int DsRoleFreeMemory(IntPtr buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsRoleGetPrimaryDomainInformation(string lpServer, DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, out IntPtr Buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsRoleGetPrimaryDomainInformation(IntPtr lpServer, DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, out IntPtr Buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool EqualDomainSid(IntPtr pSid1, IntPtr pSid2, ref bool equal);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern int FormatMessageW(int dwFlags, int lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, int arguments);

		[DllImport("activeds.dll", CharSet=CharSet.None)]
		public static extern bool FreeADsMem(IntPtr pVoid);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern uint FreeLibrary(IntPtr libName);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern int GetCurrentThreadId();

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int GetLengthSid(IntPtr sid);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetProcAddress(LoadLibrarySafeHandle hModule, string entryPoint);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidIdentifierAuthority(IntPtr sid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidSubAuthority(IntPtr sid, int index);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidSubAuthorityCount(IntPtr sid);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern void GetSystemTimeAsFileTime(IntPtr fileTime);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass, IntPtr buffer, int bufferSize, ref int returnLength);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int I_NetLogonControl2(string serverName, int FunctionCode, int QueryLevel, IntPtr data, out IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int ImpersonateAnonymousToken(IntPtr token);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int ImpersonateLoggedOnUser(IntPtr hToken);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool IsValidSid(IntPtr sid);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr LoadLibrary(string name);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern int LocalFree(IntPtr mem);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LogonUserW(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaClose(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaCreateTrustedDomainEx(PolicySafeHandle handle, TRUSTED_DOMAIN_INFORMATION_EX domainEx, TRUSTED_DOMAIN_AUTH_INFORMATION authInfo, int classInfo, out IntPtr domainHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaDeleteTrustedDomain(PolicySafeHandle handle, IntPtr pSid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaFreeMemory(IntPtr ptr);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaLookupSids(IntPtr policyHandle, int count, IntPtr[] sids, out IntPtr referencedDomains, out IntPtr names);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaNtStatusToWinError(int status);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaOpenPolicy(LSA_UNICODE_STRING target, LSA_OBJECT_ATTRIBUTES objectAttributes, int access, out IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaOpenPolicy(IntPtr lsaUnicodeString, IntPtr lsaObjectAttributes, int desiredAccess, ref IntPtr policyHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaOpenTrustedDomainByName(PolicySafeHandle policyHandle, LSA_UNICODE_STRING trustedDomain, int access, ref IntPtr trustedDomainHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaQueryForestTrustInformation(PolicySafeHandle handle, LSA_UNICODE_STRING target, ref IntPtr ForestTrustInfo);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaQueryInformationPolicy(PolicySafeHandle handle, int infoClass, out IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaQueryInformationPolicy(IntPtr policyHandle, int policyInformationClass, ref IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaQueryTrustedDomainInfoByName(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomain, TRUSTED_INFORMATION_CLASS infoClass, ref IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaSetForestTrustInformation(PolicySafeHandle handle, LSA_UNICODE_STRING target, IntPtr forestTrustInfo, int checkOnly, out IntPtr collisionInfo);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int LsaSetTrustedDomainInfoByName(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomain, TRUSTED_INFORMATION_CLASS infoClass, IntPtr buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		public static extern int NetApiBufferFree(IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern IntPtr OpenThread(uint desiredAccess, bool inheirted, int threadID);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool OpenThreadToken(IntPtr threadHandle, int desiredAccess, bool openAsSelf, ref IntPtr tokenHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int RevertToSelf();

		[DllImport("ntdll.dll", CharSet=CharSet.None)]
		public static extern int RtlInitUnicodeString(LSA_UNICODE_STRING result, IntPtr s);

		[SuppressUnmanagedCodeSecurity]
		public delegate void DsFreeNameResultW(IntPtr result);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsListDomainsInSiteW(IntPtr handle, string site, ref IntPtr info);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaConsistencyCheck(IntPtr handle, int taskID, int flags);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaFreeInfo(int type, IntPtr value);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaGetInfo2W(IntPtr handle, int type, string objectPath, IntPtr sourceGUID, string attributeName, string value, int flag, int context, ref IntPtr info);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaGetInfoW(IntPtr handle, int type, string objectPath, IntPtr sourceGUID, ref IntPtr info);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaSyncAllW(IntPtr handle, string partition, int flags, SyncReplicaFromAllServersCallback callback, IntPtr data, ref IntPtr error);

		[SuppressUnmanagedCodeSecurity]
		public delegate int DsReplicaSyncW(IntPtr handle, string partition, IntPtr uuid, int option);
	}
}