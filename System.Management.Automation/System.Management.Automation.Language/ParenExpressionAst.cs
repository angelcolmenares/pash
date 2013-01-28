namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class ParenExpressionAst : ExpressionAst, ISupportsAssignment
    {
        public ParenExpressionAst(IScriptExtent extent, PipelineBaseAst pipeline) : base(extent)
        {
            if (pipeline == null)
            {
                throw PSTraceSource.NewArgumentNullException("pipeline");
            }
            this.Pipeline = pipeline;
            base.SetParent(pipeline);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitParenExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Pipeline.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitParenExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Pipeline.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return ((ISupportsAssignment) this.Pipeline.GetPureExpression()).GetAssignableValue();
        }

        public PipelineBaseAst Pipeline { get; private set; }
    }
}

