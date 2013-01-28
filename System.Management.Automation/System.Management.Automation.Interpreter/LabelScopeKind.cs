namespace System.Management.Automation.Interpreter
{
    using System;

    internal enum LabelScopeKind
    {
        Statement,
        Block,
        Switch,
        Lambda,
        Try,
        Catch,
        Finally,
        Filter,
        Expression
    }
}

