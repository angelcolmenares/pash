namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum PSCredentialTypes
    {
        Default = 3,
        Domain = 2,
        Generic = 1
    }
}

