namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class PseudoParameterBinder
    {
        private Collection<CommandParameterAst> _ambiguousParameters;
        private Collection<AstParameterArgumentPair> _arguments;
        private MergedCommandParameterMetadata _bindableParameters;
        private bool _bindingEffective = true;
        private Dictionary<string, AstParameterArgumentPair> _boundArguments;
        private Dictionary<string, MergedCompiledCommandParameter> _boundParameters;
        private Collection<string> _boundPositionalParameter;
        private CommandAst _commandAst;
        private ReadOnlyCollection<CommandElementAst> _commandElements;
        private CommandInfo _commandInfo;
        private string _commandName;
        private int _currentParameterSetFlag = int.MaxValue;
        private int _defaultParameterSetFlag;
        private Collection<AstParameterArgumentPair> _duplicateParameters;
        private bool _function;
        private bool _isPipelineInputExpected;
        private Collection<CommandParameterAst> _parametersNotFound;
        private Type _pipelineInputType;
        private List<MergedCompiledCommandParameter> _unboundParameters;
        private static List<string> ignoredWorkflowParameters = new List<string> { "OutVariable", "OutBuffer", "ErrorVariable", "WarningVariable", "WhatIf", "Confirm", "UseTransaction" };

        private Collection<AstParameterArgumentPair> BindNamedParameters()
        {
            Collection<AstParameterArgumentPair> collection = new Collection<AstParameterArgumentPair>();
            if (this._bindingEffective)
            {
                foreach (AstParameterArgumentPair pair in this._arguments)
                {
                    if (!pair.ParameterSpecified)
                    {
                        collection.Add(pair);
                    }
                    else
                    {
                        MergedCompiledCommandParameter item = null;
                        try
                        {
                            item = this._bindableParameters.GetMatchingParameter(pair.ParameterName, false, true, null);
                        }
                        catch (ParameterBindingException)
                        {
                            this._ambiguousParameters.Add(pair.Parameter);
                            goto Label_013E;
                        }
                        if (item == null)
                        {
                            this._parametersNotFound.Add(pair.Parameter);
                        }
                        else if (this._boundParameters.ContainsKey(item.Parameter.Name))
                        {
                            this._duplicateParameters.Add(pair);
                        }
                        else
                        {
                            if (item.Parameter.ParameterSetFlags != 0)
                            {
                                this._currentParameterSetFlag &= item.Parameter.ParameterSetFlags;
                            }
                            this._unboundParameters.Remove(item);
                            if (!this._boundParameters.ContainsKey(item.Parameter.Name))
                            {
                                this._boundParameters.Add(item.Parameter.Name, item);
                            }
                            if (!this._boundArguments.ContainsKey(item.Parameter.Name))
                            {
                                this._boundArguments.Add(item.Parameter.Name, pair);
                            }
                        }
                    Label_013E:;
                    }
                }
            }
            return collection;
        }

        private void BindPipelineParameters()
        {
            bool flag = false;
            int num = 0;
            if (this._bindingEffective && this._isPipelineInputExpected)
            {
                List<MergedCompiledCommandParameter> list = new List<MergedCompiledCommandParameter>(this._unboundParameters);
                foreach (MergedCompiledCommandParameter parameter in list)
                {
                    if (parameter.Parameter.IsPipelineParameterInSomeParameterSet && (((parameter.Parameter.ParameterSetFlags & this._currentParameterSetFlag) != 0) || parameter.Parameter.IsInAllSets))
                    {
                        foreach (ParameterSetSpecificMetadata metadata in parameter.Parameter.GetMatchingParameterSetData(this._currentParameterSetFlag))
                        {
                            if (metadata.ValueFromPipeline)
                            {
                                num |= parameter.Parameter.ParameterSetFlags;
                                string name = parameter.Parameter.Name;
                                this._unboundParameters.Remove(parameter);
                                if (!this._boundParameters.ContainsKey(name))
                                {
                                    this._boundParameters.Add(name, parameter);
                                }
                                if (!this._boundArguments.ContainsKey(name))
                                {
                                    this._boundArguments.Add(name, new PipeObjectPair(name, this._pipelineInputType));
                                }
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (flag && (num != 0))
                {
                    this._currentParameterSetFlag &= num;
                }
            }
        }

        private Collection<AstParameterArgumentPair> BindPositionalParameter(Collection<AstParameterArgumentPair> unboundArguments, int validParameterSetFlags, int defaultParameterSetFlag, bool honorDefaultParameterSet)
        {
            Collection<AstParameterArgumentPair> nonPositionalArguments = new Collection<AstParameterArgumentPair>();
            if (this._bindingEffective && (unboundArguments.Count > 0))
            {
                SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> dictionary;
                List<AstParameterArgumentPair> unboundArgumentsCollection = new List<AstParameterArgumentPair>(unboundArguments);
                try
                {
                    dictionary = ParameterBinderController.EvaluateUnboundPositionalParameters(this._unboundParameters, validParameterSetFlags);
                }
                catch (InvalidOperationException)
                {
                    this._bindingEffective = false;
                    return nonPositionalArguments;
                }
                if (dictionary.Count == 0)
                {
                    return unboundArguments;
                }
                int unboundArgumentsIndex = 0;
                foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> dictionary2 in dictionary.Values)
                {
                    if (dictionary2.Count != 0)
                    {
                        AstParameterArgumentPair argument = GetNextPositionalArgument(unboundArgumentsCollection, nonPositionalArguments, ref unboundArgumentsIndex);
                        if (argument == null)
                        {
                            break;
                        }
                        bool flag = false;
                        if ((honorDefaultParameterSet && (defaultParameterSetFlag != 0)) && ((validParameterSetFlags & defaultParameterSetFlag) != 0))
                        {
                            flag = this.BindPseudoPositionalParameterInSet(defaultParameterSetFlag, dictionary2, argument, false);
                        }
                        if (!flag)
                        {
                            flag = this.BindPseudoPositionalParameterInSet(validParameterSetFlags, dictionary2, argument, true);
                        }
                        if (!flag)
                        {
                            nonPositionalArguments.Add(argument);
                        }
                        else if (validParameterSetFlags != this._currentParameterSetFlag)
                        {
                            validParameterSetFlags = this._currentParameterSetFlag;
                            ParameterBinderController.UpdatePositionalDictionary(dictionary, validParameterSetFlags);
                        }
                    }
                }
                for (int i = unboundArgumentsIndex; i < unboundArgumentsCollection.Count; i++)
                {
                    nonPositionalArguments.Add(unboundArgumentsCollection[i]);
                }
            }
            return nonPositionalArguments;
        }

        private bool BindPseudoPositionalParameterInSet(int validParameterSetFlag, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters, AstParameterArgumentPair argument, bool typeConversion)
        {
            bool flag = false;
            int num = 0;
            foreach (PositionalCommandParameter parameter in nextPositionalParameters.Values)
            {
                foreach (ParameterSetSpecificMetadata metadata in parameter.ParameterSetData)
                {
                    if (((validParameterSetFlag & metadata.ParameterSetFlag) != 0) || metadata.IsInAllSets)
                    {
                        bool flag2 = false;
                        string name = parameter.Parameter.Parameter.Name;
                        Type paramType = parameter.Parameter.Parameter.Type;
                        Type argumentType = argument.ArgumentType;
                        if (argumentType.Equals(typeof(object)))
                        {
                            flag = flag2 = true;
                        }
                        else if (IsTypeEquivalent(argumentType, paramType))
                        {
                            flag = flag2 = true;
                        }
                        else if (typeConversion)
                        {
                            flag = flag2 = true;
                        }
                        if (flag2)
                        {
                            num |= parameter.Parameter.Parameter.ParameterSetFlags;
                            this._unboundParameters.Remove(parameter.Parameter);
                            if (!this._boundParameters.ContainsKey(name))
                            {
                                this._boundParameters.Add(name, parameter.Parameter);
                                this._boundPositionalParameter.Add(name);
                            }
                            if (!this._boundArguments.ContainsKey(name))
                            {
                                this._boundArguments.Add(name, argument);
                            }
                            break;
                        }
                    }
                }
            }
            if (flag && (num != 0))
            {
                this._currentParameterSetFlag &= num;
            }
            return flag;
        }

        private void BindRemainingParameters(Collection<AstParameterArgumentPair> unboundArguments)
        {
            bool flag = false;
            int num = 0;
            if (this._bindingEffective && (unboundArguments.Count != 0))
            {
                Collection<ExpressionAst> arguments = new Collection<ExpressionAst>();
                foreach (AstParameterArgumentPair pair in unboundArguments)
                {
                    AstPair pair2 = pair as AstPair;
                    arguments.Add((ExpressionAst) pair2.Argument);
                }
                List<MergedCompiledCommandParameter> list = new List<MergedCompiledCommandParameter>(this._unboundParameters);
                foreach (MergedCompiledCommandParameter parameter in list)
                {
                    if (((parameter.Parameter.ParameterSetFlags & this._currentParameterSetFlag) != 0) || parameter.Parameter.IsInAllSets)
                    {
                        foreach (ParameterSetSpecificMetadata metadata in parameter.Parameter.GetMatchingParameterSetData(this._currentParameterSetFlag))
                        {
                            if (metadata.ValueFromRemainingArguments)
                            {
                                num |= parameter.Parameter.ParameterSetFlags;
                                string name = parameter.Parameter.Name;
                                this._unboundParameters.Remove(parameter);
                                if (!this._boundParameters.ContainsKey(name))
                                {
                                    this._boundParameters.Add(name, parameter);
                                }
                                if (!this._boundArguments.ContainsKey(name))
                                {
                                    this._boundArguments.Add(name, new AstArrayPair(name, arguments));
                                }
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (flag && (num != 0))
                {
                    this._currentParameterSetFlag &= num;
                }
            }
        }

        private static ScriptBlock CreateFakeScriptBlockForWorkflow(FunctionDefinitionAst functionDefinitionAst)
        {
            Token[] tokenArray;
            ParseError[] errorArray;
            StringBuilder builder = new StringBuilder();
            ReadOnlyCollection<ParameterAst> parameters = ((IParameterMetadataProvider) functionDefinitionAst).Parameters;
            if (parameters != null)
            {
                bool flag = true;
                foreach (ParameterAst ast in parameters)
                {
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    flag = false;
                    builder.Append(ast.Extent.Text);
                }
                if (!flag)
                {
                    builder.Append(", ");
                }
            }
            return Parser.ParseInput(string.Format(CultureInfo.InvariantCulture, "\r\n                [CmdletBinding()]\r\n                param (\r\n                    {0}\r\n                    [hashtable[]] $PSParameterCollection,\r\n                    [string[]] $PSComputerName,\r\n                    [ValidateNotNullOrEmpty()] $PSCredential,\r\n                    [int32] $PSConnectionRetryCount,\r\n                    [int32] $PSConnectionRetryIntervalSec,\r\n                    [ValidateRange(1, 2147483)][int32] $PSRunningTimeoutSec,\r\n                    [ValidateRange(1, 2147483)][int32] $PSElapsedTimeoutSec,\r\n                    [bool] $PSPersist,\r\n                    [ValidateNotNullOrEmpty()] [System.Management.Automation.Runspaces.AuthenticationMechanism] $PSAuthentication,\r\n                    [ValidateNotNullOrEmpty()][System.Management.AuthenticationLevel] $PSAuthenticationLevel,\r\n                    [ValidateNotNullOrEmpty()] [string] $PSApplicationName,\r\n                    [int32] $PSPort,\r\n                    [switch] $PSUseSSL,\r\n                    [ValidateNotNullOrEmpty()] [string] $PSConfigurationName,\r\n                    [ValidateNotNullOrEmpty()][string[]] $PSConnectionURI,\r\n                    [switch] $PSAllowRedirection,\r\n                    [ValidateNotNullOrEmpty()][System.Management.Automation.Remoting.PSSessionOption] $PSSessionOption,\r\n                    [ValidateNotNullOrEmpty()] [string] $PSCertificateThumbprint,\r\n                    [hashtable] $PSPrivateMetadata,\r\n                    [switch] $AsJob,\r\n                    [string] $JobName,\r\n                    [Parameter(ValueFromPipeline=$true)]$InputObject\r\n                    )\r\n", new object[] { builder.ToString() }), out tokenArray, out errorArray).GetScriptBlock();
        }

        internal PseudoBindingInfo DoPseudoParameterBinding(CommandAst command, Type pipeArgumentType, CommandParameterAst paramAstAtCursor, bool isForArgumentCompletion)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this.InitializeMembers();
            this._commandAst = command;
            this._commandElements = command.CommandElements;
            this._bindingEffective = this.PrepareCommandElements(LocalPipeline.GetExecutionContextFromTLS());
            if (this._bindingEffective && (this._isPipelineInputExpected || (pipeArgumentType != null)))
            {
                this._pipelineInputType = pipeArgumentType;
            }
            this._bindingEffective = this.ParseParameterArguments(paramAstAtCursor);
            if (this._bindingEffective)
            {
                Collection<AstParameterArgumentPair> unboundArguments = this.BindNamedParameters();
                this._bindingEffective = this._currentParameterSetFlag != 0;
                unboundArguments = this.BindPositionalParameter(unboundArguments, this._currentParameterSetFlag, this._defaultParameterSetFlag, isForArgumentCompletion);
                if (!this._function)
                {
                    this.BindRemainingParameters(unboundArguments);
                    this.BindPipelineParameters();
                }
            }
            if (this._bindingEffective)
            {
                return new PseudoBindingInfo(this._commandInfo, this._currentParameterSetFlag, this._defaultParameterSetFlag, this._boundParameters, this._unboundParameters, this._boundArguments, this._boundPositionalParameter, this._arguments, this._parametersNotFound, this._ambiguousParameters, this._duplicateParameters);
            }
            if (this._bindableParameters == null)
            {
                return null;
            }
            this._unboundParameters.Clear();
            this._unboundParameters.AddRange(this._bindableParameters.BindableParameters.Values);
            return new PseudoBindingInfo(this._commandInfo, this._defaultParameterSetFlag, this._arguments, this._unboundParameters);
        }

        private static Type GetActualActivityParameterType(Type parameterType)
        {
            if (parameterType.IsGenericType)
            {
                string fullName = parameterType.GetGenericTypeDefinition().FullName;
                if (fullName.Equals("System.Activities.InArgument`1", StringComparison.Ordinal) || fullName.Equals("System.Activities.InOutArgument`1", StringComparison.Ordinal))
                {
                    parameterType = parameterType.GetGenericArguments()[0];
                }
            }
            parameterType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
            return parameterType;
        }

        private static AstParameterArgumentPair GetNextPositionalArgument(List<AstParameterArgumentPair> unboundArgumentsCollection, Collection<AstParameterArgumentPair> nonPositionalArguments, ref int unboundArgumentsIndex)
        {
            while (unboundArgumentsIndex < unboundArgumentsCollection.Count)
            {
                AstParameterArgumentPair item = unboundArgumentsCollection[unboundArgumentsIndex++];
                if (!item.ParameterSpecified)
                {
                    return item;
                }
                nonPositionalArguments.Add(item);
            }
            return null;
        }

        private void InitializeMembers()
        {
            this._function = false;
            this._commandName = null;
            this._currentParameterSetFlag = int.MaxValue;
            this._defaultParameterSetFlag = 0;
            this._bindableParameters = null;
            this._arguments = this._arguments ?? new Collection<AstParameterArgumentPair>();
            this._boundParameters = this._boundParameters ?? new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            this._boundArguments = this._boundArguments ?? new Dictionary<string, AstParameterArgumentPair>(StringComparer.OrdinalIgnoreCase);
            this._unboundParameters = this._unboundParameters ?? new List<MergedCompiledCommandParameter>();
            this._boundPositionalParameter = this._boundPositionalParameter ?? new Collection<string>();
            this._arguments.Clear();
            this._boundParameters.Clear();
            this._unboundParameters.Clear();
            this._boundArguments.Clear();
            this._boundPositionalParameter.Clear();
            this._pipelineInputType = null;
            this._bindingEffective = true;
            this._isPipelineInputExpected = false;
            this._parametersNotFound = this._parametersNotFound ?? new Collection<CommandParameterAst>();
            this._ambiguousParameters = this._ambiguousParameters ?? new Collection<CommandParameterAst>();
            this._duplicateParameters = this._duplicateParameters ?? new Collection<AstParameterArgumentPair>();
            this._parametersNotFound.Clear();
            this._ambiguousParameters.Clear();
            this._duplicateParameters.Clear();
        }

        private static bool IsTypeEquivalent(Type argType, Type paramType)
        {
            bool flag = false;
            if (argType.IsEquivalentTo(paramType))
            {
                return true;
            }
            if (argType.IsSubclassOf(paramType))
            {
                return true;
            }
            if (argType.IsEquivalentTo(paramType.GetElementType()))
            {
                return true;
            }
            if (argType.IsSubclassOf(typeof(Array)) && paramType.IsSubclassOf(typeof(Array)))
            {
                flag = true;
            }
            return flag;
        }

        private bool ParseParameterArguments(CommandParameterAst paramAstAtCursor)
        {
            if (!this._bindingEffective)
            {
                return this._bindingEffective;
            }
            Collection<AstParameterArgumentPair> collection = new Collection<AstParameterArgumentPair>();
            for (int i = 0; i < this._arguments.Count; i++)
            {
                AstParameterArgumentPair item = this._arguments[i];
                if (!item.ParameterSpecified || item.ArgumentSpecified)
                {
                    collection.Add(item);
                }
                else
                {
                    string parameterName = item.ParameterName;
                    MergedCompiledCommandParameter parameter = null;
                    try
                    {
                        bool tryExactMatching = item.Parameter != paramAstAtCursor;
                        parameter = this._bindableParameters.GetMatchingParameter(parameterName, false, tryExactMatching, null);
                    }
                    catch (ParameterBindingException)
                    {
                        this._ambiguousParameters.Add(item.Parameter);
                        goto Label_01F1;
                    }
                    if (parameter == null)
                    {
                        if (i < (this._arguments.Count - 1))
                        {
                            AstParameterArgumentPair pair2 = this._arguments[i + 1];
                            if (!pair2.ParameterSpecified && pair2.ArgumentSpecified)
                            {
                                this._arguments = null;
                                return false;
                            }
                        }
                        this._parametersNotFound.Add(item.Parameter);
                    }
                    else if (parameter.Parameter.Type == typeof(SwitchParameter))
                    {
                        SwitchPair pair3 = new SwitchPair(item.Parameter);
                        collection.Add(pair3);
                    }
                    else if (i < (this._arguments.Count - 1))
                    {
                        AstParameterArgumentPair pair4 = this._arguments[i + 1];
                        if (pair4.ParameterSpecified)
                        {
                            try
                            {
                                if (this._bindableParameters.GetMatchingParameter(pair4.ParameterName, false, true, null) == null)
                                {
                                    AstPair pair5 = new AstPair(item.Parameter, pair4.Parameter);
                                    collection.Add(pair5);
                                    i++;
                                }
                                else
                                {
                                    FakePair pair6 = new FakePair(item.Parameter);
                                    collection.Add(pair6);
                                }
                                goto Label_01F1;
                            }
                            catch (ParameterBindingException)
                            {
                                FakePair pair7 = new FakePair(item.Parameter);
                                collection.Add(pair7);
                                goto Label_01F1;
                            }
                        }
                        AstPair pair8 = pair4 as AstPair;
                        AstPair pair9 = new AstPair(item.Parameter, (ExpressionAst) pair8.Argument);
                        collection.Add(pair9);
                        i++;
                    }
                    else
                    {
                        FakePair pair10 = new FakePair(item.Parameter);
                        collection.Add(pair10);
                    }
                Label_01F1:;
                }
            }
            this._arguments = collection;
            return true;
        }

        private bool PrepareCommandElements(ExecutionContext context)
        {
            int num = 0;
            bool dotSource = this._commandAst.InvocationOperator == TokenKind.Dot;
            CommandProcessorBase base2 = null;
            string resolvedCommandName = null;
            bool flag2 = false;
            try
            {
                base2 = this.PrepareFromAst(context, out resolvedCommandName) ?? context.CreateCommand(resolvedCommandName, dotSource);
            }
            catch (RuntimeException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                if ((this._commandAst.IsInWorkflow() && (resolvedCommandName != null)) && CompletionCompleters.PseudoWorkflowCommands.Contains<string>(resolvedCommandName, StringComparer.OrdinalIgnoreCase))
                {
                    flag2 = true;
                }
                else
                {
                    return false;
                }
            }
            CommandProcessor commandProcessor = base2 as CommandProcessor;
            ScriptCommandProcessorBase base3 = base2 as ScriptCommandProcessorBase;
            bool flag3 = (commandProcessor != null) && commandProcessor.CommandInfo.ImplementsDynamicParameters;
            List<object> list = flag3 ? new List<object>(this._commandElements.Count) : null;
            if (((commandProcessor != null) || (base3 != null)) || flag2)
            {
                num++;
                while (num < this._commandElements.Count)
                {
                    CommandParameterAst parameterAst = this._commandElements[num] as CommandParameterAst;
                    if (parameterAst != null)
                    {
                        if (list != null)
                        {
                            list.Add(parameterAst.Extent.Text);
                        }
                        AstPair item = (parameterAst.Argument != null) ? new AstPair(parameterAst, parameterAst.Argument) : new AstPair(parameterAst);
                        this._arguments.Add(item);
                    }
                    else
                    {
                        StringConstantExpressionAst ast2 = this._commandElements[num] as StringConstantExpressionAst;
                        if ((ast2 == null) || !ast2.Value.Trim().Equals("-", StringComparison.OrdinalIgnoreCase))
                        {
                            ExpressionAst argumentAst = this._commandElements[num] as ExpressionAst;
                            if (argumentAst != null)
                            {
                                if (list != null)
                                {
                                    list.Add(argumentAst.Extent.Text);
                                }
                                this._arguments.Add(new AstPair(null, argumentAst));
                            }
                        }
                    }
                    num++;
                }
            }
            if (commandProcessor != null)
            {
                this._function = false;
                if (flag3)
                {
                    ParameterBinderController.AddArgumentsToCommandProcessor(commandProcessor, list.ToArray());
                    bool flag4 = false;
                    bool flag5 = false;
                    do
                    {
                        CommandProcessorBase currentCommandProcessor = context.CurrentCommandProcessor;
                        try
                        {
                            context.CurrentCommandProcessor = commandProcessor;
                            commandProcessor.SetCurrentScopeToExecutionScope();
                            if (!flag4)
                            {
                                commandProcessor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(commandProcessor.arguments);
                            }
                            else
                            {
                                flag5 = true;
                                commandProcessor.CmdletParameterBinderController.ClearUnboundArguments();
                                commandProcessor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(new Collection<CommandParameterInternal>());
                            }
                        }
                        catch (ParameterBindingException exception2)
                        {
                            if ((exception2.ErrorId == "MissingArgument") || (exception2.ErrorId == "AmbiguousParameter"))
                            {
                                flag4 = true;
                            }
                        }
                        catch (Exception exception3)
                        {
                            CommandProcessorBase.CheckForSevereException(exception3);
                        }
                        finally
                        {
                            context.CurrentCommandProcessor = currentCommandProcessor;
                            commandProcessor.RestorePreviousScope();
                        }
                    }
                    while (flag4 && !flag5);
                }
                this._commandInfo = commandProcessor.CommandInfo;
                this._commandName = commandProcessor.CommandInfo.Name;
                this._bindableParameters = commandProcessor.CmdletParameterBinderController.BindableParameters;
                this._defaultParameterSetFlag = commandProcessor.CommandInfo.CommandMetadata.DefaultParameterSetFlag;
            }
            else if (base3 != null)
            {
                this._function = true;
                this._commandInfo = base3.CommandInfo;
                this._commandName = base3.CommandInfo.Name;
                this._bindableParameters = base3.ScriptParameterBinderController.BindableParameters;
                this._defaultParameterSetFlag = 0;
            }
            else if (!flag2)
            {
                return false;
            }
            if (this._commandAst.IsInWorkflow())
            {
                Type type = Type.GetType("Microsoft.PowerShell.Workflow.AstToWorkflowConverter, Microsoft.PowerShell.Activities, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                if (type != null)
                {
                    Dictionary<string, Type> dictionary = (Dictionary<string, Type>) type.GetMethod("GetActivityParameters").Invoke(null, new object[] { this._commandAst });
                    if (dictionary != null)
                    {
                        bool flag6 = dictionary.ContainsKey("PSComputerName") && !dictionary.ContainsKey("ComputerName");
                        List<MergedCompiledCommandParameter> source = new List<MergedCompiledCommandParameter>();
                        Collection<Attribute> attributes = new Collection<Attribute> {
                            new ParameterAttribute()
                        };
                        foreach (KeyValuePair<string, Type> pair2 in dictionary)
                        {
                            if (flag2 || !this._bindableParameters.BindableParameters.ContainsKey(pair2.Key))
                            {
                                Type actualActivityParameterType = GetActualActivityParameterType(pair2.Value);
                                RuntimeDefinedParameter runtimeDefinedParameter = new RuntimeDefinedParameter(pair2.Key, actualActivityParameterType, attributes);
                                CompiledCommandParameter parameter = new CompiledCommandParameter(runtimeDefinedParameter, false) {
                                    IsInAllSets = true
                                };
                                MergedCompiledCommandParameter parameter3 = new MergedCompiledCommandParameter(parameter, ParameterBinderAssociation.DeclaredFormalParameters);
                                source.Add(parameter3);
                            }
                        }
                        if (source.Any<MergedCompiledCommandParameter>())
                        {
                            MergedCommandParameterMetadata metadata = new MergedCommandParameterMetadata();
                            if (!flag2)
                            {
                                metadata.ReplaceMetadata(this._bindableParameters);
                            }
                            foreach (MergedCompiledCommandParameter parameter5 in source)
                            {
                                metadata.BindableParameters.Add(parameter5.Parameter.Name, parameter5);
                            }
                            this._bindableParameters = metadata;
                        }
                        foreach (string str2 in ignoredWorkflowParameters)
                        {
                            if (this._bindableParameters.BindableParameters.ContainsKey(str2))
                            {
                                this._bindableParameters.BindableParameters.Remove(str2);
                            }
                        }
                        if (this._bindableParameters.BindableParameters.ContainsKey("ComputerName") && flag6)
                        {
                            this._bindableParameters.BindableParameters.Remove("ComputerName");
                            string key = (from aliasPair in this._bindableParameters.AliasedParameters
                                where string.Equals("ComputerName", aliasPair.Value.Parameter.Name)
                                select aliasPair.Key).FirstOrDefault<string>();
                            this._bindableParameters.AliasedParameters.Remove(key);
                        }
                    }
                }
            }
            this._unboundParameters.AddRange(this._bindableParameters.BindableParameters.Values);
            CommandBaseAst ast4 = null;
            PipelineAst parent = this._commandAst.Parent as PipelineAst;
            if (parent.PipelineElements.Count > 1)
            {
                foreach (CommandBaseAst ast6 in parent.PipelineElements)
                {
                    if (ast6.GetHashCode() == this._commandAst.GetHashCode())
                    {
                        this._isPipelineInputExpected = ast4 != null;
                        if (this._isPipelineInputExpected)
                        {
                            this._pipelineInputType = typeof(object);
                        }
                        break;
                    }
                    ast4 = ast6;
                }
            }
            return true;
        }

        private CommandProcessorBase PrepareFromAst(ExecutionContext context, out string resolvedCommandName)
        {
            string str;
            FunctionDefinitionAst ast2;
            ExportVisitor astVisitor = new ExportVisitor();
            Ast parent = this._commandAst;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            parent.Visit(astVisitor);
            resolvedCommandName = this._commandAst.GetCommandName();
            CommandProcessorBase base2 = null;
            int num = 0;
            while (astVisitor.DiscoveredAliases.TryGetValue(resolvedCommandName, out str))
            {
                num++;
                if (num > 5)
                {
                    break;
                }
                resolvedCommandName = str;
            }
            if (astVisitor.DiscoveredFunctions.TryGetValue(resolvedCommandName, out ast2))
            {
                ScriptBlock scriptblock = ast2.IsWorkflow ? CreateFakeScriptBlockForWorkflow(ast2) : new ScriptBlock(ast2, ast2.IsFilter);
                base2 = CommandDiscovery.CreateCommandProcessorForScript(scriptblock, context, true, context.EngineSessionState);
            }
            return base2;
        }
    }
}

