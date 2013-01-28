namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    internal class ScriptBlockToPowerShellConverter
    {
        private ExecutionContext _context;
        private bool? _createLocalScope;
        private readonly PowerShell _powershell = PowerShell.Create();
        private object[] _usingValues;

        private ScriptBlockToPowerShellConverter()
        {
        }

        private void AddParameter(CommandParameterAst commandParameterAst)
        {
            string str;
            object expressionValue;
            if (commandParameterAst.Argument != null)
            {
                ExpressionAst argument = commandParameterAst.Argument;
                IScriptExtent errorPosition = commandParameterAst.ErrorPosition;
                str = ((errorPosition.EndLineNumber != argument.Extent.StartLineNumber) || (errorPosition.EndColumnNumber != argument.Extent.StartColumnNumber)) ? ": " : ":";
                expressionValue = this.GetExpressionValue(commandParameterAst.Argument);
            }
            else
            {
                str = "";
                expressionValue = null;
            }
            this._powershell.AddParameter(string.Format(CultureInfo.InvariantCulture, "-{0}{1}", new object[] { commandParameterAst.ParameterName, str }), expressionValue);
        }

        internal static PowerShell Convert(ScriptBlockAst body, IEnumerable<ParameterAst> functionParameters, ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables, bool? createLocalScope, object[] args)
        {
            string str;
            string str2;
            PowerShell shell;
            ExecutionContext.CheckStackDepth();
            if (args == null)
            {
                args = ScriptBlock.EmptyArray;
            }
            body.GetSimplePipeline(false, out str, out str2);
            if (str != null)
            {
                throw new ScriptBlockToPowerShellNotSupportedException(str, null, str2, new object[0]);
            }
            ScriptBlockToPowerShellChecker visitor = new ScriptBlockToPowerShellChecker {
                ScriptBeingConverted = body
            };
            if (functionParameters != null)
            {
                foreach (ParameterAst ast in functionParameters)
                {
                    ast.InternalVisit(visitor);
                }
            }
            body.InternalVisit(visitor);
            if (((context == null) && (visitor.HasUsingExpr || visitor.UsesParameter)) && (variables == null))
            {
                throw new PSInvalidOperationException(AutomationExceptions.CantConvertScriptBlockWithNoContext);
            }
            try
            {
                ScriptBlockToPowerShellConverter converter = new ScriptBlockToPowerShellConverter {
                    _context = context,
                    _createLocalScope = createLocalScope
                };
                if (visitor.HasUsingExpr)
                {
                    converter._usingValues = GetUsingValues(body, context, variables, filterNonUsingVariables);
                }
                if (visitor.UsesParameter)
                {
                    SessionStateScope scope = context.EngineSessionState.NewScope(false);
                    context.EngineSessionState.CurrentScope = scope;
                    context.EngineSessionState.CurrentScope.ScopeOrigin = CommandOrigin.Internal;
                    MutableTuple locals = MutableTuple.MakeTuple(Compiler.DottedLocalsTupleType, Compiler.DottedLocalsNameIndexMap);
                    bool usesCmdletBinding = false;
                    object[] objArray = ScriptBlock.BindArgumentsForScripblockInvoke((RuntimeDefinedParameter[]) ((IParameterMetadataProvider) body).GetParameterMetadata(true, ref usesCmdletBinding).Data, args, context, false, null, locals);
                    locals.SetAutomaticVariable(AutomaticVariable.Args, objArray, context);
                    scope.LocalsTuple = locals;
                }
                foreach (PipelineAst ast2 in body.EndBlock.Statements.OfType<PipelineAst>())
                {
                    converter._powershell.AddStatement();
                    converter.ConvertPipeline(ast2);
                }
                shell = converter._powershell;
            }
            finally
            {
                if (visitor.UsesParameter)
                {
                    context.EngineSessionState.RemoveScope(context.EngineSessionState.CurrentScope);
                }
            }
            return shell;
        }

        private void ConvertCommand(CommandAst commandAst)
        {
            Command command = new Command(this.GetCommandName(commandAst.CommandElements[0]), false, this._createLocalScope);
            if (commandAst.Redirections.Count > 0)
            {
                PipelineResultTypes all;
                PipelineResultTypes output = PipelineResultTypes.Output;
                switch (commandAst.Redirections[0].FromStream)
                {
                    case RedirectionStream.All:
                        all = PipelineResultTypes.All;
                        break;

                    case RedirectionStream.Error:
                        all = PipelineResultTypes.Error;
                        break;

                    case RedirectionStream.Warning:
                        all = PipelineResultTypes.Warning;
                        break;

                    case RedirectionStream.Verbose:
                        all = PipelineResultTypes.Verbose;
                        break;

                    case RedirectionStream.Debug:
                        all = PipelineResultTypes.Debug;
                        break;

                    default:
                        all = PipelineResultTypes.Error;
                        break;
                }
                command.MergeMyResults(all, output);
            }
            this._powershell.AddCommand(command);
            foreach (CommandElementAst ast in commandAst.CommandElements.Skip<CommandElementAst>(1))
            {
                ExpressionAst exprAst = ast as ExpressionAst;
                if (exprAst != null)
                {
                    VariableExpressionAst variableAst = null;
                    UsingExpressionAst ast4 = ast as UsingExpressionAst;
                    if (ast4 != null)
                    {
                        variableAst = ast4.SubExpression as VariableExpressionAst;
                        if ((variableAst != null) && variableAst.Splatted)
                        {
                            IDictionary parameters = this._usingValues[ast4.RuntimeUsingIndex] as IDictionary;
                            if (parameters != null)
                            {
                                this._powershell.AddParameters(parameters);
                            }
                            else
                            {
                                IEnumerable enumerable = this._usingValues[ast4.RuntimeUsingIndex] as IEnumerable;
                                if (enumerable != null)
                                {
                                    foreach (object obj2 in enumerable)
                                    {
                                        this._powershell.AddArgument(obj2);
                                    }
                                }
                                else
                                {
                                    this._powershell.AddArgument(this._usingValues[ast4.RuntimeUsingIndex]);
                                }
                            }
                        }
                        else
                        {
                            this._powershell.AddArgument(this._usingValues[ast4.RuntimeUsingIndex]);
                        }
                    }
                    else
                    {
                        variableAst = ast as VariableExpressionAst;
                        if ((variableAst != null) && variableAst.Splatted)
                        {
                            this.GetSplattedVariable(variableAst);
                        }
                        else
                        {
                            object expressionValue;
                            ConstantExpressionAst ast5 = ast as ConstantExpressionAst;
                            if ((ast5 != null) && LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(ast5.StaticType)))
                            {
                                string text = ast5.Extent.Text;
                                expressionValue = ast5.Value;
                                if (!text.Equals(ast5.Value.ToString(), StringComparison.Ordinal))
                                {
                                    expressionValue = ParserOps.WrappedNumber(expressionValue, text);
                                }
                            }
                            else
                            {
                                expressionValue = this.GetExpressionValue(exprAst);
                            }
                            this._powershell.AddArgument(expressionValue);
                        }
                    }
                }
                else
                {
                    this.AddParameter((CommandParameterAst) ast);
                }
            }
        }

        private void ConvertPipeline(PipelineAst pipelineAst)
        {
            foreach (CommandBaseAst ast in pipelineAst.PipelineElements)
            {
                this.ConvertCommand((CommandAst) ast);
            }
        }

        private string GetCommandName(CommandElementAst commandNameAst)
        {
            string name;
            ExpressionAst exprAst = commandNameAst as ExpressionAst;
            if (exprAst != null)
            {
                object expressionValue = this.GetExpressionValue(exprAst);
                if (expressionValue == null)
                {
                    ScriptBlockToPowerShellChecker.ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlockInvocation", null, AutomationExceptions.CantConvertWithScriptBlockInvocation, new object[0]), exprAst);
                }
                if (expressionValue is CommandInfo)
                {
                    name = ((CommandInfo) expressionValue).Name;
                }
                else
                {
                    name = expressionValue as string;
                }
            }
            else
            {
                name = commandNameAst.Extent.Text;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlockInvocation", null, AutomationExceptions.CantConvertWithScriptBlockInvocation, new object[0]);
            }
            return name;
        }

        private object GetExpressionValue(ExpressionAst exprAst)
        {
            object obj2;
            if (IsConstantValueVisitor.IsConstant(exprAst, out obj2, false, false))
            {
                return obj2;
            }
            if (this._context == null)
            {
                Runspace runspace = RunspaceFactory.CreateRunspace(InitialSessionState.Create());
                runspace.Open();
                this._context = runspace.ExecutionContext;
            }
            return Compiler.GetExpressionValue(exprAst, this._context, this._usingValues);
        }

        private void GetSplattedVariable(VariableExpressionAst variableAst)
        {
            if (this._context == null)
            {
                throw new PSInvalidOperationException(AutomationExceptions.CantConvertScriptBlockWithNoContext);
            }
            foreach (CommandParameterInternal internal2 in PipelineOps.Splat(this._context.GetVariableValue(variableAst.VariablePath), variableAst.Extent))
            {
                CommandParameter parameter = CommandParameter.FromCommandParameterInternal(internal2);
                this._powershell.AddParameter(parameter.Name, parameter.Value);
            }
        }

        internal static object[] GetUsingValues(ScriptBlock scriptBlock, ExecutionContext context, Dictionary<string, object> variables)
        {
            return GetUsingValues(scriptBlock.Ast, context, variables, false);
        }

        private static object[] GetUsingValues(Ast body, ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables)
        {
            List<Ast> list = UsingExpressionAstSearcher.FindAllUsingExpressionExceptForWorkflow(body).ToList<Ast>();
            object[] objArray = new object[list.Count];
            HashSet<string> set = ((variables != null) && filterNonUsingVariables) ? new HashSet<string>() : null;
            UsingExpressionAst usingExpr = null;
            Version strictModeVersion = null;
            try
            {
                if (context != null)
                {
                    strictModeVersion = context.EngineSessionState.CurrentScope.StrictModeVersion;
                    context.EngineSessionState.CurrentScope.StrictModeVersion = PSVersionInfo.PSVersion;
                }
                for (int i = 0; i < objArray.Length; i++)
                {
                    usingExpr = (UsingExpressionAst) list[i];
                    if (IsUsingExpressionInFunction(usingExpr, body))
                    {
                        throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), usingExpr.Extent, "UsingVariableNotSupportedInFunctionOrFilter", AutomationExceptions.UsingVariableNotSupportedInFunctionOrFilter, new object[] { usingExpr });
                    }
                    object obj2 = null;
                    if (variables != null)
                    {
                        VariableExpressionAst subExpression = usingExpr.SubExpression as VariableExpressionAst;
                        if (subExpression == null)
                        {
                            throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), usingExpr.Extent, "CantGetUsingExpressionValueWithSpecifiedVariableDictionary", AutomationExceptions.CantGetUsingExpressionValueWithSpecifiedVariableDictionary, new object[] { usingExpr.Extent.Text });
                        }
                        string userPath = subExpression.VariablePath.UserPath;
                        if (((userPath != null) && variables.TryGetValue(userPath, out obj2)) && (set != null))
                        {
                            set.Add(userPath);
                        }
                    }
                    else
                    {
                        obj2 = Compiler.GetExpressionValue(usingExpr.SubExpression, context, (IList)null);
                    }
                    objArray[i] = obj2;
                    usingExpr.RuntimeUsingIndex = i;
                }
            }
            catch (RuntimeException exception)
            {
                if (exception.ErrorRecord.FullyQualifiedErrorId.Equals("VariableIsUndefined", StringComparison.Ordinal))
                {
                    throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), usingExpr.Extent, "UsingVariableIsUndefined", AutomationExceptions.UsingVariableIsUndefined, new object[] { exception.ErrorRecord.TargetObject });
                }
                if (exception.ErrorRecord.FullyQualifiedErrorId.Equals("UsingVariableNotSupportedInFunctionOrFilter", StringComparison.Ordinal) || exception.ErrorRecord.FullyQualifiedErrorId.Equals("CantGetUsingExpressionValueWithSpecifiedVariableDictionary", StringComparison.Ordinal))
                {
                    throw;
                }
            }
            finally
            {
                if (context != null)
                {
                    context.EngineSessionState.CurrentScope.StrictModeVersion = strictModeVersion;
                }
            }
            if (set != null)
            {
                foreach (string str2 in variables.Keys.ToArray<string>())
                {
                    if (!set.Contains(str2))
                    {
                        variables.Remove(str2);
                    }
                }
            }
            return objArray;
        }

        internal static bool IsUsingExpressionInFunction(UsingExpressionAst usingExpr, Ast topLevelParent)
        {
            for (Ast ast = usingExpr.Parent; ast != null; ast = ast.Parent)
            {
                FunctionDefinitionAst ast2 = ast as FunctionDefinitionAst;
                if ((ast2 != null) && !ast2.IsWorkflow)
                {
                    return true;
                }
                if (ast == topLevelParent)
                {
                    break;
                }
            }
            return false;
        }
    }
}

