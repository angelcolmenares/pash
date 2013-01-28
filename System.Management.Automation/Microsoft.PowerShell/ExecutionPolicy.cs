namespace Microsoft.PowerShell
{
    using System;

    public enum ExecutionPolicy
    {
        AllSigned = 2,
        Bypass = 4,
        Default = 3,
        RemoteSigned = 1,
        Restricted = 3,
        Undefined = 5,
        Unrestricted = 0
    }
}

