namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class ThrowStatementAst : StatementAst
    {
        public ThrowStatementAst(IScriptExtent extent, PipelineBaseAst pipeline) : base(extent)
        {
            if (pipeline != null)
            {
                this.Pipeline = pipeline;
                base.SetParent(pipeline);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitThrowStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitThrowStatement(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.Pipeline != null))
            {
                action = this.Pipeline.InternalVisit(visitor);
            }
            return action;
        }

        public bool IsRethrow
        {
            get
            {
                if (this.Pipeline == null)
                {
                    for (Ast ast = base.Parent; ast != null; ast = ast.Parent)
                    {
                        if (ast is CatchClauseAst)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public PipelineBaseAst Pipeline { get; private set; }
    }
}

