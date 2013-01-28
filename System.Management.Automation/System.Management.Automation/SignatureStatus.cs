namespace System.Management.Automation
{
    using System;

    public enum SignatureStatus
    {
        Valid,
        UnknownError,
        NotSigned,
        HashMismatch,
        NotTrusted,
        NotSupportedFileFormat,
        Incompatible
    }
}

