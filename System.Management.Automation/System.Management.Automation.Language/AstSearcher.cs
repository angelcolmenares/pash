namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class AstSearcher : AstVisitor
    {
        private readonly Func<Ast, bool> _callback;
        private readonly bool _searchNestedScriptBlocks;
        private readonly bool _stopOnFirst;
        protected readonly List<Ast> Results;

        protected AstSearcher(Func<Ast, bool> callback, bool stopOnFirst, bool searchNestedScriptBlocks)
        {
            this._callback = callback;
            this._stopOnFirst = stopOnFirst;
            this._searchNestedScriptBlocks = searchNestedScriptBlocks;
            this.Results = new List<Ast>();
        }

        protected AstVisitAction Check(Ast ast)
        {
            if (this._callback(ast))
            {
                this.Results.Add(ast);
                if (this._stopOnFirst)
                {
                    return AstVisitAction.StopVisit;
                }
            }
            return AstVisitAction.Continue;
        }

        protected AstVisitAction CheckScriptBlock(Ast ast)
        {
            AstVisitAction skipChildren = this.Check(ast);
            if ((skipChildren == AstVisitAction.Continue) && !this._searchNestedScriptBlocks)
            {
                skipChildren = AstVisitAction.SkipChildren;
            }
            return skipChildren;
        }

        internal static bool Contains(Ast ast, Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
        {
            AstSearcher visitor = new AstSearcher(predicate, true, searchNestedScriptBlocks);
            ast.InternalVisit(visitor);
            return (visitor.Results.FirstOrDefault<Ast>() != null);
        }

        internal static IEnumerable<Ast> FindAll(Ast ast, Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
        {
            AstSearcher visitor = new AstSearcher(predicate, false, searchNestedScriptBlocks);
            ast.InternalVisit(visitor);
            return visitor.Results;
        }

        internal static Ast FindFirst(Ast ast, Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
        {
            AstSearcher visitor = new AstSearcher(predicate, true, searchNestedScriptBlocks);
            ast.InternalVisit(visitor);
            return visitor.Results.FirstOrDefault<Ast>();
        }

        internal static bool IsUsingDollarInput(Ast ast)
        {
            return Contains(ast, delegate (Ast ast_) {
                VariableExpressionAst a = ast_ as VariableExpressionAst;
                if (a == null)
                {
                    return false;
                }
                return a.VariablePath.IsVariable && a.VariablePath.UnqualifiedPath.Equals("input", StringComparison.OrdinalIgnoreCase);
            }, false);
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitAttribute(AttributeAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitCommand(CommandAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitCommandExpression(CommandExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitErrorExpression(ErrorExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitErrorStatement(ErrorStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitForStatement(ForStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst ast)
        {
            return this.CheckScriptBlock(ast);
        }

        public override AstVisitAction VisitHashtable(HashtableAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitIfStatement(IfStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitNamedBlock(NamedBlockAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitParamBlock(ParamBlockAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitParameter(ParameterAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitParenExpression(ParenExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitPipeline(PipelineAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst ast)
        {
            return this.CheckScriptBlock(ast);
        }

        public override AstVisitAction VisitStatementBlock(StatementBlockAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitSubExpression(SubExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitThrowStatement(ThrowStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitTrap(TrapStatementAst ast)
        {
            return this.CheckScriptBlock(ast);
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst ast)
        {
            return this.Check(ast);
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst ast)
        {
            return this.Check(ast);
        }
    }
}

