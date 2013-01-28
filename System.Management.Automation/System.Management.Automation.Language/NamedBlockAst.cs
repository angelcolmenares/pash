namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class NamedBlockAst : Ast
    {
        public NamedBlockAst(IScriptExtent extent, TokenKind blockName, StatementBlockAst statementBlock, bool unnamed) : base(extent)
        {
            if (!blockName.HasTrait(TokenFlags.ScriptBlockBlockName) || (unnamed && ((blockName == TokenKind.Begin) || (blockName == TokenKind.Dynamicparam))))
            {
                throw PSTraceSource.NewArgumentException("blockName");
            }
            if (statementBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("statementBlock");
            }
            this.Unnamed = unnamed;
            this.BlockKind = blockName;
            ReadOnlyCollection<StatementAst> statements = statementBlock.Statements;
            this.Statements = statements;
            foreach (StatementAst ast in statements)
            {
                ast.ClearParent();
            }
            base.SetParents((IEnumerable<Ast>) statements);
            ReadOnlyCollection<TrapStatementAst> traps = statementBlock.Traps;
            if ((traps != null) && traps.Any<TrapStatementAst>())
            {
                this.Traps = traps;
                foreach (TrapStatementAst ast2 in traps)
                {
                    ast2.ClearParent();
                }
                base.SetParents((IEnumerable<Ast>) traps);
            }
            if (!unnamed)
            {
                InternalScriptExtent extent2 = statementBlock.Extent as InternalScriptExtent;
                if (extent2 != null)
                {
                    this.OpenCurlyExtent = new InternalScriptExtent(extent2.PositionHelper, extent2.StartOffset, extent2.StartOffset + 1);
                    this.CloseCurlyExtent = new InternalScriptExtent(extent2.PositionHelper, extent2.EndOffset - 1, extent2.EndOffset);
                }
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitNamedBlock(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Statements.SelectMany(ast => ast.GetInferredType(context));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitNamedBlock(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = StatementBlockAst.InternalVisit(visitor, this.Traps, this.Statements, action);
                    break;
            }
            return action;
        }

        public TokenKind BlockKind { get; private set; }

        internal IScriptExtent CloseCurlyExtent { get; private set; }

        internal IScriptExtent OpenCurlyExtent { get; private set; }

        public ReadOnlyCollection<StatementAst> Statements { get; private set; }

        public ReadOnlyCollection<TrapStatementAst> Traps { get; private set; }

        public bool Unnamed { get; private set; }
    }
}

