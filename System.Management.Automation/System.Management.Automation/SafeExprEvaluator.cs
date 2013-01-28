using System.Collections;

namespace System.Management.Automation
{
    using System;
    using System.Linq;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;

    internal class SafeExprEvaluator : ICustomAstVisitor
    {
        internal static bool TrySafeEval(ExpressionAst ast, ExecutionContext executionContext, out object value)
        {
            bool flag;
            if (!((bool) ast.Accept(new SafeExprEvaluator())))
            {
                value = null;
                return false;
            }
            PSLanguageMode? nullable = null;
            try
            {
                if (ExecutionContext.HasEverUsedConstrainedLanguage)
                {
                    nullable = new PSLanguageMode?(executionContext.LanguageMode);
                    executionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                }
                value = Compiler.GetExpressionValue(ast, executionContext, (IList)null);
                flag = true;
            }
            catch
            {
                value = null;
                flag = false;
            }
            finally
            {
                if (nullable.HasValue)
                {
                    executionContext.LanguageMode = nullable.Value;
                }
            }
            return flag;
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return arrayExpressionAst.SubExpression.Accept(this);
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return arrayLiteralAst.Elements.All<ExpressionAst>(e => ((bool) e.Accept(this)));
        }

        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            return false;
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            return false;
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            return false;
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            return (!((bool) binaryExpressionAst.Left.Accept(this)) ? ((object) 0) : ((object) ((bool) binaryExpressionAst.Right.Accept(this))));
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            return false;
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            return false;
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            return false;
        }

        public object VisitCommand(CommandAst commandAst)
        {
            return false;
        }

        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return false;
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            return false;
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return true;
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            return false;
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            return (bool) convertExpressionAst.Child.Accept(this);
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            return false;
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            return false;
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            return false;
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return false;
        }

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return false;
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            return false;
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            return false;
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            return false;
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            return false;
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            return false;
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return false;
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            foreach (Tuple<ExpressionAst, StatementAst> tuple in hashtableAst.KeyValuePairs)
            {
                if (!((bool) tuple.Item1.Accept(this)))
                {
                    return false;
                }
                if (!((bool) tuple.Item2.Accept(this)))
                {
                    return false;
                }
            }
            return true;
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            return false;
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            return (!((bool) indexExpressionAst.Target.Accept(this)) ? ((object) 0) : ((object) ((bool) indexExpressionAst.Index.Accept(this))));
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            return false;
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            return (!((bool) memberExpressionAst.Expression.Accept(this)) ? ((object) 0) : ((object) ((bool) memberExpressionAst.Member.Accept(this))));
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            return false;
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return false;
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return false;
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return false;
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            return false;
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return parenExpressionAst.Pipeline.Accept(this);
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            ExpressionAst pureExpression = pipelineAst.GetPureExpression();
            return ((pureExpression == null) ? ((object) 0) : ((object) ((bool) pureExpression.Accept(this))));
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            return false;
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            return false;
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return true;
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            if (statementBlockAst.Traps != null)
            {
                return false;
            }
            if (statementBlockAst.Statements.Count > 1)
            {
                return false;
            }
            StatementAst ast = statementBlockAst.Statements.FirstOrDefault<StatementAst>();
            return ((ast == null) ? ((object) 0) : ((object) ((bool) ast.Accept(this))));
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return true;
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return subExpressionAst.SubExpression.Accept(this);
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            return false;
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            return false;
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            return false;
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            return false;
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return false;
        }

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return true;
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            return (bool) unaryExpressionAst.Child.Accept(this);
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return false;
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            return true;
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            return false;
        }
    }
}

