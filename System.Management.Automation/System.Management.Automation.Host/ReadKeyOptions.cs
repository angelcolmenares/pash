namespace System.Management.Automation.Host
{
    using System;

    [Flags]
    public enum ReadKeyOptions
    {
        AllowCtrlC = 1,
        IncludeKeyDown = 4,
        IncludeKeyUp = 8,
        NoEcho = 2
    }
}

