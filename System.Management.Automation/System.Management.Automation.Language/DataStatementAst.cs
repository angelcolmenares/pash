namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class DataStatementAst : StatementAst
    {
        private int _tupleIndex;
        private static readonly ExpressionAst[] EmptyCommandsAllowed = new ExpressionAst[0];

        public DataStatementAst(IScriptExtent extent, string variableName, IEnumerable<ExpressionAst> commandsAllowed, StatementBlockAst body) : base(extent)
        {
            this._tupleIndex = -1;
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            if (string.IsNullOrWhiteSpace(variableName))
            {
                variableName = null;
            }
            this.Variable = variableName;
            if ((commandsAllowed != null) && commandsAllowed.Any<ExpressionAst>())
            {
                this.CommandsAllowed = new ReadOnlyCollection<ExpressionAst>(commandsAllowed.ToArray<ExpressionAst>());
                base.SetParents((IEnumerable<Ast>) this.CommandsAllowed);
                this.HasNonConstantAllowedCommand = (from ast in this.CommandsAllowed
                    where !(ast is StringConstantExpressionAst)
                    select ast).Any<ExpressionAst>();
            }
            else
            {
                this.CommandsAllowed = new ReadOnlyCollection<ExpressionAst>(EmptyCommandsAllowed);
            }
            this.Body = body;
            base.SetParent(body);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitDataStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Body.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitDataStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Body.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public StatementBlockAst Body { get; private set; }

        public ReadOnlyCollection<ExpressionAst> CommandsAllowed { get; private set; }

        internal bool HasNonConstantAllowedCommand { get; private set; }

        internal int TupleIndex
        {
            get
            {
                return this._tupleIndex;
            }
            set
            {
                this._tupleIndex = value;
            }
        }

        public string Variable { get; private set; }
    }
}

