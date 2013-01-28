namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum ParameterBindingFlags
    {
        DelayBindScriptBlock = 4,
        IsDefaultValue = 2,
        None = 0,
        ShouldCoerceType = 1,
        ThrowOnParameterNotFound = 8
    }
}

