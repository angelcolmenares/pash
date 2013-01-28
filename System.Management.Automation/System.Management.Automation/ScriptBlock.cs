using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation.Internal;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
    [Serializable]
    public class ScriptBlock : ISerializable
    {
        private PSLanguageMode? languageMode;

        private readonly static ConditionalWeakTable<ScriptBlock, ConcurrentDictionary<Type, Delegate>> delegateTable;

        private readonly IParameterMetadataProvider _ast;

        private readonly bool _isFilter;

        private readonly CompiledScriptBlockData _scriptBlockData;

        private readonly static ConcurrentDictionary<Tuple<string, string>, ScriptBlock> _cachedScripts;

        internal readonly static object[] EmptyArray;

        public Ast Ast
        {
            get
            {
                return (Ast)this._ast;
            }
        }

        public List<Attribute> Attributes
        {
            get
            {
                return this.GetAttributes();
            }
        }

        internal Action<FunctionContext> BeginBlock
        {
            get
            {
                return this._scriptBlockData.BeginBlock;
            }
        }

        internal Expression<Action<FunctionContext>> BeginBlockTree
        {
            get
            {
                return this._scriptBlockData.BeginBlockTree;
            }
        }

        internal CmdletBindingAttribute CmdletBindingAttribute
        {
            get
            {
                return this._scriptBlockData.CmdletBindingAttribute;
            }
        }

        internal bool DebuggerHidden
        {
            get
            {
                return this._scriptBlockData.DebuggerHidden;
            }
            set
            {
                this._scriptBlockData.DebuggerHidden = value;
            }
        }

        internal bool DebuggerStepThrough
        {
            get
            {
                return this._scriptBlockData.DebuggerStepThrough;
            }
            set
            {
                this._scriptBlockData.DebuggerStepThrough = value;
            }
        }

        internal Action<FunctionContext> DynamicParamBlock
        {
            get
            {
                return this._scriptBlockData.DynamicParamBlock;
            }
        }

        internal Expression<Action<FunctionContext>> DynamicParamBlockTree
        {
            get
            {
                return this._scriptBlockData.DynamicParamBlockTree;
            }
        }

        internal Action<FunctionContext> EndBlock
        {
            get
            {
                return this._scriptBlockData.EndBlock;
            }
        }

        internal Expression<Action<FunctionContext>> EndBlockTree
        {
            get
            {
                return this._scriptBlockData.EndBlockTree;
            }
        }

        public string File
        {
            get
            {
                return this.GetFileName();
            }
        }

        internal bool HasBeginBlock
        {
            get
            {
                return this._ast.Body.BeginBlock != null;
            }
        }

        internal bool HasDynamicParameters
        {
            get
            {
                return this._ast.Body.DynamicParamBlock != null;
            }
        }

        internal bool HasEndBlock
        {
            get
            {
                return this._ast.Body.EndBlock != null;
            }
        }

        internal bool HasProcessBlock
        {
            get
            {
                return this._ast.Body.ProcessBlock != null;
            }
        }

        public bool IsFilter
        {
            get
            {
                return this.GetIsFilter();
            }
            set
            {
                this.SetIsFilter(value);
            }
        }

        internal PSLanguageMode? LanguageMode
        {
            get
            {
                return this.languageMode;
            }
            set
            {
                this.languageMode = value;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                if (this.SessionStateInternal != null)
                {
                    return this.SessionStateInternal.Module;
                }
                else
                {
                    return null;
                }
            }
        }

        internal ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                List<PSTypeName> pSTypeNames = new List<PSTypeName>();
                foreach (Attribute attribute in this.Attributes)
                {
                    OutputTypeAttribute outputTypeAttribute = attribute as OutputTypeAttribute;
                    if (outputTypeAttribute == null)
                    {
                        continue;
                    }
                    pSTypeNames.AddRange(outputTypeAttribute.Type);
                }
                return new ReadOnlyCollection<PSTypeName>(pSTypeNames);
            }
        }

        internal MergedCommandParameterMetadata ParameterMetadata
        {
            get
            {
                return this._scriptBlockData.GetParameterMetadata(this);
            }
        }

        internal Action<FunctionContext> ProcessBlock
        {
            get
            {
                return this._scriptBlockData.ProcessBlock;
            }
        }

        internal Expression<Action<FunctionContext>> ProcessBlockTree
        {
            get
            {
                return this._scriptBlockData.ProcessBlockTree;
            }
        }

        internal RuntimeDefinedParameterDictionary RuntimeDefinedParameters
        {
            get
            {
                return this._scriptBlockData.RuntimeDefinedParameters;
            }
        }

        internal CompiledScriptBlockData ScriptBlockData
        {
            get
            {
                return this._scriptBlockData;
            }
        }

        internal IScriptExtent[] SequencePoints
        {
            get
            {
                return this._scriptBlockData.SequencePoints;
            }
        }

        internal SessionState SessionState
        {
            get
            {
                if (this.SessionStateInternal == null)
                {
                    ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                    if (executionContextFromTLS != null)
                    {
                        this.SessionStateInternal = executionContextFromTLS.EngineSessionState.PublicSessionState.Internal;
                    }
                }
                if (this.SessionStateInternal != null)
                {
                    return this.SessionStateInternal.PublicSessionState;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.SessionStateInternal = value.Internal;
                    return;
                }
                else
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
            }
        }

        internal SessionStateInternal SessionStateInternal
        {
            get;
            set;
        }

        public PSToken StartPosition
        {
            get
            {
                return this.GetStartPosition();
            }
        }

        internal Action<FunctionContext> UnoptimizedBeginBlock
        {
            get
            {
                return this._scriptBlockData.UnoptimizedBeginBlock;
            }
        }

        internal Expression<Action<FunctionContext>> UnoptimizedBeginBlockTree
        {
            get
            {
                return this._scriptBlockData.UnoptimizedBeginBlockTree;
            }
        }

        internal Action<FunctionContext> UnoptimizedDynamicParamBlock
        {
            get
            {
                return this._scriptBlockData.UnoptimizedDynamicParamBlock;
            }
        }

        internal Expression<Action<FunctionContext>> UnoptimizedDynamicParamBlockTree
        {
            get
            {
                return this._scriptBlockData.UnoptimizedDynamicParamBlockTree;
            }
        }

        internal Action<FunctionContext> UnoptimizedEndBlock
        {
            get
            {
                return this._scriptBlockData.UnoptimizedEndBlock;
            }
        }

        internal Expression<Action<FunctionContext>> UnoptimizedEndBlockTree
        {
            get
            {
                return this._scriptBlockData.UnoptimizedEndBlockTree;
            }
        }

        internal Action<FunctionContext> UnoptimizedProcessBlock
        {
            get
            {
                return this._scriptBlockData.UnoptimizedProcessBlock;
            }
        }

        internal Expression<Action<FunctionContext>> UnoptimizedProcessBlockTree
        {
            get
            {
                return this._scriptBlockData.UnoptimizedProcessBlockTree;
            }
        }

        internal bool UsesCmdletBinding
        {
            get
            {
                return this._scriptBlockData.UsesCmdletBinding;
            }
        }

        static ScriptBlock()
        {
            ScriptBlock.delegateTable = new ConditionalWeakTable<ScriptBlock, ConcurrentDictionary<Type, Delegate>>();
            ScriptBlock._cachedScripts = new ConcurrentDictionary<Tuple<string, string>, ScriptBlock>();
            ScriptBlock.EmptyArray = new object[0];
        }

        internal ScriptBlock(IParameterMetadataProvider ast, bool isFilter)
            : this(ast, isFilter, new CompiledScriptBlockData(ast))
        {
        }

        private ScriptBlock(IParameterMetadataProvider ast, bool isFilter, CompiledScriptBlockData _scriptBlockData)
        {
            this.languageMode = null;
            this._ast = ast;
            this._scriptBlockData = _scriptBlockData;
            this._isFilter = isFilter;
            this.SetLanguageModeFromContext();
        }

        protected ScriptBlock(SerializationInfo info, StreamingContext context)
        {
            this.languageMode = null;
        }

        internal static object[] BindArgumentsForScripblockInvoke(RuntimeDefinedParameter[] parameters, object[] args, ExecutionContext context, bool dotting, Dictionary<string, PSVariable> backupWhenDotting, MutableTuple locals)
        {
            object value;
            CommandLineParameters commandLineParameter = new CommandLineParameters();
            if ((int)parameters.Length != 0)
            {
                for (int i = 0; i < (int)parameters.Length; i++)
                {
                    RuntimeDefinedParameter variableAtScope = parameters[i];
                    bool flag = false;
                    if (i < (int)args.Length)
                    {
                        value = args[i];
                    }
                    else
                    {
                        value = variableAtScope.Value;
                        if (value as Compiler.DefaultValueExpressionWrapper != null)
                        {
                            value = ((Compiler.DefaultValueExpressionWrapper)value).GetValue(context, null, null);
                        }
                        flag = true;
                    }
                    bool flag1 = false;
                    if (!dotting || backupWhenDotting == null)
                    {
                        flag1 = locals.TrySetParameter(variableAtScope.Name, value);
                    }
                    else
                    {
                        backupWhenDotting[variableAtScope.Name] = context.EngineSessionState.GetVariableAtScope(variableAtScope.Name, "local");
                    }
                    if (!flag1)
                    {
                        PSVariable pSVariable = new PSVariable(variableAtScope.Name, value, ScopedItemOptions.None, variableAtScope.Attributes);
                        context.EngineSessionState.SetVariable(pSVariable, false, CommandOrigin.Internal);
                    }
                    if (!flag)
                    {
                        commandLineParameter.Add(variableAtScope.Name, value);
                        commandLineParameter.MarkAsBoundPositionally(variableAtScope.Name);
                    }
                }
                locals.SetAutomaticVariable(AutomaticVariable.PSBoundParameters, commandLineParameter.GetValueToBindToPSBoundParameters(), context);
                int length = (int)args.Length - (int)parameters.Length;
                if (length > 0)
                {
                    object[] objArray = new object[length];
                    Array.Copy(args, (int)parameters.Length, objArray, 0, (int)objArray.Length);
                    return objArray;
                }
                else
                {
                    return ScriptBlock.EmptyArray;
                }
            }
            else
            {
                return args;
            }
        }

        internal static void CacheScriptBlock(ScriptBlock scriptBlock, string fileName, string fileContents)
        {
            if (ScriptBlock._cachedScripts.Count > 0x400)
            {
                ScriptBlock._cachedScripts.Clear();
            }
            Tuple<string, string> tuple = Tuple.Create<string, string>(fileName, fileContents);
            ScriptBlock._cachedScripts.TryAdd(tuple, scriptBlock);
        }

        public void CheckRestrictedLanguage(IEnumerable<string> allowedCommands, IEnumerable<string> allowedVariables, bool allowEnvironmentVariables)
        {
            Parser parser = new Parser();
            if (this.HasBeginBlock || this.HasProcessBlock || this._ast.Body.ParamBlock != null)
            {
                NamedBlockAst beginBlock = this._ast.Body.BeginBlock;
                Ast paramBlock = beginBlock;
                if (beginBlock == null)
                {
                    NamedBlockAst processBlock = this._ast.Body.ProcessBlock;
                    paramBlock = processBlock;
                    if (processBlock == null)
                    {
                        paramBlock = this._ast.Body.ParamBlock;
                    }
                }
                Ast ast = paramBlock;
                parser.ReportError(ast.Extent, ParserStrings.InvalidScriptBlockInDataSection, new object[0]);
            }
            if (this.HasEndBlock)
            {
                RestrictedLanguageChecker restrictedLanguageChecker = new RestrictedLanguageChecker(parser, allowedCommands, allowedVariables, allowEnvironmentVariables);
                NamedBlockAst endBlock = this._ast.Body.EndBlock;
                StatementBlockAst.InternalVisit(restrictedLanguageChecker, endBlock.Traps, endBlock.Statements, AstVisitAction.Continue);
            }
            if (!parser.ErrorList.Any<ParseError>())
            {
                return;
            }
            else
            {
                throw new ParseException(parser.ErrorList.ToArray());
            }
        }

        internal ScriptBlock Clone(bool cloneHelpInfo = false)
        {
            return new ScriptBlock(this._ast, this._isFilter, this._scriptBlockData);
        }

        internal bool Compile(bool optimized)
        {
            return this._scriptBlockData.Compile(optimized);
        }

        internal static ScriptBlock Create(ExecutionContext context, string script)
        {
            ScriptBlock engineSessionState = ScriptBlock.Create(context.Engine.EngineNewParser, null, script);
            if (context.EngineSessionState != null && context.EngineSessionState.Module != null)
            {
                engineSessionState.SessionStateInternal = context.EngineSessionState;
            }
            return engineSessionState;
        }

        public static ScriptBlock Create(string script)
        {
            return ScriptBlock.Create(new Parser(), null, script);
        }

        internal static ScriptBlock Create(Parser parser, string fileName, string fileContents)
        {
            ParseError[] parseErrorArray = null;
            ScriptBlock scriptBlock = ScriptBlock.TryGetCachedScriptBlock(fileName, fileContents);
            if (scriptBlock == null)
            {
                ScriptBlockAst scriptBlockAst = parser.Parse(fileName, fileContents, null, out parseErrorArray);
                if ((int)parseErrorArray.Length == 0)
                {
                    ScriptBlock scriptBlock1 = new ScriptBlock(scriptBlockAst, false);
                    ScriptBlock.CacheScriptBlock(scriptBlock1, fileName, fileContents);
                    return scriptBlock1.Clone(false);
                }
                else
                {
                    throw new ParseException(parseErrorArray);
                }
            }
            else
            {
                return scriptBlock;
            }
        }

        internal Delegate CreateDelegate(Type delegateType)
        {
            Expression nullConstant;
            Expression expression;
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            if (!method.ContainsGenericParameters)
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
                ParameterInfo[] parameterInfoArray = parameters;
                for (int i = 0; i < (int)parameterInfoArray.Length; i++)
                {
                    ParameterInfo parameterInfo = parameterInfoArray[i];
                    parameterExpressions.Add(Expression.Parameter(parameterInfo.ParameterType));
                }
                bool flag = !method.ReturnType.Equals(typeof(void));
                if ((int)parameters.Length != 2 || flag)
                {
                    nullConstant = ExpressionCache.NullConstant;
                    expression = ExpressionCache.NullConstant;
                }
                else
                {
                    nullConstant = parameterExpressions[1].Cast(typeof(object));
                    expression = parameterExpressions[0].Cast(typeof(object));
                }
                ConstantExpression constantExpression = Expression.Constant(this);
                MethodInfo scriptBlockInvokeAsDelegateHelper = CachedReflectionInfo.ScriptBlock_InvokeAsDelegateHelper;
                Expression expression1 = nullConstant;
                Expression expression2 = expression;
                Type type = typeof(object);
                List<ParameterExpression> parameterExpressions1 = parameterExpressions;
                Expression expression3 = Expression.Call(constantExpression, scriptBlockInvokeAsDelegateHelper, expression1, expression2, Expression.NewArrayInit(type, parameterExpressions1.Select<ParameterExpression, Expression>((ParameterExpression p) => p.Cast(typeof(object)))));
                if (flag)
                {
                    expression3 = Expression.Dynamic(PSConvertBinder.Get(method.ReturnType), method.ReturnType, expression3);
                }
                return Expression.Lambda(delegateType, expression3, parameterExpressions).Compile();
            }
            else
            {
                object[] objArray = new object[2];
                objArray[0] = "CantConvertScriptBlockToOpenGenericType";
                objArray[1] = delegateType;
                throw new ScriptBlockToPowerShellNotSupportedException("CantConvertScriptBlockToOpenGenericType", null, "AutomationExceptions", objArray);
            }
        }

        internal Collection<PSObject> DoInvoke(object dollarUnder, object input, object[] args)
        {
            ArrayList arrayLists = new ArrayList();
            Pipe pipe = new Pipe(arrayLists);
            this.InvokeWithPipe(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, dollarUnder, input, AutomationNull.Value, pipe, null, args);
            return ScriptBlock.GetWrappedResult(arrayLists);
        }

        internal object DoInvokeReturnAsIs(bool useLocalScope, ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior, object dollarUnder, object input, object scriptThis, object[] args)
        {
            ArrayList arrayLists = new ArrayList();
            Pipe pipe = new Pipe(arrayLists);
            this.InvokeWithPipe(useLocalScope, errorHandlingBehavior, dollarUnder, input, scriptThis, pipe, null, args);
            return ScriptBlock.GetRawResult(arrayLists);
        }

        internal List<Attribute> GetAttributes()
        {
            return this._scriptBlockData.GetAttributes();
        }

        private Action<FunctionContext> GetCodeToInvoke(ref bool optimized)
        {
			optimized = false; //TODO: DELETE!!!
            if (this.HasBeginBlock || this.HasEndBlock && this.HasProcessBlock)
            {
                throw PSTraceSource.NewInvalidOperationException("AutomationExceptions", "ScriptBlockInvokeOnOneClauseOnly", new object[0]);
            }
            else
            {
                optimized = this._scriptBlockData.Compile(optimized);
                if (optimized == false)
                {
                    Action<FunctionContext> unoptimizedProcessBlock = this._scriptBlockData.UnoptimizedProcessBlock;
                    Action<FunctionContext> unoptimizedEndBlock = unoptimizedProcessBlock;
                    if (unoptimizedProcessBlock == null)
                    {
                        unoptimizedEndBlock = this._scriptBlockData.UnoptimizedEndBlock;
                    }
                    return unoptimizedEndBlock;
                }
                else
                {
                    Action<FunctionContext> processBlock = this._scriptBlockData.ProcessBlock;
                    Action<FunctionContext> endBlock = processBlock;
                    if (processBlock == null)
                    {
                        endBlock = this._scriptBlockData.EndBlock;
                    }
                    return endBlock;
                }
            }
        }

        internal ExecutionContext GetContextFromTLS()
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                return executionContextFromTLS;
            }
            else
            {
                string str = this.ToString();
                str = ErrorCategoryInfo.Ellipsize(Thread.CurrentThread.CurrentUICulture, str);
                object[] objArray = new object[1];
                objArray[0] = str;
                PSInvalidOperationException pSInvalidOperationException = PSTraceSource.NewInvalidOperationException("ParserStrings", "ScriptBlockDelegateInvokedFromWrongThread", objArray);
                pSInvalidOperationException.SetErrorId("ScriptBlockDelegateInvokedFromWrongThread");
                throw pSInvalidOperationException;
            }
        }

        internal Delegate GetDelegate(Type delegateType)
        {
            ConcurrentDictionary<Type, Delegate> orCreateValue = ScriptBlock.delegateTable.GetOrCreateValue(this);
            return orCreateValue.GetOrAdd(delegateType, new Func<Type, Delegate>(this.CreateDelegate));
        }

        internal string GetFileName()
        {
            return this._ast.Body.Extent.File;
        }

        internal HelpInfo GetHelpInfo(ExecutionContext context, CommandInfo commandInfo, bool dontSearchOnRemoteComputer, Dictionary<Ast, Token[]> scriptBlockTokenCache, out string helpFile, out string helpUriFromDotLink)
        {
            helpUriFromDotLink = null;
            Tuple<List<Token>, List<string>> helpCommentTokens = HelpCommentsParser.GetHelpCommentTokens(this._ast, scriptBlockTokenCache);
            if (helpCommentTokens == null)
            {
                helpFile = null;
                return null;
            }
            else
            {
                return HelpCommentsParser.CreateFromComments(context, commandInfo, helpCommentTokens.Item1, helpCommentTokens.Item2, dontSearchOnRemoteComputer, out helpFile, out helpUriFromDotLink);
            }
        }

        internal bool GetIsFilter()
        {
            return this._isFilter;
        }

        public ScriptBlock GetNewClosure()
        {
            PSModuleInfo pSModuleInfo = new PSModuleInfo(true);
            pSModuleInfo.CaptureLocals();
            return pSModuleInfo.NewBoundScriptBlock(this);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                string str = this.ToString();
                info.AddValue("ScriptText", str);
                info.SetType(typeof(ScriptBlockSerializationHelper));
                return;
            }
            else
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
        }

        public PowerShell GetPowerShell(object[] args)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            bool? nullable = null;
            return this.GetPowerShellImpl(executionContextFromTLS, null, false, nullable, args);
        }

        public PowerShell GetPowerShell(Dictionary<string, object> variables, object[] args)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            Dictionary<string, object> strs = null;
            if (variables != null)
            {
                strs = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase);
                executionContextFromTLS = null;
            }
            bool? nullable = null;
            return this.GetPowerShellImpl(executionContextFromTLS, strs, false, nullable, args);
        }

        public PowerShell GetPowerShell(Dictionary<string, object> variables, out Dictionary<string, object> usingVariables, object[] args)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            Dictionary<string, object> strs = null;
            if (variables != null)
            {
                strs = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase);
                executionContextFromTLS = null;
            }
            bool? nullable = null;
            PowerShell powerShellImpl = this.GetPowerShellImpl(executionContextFromTLS, strs, true, nullable, args);
            usingVariables = strs;
            return powerShellImpl;
        }

        internal PowerShell GetPowerShell(ExecutionContext context, bool? useLocalScope, object[] args)
        {
            return this.GetPowerShellImpl(context, null, false, useLocalScope, args);
        }

        internal PowerShell GetPowerShellImpl(ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables, bool? createLocalScope, object[] args)
        {
            return this._ast.GetPowerShell(context, variables, filterNonUsingVariables, createLocalScope, args);
        }

        internal static object GetRawResult(ArrayList result)
        {
            if (result.Count != 0)
            {
                if (result.Count != 1)
                {
                    return LanguagePrimitives.AsPSObjectOrNull(result.ToArray());
                }
                else
                {
                    return LanguagePrimitives.AsPSObjectOrNull(result[0]);
                }
            }
            else
            {
                return AutomationNull.Value;
            }
        }

        private PipelineAst GetSimplePipeline(Func<string, PipelineAst> errorHandler)
        {
            Func<string, PipelineAst> func = errorHandler;
            Func<string, PipelineAst> func1 = func;
            if (func == null)
            {
                func1 = (string argument0) => null;
            }
            errorHandler = func1;
            if (this.HasBeginBlock || this.HasProcessBlock)
            {
                return errorHandler("CanConvertOneClauseOnly");
            }
            else
            {
                ReadOnlyCollection<StatementAst> statements = this._ast.Body.EndBlock.Statements;
                if (statements.Any<StatementAst>())
                {
                    if (statements.Count <= 1)
                    {
                        if (this._ast.Body.EndBlock.Traps == null || !this._ast.Body.EndBlock.Traps.Any<TrapStatementAst>())
                        {
                            PipelineAst item = statements[0] as PipelineAst;
                            if (item != null)
                            {
                                return item;
                            }
                            else
                            {
                                return errorHandler("CanOnlyConvertOnePipeline");
                            }
                        }
                        else
                        {
                            return errorHandler("CantConvertScriptBlockWithTrap");
                        }
                    }
                    else
                    {
                        return errorHandler("CanOnlyConvertOnePipeline");
                    }
                }
                else
                {
                    return errorHandler("CantConvertEmptyPipeline");
                }
            }
        }

        internal PSToken GetStartPosition()
        {
            return new PSToken(((Ast)this._ast).Extent);
        }

        public SteppablePipeline GetSteppablePipeline()
        {
            return this.GetSteppablePipelineImpl(CommandOrigin.Internal);
        }

        public SteppablePipeline GetSteppablePipeline(CommandOrigin commandOrigin)
        {
            return this.GetSteppablePipelineImpl(commandOrigin);
        }

        internal SteppablePipeline GetSteppablePipelineImpl(CommandOrigin commandOrigin)
		{
			ScriptBlock scriptBlock = this;
            PipelineAst simplePipeline = scriptBlock.GetSimplePipeline((string errorId) => { throw PSTraceSource.NewInvalidOperationException("AutomationExceptions", errorId, new object[0]); });
			if (simplePipeline.PipelineElements[0] as CommandAst != null)
			{
				return PipelineOps.GetSteppablePipeline(simplePipeline, commandOrigin);
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException("AutomationExceptions", "CantConvertEmptyPipeline", new object[0]);
			}
		}

        internal string GetWithInputHandlingForInvokeCommand()
        {
            return this._ast.GetWithInputHandlingForInvokeCommand();
        }

        private static Collection<PSObject> GetWrappedResult(ArrayList result)
        {
            if (result == null || result.Count == 0)
            {
                return new Collection<PSObject>();
            }
            else
            {
                Collection<PSObject> pSObjects = new Collection<PSObject>();
                foreach (object obj in result)
                {
                    pSObjects.Add(LanguagePrimitives.AsPSObjectOrNull(obj));
                }
                return pSObjects;
            }
        }

        public Collection<PSObject> Invoke(object[] args)
        {
            ExecutionContext contextFromTLS = this.GetContextFromTLS();
            return this.DoInvoke(AutomationNull.Value, AutomationNull.Value, args);
        }

        internal object InvokeAsDelegateHelper(object dollarUnder, object dollarThis, object[] args)
        {
            ExecutionContext contextFromTLS = this.GetContextFromTLS();
            ArrayList arrayLists = new ArrayList();
            Pipe pipe = new Pipe(arrayLists);
            this.InvokeWithPipe(true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, dollarUnder, null, dollarThis, pipe, null, args);
            return ScriptBlock.GetRawResult(arrayLists);
        }

        public object InvokeReturnAsIs(object[] args)
        {
            ExecutionContext contextFromTLS = this.GetContextFromTLS();
            return this.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, args);
        }

        internal void InvokeUsingCmdlet(Cmdlet contextCmdlet, bool useLocalScope, ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior, object dollarUnder, object input, object scriptThis, object[] args)
        {
            Pipe outputPipe = ((MshCommandRuntime)contextCmdlet.CommandRuntime).OutputPipe;
            this.InvokeWithPipe(useLocalScope, errorHandlingBehavior, dollarUnder, input, scriptThis, outputPipe, null, args);
        }

        internal void InvokeWithPipe(bool useLocalScope, ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior, object dollarUnder, object input, object scriptThis, Pipe outputPipe, InvocationInfo invocationInfo, object[] args)
        {
            bool flag;
            Action action = null;
            ExecutionContext contextFromTLS = this.GetContextFromTLS();
            if (this.SessionStateInternal == null || this.SessionStateInternal.ExecutionContext == contextFromTLS)
            {
                RunspaceBase currentRunspace = (RunspaceBase)contextFromTLS.CurrentRunspace;
                RunspaceBase runspaceBase = currentRunspace;
                if (action == null)
                {
                    action = () => this.InvokeWithPipeImpl(useLocalScope, errorHandlingBehavior, dollarUnder, input, scriptThis, outputPipe, invocationInfo, args);
                }
                flag = !runspaceBase.RunActionIfNoRunningPipelinesWithThreadCheck(action);
            }
            else
            {
                contextFromTLS = this.SessionStateInternal.ExecutionContext;
                flag = true;
            }
            if (flag)
            {
                contextFromTLS.Events.SubscribeEvent(null, "PowerShell.OnScriptBlockInvoke", "PowerShell.OnScriptBlockInvoke", null, new PSEventReceivedEventHandler(ScriptBlock.OnScriptBlockInvokeEventHandler), true, false, true, 1);
                ScriptBlockInvocationEventArgs scriptBlockInvocationEventArg = new ScriptBlockInvocationEventArgs(this, useLocalScope, errorHandlingBehavior, dollarUnder, input, scriptThis, outputPipe, invocationInfo, args);
                object[] objArray = new object[1];
                objArray[0] = scriptBlockInvocationEventArg;
                contextFromTLS.Events.GenerateEvent("PowerShell.OnScriptBlockInvoke", null, objArray, null, true, true);
                if (scriptBlockInvocationEventArg.Exception != null)
                {
                    throw scriptBlockInvocationEventArg.Exception;
                }
            }
        }

        internal void InvokeWithPipeImpl(bool createLocalScope, ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior, object dollarUnder, object input, object scriptThis, Pipe outputPipe, InvocationInfo invocationInfo, object[] args)
        {
            bool flag;
            bool hasValue;
            ExecutionContext contextFromTLS = this.GetContextFromTLS();
            if (!contextFromTLS.CurrentPipelineStopping)
            {
                if (args == null)
                {
                    args = new object[0];
                }
                if (contextFromTLS._debuggingMode > 0)
                {
                    flag = false;
                }
                else
                {
                    flag = createLocalScope;
                }
                bool flag1 = flag;
                Action<FunctionContext> codeToInvoke = this.GetCodeToInvoke(ref flag1);
                if (codeToInvoke != null)
                {
                    if (outputPipe == null)
                    {
                        Pipe pipe = new Pipe();
                        pipe.NullPipe = true;
                        outputPipe = pipe;
                    }
                    MutableTuple mutableTuple = this.MakeLocalsTuple(flag1);
                    if (dollarUnder != AutomationNull.Value)
                    {
                        mutableTuple.SetAutomaticVariable(AutomaticVariable.Underbar, dollarUnder, contextFromTLS);
                    }
                    if (input != AutomationNull.Value)
                    {
                        mutableTuple.SetAutomaticVariable(AutomaticVariable.Input, input, contextFromTLS);
                    }
                    if (scriptThis != AutomationNull.Value)
                    {
                        mutableTuple.SetAutomaticVariable(AutomaticVariable.This, scriptThis, contextFromTLS);
                    }
                    this.SetPSScriptRootAndPSCommandPath(mutableTuple, contextFromTLS);
                    Pipe shellFunctionErrorOutputPipe = contextFromTLS.ShellFunctionErrorOutputPipe;
                    PipelineWriter externalErrorOutput = contextFromTLS.ExternalErrorOutput;
                    CommandOrigin scopeOrigin = contextFromTLS.EngineSessionState.CurrentScope.ScopeOrigin;
                    SessionStateInternal engineSessionState = contextFromTLS.EngineSessionState;
                    PSLanguageMode? nullable = null;
                    PSLanguageMode? languageMode = null;
                    PSLanguageMode? languageMode1 = this.LanguageMode;
                    if (languageMode1.HasValue)
                    {
                        PSLanguageMode? nullable1 = this.LanguageMode;
                        PSLanguageMode pSLanguageMode = contextFromTLS.LanguageMode;
                        if (nullable1.GetValueOrDefault() != pSLanguageMode)
                        {
                            hasValue = true;
                        }
                        else
                        {
                            hasValue = !nullable1.HasValue;
                        }
                        if (hasValue)
                        {
                            nullable = new PSLanguageMode?(contextFromTLS.LanguageMode);
                            languageMode = this.LanguageMode;
                        }
                    }
                    Dictionary<string, PSVariable> strs = null;
                    try
                    {
                        try
                        {
                            InvocationInfo invocationInfo1 = invocationInfo;
                            if (invocationInfo1 == null)
                            {
                                invocationInfo1 = new InvocationInfo(null, ((Ast)this._ast).Extent, contextFromTLS);
                            }
                            mutableTuple.SetAutomaticVariable(AutomaticVariable.MyInvocation, invocationInfo1, contextFromTLS);
                            if (this.SessionStateInternal != null)
                            {
                                contextFromTLS.EngineSessionState = this.SessionStateInternal;
                            }
                            ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior1 = errorHandlingBehavior;
                            switch (errorHandlingBehavior1)
                            {
                                case ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe:
                                    {
                                        WriteToCurrentErrorPipe(createLocalScope, outputPipe, ref args, contextFromTLS, codeToInvoke, mutableTuple, languageMode, ref strs);
                                        break;
                                    }
                                case ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe:
                                    {
                                        contextFromTLS.ShellFunctionErrorOutputPipe = null;
                                        WriteToCurrentErrorPipe(createLocalScope, outputPipe, ref args, contextFromTLS, codeToInvoke, mutableTuple, languageMode, ref strs);
                                        break;
                                    }
                                case ScriptBlock.ErrorHandlingBehavior.SwallowErrors:
                                    {
                                        contextFromTLS.ShellFunctionErrorOutputPipe = null;
                                        contextFromTLS.ExternalErrorOutput = new DiscardingPipelineWriter();
                                        WriteToCurrentErrorPipe(createLocalScope, outputPipe, ref args, contextFromTLS, codeToInvoke, mutableTuple, languageMode, ref strs);
                                        break;
                                    }
                                default:
                                    {
                                        WriteToCurrentErrorPipe(createLocalScope, outputPipe, ref args, contextFromTLS, codeToInvoke, mutableTuple, languageMode, ref strs);
                                        break;
                                    }
                            }
                        }
                        catch (TargetInvocationException targetInvocationException1)
                        {
                            TargetInvocationException targetInvocationException = targetInvocationException1;
                            throw targetInvocationException.InnerException;
                        }
                    }
					catch(Exception ex)
					{
						var msg = ex.Message;
					}
                    finally
                    {
                        if (nullable.HasValue)
                        {
                            contextFromTLS.LanguageMode = nullable.Value;
                        }
                        contextFromTLS.ShellFunctionErrorOutputPipe = shellFunctionErrorOutputPipe;
                        contextFromTLS.ExternalErrorOutput = externalErrorOutput;
                        contextFromTLS.EngineSessionState.CurrentScope.ScopeOrigin = scopeOrigin;
                        if (!createLocalScope)
                        {
                            if (strs != null)
                            {
                                contextFromTLS.EngineSessionState.CurrentScope.DottedScopes.Pop();
                                foreach (KeyValuePair<string, PSVariable> str in strs)
                                {
                                    if (str.Value == null)
                                    {
                                        contextFromTLS.EngineSessionState.RemoveVariable(str.Key);
                                    }
                                    else
                                    {
                                        contextFromTLS.EngineSessionState.SetVariable(str.Value, false, CommandOrigin.Internal);
                                    }
                                }
                            }
                        }
                        else
                        {
                            contextFromTLS.EngineSessionState.RemoveScope(contextFromTLS.EngineSessionState.CurrentScope);
                        }
                        contextFromTLS.EngineSessionState = engineSessionState;
                    }
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                throw new PipelineStoppedException();
            }
        }

        private void WriteToCurrentErrorPipe(bool createLocalScope, Pipe outputPipe, ref object[] args, ExecutionContext contextFromTLS, Action<FunctionContext> codeToInvoke, MutableTuple mutableTuple, PSLanguageMode? languageMode, ref Dictionary<string, PSVariable> strs)
        {
            if (!createLocalScope)
            {
                if (contextFromTLS.EngineSessionState.CurrentScope.LocalsTuple != null)
                {
                    contextFromTLS.EngineSessionState.CurrentScope.DottedScopes.Push(mutableTuple);
                    strs = new Dictionary<string, PSVariable>();
                }
                else
                {
                    contextFromTLS.EngineSessionState.CurrentScope.LocalsTuple = mutableTuple;
                }
            }
            else
            {
                SessionStateScope sessionStateScope = contextFromTLS.EngineSessionState.NewScope(false);
                contextFromTLS.EngineSessionState.CurrentScope = sessionStateScope;
                sessionStateScope.LocalsTuple = mutableTuple;
            }
            if (languageMode.HasValue)
            {
                contextFromTLS.LanguageMode = languageMode.Value;
            }
            args = ScriptBlock.BindArgumentsForScripblockInvoke((RuntimeDefinedParameter[])this.RuntimeDefinedParameters.Data, args, contextFromTLS, !createLocalScope, strs, mutableTuple);
            mutableTuple.SetAutomaticVariable(AutomaticVariable.Args, args, contextFromTLS);
            contextFromTLS.EngineSessionState.CurrentScope.ScopeOrigin = CommandOrigin.Internal;
            FunctionContext functionContext = new FunctionContext();
            functionContext._executionContext = contextFromTLS;
            functionContext._outputPipe = outputPipe;
            functionContext._localsTuple = mutableTuple;
            functionContext._scriptBlock = this;
            functionContext._sequencePoints = this.SequencePoints;
            FunctionContext functionContext1 = functionContext;
            codeToInvoke(functionContext1);
        }

        internal bool IsSingleFunctionDefinition(string functionName)
        {
            if (this.HasEndBlock && !this.HasDynamicParameters && !this.HasBeginBlock && !this.HasProcessBlock && this._ast.Body.EndBlock.Traps == null && this._ast.Body.ParamBlock == null && this._ast.Body.EndBlock.Statements.Count == 1)
            {
                FunctionDefinitionAst item = this._ast.Body.EndBlock.Statements[0] as FunctionDefinitionAst;
                if (item != null)
                {
                    return item.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        internal bool IsUsingDollarInput()
        {
            return AstSearcher.IsUsingDollarInput(this.Ast);
        }

        internal MutableTuple MakeLocalsTuple(bool createLocalScope)
        {
            MutableTuple mutableTuple;
            MutableTuple mutableTuple1;
            if (!createLocalScope)
            {
                if (this.UsesCmdletBinding)
                {
                    mutableTuple1 = MutableTuple.MakeTuple(this._scriptBlockData.UnoptimizedLocalsMutableTupleType, Compiler.DottedScriptCmdletLocalsNameIndexMap);
                }
                else
                {
                    mutableTuple1 = MutableTuple.MakeTuple(this._scriptBlockData.UnoptimizedLocalsMutableTupleType, Compiler.DottedLocalsNameIndexMap);
                }
                mutableTuple = mutableTuple1;
            }
            else
            {
                mutableTuple = MutableTuple.MakeTuple(this._scriptBlockData.LocalsMutableTupleType, this._scriptBlockData.NameToIndexMap);
            }
            return mutableTuple;
        }

        private static void OnScriptBlockInvokeEventHandler(object sender, PSEventArgs args)
        {
            ScriptBlockInvocationEventArgs sourceEventArgs = args.SourceEventArgs as ScriptBlockInvocationEventArgs;
            try
            {
                ScriptBlock scriptBlock = sourceEventArgs.ScriptBlock;
                scriptBlock.InvokeWithPipeImpl(sourceEventArgs.UseLocalScope, sourceEventArgs.ErrorHandlingBehavior, sourceEventArgs.DollarUnder, sourceEventArgs.Input, sourceEventArgs.ScriptThis, sourceEventArgs.OutputPipe, sourceEventArgs.InvocationInfo, sourceEventArgs.Args);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                sourceEventArgs.Exception = exception;
            }
        }

        internal static void SetAutomaticVariable(AutomaticVariable variable, object value, MutableTuple locals)
        {
            locals.SetValue((int)variable, value);
        }

        internal void SetIsFilter(bool value)
        {
            throw new PSInvalidOperationException();
        }

        private void SetLanguageModeFromContext()
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                this.LanguageMode = new PSLanguageMode?(executionContextFromTLS.LanguageMode);
            }
        }

        internal void SetPSScriptRootAndPSCommandPath(MutableTuple locals, ExecutionContext context)
        {
            string empty = string.Empty;
            string file = string.Empty;
            if (!string.IsNullOrEmpty(this.File))
            {
                empty = Path.GetDirectoryName(this.File);
                file = this.File;
            }
            locals.SetAutomaticVariable(AutomaticVariable.PSScriptRoot, empty, context);
            locals.SetAutomaticVariable(AutomaticVariable.PSCommandPath, file, context);
        }

        public override string ToString()
        {
            if (this._ast as ScriptBlockAst == null)
            {
                FunctionDefinitionAst functionDefinitionAst = (FunctionDefinitionAst)this._ast;
                if (functionDefinitionAst.Parameters != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("param(");
                    string str = "";
                    foreach (ParameterAst parameter in functionDefinitionAst.Parameters)
                    {
                        stringBuilder.Append(str);
                        stringBuilder.Append(parameter.ToString());
                        str = ", ";
                    }
                    stringBuilder.Append(")");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(functionDefinitionAst.Body.ToStringForSerialization());
                    return stringBuilder.ToString();
                }
                else
                {
                    return functionDefinitionAst.Body.ToStringForSerialization();
                }
            }
            else
            {
                return ((ScriptBlockAst)this._ast).ToStringForSerialization();
            }
        }

        internal static ScriptBlock TryGetCachedScriptBlock(string fileName, string fileContents)
        {
            ScriptBlock scriptBlock = null;
            Tuple<string, string> tuple = Tuple.Create<string, string>(fileName, fileContents);
            if (!ScriptBlock._cachedScripts.TryGetValue(tuple, out scriptBlock))
            {
                return null;
            }
            else
            {
                return scriptBlock.Clone(false);
            }
        }

        internal enum ErrorHandlingBehavior
        {
            WriteToCurrentErrorPipe = 1,
            WriteToExternalErrorPipe = 2,
            SwallowErrors = 3
        }
    }
}