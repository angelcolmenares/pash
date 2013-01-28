namespace System.Management.Automation.Internal
{
    using System;
    using System.Runtime.InteropServices;

    internal class PSCryptoNativeUtils
    {
        public const int ALG_CLASS_DATA_ENCRYPT = 0x6000;
        public const int ALG_CLASS_KEY_EXCHANGE = 0xa000;
        public const int ALG_SID_AES_128 = 14;
        public const int ALG_SID_AES_256 = 0x10;
        public const int ALG_SID_RSA_ANY = 0;
        public const int ALG_TYPE_BLOCK = 0x600;
        public const int ALG_TYPE_RSA = 0x400;
        public const int AT_KEYEXCHANGE = 1;
        public const int CALG_AES_128 = 0x660e;
        public const int CALG_AES_256 = 0x6610;
        public const int CALG_RSA_KEYX = 0xa400;
        public const int CRYPT_CREATE_SALT = 4;
        public const int CRYPT_EXPORTABLE = 1;
        public const uint CRYPT_VERIFYCONTEXT = 0xf0000000;
        public const int PROV_RSA_AES = 0x18;
        public const int PROV_RSA_FULL = 1;
        public const int PUBLICKEYBLOB = 6;
        public const int SIMPLEBLOB = 1;

		/*
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptAcquireContext(ref PSSafeCryptProvHandle phProv, [In, MarshalAs(UnmanagedType.LPWStr)] string szContainer, [In, MarshalAs(UnmanagedType.LPWStr)] string szProvider, int dwProvType, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptDecrypt(PSSafeCryptKey hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, byte[] pbData, ref int pdwDataLen);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptDestroyKey(IntPtr hKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptDuplicateKey(PSSafeCryptKey hKey, ref int pdwReserved, int dwFlags, ref PSSafeCryptKey phKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptEncrypt(PSSafeCryptKey hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, byte[] pbData, ref int pdwDataLen, int dwBufLen);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptExportKey(PSSafeCryptKey hKey, PSSafeCryptKey hExpKey, int dwBlobType, int dwFlags, byte[] pbData, ref int pdwDataLen);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptGenKey(PSSafeCryptProvHandle hProv, int Algid, int dwFlags, ref PSSafeCryptKey phKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptImportKey(PSSafeCryptProvHandle hProv, byte[] pbData, int dwDataLen, PSSafeCryptKey hPubKey, int dwFlags, ref PSSafeCryptKey phKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll")]
        public static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

		*/


		public static bool CryptAcquireContext(ref PSSafeCryptProvHandle phProv, [In, MarshalAs(UnmanagedType.LPWStr)] string szContainer, [In, MarshalAs(UnmanagedType.LPWStr)] string szProvider, int dwProvType, int dwFlags)
		{
			return true;
		}

		public static bool CryptDecrypt(PSSafeCryptKey hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, byte[] pbData, ref int pdwDataLen)
		{
			return true;
		}

		public static bool CryptDestroyKey(IntPtr hKey)
		{
			return true;
		}

		public static bool CryptDuplicateKey(PSSafeCryptKey hKey, ref int pdwReserved, int dwFlags, ref PSSafeCryptKey phKey)
		{
			return true;
		}

		public static bool CryptEncrypt(PSSafeCryptKey hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, byte[] pbData, ref int pdwDataLen, int dwBufLen)
		{
			pdwDataLen = pbData.Length;
			return true;
		}

		public static bool CryptExportKey(PSSafeCryptKey hKey, PSSafeCryptKey hExpKey, int dwBlobType, int dwFlags, byte[] pbData, ref int pdwDataLen)
		{
			pdwDataLen = pbData.Length;
			return true;
		}

		public static bool CryptGenKey(PSSafeCryptProvHandle hProv, int Algid, int dwFlags, ref PSSafeCryptKey phKey)
		{
			return true;
		}

		public static bool CryptImportKey(PSSafeCryptProvHandle hProv, byte[] pbData, int dwDataLen, PSSafeCryptKey hPubKey, int dwFlags, ref PSSafeCryptKey phKey)
		{
			return true;
		}

		public static bool CryptReleaseContext(IntPtr hProv, int dwFlags)
		{
			return true;
		}

		public static int GetLastError()
		{
			return 0;
		}

    }
}

