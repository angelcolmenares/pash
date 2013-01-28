namespace System.Management.Automation.Security
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    internal static class NativeMethods
    {
        internal const int ACL_REVISION = 2;
        internal const int CRYPT_E_NOT_FOUND = -2146885628;
        internal const int E_INVALID_DATA = -2147024883;
        internal const int ERROR_NO_TOKEN = 0x3f0;
        internal const int ERROR_SUCCESS = 0;
        internal const int INHERIT_ONLY_ACE = 8;
        internal const string NCRYPT_WINDOW_HANDLE_PROPERTY = "HWND Handle";
        internal const int NTE_NOT_SUPPORTED = -2146893783;
        internal const int SE_PRIVILEGE_ENABLED = 2;
        internal const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1;
        internal const int SE_PRIVILEGE_REMOVED = 4;
        internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        internal const uint STATUS_INVALID_PARAMETER = 0xc000000d;
        internal const int STATUS_SUCCESS = 0;
        internal const int SUB_CONTAINERS_AND_OBJECTS_INHERIT = 3;
        internal const int SYSTEM_SCOPED_POLICY_ID_ACE_TYPE = 0x13;
        internal const int TOKEN_ADJUST_DEFAULT = 0x80;
        internal const int TOKEN_ADJUST_GROUPS = 0x40;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        internal const int TOKEN_ADJUST_SESSIONID = 0x100;
        internal const int TOKEN_ASSIGN_PRIMARY = 1;
        internal const int TOKEN_DUPLICATE = 2;
        internal const int TOKEN_IMPERSONATE = 4;
        internal const int TOKEN_QUERY = 8;
        internal const int TOKEN_QUERY_SOURCE = 0x10;

        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGE NewState, int BufferLength, ref TOKEN_PRIVILEGE PreviousState, ref uint ReturnLength);
        [DllImport("certca.dll")]
        internal static extern int CCFindCertificateBuildFilter([MarshalAs(UnmanagedType.LPWStr)] string filter, ref IntPtr certFilter);
        [DllImport("certca.dll")]
        internal static extern void CCFindCertificateFreeFilter(IntPtr certFilter);
        [DllImport("certca.dll")]
        internal static extern IntPtr CCFindCertificateFromFilter(IntPtr storeHandle, IntPtr certFilter, IntPtr prevCertContext);
        [DllImport("certca.dll")]
        internal static extern void CCFreeStringArray(IntPtr papwsz);
        [DllImport("certca.dll")]
        internal static extern int CCGetCertNameList(IntPtr certContext, AltNameType dwAltNameChoice, CryptDecodeFlags dwFlags, out int cName, out IntPtr papwszName);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertAddCertificateContextToStore(IntPtr hCertStore, IntPtr pCertContext, int dwAddDisposition, ref IntPtr ppStoreContext);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertCloseStore(IntPtr hCertStore, int dwFlags);
        [DllImport("Crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertControlStore(IntPtr hCertStore, int dwFlags, CertControlStoreType dwCtrlType, IntPtr pvCtrlPara);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertDeleteCertificateFromStore(IntPtr pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CertDuplicateCertificateContext(IntPtr pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CertEnumCertificatesInStore(IntPtr storeHandle, IntPtr certContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertEnumSystemStore(CertStoreFlags Flags, IntPtr notUsed1, IntPtr notUsed2, CertEnumSystemStoreCallBackProto fn);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CertFindCertificateInStore(IntPtr hCertStore, CertOpenStoreEncodingType dwEncodingType, int dwFindFlags, CertFindType dwFindType, [MarshalAs(UnmanagedType.LPWStr)] string pvFindPara, IntPtr notUsed1);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertFreeCertificateContext(IntPtr certContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertGetCertificateContextProperty(IntPtr pCertContext, CertPropertyId dwPropId, IntPtr pvData, ref int pcbData);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertGetEnhancedKeyUsage(IntPtr pCertContext, int dwFlags, IntPtr pUsage, out int pcbUsage);
        [DllImport("Crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CertOpenStore(CertOpenStoreProvider storeProvider, CertOpenStoreEncodingType dwEncodingType, IntPtr notUsed1, CertOpenStoreFlags dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string storeName);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CertSetCertificateContextProperty(IntPtr pCertContext, CertPropertyId dwPropId, int dwFlags, IntPtr pvData);
        [return: MarshalAs(UnmanagedType.Bool)]
        
		/*
		[DllImport("kernel32.dll")]
        internal static extern bool CloseHandle([In] IntPtr hObject);
        */

		internal static bool CloseHandle([In] IntPtr hObject)
		{
			hObject = IntPtr.Zero;
			return true;
		}


        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptAcquireContext(ref IntPtr hProv, string strContainerName, string strProviderName, int nProviderType, int uiProviderFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern string CryptFindLocalizedName(string pwszCryptName);
        [DllImport("crypt32.dll")]
        internal static extern IntPtr CryptFindOIDInfo(int dwKeyType, IntPtr pvKey, int dwGroupId);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern unsafe bool CryptSetProvParam(IntPtr hProv, ProviderParam dwParam, ref void* pbData, int dwFlags);
        [DllImport("cryptUI.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptUIWizDigitalSign(int dwFlags, IntPtr hwndParentNotUsed, IntPtr pwszWizardTitleNotUsed, IntPtr pDigitalSignInfo, IntPtr ppSignContextNotUsed);
        [ArchitectureSensitive]
        internal static uint DestroyWintrustDataStruct(WINTRUST_DATA wtd)
        {
            uint num = 0x80004005;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            Guid structure = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
            try
            {
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                wtd.dwStateAction = 2;
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(wtd));
                Marshal.StructureToPtr(wtd, ptr, false);
                num = WinVerifyTrust(IntPtr.Zero, zero, ptr);
                wtd = (WINTRUST_DATA) Marshal.PtrToStructure(ptr, typeof(WINTRUST_DATA));
            }
            finally
            {
                Marshal.DestroyStructure(ptr, typeof(WINTRUST_DATA));
                Marshal.FreeCoTaskMem(ptr);
                Marshal.DestroyStructure(zero, typeof(Guid));
                Marshal.FreeCoTaskMem(zero);
            }
            if (wtd.dwUnionChoice == 3)
            {
                WINTRUST_BLOB_INFO wintrust_blob_info = (WINTRUST_BLOB_INFO) Marshal.PtrToStructure(wtd.Choice.pBlob, typeof(WINTRUST_BLOB_INFO));
                Marshal.FreeCoTaskMem(wintrust_blob_info.pbMemObject);
                Marshal.DestroyStructure(wtd.Choice.pBlob, typeof(WINTRUST_BLOB_INFO));
                Marshal.FreeCoTaskMem(wtd.Choice.pBlob);
                return num;
            }
            Marshal.DestroyStructure(wtd.Choice.pFile, typeof(WINTRUST_FILE_INFO));
            Marshal.FreeCoTaskMem(wtd.Choice.pFile);
            return num;
        }

        [ArchitectureSensitive]
        internal static int GetCertChoiceFromSigningOption(SigningOption option)
        {
            switch (option)
            {
                case SigningOption.AddOnlyCertificate:
                    return 0;

                case SigningOption.AddFullCertificateChain:
                    return 1;

                case SigningOption.AddFullCertificateChainExceptRoot:
                    return 2;
            }
            return 2;
        }

		/*
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr GetCurrentThread();
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDesktopWindow();
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern uint GetLengthSid(IntPtr pSid);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetNamedSecurityInfo(string pObjectName, SeObjectType ObjectType, SecurityInformation SecurityInfo, out IntPtr ppsidOwner, out IntPtr ppsidGroup, out IntPtr ppDacl, out IntPtr ppSacl, out IntPtr ppSecurityDescriptor);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);
        */

		internal static IntPtr GetConsoleWindow()
		{
			return IntPtr.Zero;
		}
		internal static IntPtr GetCurrentProcess()
		{
			return IntPtr.Zero;
		}
		internal static IntPtr GetCurrentThread()
		{
			return IntPtr.Zero;
		}
		internal static IntPtr GetDesktopWindow()
		{
			return IntPtr.Zero;
		}
		internal static uint GetLengthSid(IntPtr pSid)
		{
			return 16;
		}
		internal static int GetNamedSecurityInfo(string pObjectName, SeObjectType ObjectType, SecurityInformation SecurityInfo, out IntPtr ppsidOwner, out IntPtr ppsidGroup, out IntPtr ppDacl, out IntPtr ppSacl, out IntPtr ppSecurityDescriptor)
		{
			ppsidOwner = IntPtr.Zero;
			ppsidGroup = IntPtr.Zero;
			ppDacl = IntPtr.Zero;
			ppSacl = IntPtr.Zero;
			ppSecurityDescriptor = IntPtr.Zero;
			return 0;
		}
		internal static bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision)
		{
			return true;
		}

		[ArchitectureSensitive]
        internal static CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO InitSignInfoExtendedStruct(string description, string moreInfoUrl, string hashAlgorithm)
        {
            CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO cryptui_wiz_digital_sign_extended_info;
            cryptui_wiz_digital_sign_extended_info = new CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO {
                dwSize = (int) Marshal.SizeOf(typeof(CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO)),
                dwAttrFlagsNotUsed = 0,
                pwszDescription = description,
                pwszMoreInfoLocation = moreInfoUrl,
                pszHashAlg = null,
                pwszSigningCertDisplayStringNotUsed = IntPtr.Zero,
                hAdditionalCertStoreNotUsed = IntPtr.Zero,
                psAuthenticatedNotUsed = IntPtr.Zero,
                psUnauthenticatedNotUsed = IntPtr.Zero
            };
            if (hashAlgorithm != null)
            {
                cryptui_wiz_digital_sign_extended_info.pszHashAlg = hashAlgorithm;
            }
            return cryptui_wiz_digital_sign_extended_info;
        }

        [ArchitectureSensitive]
        internal static CRYPTUI_WIZ_DIGITAL_SIGN_INFO InitSignInfoStruct(string fileName, X509Certificate2 signingCert, string timeStampServerUrl, string hashAlgorithm, SigningOption option)
        {
            CRYPTUI_WIZ_DIGITAL_SIGN_INFO cryptui_wiz_digital_sign_info;
            cryptui_wiz_digital_sign_info = new CRYPTUI_WIZ_DIGITAL_SIGN_INFO {
                dwSize = (int) Marshal.SizeOf(typeof(CRYPTUI_WIZ_DIGITAL_SIGN_INFO)),
                dwSubjectChoice = 1,
                pwszFileName = fileName,
                dwSigningCertChoice = 1,
                pSigningCertContext = signingCert.Handle,
                pwszTimestampURL = timeStampServerUrl,
                dwAdditionalCertChoice = GetCertChoiceFromSigningOption(option)
            };
            CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO structure = InitSignInfoExtendedStruct("", "", hashAlgorithm);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            cryptui_wiz_digital_sign_info.pSignExtInfo = ptr;
            return cryptui_wiz_digital_sign_info;
        }

        [ArchitectureSensitive]
        internal static WINTRUST_BLOB_INFO InitWintrustBlobInfoStruct(string fileName, string content)
        {
            WINTRUST_BLOB_INFO structure = new WINTRUST_BLOB_INFO();
            byte[] bytes = Encoding.Unicode.GetBytes(content);
            structure.gSubject.Data1 = 0x603bcc1f;
            structure.gSubject.Data2 = 0x4b59;
            structure.gSubject.Data3 = 0x4e08;
            structure.gSubject.Data4 = new byte[] { 0xb7, 0x24, 210, 0xc6, 0x29, 0x7e, 0xf3, 0x51 };
            structure.cbStruct = (int) Marshal.SizeOf(structure);
            structure.pcwszDisplayName = fileName;
            structure.cbMemObject = (int) bytes.Length;
            structure.pbMemObject = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, structure.pbMemObject, bytes.Length);
            return structure;
        }

        [ArchitectureSensitive]
        internal static WINTRUST_DATA InitWintrustDataStructFromBlob(WINTRUST_BLOB_INFO wbi)
        {
            WINTRUST_DATA wintrust_data = new WINTRUST_DATA {
                cbStruct = (int) Marshal.SizeOf(wbi),
                pPolicyCallbackData = IntPtr.Zero,
                pSIPClientData = IntPtr.Zero,
                dwUIChoice = 2,
                fdwRevocationChecks = 0,
                dwUnionChoice = 3
            };
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(wbi));
            Marshal.StructureToPtr(wbi, ptr, false);
            wintrust_data.Choice.pBlob = ptr;
            wintrust_data.dwStateAction = 1;
            wintrust_data.hWVTStateData = IntPtr.Zero;
            wintrust_data.pwszURLReference = null;
            wintrust_data.dwProvFlags = 0;
            return wintrust_data;
        }

        [ArchitectureSensitive]
        internal static WINTRUST_DATA InitWintrustDataStructFromFile(WINTRUST_FILE_INFO wfi)
        {
            WINTRUST_DATA wintrust_data;
            wintrust_data = new WINTRUST_DATA {
                cbStruct = (int)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)),
                pPolicyCallbackData = IntPtr.Zero,
                pSIPClientData = IntPtr.Zero,
                dwUIChoice = 2,
                fdwRevocationChecks = 0,
                dwUnionChoice = 1
            };
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(wfi));
            Marshal.StructureToPtr(wfi, ptr, false);
            wintrust_data.Choice.pFile = ptr;
            wintrust_data.dwStateAction = 1;
            wintrust_data.hWVTStateData = IntPtr.Zero;
            wintrust_data.pwszURLReference = null;
            wintrust_data.dwProvFlags = 0;
            return wintrust_data;
        }

        [ArchitectureSensitive]
        internal static WINTRUST_FILE_INFO InitWintrustFileInfoStruct(string fileName)
        {
            return new WINTRUST_FILE_INFO { cbStruct = (int) Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)), pcwszFilePath = fileName, hFileNotUsed = IntPtr.Zero, pgKnownSubjectNotUsed = IntPtr.Zero };
        }

		/*
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool IsValidSid(IntPtr pSid);
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr hMem);
        [DllImport("certenroll.dll")]
        internal static extern int LogCertCopy(bool fMachine, IntPtr pCertContext);
        [DllImport("certenroll.dll")]
        internal static extern int LogCertDelete(bool fMachine, IntPtr pCertContext);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int LsaFreeMemory(IntPtr Buffer);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int LsaQueryCAPs(IntPtr[] CAPIDs, int CAPIDCount, out IntPtr CAPs, out int CAPCount);
        [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
        internal static extern int NCryptDeleteKey(IntPtr hKey, int dwFlags);
        [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
        internal static extern int NCryptFreeObject(IntPtr hObject);
        [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
        internal static extern int NCryptOpenKey(IntPtr hProv, ref IntPtr hKey, string strKeyName, int dwLegacySpec, int dwFlags);
        [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
        internal static extern int NCryptOpenStorageProvider(ref IntPtr hProv, string strProviderName, int dwFlags);
        [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
        internal static extern unsafe int NCryptSetProperty(IntPtr hProv, string pszProperty, ref void* pbInput, int cbInput, int dwFlags);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool OpenThreadToken(IntPtr ThreadHandle, int DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);
        [DllImport("kernel32.dll")]
        internal static extern bool ProcessIdToSessionId(int dwProcessId, out uint pSessionId);
        [DllImport("ntdll.dll", CharSet=CharSet.Unicode)]
        internal static extern int RtlAddScopedPolicyIDAce(IntPtr Acl, int AceRevision, int AceFlags, int AccessMask, IntPtr Sid);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        internal static extern bool SaferCloseLevel([In] IntPtr hLevelHandle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool SaferComputeTokenFromLevel([In] IntPtr LevelHandle, [In] IntPtr InAccessToken, ref IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool SaferIdentifyLevel(int dwNumProperties, [In] ref SAFER_CODE_PROPERTIES pCodeProperties, out IntPtr pLevelHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string bucket);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int SetNamedSecurityInfo(string pObjectName, SeObjectType ObjectType, SecurityInformation SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);
        [DllImport("wintrust.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern uint WinVerifyTrust(IntPtr hWndNotUsed, IntPtr pgActionID, IntPtr pWinTrustData);
        [DllImport("wintrust.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr WTHelperGetProvCertFromChain(IntPtr pSgnr, int idxCert);
        [DllImport("wintrust.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr WTHelperGetProvSignerFromChain(IntPtr pProvData, int idxSigner, int fCounterSigner, int idxCounterSigner);
        [DllImport("wintrust.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr WTHelperProvDataFromStateData(IntPtr hStateData);


		*/

		
		internal static bool IsValidSid(IntPtr pSid) {
			return true;
		}
		internal static IntPtr LocalFree(IntPtr hMem) {
			return IntPtr.Zero;
		}
		internal static int LogCertCopy(bool fMachine, IntPtr pCertContext) {
			return 0;
		}
		internal static int LogCertDelete(bool fMachine, IntPtr pCertContext) {
			return 0;
		}
		internal static bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid) {
			return true;
		}
		internal static int LsaFreeMemory(IntPtr Buffer) {
			Buffer = IntPtr.Zero;
			return 0;
		}
		internal static int LsaQueryCAPs(IntPtr[] CAPIDs, int CAPIDCount, out IntPtr CAPs, out int CAPCount) {
			CAPs = IntPtr.Zero;
			CAPCount = 0;
			return 0;
		}
		internal static int NCryptDeleteKey(IntPtr hKey, int dwFlags) {
			return 0;
		}
		internal static int NCryptFreeObject(IntPtr hObject) {
			hObject = IntPtr.Zero;
			return 0;
		}
		internal static int NCryptOpenKey(IntPtr hProv, ref IntPtr hKey, string strKeyName, int dwLegacySpec, int dwFlags) {
			return 0;
		}
		internal static int NCryptOpenStorageProvider(ref IntPtr hProv, string strProviderName, int dwFlags) {
			return 0;
		}
		internal static unsafe int NCryptSetProperty(IntPtr hProv, string pszProperty, ref void* pbInput, int cbInput, int dwFlags) {
			return 0;
		}
		internal static bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle) {
			TokenHandle = IntPtr.Zero;
			return true;
		}
		internal static bool OpenThreadToken(IntPtr ThreadHandle, int DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle) {
			TokenHandle = IntPtr.Zero;
			return true;
		}
		internal static bool ProcessIdToSessionId (int dwProcessId, out uint pSessionId)
		{
			var process = System.Diagnostics.Process.GetProcessById (dwProcessId);
			if (process != null) {
				pSessionId = 0; //TODO: NOT IMPLEMENTED Convert.ToUInt32 (process.SessionId);
			} else {
				pSessionId = 0;
			}
			return true;
		}
		internal static int RtlAddScopedPolicyIDAce(IntPtr Acl, int AceRevision, int AceFlags, int AccessMask, IntPtr Sid) {
			return 0;
		}
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static bool SaferCloseLevel([In] IntPtr hLevelHandle) {
			return true;
		}
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static bool SaferComputeTokenFromLevel([In] IntPtr LevelHandle, [In] IntPtr InAccessToken, ref IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved) {
			OutAccessToken = IntPtr.Zero;
			return true;
		}
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static bool SaferIdentifyLevel(int dwNumProperties, [In] ref SAFER_CODE_PROPERTIES pCodeProperties, out IntPtr pLevelHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string bucket) {
			var path = pCodeProperties.ImagePath;
			pLevelHandle = IntPtr.Zero;
			var exePath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly ().Location).Directory.FullName;
			if (path.IndexOf (exePath, StringComparison.OrdinalIgnoreCase) == -1) return false;
			return true;
		}
		internal static int SetNamedSecurityInfo(string pObjectName, SeObjectType ObjectType, SecurityInformation SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl) {
			return 0;
		}
		internal static uint WinVerifyTrust(IntPtr hWndNotUsed, IntPtr pgActionID, IntPtr pWinTrustData) {
			return 0;
		}
		internal static IntPtr WTHelperGetProvCertFromChain(IntPtr pSgnr, int idxCert) {
			return pSgnr;
		}
		internal static IntPtr WTHelperGetProvSignerFromChain(IntPtr pProvData, int idxSigner, int fCounterSigner, int idxCounterSigner) {
			return IntPtr.Zero;
		}
		internal static IntPtr WTHelperProvDataFromStateData(IntPtr hStateData) {
			return IntPtr.Zero;
		}

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct ACE_HEADER
        {
            internal byte AceType;
            internal byte AceFlags;
            internal ushort AceSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct ACL
        {
            internal byte AclRevision;
            internal byte Sbz1;
            internal ushort AclSize;
            internal ushort AceCount;
            internal ushort Sbz2;
        }

        [Flags]
        internal enum AddCertificateContext : int
        {
            CERT_STORE_ADD_ALWAYS = 4,
            CERT_STORE_ADD_NEW = 1,
            CERT_STORE_ADD_NEWER = 6,
            CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES = 7,
            CERT_STORE_ADD_REPLACE_EXISTING = 3,
            CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES = 5,
            CERT_STORE_ADD_USE_EXISTING = 2
        }

        internal enum AltNameType : int
        {
            CERT_ALT_NAME_DIRECTORY_NAME = 5,
            CERT_ALT_NAME_DNS_NAME = 3,
            CERT_ALT_NAME_EDI_PARTY_NAME = 6,
            CERT_ALT_NAME_IP_ADDRESS = 8,
            CERT_ALT_NAME_OTHER_NAME = 1,
            CERT_ALT_NAME_REGISTERED_ID = 9,
            CERT_ALT_NAME_RFC822_NAME = 2,
            CERT_ALT_NAME_URL = 7,
            CERT_ALT_NAME_X400_ADDRESS = 4
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Anonymous_a3ae7823_8a1d_432c_bc07_a72b6fc6c7d8
        {
            [FieldOffset(0)]
            public int Algid;
            [FieldOffset(0)]
            public int dwLength;
            [FieldOffset(0)]
            public int dwValue;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CENTRAL_ACCESS_POLICY
        {
            internal IntPtr CAPID;
            internal System.Management.Automation.Security.NativeMethods.LSA_UNICODE_STRING Name;
            internal System.Management.Automation.Security.NativeMethods.LSA_UNICODE_STRING Description;
            internal System.Management.Automation.Security.NativeMethods.LSA_UNICODE_STRING ChangeId;
            internal int Flags;
            internal int CAPECount;
            internal IntPtr CAPEs;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_CONTEXT
        {
            public int dwCertEncodingType;
            public IntPtr pbCertEncoded;
            public int cbCertEncoded;
            public IntPtr pCertInfo;
            public IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_ENHKEY_USAGE
        {
            internal int cUsageIdentifier;
            internal IntPtr rgpszUsageIdentifier;
        }

        [Flags]
        internal enum CertControlStoreType : int
        {
            CERT_STORE_CTRL_AUTO_RESYNC = 4,
            CERT_STORE_CTRL_COMMIT = 3,
            CERT_STORE_CTRL_RESYNC = 1
        }

        internal delegate bool CertEnumSystemStoreCallBackProto([MarshalAs(UnmanagedType.LPWStr)] string storeName, uint dwFlagsNotUsed, IntPtr notUsed1, IntPtr notUsed2, IntPtr notUsed3);

        [Flags]
        internal enum CertFindType
        {
            CERT_COMPARE_ANY = 0,
            CERT_FIND_CROSS_CERT_DIST_POINTS = 0x110000,
            CERT_FIND_HASH_STR = 0x140000,
            CERT_FIND_ISSUER_STR = 0x80004,
            CERT_FIND_SUBJECT_INFO_ACCESS = 0x130000,
            CERT_FIND_SUBJECT_STR = 0x80007
        }

        [Flags]
        internal enum CertOpenStoreEncodingType
        {
            X509_ASN_ENCODING = 1
        }

        [Flags]
        internal enum CertOpenStoreFlags
        {
            CERT_STORE_BACKUP_RESTORE_FLAG = 0x800,
            CERT_STORE_CREATE_NEW_FLAG = 0x2000,
            CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 4,
            CERT_STORE_DELETE_FLAG = 0x10,
            CERT_STORE_ENUM_ARCHIVED_FLAG = 0x200,
            CERT_STORE_MANIFOLD_FLAG = 0x100,
            CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x1000,
            CERT_STORE_NO_CRYPT_RELEASE_FLAG = 1,
            CERT_STORE_OPEN_EXISTING_FLAG = 0x4000,
            CERT_STORE_READONLY_FLAG = 0x8000,
            CERT_STORE_SET_LOCALIZED_NAME_FLAG = 2,
            CERT_STORE_SHARE_CONTEXT_FLAG = 0x80,
            CERT_STORE_SHARE_STORE_FLAG = 0x40,
            CERT_STORE_UNSAFE_PHYSICAL_FLAG = 0x20,
            CERT_STORE_UPDATE_KEYID_FLAG = 0x400,
            CERT_SYSTEM_STORE_CURRENT_SERVICE = 0x40000,
            CERT_SYSTEM_STORE_CURRENT_USER = 0x10000,
            CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = 0x70000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE = 0x20000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = 0x90000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = 0x80000,
            CERT_SYSTEM_STORE_SERVICES = 0x50000,
            CERT_SYSTEM_STORE_USERS = 0x60000
        }

        [Flags]
        internal enum CertOpenStoreProvider
        {
            CERT_STORE_PROV_MEMORY = 2,
            CERT_STORE_PROV_SYSTEM = 10,
            CERT_STORE_PROV_SYSTEM_REGISTRY = 13
        }

        [Flags]
        internal enum CertPropertyId
        {
            CERT_KEY_PROV_HANDLE_PROP_ID = 1,
            CERT_KEY_PROV_INFO_PROP_ID = 2,
            CERT_MD5_HASH_PROP_ID = 4,
            CERT_SEND_AS_TRUSTED_ISSUER_PROP_ID = 0x66,
            CERT_SHA1_HASH_PROP_ID = 3
        }

        [Flags]
        internal enum CertStoreFlags
        {
            CERT_SYSTEM_STORE_CURRENT_SERVICE = 0x40000,
            CERT_SYSTEM_STORE_CURRENT_USER = 0x10000,
            CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = 0x70000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE = 0x20000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = 0x90000,
            CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = 0x80000,
            CERT_SYSTEM_STORE_SERVICES = 0x50000,
            CERT_SYSTEM_STORE_USERS = 0x60000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_ATTR_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_KEY_PROV_INFO
        {
            public string pwszContainerName;
            public string pwszProvName;
            public System.Management.Automation.Security.NativeMethods.PROV dwProvType;
            public int dwFlags;
            public int cProvParam;
            public IntPtr rgProvParam;
            public int dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_OID_INFO
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pszOID;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;
            public int dwGroupId;
            public System.Management.Automation.Security.NativeMethods.Anonymous_a3ae7823_8a1d_432c_bc07_a72b6fc6c7d8 Union1;
            public System.Management.Automation.Security.NativeMethods.CRYPT_ATTR_BLOB ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_PROVIDER_CERT
        {
            private int cbStruct;
            internal IntPtr pCert;
            private int fCommercial;
            private int fTrustedRoot;
            private int fSelfSigned;
            private int fTestCert;
            private int dwRevokedReason;
            private int dwConfidence;
            private int dwError;
            private IntPtr pTrustListContext;
            private int fTrustListSignerCert;
            private IntPtr pCtlContext;
            private int dwCtlError;
            private int fIsCyclic;
            private IntPtr pChainElement;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_PROVIDER_SGNR
        {
            private int cbStruct;
            private System.Runtime.InteropServices.ComTypes.FILETIME sftVerifyAsOf;
            private int csCertChain;
            private IntPtr pasCertChain;
            private int dwSignerType;
            private IntPtr psSigner;
            private int dwError;
            internal int csCounterSigners;
            internal IntPtr pasCounterSigners;
            private IntPtr pChainContext;
        }

        internal enum CryptDecodeFlags : int
        {
            CRYPT_DECODE_ENABLE_IA5CONVERSION_FLAG = 0x6000000,
            CRYPT_DECODE_ENABLE_PUNYCODE_FLAG = 0x2000000,
            CRYPT_DECODE_ENABLE_UTF8PERCENT_FLAG = 0x4000000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO
        {
            internal int dwSize;
            internal int dwAttrFlagsNotUsed;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszDescription;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszMoreInfoLocation;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszHashAlg;
            internal IntPtr pwszSigningCertDisplayStringNotUsed;
            internal IntPtr hAdditionalCertStoreNotUsed;
            internal IntPtr psAuthenticatedNotUsed;
            internal IntPtr psUnauthenticatedNotUsed;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTUI_WIZ_DIGITAL_SIGN_INFO
        {
            internal int dwSize;
            internal int dwSubjectChoice;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszFileName;
            internal int dwSigningCertChoice;
            internal IntPtr pSigningCertContext;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszTimestampURL;
            internal int dwAdditionalCertChoice;
            internal IntPtr pSignExtInfo;
        }

        [Flags]
        internal enum CryptUIFlags
        {
            CRYPTUI_WIZ_NO_UI = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct GUID
        {
            internal int Data1;
            internal ushort Data2;
            internal ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            internal byte[] Data4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LSA_UNICODE_STRING
        {
            internal ushort Length;
            internal ushort MaximumLength;
            internal IntPtr Buffer;
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
            internal System.Management.Automation.Security.NativeMethods.LUID Luid;
            internal int Attributes;
        }

        [Flags]
        internal enum NCryptDeletKeyFlag
        {
            NCRYPT_MACHINE_KEY_FLAG = 0x20,
            NCRYPT_SILENT_FLAG = 0x40
        }

        internal enum PROV : int
        {
            DH_SCHANNEL = 0x12,
            DSS = 3,
            DSS_DH = 13,
            EC_ECDSA_FULL = 0x10,
            EC_ECDSA_SIG = 14,
            EC_ECNRA_FULL = 0x11,
            EC_ECNRA_SIG = 15,
            FORTEZZA = 4,
            INTEL_SEC = 0x16,
            MS_EXCHANGE = 5,
            RNG = 0x15,
            RSA_FULL = 1,
            RSA_SCHANNEL = 12,
            RSA_SIG = 2,
            SPYRUS_LYNKS = 20,
            SSL = 6
        }

        [Flags]
        internal enum ProviderFlagsEnum : uint
        {
            CRYPT_DELETEKEYSET = 0x10,
            CRYPT_MACHINE_KEYSET = 0x20,
            CRYPT_NEWKEYSET = 8,
            CRYPT_SILENT = 0x40,
            CRYPT_VERIFYCONTEXT = 0xf0000000
        }

        internal enum ProviderParam
        {
            PP_CLIENT_HWND = 1
        }

        internal enum SecurityInformation : uint
        {
            ATTRIBUTE_SECURITY_INFORMATION = 0x20,
            BACKUP_SECURITY_INFORMATION = 0x10000,
            DACL_SECURITY_INFORMATION = 4,
            GROUP_SECURITY_INFORMATION = 2,
            LABEL_SECURITY_INFORMATION = 0x10,
            OWNER_SECURITY_INFORMATION = 1,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            SACL_SECURITY_INFORMATION = 8,
            SCOPE_SECURITY_INFORMATION = 0x40,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000
        }

        internal enum SeObjectType : int
        {
            SE_DS_OBJECT = 8,
            SE_DS_OBJECT_ALL = 9,
            SE_FILE_OBJECT = 1,
            SE_KERNEL_OBJECT = 6,
            SE_LMSHARE = 5,
            SE_PRINTER = 3,
            SE_PROVIDER_DEFINED_OBJECT = 10,
            SE_REGISTRY_KEY = 4,
            SE_REGISTRY_WOW64_32KEY = 12,
            SE_SERVICE = 2,
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_WINDOW_OBJECT = 7,
            SE_WMIGUID_OBJECT = 11
        }

        [Flags]
        internal enum SignInfoAdditionalCertChoice
        {
            CRYPTUI_WIZ_DIGITAL_SIGN_ADD_CHAIN = 1,
            CRYPTUI_WIZ_DIGITAL_SIGN_ADD_CHAIN_NO_ROOT = 2
        }

        [Flags]
        internal enum SignInfoCertChoice
        {
            CRYPTUI_WIZ_DIGITAL_SIGN_CERT = 1
        }

        [Flags]
        internal enum SignInfoSubjectChoice
        {
            CRYPTUI_WIZ_DIGITAL_SIGN_SUBJECT_FILE = 1
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SYSTEM_AUDIT_ACE
        {
            internal System.Management.Automation.Security.NativeMethods.ACE_HEADER Header;
            internal int Mask;
            internal int SidStart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_PRIVILEGE
        {
            internal int PrivilegeCount;
            internal System.Management.Automation.Security.NativeMethods.LUID_AND_ATTRIBUTES Privilege;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINTRUST_BLOB_INFO
        {
            internal int cbStruct;
            internal System.Management.Automation.Security.NativeMethods.GUID gSubject;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pcwszDisplayName;
            internal int cbMemObject;
            internal IntPtr pbMemObject;
            internal int cbMemSignedMsg;
            internal IntPtr pbMemSignedMsg;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct WinTrust_Choice
        {
            [FieldOffset(0)]
            internal IntPtr pBlob;
            [FieldOffset(0)]
            internal IntPtr pCatalog;
            [FieldOffset(0)]
            internal IntPtr pCert;
            [FieldOffset(0)]
            internal IntPtr pFile;
            [FieldOffset(0)]
            internal IntPtr pSgnr;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINTRUST_DATA
        {
            internal int cbStruct;
            internal IntPtr pPolicyCallbackData;
            internal IntPtr pSIPClientData;
            internal int dwUIChoice;
            internal int fdwRevocationChecks;
            internal int dwUnionChoice;
            internal System.Management.Automation.Security.NativeMethods.WinTrust_Choice Choice;
            internal int dwStateAction;
            internal IntPtr hWVTStateData;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszURLReference;
            internal int dwProvFlags;
            internal int dwUIContext;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINTRUST_FILE_INFO
        {
            internal int cbStruct;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pcwszFilePath;
            internal IntPtr hFileNotUsed;
            internal IntPtr pgKnownSubjectNotUsed;
        }

        [Flags]
        internal enum WintrustAction
        {
            WTD_STATEACTION_IGNORE,
            WTD_STATEACTION_VERIFY,
            WTD_STATEACTION_CLOSE,
            WTD_STATEACTION_AUTO_CACHE,
            WTD_STATEACTION_AUTO_CACHE_FLUSH
        }

        [Flags]
        internal enum WintrustProviderFlags
        {
            WTD_CACHE_ONLY_URL_RETRIEVAL = 0x1000,
            WTD_HASH_ONLY_FLAG = 0x200,
            WTD_LIFETIME_SIGNING_FLAG = 0x800,
            WTD_NO_IE4_CHAIN_FLAG = 2,
            WTD_NO_POLICY_USAGE_FLAG = 4,
            WTD_PROV_FLAGS_MASK = 0xffff,
            WTD_REVOCATION_CHECK_CHAIN = 0x40,
            WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x80,
            WTD_REVOCATION_CHECK_END_CERT = 0x20,
            WTD_REVOCATION_CHECK_NONE = 0x10,
            WTD_SAFER_FLAG = 0x100,
            WTD_USE_DEFAULT_OSVER_CHECK = 0x400,
            WTD_USE_IE4_TRUST_FLAG = 1
        }

        [Flags]
        internal enum WintrustUIChoice
        {
            WTD_UI_ALL = 1,
            WTD_UI_NOBAD = 3,
            WTD_UI_NOGOOD = 4,
            WTD_UI_NONE = 2
        }

        [Flags]
        internal enum WintrustUnionChoice
        {
            WTD_CHOICE_BLOB = 3,
            WTD_CHOICE_FILE = 1
        }
    }
}

