namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal static class VariablePathExtentions
    {
        internal static bool IsAnyLocal(this VariablePath variablePath)
        {
            if (!variablePath.IsUnscopedVariable && !variablePath.IsLocal)
            {
                return variablePath.IsPrivate;
            }
            return true;
        }
    }
}

