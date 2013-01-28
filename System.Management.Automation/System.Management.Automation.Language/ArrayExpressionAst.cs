namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ArrayExpressionAst : ExpressionAst
    {
        public ArrayExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock) : base(extent)
        {
            if (statementBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("statementBlock");
            }
            this.SubExpression = statementBlock;
            base.SetParent(statementBlock);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitArrayExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(typeof(object[]));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitArrayExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.SubExpression.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public override Type StaticType
        {
            get
            {
                return typeof(object[]);
            }
        }

        public StatementBlockAst SubExpression { get; private set; }

        
    }
}

