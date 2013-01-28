namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal delegate object PowerShellBinaryOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval);
}

