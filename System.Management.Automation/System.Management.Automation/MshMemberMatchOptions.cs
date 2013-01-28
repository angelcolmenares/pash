namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum MshMemberMatchOptions
    {
        None,
        IncludeHidden,
        OnlySerializable
    }
}

