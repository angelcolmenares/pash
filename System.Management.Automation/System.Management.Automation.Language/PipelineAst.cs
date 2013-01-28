namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class PipelineAst : PipelineBaseAst
    {
        public PipelineAst(IScriptExtent extent, IEnumerable<CommandBaseAst> pipelineElements) : base(extent)
        {
            if ((pipelineElements == null) || !pipelineElements.Any<CommandBaseAst>())
            {
                throw PSTraceSource.NewArgumentException("pipelineElements");
            }
            this.PipelineElements = new ReadOnlyCollection<CommandBaseAst>(pipelineElements.ToArray<CommandBaseAst>());
            base.SetParents((IEnumerable<Ast>) this.PipelineElements);
        }

        public PipelineAst(IScriptExtent extent, CommandBaseAst commandAst) : base(extent)
        {
            if (commandAst == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandAst");
            }
            this.PipelineElements = new ReadOnlyCollection<CommandBaseAst>(new CommandBaseAst[] { commandAst });
            base.SetParent(commandAst);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitPipeline(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.PipelineElements.Last<CommandBaseAst>().GetInferredType(context);
        }

        public override ExpressionAst GetPureExpression()
        {
            if (this.PipelineElements.Count == 1)
            {
                CommandExpressionAst ast = this.PipelineElements[0] as CommandExpressionAst;
                if ((ast != null) && !ast.Redirections.Any<RedirectionAst>())
                {
                    return ast.Expression;
                }
            }
            return null;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitPipeline(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (CommandBaseAst ast in this.PipelineElements)
                    {
                        action = ast.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            return action;
                        }
                    }
                    break;
            }
            return action;
        }

        public ReadOnlyCollection<CommandBaseAst> PipelineElements { get; private set; }
    }
}

