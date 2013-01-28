namespace System.Management.Automation.Provider
{
    using System;

    [Flags]
    public enum ProviderCapabilities
    {
        Credentials = 0x20,
        Exclude = 2,
        ExpandWildcards = 8,
        Filter = 4,
        Include = 1,
        None = 0,
        ShouldProcess = 0x10,
        Transactions = 0x40
    }
}

