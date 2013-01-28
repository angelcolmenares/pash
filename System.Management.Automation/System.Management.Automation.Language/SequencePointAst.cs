namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal class SequencePointAst : Ast
    {
        public SequencePointAst(IScriptExtent extent) : base(extent)
        {
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return null;
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            return AstVisitAction.Continue;
        }
    }
}

