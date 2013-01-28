namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class ConstantValueVisitor : ICustomAstVisitor
    {
        [Conditional("ASSERTIONS_TRACE"), Conditional("DEBUG")]
        private void CheckIsConstant(Ast ast, string msg)
        {
        }

        private static object CompileAndInvoke(Ast ast)
        {
            object obj2;
            try
            {
                Compiler visitor = new Compiler {
                    CompilingConstantExpression = true
                };
                obj2 = Expression.Lambda((Expression) ast.Accept(visitor), new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return arrayExpressionAst.SubExpression.Accept(this);
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return (from e in arrayLiteralAst.Elements select e.Accept(this)).ToArray<object>();
        }

        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            return AutomationNull.Value;
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            return CompileAndInvoke(binaryExpressionAst);
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            return AutomationNull.Value;
        }

        public object VisitCommand(CommandAst commandAst)
        {
            return AutomationNull.Value;
        }

        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            return AutomationNull.Value;
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return constantExpressionAst.Value;
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            return CompileAndInvoke(convertExpressionAst);
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            Hashtable hashtable = new Hashtable();
            foreach (Tuple<ExpressionAst, StatementAst> tuple in hashtableAst.KeyValuePairs)
            {
                hashtable.Add(tuple.Item1.Accept(this), tuple.Item2.Accept(this));
            }
            return hashtable;
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            return AutomationNull.Value;
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            Type reflectionType = ((TypeExpressionAst) memberExpressionAst.Expression).TypeName.GetReflectionType();
            string name = ((StringConstantExpressionAst) memberExpressionAst.Member).Value;
            return ((FieldInfo) reflectionType.GetMember(name, MemberTypes.Field, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)[0]).GetValue(null);
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            return AutomationNull.Value;
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return AutomationNull.Value;
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return AutomationNull.Value;
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return AutomationNull.Value;
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            return AutomationNull.Value;
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return parenExpressionAst.Pipeline.Accept(this);
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            return pipelineAst.GetPureExpression().Accept(this);
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            return AutomationNull.Value;
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return new ScriptBlock(scriptBlockExpressionAst.ScriptBlock, false);
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return statementBlockAst.Statements.First<StatementAst>().Accept(this);
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return stringConstantExpressionAst.Value;
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return subExpressionAst.SubExpression.Accept(this);
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            return AutomationNull.Value;
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return AutomationNull.Value;
        }

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return TypeOps.ResolveTypeName(typeExpressionAst.TypeName);
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            return CompileAndInvoke(unaryExpressionAst);
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return usingExpressionAst.SubExpression.Accept(this);
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            string unqualifiedPath = variableExpressionAst.VariablePath.UnqualifiedPath;
            if (unqualifiedPath.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (unqualifiedPath.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return null;
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            return AutomationNull.Value;
        }

        internal bool AttributeArgument { get; set; }

        internal bool RequiresArgument { get; set; }
    }
}

