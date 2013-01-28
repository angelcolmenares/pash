namespace System.Management.Automation.Security
{
    using System;

    internal class NativeConstants
    {
        internal const int CRYPT_OID_INFO_CNG_ALGID_KEY = 5;
        internal const int CRYPT_OID_INFO_NAME_KEY = 2;
        internal const int CRYPT_OID_INFO_OID_KEY = 1;
        public const int ERROR_ACCESS_DISABLED_BY_POLICY = 0x4ec;
        public const int ERROR_ACCESS_DISABLED_NO_SAFER_UI_BY_POLICY = 0x312;
        public const int ERROR_MORE_DATA = 0xea;
        public const int S_FALSE = 1;
        public const int S_OK = 0;
        public const int SAFER_CRITERIA_AUTHENTICODE = 8;
        public const int SAFER_CRITERIA_IMAGEHASH = 4;
        public const int SAFER_CRITERIA_IMAGEPATH = 1;
        public const int SAFER_CRITERIA_IMAGEPATH_NT = 0x1000;
        public const int SAFER_CRITERIA_NOSIGNEDHASH = 2;
        public const int SAFER_CRITERIA_URLZONE = 0x10;
        public const int SAFER_MAX_HASH_SIZE = 0x40;
        public const int SAFER_TOKEN_COMPARE_ONLY = 2;
        public const int SAFER_TOKEN_MAKE_INERT = 4;
        public const int SAFER_TOKEN_NULL_IF_EQUAL = 1;
        public const string SRP_POLICY_SCRIPT = "SCRIPT";
        public const int WTD_UI_NONE = 2;
    }
}

