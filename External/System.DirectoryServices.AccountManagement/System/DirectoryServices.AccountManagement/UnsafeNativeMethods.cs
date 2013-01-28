using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	[SuppressUnmanagedCodeSecurity]
	internal class UnsafeNativeMethods
	{
		public const int CRED_MAX_USERNAME_LENGTH = 0x202;

		public const int CRED_MAX_DOMAIN_TARGET_LENGTH = 0x152;

		public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;

		public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;

		public const int FORMAT_MESSAGE_FROM_STRING = 0x400;

		public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;

		public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

		public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;

		public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;

		private UnsafeNativeMethods()
		{
		}

		[SecurityCritical]
		public static int ADsOpenObject(string path, string userName, string password, int flags, out Guid iid, out object ppObject)
		{
			int num;
			try
			{
				num = UnsafeNativeMethods.IntADsOpenObject(path, userName, password, flags, out iid, out ppObject);
			}
			catch (EntryPointNotFoundException entryPointNotFoundException)
			{
				throw new InvalidOperationException(StringResources.AdsiNotInstalled);
			}
			return num;
		}

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		public static extern bool AuthzFreeContext(IntPtr AuthzClientContext);

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		public static extern bool AuthzFreeResourceManager(IntPtr rm);

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		public static extern bool AuthzGetInformationFromContext(IntPtr hAuthzClientContext, int InfoClass, int BufferSize, out int pSizeRequired, IntPtr Buffer);

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		public static extern bool AuthzInitializeContextFromSid(int Flags, IntPtr UserSid, IntPtr AuthzResourceManager, IntPtr pExpirationTime, UnsafeNativeMethods.LUID Identitifier, IntPtr DynamicGroupArgs, out IntPtr pAuthzClientContext);

		[DllImport("authz.dll", CharSet=CharSet.Unicode)]
		public static extern bool AuthzInitializeResourceManager(int flags, IntPtr pfnAccessCheck, IntPtr pfnComputeDynamicGroups, IntPtr pfnFreeDynamicGroups, string name, out IntPtr rm);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool ConvertSidToStringSid(IntPtr sid, ref string stringSid);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool ConvertStringSidToSid(string stringSid, ref IntPtr sid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool CopySid(int destinationLength, IntPtr pSidDestination, IntPtr pSidSource);

		[DllImport("Credui.dll", CharSet=CharSet.Unicode)]
		public static extern int CredUIParseUserName(string pszUserName, StringBuilder pszUser, uint ulUserMaxChars, StringBuilder pszDomain, uint ulDomainMaxChars);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsGetDcName(string computerName, string domainName, IntPtr domainGuid, string siteName, int flags, out IntPtr domainControllerInfo);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		public static extern int DsRoleFreeMemory(IntPtr buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsRoleGetPrimaryDomainInformation(string lpServer, UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, out IntPtr Buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int DsRoleGetPrimaryDomainInformation(IntPtr lpServer, UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, out IntPtr Buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool EqualDomainSid(IntPtr pSid1, IntPtr pSid2, ref bool equal);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern int FormatMessageW(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern int GetLengthSid(IntPtr sid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidIdentifierAuthority(IntPtr sid);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidSubAuthority(IntPtr sid, int index);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern IntPtr GetSidSubAuthorityCount(IntPtr sid);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass, IntPtr buffer, int bufferSize, ref int returnLength);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int ImpersonateLoggedOnUser(IntPtr hToken);

		[DllImport("activeds.dll", CharSet=CharSet.Unicode)]
		private static extern int IntADsOpenObject(string path, string userName, string password, int flags, out Guid iid, out object ppObject);

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		public static extern bool IsValidSid(IntPtr sid);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern IntPtr LocalFree(IntPtr ptr);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool LookupAccountSid(string computerName, IntPtr sid, StringBuilder name, ref int nameLength, StringBuilder domainName, ref int domainNameLength, ref int usage);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaClose(IntPtr policyHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaFreeMemory(IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaLookupSids(IntPtr policyHandle, int count, IntPtr[] sids, out IntPtr referencedDomains, out IntPtr names);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaOpenPolicy(IntPtr lsaUnicodeString, IntPtr lsaObjectAttributes, int desiredAccess, ref IntPtr policyHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int LsaQueryInformationPolicy(IntPtr policyHandle, int policyInformationClass, ref IntPtr buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		public static extern int NetApiBufferFree(IntPtr buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int NetWkstaGetInfo(string server, int level, ref IntPtr buffer);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool OpenThreadToken(IntPtr threadHandle, int desiredAccess, bool openAsSelf, ref IntPtr tokenHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int RevertToSelf();

		internal enum ADS_OPTION_ENUM
		{
			ADS_OPTION_SERVERNAME,
			ADS_OPTION_REFERRALS,
			ADS_OPTION_PAGE_SIZE,
			ADS_OPTION_SECURITY_MASK,
			ADS_OPTION_MUTUAL_AUTH_STATUS,
			ADS_OPTION_QUOTA,
			ADS_OPTION_PASSWORD_PORTNUMBER,
			ADS_OPTION_PASSWORD_METHOD,
			ADS_OPTION_ACCUMULATIVE_MODIFICATION,
			ADS_OPTION_SKIP_SID_LOOKUP
		}

		internal enum ADS_PASSWORD_ENCODING_ENUM
		{
			ADS_PASSWORD_ENCODE_REQUIRE_SSL,
			ADS_PASSWORD_ENCODE_CLEAR
		}

		[Guid("927971f5-0939-11d1-8be1-00c04fd8d503")]
		[SuppressUnmanagedCodeSecurity]
		public class ADsLargeInteger
		{
			public extern ADsLargeInteger();
		}

		internal sealed class AUTHZ_RM_FLAG
		{
			public static int AUTHZ_RM_FLAG_NO_AUDIT;

			public static int AUTHZ_RM_FLAG_INITIALIZE_UNDER_IMPERSONATION;

			public static int AUTHZ_VALID_RM_INIT_FLAGS;

			static AUTHZ_RM_FLAG()
			{
				UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_RM_FLAG_NO_AUDIT = 1;
				UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_RM_FLAG_INITIALIZE_UNDER_IMPERSONATION = 2;
				UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_VALID_RM_INIT_FLAGS = UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_RM_FLAG_NO_AUDIT | UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_RM_FLAG_INITIALIZE_UNDER_IMPERSONATION;
			}

			private AUTHZ_RM_FLAG()
			{
			}
		}

		public sealed class DomainControllerInfo
		{
			public string DomainControllerName;

			public string DomainControllerAddress;

			public int DomainControllerAddressType;

			public Guid DomainGuid;

			public string DomainName;

			public string DnsForestName;

			public int Flags;

			public string DcSiteName;

			public string ClientSiteName;

			public DomainControllerInfo()
			{
				this.DomainGuid = new Guid();
			}
		}

		public enum DSROLE_MACHINE_ROLE
		{
			DsRole_RoleStandaloneWorkstation,
			DsRole_RoleMemberWorkstation,
			DsRole_RoleStandaloneServer,
			DsRole_RoleMemberServer,
			DsRole_RoleBackupDomainController,
			DsRole_RolePrimaryDomainController,
			DsRole_WorkstationWithSharedAccountDomain,
			DsRole_ServerWithSharedAccountDomain,
			DsRole_MemberWorkstationWithSharedAccountDomain,
			DsRole_MemberServerWithSharedAccountDomain
		}

		public sealed class DSROLE_PRIMARY_DOMAIN_INFO_BASIC
		{
			public UnsafeNativeMethods.DSROLE_MACHINE_ROLE MachineRole;

			public uint Flags;

			public string DomainNameFlat;

			public string DomainNameDns;

			public string DomainForestName;

			public Guid DomainGuid;

			public DSROLE_PRIMARY_DOMAIN_INFO_BASIC()
			{
				this.DomainGuid = new Guid();
			}
		}

		public enum DSROLE_PRIMARY_DOMAIN_INFO_LEVEL
		{
			DsRolePrimaryDomainInfoBasic = 1,
			DsRoleUpgradeStatus = 2,
			DsRoleOperationState = 3,
			DsRolePrimaryDomainInfoBasicEx = 4
		}

		[Guid("FD8256D0-FD15-11CE-ABC4-02608C9E7553")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADs
		{
			string ADsPath
			{
				get;
			}

			string Class
			{
				get;
			}

			string GUID
			{
				get;
			}

			string Name
			{
				get;
			}

			string Parent
			{
				get;
			}

			string Schema
			{
				get;
			}

			object Get(string bstrName);

			object GetEx(string bstrName);

			void GetInfo();

			void GetInfoEx(object vProperties, int lnReserved);

			void Put(string bstrName, object vProp);

			void PutEx(int lnControlCode, string bstrName, object vProp);

			void SetInfo();
		}

		[Guid("7E99C0A2-F935-11D2-BA96-00C04FB6D0D1")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADsDNWithBinary
		{
			object BinaryValue
			{
				get;
				set;
			}

			string DNString
			{
				get;
				set;
			}

		}

		[Guid("27636b00-410f-11cf-b1ff-02608c9e7553")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADsGroup
		{
			string ADsPath
			{
				get;
			}

			string Class
			{
				get;
			}

			string Description
			{
				get;
				set;
			}

			string GUID
			{
				get;
			}

			string Name
			{
				get;
			}

			string Parent
			{
				get;
			}

			string Schema
			{
				get;
			}

			void Add(string bstrNewItem);

			object Get(string bstrName);

			object GetEx(string bstrName);

			void GetInfo();

			void GetInfoEx(object vProperties, int lnReserved);

			bool IsMember(string bstrMember);

			UnsafeNativeMethods.IADsMembers Members();

			void Put(string bstrName, object vProp);

			void PutEx(int lnControlCode, string bstrName, object vProp);

			void Remove(string bstrItemToBeRemoved);

			void SetInfo();
		}

		[Guid("9068270b-0939-11D1-8be1-00c04fd8d503")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADsLargeInteger
		{
			int HighPart
			{
				get;
				set;
			}

			int LowPart
			{
				get;
				set;
			}

		}

		[Guid("451a0030-72ec-11cf-b03b-00aa006e0975")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADsMembers
		{
			object _NewEnum
			{
				get;
			}

			int Count
			{
				get;
			}

			object Filter
			{
				get;
				set;
			}

		}

		[Guid("46f14fda-232b-11d1-a808-00c04fd8d5a8")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IAdsObjectOptions
		{
			object GetOption(int option);

			void PutOption(int option, object vProp);
		}

		[Guid("d592aed4-f420-11d0-a36e-00c04fb950dc")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		public interface IADsPathname
		{
			int EscapedMode
			{
				get;
				set;
			}

			void AddLeafElement(string bstrLeafElement);

			object CopyPath();

			string GetElement(int lnElementIndex);

			string GetEscapedElement(int lnReserved, string bstrInStr);

			int GetNumElements();

			void RemoveLeafElement();

			string Retrieve(int lnFormatType);

			void Set(string bstrADsPath, int lnSetType);

			void SetDisplayType(int lnDisplayType);
		}

		public sealed class LSA_OBJECT_ATTRIBUTES
		{
			public int length;

			public IntPtr rootDirectory;

			public IntPtr objectName;

			public int attributes;

			public IntPtr securityDescriptor;

			public IntPtr securityQualityOfService;

			public LSA_OBJECT_ATTRIBUTES()
			{
				this.rootDirectory = IntPtr.Zero;
				this.objectName = IntPtr.Zero;
				this.securityDescriptor = IntPtr.Zero;
				this.securityQualityOfService = IntPtr.Zero;
			}
		}

		public sealed class LSA_REFERENCED_DOMAIN_LIST
		{
			public int entries;

			public IntPtr domains;

			private LSA_REFERENCED_DOMAIN_LIST()
			{
				this.domains = IntPtr.Zero;
			}
		}

		public sealed class LSA_TRANSLATED_NAME
		{
			public int use;

			public UnsafeNativeMethods.LSA_UNICODE_STRING name;

			public int domainIndex;

			public LSA_TRANSLATED_NAME()
			{
				this.name = new UnsafeNativeMethods.LSA_UNICODE_STRING();
			}
		}

		public sealed class LSA_TRUST_INFORMATION
		{
			public UnsafeNativeMethods.LSA_UNICODE_STRING name;

			private IntPtr pSid;

			public LSA_TRUST_INFORMATION()
			{
				this.name = new UnsafeNativeMethods.LSA_UNICODE_STRING();
				this.pSid = IntPtr.Zero;
			}
		}

		public sealed class LSA_UNICODE_STRING
		{
			public ushort length;

			public ushort maximumLength;

			public IntPtr buffer;

			public LSA_UNICODE_STRING()
			{
				this.buffer = IntPtr.Zero;
			}
		}

		public sealed class LSA_UNICODE_STRING_Managed
		{
			public ushort length;

			public ushort maximumLength;

			public string buffer;

			public LSA_UNICODE_STRING_Managed()
			{
			}
		}

		public struct LUID
		{
			public int low;

			public int high;

		}

		[Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
		[SuppressUnmanagedCodeSecurity]
		public class Pathname
		{
			public extern Pathname();
		}

		public sealed class POLICY_ACCOUNT_DOMAIN_INFO
		{
			public UnsafeNativeMethods.LSA_UNICODE_STRING domainName;

			public IntPtr domainSid;

			public POLICY_ACCOUNT_DOMAIN_INFO()
			{
				this.domainName = new UnsafeNativeMethods.LSA_UNICODE_STRING();
				this.domainSid = IntPtr.Zero;
			}
		}

		public sealed class SID_AND_ATTR
		{
			public IntPtr pSid;

			public int attrs;

			public SID_AND_ATTR()
			{
				this.pSid = IntPtr.Zero;
			}
		}

		public sealed class SID_IDENTIFIER_AUTHORITY
		{
			public byte b1;

			public byte b2;

			public byte b3;

			public byte b4;

			public byte b5;

			public byte b6;

			public SID_IDENTIFIER_AUTHORITY()
			{
			}
		}

		public sealed class TOKEN_GROUPS
		{
			public int groupCount;

			public IntPtr groups;

			public TOKEN_GROUPS()
			{
				this.groups = IntPtr.Zero;
			}
		}

		public sealed class TOKEN_USER
		{
			public UnsafeNativeMethods.SID_AND_ATTR sidAndAttributes;

			public TOKEN_USER()
			{
				this.sidAndAttributes = new UnsafeNativeMethods.SID_AND_ATTR();
			}
		}

		public sealed class WKSTA_INFO_100
		{
			public int wki100_platform_id;

			public string wki100_computername;

			public string wki100_langroup;

			public int wki100_ver_major;

			public int wki100_ver_minor;

			public WKSTA_INFO_100()
			{
			}
		}
	}
}