namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Security;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    internal static class SignatureHelper
    {
        [TraceSource("SignatureHelper", "tracer for SignatureHelper")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("SignatureHelper", "tracer for SignatureHelper");

        [ArchitectureSensitive]
        private static X509Certificate2 GetCertFromChain(IntPtr pSigner)
        {
            X509Certificate2 certificate = null;
            IntPtr ptr = System.Management.Automation.Security.NativeMethods.WTHelperGetProvCertFromChain(pSigner, 0);
            if (ptr != IntPtr.Zero)
            {
                System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_CERT crypt_provider_cert = (System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_CERT) Marshal.PtrToStructure(ptr, typeof(System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_CERT));
                certificate = new X509Certificate2(crypt_provider_cert.pCert);
            }
            return certificate;
        }

        [ArchitectureSensitive]
        private static uint GetLastWin32Error()
        {
            return SecuritySupport.GetDWORDFromInt(Marshal.GetLastWin32Error());
        }

        [ArchitectureSensitive]
        internal static System.Management.Automation.Signature GetSignature(string fileName, string fileContent)
        {
            System.Management.Automation.Signature signature = null;
            uint error = 0x80004005;
            Utils.CheckArgForNullOrEmpty(fileName, "fileName");
            SecuritySupport.CheckIfFileExists(fileName);
            try
            {
                System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wintrust_data;
                error = GetWinTrustData(fileName, fileContent, out wintrust_data);
                if (error != 0)
                {
                    tracer.WriteLine("GetWinTrustData failed: {0:x}", new object[] { error });
                }
                signature = GetSignatureFromWintrustData(fileName, error, wintrust_data);
                error = System.Management.Automation.Security.NativeMethods.DestroyWintrustDataStruct(wintrust_data);
                if (error != 0)
                {
                    tracer.WriteLine("DestroyWinTrustDataStruct failed: {0:x}", new object[] { error });
                }
            }
            catch (AccessViolationException)
            {
                signature = new System.Management.Automation.Signature(fileName, 0x800b0100);
            }
            return signature;
        }

        [ArchitectureSensitive]
        private static System.Management.Automation.Signature GetSignatureFromWintrustData(string filePath, uint error, System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wtd)
        {
            System.Management.Automation.Signature signature = null;
            X509Certificate2 signer = null;
            X509Certificate2 timestamper = null;
            tracer.WriteLine("GetSignatureFromWintrustData: error: {0}", new object[] { error });
            IntPtr pProvData = System.Management.Automation.Security.NativeMethods.WTHelperProvDataFromStateData(wtd.hWVTStateData);
            if (pProvData != IntPtr.Zero)
            {
                IntPtr pSigner = System.Management.Automation.Security.NativeMethods.WTHelperGetProvSignerFromChain(pProvData, 0, 0, 0);
                if (pSigner != IntPtr.Zero)
                {
                    signer = GetCertFromChain(pSigner);
                    if (signer != null)
                    {
                        System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR crypt_provider_sgnr = (System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR) Marshal.PtrToStructure(pSigner, typeof(System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR));
                        if (crypt_provider_sgnr.csCounterSigners == 1)
                        {
                            timestamper = GetCertFromChain(crypt_provider_sgnr.pasCounterSigners);
                        }
                        if (timestamper != null)
                        {
                            signature = new System.Management.Automation.Signature(filePath, error, signer, timestamper);
                        }
                        else
                        {
                            signature = new System.Management.Automation.Signature(filePath, error, signer);
                        }
                    }
                }
            }
            if ((signature == null) && (error != 0))
            {
                signature = new System.Management.Automation.Signature(filePath, error);
            }
            return signature;
        }

        [ArchitectureSensitive]
        private static uint GetWinTrustData(string fileName, string fileContent, out System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wtData)
        {
            uint num = 0x80004005;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            Guid structure = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
            try
            {
                System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wintrust_data;
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                if (fileContent == null)
                {
                    wintrust_data = System.Management.Automation.Security.NativeMethods.InitWintrustDataStructFromFile(System.Management.Automation.Security.NativeMethods.InitWintrustFileInfoStruct(fileName));
                }
                else
                {
                    wintrust_data = System.Management.Automation.Security.NativeMethods.InitWintrustDataStructFromBlob(System.Management.Automation.Security.NativeMethods.InitWintrustBlobInfoStruct(fileName, fileContent));
                }
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(wintrust_data));
                Marshal.StructureToPtr(wintrust_data, ptr, false);
                num = System.Management.Automation.Security.NativeMethods.WinVerifyTrust(IntPtr.Zero, zero, ptr);
                wtData = (System.Management.Automation.Security.NativeMethods.WINTRUST_DATA) Marshal.PtrToStructure(ptr, typeof(System.Management.Automation.Security.NativeMethods.WINTRUST_DATA));
            }
            finally
            {
                Marshal.DestroyStructure(zero, typeof(Guid));
                Marshal.FreeCoTaskMem(zero);
                Marshal.DestroyStructure(ptr, typeof(System.Management.Automation.Security.NativeMethods.WINTRUST_DATA));
                Marshal.FreeCoTaskMem(ptr);
            }
            return num;
        }

        [ArchitectureSensitive]
        internal static System.Management.Automation.Signature SignFile(SigningOption option, string fileName, X509Certificate2 certificate, string timeStampServerUrl, string hashAlgorithm)
        {
            bool flag = false;
            System.Management.Automation.Signature signature = null;
            IntPtr zero = IntPtr.Zero;
            uint error = 0;
            string pszOID = null;
            Utils.CheckArgForNullOrEmpty(fileName, "fileName");
            Utils.CheckArgForNull(certificate, "certificate");
            if (!string.IsNullOrEmpty(timeStampServerUrl) && ((timeStampServerUrl.Length <= 7) || (timeStampServerUrl.IndexOf("http://", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                throw PSTraceSource.NewArgumentException("certificate", "Authenticode", "TimeStampUrlRequired", new object[0]);
            }
            if (!string.IsNullOrEmpty(hashAlgorithm))
            {
                IntPtr pvKey = Marshal.StringToHGlobalUni(hashAlgorithm);
                IntPtr ptr = System.Management.Automation.Security.NativeMethods.CryptFindOIDInfo(2, pvKey, 0);
                if (ptr == IntPtr.Zero)
                {
                    throw PSTraceSource.NewArgumentException("certificate", "Authenticode", "InvalidHashAlgorithm", new object[0]);
                }
                System.Management.Automation.Security.NativeMethods.CRYPT_OID_INFO crypt_oid_info = (System.Management.Automation.Security.NativeMethods.CRYPT_OID_INFO) Marshal.PtrToStructure(ptr, typeof(System.Management.Automation.Security.NativeMethods.CRYPT_OID_INFO));
                pszOID = crypt_oid_info.pszOID;
            }
            if (!SecuritySupport.CertIsGoodForSigning(certificate))
            {
                throw PSTraceSource.NewArgumentException("certificate", "Authenticode", "CertNotGoodForSigning", new object[0]);
            }
            SecuritySupport.CheckIfFileExists(fileName);
            try
            {
                string str2 = null;
                if (!string.IsNullOrEmpty(timeStampServerUrl))
                {
                    str2 = timeStampServerUrl;
                }
                System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO structure = System.Management.Automation.Security.NativeMethods.InitSignInfoStruct(fileName, certificate, str2, pszOID, option);
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                flag = System.Management.Automation.Security.NativeMethods.CryptUIWizDigitalSign(1, IntPtr.Zero, IntPtr.Zero, zero, IntPtr.Zero);
                Marshal.DestroyStructure(structure.pSignExtInfo, typeof(System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO));
                Marshal.FreeCoTaskMem(structure.pSignExtInfo);
                if (!flag)
                {
                    error = GetLastWin32Error();
                    switch (error)
                    {
                        case 0x80004005:
                        case 0x80070001:
                        case 0x80072ee7:
                            flag = true;
                            goto Label_01CF;

                        case 0x80090008:
                            throw PSTraceSource.NewArgumentException("certificate", "Authenticode", "InvalidHashAlgorithm", new object[0]);
                    }
                    tracer.TraceError("CryptUIWizDigitalSign: failed: {0:x}", new object[] { error });
                }
            Label_01CF:
                if (flag)
                {
                    return GetSignature(fileName, null);
                }
                signature = new System.Management.Automation.Signature(fileName, error);
            }
            finally
            {
                Marshal.DestroyStructure(zero, typeof(System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO));
                Marshal.FreeCoTaskMem(zero);
            }
            return signature;
        }
    }
}

