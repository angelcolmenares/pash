namespace System.Management.Automation.Interpreter
{
    using System;

    [Flags]
    internal enum ExpressionAccess
    {
        None,
        Read,
        Write,
        ReadWrite
    }
}

