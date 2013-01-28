namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class SemanticChecks : AstVisitor
    {
        private readonly IsConstantValueVisitor _isConstantValueVisitor;
        private readonly Parser _parser;

        private SemanticChecks(Parser parser)
        {
            IsConstantValueVisitor visitor = new IsConstantValueVisitor {
                CheckingAttributeArgument = true
            };
            this._isConstantValueVisitor = visitor;
            this._parser = parser;
        }

        private void CheckArrayLiteralAssignment(ArrayLiteralAst ast, Action<Ast> reportError)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            foreach (ExpressionAst ast2 in ast.Elements)
            {
                this.CheckAssignmentTarget(ast2, true, reportError);
            }
        }

        internal static void CheckArrayTypeNameDepth(ITypeName typeName, IScriptExtent extent, Parser parser)
        {
            int num = 0;
            for (ITypeName name = typeName; !(name is TypeName); name = ((ArrayTypeName) name).ElementType)
            {
                num++;
                if (num > 200)
                {
                    parser.ReportError(extent, ParserStrings.ScriptTooComplicated, new object[0]);
                    return;
                }
                if (!(name is ArrayTypeName))
                {
                    break;
                }
            }
        }

        private void CheckAssignmentTarget(ExpressionAst ast, bool simpleAssignment, Action<Ast> reportError)
        {
            ArrayLiteralAst ast2 = ast as ArrayLiteralAst;
            Ast pipeline = null;
            if (ast2 != null)
            {
                if (simpleAssignment)
                {
                    this.CheckArrayLiteralAssignment(ast2, reportError);
                }
                else
                {
                    pipeline = ast2;
                }
            }
            else
            {
                ParenExpressionAst ast4 = ast as ParenExpressionAst;
                if (ast4 != null)
                {
                    ExpressionAst pureExpression = ast4.Pipeline.GetPureExpression();
                    if (pureExpression == null)
                    {
                        pipeline = ast4.Pipeline;
                    }
                    else
                    {
                        this.CheckAssignmentTarget(pureExpression, simpleAssignment, reportError);
                    }
                }
                else if (ast is ISupportsAssignment)
                {
                    if (ast is AttributedExpressionAst)
                    {
                        ExpressionAst child = ast;
                        int num = 0;
                        IScriptExtent extent = null;
                        while (child is AttributedExpressionAst)
                        {
                            ConvertExpressionAst ast7 = child as ConvertExpressionAst;
                            if (ast7 != null)
                            {
                                num++;
                                Type reflectionType = ast7.Type.TypeName.GetReflectionType();
                                if (typeof(PSReference).Equals(reflectionType))
                                {
                                    extent = ast7.Type.Extent;
                                }
                                else if (typeof(void).Equals(reflectionType))
                                {
                                    this._parser.ReportError(ast7.Type.Extent, ParserStrings.VoidTypeConstraintNotAllowed, new object[0]);
                                }
                            }
                            child = ((AttributedExpressionAst) child).Child;
                        }
                        if ((extent != null) && (num > 1))
                        {
                            this._parser.ReportError(extent, ParserStrings.ReferenceNeedsToBeByItselfInTypeConstraint, new object[0]);
                        }
                        else
                        {
                            this.CheckAssignmentTarget(child, simpleAssignment, reportError);
                        }
                    }
                }
                else
                {
                    pipeline = ast;
                }
            }
            if (pipeline != null)
            {
                reportError(pipeline);
            }
        }

        internal static void CheckAst(Parser parser, ScriptBlockAst ast)
        {
            SemanticChecks visitor = new SemanticChecks(parser);
            ast.InternalVisit(visitor);
        }

        private void CheckForDuplicateParameters(IEnumerable<ParameterAst> parameters)
        {
            if (parameters.Any<ParameterAst>())
            {
                HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (ParameterAst ast in parameters)
                {
                    string userPath = ast.Name.VariablePath.UserPath;
                    if (set.Contains(userPath))
                    {
                        this._parser.ReportError(ast.Name.Extent, ParserStrings.DuplicateFormalParameter, new object[] { userPath });
                    }
                    else
                    {
                        set.Add(userPath);
                    }
                    TypeConstraintAst ast2 = (from t in ast.Attributes.OfType<TypeConstraintAst>()
                        where typeof(void).Equals(t.TypeName.GetReflectionType())
                        select t).FirstOrDefault<TypeConstraintAst>();
                    if (ast2 != null)
                    {
                        this._parser.ReportError(ast2.Extent, ParserStrings.VoidTypeConstraintNotAllowed, new object[0]);
                    }
                }
            }
        }

        private void CheckForFlowOutOfFinally(Ast ast, string label)
        {
            for (Ast ast2 = ast.Parent; ast2 != null; ast2 = ast2.Parent)
            {
                if ((ast2 is ScriptBlockAst) || (ast2 is TrapStatementAst))
                {
                    break;
                }
                if (ast2 is NamedBlockAst)
                {
                    return;
                }
                if (((label != null) && (ast2 is LoopStatementAst)) && LoopFlowException.MatchLoopLabel(label, ((LoopStatementAst) ast2).Label ?? ""))
                {
                    return;
                }
                StatementBlockAst ast3 = ast2 as StatementBlockAst;
                if (ast3 != null)
                {
                    TryStatementAst parent = ast3.Parent as TryStatementAst;
                    if ((parent != null) && (parent.Finally == ast3))
                    {
                        this._parser.ReportError(ast.Extent, ParserStrings.ControlLeavingFinally, new object[0]);
                        return;
                    }
                }
            }
        }

        private ExpressionAst CheckUsingExpression(ExpressionAst exprAst)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            if (exprAst is VariableExpressionAst)
            {
                return null;
            }
            MemberExpressionAst ast = exprAst as MemberExpressionAst;
            if (((ast != null) && !(ast is InvokeMemberExpressionAst)) && (ast.Member is StringConstantExpressionAst))
            {
                return this.CheckUsingExpression(ast.Expression);
            }
            IndexExpressionAst ast2 = exprAst as IndexExpressionAst;
            if (ast2 == null)
            {
                return exprAst;
            }
            if (!this.IsValidAttributeArgument(ast2.Index))
            {
                return ast2.Index;
            }
            return this.CheckUsingExpression(ast2.Target);
        }

        private static IEnumerable<string> GetConstantDataStatementAllowedCommands(DataStatementAst dataStatementAst)
        {
            yield return "ConvertFrom-StringData";
            foreach (ExpressionAst iteratorVariable0 in dataStatementAst.CommandsAllowed)
            {
                yield return ((StringConstantExpressionAst) iteratorVariable0).Value;
            }
        }

        private static string GetLabel(ExpressionAst expr)
        {
            if (expr == null)
            {
                return "";
            }
            StringConstantExpressionAst ast = expr as StringConstantExpressionAst;
            if (ast == null)
            {
                return null;
            }
            return ast.Value;
        }

        private bool IsValidAttributeArgument(Ast ast)
        {
            var obj = ast.Accept(this._isConstantValueVisitor);
            if (obj is bool) return (bool)obj;
            return false;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            this.CheckAssignmentTarget(assignmentStatementAst.Left, assignmentStatementAst.Operator == TokenKind.Equals, delegate (Ast ast) {
                this._parser.ReportError(ast.Extent, ParserStrings.InvalidLeftHandSide, new object[0]);
            });
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (NamedAttributeArgumentAst ast in attributeAst.NamedArguments)
            {
                string argumentName = ast.ArgumentName;
                if (set.Contains(argumentName))
                {
                    this._parser.ReportError(ast.Extent, ParserStrings.DuplicateNamedArgument, new object[] { argumentName });
                }
                else
                {
                    set.Add(argumentName);
                    if (!ast.ExpressionOmitted && !this.IsValidAttributeArgument(ast.Argument))
                    {
                        this._parser.ReportError(ast.Argument.Extent, ParserStrings.ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock, new object[0]);
                    }
                }
            }
            foreach (ExpressionAst ast2 in attributeAst.PositionalArguments)
            {
                if (!this.IsValidAttributeArgument(ast2))
                {
                    this._parser.ReportError(ast2.Extent, ParserStrings.ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock, new object[0]);
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            AttributeBaseAst attribute = attributedExpressionAst.Attribute;
            while (attributedExpressionAst != null)
            {
                if (attributedExpressionAst.Child is VariableExpressionAst)
                {
                    return AstVisitAction.Continue;
                }
                attributedExpressionAst = attributedExpressionAst.Child as AttributedExpressionAst;
            }
            this._parser.ReportError(attribute.Extent, ParserStrings.UnexpectedAttribute, new object[] { attribute.TypeName.FullName });
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            if ((binaryExpressionAst.Operator == TokenKind.AndAnd) || (binaryExpressionAst.Operator == TokenKind.OrOr))
            {
                this._parser.ReportError(binaryExpressionAst.ErrorPosition, ParserStrings.InvalidEndOfLine, new object[] { binaryExpressionAst.Operator.Text() });
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            if (!blockStatementAst.IsInWorkflow())
            {
                this._parser.ReportError(blockStatementAst.Kind.Extent, ParserStrings.UnexpectedKeyword, new object[] { blockStatementAst.Kind.Text });
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            this.CheckForFlowOutOfFinally(breakStatementAst, GetLabel(breakStatementAst.Label));
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            this.CheckForFlowOutOfFinally(continueStatementAst, GetLabel(continueStatementAst.Label));
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            Func<Ast, bool> predicate = null;
            if (convertExpressionAst.Type.TypeName.FullName.Equals("ordered", StringComparison.OrdinalIgnoreCase) && !(convertExpressionAst.Child is HashtableAst))
            {
                this._parser.ReportError(convertExpressionAst.Extent, ParserStrings.OrderedAttributeOnlyOnHashLiteralNode, new object[] { convertExpressionAst.Type.TypeName.FullName });
            }
            if (typeof(PSReference).Equals(convertExpressionAst.Type.TypeName.GetReflectionType()))
            {
                ExpressionAst child = convertExpressionAst.Child;
                bool flag = false;
                while (true)
                {
                    AttributedExpressionAst ast2 = child as AttributedExpressionAst;
                    if (ast2 == null)
                    {
                        break;
                    }
                    ConvertExpressionAst ast3 = ast2 as ConvertExpressionAst;
                    if ((ast3 != null) && typeof(PSReference).Equals(ast3.Type.TypeName.GetReflectionType()))
                    {
                        flag = true;
                        this._parser.ReportError(ast3.Type.Extent, ParserStrings.ReferenceNeedsToBeByItselfInTypeSequence, new object[0]);
                    }
                    child = ast2.Child;
                }
                for (AttributedExpressionAst ast4 = convertExpressionAst.Parent as AttributedExpressionAst; ast4 != null; ast4 = ast4.Child as AttributedExpressionAst)
                {
                    ConvertExpressionAst ast5 = ast4 as ConvertExpressionAst;
                    if ((ast5 != null) && !flag)
                    {
                        if (typeof(PSReference).Equals(ast5.Type.TypeName.GetReflectionType()))
                        {
                            break;
                        }
                        Ast parent = ast4.Parent;
                        bool flag2 = false;
                        while (parent != null)
                        {
                            if (parent is AssignmentStatementAst)
                            {
                                if (predicate == null)
                                {
                                    predicate = ast1 => ast1 == convertExpressionAst;
                                }
                                flag2 = ((AssignmentStatementAst) parent).Left.Find(predicate, true) != null;
                                break;
                            }
                            if (parent is CommandExpressionAst)
                            {
                                break;
                            }
                            parent = parent.Parent;
                        }
                        if (!flag2)
                        {
                            this._parser.ReportError(convertExpressionAst.Type.Extent, ParserStrings.ReferenceNeedsToBeLastTypeInTypeConversion, new object[0]);
                        }
                    }
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            IEnumerable<string> allowedCommands = dataStatementAst.HasNonConstantAllowedCommand ? null : GetConstantDataStatementAllowedCommands(dataStatementAst);
            RestrictedLanguageChecker visitor = new RestrictedLanguageChecker(this._parser, allowedCommands, null, false);
            dataStatementAst.Body.InternalVisit(visitor);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            if (((forEachStatementAst.Flags & ForEachFlags.Parallel) == ForEachFlags.Parallel) && !forEachStatementAst.IsInWorkflow())
            {
                this._parser.ReportError(forEachStatementAst.Extent, ParserStrings.ParallelNotSupported, new object[0]);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if ((functionDefinitionAst.Parameters != null) && (functionDefinitionAst.Body.ParamBlock != null))
            {
                this._parser.ReportError(functionDefinitionAst.Body.ParamBlock.Extent, ParserStrings.OnlyOneParameterListAllowed, new object[0]);
            }
            else if (functionDefinitionAst.Parameters != null)
            {
                this.CheckForDuplicateParameters(functionDefinitionAst.Parameters);
            }
            if (functionDefinitionAst.IsWorkflow)
            {
                try
                {
                    foreach (ParseError error in Utils.GetAstToWorkflowConverterAndEnsureWorkflowModuleLoaded(null).ValidateAst(functionDefinitionAst))
                    {
                        this._parser.ReportError(error);
                    }
                }
                catch (NotSupportedException)
                {
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Tuple<ExpressionAst, StatementAst> tuple in hashtableAst.KeyValuePairs)
            {
                ConstantExpressionAst ast = tuple.Item1 as ConstantExpressionAst;
                if (ast != null)
                {
                    string item = ast.Value.ToString();
                    if (set.Contains(item))
                    {
                        this._parser.ReportError(tuple.Item1.Extent, ParserStrings.DuplicateKeyInHashLiteral, new object[] { item });
                    }
                    else
                    {
                        set.Add(item);
                    }
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            this.CheckForDuplicateParameters(paramBlockAst.Parameters);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            foreach (AttributeBaseAst ast in parameterAst.Attributes)
            {
                if ((ast is TypeConstraintAst) && ast.TypeName.FullName.Equals("ordered", StringComparison.OrdinalIgnoreCase))
                {
                    this._parser.ReportError(ast.Extent, ParserStrings.OrderedAttributeOnlyOnHashLiteralNode, new object[] { ast.TypeName.FullName });
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            this.CheckForFlowOutOfFinally(returnStatementAst, null);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            if (((switchStatementAst.Flags & SwitchFlags.Parallel) == SwitchFlags.Parallel) && !switchStatementAst.IsInWorkflow())
            {
                this._parser.ReportError(switchStatementAst.Extent, ParserStrings.ParallelNotSupported, new object[0]);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            if (tryStatementAst.CatchClauses.Count > 1)
            {
                for (int i = 0; i < (tryStatementAst.CatchClauses.Count - 1); i++)
                {
                    CatchClauseAst ast = tryStatementAst.CatchClauses[i];
                    for (int j = i + 1; j < tryStatementAst.CatchClauses.Count; j++)
                    {
                        CatchClauseAst ast2 = tryStatementAst.CatchClauses[j];
                        if (ast.IsCatchAll)
                        {
                            this._parser.ReportError(Parser.Before(ast2.Extent), ParserStrings.EmptyCatchNotLast, new object[0]);
                            break;
                        }
                        if (!ast2.IsCatchAll)
                        {
                            foreach (TypeConstraintAst ast3 in ast.CatchTypes)
                            {
                                Type reflectionType = ast3.TypeName.GetReflectionType();
                                if (reflectionType != null)
                                {
                                    foreach (TypeConstraintAst ast4 in ast2.CatchTypes)
                                    {
                                        Type type2 = ast4.TypeName.GetReflectionType();
                                        if ((type2 != null) && ((reflectionType == type2) || type2.IsSubclassOf(reflectionType)))
                                        {
                                            this._parser.ReportError(ast4.Extent, ParserStrings.ExceptionTypeAlreadyCaught, new object[] { type2.FullName });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            CheckArrayTypeNameDepth(typeConstraintAst.TypeName, typeConstraintAst.Extent, this._parser);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            CheckArrayTypeNameDepth(typeExpressionAst.TypeName, typeExpressionAst.Extent, this._parser);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            Action<Ast> reportError = null;
            switch (unaryExpressionAst.TokenKind)
            {
                case TokenKind.MinusMinus:
                case TokenKind.PlusPlus:
                case TokenKind.PostfixPlusPlus:
                case TokenKind.PostfixMinusMinus:
                    if (reportError == null)
                    {
                        reportError = delegate (Ast ast) {
                            this._parser.ReportError(ast.Extent, ParserStrings.OperatorRequiresVariableOrProperty, new object[] { unaryExpressionAst.TokenKind.Text() });
                        };
                    }
                    this.CheckAssignmentTarget(unaryExpressionAst.Child, false, reportError);
                    break;
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            ExpressionAst subExpression = usingExpressionAst.SubExpression;
            ExpressionAst ast2 = this.CheckUsingExpression(subExpression);
            if (ast2 != null)
            {
                this._parser.ReportError(ast2.Extent, ParserStrings.InvalidUsingExpression, new object[0]);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if ((variableExpressionAst.Splatted && !(variableExpressionAst.Parent is CommandAst)) && !(variableExpressionAst.Parent is UsingExpressionAst))
            {
                if ((variableExpressionAst.Parent is ArrayLiteralAst) && (variableExpressionAst.Parent.Parent is CommandAst))
                {
                    this._parser.ReportError(variableExpressionAst.Extent, ParserStrings.SplattingNotPermittedInArgumentList, new object[] { variableExpressionAst.VariablePath.UserPath });
                }
                else
                {
                    this._parser.ReportError(variableExpressionAst.Extent, ParserStrings.SplattingNotPermitted, new object[] { variableExpressionAst.VariablePath.UserPath });
                }
            }
            return AstVisitAction.Continue;
        }

        
    }
}

