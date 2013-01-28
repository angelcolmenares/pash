namespace System.Management.Automation.Language
{
    using System;

    public abstract class CommandElementAst : Ast
    {
        protected CommandElementAst(IScriptExtent extent) : base(extent)
        {
        }
    }
}

