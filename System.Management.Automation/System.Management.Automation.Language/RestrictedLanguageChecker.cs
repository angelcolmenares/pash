namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class RestrictedLanguageChecker : AstVisitor
    {
        private readonly IEnumerable<string> _allowedCommands;
        private readonly IEnumerable<string> _allowedVariables;
        private readonly bool _allowEnvironmentVariables;
        private readonly bool _allVariablesAreAllowed;
        private static readonly HashSet<string> _defaultAllowedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PSCulture", "PSUICulture", "true", "false", "null" };
        private readonly Parser _parser;

        internal RestrictedLanguageChecker(Parser parser, IEnumerable<string> allowedCommands, IEnumerable<string> allowedVariables, bool allowEnvironmentVariables)
        {
            this._parser = parser;
            this._allowedCommands = allowedCommands;
            if (allowedVariables != null)
            {
                if ((allowedVariables.Count<string>() == 1) && allowedVariables.Contains<string>("*"))
                {
                    this._allVariablesAreAllowed = true;
                }
                else
                {
                    this._allowedVariables = new HashSet<string>(_defaultAllowedVariables).Union<string>(allowedVariables);
                }
            }
            else
            {
                this._allowedVariables = _defaultAllowedVariables;
            }
            this._allowEnvironmentVariables = allowEnvironmentVariables;
        }

        internal static void CheckDataStatementAstAtRuntime(DataStatementAst dataStatementAst, string[] allowedCommands)
        {
            Parser parser = new Parser();
            RestrictedLanguageChecker visitor = new RestrictedLanguageChecker(parser, allowedCommands, null, false);
            dataStatementAst.Body.InternalVisit(visitor);
            if (parser.ErrorList.Any<ParseError>())
            {
                throw new ParseException(parser.ErrorList.ToArray());
            }
        }

        private void CheckTypeName(Ast ast, ITypeName typename)
        {
            Type reflectionType = typename.GetReflectionType();
            if ((reflectionType == null) || (Type.GetTypeCode(reflectionType.IsArray ? reflectionType.GetElementType() : reflectionType) == TypeCode.Object))
            {
                this.ReportError(ast, () => ParserStrings.TypeNotAllowedInDataSection, new object[] { typename.FullName });
            }
        }

        private void ReportError(Ast ast, Expression<Func<string>> errorExpr, params object[] args)
        {
            this.ReportError(ast.Extent, errorExpr, args);
            this.FoundError = true;
        }

        private void ReportError(IScriptExtent extent, Expression<Func<string>> errorExpr, params object[] args)
        {
            this._parser.ReportError(extent, errorExpr, args);
            this.FoundError = true;
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            this.ReportError(assignmentStatementAst, () => ParserStrings.AssignmentStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
        {
            this.ReportError(attributeAst, () => ParserStrings.AttributeNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            this.ReportError(attributedExpressionAst, () => ParserStrings.AttributeNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            if (binaryExpressionAst.Operator.HasTrait(TokenFlags.DisallowedInRestrictedMode))
            {
                this.ReportError(binaryExpressionAst.ErrorPosition, () => ParserStrings.OperatorNotSupportedInDataSection, new object[] { binaryExpressionAst.Operator.Text() });
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            this.ReportError(blockStatementAst, () => ParserStrings.ParallelAndSequenceBlockNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            this.ReportError(breakStatementAst, () => ParserStrings.FlowControlStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            string commandName;
            if (commandAst.InvocationOperator == TokenKind.Dot)
            {
                this.ReportError(commandAst, () => ParserStrings.DotSourcingNotSupportedInDataSection, new object[0]);
                return AstVisitAction.Continue;
            }
            if (this._allowedCommands != null)
            {
                commandName = commandAst.GetCommandName();
                if (commandName == null)
                {
                    if (commandAst.InvocationOperator == TokenKind.Ampersand)
                    {
                        this.ReportError(commandAst, () => ParserStrings.OperatorNotSupportedInDataSection, new object[] { TokenKind.Ampersand.Text() });
                    }
                    else
                    {
                        this.ReportError(commandAst, () => ParserStrings.CmdletNotInAllowedListForDataSection, new object[] { commandAst.Extent.Text });
                    }
                    return AstVisitAction.Continue;
                }
                if (!(from allowedCommand in this._allowedCommands
                    where allowedCommand.Equals(commandName, StringComparison.OrdinalIgnoreCase)
                    select allowedCommand).Any<string>())
                {
                    this.ReportError(commandAst, () => ParserStrings.CmdletNotInAllowedListForDataSection, new object[] { commandName });
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            this.ReportError(continueStatementAst, () => ParserStrings.FlowControlStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            this.ReportError(dataStatementAst, () => ParserStrings.DataSectionStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            this.ReportError(doUntilStatementAst, () => ParserStrings.DoWhileStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            this.ReportError(doWhileStatementAst, () => ParserStrings.DoWhileStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            this.ReportError(exitStatementAst, () => ParserStrings.FlowControlStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            this.ReportError(fileRedirectionAst, () => ParserStrings.RedirectionNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            this.ReportError(forEachStatementAst, () => ParserStrings.ForeachStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
        {
            this.ReportError(forStatementAst, () => ParserStrings.ForWhileStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            this.ReportError(functionDefinitionAst, () => ParserStrings.FunctionDeclarationNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            this.ReportError(indexExpressionAst, () => ParserStrings.ArrayReferenceNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            this.ReportError(methodCallAst, () => ParserStrings.MethodCallNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            this.ReportError(memberExpressionAst, () => ParserStrings.PropertyReferenceNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            this.ReportError(mergingRedirectionAst, () => ParserStrings.RedirectionNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            this.ReportError(paramBlockAst, () => ParserStrings.ParameterDeclarationNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            this.ReportError(returnStatementAst, () => ParserStrings.FlowControlStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            this.ReportError(scriptBlockAst, () => ParserStrings.ScriptBlockNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            this.ReportError(scriptBlockExpressionAst, () => ParserStrings.ScriptBlockNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            this.ReportError(switchStatementAst, () => ParserStrings.SwitchStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            this.ReportError(throwStatementAst, () => ParserStrings.FlowControlStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
        {
            this.ReportError(trapStatementAst, () => ParserStrings.TrapStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            this.ReportError(tryStatementAst, () => ParserStrings.TryStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            this.CheckTypeName(typeConstraintAst, typeConstraintAst.TypeName);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            this.CheckTypeName(typeExpressionAst, typeExpressionAst.TypeName);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            if (unaryExpressionAst.TokenKind.HasTrait(TokenFlags.DisallowedInRestrictedMode))
            {
                this.ReportError(unaryExpressionAst, () => ParserStrings.OperatorNotSupportedInDataSection, new object[] { unaryExpressionAst.TokenKind.Text() });
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            VariablePath variablePath = variableExpressionAst.VariablePath;
            if ((!this._allVariablesAreAllowed && !this._allowedVariables.Contains<string>(variablePath.UserPath, StringComparer.OrdinalIgnoreCase)) && ((!this._allowEnvironmentVariables || !variablePath.IsDriveQualified) || !variablePath.DriveName.Equals("env", StringComparison.OrdinalIgnoreCase)))
            {
                this.ReportError(variableExpressionAst, () => ParserStrings.VariableReferenceNotSupportedInDataSection, new object[0]);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            this.ReportError(whileStatementAst, () => ParserStrings.ForWhileStatementNotSupportedInDataSection, new object[0]);
            return AstVisitAction.Continue;
        }

        private bool FoundError { get; set; }
    }
}

