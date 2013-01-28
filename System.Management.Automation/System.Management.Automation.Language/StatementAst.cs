namespace System.Management.Automation.Language
{
    using System;

    public abstract class StatementAst : Ast
    {
        protected StatementAst(IScriptExtent extent) : base(extent)
        {
        }
    }
}

