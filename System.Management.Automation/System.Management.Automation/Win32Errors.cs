namespace System.Management.Automation
{
    using System;

    internal static class Win32Errors
    {
        internal const uint CERT_E_UNTRUSTEDROOT = 0x800b0109;
        internal const uint CRYPT_E_BAD_MSG = 0x8009200d;
        internal const uint E_FAIL = 0x80004005;
        internal const uint NO_ERROR = 0;
        internal const uint NTE_BAD_ALGID = 0x80090008;
        internal const uint TRUST_E_BAD_DIGEST = 0x80096010;
        internal const uint TRUST_E_EXPLICIT_DISTRUST = 0x800b0111;
        internal const uint TRUST_E_NOSIGNATURE = 0x800b0100;
        internal const uint TRUST_E_PROVIDER_UNKNOWN = 0x800b0001;
        internal const uint TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800b0003;
    }
}

