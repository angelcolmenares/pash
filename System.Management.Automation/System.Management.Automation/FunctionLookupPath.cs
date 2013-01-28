namespace System.Management.Automation
{
    using System;

    internal class FunctionLookupPath : VariablePath
    {
        internal FunctionLookupPath(string path) : base(path, VariablePathFlags.Unqualified | VariablePathFlags.Function)
        {
        }
    }
}

