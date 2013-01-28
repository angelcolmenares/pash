namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum ShouldProcessReason
    {
        None,
        WhatIf
    }
}

