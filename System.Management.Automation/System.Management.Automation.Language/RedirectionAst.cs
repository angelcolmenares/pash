namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public abstract class RedirectionAst : Ast
    {
        protected RedirectionAst(IScriptExtent extent, RedirectionStream from) : base(extent)
        {
            this.FromStream = from;
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        public RedirectionStream FromStream { get; private set; }
    }
}

