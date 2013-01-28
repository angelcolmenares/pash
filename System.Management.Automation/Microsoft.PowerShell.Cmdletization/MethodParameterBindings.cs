namespace Microsoft.PowerShell.Cmdletization
{
    using System;

    [Flags]
    public enum MethodParameterBindings
    {
        Error = 4,
        In = 1,
        Out = 2
    }
}

