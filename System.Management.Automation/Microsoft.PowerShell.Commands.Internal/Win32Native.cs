namespace Microsoft.PowerShell.Commands.Internal
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class Win32Native
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const int ANONYMOUS_LOGON_LUID = 0x3e6;
        internal const string CRYPT32 = "crypt32.dll";
        internal const int CRYPTPROTECTMEMORY_BLOCK_SIZE = 0x10;
        internal const int CRYPTPROTECTMEMORY_CROSS_PROCESS = 1;
        internal const int CRYPTPROTECTMEMORY_SAME_LOGON = 2;
        internal const int CRYPTPROTECTMEMORY_SAME_PROCESS = 0;
        internal const int DOMAIN_USER_RID_GUEST = 0x1f5;
        internal const int DUPLICATE_CLOSE_SOURCE = 1;
        internal const int DUPLICATE_SAME_ACCESS = 2;
        internal const int DUPLICATE_SAME_ATTRIBUTES = 4;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_ALREADY_EXISTS = 0xb7;
        internal const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_BAD_PATHNAME = 0xa1;
        internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;
        internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;
        internal const int ERROR_DLL_INIT_FAILED = 0x45a;
        internal const int ERROR_ENVVAR_NOT_FOUND = 0xcb;
        internal const int ERROR_FILE_EXISTS = 80;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xce;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        internal const int ERROR_INVALID_ACL = 0x538;
        internal const int ERROR_INVALID_DATA = 13;
        internal const int ERROR_INVALID_DRIVE = 15;
        internal const int ERROR_INVALID_FUNCTION = 1;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_INVALID_NAME = 0x7b;
        internal const int ERROR_INVALID_OWNER = 0x51b;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_INVALID_PRIMARY_GROUP = 0x51c;
        internal const int ERROR_INVALID_SECURITY_DESCR = 0x53a;
        internal const int ERROR_INVALID_SID = 0x539;
        internal const int ERROR_INVALID_TRANSACTION = 0x1a2c;
        internal const int ERROR_MAX_KTM_CODE = 0x1a8f;
        internal const int ERROR_MIN_KTM_CODE = 0x1a2c;
        internal const int ERROR_MORE_DATA = 0xea;
        internal const int ERROR_NO_DATA = 0xe8;
        internal const int ERROR_NO_MORE_FILES = 0x12;
        internal const int ERROR_NO_SECURITY_ON_OBJECT = 0x546;
        internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
        internal const int ERROR_NO_TOKEN = 0x3f0;
        internal const int ERROR_NON_ACCOUNT_SID = 0x4e9;
        internal const int ERROR_NONE_MAPPED = 0x534;
        internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_NOT_SUPPORTED = 50;
        internal const int ERROR_OPERATION_ABORTED = 0x3e3;
        internal const int ERROR_PATH_NOT_FOUND = 3;
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xe9;
        internal const int ERROR_PRIVILEGE_NOT_HELD = 0x522;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6fd;
        internal const int ERROR_UNKNOWN_REVISION = 0x519;
        internal const int EVENT_MODIFY_STATE = 2;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        internal const int HWND_BROADCAST = 0xffff;
        internal const int INVALID_FILE_SIZE = -1;
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal const string KERNEL32 = "kernel32.dll";
        internal const int KEY_CREATE_LINK = 0x20;
        internal const int KEY_CREATE_SUB_KEY = 4;
        internal const int KEY_ENUMERATE_SUB_KEYS = 8;
        internal const int KEY_NOTIFY = 0x10;
        internal const int KEY_QUERY_VALUE = 1;
        internal const int KEY_READ = 0x20019;
        internal const int KEY_SET_VALUE = 2;
        internal const int KEY_WOW64_32KEY = 0x200;
        internal const int KEY_WOW64_64KEY = 0x100;
        internal const int KEY_WRITE = 0x20006;
        internal const int LMEM_FIXED = 0;
        internal const int LMEM_ZEROINIT = 0x40;
        internal const int LPTR = 0x40;
        internal const string LSTRCPY = "lstrcpy";
        internal const string LSTRCPYN = "lstrcpyn";
        internal const string LSTRLEN = "lstrlen";
        internal const string LSTRLENA = "lstrlenA";
        internal const string LSTRLENW = "lstrlenW";
        private const int MAX_DEFAULTCHAR = 2;
        private const int MAX_LEADBYTES = 12;
        internal const string MICROSOFT_KERBEROS_NAME = "Kerberos";
        internal const string MOVEMEMORY = "RtlMoveMemory";
        internal const string MSCORWKS = "mscorwks.dll";
        internal const int MUTEX_ALL_ACCESS = 0x1f0001;
        internal const int MUTEX_MODIFY_STATE = 1;
        internal static readonly IntPtr NULL = IntPtr.Zero;
        internal const string OLE32 = "ole32.dll";
        internal const string OLEAUT32 = "oleaut32.dll";
        internal static readonly int PAGE_SIZE;
        internal const int READ_CONTROL = 0x20000;
        internal const int REG_BINARY = 3;
        internal const int REG_DWORD = 4;
        internal const int REG_DWORD_BIG_ENDIAN = 5;
        internal const int REG_DWORD_LITTLE_ENDIAN = 4;
        internal const int REG_EXPAND_SZ = 2;
        internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
        internal const int REG_LINK = 6;
        internal const int REG_MULTI_SZ = 7;
        internal const int REG_NONE = 0;
        internal const int REG_QWORD = 11;
        internal const int REG_RESOURCE_LIST = 8;
        internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
        internal const int REG_SZ = 1;
        private const string resBaseName = "RegistryProviderStrings";
        internal const int SE_GROUP_ENABLED = 4;
        internal const int SE_GROUP_ENABLED_BY_DEFAULT = 2;
        internal const uint SE_GROUP_LOGON_ID = 0xc0000000;
        internal const int SE_GROUP_MANDATORY = 1;
        internal const int SE_GROUP_OWNER = 8;
        internal const int SE_GROUP_RESOURCE = 0x20000000;
        internal const int SE_GROUP_USE_FOR_DENY_ONLY = 0x10;
        internal const int SE_PRIVILEGE_DISABLED = 0;
        internal const int SE_PRIVILEGE_ENABLED = 2;
        internal const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1;
        internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        internal const string SECUR32 = "secur32.dll";
        internal const int SECURITY_ANONYMOUS = 0;
        internal const int SECURITY_ANONYMOUS_LOGON_RID = 7;
        internal const int SECURITY_AUTHENTICATED_USER_RID = 11;
        internal const int SECURITY_BUILTIN_DOMAIN_RID = 0x20;
        internal const int SECURITY_LOCAL_SYSTEM_RID = 0x12;
        internal const int SECURITY_SQOS_PRESENT = 0x100000;
        internal const int SEM_FAILCRITICALERRORS = 1;
        internal const int SEMAPHORE_MODIFY_STATE = 2;
        internal const string SHFOLDER = "shfolder.dll";
        internal const string SHIM = "mscoree.dll";
        internal const int STANDARD_RIGHTS_READ = 0x20000;
        internal const int STANDARD_RIGHTS_WRITE = 0x20000;
        internal const uint STATUS_ACCESS_DENIED = 0xc0000022;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
        internal const uint STATUS_NO_MEMORY = 0xc0000017;
        internal const uint STATUS_NONE_MAPPED = 0xc0000073;
        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;
        internal const int STATUS_SOME_NOT_MAPPED = 0x107;
        internal const int STATUS_SUCCESS = 0;
        internal const int SYNCHRONIZE = 0x100000;
        internal const string USER32 = "user32.dll";
        internal const int WM_SETTINGCHANGE = 0x1a;

        static Win32Native()
        {
            SYSTEM_INFO lpSystemInfo = new SYSTEM_INFO();
            GetSystemInfo(out lpSystemInfo);
            PAGE_SIZE = (int) lpSystemInfo.dwPageSize;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr tokenHandler, bool disableAllPrivilege, ref TOKEN_PRIVILEGE newPrivilegeState, int bufferLength, out TOKEN_PRIVILEGE previousPrivilegeState, ref int returnLength);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool GetCPInfo(int codePage, out CPINFO lpCpInfo);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();
        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (FormatMessage(0x3200, NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, NULL) != 0)
            {
                return lpBuffer.ToString();
            }
            string format = RegistryProviderStrings.UnknownError_Num;
            return string.Format(CultureInfo.CurrentCulture, format, new object[] { errorCode.ToString(CultureInfo.InvariantCulture) });
        }

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetSecurityDescriptorLength(IntPtr byteArray);
        [DllImport("advapi32.dll", EntryPoint="GetSecurityInfo", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetSecurityInfoByHandle(SafeHandle handle, int objectType, int securityInformation, out IntPtr sidOwner, out IntPtr sidGroup, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);
        [DllImport("kernel32.dll")]
        internal static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int lstrlen(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int lstrlen(sbyte[] ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        internal static extern int lstrlenA(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int lstrlenW(IntPtr ptr);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool PrivilegeCheck(IntPtr tokenHandler, ref PRIVILEGE_SET requiredPrivileges, out bool pfResult);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegConnectRegistry(string machineName, SafeRegistryHandle key, out SafeRegistryHandle result);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegCreateKeyEx(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, int dwOptions, int samDesigner, SECURITY_ATTRIBUTES lpSecurityAttributes, out SafeRegistryHandle hkResult, out int lpdwDisposition);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegCreateKeyTransacted(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, int dwOptions, int samDesigner, SECURITY_ATTRIBUTES lpSecurityAttributes, out SafeRegistryHandle hkResult, out int lpdwDisposition, SafeTransactionHandle hTransaction, IntPtr pExtendedParameter);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteKey(SafeRegistryHandle hKey, string lpSubKey);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteKeyTransacted(SafeRegistryHandle hKey, string lpSubKey, int samDesired, int reserved, SafeTransactionHandle hTransaction, IntPtr pExtendedParameter);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteValue(SafeRegistryHandle hKey, string lpValueName);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegEnumKeyEx(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpName, out int lpcbName, int[] lpReserved, StringBuilder lpClass, int[] lpcbClass, long[] lpftLastWriteTime);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegEnumValue(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpValueName, ref int lpcbValueName, IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData, int[] lpcbData);
        [DllImport("advapi32.dll")]
        internal static extern int RegFlushKey(SafeRegistryHandle hKey);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegOpenKeyEx(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegOpenKeyTransacted(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult, SafeTransactionHandle hTransaction, IntPtr pExtendedParameter);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryInfoKey(SafeRegistryHandle hKey, StringBuilder lpClass, int[] lpcbClass, IntPtr lpReserved_MustBeZero, ref int lpcSubKeys, int[] lpcbMaxSubKeyLen, int[] lpcbMaxClassLen, ref int lpcValues, int[] lpcbMaxValueNameLen, int[] lpcbMaxValueLen, int[] lpcbSecurityDescriptor, int[] lpftLastWriteTime);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, StringBuilder lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, string lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, byte[] lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref int lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref long lpData, int cbData);
        [DllImport("advapi32.dll", EntryPoint="SetSecurityInfo", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SetSecurityInfoByHandle(SafeHandle handle, int objectType, int securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);
        [DllImport("kernel32.dll")]
        internal static extern UIntPtr VirtualQuery(UIntPtr lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer, UIntPtr dwLength);

        [StructLayout(LayoutKind.Sequential)]
        internal struct CPINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            internal int MaxCharSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
            public byte[] DefaultChar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] LeadBytes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME
        {
            internal int ftTimeLow;
            internal int ftTimeHigh;
            public FILE_TIME(long fileTime)
            {
                this.ftTimeLow = (int) fileTime;
                this.ftTimeHigh = (int) (fileTime >> 0x20);
            }

            public long ToTicks()
            {
                return (long) ((this.ftTimeHigh << 0x20) + this.ftTimeLow);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct KERB_S4U_LOGON
        {
            internal int MessageType;
            internal int Flags;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING ClientUpn;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING ClientRealm;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_OBJECT_ATTRIBUTES
        {
            internal int Length;
            internal IntPtr RootDirectory;
            internal IntPtr ObjectName;
            internal int Attributes;
            internal IntPtr SecurityDescriptor;
            internal IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_REFERENCED_DOMAIN_LIST
        {
            internal int Entries;
            internal IntPtr Domains;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_NAME
        {
            internal int Use;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING Name;
            internal int DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_SID
        {
            internal int Use;
            internal int Rid;
            internal int DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_SID2
        {
            internal int Use;
            internal IntPtr Sid;
            internal int DomainIndex;
            private int Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRUST_INFORMATION
        {
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING Name;
            internal IntPtr Sid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LUID
        {
            internal int LowPart;
            internal int HighPart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LUID_AND_ATTRIBUTES
        {
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID Luid;
            internal int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORY_BASIC_INFORMATION
        {
            internal UIntPtr BaseAddress;
            internal UIntPtr AllocationBase;
            internal int AllocationProtect;
            internal UIntPtr RegionSize;
            internal int State;
            internal int Protect;
            internal int Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MEMORYSTATUS
        {
            internal int length;
            internal int memoryLoad;
            internal int totalPhys;
            internal int availPhys;
            internal int totalPageFile;
            internal int availPageFile;
            internal int totalVirtual;
            internal int availVirtual;
            internal MEMORYSTATUS()
            {
                this.length = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MEMORYSTATUSEX
        {
            internal int length;
            internal int memoryLoad;
            internal ulong totalPhys;
            internal ulong availPhys;
            internal ulong totalPageFile;
            internal ulong availPageFile;
            internal ulong totalVirtual;
            internal ulong availVirtual;
            internal ulong availExtendedVirtual;
            internal MEMORYSTATUSEX()
            {
                this.length = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class OSVERSIONINFO
        {
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string CSDVersion;
            internal OSVERSIONINFO()
            {
                this.OSVersionInfoSize = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class OSVERSIONINFOEX
        {
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
            public OSVERSIONINFOEX()
            {
                this.OSVersionInfoSize = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct PRIVILEGE_SET
        {
            internal int PrivilegeCount;
            internal int Control;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID_AND_ATTRIBUTES Privilege;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct QUOTA_LIMITS
        {
            internal IntPtr PagedPoolLimit;
            internal IntPtr NonPagedPoolLimit;
            internal IntPtr MinimumWorkingSetSize;
            internal IntPtr MaximumWorkingSetSize;
            internal IntPtr PagefileLimit;
            internal IntPtr TimeLimit;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal unsafe byte* pSecurityDescriptor = null;
            internal int bInheritHandle;
        }

        internal enum SECURITY_IMPERSONATION_LEVEL : short
        {
            Anonymous = 0,
            Delegation = 4,
            Identification = 1,
            Impersonation = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SECURITY_LOGON_SESSION_DATA
        {
            internal int Size;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID LogonId;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING UserName;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING LogonDomain;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.UNICODE_INTPTR_STRING AuthenticationPackage;
            internal int LogonType;
            internal int Session;
            internal IntPtr Sid;
            internal long LogonTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SID_AND_ATTRIBUTES
        {
            internal IntPtr Sid;
            internal int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            internal int dwOemId;
            internal int dwPageSize;
            internal IntPtr lpMinimumApplicationAddress;
            internal IntPtr lpMaximumApplicationAddress;
            internal IntPtr dwActiveProcessorMask;
            internal int dwNumberOfProcessors;
            internal int dwProcessorType;
            internal int dwAllocationGranularity;
            internal short wProcessorLevel;
            internal short wProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_GROUPS
        {
            internal int GroupCount;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.SID_AND_ATTRIBUTES Groups;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_PRIVILEGE
        {
            internal int PrivilegeCount;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID_AND_ATTRIBUTES Privilege;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_SOURCE
        {
            private const int TOKEN_SOURCE_LENGTH = 8;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            internal char[] Name;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID SourceIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_STATISTICS
        {
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID TokenId;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID AuthenticationId;
            internal long ExpirationTime;
            internal int TokenType;
            internal int ImpersonationLevel;
            internal int DynamicCharged;
            internal int DynamicAvailable;
            internal int GroupCount;
            internal int PrivilegeCount;
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.LUID ModifiedId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_USER
        {
            internal Microsoft.PowerShell.Commands.Internal.Win32Native.SID_AND_ATTRIBUTES User;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct UNICODE_INTPTR_STRING
        {
            internal ushort Length;
            internal ushort MaxLength;
            internal IntPtr Buffer;
            internal UNICODE_INTPTR_STRING(int length, int maximumLength, IntPtr buffer)
            {
                this.Length = (ushort) length;
                this.MaxLength = (ushort) maximumLength;
                this.Buffer = buffer;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct UNICODE_STRING
        {
            internal ushort Length;
            internal ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string Buffer;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal int ftCreationTimeLow;
            internal int ftCreationTimeHigh;
            internal int ftLastAccessTimeLow;
            internal int ftLastAccessTimeHigh;
            internal int ftLastWriteTimeLow;
            internal int ftLastWriteTimeHigh;
            internal int fileSizeHigh;
            internal int fileSizeLow;
        }
    }
}

