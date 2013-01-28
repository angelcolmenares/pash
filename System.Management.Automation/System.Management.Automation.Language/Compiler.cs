namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal class Compiler : ICustomAstVisitor
    {
        private static readonly Dictionary<CallInfo, Delegate> _attributeGeneratorCache;
        private Expression<Action<FunctionContext>> _beginBlockLambda;
        private static readonly Expression _callCheckForInterrupts;
        private static readonly CatchBlock _catchFlowControl;
        private bool _compilingScriptCmdlet;
        private bool _compilingSingleExpression;
        private bool _compilingTrap;
        private static readonly Expression _currentExceptionBeingHandled;
        private string _currentFunctionName;
        private SymbolDocumentInfo _debugSymbolDocument;
        private Expression<Action<FunctionContext>> _dynamicParamBlockLambda;
        private Expression<Action<FunctionContext>> _endBlockLambda;
        internal static readonly ParameterExpression _executionContextParameter;
        private int _foreachTupleIndex;
        internal static readonly ParameterExpression _functionContext;
        private static readonly Expression _getCurrentPipe;
        private readonly List<LoopGotoTargets> _loopTargets;
        private static readonly ParameterExpression _outputPipeParameter;
        private Expression<Action<FunctionContext>> _processBlockLambda;
        private LabelTarget _returnTarget;
        private readonly List<IScriptExtent> _sequencePoints;
        private static readonly Expression _setDollarQuestionToTrue;
        internal static readonly CatchBlock[] _stmtCatchHandlers;
        private int _stmtCount;
        private int _switchTupleIndex;
        private int _tempCounter;
        private int _trapNestingCount;
        internal static readonly Dictionary<string, int> DottedLocalsNameIndexMap;
        internal static readonly Type DottedLocalsTupleType;
        internal static readonly Dictionary<string, int> DottedScriptCmdletLocalsNameIndexMap;
        internal static Type DottedScriptCmdletLocalsTupleType;
        private bool generatedCallToDefineWorkflows;

        static Compiler()
        {
            int num;
            DottedLocalsTupleType = MutableTuple.MakeTupleType(SpecialVariables.AutomaticVariableTypes);
            DottedScriptCmdletLocalsTupleType = MutableTuple.MakeTupleType(SpecialVariables.AutomaticVariableTypes.Concat<Type>(SpecialVariables.PreferenceVariableTypes).ToArray<Type>());
            DottedLocalsNameIndexMap = new Dictionary<string, int>(SpecialVariables.AutomaticVariableTypes.Length, StringComparer.OrdinalIgnoreCase);
            DottedScriptCmdletLocalsNameIndexMap = new Dictionary<string, int>(SpecialVariables.AutomaticVariableTypes.Length + SpecialVariables.PreferenceVariableTypes.Length, StringComparer.OrdinalIgnoreCase);
            _attributeGeneratorCache = new Dictionary<CallInfo, Delegate>();
            _functionContext = Expression.Parameter(typeof(FunctionContext), "funcContext");
            _executionContextParameter = Expression.Variable(typeof(System.Management.Automation.ExecutionContext), "context");
            _outputPipeParameter = Expression.Variable(typeof(Pipe), "pipe");
            _setDollarQuestionToTrue = Expression.Assign(Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_QuestionMarkVariableValue), ExpressionCache.TrueConstant);
            _callCheckForInterrupts = Expression.Call(CachedReflectionInfo.PipelineOps_CheckForInterrupts, _executionContextParameter);
            _getCurrentPipe = Expression.Field(_functionContext, CachedReflectionInfo.FunctionContext__outputPipe);
            ParameterExpression variable = Expression.Variable(typeof(Exception), "exception");
            _catchFlowControl = Expression.Catch(typeof(FlowControlException), Expression.Rethrow());
            CatchBlock block = Expression.Catch(variable, Expression.Block(new Expression[] { Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_CheckActionPreference, _functionContext, variable) }));
            _stmtCatchHandlers = new CatchBlock[] { _catchFlowControl, block };
            _currentExceptionBeingHandled = Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_CurrentExceptionBeingHandled);
            for (num = 0; num < SpecialVariables.AutomaticVariables.Length; num++)
            {
                DottedLocalsNameIndexMap.Add(SpecialVariables.AutomaticVariables[num], num);
                DottedScriptCmdletLocalsNameIndexMap.Add(SpecialVariables.AutomaticVariables[num], num);
            }
            for (num = 0; num < SpecialVariables.PreferenceVariables.Length; num++)
            {
                DottedScriptCmdletLocalsNameIndexMap.Add(SpecialVariables.PreferenceVariables[num], num + 9);
            }
        }

        internal Compiler()
        {
            this._switchTupleIndex = -1;
            this._foreachTupleIndex = -1;
            this._loopTargets = new List<LoopGotoTargets>();
            this._sequencePoints = new List<IScriptExtent>();
        }

        private Compiler(List<IScriptExtent> sequencePoints)
        {
            this._switchTupleIndex = -1;
            this._foreachTupleIndex = -1;
            this._loopTargets = new List<LoopGotoTargets>();
            this._sequencePoints = sequencePoints;
        }

        private void AddMergeRedirectionExpressions(ReadOnlyCollection<RedirectionAst> redirections, List<ParameterExpression> temps, List<Expression> exprs, List<Expression> finallyExprs)
        {
            foreach (MergingRedirectionAst ast in redirections.OfType<MergingRedirectionAst>())
            {
                ParameterExpression item = this.NewTemp(typeof(Pipe[]), "savedPipes");
                temps.Add(item);
                ConstantExpression instance = Expression.Constant(this.VisitMergingRedirection(ast));
                exprs.Add(Expression.Assign(item, Expression.Call(instance, CachedReflectionInfo.MergingRedirection_BindForExpression, _executionContextParameter, _functionContext)));
                finallyExprs.Insert(0, Expression.Call(instance.Cast(typeof(CommandRedirection)), CachedReflectionInfo.CommandRedirection_UnbindForExpression, _functionContext, item));
            }
        }

        private IEnumerable<Expression> BuildHashtable(ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>> keyValuePairs, ParameterExpression temp, bool ordered)
        {
            yield return Expression.Assign(temp, Expression.New(ordered ? CachedReflectionInfo.OrderedDictionary_ctor : CachedReflectionInfo.Hashtable_ctor, new Expression[] { ExpressionCache.Constant(keyValuePairs.Count), ExpressionCache.CurrentCultureIgnoreCaseComparer.Cast(typeof(IEqualityComparer)) }));
            foreach (Tuple<ExpressionAst, StatementAst> iteratorVariable0 in keyValuePairs)
            {
                Expression iteratorVariable1 = Expression.Convert(this.Compile(iteratorVariable0.Item1), typeof(object));
                Expression iteratorVariable2 = Expression.Convert(this.CaptureStatementResults(iteratorVariable0.Item2, CaptureAstContext.Assignment, null), typeof(object));
                Expression iteratorVariable3 = Expression.Constant(iteratorVariable0.Item1.Extent);
                yield return Expression.Call(CachedReflectionInfo.HashtableOps_AddKeyValuePair, temp, iteratorVariable1, iteratorVariable2, iteratorVariable3);
            }
            yield return temp;
        }

        internal Expression CallAddPipe(Expression expr, Expression pipe)
        {
            if (!PSEnumerableBinder.IsStaticTypePossiblyEnumerable(expr.Type))
            {
                return Expression.Call(pipe, CachedReflectionInfo.Pipe_Add, new Expression[] { expr.Cast(typeof(object)) });
            }
            return Expression.Dynamic(PSPipeWriterBinder.Get(), typeof(void), expr, pipe, _executionContextParameter);
        }

        internal static Expression CallGetVariable(Expression variablePath, VariableExpressionAst varAst)
        {
            return Expression.Call(CachedReflectionInfo.VariableOps_GetVariableValue, variablePath, _executionContextParameter, Expression.Constant(varAst).Cast(typeof(VariableExpressionAst)));
        }

        internal static Expression CallSetVariable(Expression variablePath, Expression rhs, Expression attributes = null)
        {
            return Expression.Call(CachedReflectionInfo.VariableOps_SetVariableValue, variablePath, rhs.Cast(typeof(object)), _executionContextParameter, attributes ?? ExpressionCache.NullConstant.Cast(typeof(AttributeAst[])));
        }

        internal static Expression CallStringEquals(Expression left, Expression right, bool ignoreCase)
        {
            return Expression.Call(CachedReflectionInfo.String_Equals, left, right, ignoreCase ? ExpressionCache.StringComparisonInvariantCultureIgnoreCase : ExpressionCache.StringComparisonInvariantCulture);
        }

        private Expression CaptureAstResults(Ast ast, CaptureAstContext context, MergeRedirectExprs generateRedirectExprs = null)
        {
            Expression expression;
            List<ParameterExpression> list = new List<ParameterExpression>();
            List<Expression> exprs = new List<Expression>();
            List<Expression> finallyExprs = new List<Expression>();
            ParameterExpression item = this.NewTemp(typeof(Pipe), "oldPipe");
            ParameterExpression expression3 = this.NewTemp(typeof(ArrayList), "arrayList");
            list.Add(expression3);
            list.Add(item);
            exprs.Add(Expression.Assign(item, _getCurrentPipe));
            exprs.Add(Expression.Assign(expression3, Expression.New(CachedReflectionInfo.ArrayList_ctor)));
            exprs.Add(Expression.Assign(_getCurrentPipe, Expression.New(CachedReflectionInfo.Pipe_ctor, new Expression[] { expression3 })));
            if (generateRedirectExprs != null)
            {
                generateRedirectExprs(exprs, finallyExprs);
            }
            exprs.Add(this.Compile(ast));
            switch (context)
            {
                case CaptureAstContext.Assignment:
                    expression = Expression.Call(CachedReflectionInfo.PipelineOps_PipelineResult, expression3);
                    finallyExprs.Add(Expression.Call(CachedReflectionInfo.PipelineOps_FlushPipe, item, expression3));
                    break;

                case CaptureAstContext.Condition:
                    expression = Expression.Dynamic(PSPipelineResultToBoolBinder.Get(), typeof(bool), expression3);
                    break;

                case CaptureAstContext.Enumerable:
                    expression = expression3;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("context");
            }
            finallyExprs.Add(Expression.Assign(_getCurrentPipe, item));
            exprs.Add(expression);
            return Expression.Block(list.ToArray(), new Expression[] { Expression.TryFinally(Expression.Block(exprs), Expression.Block(finallyExprs)) });
        }

        private Expression CaptureStatementResults(StatementAst stmt, CaptureAstContext context, MergeRedirectExprs generateRedirectExprs = null)
        {
            Expression right = this.CaptureStatementResultsHelper(stmt, context, generateRedirectExprs);
            if ((context == CaptureAstContext.Condition) && (AstSearcher.FindFirst(stmt, ast => ast is CommandAst, false) != null))
            {
                ParameterExpression left = this.NewTemp(right.Type, "condTmp");
                right = Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, right), _setDollarQuestionToTrue, left });
            }
            return right;
        }

        private Expression CaptureStatementResultsHelper(StatementAst stmt, CaptureAstContext context, MergeRedirectExprs generateRedirectExprs)
        {
            CommandExpressionAst commandExpr = stmt as CommandExpressionAst;
            if (commandExpr != null)
            {
                if (commandExpr.Redirections.Any<RedirectionAst>())
                {
                    return this.GetRedirectedExpression(commandExpr, true);
                }
                return this.Compile(commandExpr.Expression);
            }
            AssignmentStatementAst ast = stmt as AssignmentStatementAst;
            if (ast != null)
            {
                Expression expression = this.Compile(ast);
                if (stmt.Parent is StatementBlockAst)
                {
                    expression = Expression.Block(expression, ExpressionCache.Empty);
                }
                return expression;
            }
            PipelineAst ast3 = stmt as PipelineAst;
            if (ast3 != null)
            {
                ExpressionAst pureExpression = ast3.GetPureExpression();
                if (pureExpression != null)
                {
                    return this.Compile(pureExpression);
                }
            }
            return this.CaptureAstResults(stmt, context, generateRedirectExprs);
        }

        internal static PSMethodInvocationConstraints CombineTypeConstraintForMethodResolution(Type targetType, IEnumerable<Type> argTypes)
        {
            if ((targetType != null) || ((argTypes != null) && argTypes.Any<Type>()))
            {
                return new PSMethodInvocationConstraints(targetType, argTypes);
            }
            return null;
        }

        internal static PSMethodInvocationConstraints CombineTypeConstraintForMethodResolution(Type targetType, Type argType)
        {
            if ((targetType == null) && (argType == null))
            {
                return null;
            }
            return new PSMethodInvocationConstraints(targetType, new Type[] { argType });
        }

        internal Expression Compile(Ast ast)
        {
            return (Expression) ast.Accept(this);
        }

        internal void Compile(CompiledScriptBlockData scriptBlock, bool optimize)
        {
            IParameterMetadataProvider ast = scriptBlock.Ast;
            this.Optimize = optimize;
            this._compilingScriptCmdlet = scriptBlock.UsesCmdletBinding;
            string file = ((Ast) ast).Extent.File;
            if (file != null)
            {
                this._debugSymbolDocument = Expression.SymbolDocument(file);
            }
            Tuple<Type, Dictionary<string, int>> tuple = VariableAnalysis.Analyze(ast, !optimize, this._compilingScriptCmdlet);
            this.LocalVariablesTupleType = tuple.Item1;
            Dictionary<string, int> dictionary = tuple.Item2;
            if (!dictionary.TryGetValue("switch", out this._switchTupleIndex))
            {
                this._switchTupleIndex = -2;
            }
            if (!dictionary.TryGetValue("foreach", out this._foreachTupleIndex))
            {
                this._foreachTupleIndex = -2;
            }
            this.LocalVariablesParameter = Expression.Variable(this.LocalVariablesTupleType, "locals");
            ast.Body.Accept(this);
            if (!this._sequencePoints.Any<IScriptExtent>())
            {
                this._sequencePoints.Add(((Ast) ast).Extent);
            }
            if (optimize)
            {
                scriptBlock.DynamicParamBlockTree = this._dynamicParamBlockLambda;
                scriptBlock.BeginBlockTree = this._beginBlockLambda;
                scriptBlock.ProcessBlockTree = this._processBlockLambda;
                scriptBlock.EndBlockTree = this._endBlockLambda;
                scriptBlock.LocalsMutableTupleType = this.LocalVariablesTupleType;
                scriptBlock.NameToIndexMap = dictionary;
            }
            else
            {
                scriptBlock.UnoptimizedDynamicParamBlockTree = this._dynamicParamBlockLambda;
                scriptBlock.UnoptimizedBeginBlockTree = this._beginBlockLambda;
                scriptBlock.UnoptimizedProcessBlockTree = this._processBlockLambda;
                scriptBlock.UnoptimizedEndBlockTree = this._endBlockLambda;
                scriptBlock.UnoptimizedLocalsMutableTupleType = this.LocalVariablesTupleType;
            }
            scriptBlock.CompileInterpretDecision = (this._stmtCount > 300) ? CompileInterpretChoice.NeverCompile : CompileInterpretChoice.CompileOnDemand;
            if (scriptBlock.SequencePoints == null)
            {
                scriptBlock.SequencePoints = this._sequencePoints.ToArray();
            }
        }

        private Expression CompileAssignment(AssignmentStatementAst assignmentStatementAst, MergeRedirectExprs generateRedirectExprs = null)
        {
            ArrayLiteralAst left = assignmentStatementAst.Left as ArrayLiteralAst;
            if (assignmentStatementAst.Left is ParenExpressionAst)
            {
                left = ((ParenExpressionAst) assignmentStatementAst.Left).Pipeline.GetPureExpression() as ArrayLiteralAst;
            }
            Expression expression = this.CaptureStatementResults(assignmentStatementAst.Right, (left != null) ? CaptureAstContext.Enumerable : CaptureAstContext.Assignment, generateRedirectExprs);
            if (left != null)
            {
                expression = Expression.Dynamic(PSArrayAssignmentRHSBinder.Get(left.Elements.Count), typeof(IList), expression);
            }
            List<Expression> expressions = new List<Expression> {
                this.UpdatePosition(assignmentStatementAst),
                this.ReduceAssignment((ISupportsAssignment) assignmentStatementAst.Left, assignmentStatementAst.Operator, expression)
            };
            return Expression.Block(expressions);
        }

        internal Expression CompileExpressionOperand(ExpressionAst exprAst)
        {
            Expression expression = this.Compile(exprAst);
            if (expression.Type.Equals(typeof(void)))
            {
                expression = Expression.Block(expression, ExpressionCache.NullConstant);
            }
            return expression;
        }

        private Expression CompileIncrementOrDecrement(ExpressionAst exprAst, int valueToAdd, bool prefix)
        {
            ParameterExpression expression;
            IAssignableValue assignableValue = ((ISupportsAssignment) exprAst).GetAssignableValue();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            List<Expression> exprs = new List<Expression>();
            Expression expression2 = assignableValue.GetValue(this, exprs, temps);
            if (prefix)
            {
                DynamicExpression right = Expression.Dynamic(PSUnaryOperationBinder.Get((valueToAdd == 1) ? ExpressionType.Increment : ExpressionType.Decrement), typeof(object), expression2);
                expression = Expression.Parameter(right.Type);
                exprs.Add(Expression.Assign(expression, right));
                exprs.Add(assignableValue.SetValue(this, expression));
                exprs.Add(expression);
            }
            else
            {
                expression = Expression.Parameter(expression2.Type);
                exprs.Add(Expression.Assign(expression, expression2));
                DynamicExpression rhs = Expression.Dynamic(PSUnaryOperationBinder.Get((valueToAdd == 1) ? ExpressionType.Increment : ExpressionType.Decrement), typeof(object), expression);
                exprs.Add(assignableValue.SetValue(this, rhs));
                if (expression.Type.IsValueType)
                {
                    exprs.Add(expression);
                }
                else
                {
                    exprs.Add(Expression.Condition(Expression.Equal(expression, ExpressionCache.NullConstant), ExpressionCache.Constant(0).Cast(typeof(object)), expression));
                }
            }
            temps.Add(expression);
            return Expression.Block((IEnumerable<ParameterExpression>) temps, (IEnumerable<Expression>) exprs);
        }

        private Expression<Action<FunctionContext>> CompileNamedBlock(NamedBlockAst namedBlockAst, string funcName)
        {
            IScriptExtent entryExtent = null;
            IScriptExtent exitExtent = null;
            if (namedBlockAst.Unnamed)
            {
                ScriptBlockAst parent = (ScriptBlockAst) namedBlockAst.Parent;
                if ((parent.Parent != null) && (parent.Extent is InternalScriptExtent))
                {
                    InternalScriptExtent extent = (InternalScriptExtent) parent.Extent;
                    entryExtent = new InternalScriptExtent(extent.PositionHelper, extent.StartOffset, extent.StartOffset + 1);
                    exitExtent = new InternalScriptExtent(extent.PositionHelper, extent.EndOffset - 1, extent.EndOffset);
                }
            }
            else
            {
                entryExtent = namedBlockAst.OpenCurlyExtent;
                exitExtent = namedBlockAst.CloseCurlyExtent;
            }
            return this.CompileSingleLambda(namedBlockAst.Statements, namedBlockAst.Traps, funcName, entryExtent, exitExtent);
        }

        private Func<FunctionContext, object> CompileSingleExpression(ExpressionAst expressionAst, out IScriptExtent[] sequencePoints, out Type localsTupleType)
        {
            this.Optimize = false;
            this._compilingSingleExpression = true;
            Tuple<Type, Dictionary<string, int>> tuple = VariableAnalysis.AnalyzeExpression(expressionAst);
            this.LocalVariablesTupleType = localsTupleType = tuple.Item1;
            this.LocalVariablesParameter = Expression.Variable(this.LocalVariablesTupleType, "locals");
            this._returnTarget = Expression.Label(typeof(object), "returnTarget");
            this._loopTargets.Clear();
            List<Expression> exprs = new List<Expression>();
            this.GenerateFunctionProlog(exprs, null);
            Expression defaultValue = this.Compile(expressionAst).Cast(typeof(object));
            exprs.Add(Expression.Label(this._returnTarget, defaultValue));
            BlockExpression body = Expression.Block(new ParameterExpression[] { _executionContextParameter, _outputPipeParameter, this.LocalVariablesParameter }, exprs);
            ParameterExpression[] parameters = new ParameterExpression[] { _functionContext };
            sequencePoints = this._sequencePoints.ToArray();
            return Expression.Lambda<Func<FunctionContext, object>>(body, parameters).Compile();
        }

        private Expression<Action<FunctionContext>> CompileSingleLambda(ReadOnlyCollection<StatementAst> statements, ReadOnlyCollection<TrapStatementAst> traps, string funcName, IScriptExtent entryExtent, IScriptExtent exitExtent)
        {
            this._currentFunctionName = funcName;
            this._loopTargets.Clear();
            this._returnTarget = Expression.Label("returnTarget");
            List<Expression> exprs = new List<Expression>();
            this.GenerateFunctionProlog(exprs, entryExtent);
            List<Expression> list2 = new List<Expression>();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            this.CompileStatementListWithTraps(statements, traps, list2, temps);
            exprs.AddRange(list2);
            exprs.Add(Expression.Label(this._returnTarget));
            Version version = Environment.Version;
            if (((version.Major == 4) && (version.Minor == 0)) && ((version.Build == 0x766f) && (version.Revision < 0x40d6)))
            {
                exprs.Add(Expression.Call(CachedReflectionInfo.PipelineOps_Nop, new Expression[0]));
            }
            this.GenerateFunctionEpilog(exprs, exitExtent);
            temps.Add(_outputPipeParameter);
            temps.Add(this.LocalVariablesParameter);
            Expression body = Expression.Block((IEnumerable<ParameterExpression>) temps, (IEnumerable<Expression>) exprs);
            if (!this._compilingTrap && (((traps != null) && traps.Any<TrapStatementAst>()) || (from stmt in statements
                where AstSearcher.Contains(stmt, ast => ast is TrapStatementAst, false)
                select stmt).Any<StatementAst>()))
            {
                body = Expression.Block(new ParameterExpression[] { _executionContextParameter }, new Expression[] { Expression.TryCatchFinally(body, Expression.Call(Expression.Field(_executionContextParameter, CachedReflectionInfo.ExecutionContext_Debugger), CachedReflectionInfo.Debugger_ExitScriptFunction), new CatchBlock[] { Expression.Catch(typeof(ReturnException), ExpressionCache.Empty) }) });
            }
            else
            {
                body = Expression.Block(new ParameterExpression[] { _executionContextParameter }, new Expression[] { Expression.TryFinally(body, Expression.Call(Expression.Field(_executionContextParameter, CachedReflectionInfo.ExecutionContext_Debugger), CachedReflectionInfo.Debugger_ExitScriptFunction)) });
            }
            return Expression.Lambda<Action<FunctionContext>>(body, funcName, new ParameterExpression[] { _functionContext });
        }

        private void CompileStatementListWithTraps(ReadOnlyCollection<StatementAst> statements, ReadOnlyCollection<TrapStatementAst> traps, List<Expression> exprs, List<ParameterExpression> temps)
        {
            if (statements.Count == 0)
            {
                exprs.Add(ExpressionCache.Empty);
            }
            else
            {
                Expression expression;
                ParameterExpression expression2;
                ParameterExpression expression3;
                List<Expression> list = exprs;
                if (traps != null)
                {
                    exprs = new List<Expression>();
                    expression = Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_ExceptionHandlerInEnclosingStatementBlock);
                    expression2 = this.NewTemp(typeof(bool), "oldActiveHandler");
                    exprs.Add(Expression.Assign(expression2, expression));
                    exprs.Add(Expression.Assign(expression, ExpressionCache.Constant(true)));
                    List<Expression> initializers = new List<Expression>();
                    List<Action<FunctionContext>> list3 = new List<Action<FunctionContext>>();
                    List<Type> list4 = new List<Type>();
                    foreach (TrapStatementAst ast in traps)
                    {
                        initializers.Add((ast.TrapType != null) ? this.CompileTypeName(ast.TrapType.TypeName) : ExpressionCache.CatchAllType);
                        Tuple<Action<FunctionContext>, Type> tuple = this.CompileTrap(ast);
                        list3.Add(tuple.Item1);
                        list4.Add(tuple.Item2);
                    }
                    exprs.Add(Expression.Call(_functionContext, CachedReflectionInfo.FunctionContext_PushTrapHandlers, Expression.NewArrayInit(typeof(Type), initializers), Expression.Constant(list3.ToArray()), Expression.Constant(list4.ToArray())));
                    expression3 = this.NewTemp(typeof(bool), "trapHandlersPushed");
                    exprs.Add(Expression.Assign(expression3, ExpressionCache.Constant(true)));
                    this._trapNestingCount++;
                }
                else
                {
                    expression2 = null;
                    expression = null;
                    expression3 = null;
                    if (this._trapNestingCount > 0)
                    {
                        exprs = new List<Expression>();
                        exprs.Add(Expression.Call(_functionContext, CachedReflectionInfo.FunctionContext_PushTrapHandlers, ExpressionCache.NullTypeArray, ExpressionCache.NullDelegateArray, ExpressionCache.NullTypeArray));
                        expression3 = this.NewTemp(typeof(bool), "trapHandlersPushed");
                        exprs.Add(Expression.Assign(expression3, ExpressionCache.Constant(true)));
                    }
                }
                this._stmtCount += statements.Count;
                if (statements.Count == 1)
                {
                    List<Expression> exprList = new List<Expression>(3);
                    this.CompileTrappableExpression(exprList, statements[0]);
                    exprList.Add(ExpressionCache.Empty);
                    TryExpression item = Expression.TryCatch(Expression.Block(exprList), _stmtCatchHandlers);
                    exprs.Add(item);
                }
                else
                {
                    SwitchCase[] cases = new SwitchCase[statements.Count + 1];
                    LabelTarget[] targetArray = new LabelTarget[statements.Count + 1];
                    for (int i = 0; i <= statements.Count; i++)
                    {
                        targetArray[i] = Expression.Label();
                        cases[i] = Expression.SwitchCase(Expression.Goto(targetArray[i]), new Expression[] { ExpressionCache.Constant(i) });
                    }
                    ParameterExpression expression5 = Expression.Variable(typeof(int), "stmt");
                    temps.Add(expression5);
                    exprs.Add(Expression.Assign(expression5, ExpressionCache.Constant(0)));
                    LabelTarget target = Expression.Label();
                    exprs.Add(Expression.Label(target));
                    List<Expression> list6 = new List<Expression> {
                        Expression.Switch(expression5, cases)
                    };
                    for (int j = 0; j < statements.Count; j++)
                    {
                        list6.Add(Expression.Label(targetArray[j]));
                        list6.Add(Expression.Assign(expression5, ExpressionCache.Constant((int) (j + 1))));
                        this.CompileTrappableExpression(list6, statements[j]);
                    }
                    list6.Add(ExpressionCache.Empty);
                    ParameterExpression expression6 = Expression.Variable(typeof(Exception), "exception");
                    MethodCallExpression expression7 = Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_CheckActionPreference, _functionContext, expression6);
                    CatchBlock block = Expression.Catch(expression6, Expression.Block(expression7, Expression.Goto(target)));
                    TryExpression expression8 = Expression.TryCatch(Expression.Block(list6), new CatchBlock[] { _catchFlowControl, block });
                    exprs.Add(expression8);
                    exprs.Add(Expression.Label(targetArray[statements.Count]));
                }
                if (this._trapNestingCount > 0)
                {
                    List<ParameterExpression> variables = new List<ParameterExpression>();
                    List<Expression> expressions = new List<Expression>();
                    if (expression2 != null)
                    {
                        variables.Add(expression2);
                        expressions.Add(Expression.Assign(expression, expression2));
                    }
                    variables.Add(expression3);
                    expressions.Add(Expression.IfThen(expression3, Expression.Call(_functionContext, CachedReflectionInfo.FunctionContext_PopTrapHandlers)));
                    list.Add(Expression.Block(variables, new Expression[] { Expression.TryFinally(Expression.Block(exprs), Expression.Block(expressions)) }));
                }
                if (traps != null)
                {
                    this._trapNestingCount--;
                }
            }
        }

        private Tuple<Action<FunctionContext>, Type> CompileTrap(TrapStatementAst trap)
        {
            Compiler compiler = new Compiler(this._sequencePoints) {
                _compilingTrap = true
            };
            string funcName = this._currentFunctionName + "<trap>";
            if (trap.TrapType != null)
            {
                funcName = funcName + "<" + trap.TrapType.TypeName.Name + ">";
            }
            Tuple<Type, Dictionary<string, int>> tuple = VariableAnalysis.AnalyzeTrap(trap);
            compiler.LocalVariablesTupleType = tuple.Item1;
            compiler.LocalVariablesParameter = Expression.Variable(compiler.LocalVariablesTupleType, "locals");
            Expression<Action<FunctionContext>> expression = compiler.CompileSingleLambda(trap.Body.Statements, trap.Body.Traps, funcName, null, null);
            return Tuple.Create<Action<FunctionContext>, Type>(expression.Compile(), compiler.LocalVariablesTupleType);
        }

        private void CompileTrappableExpression(List<Expression> exprList, StatementAst stmt)
        {
            Expression item = this.Compile(stmt);
            exprList.Add(item);
            PipelineAst ast = stmt as PipelineAst;
            if (ast != null)
            {
                if ((ast.PipelineElements.Count == 1) && (ast.PipelineElements[0] is CommandExpressionAst))
                {
                    exprList.Add(_setDollarQuestionToTrue);
                }
            }
            else if (stmt is AssignmentStatementAst)
            {
                Ast right = null;
                for (AssignmentStatementAst ast3 = (AssignmentStatementAst) stmt; ast3 != null; ast3 = right as AssignmentStatementAst)
                {
                    right = ast3.Right;
                }
                ast = right as PipelineAst;
                if ((right is CommandExpressionAst) || (((ast != null) && (ast.PipelineElements.Count == 1)) && (ast.PipelineElements[0] is CommandExpressionAst)))
                {
                    exprList.Add(_setDollarQuestionToTrue);
                }
            }
        }

        internal Expression CompileTypeName(ITypeName typeName)
        {
            Type reflectionType;
            try
            {
                reflectionType = typeName.GetReflectionType();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                reflectionType = null;
            }
            if (reflectionType != null)
            {
                return Expression.Constant(reflectionType);
            }
            return Expression.Call(CachedReflectionInfo.TypeOps_ResolveTypeName, Expression.Constant(typeName));
        }

        internal static Expression ConvertValue(Expression expr, List<AttributeBaseAst> conversions)
        {
            foreach (AttributeBaseAst ast in conversions)
            {
                if (ast is TypeConstraintAst)
                {
                    expr = ConvertValue(ast.TypeName, expr);
                }
            }
            return expr;
        }

        internal static Expression ConvertValue(ITypeName typeName, Expression expr)
        {
            Type reflectionType = typeName.GetReflectionType();
            if (reflectionType == null)
            {
                return Expression.Dynamic(PSDynamicConvertBinder.Get(), typeof(object), Expression.Call(CachedReflectionInfo.TypeOps_ResolveTypeName, Expression.Constant(typeName)), expr);
            }
            if (reflectionType.Equals(typeof(void)))
            {
                return Expression.Block(typeof(void), new Expression[] { expr });
            }
            return expr.Convert(reflectionType);
        }

        internal static Expression CreateThrow(Type resultType, Type exception, params object[] exceptionArgs)
        {
            Type[] emptyTypes = Type.EmptyTypes;
            if (exceptionArgs != null)
            {
                emptyTypes = new Type[exceptionArgs.Length];
                for (int i = 0; i < exceptionArgs.Length; i++)
                {
                    emptyTypes[i] = exceptionArgs[i].GetType();
                }
            }
            return CreateThrow(resultType, exception, emptyTypes, exceptionArgs);
        }

        internal static Expression CreateThrow(Type resultType, Type exception, Type[] exceptionArgTypes, params object[] exceptionArgs)
        {
            Expression[] arguments = new Expression[exceptionArgs.Length];
            for (int i = 0; i < exceptionArgs.Length; i++)
            {
                object obj2 = exceptionArgs[i];
                arguments[i] = Expression.Constant(obj2, exceptionArgTypes[i]);
            }
            ConstructorInfo constructor = exception.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, exceptionArgTypes, null);
            if (constructor == null)
            {
                throw new PSArgumentException("Type doesn't have constructor with a given signature");
            }
            return Expression.Throw(Expression.New(constructor, arguments), resultType);
        }

        private Expression GenerateBreakOrContinue(Ast ast, ExpressionAst label, Func<LoopGotoTargets, LabelTarget> fieldSelector, Func<LabelTarget, Expression> exprGenerator, ConstructorInfo nonLocalExceptionCtor)
        {
            Expression expression2;
            Func<LoopGotoTargets, LabelTarget> selector = null;
            LabelTarget arg = null;
            Expression expr = null;
            if (label != null)
            {
                expr = this.Compile(label);
                if (this._loopTargets.Any<LoopGotoTargets>())
                {
                    Func<LoopGotoTargets, bool> predicate = null;
                    StringConstantExpressionAst labelStrAst = label as StringConstantExpressionAst;
                    if (labelStrAst != null)
                    {
                        if (predicate == null)
                        {
                            predicate = t => t.Label.Equals(labelStrAst.Value, StringComparison.OrdinalIgnoreCase);
                        }
                        if (selector == null)
                        {
                            selector = t => fieldSelector(t);
                        }
                        arg = this._loopTargets.Where<LoopGotoTargets>(predicate).Select<LoopGotoTargets, LabelTarget>(selector).LastOrDefault<LabelTarget>();
                    }
                }
            }
            else if (this._loopTargets.Any<LoopGotoTargets>())
            {
                arg = fieldSelector(this._loopTargets.Last<LoopGotoTargets>());
            }
            if (arg != null)
            {
                expression2 = exprGenerator(arg);
            }
            else
            {
                expr = expr ?? ExpressionCache.ConstEmptyString;
                expression2 = Expression.Throw(Expression.New(nonLocalExceptionCtor, new Expression[] { expr.Convert(typeof(string)) }));
            }
            return Expression.Block(this.UpdatePosition(ast), expression2);
        }

        public Expression GenerateCallContains(Expression lhs, Expression rhs, bool ignoreCase)
        {
            return Expression.Call(CachedReflectionInfo.ParserOps_ContainsOperatorCompiled, _executionContextParameter, Expression.Constant(CallSite<Func<CallSite, object, IEnumerator>>.Create(PSEnumerableBinder.Get())), Expression.Constant(CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(ExpressionType.Equal, ignoreCase, true))), lhs.Cast(typeof(object)), rhs.Cast(typeof(object)));
        }

        private Expression GenerateDoLoop(LoopStatementAst loopStatement)
        {
            int num = this._stmtCount;
            string label = loopStatement.Label;
            List<Expression> exprs = new List<Expression>();
            LabelTarget target = Expression.Label(!string.IsNullOrEmpty(label) ? label : null);
            LabelTarget continueLabel = Expression.Label(!string.IsNullOrEmpty(label) ? (label + "Continue") : "continue");
            LabelTarget breakLabel = Expression.Label(!string.IsNullOrEmpty(label) ? (label + "Break") : "break");
            EnterLoopExpression item = new EnterLoopExpression();
            exprs.Add(Expression.Label(target));
            exprs.Add(item);
            this._loopTargets.Add(new LoopGotoTargets(label ?? "", breakLabel, continueLabel));
            List<Expression> expressions = new List<Expression> {
                _callCheckForInterrupts,
                this.Compile(loopStatement.Body),
                ExpressionCache.Empty
            };
            this._loopTargets.RemoveAt(this._loopTargets.Count - 1);
            exprs.Add(Expression.TryCatch(Expression.Block(expressions), GenerateLoopBreakContinueCatchBlocks(label, breakLabel, continueLabel)));
            exprs.Add(Expression.Label(continueLabel));
            Expression expression = this.CaptureStatementResults(loopStatement.Condition, CaptureAstContext.Condition, null).Convert(typeof(bool));
            if (loopStatement is DoUntilStatementAst)
            {
                expression = Expression.Not(expression);
            }
            exprs.Add(Expression.IfThen(expression, Expression.Goto(target)));
            exprs.Add(Expression.Label(breakLabel));
            item.LoopStatementCount = this._stmtCount - num;
            return (item.Loop = new PowerShellLoopExpression(exprs));
        }

        private void GenerateFunctionEpilog(List<Expression> exprs, IScriptExtent exitExtent)
        {
            if (exitExtent != null)
            {
                exprs.Add(this.UpdatePosition(new SequencePointAst(exitExtent)));
            }
        }

        private void GenerateFunctionProlog(List<Expression> exprs, IScriptExtent entryExtent)
        {
            exprs.Add(Expression.Assign(_executionContextParameter, Expression.Field(_functionContext, CachedReflectionInfo.FunctionContext__executionContext)));
            exprs.Add(Expression.Assign(_outputPipeParameter, Expression.Field(_functionContext, CachedReflectionInfo.FunctionContext__outputPipe)));
            exprs.Add(Expression.Assign(this.LocalVariablesParameter, Expression.Field(_functionContext, CachedReflectionInfo.FunctionContext__localsTuple).Cast(this.LocalVariablesTupleType)));
            if (!this._compilingSingleExpression)
            {
                exprs.Add(Expression.Assign(Expression.Field(_functionContext, CachedReflectionInfo.FunctionContext__functionName), Expression.Constant(this._currentFunctionName)));
                if (entryExtent != null)
                {
                    this._sequencePoints.Add(entryExtent);
                    exprs.Add(new UpdatePositionExpr(entryExtent, this._sequencePoints.Count - 1, this._debugSymbolDocument, false));
                }
                exprs.Add(Expression.Call(Expression.Field(_executionContextParameter, CachedReflectionInfo.ExecutionContext_Debugger), CachedReflectionInfo.Debugger_EnterScriptFunction, new Expression[] { _functionContext }));
            }
        }

        private Expression GenerateIteratorStatement(VariablePath iteratorVariablePath, Func<Expression> generateMoveNextUpdatePosition, int iteratorTupleIndex, LabeledStatementAst stmt, Action<List<Expression>, Expression> generateBody)
        {
            List<ParameterExpression> first = new List<ParameterExpression>();
            List<Expression> expressions = new List<Expression>();
            AutomaticVarSaver saver = new AutomaticVarSaver(this, iteratorVariablePath, iteratorTupleIndex);
            bool flag = stmt is ForEachStatementAst;
            expressions.Add(saver.SaveAutomaticVar());
            ParameterExpression item = this.NewTemp(typeof(object), "enumerable");
            first.Add(item);
            if (flag)
            {
                expressions.Add(this.UpdatePosition(stmt.Condition));
            }
            expressions.Add(Expression.Assign(item, this.GetRangeEnumerator(stmt.Condition.GetPureExpression()) ?? this.CaptureStatementResults(stmt.Condition, CaptureAstContext.Enumerable, null).Convert(typeof(object))));
            ParameterExpression iteratorTemp = this.NewTemp(typeof(IEnumerator), iteratorVariablePath.UnqualifiedPath);
            first.Add(iteratorTemp);
            expressions.Add(Expression.Assign(iteratorTemp, Expression.Dynamic(PSEnumerableBinder.Get(), typeof(IEnumerator), item)));
            BinaryExpression test = flag ? Expression.AndAlso(Expression.Equal(iteratorTemp, ExpressionCache.NullConstant), Expression.NotEqual(item, ExpressionCache.NullConstant)) : Expression.Equal(iteratorTemp, ExpressionCache.NullConstant);
            BinaryExpression ifTrue = Expression.Assign(iteratorTemp, Expression.Call(Expression.NewArrayInit(typeof(object), new Expression[] { Expression.Convert(item, typeof(object)) }), CachedReflectionInfo.IEnumerable_GetEnumerator));
            expressions.Add(Expression.IfThen(test, ifTrue));
            expressions.Add(saver.SetNewValue(iteratorTemp));
            BlockExpression moveNext = Expression.Block(generateMoveNextUpdatePosition(), Expression.Call(iteratorTemp, CachedReflectionInfo.IEnumerator_MoveNext));
            Expression expression4 = this.GenerateWhileLoop(stmt.Label, () => moveNext, delegate (List<Expression> loopBody, LabelTarget breakTarget, LabelTarget continueTarget) {
                generateBody(loopBody, Expression.Property(iteratorTemp, CachedReflectionInfo.IEnumerator_Current));
            }, null);
            if (flag)
            {
                expressions.Add(Expression.IfThen(Expression.NotEqual(iteratorTemp, ExpressionCache.NullConstant), expression4));
            }
            else
            {
                expressions.Add(expression4);
            }
            return Expression.Block(first.Concat<ParameterExpression>(saver.GetTemps()), new Expression[] { Expression.TryFinally(Expression.Block(expressions), saver.RestoreAutomaticVar()) });
        }

        private static CatchBlock[] GenerateLoopBreakContinueCatchBlocks(string label, LabelTarget breakLabel, LabelTarget continueLabel)
        {
            ParameterExpression variable = Expression.Parameter(typeof(BreakException));
            ParameterExpression expression2 = Expression.Parameter(typeof(ContinueException));
            CatchBlock[] blockArray = new CatchBlock[2];
            Expression[] arguments = new Expression[] { Expression.Constant(label ?? "", typeof(string)) };
            blockArray[0] = Expression.Catch(variable, Expression.IfThenElse(Expression.Call(variable, CachedReflectionInfo.LoopFlowException_MatchLabel, arguments), Expression.Break(breakLabel), Expression.Rethrow()));
            Expression[] expressionArray2 = new Expression[] { Expression.Constant(label ?? "", typeof(string)) };
            blockArray[1] = Expression.Catch(expression2, Expression.IfThenElse(Expression.Call(expression2, CachedReflectionInfo.LoopFlowException_MatchLabel, expressionArray2), Expression.Continue(continueLabel), Expression.Rethrow()));
            return blockArray;
        }

        private Expression GenerateWhileLoop(string loopLabel, Func<Expression> generateCondition, Action<List<Expression>, LabelTarget, LabelTarget> generateLoopBody, PipelineBaseAst continueAst = null)
        {
            string str;
            string str1;
            string str2;
            LabelTarget labelTarget;
            int num = this._stmtCount;
            List<Expression> expressions = new List<Expression>();
            if (!string.IsNullOrEmpty(loopLabel))
            {
                str = string.Concat(loopLabel, "Continue");
            }
            else
            {
                str = "continue";
            }
            LabelTarget labelTarget1 = Expression.Label(str);
            if (!string.IsNullOrEmpty(loopLabel))
            {
                str1 = string.Concat(loopLabel, "Break");
            }
            else
            {
                str1 = "break";
            }
            LabelTarget labelTarget2 = Expression.Label(str1);
            EnterLoopExpression enterLoopExpression = new EnterLoopExpression();
            if (continueAst != null)
            {
                if (!string.IsNullOrEmpty(loopLabel))
                {
                    str2 = string.Concat(loopLabel, "LoopTop");
                }
                else
                {
                    str2 = "looptop";
                }
                labelTarget = Expression.Label(str2);
            }
            else
            {
                labelTarget = labelTarget1;
            }
            LabelTarget labelTarget3 = labelTarget;
            expressions.Add(Expression.Label(labelTarget3));
            expressions.Add(enterLoopExpression);
            List<Expression> expressions1 = new List<Expression>();
            expressions1.Add(Compiler._callCheckForInterrupts);
            List<Compiler.LoopGotoTargets> loopGotoTargets = this._loopTargets;
            string str3 = loopLabel;
            string str4 = str3;
            if (str3 == null)
            {
                str4 = "";
            }
            loopGotoTargets.Add(new Compiler.LoopGotoTargets(str4, labelTarget2, labelTarget1));
            generateLoopBody(expressions1, labelTarget2, labelTarget1);
            if (continueAst == null)
            {
                expressions1.Add(Expression.Goto(labelTarget3));
            }
            this._loopTargets.RemoveAt(this._loopTargets.Count - 1);
            Expression expression = Expression.TryCatch(Expression.Block(expressions1), Compiler.GenerateLoopBreakContinueCatchBlocks(loopLabel, labelTarget2, labelTarget1));
            if (continueAst != null)
            {
                List<Expression> expressions2 = new List<Expression>();
                expressions2.Add(expression);
                expressions2.Add(Expression.Label(labelTarget1));
                if (continueAst.GetPureExpression() != null)
                {
                    expressions2.Add(this.UpdatePosition(continueAst));
                }
                expressions2.Add(this.CaptureStatementResults(continueAst, Compiler.CaptureAstContext.Assignment, null));
                expressions2.Add(Expression.Goto(labelTarget3));
                expression = Expression.Block(expressions2);
            }
            if (generateCondition == null)
            {
                expressions.Add(expression);
            }
            else
            {
                expressions.Add(Expression.IfThen(generateCondition().Convert(typeof(bool)), expression));
            }
            expressions.Add(Expression.Label(labelTarget2));
            enterLoopExpression.LoopStatementCount = this._stmtCount - num;
            PowerShellLoopExpression powerShellLoopExpression = new PowerShellLoopExpression(expressions);
            PowerShellLoopExpression powerShellLoopExpression1 = powerShellLoopExpression;
            enterLoopExpression.Loop = powerShellLoopExpression;
            return powerShellLoopExpression1;
        }

        internal static Attribute GetAttribute(AttributeAst attributeAst)
        {
            Attribute attribute;
            int count = attributeAst.PositionalArguments.Count;
            IEnumerable<string> source = from name in attributeAst.NamedArguments select name.ArgumentName;
            int argCount = count + source.Count<string>();
            CallInfo callInfo = new CallInfo(argCount, source);
            object[] args = new object[argCount + 1];
            Type reflectionAttributeType = attributeAst.TypeName.GetReflectionAttributeType();
            if (reflectionAttributeType == null)
            {
                throw InterpreterError.NewInterpreterException(attributeAst, typeof(RuntimeException), attributeAst.Extent, "CustomAttributeTypeNotFound", ParserStrings.CustomAttributeTypeNotFound, new object[] { attributeAst.TypeName.FullName });
            }
            args[0] = reflectionAttributeType;
            ConstantValueVisitor visitor = new ConstantValueVisitor {
                AttributeArgument = true
            };
            int num3 = 1;
            foreach (ExpressionAst ast in attributeAst.PositionalArguments)
            {
                args[num3++] = ast.Accept(visitor);
            }
            foreach (NamedAttributeArgumentAst ast2 in attributeAst.NamedArguments)
            {
                args[num3++] = ast2.Argument.Accept(visitor);
            }
            try
            {
                attribute = (Attribute) GetAttributeGenerator(callInfo).DynamicInvoke(args);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = exception.InnerException;
                RuntimeException exception3 = innerException as RuntimeException;
                if (exception3 == null)
                {
                    exception3 = InterpreterError.NewInterpreterExceptionWithInnerException(null, typeof(RuntimeException), attributeAst.Extent, "ExceptionConstructingAttribute", ExtendedTypeSystem.ExceptionConstructingAttribute, innerException, new object[] { innerException.Message, attributeAst.TypeName.FullName });
                }
                InterpreterError.UpdateExceptionErrorRecordPosition(exception3, attributeAst.Extent);
                throw exception3;
            }
            return attribute;
        }

        internal static Attribute GetAttribute(TypeConstraintAst typeConstraintAst)
        {
            Type type = TypeOps.ResolveTypeName(typeConstraintAst.TypeName);
            return new ArgumentTypeConverterAttribute(new Type[] { type });
        }

        private static Delegate GetAttributeGenerator(CallInfo callInfo)
        {
            Delegate delegate2;
            lock (_attributeGeneratorCache)
            {
                if (_attributeGeneratorCache.TryGetValue(callInfo, out delegate2))
                {
                    return delegate2;
                }
                PSAttributeGenerator binder = PSAttributeGenerator.Get(callInfo);
                ParameterExpression[] parameters = new ParameterExpression[callInfo.ArgumentCount + 1];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = Expression.Variable(typeof(object));
                }
                delegate2 = Expression.Lambda(Expression.Dynamic(binder, typeof(object), parameters), parameters).Compile();
                _attributeGeneratorCache.Add(callInfo, delegate2);
            }
            return delegate2;
        }

        internal Expression GetAutomaticVariable(VariableExpressionAst varAst)
        {
            int tupleIndex = varAst.TupleIndex;
            Expression local = this.GetLocal(tupleIndex);
            Expression ifFalse = Expression.Call(CachedReflectionInfo.VariableOps_GetAutomaticVariableValue, ExpressionCache.Constant(tupleIndex), _executionContextParameter, Expression.Constant(varAst)).Convert(local.Type);
            if (!this.Optimize)
            {
                return ifFalse;
            }
            return Expression.Condition(Expression.Call(this.LocalVariablesParameter, CachedReflectionInfo.MutableTuple_IsValueSet, new Expression[] { ExpressionCache.Constant(tupleIndex) }), local, ifFalse);
        }

        private Expression GetCommandArgumentExpression(CommandElementAst element)
        {
            ConstantExpressionAst ast = element as ConstantExpressionAst;
            if ((ast != null) && LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(ast.StaticType)))
            {
                string text = ast.Extent.Text;
                if (!text.Equals(ast.Value.ToString(), StringComparison.Ordinal))
                {
                    return Expression.Constant(ParserOps.WrappedNumber(ast.Value, text));
                }
            }
            Expression expression = this.Compile(element);
            if (expression.Type.Equals(typeof(object[])))
            {
                return Expression.Call(CachedReflectionInfo.PipelineOps_CheckAutomationNullInCommandArgumentArray, expression);
            }
            if ((ast == null) && expression.Type.Equals(typeof(object)))
            {
                expression = Expression.Call(CachedReflectionInfo.PipelineOps_CheckAutomationNullInCommandArgument, expression);
            }
            return expression;
        }

        private object GetCommandRedirections(CommandBaseAst command)
        {
            int count = command.Redirections.Count;
            if (count == 0)
            {
                return null;
            }
            object[] array = new object[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = command.Redirections[i].Accept(this);
            }
            if ((from r in array
                where r is Expression
                select r).Any<object>())
            {
                return Expression.NewArrayInit(typeof(CommandRedirection), (IEnumerable<Expression>) (from r in array select (r as Expression) ?? Expression.Constant(r)));
            }
            return Array.ConvertAll<object, CommandRedirection>(array, r => (CommandRedirection) r);
        }

        internal static object GetExpressionValue(ExpressionAst expressionAst, System.Management.Automation.ExecutionContext context, IList usingValues = null)
        {
            return GetExpressionValue(expressionAst, context, null, usingValues);
        }

        internal static object GetExpressionValue(ExpressionAst expressionAst, System.Management.Automation.ExecutionContext context, SessionStateInternal sessionStateInternal, IList usingValues = null)
        {
            Func<FunctionContext, object> lambda = null;
            IScriptExtent[] sequencePoints = null;
            Type localsTupleType = null;
            return GetExpressionValue(expressionAst, context, sessionStateInternal, usingValues, ref lambda, ref sequencePoints, ref localsTupleType);
        }

        private static object GetExpressionValue(ExpressionAst expressionAst, System.Management.Automation.ExecutionContext context, SessionStateInternal sessionStateInternal, IList usingValues, ref Func<FunctionContext, object> lambda, ref IScriptExtent[] sequencePoints, ref Type localsTupleType)
        {
            object obj2;
            object obj4;
            if (IsConstantValueVisitor.IsConstant(expressionAst, out obj2, false, false))
            {
                return obj2;
            }
            VariableExpressionAst varAst = expressionAst as VariableExpressionAst;
            if (varAst != null)
            {
                return VariableOps.GetVariableValue(varAst.VariablePath, context, varAst);
            }
            if (lambda == null)
            {
                lambda = new Compiler().CompileSingleExpression(expressionAst, out sequencePoints, out localsTupleType);
            }
            SessionStateInternal engineSessionState = context.EngineSessionState;
            try
            {
                if ((sessionStateInternal != null) && (context.EngineSessionState != sessionStateInternal))
                {
                    context.EngineSessionState = sessionStateInternal;
                }
                ArrayList resultList = new ArrayList();
                Pipe pipe = new Pipe(resultList);
                try
                {
                    FunctionContext arg = new FunctionContext {
                        _sequencePoints = sequencePoints,
                        _executionContext = context,
                        _outputPipe = pipe,
                        _localsTuple = MutableTuple.MakeTuple(localsTupleType, DottedLocalsNameIndexMap)
                    };
                    if (usingValues != null)
                    {
                        PSBoundParametersDictionary dictionary = new PSBoundParametersDictionary {
                            ImplicitUsingParameters = usingValues
                        };
                        arg._localsTuple.SetAutomaticVariable(AutomaticVariable.PSBoundParameters, dictionary, context);
                    }
                    object obj3 = lambda(arg);
                    if (obj3 == AutomationNull.Value)
                    {
                        return ((resultList.Count == 0) ? null : PipelineOps.PipelineResult(resultList));
                    }
                    obj4 = obj3;
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            catch (TerminateException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                obj4 = null;
            }
            finally
            {
                context.EngineSessionState = engineSessionState;
            }
            return obj4;
        }

        internal static PSMethodInvocationConstraints GetInvokeMemberConstraints(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            ReadOnlyCollection<ExpressionAst> arguments = invokeMemberExpressionAst.Arguments;
            return CombineTypeConstraintForMethodResolution(GetTypeConstraintForMethodResolution(invokeMemberExpressionAst.Expression), (arguments != null) ? arguments.Select<ExpressionAst, Type>(new Func<ExpressionAst, Type>(Compiler.GetTypeConstraintForMethodResolution)) : null);
        }

        internal Expression GetLocal(int tupleIndex)
        {
            Expression localVariablesParameter = this.LocalVariablesParameter;
            foreach (PropertyInfo info in MutableTuple.GetAccessPath(this.LocalVariablesTupleType, tupleIndex))
            {
                localVariablesParameter = Expression.Property(localVariablesParameter, info);
            }
            return localVariablesParameter;
        }

        internal static RuntimeDefinedParameterDictionary GetParameterMetaData(ReadOnlyCollection<ParameterAst> parameters, bool automaticPositions, ref bool usesCmdletBinding)
        {
            RuntimeDefinedParameterDictionary dictionary = new RuntimeDefinedParameterDictionary();
            List<RuntimeDefinedParameter> list = new List<RuntimeDefinedParameter>();
            bool customParameterSet = false;
            foreach (ParameterAst ast in parameters)
            {
                RuntimeDefinedParameter item = GetRuntimeDefinedParameter(ast, ref customParameterSet, ref usesCmdletBinding);
                list.Add(item);
                dictionary.Add(ast.Name.VariablePath.UserPath, item);
            }
            int num = 0;
            if (automaticPositions && !customParameterSet)
            {
                foreach (RuntimeDefinedParameter parameter2 in list)
                {
                    ParameterAttribute attribute = (ParameterAttribute) (from attr in parameter2.Attributes
                        where attr is ParameterAttribute
                        select attr).First<Attribute>();
                    if (!parameter2.ParameterType.Equals(typeof(SwitchParameter)))
                    {
                        attribute.Position = num++;
                    }
                }
            }
            dictionary.Data = list.ToArray();
            return dictionary;
        }

        private Expression GetRangeEnumerator(ExpressionAst condExpr)
        {
            Expression expression = null;
            if (condExpr != null)
            {
                BinaryExpressionAst ast = condExpr as BinaryExpressionAst;
                if ((ast != null) && (ast.Operator == TokenKind.DotDot))
                {
                    Expression expr = this.Compile(ast.Left);
                    Expression expression3 = this.Compile(ast.Right);
                    expression = Expression.New(CachedReflectionInfo.RangeEnumerator_ctor, new Expression[] { expr.Convert(typeof(int)), expression3.Convert(typeof(int)) });
                }
            }
            return expression;
        }

        private Expression GetRedirectedExpression(CommandExpressionAst commandExpr, bool captureForInput)
        {
            MergeRedirectExprs generateRedirectExprs = null;
            MergeRedirectExprs exprs2 = null;
            List<Expression> exprs = new List<Expression>();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            List<Expression> finallyExprs = new List<Expression>();
            if (!captureForInput)
            {
                exprs.Add(this.UpdatePosition(commandExpr));
            }
            bool flag = commandExpr.Redirections.Where<RedirectionAst>(delegate (RedirectionAst r) {
                if (!(r is FileRedirectionAst))
                {
                    return false;
                }
                if (r.FromStream != RedirectionStream.Output)
                {
                    return (r.FromStream == RedirectionStream.All);
                }
                return true;
            }).Any<RedirectionAst>();
            ParameterExpression item = null;
            SubExpressionAst expression = commandExpr.Expression as SubExpressionAst;
            if ((expression != null) && captureForInput)
            {
                ParameterExpression expression2 = this.NewTemp(typeof(Pipe), "oldPipe");
                item = this.NewTemp(typeof(ArrayList), "arrayList");
                temps.Add(item);
                temps.Add(expression2);
                exprs.Add(Expression.Assign(expression2, _getCurrentPipe));
                exprs.Add(Expression.Assign(item, Expression.New(CachedReflectionInfo.ArrayList_ctor)));
                exprs.Add(Expression.Assign(_getCurrentPipe, Expression.New(CachedReflectionInfo.Pipe_ctor, new Expression[] { item })));
                finallyExprs.Add(Expression.Assign(_getCurrentPipe, expression2));
            }
            foreach (FileRedirectionAst ast2 in commandExpr.Redirections.OfType<FileRedirectionAst>())
            {
                object obj2 = this.VisitFileRedirection(ast2);
                ParameterExpression expression3 = this.NewTemp(typeof(Pipe[]), "savedPipes");
                temps.Add(expression3);
                ParameterExpression expression4 = this.NewTemp(typeof(FileRedirection), "fileRedirection");
                temps.Add(expression4);
                exprs.Add(Expression.Assign(expression4, (Expression) obj2));
                exprs.Add(Expression.Assign(expression3, Expression.Call(expression4, CachedReflectionInfo.FileRedirection_BindForExpression, new Expression[] { _functionContext })));
                finallyExprs.Add(Expression.Call(expression4.Cast(typeof(CommandRedirection)), CachedReflectionInfo.CommandRedirection_UnbindForExpression, _functionContext, expression3));
                finallyExprs.Add(Expression.Call(expression4, CachedReflectionInfo.FileRedirection_Dispose));
            }
            Expression expression5 = null;
            ParenExpressionAst ast3 = commandExpr.Expression as ParenExpressionAst;
            if (ast3 != null)
            {
                AssignmentStatementAst pipeline = ast3.Pipeline as AssignmentStatementAst;
                if (pipeline != null)
                {
                    if (generateRedirectExprs == null)
                    {
                        generateRedirectExprs = delegate (List<Expression> mergeExprs, List<Expression> mergeFinallyExprs) {
                            this.AddMergeRedirectionExpressions(commandExpr.Redirections, temps, mergeExprs, mergeFinallyExprs);
                        };
                    }
                    expression5 = this.CompileAssignment(pipeline, generateRedirectExprs);
                }
                else
                {
                    if (exprs2 == null)
                    {
                        exprs2 = delegate (List<Expression> mergeExprs, List<Expression> mergeFinallyExprs) {
                            this.AddMergeRedirectionExpressions(commandExpr.Redirections, temps, mergeExprs, mergeFinallyExprs);
                        };
                    }
                    expression5 = this.CaptureAstResults(ast3.Pipeline, CaptureAstContext.Assignment, exprs2);
                }
            }
            else if (expression != null)
            {
                this.AddMergeRedirectionExpressions(commandExpr.Redirections, temps, exprs, finallyExprs);
                exprs.Add(this.Compile(expression.SubExpression));
                if (item != null)
                {
                    expression5 = Expression.Call(CachedReflectionInfo.PipelineOps_PipelineResult, item);
                }
            }
            else
            {
                this.AddMergeRedirectionExpressions(commandExpr.Redirections, temps, exprs, finallyExprs);
                expression5 = this.Compile(commandExpr.Expression);
            }
            if (expression5 != null)
            {
                if (!flag && captureForInput)
                {
                    exprs.Add(expression5);
                }
                else
                {
                    exprs.Add(this.CallAddPipe(expression5, _getCurrentPipe));
                    exprs.Add(ExpressionCache.AutomationNullConstant);
                }
            }
            if (finallyExprs.Count != 0)
            {
                return Expression.Block(temps.ToArray(), new Expression[] { Expression.TryFinally(Expression.Block(exprs), Expression.Block(finallyExprs)) });
            }
            return Expression.Block(temps.ToArray(), exprs);
        }

        private static RuntimeDefinedParameter GetRuntimeDefinedParameter(ParameterAst parameterAst, ref bool customParameterSet, ref bool usesCmdletBinding)
        {
            object obj3;
            List<Attribute> list = new List<Attribute>();
            bool flag = false;
            foreach (AttributeBaseAst ast in parameterAst.Attributes)
            {
                Attribute item = ast.GetAttribute();
                list.Add(item);
                ParameterAttribute attribute2 = item as ParameterAttribute;
                if (attribute2 != null)
                {
                    flag = true;
                    usesCmdletBinding = true;
                    if ((attribute2.Position != -2147483648) || !attribute2.ParameterSetName.Equals("__AllParameterSets", StringComparison.OrdinalIgnoreCase))
                    {
                        customParameterSet = true;
                    }
                }
            }
            list.Reverse();
            if (!flag)
            {
                list.Insert(0, new ParameterAttribute());
            }
            RuntimeDefinedParameter parameter = new RuntimeDefinedParameter(parameterAst.Name.VariablePath.UserPath, parameterAst.StaticType, new Collection<Attribute>(list.ToArray()));
            if (parameterAst.DefaultValue != null)
            {
                object obj2;
                if (IsConstantValueVisitor.IsConstant(parameterAst.DefaultValue, out obj2, false, false))
                {
                    parameter.Value = obj2;
                    return parameter;
                }
                DefaultValueExpressionWrapper wrapper = new DefaultValueExpressionWrapper {
                    Expression = parameterAst.DefaultValue
                };
                parameter.Value = wrapper;
                return parameter;
            }
            if (TryGetDefaultParameterValue(parameterAst.StaticType, out obj3) && (obj3 != null))
            {
                parameter.Value = obj3;
            }
            return parameter;
        }

        private Action<List<Expression>, Expression> GetSwitchBodyGenerator(SwitchStatementAst switchStatementAst, AutomaticVarSaver avs, ParameterExpression skipDefault)
        {
            return delegate (List<Expression> exprs, Expression newValue) {
                PSSwitchClauseEvalBinder binder = PSSwitchClauseEvalBinder.Get(switchStatementAst.Flags);
                exprs.Add(avs.SetNewValue(newValue));
                if (skipDefault != null)
                {
                    exprs.Add(Expression.Assign(skipDefault, ExpressionCache.Constant(false)));
                }
                IsConstantValueVisitor visitor = new IsConstantValueVisitor();
                ConstantValueVisitor visitor2 = new ConstantValueVisitor();
                int count = switchStatementAst.Clauses.Count;
                for (int j = 0; j < count; j++)
                {
                    Expression expression;
                    Tuple<ExpressionAst, StatementBlockAst> tuple = switchStatementAst.Clauses[j];
                    object obj2 = ((bool) tuple.Item1.Accept(visitor)) ? tuple.Item1.Accept(visitor2) : null;
                    if (obj2 is ScriptBlock)
                    {
                        MethodCallExpression expression2 = Expression.Call(Expression.Constant(obj2), CachedReflectionInfo.ScriptBlock_DoInvokeReturnAsIs, new Expression[] { ExpressionCache.Constant(true), Expression.Constant(ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe), this.GetLocal(0).Convert(typeof(object)), ExpressionCache.AutomationNullConstant, ExpressionCache.AutomationNullConstant, ExpressionCache.NullObjectArray });
                        expression = Expression.Dynamic(PSConvertBinder.Get(typeof(bool)), typeof(bool), expression2);
                    }
                    else if (obj2 != null)
                    {
                        SwitchFlags flags = switchStatementAst.Flags;
                        Expression expression3 = Expression.Constant(((obj2 is Regex) || (obj2 is WildcardPattern)) ? obj2 : obj2.ToString());
                        Expression expression4 = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), this.GetLocal(0), _executionContextParameter);
                        if (((flags & SwitchFlags.Regex) != SwitchFlags.None) || (obj2 is Regex))
                        {
                            expression = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedRegex, ExpressionCache.Constant((flags & SwitchFlags.CaseSensitive) != SwitchFlags.None), expression3, Expression.Constant(tuple.Item1.Extent), expression4, _executionContextParameter);
                        }
                        else if (((flags & SwitchFlags.Wildcard) != SwitchFlags.None) || (obj2 is WildcardPattern))
                        {
                            expression = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedWildcard, ExpressionCache.Constant((flags & SwitchFlags.CaseSensitive) != SwitchFlags.None), expression3, expression4, _executionContextParameter);
                        }
                        else
                        {
                            expression = CallStringEquals(expression3, expression4, (flags & SwitchFlags.CaseSensitive) == SwitchFlags.None);
                        }
                    }
                    else
                    {
                        Expression expression5 = this.Compile(tuple.Item1);
                        expression = Expression.Dynamic(binder, typeof(bool), expression5, this.GetLocal(0), _executionContextParameter);
                    }
                    exprs.Add(this.UpdatePosition(tuple.Item1));
                    if (skipDefault != null)
                    {
                        exprs.Add(Expression.IfThen(expression, Expression.Block(this.Compile(tuple.Item2), Expression.Assign(skipDefault, ExpressionCache.Constant(true)))));
                    }
                    else
                    {
                        exprs.Add(Expression.IfThen(expression, this.Compile(tuple.Item2)));
                    }
                }
                if (skipDefault != null)
                {
                    exprs.Add(Expression.IfThen(Expression.Not(skipDefault), this.Compile(switchStatementAst.Default)));
                }
            };
        }

        internal static Type GetTypeConstraintForMethodResolution(ExpressionAst expr)
        {
            while (expr is ParenExpressionAst)
            {
                expr = ((ParenExpressionAst) expr).Pipeline.GetPureExpression();
            }
            ConvertExpressionAst ast = null;
            while (expr is AttributedExpressionAst)
            {
                if ((expr is ConvertExpressionAst) && !((ConvertExpressionAst) expr).IsRef())
                {
                    ast = (ConvertExpressionAst) expr;
                    break;
                }
                expr = ((AttributedExpressionAst) expr).Child;
            }
            if (ast != null)
            {
                return ast.Type.TypeName.GetReflectionType();
            }
            return null;
        }

        internal static Expression InvokeMember(string name, PSMethodInvocationConstraints constraints, Expression target, IEnumerable<Expression> args, bool @static, bool propertySet)
        {
            return Expression.Dynamic(PSInvokeMemberBinder.Get(name, new CallInfo(args.Count<Expression>(), new string[0]), @static, propertySet, constraints), typeof(object), args.Prepend<Expression>(target));
        }

        internal static Expression IsStrictMode(int version, Expression executionContext = null)
        {
            if (executionContext == null)
            {
                executionContext = ExpressionCache.NullExecutionContext;
            }
            return Expression.Call(CachedReflectionInfo.ExecutionContext_IsStrictVersion, executionContext, ExpressionCache.Constant(version));
        }

        internal ParameterExpression NewTemp(Type type, string name)
        {
            return Expression.Variable(type, string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { name, this._tempCounter++ }));
        }

        internal Expression ReduceAssignment(ISupportsAssignment left, TokenKind tokenKind, Expression right)
        {
            IAssignableValue assignableValue = left.GetAssignableValue();
            ExpressionType extension = ExpressionType.Extension;
            switch (tokenKind)
            {
                case TokenKind.Equals:
                    return assignableValue.SetValue(this, right);

                case TokenKind.PlusEquals:
                    extension = ExpressionType.Add;
                    break;

                case TokenKind.MinusEquals:
                    extension = ExpressionType.Subtract;
                    break;

                case TokenKind.MultiplyEquals:
                    extension = ExpressionType.Multiply;
                    break;

                case TokenKind.DivideEquals:
                    extension = ExpressionType.Divide;
                    break;

                case TokenKind.RemainderEquals:
                    extension = ExpressionType.Modulo;
                    break;
            }
            List<Expression> exprs = new List<Expression>();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            Expression expression = assignableValue.GetValue(this, exprs, temps);
            exprs.Add(assignableValue.SetValue(this, Expression.Dynamic(PSBinaryOperationBinder.Get(extension, true, false), typeof(object), expression, right)));
            return Expression.Block((IEnumerable<ParameterExpression>) temps, (IEnumerable<Expression>) exprs);
        }

        internal static Expression ThrowRuntimeError(string errorID, string resourceString, params Expression[] exceptionArgs)
        {
            return ThrowRuntimeError(errorID, resourceString, typeof(object), exceptionArgs);
        }

        internal static Expression ThrowRuntimeError(string errorID, string resourceString, Type throwResultType, params Expression[] exceptionArgs)
        {
            return ThrowRuntimeError(typeof(RuntimeException), errorID, resourceString, throwResultType, exceptionArgs);
        }

        internal static Expression ThrowRuntimeError(Type exceptionType, string errorID, string resourceString, Type throwResultType, params Expression[] exceptionArgs)
        {
            Expression expression = (exceptionArgs != null) ? Expression.NewArrayInit(typeof(object), (IEnumerable<Expression>) (from e in exceptionArgs select e.Cast(typeof(object)))) : ExpressionCache.NullConstant;
            Expression[] arguments = new Expression[] { ExpressionCache.NullConstant, Expression.Constant(exceptionType), ExpressionCache.NullExtent, Expression.Constant(errorID), Expression.Constant(resourceString), expression };
            return Expression.Throw(Expression.Call(CachedReflectionInfo.InterpreterError_NewInterpreterException, arguments), throwResultType);
        }

        internal static Expression ThrowRuntimeErrorWithInnerException(string errorID, string resourceString, Expression innerException, params Expression[] exceptionArgs)
        {
            return ThrowRuntimeErrorWithInnerException(errorID, Expression.Constant(resourceString), innerException, typeof(object), exceptionArgs);
        }

        internal static Expression ThrowRuntimeErrorWithInnerException(string errorID, Expression resourceString, Expression innerException, Type throwResultType, params Expression[] exceptionArgs)
        {
            Expression expression = (exceptionArgs != null) ? Expression.NewArrayInit(typeof(object), exceptionArgs) : ExpressionCache.NullConstant;
            Expression[] arguments = new Expression[] { ExpressionCache.NullConstant, Expression.Constant(typeof(RuntimeException)), ExpressionCache.NullExtent, Expression.Constant(errorID), resourceString, innerException, expression };
            return Expression.Throw(Expression.Call(CachedReflectionInfo.InterpreterError_NewInterpreterExceptionWithInnerException, arguments), throwResultType);
        }

        internal static bool TryGetDefaultParameterValue(Type type, out object value)
        {
            if (type.Equals(typeof(string)))
            {
                value = string.Empty;
                return true;
            }
            if (type.IsClass)
            {
                value = null;
                return true;
            }
            if (type.Equals(typeof(bool)))
            {
                value = Boxed.False;
                return true;
            }
            if (type.Equals(typeof(SwitchParameter)))
            {
                value = new SwitchParameter(false);
                return true;
            }
            if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(type)))
            {
                value = 0;
                return true;
            }
            value = null;
            return false;
        }

        internal Expression UpdatePosition(Ast ast)
        {
            this._sequencePoints.Add(ast.Extent);
            if (this._sequencePoints.Count != 1)
            {
                return new UpdatePositionExpr(ast.Extent, this._sequencePoints.Count - 1, this._debugSymbolDocument, !this._compilingSingleExpression);
            }
            return ExpressionCache.Empty;
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            Expression instance = this.CaptureAstResults(arrayExpressionAst.SubExpression, CaptureAstContext.Enumerable, null);
            if (instance.Type.IsArray)
            {
                return instance;
            }
            if (instance.Type.Equals(typeof(ArrayList)))
            {
                return Expression.Call(instance, CachedReflectionInfo.ArrayList_ToArray);
            }
            if (instance.Type.Equals(typeof(object[])))
            {
                return instance;
            }
            if (instance.Type.IsPrimitive)
            {
                return Expression.NewArrayInit(typeof(object), new Expression[] { instance.Cast(typeof(object)) });
            }
            if (instance.Type.Equals(typeof(void)))
            {
                return Expression.NewArrayInit(typeof(object), new Expression[0]);
            }
            return Expression.Dynamic(PSToObjectArrayBinder.Get(), typeof(object[]), instance);
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return Expression.NewArrayInit(typeof(object), (IEnumerable<Expression>) (from elem in arrayLiteralAst.Elements select this.Compile(elem).Cast(typeof(object))));
        }

        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            return this.CompileAssignment(assignmentStatementAst, null);
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            return null;
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            return attributedExpressionAst.Child.Accept(this);
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            object obj2;
            if (!this.CompilingConstantExpression && IsConstantValueVisitor.IsConstant(binaryExpressionAst, out obj2, false, false))
            {
                return Expression.Constant(obj2);
            }
            Expression expr = this.CompileExpressionOperand(binaryExpressionAst.Left);
            Expression expression2 = this.CompileExpressionOperand(binaryExpressionAst.Right);
            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.DotDot:
                    return Expression.Call(CachedReflectionInfo.IntOps_Range, expr.Convert(typeof(int)), expression2.Convert(typeof(int)));

                case TokenKind.Multiply:
                    if (!expr.Type.Equals(typeof(double)) || !expression2.Type.Equals(typeof(double)))
                    {
                        return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Multiply, true, false), typeof(object), expr, expression2);
                    }
                    return Expression.Multiply(expr, expression2);

                case TokenKind.Divide:
                    if (!expr.Type.Equals(typeof(double)) || !expression2.Type.Equals(typeof(double)))
                    {
                        return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Divide, true, false), typeof(object), expr, expression2);
                    }
                    return Expression.Divide(expr, expression2);

                case TokenKind.Rem:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Modulo, true, false), typeof(object), expr, expression2);

                case TokenKind.Plus:
                    if (!expr.Type.Equals(typeof(double)) || !expression2.Type.Equals(typeof(double)))
                    {
                        return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Add, true, false), typeof(object), expr, expression2);
                    }
                    return Expression.Add(expr, expression2);

                case TokenKind.Minus:
                    if (!expr.Type.Equals(typeof(double)) || !expression2.Type.Equals(typeof(double)))
                    {
                        return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Subtract, true, false), typeof(object), expr, expression2);
                    }
                    return Expression.Subtract(expr, expression2);

                case TokenKind.Format:
                    if (!expr.Type.Equals(typeof(string)))
                    {
                        expr = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), expr, _executionContextParameter);
                    }
                    return Expression.Call(CachedReflectionInfo.StringOps_FormatOperator, expr, expression2.Cast(typeof(object)));

                case TokenKind.And:
                    return Expression.AndAlso(expr.Convert(typeof(bool)), expression2.Convert(typeof(bool)));

                case TokenKind.Or:
                    return Expression.OrElse(expr.Convert(typeof(bool)), expression2.Convert(typeof(bool)));

                case TokenKind.Xor:
                    return Expression.NotEqual(expr.Convert(typeof(bool)), expression2.Convert(typeof(bool)));

                case TokenKind.Band:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.And, true, false), typeof(object), expr, expression2);

                case TokenKind.Bor:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Or, true, false), typeof(object), expr, expression2);

                case TokenKind.Bxor:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.ExclusiveOr, true, false), typeof(object), expr, expression2);

                case TokenKind.Join:
                    return Expression.Call(CachedReflectionInfo.ParserOps_JoinOperator, _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)));

                case TokenKind.Ieq:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Equal, true, false), typeof(object), expr, expression2);

                case TokenKind.Ine:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.NotEqual, true, false), typeof(object), expr, expression2);

                case TokenKind.Ige:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.GreaterThanOrEqual, true, false), typeof(object), expr, expression2);

                case TokenKind.Igt:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.GreaterThan, true, false), typeof(object), expr, expression2);

                case TokenKind.Ilt:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.LessThan, true, false), typeof(object), expr, expression2);

                case TokenKind.Ile:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.LessThanOrEqual, true, false), typeof(object), expr, expression2);

                case TokenKind.Ilike:
                    return Expression.Call(CachedReflectionInfo.ParserOps_LikeOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false), ExpressionCache.Constant(true) });

                case TokenKind.Inotlike:
                    return Expression.Call(CachedReflectionInfo.ParserOps_LikeOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true), ExpressionCache.Constant(true) });

                case TokenKind.Imatch:
                    return Expression.Call(CachedReflectionInfo.ParserOps_MatchOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false), ExpressionCache.Constant(true) });

                case TokenKind.Inotmatch:
                    return Expression.Call(CachedReflectionInfo.ParserOps_MatchOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true), ExpressionCache.Constant(true) });

                case TokenKind.Ireplace:
                    return Expression.Call(CachedReflectionInfo.ParserOps_ReplaceOperator, _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true));

                case TokenKind.Icontains:
                    return this.GenerateCallContains(expr, expression2, true);

                case TokenKind.Inotcontains:
                    return Expression.Not(this.GenerateCallContains(expr, expression2, true));

                case TokenKind.Iin:
                    return this.GenerateCallContains(expression2, expr, true);

                case TokenKind.Inotin:
                    return Expression.Not(this.GenerateCallContains(expression2, expr, true));

                case TokenKind.Isplit:
                    return Expression.Call(CachedReflectionInfo.ParserOps_SplitOperator, _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true));

                case TokenKind.Ceq:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Equal, false, false), typeof(object), expr, expression2);

                case TokenKind.Cne:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.NotEqual, false, false), typeof(object), expr, expression2);

                case TokenKind.Cge:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.GreaterThanOrEqual, false, false), typeof(object), expr, expression2);

                case TokenKind.Cgt:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.GreaterThan, false, false), typeof(object), expr, expression2);

                case TokenKind.Clt:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.LessThan, false, false), typeof(object), expr, expression2);

                case TokenKind.Cle:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.LessThanOrEqual, false, false), typeof(object), expr, expression2);

                case TokenKind.Clike:
                    return Expression.Call(CachedReflectionInfo.ParserOps_LikeOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false), ExpressionCache.Constant(false) });

                case TokenKind.Cnotlike:
                    return Expression.Call(CachedReflectionInfo.ParserOps_LikeOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true), ExpressionCache.Constant(false) });

                case TokenKind.Cmatch:
                    return Expression.Call(CachedReflectionInfo.ParserOps_MatchOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false), ExpressionCache.Constant(false) });

                case TokenKind.Cnotmatch:
                    return Expression.Call(CachedReflectionInfo.ParserOps_MatchOperator, new Expression[] { _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(true), ExpressionCache.Constant(false) });

                case TokenKind.Creplace:
                    return Expression.Call(CachedReflectionInfo.ParserOps_ReplaceOperator, _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false));

                case TokenKind.Ccontains:
                    return this.GenerateCallContains(expr, expression2, false);

                case TokenKind.Cnotcontains:
                    return Expression.Not(this.GenerateCallContains(expr, expression2, false));

                case TokenKind.Cin:
                    return this.GenerateCallContains(expression2, expr, false);

                case TokenKind.Cnotin:
                    return Expression.Not(this.GenerateCallContains(expression2, expr, false));

                case TokenKind.Csplit:
                    return Expression.Call(CachedReflectionInfo.ParserOps_SplitOperator, _executionContextParameter, Expression.Constant(binaryExpressionAst.ErrorPosition), expr.Cast(typeof(object)), expression2.Cast(typeof(object)), ExpressionCache.Constant(false));

                case TokenKind.Is:
                case TokenKind.IsNot:
                {
                    if (!(expression2 is ConstantExpression) || !expression2.Type.Equals(typeof(Type)))
                    {
                        break;
                    }
                    Type type = (Type) ((ConstantExpression) expression2).Value;
                    if (type.Equals(typeof(PSCustomObject)) || type.Equals(typeof(PSObject)))
                    {
                        break;
                    }
                    expr = expr.Type.IsValueType ? expr : Expression.Call(CachedReflectionInfo.PSObject_Base, expr);
                    if (binaryExpressionAst.Operator != TokenKind.Is)
                    {
                        return Expression.Not(Expression.TypeIs(expr, type));
                    }
                    return Expression.TypeIs(expr, type);
                }
                case TokenKind.As:
                    return Expression.Call(CachedReflectionInfo.TypeOps_AsOperator, expr.Cast(typeof(object)), expression2.Convert(typeof(Type)));

                case TokenKind.Shl:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.LeftShift, true, false), typeof(object), expr, expression2);

                case TokenKind.Shr:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.RightShift, true, false), typeof(object), expr, expression2);

                default:
                    throw new InvalidOperationException("Unknown token in binary operator.");
            }
            Expression expression = Expression.Call(CachedReflectionInfo.TypeOps_IsInstance, expr.Cast(typeof(object)), expression2.Cast(typeof(object)));
            if (binaryExpressionAst.Operator == TokenKind.IsNot)
            {
                expression = Expression.Not(expression);
            }
            return expression;
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            return null;
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            return this.GenerateBreakOrContinue(breakStatementAst, breakStatementAst.Label, lgt => lgt.BreakLabel, new Func<LabelTarget, Expression>(Expression.Break), CachedReflectionInfo.BreakException_ctor);
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            return null;
        }

        public object VisitCommand(CommandAst commandAst)
        {
            ReadOnlyCollection<CommandElementAst> commandElements = commandAst.CommandElements;
            Expression[] initializers = new Expression[commandElements.Count];
            for (int i = 0; i < commandElements.Count; i++)
            {
                CommandElementAst ast = commandElements[i];
                if (ast is CommandParameterAst)
                {
                    initializers[i] = this.Compile(ast);
                }
                else
                {
                    CommandElementAst subExpression = ast;
                    bool splatted = false;
                    UsingExpressionAst ast3 = ast as UsingExpressionAst;
                    if (ast3 != null)
                    {
                        subExpression = ast3.SubExpression;
                    }
                    VariableExpressionAst ast4 = subExpression as VariableExpressionAst;
                    if (ast4 != null)
                    {
                        splatted = ast4.Splatted;
                    }
                    initializers[i] = Expression.Call(CachedReflectionInfo.CommandParameterInternal_CreateArgument, Expression.Constant(ast.Extent), Expression.Convert(this.GetCommandArgumentExpression(ast), typeof(object)), Expression.Constant(splatted));
                }
            }
            Expression expression = Expression.NewArrayInit(typeof(CommandParameterInternal), initializers);
            if ((((commandElements.Count == 2) && (commandElements[1] is ParenExpressionAst)) && ((((ParenExpressionAst) commandElements[1]).Pipeline.GetPureExpression() is ArrayLiteralAst) && (commandElements[0].Extent.EndColumnNumber == commandElements[1].Extent.StartColumnNumber))) && (commandElements[0].Extent.EndLineNumber == commandElements[1].Extent.StartLineNumber))
            {
                expression = Expression.Block(Expression.IfThen(IsStrictMode(2, _executionContextParameter), ThrowRuntimeError("StrictModeFunctionCallWithParens", ParserStrings.StrictModeFunctionCallWithParens, new Expression[0])), expression);
            }
            return expression;
        }

        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            ExpressionAst ast = commandExpressionAst.Expression;
            Expression expr = this.Compile(ast);
            UnaryExpressionAst ast2 = ast as UnaryExpressionAst;
            if (((ast2 == null) || !ast2.TokenKind.HasTrait(TokenFlags.PrefixOrPostfixOperator)) && !expr.Type.Equals(typeof(void)))
            {
                return this.CallAddPipe(expr, _getCurrentPipe);
            }
            return expr;
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            ExpressionAst argument = commandParameterAst.Argument;
            IScriptExtent errorPosition = commandParameterAst.ErrorPosition;
            if (argument != null)
            {
                bool b = (errorPosition.EndLineNumber != argument.Extent.StartLineNumber) || (errorPosition.EndColumnNumber != argument.Extent.StartColumnNumber);
                return Expression.Call(CachedReflectionInfo.CommandParameterInternal_CreateParameterWithArgument, new Expression[] { Expression.Constant(errorPosition), Expression.Constant(commandParameterAst.ParameterName), Expression.Constant(errorPosition.Text), Expression.Constant(argument.Extent), Expression.Convert(this.GetCommandArgumentExpression(argument), typeof(object)), ExpressionCache.Constant(b) });
            }
            return Expression.Call(CachedReflectionInfo.CommandParameterInternal_CreateParameter, Expression.Constant(errorPosition), Expression.Constant(commandParameterAst.ParameterName), Expression.Constant(errorPosition.Text));
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return Expression.Constant(constantExpressionAst.Value);
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            return this.GenerateBreakOrContinue(continueStatementAst, continueStatementAst.Label, lgt => lgt.ContinueLabel, new Func<LabelTarget, Expression>(Expression.Continue), CachedReflectionInfo.ContinueException_ctor);
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            object obj2;
            if (!this.CompilingConstantExpression && IsConstantValueVisitor.IsConstant(convertExpressionAst, out obj2, false, false))
            {
                return Expression.Constant(obj2);
            }
            ITypeName typeName = convertExpressionAst.Type.TypeName;
            HashtableAst child = convertExpressionAst.Child as HashtableAst;
            Expression expression = null;
            if (child != null)
            {
                ParameterExpression temp = this.NewTemp(typeof(OrderedDictionary), "orderedDictionary");
                if (typeName.FullName.Equals("ordered", StringComparison.OrdinalIgnoreCase))
                {
                    return Expression.Block(typeof(OrderedDictionary), new ParameterExpression[] { temp }, this.BuildHashtable(child.KeyValuePairs, temp, true));
                }
                if (typeName.FullName.Equals("PSCustomObject", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Block(typeof(OrderedDictionary), new ParameterExpression[] { temp }, this.BuildHashtable(child.KeyValuePairs, temp, true));
                }
            }
            if (convertExpressionAst.IsRef())
            {
                VariableExpressionAst ast2 = convertExpressionAst.Child as VariableExpressionAst;
                if (((ast2 != null) && ast2.VariablePath.IsVariable) && !ast2.IsConstantVariable())
                {
                    IEnumerable<PropertyInfo> enumerable;
                    bool flag;
                    Type type = ast2.GetVariableType(this, out enumerable, out flag);
                    return Expression.Call(CachedReflectionInfo.VariableOps_GetVariableAsRef, Expression.Constant(ast2.VariablePath), _executionContextParameter, ((type != null) && !type.Equals(typeof(object))) ? Expression.Constant(type) : ExpressionCache.NullType);
                }
            }
            if (expression == null)
            {
                expression = this.Compile(convertExpressionAst.Child);
            }
            if (typeName.FullName.Equals("PSCustomObject", StringComparison.OrdinalIgnoreCase))
            {
                return Expression.Dynamic(PSCustomObjectConverter.Get(), typeof(object), expression);
            }
            return ConvertValue(typeName, expression);
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            Func<ExpressionAst, Expression> selector = null;
            List<Expression> expressions = new List<Expression> {
                this.UpdatePosition(dataStatementAst)
            };
            if (dataStatementAst.HasNonConstantAllowedCommand)
            {
                if (selector == null)
                {
                    selector = elem => this.Compile(elem).Convert(typeof(string));
                }
                expressions.Add(Expression.Call(CachedReflectionInfo.RestrictedLanguageChecker_CheckDataStatementAstAtRuntime, Expression.Constant(dataStatementAst), Expression.NewArrayInit(typeof(string), dataStatementAst.CommandsAllowed.Select<ExpressionAst, Expression>(selector))));
            }
            ParameterExpression left = this.NewTemp(typeof(PSLanguageMode), "oldLanguageMode");
            MemberExpression right = Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_LanguageMode);
            expressions.Add(Expression.Assign(left, right));
            expressions.Add(Expression.Assign(right, Expression.Constant(PSLanguageMode.RestrictedLanguage)));
            Expression item = (dataStatementAst.Variable != null) ? this.CaptureAstResults(dataStatementAst.Body, CaptureAstContext.Assignment, null).Cast(typeof(object)) : this.Compile(dataStatementAst.Body);
            expressions.Add(item);
            BlockExpression expression4 = Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.TryFinally(Expression.Block(expressions), Expression.Assign(right, left)) });
            if (dataStatementAst.Variable == null)
            {
                return expression4;
            }
            if (dataStatementAst.TupleIndex >= 0)
            {
                return Expression.Assign(this.GetLocal(dataStatementAst.TupleIndex), expression4);
            }
            return CallSetVariable(Expression.Constant(new VariablePath("local:" + dataStatementAst.Variable)), expression4, null);
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            return this.GenerateDoLoop(doUntilStatementAst);
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            return this.GenerateDoLoop(doWhileStatementAst);
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return ExpressionCache.Constant(1);
        }

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return null;
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            Expression expr = (exitStatementAst.Pipeline != null) ? this.CaptureStatementResults(exitStatementAst.Pipeline, CaptureAstContext.Assignment, null) : ExpressionCache.Constant(0);
            return Expression.Block(this.UpdatePosition(exitStatementAst), Expression.Throw(Expression.Call(CachedReflectionInfo.PipelineOps_GetExitException, expr.Convert(typeof(object))), typeof(void)));
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            ConstantExpression expression = Expression.Constant(expandableStringExpressionAst.FormatExpression);
            ReadOnlyCollection<ExpressionAst> nestedExpressions = expandableStringExpressionAst.NestedExpressions;
            PSToStringBinder toStringBinder = PSToStringBinder.Get();
            NewArrayExpression expression2 = Expression.NewArrayInit(typeof(string), (IEnumerable<Expression>) (from e in nestedExpressions select Expression.Dynamic(toStringBinder, typeof(string), this.Compile(e), _executionContextParameter)));
            return Expression.Call(CachedReflectionInfo.StringOps_FormatOperator, expression, expression2);
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            Expression expression;
            StringConstantExpressionAst location = fileRedirectionAst.Location as StringConstantExpressionAst;
            if (location != null)
            {
                expression = this.Compile(location);
            }
            else
            {
                expression = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), this.CompileExpressionOperand(fileRedirectionAst.Location), _executionContextParameter);
            }
            return Expression.New(CachedReflectionInfo.FileRedirection_ctor, new Expression[] { Expression.Constant(fileRedirectionAst.FromStream), ExpressionCache.Constant(fileRedirectionAst.Append), expression });
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            Action<List<Expression>, Expression> generateBody = delegate (List<Expression> exprs, Expression newValue) {
                exprs.Add(this.ReduceAssignment(forEachStatementAst.Variable, TokenKind.Equals, newValue));
                exprs.Add(this.Compile(forEachStatementAst.Body));
            };
            return this.GenerateIteratorStatement(SpecialVariables.foreachVarPath, () => this.UpdatePosition(forEachStatementAst.Variable), this._foreachTupleIndex, forEachStatementAst, generateBody);
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            Expression expression = (forStatementAst.Initializer != null) ? this.CaptureStatementResults(forStatementAst.Initializer, CaptureAstContext.Assignment, null) : null;
            Func<Expression> generateCondition = null;
            if (forStatementAst.Condition != null)
            {
                generateCondition = () => Expression.Block(this.UpdatePosition(forStatementAst.Condition), this.CaptureStatementResults(forStatementAst.Condition, CaptureAstContext.Condition, null));
            }
            Expression expression2 = this.GenerateWhileLoop(forStatementAst.Label, generateCondition, delegate (List<Expression> loopBody, LabelTarget breakTarget, LabelTarget continueTarget) {
                loopBody.Add(this.Compile(forStatementAst.Body));
            }, forStatementAst.Iterator);
            if (expression != null)
            {
                return Expression.Block(expression, expression2);
            }
            return expression2;
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if (!functionDefinitionAst.IsWorkflow)
            {
                return Expression.Call(CachedReflectionInfo.FunctionOps_DefineFunction, _executionContextParameter, Expression.Constant(functionDefinitionAst), Expression.Constant(new ScriptBlockExpressionWrapper(functionDefinitionAst)));
            }
            if (this.generatedCallToDefineWorkflows)
            {
                return ExpressionCache.Empty;
            }
            Ast parent = functionDefinitionAst.Parent;
            while (!(parent is ScriptBlockAst))
            {
                parent = parent.Parent;
            }
            this.generatedCallToDefineWorkflows = true;
            return Expression.Call(CachedReflectionInfo.FunctionOps_DefineWorkflows, _executionContextParameter, Expression.Constant(parent, typeof(ScriptBlockAst)));
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            ParameterExpression temp = this.NewTemp(typeof(Hashtable), "hashtable");
            return Expression.Block(typeof(Hashtable), new ParameterExpression[] { temp }, this.BuildHashtable(hashtableAst.KeyValuePairs, temp, false));
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            int count = ifStmtAst.Clauses.Count;
            Tuple<BlockExpression, Expression>[] tupleArray = new Tuple<BlockExpression, Expression>[count];
            for (int i = 0; i < count; i++)
            {
                Tuple<PipelineBaseAst, StatementBlockAst> tuple = ifStmtAst.Clauses[i];
                BlockExpression expression = Expression.Block(this.UpdatePosition(tuple.Item1), this.CaptureStatementResults(tuple.Item1, CaptureAstContext.Condition, null).Convert(typeof(bool)));
                Expression expression2 = this.Compile(tuple.Item2);
                tupleArray[i] = Tuple.Create<BlockExpression, Expression>(expression, expression2);
            }
            Expression ifFalse = null;
            if (ifStmtAst.ElseClause != null)
            {
                ifFalse = this.Compile(ifStmtAst.ElseClause);
            }
            Expression expression4 = null;
            for (int j = count - 1; j >= 0; j--)
            {
                BlockExpression test = tupleArray[j].Item1;
                Expression ifTrue = tupleArray[j].Item2;
                if (ifFalse != null)
                {
                    expression4 = ifFalse = Expression.IfThenElse(test, ifTrue, ifFalse);
                }
                else
                {
                    expression4 = ifFalse = Expression.IfThen(test, ifTrue);
                }
            }
            return expression4;
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            Expression element = this.CompileExpressionOperand(indexExpressionAst.Target);
            ExpressionAst index = indexExpressionAst.Index;
            ArrayLiteralAst ast2 = index as ArrayLiteralAst;
            PSMethodInvocationConstraints constraints = CombineTypeConstraintForMethodResolution(GetTypeConstraintForMethodResolution(indexExpressionAst.Target), GetTypeConstraintForMethodResolution(index));
            if ((ast2 != null) && (ast2.Elements.Count > 1))
            {
                return Expression.Dynamic(PSGetIndexBinder.Get(ast2.Elements.Count, constraints, true), typeof(object), ast2.Elements.Select<ExpressionAst, Expression>(new Func<ExpressionAst, Expression>(this.CompileExpressionOperand)).Prepend<Expression>(element));
            }
            return Expression.Dynamic(PSGetIndexBinder.Get(1, constraints, true), typeof(object), element, this.CompileExpressionOperand(index));
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            PSMethodInvocationConstraints invokeMemberConstraints = GetInvokeMemberConstraints(invokeMemberExpressionAst);
            StringConstantExpressionAst member = invokeMemberExpressionAst.Member as StringConstantExpressionAst;
            if (member == null)
            {
                throw new NotImplementedException("invoke member w/ expression name");
            }
            Expression target = this.CompileExpressionOperand(invokeMemberExpressionAst.Expression);
            IEnumerable<Expression> args = (invokeMemberExpressionAst.Arguments == null) ? ((IEnumerable<Expression>) new Expression[0]) : invokeMemberExpressionAst.Arguments.Select<ExpressionAst, Expression>(new Func<ExpressionAst, Expression>(this.CompileExpressionOperand));
            return InvokeMember(member.Value, invokeMemberConstraints, target, args, invokeMemberExpressionAst.Static, false);
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            Expression expression = this.CompileExpressionOperand(memberExpressionAst.Expression);
            StringConstantExpressionAst member = memberExpressionAst.Member as StringConstantExpressionAst;
            if (member != null)
            {
                return Expression.Dynamic(PSGetMemberBinder.Get(member.Value, memberExpressionAst.Static), typeof(object), expression);
            }
            Expression expression2 = this.Compile(memberExpressionAst.Member);
            return Expression.Dynamic(PSGetDynamicMemberBinder.Get(memberExpressionAst.Static), typeof(object), expression, expression2);
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            return new MergingRedirection(mergingRedirectionAst.FromStream, mergingRedirectionAst.ToStream);
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return null;
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return null;
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return null;
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            return null;
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            PipelineBaseAst pipeline = parenExpressionAst.Pipeline;
            AssignmentStatementAst assignmentStatementAst = pipeline as AssignmentStatementAst;
            if (assignmentStatementAst == null)
            {
                return this.CaptureStatementResults(pipeline, CaptureAstContext.Assignment, null);
            }
            return this.CompileAssignment(assignmentStatementAst, null);
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            List<ParameterExpression> list = new List<ParameterExpression>();
            List<Expression> list2 = new List<Expression>();
            if (!(pipelineAst.Parent is AssignmentStatementAst) && !(pipelineAst.Parent is ParenExpressionAst))
            {
                list2.Add(this.UpdatePosition(pipelineAst));
            }
            ReadOnlyCollection<CommandBaseAst> pipelineElements = pipelineAst.PipelineElements;
            CommandExpressionAst commandExpr = pipelineElements[0] as CommandExpressionAst;
            if ((commandExpr != null) && (pipelineElements.Count == 1))
            {
                if (commandExpr.Redirections.Count > 0)
                {
                    return this.GetRedirectedExpression(commandExpr, false);
                }
                list2.Add(this.Compile(commandExpr));
            }
            else
            {
                Expression redirectedExpression;
                int num;
                int count;
                Expression nullCommandRedirections;
                if (commandExpr != null)
                {
                    if (commandExpr.Redirections.Count > 0)
                    {
                        redirectedExpression = this.GetRedirectedExpression(commandExpr, true);
                    }
                    else
                    {
                        redirectedExpression = this.GetRangeEnumerator(commandExpr.Expression) ?? this.Compile(commandExpr.Expression);
                    }
                    num = 1;
                    count = pipelineElements.Count - 1;
                }
                else
                {
                    redirectedExpression = ExpressionCache.AutomationNullConstant;
                    num = 0;
                    count = pipelineElements.Count;
                }
                Expression[] initializers = new Expression[count];
                CommandBaseAst[] astArray = new CommandBaseAst[count];
                object[] array = new object[count];
                for (int i = 0; num < pipelineElements.Count; i++)
                {
                    CommandBaseAst ast = pipelineElements[num];
                    initializers[i] = this.Compile(ast);
                    array[i] = this.GetCommandRedirections(ast);
                    astArray[i] = ast;
                    num++;
                }
                if ((from r in array
                    where r is Expression
                    select r).Any<object>())
                {
                    nullCommandRedirections = Expression.NewArrayInit(typeof(CommandRedirection[]), (IEnumerable<Expression>) (from r in array select (r as Expression) ?? Expression.Constant(r, typeof(CommandRedirection[]))));
                }
                else if ((from r in array
                    where r != null
                    select r).Any<object>())
                {
                    nullCommandRedirections = Expression.Constant(Array.ConvertAll<object, CommandRedirection[]>(array, r => r as CommandRedirection[]));
                }
                else
                {
                    nullCommandRedirections = ExpressionCache.NullCommandRedirections;
                }
                if (commandExpr != null)
                {
                    ParameterExpression expression3 = Expression.Variable(redirectedExpression.Type);
                    list.Add(expression3);
                    list2.Add(Expression.Assign(expression3, redirectedExpression));
                    redirectedExpression = expression3;
                }
                Expression item = Expression.Call(CachedReflectionInfo.PipelineOps_InvokePipeline, new Expression[] { redirectedExpression.Cast(typeof(object)), (commandExpr != null) ? ExpressionCache.FalseConstant : ExpressionCache.TrueConstant, Expression.NewArrayInit(typeof(CommandParameterInternal[]), initializers), Expression.Constant(astArray), nullCommandRedirections, _functionContext });
                list2.Add(item);
            }
            return Expression.Block((IEnumerable<ParameterExpression>) list, (IEnumerable<Expression>) list2);
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            Expression expression;
            if (this._compilingTrap)
            {
                expression = Expression.Throw(Expression.New(CachedReflectionInfo.ReturnException_ctor, new Expression[] { Expression.Constant(ExpressionCache.AutomationNullConstant) }));
            }
            else
            {
                expression = Expression.Return(this._returnTarget, this._returnTarget.Type.Equals(typeof(object)) ? ExpressionCache.AutomationNullConstant : ExpressionCache.Empty);
            }
            if (returnStatementAst.Pipeline != null)
            {
                PipelineBaseAst pipeline = returnStatementAst.Pipeline;
                AssignmentStatementAst assignmentStatementAst = pipeline as AssignmentStatementAst;
                Expression expression2 = (assignmentStatementAst != null) ? this.CallAddPipe(this.CompileAssignment(assignmentStatementAst, null), _getCurrentPipe) : this.Compile(pipeline);
                return Expression.Block(expression2, expression);
            }
            return expression;
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            FunctionDefinitionAst parent = scriptBlockAst.Parent as FunctionDefinitionAst;
            string funcName = (parent != null) ? parent.Name : "<ScriptBlock>";
            if (scriptBlockAst.DynamicParamBlock != null)
            {
                this._dynamicParamBlockLambda = this.CompileNamedBlock(scriptBlockAst.DynamicParamBlock, funcName + "<DynamicParam>");
            }
            if (scriptBlockAst.BeginBlock != null)
            {
                this._beginBlockLambda = this.CompileNamedBlock(scriptBlockAst.BeginBlock, funcName + "<Begin>");
            }
            if (scriptBlockAst.ProcessBlock != null)
            {
                string str2 = funcName;
                if (!scriptBlockAst.ProcessBlock.Unnamed)
                {
                    str2 = funcName + "<Process>";
                }
                this._processBlockLambda = this.CompileNamedBlock(scriptBlockAst.ProcessBlock, str2);
            }
            if (scriptBlockAst.EndBlock != null)
            {
                if (!scriptBlockAst.EndBlock.Unnamed)
                {
                    funcName = funcName + "<End>";
                }
                this._endBlockLambda = this.CompileNamedBlock(scriptBlockAst.EndBlock, funcName);
            }
            return null;
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return Expression.Call(Expression.Constant(new ScriptBlockExpressionWrapper(scriptBlockExpressionAst.ScriptBlock)), CachedReflectionInfo.ScriptBlockExpressionWrapper_GetScriptBlock, _executionContextParameter, ExpressionCache.Constant(false));
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            List<Expression> exprs = new List<Expression>();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            this.CompileStatementListWithTraps(statementBlockAst.Statements, statementBlockAst.Traps, exprs, temps);
            if (!exprs.Any<Expression>())
            {
                exprs.Add(ExpressionCache.Empty);
            }
            return Expression.Block(typeof(void), temps, exprs);
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return Expression.Constant(stringConstantExpressionAst.Value);
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            if (!subExpressionAst.SubExpression.Statements.Any<StatementAst>())
            {
                return ExpressionCache.NullConstant;
            }
            return this.CaptureAstResults(subExpressionAst.SubExpression, CaptureAstContext.Assignment, null);
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            AutomaticVarSaver avs = new AutomaticVarSaver(this, SpecialVariables.UnderbarVarPath, 0);
            List<ParameterExpression> first = new List<ParameterExpression>();
            ParameterExpression item = null;
            if (switchStatementAst.Default != null)
            {
                item = this.NewTemp(typeof(bool), "skipDefault");
                first.Add(item);
            }
            Action<List<Expression>, Expression> switchBodyGenerator = this.GetSwitchBodyGenerator(switchStatementAst, avs, item);
            if ((switchStatementAst.Flags & SwitchFlags.File) != SwitchFlags.None)
            {
                List<Expression> expressions = new List<Expression>();
                ParameterExpression expression2 = this.NewTemp(typeof(string), "path");
                first.Add(expression2);
                expressions.Add(this.UpdatePosition(switchStatementAst.Condition));
                DynamicExpression expression3 = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), this.CaptureStatementResults(switchStatementAst.Condition, CaptureAstContext.Assignment, null), _executionContextParameter);
                expressions.Add(Expression.Assign(expression2, Expression.Call(CachedReflectionInfo.SwitchOps_ResolveFilePath, Expression.Constant(switchStatementAst.Condition.Extent), expression3, _executionContextParameter)));
                List<Expression> list3 = new List<Expression>();
                ParameterExpression expression4 = this.NewTemp(typeof(StreamReader), "streamReader");
                ParameterExpression line = this.NewTemp(typeof(string), "line");
                first.Add(expression4);
                first.Add(line);
                list3.Add(Expression.Assign(expression4, Expression.New(CachedReflectionInfo.StreamReader_ctor, new Expression[] { expression2 })));
                BinaryExpression loopTest = Expression.NotEqual(Expression.Assign(line, Expression.Call(expression4, CachedReflectionInfo.StreamReader_ReadLine)).Cast(typeof(object)), ExpressionCache.NullConstant);
                list3.Add(avs.SaveAutomaticVar());
                list3.Add(this.GenerateWhileLoop(switchStatementAst.Label, () => loopTest, delegate (List<Expression> loopBody, LabelTarget breakTarget, LabelTarget continueTarget) {
                    switchBodyGenerator(loopBody, line);
                }, null));
                BlockExpression body = Expression.Block(list3);
                BlockExpression @finally = Expression.Block(Expression.IfThen(Expression.NotEqual(expression4, ExpressionCache.NullConstant), Expression.Call(expression4.Cast(typeof(IDisposable)), CachedReflectionInfo.IDisposable_Dispose)), avs.RestoreAutomaticVar());
                ParameterExpression expression7 = this.NewTemp(typeof(Exception), "exception");
                BlockExpression expression8 = Expression.Block(body.Type, new Expression[] { Expression.Call(CachedReflectionInfo.CommandProcessorBase_CheckForSevereException, expression7), ThrowRuntimeErrorWithInnerException("FileReadError", ParserStrings.FileReadError, expression7, new Expression[] { Expression.Property(expression7, CachedReflectionInfo.Exception_Message) }) });
                expressions.Add(Expression.TryCatchFinally(body, @finally, new CatchBlock[] { Expression.Catch(typeof(FlowControlException), Expression.Rethrow(body.Type)), Expression.Catch(expression7, expression8) }));
                return Expression.Block(first.Concat<ParameterExpression>(avs.GetTemps()), expressions);
            }
            TryExpression expression9 = Expression.TryFinally(Expression.Block(avs.SaveAutomaticVar(), this.GenerateIteratorStatement(SpecialVariables.switchVarPath, () => this.UpdatePosition(switchStatementAst.Condition), this._switchTupleIndex, switchStatementAst, switchBodyGenerator)), avs.RestoreAutomaticVar());
            return Expression.Block(first.Concat<ParameterExpression>(avs.GetTemps()), new Expression[] { expression9 });
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            Expression expr = throwStatementAst.IsRethrow ? _currentExceptionBeingHandled : ((throwStatementAst.Pipeline == null) ? ExpressionCache.NullConstant : this.CaptureStatementResults(throwStatementAst.Pipeline, CaptureAstContext.Assignment, null));
            return Expression.Block(this.UpdatePosition(throwStatementAst), Expression.Throw(Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToException, expr.Convert(typeof(object)), Expression.Constant(throwStatementAst.Extent))));
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            return null;
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            List<ParameterExpression> temps = new List<ParameterExpression>();
            List<Expression> exprs = new List<Expression>();
            List<Expression> expressions = new List<Expression>();
            ParameterExpression item = this.NewTemp(typeof(bool), "oldActiveHandler");
            temps.Add(item);
            MemberExpression right = Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_ExceptionHandlerInEnclosingStatementBlock);
            exprs.Add(Expression.Assign(item, right));
            exprs.Add(Expression.Assign(right, ExpressionCache.Constant(true)));
            expressions.Add(Expression.Assign(right, item));
            this.CompileStatementListWithTraps(tryStatementAst.Body.Statements, tryStatementAst.Body.Traps, exprs, temps);
            List<CatchBlock> source = new List<CatchBlock>();
            if ((tryStatementAst.CatchClauses.Count == 1) && tryStatementAst.CatchClauses[0].IsCatchAll)
            {
                AutomaticVarSaver saver = new AutomaticVarSaver(this, SpecialVariables.UnderbarVarPath, 0);
                ParameterExpression expression = this.NewTemp(typeof(RuntimeException), "rte");
                ParameterExpression left = this.NewTemp(typeof(RuntimeException), "oldrte");
                NewExpression newValue = Expression.New(CachedReflectionInfo.ErrorRecord__ctor, new Expression[] { Expression.Property(expression, CachedReflectionInfo.RuntimeException_ErrorRecord), expression });
                List<Expression> list5 = new List<Expression> {
                    Expression.Assign(left, _currentExceptionBeingHandled),
                    Expression.Assign(_currentExceptionBeingHandled, expression),
                    saver.SaveAutomaticVar(),
                    saver.SetNewValue(newValue)
                };
                StatementBlockAst body = tryStatementAst.CatchClauses[0].Body;
                this.CompileStatementListWithTraps(body.Statements, body.Traps, list5, temps);
                TryExpression expression6 = Expression.TryFinally(Expression.Block(typeof(void), list5), Expression.Block(typeof(void), new Expression[] { saver.RestoreAutomaticVar(), Expression.Assign(_currentExceptionBeingHandled, left) }));
                source.Add(Expression.Catch(typeof(PipelineStoppedException), Expression.Rethrow()));
                source.Add(Expression.Catch(expression, Expression.Block(saver.GetTemps().Append<ParameterExpression>(left).ToArray<ParameterExpression>(), new Expression[] { expression6 })));
            }
            else if (tryStatementAst.CatchClauses.Any<CatchClauseAst>())
            {
                int num = 0;
                foreach (CatchClauseAst ast2 in tryStatementAst.CatchClauses)
                {
                    num += Math.Max(ast2.CatchTypes.Count, 1);
                }
                Type[] typeArray = new Type[num];
                Expression array = Expression.Constant(typeArray);
                List<Expression> list7 = new List<Expression>();
                List<SwitchCase> list8 = new List<SwitchCase>();
                int i = 0;
                int index = 0;
                ParameterExpression expression8 = Expression.Parameter(typeof(RuntimeException));
                foreach (CatchClauseAst ast3 in tryStatementAst.CatchClauses)
                {
                    if (ast3.IsCatchAll)
                    {
                        typeArray[index] = typeof(ExceptionHandlingOps.CatchAll);
                    }
                    else
                    {
                        foreach (TypeConstraintAst ast4 in ast3.CatchTypes)
                        {
                            typeArray[index] = ast4.TypeName.GetReflectionType();
                            if (typeArray[index] == null)
                            {
                                IndexExpression expression9 = Expression.ArrayAccess(array, new Expression[] { ExpressionCache.Constant(index) });
                                list7.Add(Expression.IfThen(Expression.Equal(expression9, ExpressionCache.NullType), Expression.Assign(expression9, Expression.Call(CachedReflectionInfo.TypeOps_ResolveTypeName, Expression.Constant(ast4.TypeName)))));
                            }
                            index++;
                        }
                    }
                    BlockExpression expression10 = Expression.Block(typeof(void), new Expression[] { this.Compile(ast3.Body) });
                    if (ast3.IsCatchAll)
                    {
                        list8.Add(Expression.SwitchCase(expression10, new Expression[] { ExpressionCache.Constant(i) }));
                        i++;
                    }
                    else
                    {
                        list8.Add(Expression.SwitchCase(expression10, Enumerable.Range(i, i + ast3.CatchTypes.Count).Select<int, Expression>(new Func<int, Expression>(ExpressionCache.Constant))));
                        i += ast3.CatchTypes.Count;
                    }
                }
                if (list7.Any<Expression>())
                {
                    array = Expression.Block(list7.Append<Expression>(array));
                }
                AutomaticVarSaver saver2 = new AutomaticVarSaver(this, SpecialVariables.UnderbarVarPath, 0);
                MethodCallExpression switchValue = Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_FindMatchingHandler, this.LocalVariablesParameter, expression8, array, _executionContextParameter);
                ParameterExpression expression12 = this.NewTemp(typeof(RuntimeException), "oldrte");
                TryExpression expression13 = Expression.TryFinally(Expression.Block(typeof(void), new Expression[] { Expression.Assign(expression12, _currentExceptionBeingHandled), Expression.Assign(_currentExceptionBeingHandled, expression8), saver2.SaveAutomaticVar(), Expression.Switch(switchValue, Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_CheckActionPreference, _functionContext, expression8), list8.ToArray()) }), Expression.Block(saver2.RestoreAutomaticVar(), Expression.Assign(_currentExceptionBeingHandled, expression12)));
                source.Add(Expression.Catch(typeof(PipelineStoppedException), Expression.Rethrow()));
                source.Add(Expression.Catch(expression8, Expression.Block(saver2.GetTemps().Append<ParameterExpression>(expression12).ToArray<ParameterExpression>(), new Expression[] { expression13 })));
            }
            if (tryStatementAst.Finally != null)
            {
                ParameterExpression expression14 = this.NewTemp(typeof(bool), "oldIsStopping");
                temps.Add(expression14);
                expressions.Add(Expression.Assign(expression14, Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_SuspendStoppingPipeline, _executionContextParameter)));
                List<Expression> list9 = new List<Expression>();
                this.CompileStatementListWithTraps(tryStatementAst.Finally.Statements, tryStatementAst.Finally.Traps, list9, temps);
                if (!list9.Any<Expression>())
                {
                    list9.Add(ExpressionCache.Empty);
                }
                expressions.Add(Expression.Block(new Expression[] { Expression.TryFinally(Expression.Block(list9), Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_RestoreStoppingPipeline, _executionContextParameter, expression14)) }));
            }
            if (!exprs.Last<Expression>().Type.Equals(typeof(void)))
            {
                exprs.Add(ExpressionCache.Empty);
            }
            if (source.Any<CatchBlock>())
            {
                return Expression.Block(temps.ToArray(), new Expression[] { Expression.TryCatchFinally(Expression.Block(exprs), Expression.Block(expressions), source.ToArray()) });
            }
            return Expression.Block(temps.ToArray(), new Expression[] { Expression.TryFinally(Expression.Block(exprs), Expression.Block(expressions)) });
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return null;
        }

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return this.CompileTypeName(typeExpressionAst.TypeName);
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            object obj2;
            if (!this.CompilingConstantExpression && IsConstantValueVisitor.IsConstant(unaryExpressionAst, out obj2, false, false))
            {
                return Expression.Constant(obj2);
            }
            ExpressionAst child = unaryExpressionAst.Child;
            switch (unaryExpressionAst.TokenKind)
            {
                case TokenKind.MinusMinus:
                    return this.CompileIncrementOrDecrement(child, -1, true);

                case TokenKind.PlusPlus:
                    return this.CompileIncrementOrDecrement(child, 1, true);

                case TokenKind.Exclaim:
                case TokenKind.Not:
                    return Expression.Dynamic(PSUnaryOperationBinder.Get(ExpressionType.Not), typeof(object), this.CompileExpressionOperand(child));

                case TokenKind.Plus:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Add, true, false), typeof(object), ExpressionCache.Constant(0), this.CompileExpressionOperand(child));

                case TokenKind.Minus:
                    return Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Subtract, true, false), typeof(object), ExpressionCache.Constant(0), this.CompileExpressionOperand(child));

                case TokenKind.Bnot:
                    return Expression.Dynamic(PSUnaryOperationBinder.Get(ExpressionType.OnesComplement), typeof(object), this.CompileExpressionOperand(child));

                case TokenKind.Join:
                    return Expression.Call(CachedReflectionInfo.ParserOps_UnaryJoinOperator, _executionContextParameter, Expression.Constant(unaryExpressionAst.Extent), this.CompileExpressionOperand(child).Cast(typeof(object)));

                case TokenKind.Isplit:
                case TokenKind.Csplit:
                    return Expression.Call(CachedReflectionInfo.ParserOps_UnarySplitOperator, _executionContextParameter, Expression.Constant(unaryExpressionAst.Extent), this.CompileExpressionOperand(child).Cast(typeof(object)));

                case TokenKind.PostfixPlusPlus:
                    return this.CompileIncrementOrDecrement(child, 1, false);

                case TokenKind.PostfixMinusMinus:
                    return this.CompileIncrementOrDecrement(child, -1, false);
            }
            throw new InvalidOperationException("Unknown token in unary operator.");
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpression)
        {
            return Expression.Call(CachedReflectionInfo.VariableOps_GetUsingValue, this.LocalVariablesParameter, ExpressionCache.Constant(usingExpression.RuntimeUsingIndex), _executionContextParameter);
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            VariablePath variablePath = variableExpressionAst.VariablePath;
            if (variablePath.IsVariable)
            {
                if (variablePath.UnqualifiedPath.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    return ExpressionCache.NullConstant;
                }
                if (variablePath.UnqualifiedPath.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return ExpressionCache.Constant(true);
                }
                if (variablePath.UnqualifiedPath.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return ExpressionCache.Constant(false);
                }
            }
            int tupleIndex = variableExpressionAst.TupleIndex;
            if (variableExpressionAst.Automatic)
            {
                if (!variableExpressionAst.VariablePath.UnqualifiedPath.Equals("?", StringComparison.OrdinalIgnoreCase))
                {
                    return this.GetAutomaticVariable(variableExpressionAst);
                }
                if (this.Optimize)
                {
                    return Expression.Property(_executionContextParameter, CachedReflectionInfo.ExecutionContext_QuestionMarkVariableValue);
                }
                return CallGetVariable(Expression.Constant(variableExpressionAst.VariablePath), variableExpressionAst);
            }
            if (tupleIndex < 0)
            {
                return CallGetVariable(Expression.Constant(variableExpressionAst.VariablePath), variableExpressionAst);
            }
            return this.GetLocal(tupleIndex);
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            return this.GenerateWhileLoop(whileStatementAst.Label, () => Expression.Block(this.UpdatePosition(whileStatementAst.Condition), this.CaptureStatementResults(whileStatementAst.Condition, CaptureAstContext.Condition, null)), delegate (List<Expression> loopBody, LabelTarget breakTarget, LabelTarget continueTarget) {
                loopBody.Add(this.Compile(whileStatementAst.Body));
            }, null);
        }

        internal bool CompilingConstantExpression { get; set; }

        internal ParameterExpression LocalVariablesParameter { get; private set; }

        internal Type LocalVariablesTupleType { get; private set; }

        internal bool Optimize { get; private set; }

        private class AutomaticVarSaver
        {
            private readonly Compiler _compiler;

            private readonly int _automaticVar;

            private readonly VariablePath _autoVarPath;

            private ParameterExpression _oldValue;

            internal AutomaticVarSaver(Compiler compiler, VariablePath autoVarPath, int automaticVar)
            {
                this._compiler = compiler;
                this._autoVarPath = autoVarPath;
                this._automaticVar = automaticVar;
            }

            internal IEnumerable<ParameterExpression> GetTemps()
            {
                yield return this._oldValue;
            }

            internal Expression RestoreAutomaticVar()
            {
                if (this._automaticVar >= 0)
                {
                    return Expression.Assign(this._compiler.GetLocal(this._automaticVar), this._oldValue);
                }
                else
                {
                    return Compiler.CallSetVariable(Expression.Constant(this._autoVarPath), this._oldValue, null);
                }
            }

            internal Expression SaveAutomaticVar()
            {
                Expression local;
                if (this._automaticVar < 0)
                {
                    local = Compiler.CallGetVariable(Expression.Constant(this._autoVarPath), null);
                }
                else
                {
                    local = this._compiler.GetLocal(this._automaticVar);
                }
                Expression expression = local;
                this._oldValue = this._compiler.NewTemp(expression.Type, string.Concat("old_", this._autoVarPath.UnqualifiedPath));
                return Expression.Assign(this._oldValue, expression);
            }

            internal Expression SetNewValue(Expression newValue)
            {
                if (this._automaticVar >= 0)
                {
                    return Expression.Assign(this._compiler.GetLocal(this._automaticVar), newValue);
                }
                else
                {
                    return Compiler.CallSetVariable(Expression.Constant(this._autoVarPath), newValue, null);
                }
            }
        }

        private enum CaptureAstContext
        {
            Assignment,
            Condition,
            Enumerable
        }

        internal class DefaultValueExpressionWrapper
        {
            private Func<FunctionContext, object> _delegate;
            private Type _localsTupleType;
            private IScriptExtent[] _sequencePoints;

            internal object GetValue(System.Management.Automation.ExecutionContext context, SessionStateInternal sessionStateInternal, IList usingValues = null)
            {
                lock (this)
                {
                    return Compiler.GetExpressionValue(this.Expression, context, sessionStateInternal, usingValues, ref this._delegate, ref this._sequencePoints, ref this._localsTupleType);
                }
            }

            internal ExpressionAst Expression { get; set; }
        }

        private class LoopGotoTargets
        {
            internal LoopGotoTargets(string label, LabelTarget breakLabel, LabelTarget continueLabel)
            {
                this.Label = label;
                this.BreakLabel = breakLabel;
                this.ContinueLabel = continueLabel;
            }

            internal LabelTarget BreakLabel { get; private set; }

            internal LabelTarget ContinueLabel { get; private set; }

            internal string Label { get; private set; }
        }

        private delegate void MergeRedirectExprs(List<Expression> exprs, List<Expression> finallyExprs);
    }
}

