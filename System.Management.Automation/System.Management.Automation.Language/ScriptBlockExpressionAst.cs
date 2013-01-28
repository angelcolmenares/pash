namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ScriptBlockExpressionAst : ExpressionAst
    {
        public ScriptBlockExpressionAst(IScriptExtent extent, ScriptBlockAst scriptBlock) : base(extent)
        {
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("scriptBlock");
            }
            this.ScriptBlock = scriptBlock;
            base.SetParent(scriptBlock);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitScriptBlockExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(typeof(System.Management.Automation.ScriptBlock));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitScriptBlockExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.ScriptBlock.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public ScriptBlockAst ScriptBlock { get; private set; }

        public override Type StaticType
        {
            get
            {
                return typeof(System.Management.Automation.ScriptBlock);
            }
        }

        
    }
}

