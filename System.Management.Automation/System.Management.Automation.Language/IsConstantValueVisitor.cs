namespace System.Management.Automation.Language
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class IsConstantValueVisitor : ICustomAstVisitor
    {
        public static bool IsConstant(Ast ast, out object constantValue, bool forAttribute = false, bool forRequires = false)
        {
            try
            {
                IsConstantValueVisitor visitor2 = new IsConstantValueVisitor {
                    CheckingAttributeArgument = forAttribute,
                    CheckingRequiresArgument = forRequires
                };
                if ((bool) ast.Accept(visitor2))
                {
                    Ast parent = ast.Parent;
                    while (parent != null)
                    {
                        if (parent is DataStatementAst)
                        {
                            break;
                        }
                        parent = parent.Parent;
                    }
                    if (parent == null)
                    {
                        ConstantValueVisitor visitor = new ConstantValueVisitor {
                            AttributeArgument = forAttribute,
                            RequiresArgument = forRequires
                        };
                        constantValue = ast.Accept(visitor);
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            constantValue = null;
            return false;
        }

        private static bool IsNullDivisor(ExpressionAst operand)
        {
            VariableExpressionAst ast = operand as VariableExpressionAst;
            if (ast != null)
            {
                BinaryExpressionAst parent = operand.Parent as BinaryExpressionAst;
                if ((parent == null) || (parent.Right != operand))
                {
                    return false;
                }
                switch (parent.Operator)
                {
                    case TokenKind.Divide:
                    case TokenKind.Rem:
                    case TokenKind.DivideEquals:
                    case TokenKind.RemainderEquals:
                    {
                        string unqualifiedPath = ast.VariablePath.UnqualifiedPath;
                        if (!unqualifiedPath.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            return unqualifiedPath.Equals("null", StringComparison.OrdinalIgnoreCase);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return false;
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return ((!this.CheckingAttributeArgument && !this.CheckingRequiresArgument) ? ((object) 0) : ((object) !(from e in arrayLiteralAst.Elements
                where !((bool) e.Accept(this))
                select e).Any<ExpressionAst>()));
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
            return (((!binaryExpressionAst.Operator.HasTrait(TokenFlags.CanConstantFold) || !((bool) binaryExpressionAst.Left.Accept(this))) || !((bool) binaryExpressionAst.Right.Accept(this))) ? ((object) false) : ((object) !IsNullDivisor(binaryExpressionAst.Right)));
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
            Type reflectionType = convertExpressionAst.Type.TypeName.GetReflectionType();
            if (reflectionType == null)
            {
                return false;
            }
            if (!reflectionType.IsSafePrimitive())
            {
                return false;
            }
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
            return (!this.CheckingRequiresArgument ? ((object) false) : ((object) hashtableAst.KeyValuePairs.All<Tuple<ExpressionAst, StatementAst>>(pair => (((bool) pair.Item1.Accept(this)) && ((bool) pair.Item2.Accept(this))))));
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            return false;
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            return false;
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            return false;
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            if (!memberExpressionAst.Static || !(memberExpressionAst.Expression is TypeExpressionAst))
            {
                return false;
            }
            Type reflectionType = ((TypeExpressionAst) memberExpressionAst.Expression).TypeName.GetReflectionType();
            if (reflectionType == null)
            {
                return false;
            }
            StringConstantExpressionAst member = memberExpressionAst.Member as StringConstantExpressionAst;
            if (member == null)
            {
                return false;
            }
            MemberInfo[] infoArray = reflectionType.GetMember(member.Value, MemberTypes.Field, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (infoArray.Length != 1)
            {
                return false;
            }
            return ((((FieldInfo) infoArray[0]).Attributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope);
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
            return ((pureExpression == null) ? ((object) false) : ((object) ((bool) pureExpression.Accept(this))));
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
            return this.CheckingAttributeArgument;
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
            return ((ast == null) ? ((object) false) : ((object) ((bool) ast.Accept(this))));
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
            return (this.CheckingAttributeArgument ? ((object) true) : ((object) (typeExpressionAst.TypeName.GetReflectionType() != null)));
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            return (!unaryExpressionAst.TokenKind.HasTrait(TokenFlags.CanConstantFold) ? ((object) false) : ((object) ((bool) unaryExpressionAst.Child.Accept(this))));
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return usingExpressionAst.SubExpression.Accept(this);
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            return variableExpressionAst.IsConstantVariable();
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            return false;
        }

        internal bool CheckingAttributeArgument { get; set; }

        internal bool CheckingRequiresArgument { get; set; }
    }
}

