namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class CmdletParameterBinderController : ParameterBinderController
    {
        private List<string> _aliasList;
        private Dictionary<MergedCompiledCommandParameter, object> _allDefaultParameterValuePairs;
        private readonly CommandMetadata _commandMetadata;
        private readonly MshCommandRuntime _commandRuntime;
        private ReflectionParameterBinder _commonParametersBinder;
        private readonly Dictionary<string, CommandParameterInternal> _defaultParameterValues;
        private readonly Dictionary<MergedCompiledCommandParameter, DelayedScriptBlockArgument> _delayBindScriptBlocks;
        private ParameterBinderBase _dynamicParameterBinder;
        private ReflectionParameterBinder _pagingParameterBinder;
        private int _parameterSetToBePrioritizedInPipelingBinding;
        private ReflectionParameterBinder _shouldProcessParameterBinder;
        [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
        private static readonly PSTraceSource _tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");
        private ReflectionParameterBinder _transactionParameterBinder;
        private bool _useDefaultParameterBinding;
        private HashSet<string> _warningSet;
        private const string Separator = ":::";

        internal CmdletParameterBinderController(Cmdlet cmdlet, CommandMetadata commandMetadata, ParameterBinderBase parameterBinder)
            : base(cmdlet.MyInvocation, cmdlet.Context, parameterBinder)
        {
            this._warningSet = new HashSet<string>();
            this._useDefaultParameterBinding = true;
            this._delayBindScriptBlocks = new Dictionary<MergedCompiledCommandParameter, DelayedScriptBlockArgument>();
            this._defaultParameterValues = new Dictionary<string, CommandParameterInternal>(StringComparer.OrdinalIgnoreCase);
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetadata");
            }
            this.Command = cmdlet;
            this._commandRuntime = (MshCommandRuntime)cmdlet.CommandRuntime;
            this._commandMetadata = commandMetadata;
            if (commandMetadata.ImplementsDynamicParameters)
            {
                base.UnboundParameters = base.BindableParameters.ReplaceMetadata(commandMetadata.StaticCommandParameterMetadata);
                base.BindableParameters.GenerateParameterSetMappingFromMetadata(commandMetadata.DefaultParameterSetName);
            }
            else
            {
                base._bindableParameters = commandMetadata.StaticCommandParameterMetadata;
                base.UnboundParameters = new List<MergedCompiledCommandParameter>(base._bindableParameters.BindableParameters.Values);
            }
        }

        private void ApplyDefaultParameterBinding(string bindingStage, bool isDynamic)
        {
            if (this._useDefaultParameterBinding)
            {
                if (isDynamic)
                {
                    this._allDefaultParameterValuePairs = this.GetDefaultParameterValuePairs(false);
                }
                Dictionary<MergedCompiledCommandParameter, object> qualifiedParameterValuePairs = this.GetQualifiedParameterValuePairs(base._currentParameterSetFlag, this._allDefaultParameterValuePairs);
                if (qualifiedParameterValuePairs != null)
                {
                    bool flag = false;
                    using (ParameterBinderBase.bindingTracer.TraceScope("BIND DEFAULT <parameter, value> pairs after [{0}] for [{1}]", new object[] { bindingStage, this._commandMetadata.Name }))
                    {
                        flag = this.BindDefaultParameters(base._currentParameterSetFlag, qualifiedParameterValuePairs);
                        if (flag && !base.DefaultParameterBindingInUse)
                        {
                            base.DefaultParameterBindingInUse = true;
                        }
                    }
                    _tracer.WriteLine("BIND DEFAULT after [{0}] result [{1}]", new object[] { bindingStage, flag });
                }
            }
        }

        private bool AtLeastOneUnboundValidParameterSetTakesPipelineInput(int validParameterSetFlags)
        {
            foreach (MergedCompiledCommandParameter parameter in base.UnboundParameters)
            {
                if (parameter.Parameter.DoesParameterSetTakePipelineInput(validParameterSetFlags))
                {
                    return true;
                }
            }
            return false;
        }

        private void BackupDefaultParameter(MergedCompiledCommandParameter parameter)
        {
            if (!this._defaultParameterValues.ContainsKey(parameter.Parameter.Name))
            {
                object defaultParameterValue = this.GetDefaultParameterValue(parameter.Parameter.Name);
                this._defaultParameterValues.Add(parameter.Parameter.Name, CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameter.Parameter.Name, "-" + parameter.Parameter.Name + ":", PositionUtilities.EmptyExtent, defaultParameterValue, false));
            }
        }

        internal void BindCommandLineParameters(Collection<CommandParameterInternal> arguments)
        {
            int num;
            _tracer.WriteLine("Argument count: {0}", new object[] { arguments.Count });
            if (this._commandMetadata.Obsolete != null)
            {
                if (this._commandMetadata.Obsolete.IsError)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(this._commandMetadata.Obsolete.Message);
                    exception.SetErrorId("UseOfDeprecatedCmdlet");
                    exception.ErrorRecord.SetTargetObject(this._commandMetadata.Name);
                    throw exception;
                }
                this._commandRuntime.WriteWarning(this._commandMetadata.Obsolete.Message);
            }
            this.BindCommandLineParametersNoValidation(arguments);
            bool isPipelineInputExpected = !this._commandRuntime.IsClosed || !this._commandRuntime.InputPipe.Empty;
            if (!isPipelineInputExpected)
            {
                num = this.ValidateParameterSets(false, true);
            }
            else
            {
                num = this.ValidateParameterSets(true, false);
            }
            if ((num == 1) && !base.DefaultParameterBindingInUse)
            {
                this.ApplyDefaultParameterBinding("Mandatory Checking", false);
            }
            if ((num > 1) && isPipelineInputExpected)
            {
                int num2 = this.FilterParameterSetsTakingNoPipelineInput();
                if (num2 != base._currentParameterSetFlag)
                {
                    base._currentParameterSetFlag = num2;
                    num = this.ValidateParameterSets(true, false);
                }
            }
            IDisposable disposable = ParameterBinderBase.bindingTracer.TraceScope("MANDATORY PARAMETER CHECK on cmdlet [{0}]", new object[] { this._commandMetadata.Name });
            try
            {
                Collection<MergedCompiledCommandParameter> collection;
                this.HandleUnboundMandatoryParameters(num, true, isPipelineInputExpected, out collection);
                if (base.DefaultParameterBinder is ScriptParameterBinder)
                {
                    base.BindUnboundScriptParameters();
                }
            }
            catch (ParameterBindingException exception2)
            {
                if (!base.DefaultParameterBindingInUse)
                {
                    throw;
                }
                base.ThrowElaboratedBindingException(exception2);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            if (!isPipelineInputExpected)
            {
                this.VerifyParameterSetSelected();
            }
            base._prePipelineProcessingParameterSetFlags = base._currentParameterSetFlag;
        }

        internal void BindCommandLineParametersNoValidation(Collection<CommandParameterInternal> arguments)
        {
            ParameterBindingException exception;
            ParameterBindingException exception2;
            PSScriptCmdlet command = this.Command as PSScriptCmdlet;
            if (command != null)
            {
                command.PrepareForBinding(((ScriptParameterBinder)base.DefaultParameterBinder).LocalScope, base.CommandLineParameters);
            }
            foreach (CommandParameterInternal internal2 in arguments)
            {
                base.UnboundArguments.Add(internal2);
            }
            CommandMetadata metadata = this._commandMetadata;
            this._warningSet.Clear();
            this._allDefaultParameterValuePairs = this.GetDefaultParameterValuePairs(true);
            base.DefaultParameterBindingInUse = false;
            base.BoundDefaultParameters.Clear();
            base.ReparseUnboundArguments();
            using (ParameterBinderBase.bindingTracer.TraceScope("BIND NAMED cmd line args [{0}]", new object[] { this._commandMetadata.Name }))
            {
                base.UnboundArguments = this.BindParameters(base._currentParameterSetFlag, base.UnboundArguments);
            }
            using (ParameterBinderBase.bindingTracer.TraceScope("BIND POSITIONAL cmd line args [{0}]", new object[] { this._commandMetadata.Name }))
            {
                base.UnboundArguments = base.BindPositionalParameters(base.UnboundArguments, base._currentParameterSetFlag, metadata.DefaultParameterSetFlag, out exception2);
                exception = exception2;
            }
            this.ApplyDefaultParameterBinding("POSITIONAL BIND", false);
            this.ValidateParameterSets(true, false);
            this.HandleCommandLineDynamicParameters(out exception2);
            this.ApplyDefaultParameterBinding("DYNAMIC BIND", true);
            if (exception == null)
            {
                exception = exception2;
            }
            this.HandleRemainingArguments();
            this.VerifyArgumentsProcessed(exception);
        }

        private bool BindDefaultParameters(int validParameterSetFlag, Dictionary<MergedCompiledCommandParameter, object> defaultParameterValues)
        {
            bool flag = false;
            foreach (MergedCompiledCommandParameter parameter in defaultParameterValues.Keys)
            {
                object obj2 = defaultParameterValues[parameter];
                string name = parameter.Parameter.Name;
                try
                {
                    ScriptBlock block = obj2 as ScriptBlock;
                    if (block != null)
                    {
                        PSObject obj3 = this.WrapBindingState();
                        Collection<PSObject> collection = block.Invoke(new object[] { obj3 });
                        if ((collection == null) || (collection.Count == 0))
                        {
                            continue;
                        }
                        if (collection.Count == 1)
                        {
                            obj2 = collection[0];
                        }
                        else
                        {
                            obj2 = collection;
                        }
                    }
                    CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, name, "-" + name + ":", PositionUtilities.EmptyExtent, obj2, false);
                    bool flag2 = this.BindParameter(validParameterSetFlag, argument, parameter, ParameterBindingFlags.DelayBindScriptBlock | ParameterBindingFlags.ShouldCoerceType);
                    if (flag2 && !flag)
                    {
                        flag = true;
                    }
                    if (flag2)
                    {
                        base.BoundDefaultParameters.Add(name);
                    }
                }
                catch (ParameterBindingException exception)
                {
                    if (!this._warningSet.Contains(this._commandMetadata.Name + ":::" + name))
                    {
                        string text = string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.FailToBindDefaultParameter, new object[] { LanguagePrimitives.IsNull(obj2) ? "null" : obj2.ToString(), name, exception.Message });
                        this._commandRuntime.WriteWarning(text);
                        this._warningSet.Add(this._commandMetadata.Name + ":::" + name);
                    }
                }
            }
            return flag;
        }

        private bool BindParameter(CommandParameterInternal argument, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = false;
            switch (parameter.BinderAssociation)
            {
                case ParameterBinderAssociation.DeclaredFormalParameters:
                    flag = base.DefaultParameterBinder.BindParameter(argument, parameter.Parameter, flags);
                    break;

                case ParameterBinderAssociation.DynamicParameters:
                    if (this._dynamicParameterBinder != null)
                    {
                        flag = this._dynamicParameterBinder.BindParameter(argument, parameter.Parameter, flags);
                    }
                    break;

                case ParameterBinderAssociation.CommonParameters:
                    flag = this.CommonParametersBinder.BindParameter(argument, parameter.Parameter, flags);
                    break;

                case ParameterBinderAssociation.ShouldProcessParameters:
                    flag = this.ShouldProcessParametersBinder.BindParameter(argument, parameter.Parameter, flags);
                    break;

                case ParameterBinderAssociation.TransactionParameters:
                    flag = this.TransactionParametersBinder.BindParameter(argument, parameter.Parameter, flags);
                    break;

                case ParameterBinderAssociation.PagingParameters:
                    flag = this.PagingParametersBinder.BindParameter(argument, parameter.Parameter, flags);
                    break;
            }
            if (flag && ((flags & ParameterBindingFlags.IsDefaultValue) == ParameterBindingFlags.None))
            {
                if (parameter.Parameter.ParameterSetFlags != 0)
                {
                    base._currentParameterSetFlag &= parameter.Parameter.ParameterSetFlags;
                }
                base.UnboundParameters.Remove(parameter);
                if (!base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                {
                    base.BoundParameters.Add(parameter.Parameter.Name, parameter);
                }
                if (!base.BoundArguments.ContainsKey(parameter.Parameter.Name))
                {
                    base.BoundArguments.Add(parameter.Parameter.Name, argument);
                }
            }
            return flag;
        }

        internal override bool BindParameter(int parameterSets, CommandParameterInternal argument, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = true;
            if ((((flags & ParameterBindingFlags.DelayBindScriptBlock) != ParameterBindingFlags.None) && parameter.Parameter.DoesParameterSetTakePipelineInput(parameterSets)) && argument.ArgumentSpecified)
            {
                object argumentValue = argument.ArgumentValue;
                if (((argumentValue is ScriptBlock) || (argumentValue is DelayedScriptBlockArgument)) && !IsParameterScriptBlockBindable(parameter))
                {
                    if (this._commandRuntime.IsClosed && this._commandRuntime.InputPipe.Empty)
                    {
                        ParameterBindingException exception = new ParameterBindingException(ErrorCategory.MetadataError, this.Command.MyInvocation, base.GetErrorExtent(argument), parameter.Parameter.Name, parameter.Parameter.Type, null, "ParameterBinderStrings", "ScriptBlockArgumentNoInput", new object[0]);
                        throw exception;
                    }
                    ParameterBinderBase.bindingTracer.WriteLine("Adding ScriptBlock to delay-bind list for parameter '{0}'", new object[] { parameter.Parameter.Name });
                    DelayedScriptBlockArgument argument2 = argumentValue as DelayedScriptBlockArgument;
                    if (argument2 == null)
                    {
                        argument2 = new DelayedScriptBlockArgument
                        {
                            _argument = argument,
                            _parameterBinder = this
                        };
                    }
                    if (!this._delayBindScriptBlocks.ContainsKey(parameter))
                    {
                        this._delayBindScriptBlocks.Add(parameter, argument2);
                    }
                    if (parameter.Parameter.ParameterSetFlags != 0)
                    {
                        base._currentParameterSetFlag &= parameter.Parameter.ParameterSetFlags;
                    }
                    base.UnboundParameters.Remove(parameter);
                    if (!base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                    {
                        base.BoundParameters.Add(parameter.Parameter.Name, parameter);
                    }
                    if (!base.BoundArguments.ContainsKey(parameter.Parameter.Name))
                    {
                        base.BoundArguments.Add(parameter.Parameter.Name, argument);
                    }
                    if (base.DefaultParameterBinder.RecordBoundParameters && !base.DefaultParameterBinder.CommandLineParameters.ContainsKey(parameter.Parameter.Name))
                    {
                        base.DefaultParameterBinder.CommandLineParameters.Add(parameter.Parameter.Name, argument2);
                    }
                    flag = false;
                }
            }
            bool flag2 = false;
            if (flag)
            {
                try
                {
                    flag2 = this.BindParameter(argument, parameter, flags);
                }
                catch (Exception innerException)
                {
                    bool flag3 = true;
                    if ((flags & ParameterBindingFlags.ShouldCoerceType) == ParameterBindingFlags.None)
                    {
                        while (innerException != null)
                        {
                            if (innerException is PSInvalidCastException)
                            {
                                flag3 = false;
                                break;
                            }
                            innerException = innerException.InnerException;
                        }
                    }
                    if (flag3)
                    {
                        throw;
                    }
                }
            }
            return flag2;
        }

        internal override Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters)
        {
            return this.BindParameters(int.MaxValue, parameters);
        }

        private Collection<CommandParameterInternal> BindParameters(int parameterSets, Collection<CommandParameterInternal> arguments)
        {
            Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
            foreach (CommandParameterInternal internal2 in arguments)
            {
                if (!internal2.ParameterNameSpecified)
                {
                    collection.Add(internal2);
                }
                else
                {
                    MergedCompiledCommandParameter parameter = base.BindableParameters.GetMatchingParameter(internal2.ParameterName, false, true, new InvocationInfo(base.InvocationInfo.MyCommand, internal2.ParameterExtent));
                    if (parameter != null)
                    {
                        if (base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                        {
                            ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, base.InvocationInfo, base.GetParameterErrorExtent(internal2), internal2.ParameterName, null, null, "ParameterBinderStrings", "ParameterAlreadyBound", new object[0]);
                            throw exception;
                        }
                        if (((parameter.Parameter.ParameterSetFlags & parameterSets) == 0) && !parameter.Parameter.IsInAllSets)
                        {
                            string parameterSetName = base.BindableParameters.GetParameterSetName(parameterSets);
                            ParameterBindingException pbex = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, internal2.ParameterName, null, null, "ParameterBinderStrings", "ParameterNotInParameterSet", new object[] { parameterSetName });
                            if (!base.DefaultParameterBindingInUse)
                            {
                                throw pbex;
                            }
                            base.ThrowElaboratedBindingException(pbex);
                        }
                        try
                        {
                            this.BindParameter(parameterSets, internal2, parameter, ParameterBindingFlags.DelayBindScriptBlock | ParameterBindingFlags.ShouldCoerceType);
                        }
                        catch (ParameterBindingException exception3)
                        {
                            if (!base.DefaultParameterBindingInUse)
                            {
                                throw;
                            }
                            base.ThrowElaboratedBindingException(exception3);
                        }
                    }
                    else if (internal2.ParameterName.Equals("-%", StringComparison.Ordinal))
                    {
                        base.DefaultParameterBinder.CommandLineParameters.SetImplicitUsingParameters(internal2.ArgumentValue);
                    }
                    else
                    {
                        collection.Add(internal2);
                    }
                }
            }
            return collection;
        }

        private bool BindPipelineParameter(object parameterValue, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = false;
            if (parameterValue != AutomationNull.Value)
            {
                object[] args = new object[] { parameter.Parameter.Name, parameterValue ?? "null" };
                _tracer.WriteLine("Adding PipelineParameter name={0}; value={1}", args);
                this.BackupDefaultParameter(parameter);
                CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameter.Parameter.Name, "-" + parameter.Parameter.Name + ":", PositionUtilities.EmptyExtent, parameterValue, false);
                flags &= ~ParameterBindingFlags.DelayBindScriptBlock;
                flag = this.BindParameter(base._currentParameterSetFlag, argument, parameter, flags);
                if (flag)
                {
                    base.ParametersBoundThroughPipelineInput.Add(parameter);
                }
            }
            return flag;
        }

        internal bool BindPipelineParameters(PSObject inputToOperateOn)
        {
            bool flag;
            try
            {
                using (ParameterBinderBase.bindingTracer.TraceScope("BIND PIPELINE object to parameters: [{0}]", new object[] { this._commandMetadata.Name }))
                {
                    bool flag2;
                    bool flag3 = this.InvokeAndBindDelayBindScriptBlock(inputToOperateOn, out flag2);
                    bool flag4 = !flag2 || flag3;
                    bool flag5 = false;
                    if (flag4)
                    {
                        flag5 = this.BindPipelineParametersPrivate(inputToOperateOn);
                    }
                    flag = (flag2 && flag3) || flag5;
                }
            }
            catch (ParameterBindingException)
            {
                this.RestoreDefaultParameterValues(base.ParametersBoundThroughPipelineInput);
                throw;
            }
            try
            {
                this.VerifyParameterSetSelected();
            }
            catch (ParameterBindingException)
            {
                this.RestoreDefaultParameterValues(base.ParametersBoundThroughPipelineInput);
                throw;
            }
            if (!flag)
            {
                this.RestoreDefaultParameterValues(base.ParametersBoundThroughPipelineInput);
            }
            return flag;
        }

        private bool BindPipelineParametersPrivate(PSObject inputToOperateOn)
        {
            ConsolidatedString str;
            object[] args = new object[] { ((inputToOperateOn == null) || (inputToOperateOn == AutomationNull.Value)) ? "null" : ((((str = inputToOperateOn.InternalTypeNames).Count > 0) && (str[0] != null)) ? str[0] : inputToOperateOn.BaseObject.GetType().FullName) };
            ParameterBinderBase.bindingTracer.WriteLine("PIPELINE object TYPE = [{0}]", args);
            bool flag = false;
            ParameterBinderBase.bindingTracer.WriteLine("RESTORING pipeline parameter's original values", new object[0]);
            this.RestoreDefaultParameterValues(base.ParametersBoundThroughPipelineInput);
            base.ParametersBoundThroughPipelineInput.Clear();
            base._currentParameterSetFlag = base._prePipelineProcessingParameterSetFlags;
            int validParameterSets = base._currentParameterSetFlag;
            bool flag2 = this._parameterSetToBePrioritizedInPipelingBinding != 0;
            int num2 = flag2 ? 2 : 1;
            if (flag2)
            {
                validParameterSets = this._parameterSetToBePrioritizedInPipelingBinding;
            }
            for (int i = 0; i < num2; i++)
            {
                for (CurrentlyBinding binding = CurrentlyBinding.ValueFromPipelineNoCoercion; binding <= CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion; binding += 1)
                {
                    if (this.BindUnboundParametersForBindingState(inputToOperateOn, binding, ref validParameterSets))
                    {
                        if (flag2)
                        {
                            if (i == 1)
                            {
                                this.ValidateParameterSets(true, true);
                            }
                            validParameterSets = base._currentParameterSetFlag & validParameterSets;
                        }
                        else
                        {
                            this.ValidateParameterSets(true, true);
                            validParameterSets = base._currentParameterSetFlag;
                        }
                        flag = true;
                    }
                }
                if (flag2 && (i == 0))
                {
                    if (base._currentParameterSetFlag == this._parameterSetToBePrioritizedInPipelingBinding)
                    {
                        break;
                    }
                    validParameterSets = base._currentParameterSetFlag & ~this._parameterSetToBePrioritizedInPipelingBinding;
                }
            }
            this.ValidateParameterSets(false, true);
            if (!base.DefaultParameterBindingInUse)
            {
                this.ApplyDefaultParameterBinding("PIPELINE BIND", false);
            }
            return flag;
        }

        private bool BindUnboundParametersForBindingState(PSObject inputToOperateOn, CurrentlyBinding currentlyBinding, ref int validParameterSets)
        {
            bool flag = false;
            int num = validParameterSets;
            int defaultParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
            if ((defaultParameterSetFlag != 0) && ((validParameterSets & defaultParameterSetFlag) != 0))
            {
                int num3 = defaultParameterSetFlag;
                flag = this.BindUnboundParametersForBindingStateInParameterSet(inputToOperateOn, currentlyBinding, ref num3);
                if (!flag)
                {
                    num &= ~defaultParameterSetFlag;
                }
                else
                {
                    num = defaultParameterSetFlag;
                }
            }
            if (!flag)
            {
                flag = this.BindUnboundParametersForBindingStateInParameterSet(inputToOperateOn, currentlyBinding, ref num);
                if (flag)
                {
                    validParameterSets = num;
                }
            }
            _tracer.WriteLine("aParameterWasBound = {0}", new object[] { flag });
            return flag;
        }

        private bool BindUnboundParametersForBindingStateInParameterSet(PSObject inputToOperateOn, CurrentlyBinding currentlyBinding, ref int validParameterSets)
        {
            bool flag = false;
            List<MergedCompiledCommandParameter> list = new List<MergedCompiledCommandParameter>(base.UnboundParameters);
            foreach (MergedCompiledCommandParameter parameter in list)
            {
                if (parameter.Parameter.IsPipelineParameterInSomeParameterSet && (((validParameterSets & parameter.Parameter.ParameterSetFlags) != 0) || parameter.Parameter.IsInAllSets))
                {
                    IEnumerable<ParameterSetSpecificMetadata> matchingParameterSetData = parameter.Parameter.GetMatchingParameterSetData(validParameterSets);
                    bool flag2 = false;
                    foreach (ParameterSetSpecificMetadata metadata in matchingParameterSetData)
                    {
                        if ((currentlyBinding == CurrentlyBinding.ValueFromPipelineNoCoercion) && metadata.ValueFromPipeline)
                        {
                            flag2 = this.BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.None);
                        }
                        else if (((currentlyBinding == CurrentlyBinding.ValueFromPipelineByPropertyNameNoCoercion) && metadata.ValueFromPipelineByPropertyName) && (inputToOperateOn != null))
                        {
                            flag2 = this.BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.None);
                        }
                        else if ((currentlyBinding == CurrentlyBinding.ValueFromPipelineWithCoercion) && metadata.ValueFromPipeline)
                        {
                            flag2 = this.BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
                        }
                        else if (((currentlyBinding == CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion) && metadata.ValueFromPipelineByPropertyName) && (inputToOperateOn != null))
                        {
                            flag2 = this.BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
                        }
                        if (flag2)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        private bool BindValueFromPipeline(PSObject inputToOperateOn, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = false;
            ParameterBinderBase.bindingTracer.WriteLine(((flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None) ? "Parameter [{0}] PIPELINE INPUT ValueFromPipeline WITH COERCION" : "Parameter [{0}] PIPELINE INPUT ValueFromPipeline NO COERCION", new object[] { parameter.Parameter.Name });
            ParameterBindingException pbex = null;
            try
            {
                flag = this.BindPipelineParameter(inputToOperateOn, parameter, flags);
            }
            catch (ParameterBindingArgumentTransformationException exception2)
            {
                PSInvalidCastException innerException;
                if (exception2.InnerException is ArgumentTransformationMetadataException)
                {
                    innerException = exception2.InnerException.InnerException as PSInvalidCastException;
                }
                else
                {
                    innerException = exception2.InnerException as PSInvalidCastException;
                }
                if (innerException == null)
                {
                    pbex = exception2;
                }
                flag = false;
            }
            catch (ParameterBindingValidationException exception4)
            {
                pbex = exception4;
            }
            catch (ParameterBindingParameterDefaultValueException exception5)
            {
                pbex = exception5;
            }
            catch (ParameterBindingException)
            {
                flag = false;
            }
            if (pbex != null)
            {
                if (!base.DefaultParameterBindingInUse)
                {
                    throw pbex;
                }
                base.ThrowElaboratedBindingException(pbex);
            }
            return flag;
        }

        private bool BindValueFromPipelineByPropertyName(PSObject inputToOperateOn, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = false;
            ParameterBinderBase.bindingTracer.WriteLine(((flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None) ? "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName WITH COERCION" : "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName NO COERCION", new object[] { parameter.Parameter.Name });
            PSMemberInfo info = inputToOperateOn.Properties[parameter.Parameter.Name];
            if (info == null)
            {
                foreach (string str in parameter.Parameter.Aliases)
                {
                    info = inputToOperateOn.Properties[str];
                    if (info != null)
                    {
                        break;
                    }
                }
            }
            if (info != null)
            {
                ParameterBindingException pbex = null;
                try
                {
                    flag = this.BindPipelineParameter(info.Value, parameter, flags);
                }
                catch (ParameterBindingArgumentTransformationException exception2)
                {
                    pbex = exception2;
                }
                catch (ParameterBindingValidationException exception3)
                {
                    pbex = exception3;
                }
                catch (ParameterBindingParameterDefaultValueException exception4)
                {
                    pbex = exception4;
                }
                catch (ParameterBindingException)
                {
                    flag = false;
                }
                if (pbex == null)
                {
                    return flag;
                }
                if (!base.DefaultParameterBindingInUse)
                {
                    throw pbex;
                }
                base.ThrowElaboratedBindingException(pbex);
            }
            return flag;
        }

        private static string BuildLabel(string parameterName, StringBuilder usedHotKeys)
        {
            bool flag = false;
            StringBuilder builder = new StringBuilder(parameterName);
            string str = usedHotKeys.ToString();
            for (int i = 0; i < parameterName.Length; i++)
            {
                if (char.IsUpper(parameterName[i]) && (str.IndexOf(parameterName[i]) == -1))
                {
                    builder.Insert(i, '&');
                    usedHotKeys.Append(parameterName[i]);
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                for (int j = 0; j < parameterName.Length; j++)
                {
                    if (char.IsLower(parameterName[j]) && (str.IndexOf(parameterName[j]) == -1))
                    {
                        builder.Insert(j, '&');
                        usedHotKeys.Append(parameterName[j]);
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                for (int k = 0; k < parameterName.Length; k++)
                {
                    if (!char.IsLetter(parameterName[k]) && (str.IndexOf(parameterName[k]) == -1))
                    {
                        builder.Insert(k, '&');
                        usedHotKeys.Append(parameterName[k]);
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                builder.Insert(0, '&');
            }
            return builder.ToString();
        }

        internal static string BuildMissingParamsString(Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
        {
            StringBuilder builder = new StringBuilder();
            foreach (MergedCompiledCommandParameter parameter in missingMandatoryParameters)
            {
                builder.AppendFormat(" {0}", parameter.Parameter.Name);
            }
            return builder.ToString();
        }

        private Collection<FieldDescription> CreatePromptDataStructures(Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
        {
            StringBuilder usedHotKeys = new StringBuilder();
            Collection<FieldDescription> collection = new Collection<FieldDescription>();
            foreach (MergedCompiledCommandParameter parameter in missingMandatoryParameters)
            {
                ParameterSetSpecificMetadata parameterSetData = parameter.Parameter.GetParameterSetData(base._currentParameterSetFlag);
                FieldDescription item = new FieldDescription(parameter.Parameter.Name);
                string helpMessage = null;
                try
                {
                    helpMessage = parameterSetData.GetHelpMessage(this.Command);
                }
                catch (InvalidOperationException)
                {
                }
                catch (ArgumentException)
                {
                }
                if (!string.IsNullOrEmpty(helpMessage))
                {
                    item.HelpMessage = helpMessage;
                }
                item.SetParameterType(parameter.Parameter.Type);
                item.Label = BuildLabel(parameter.Parameter.Name, usedHotKeys);
                foreach (ValidateArgumentsAttribute attribute in parameter.Parameter.ValidationAttributes)
                {
                    item.Attributes.Add(attribute);
                }
                foreach (ArgumentTransformationAttribute attribute2 in parameter.Parameter.ArgumentTransformationAttributes)
                {
                    item.Attributes.Add(attribute2);
                }
                item.IsMandatory = true;
                collection.Add(item);
            }
            return collection;
        }

        private int FilterParameterSetsTakingNoPipelineInput()
        {
            int num = 0;
            bool flag = false;
            foreach (KeyValuePair<MergedCompiledCommandParameter, DelayedScriptBlockArgument> pair in this._delayBindScriptBlocks)
            {
                num |= pair.Key.Parameter.ParameterSetFlags;
            }
            foreach (MergedCompiledCommandParameter parameter in base.UnboundParameters)
            {
                if (!parameter.Parameter.IsPipelineParameterInSomeParameterSet)
                {
                    continue;
                }
                foreach (ParameterSetSpecificMetadata metadata in parameter.Parameter.GetMatchingParameterSetData(base._currentParameterSetFlag))
                {
                    if (metadata.ValueFromPipeline || metadata.ValueFromPipelineByPropertyName)
                    {
                        if ((metadata.ParameterSetFlag == 0) && metadata.IsInAllSets)
                        {
                            num = 0;
                            flag = true;
                            break;
                        }
                        num |= metadata.ParameterSetFlag;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            if (num != 0)
            {
                return (base._currentParameterSetFlag & num);
            }
            return base._currentParameterSetFlag;
        }

        private List<string> GetAliasOfCurrentCmdlet()
        {
            List<string> list = base.Context.SessionState.Internal.GetAliasesByCommandName(this._commandMetadata.Name).ToList<string>();
            if (list.Count <= 0)
            {
                return null;
            }
            return list;
        }

        internal object GetDefaultParameterValue(string name)
        {
            MergedCompiledCommandParameter parameter = base.BindableParameters.GetMatchingParameter(name, false, true, null);
            object defaultParameterValue = null;
            try
            {
                switch (parameter.BinderAssociation)
                {
                    case ParameterBinderAssociation.DeclaredFormalParameters:
                        return base.DefaultParameterBinder.GetDefaultParameterValue(name);

                    case ParameterBinderAssociation.DynamicParameters:
                        if (this._dynamicParameterBinder != null)
                        {
                            defaultParameterValue = this._dynamicParameterBinder.GetDefaultParameterValue(name);
                        }
                        return defaultParameterValue;

                    case ParameterBinderAssociation.CommonParameters:
                        return this.CommonParametersBinder.GetDefaultParameterValue(name);

                    case ParameterBinderAssociation.ShouldProcessParameters:
                        return this.ShouldProcessParametersBinder.GetDefaultParameterValue(name);
                }
                return defaultParameterValue;
            }
            catch (GetValueException exception)
            {
                ParameterBindingParameterDefaultValueException exception2 = new ParameterBindingParameterDefaultValueException(exception, ErrorCategory.ReadError, this.Command.MyInvocation, null, name, null, null, "ParameterBinderStrings", "GetDefaultValueFailed", new object[] { exception.Message });
                throw exception2;
            }
            return defaultParameterValue;
        }

        private Dictionary<MergedCompiledCommandParameter, object> GetDefaultParameterValuePairs(bool needToGetAlias)
        {
            if (this.DefaultParameterValues == null)
            {
                this._useDefaultParameterBinding = false;
                return null;
            }
            Dictionary<MergedCompiledCommandParameter, object> result = new Dictionary<MergedCompiledCommandParameter, object>();
            if (needToGetAlias && (this.DefaultParameterValues.Count > 0))
            {
                this._aliasList = this.GetAliasOfCurrentCmdlet();
            }
            this._useDefaultParameterBinding = true;
            string name = this._commandMetadata.Name;
            IDictionary<string, MergedCompiledCommandParameter> bindableParameters = base.BindableParameters.BindableParameters;
            IDictionary<string, MergedCompiledCommandParameter> aliasedParameters = base.BindableParameters.AliasedParameters;
            HashSet<MergedCompiledCommandParameter> parametersToRemove = new HashSet<MergedCompiledCommandParameter>();
            Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
            List<object> list = new List<object>();
            foreach (DictionaryEntry entry in this.DefaultParameterValues)
            {
                string key = entry.Key as string;
                if (key != null)
                {
                    key = key.Trim();
                    string cmdletName = null;
                    string parameterName = null;
                    if (!DefaultParameterDictionary.CheckKeyIsValid(key, ref cmdletName, ref parameterName))
                    {
                        if (key.Equals("Disabled", StringComparison.OrdinalIgnoreCase) && LanguagePrimitives.IsTrue(entry.Value))
                        {
                            this._useDefaultParameterBinding = false;
                            return null;
                        }
                        if (!key.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(entry.Key);
                        }
                    }
                    else if (WildcardPattern.ContainsWildcardCharacters(key))
                    {
                        dictionary4.Add(cmdletName + ":::" + parameterName, entry.Value);
                    }
                    else if (cmdletName.Equals(name, StringComparison.OrdinalIgnoreCase) || this.MatchAnyAlias(cmdletName))
                    {
                        this.GetDefaultParameterValuePairsHelper(cmdletName, parameterName, entry.Value, bindableParameters, aliasedParameters, result, parametersToRemove);
                    }
                }
            }
            foreach (KeyValuePair<string, object> pair in dictionary4)
            {
                string str5 = pair.Key;
                string str6 = str5.Substring(0, str5.IndexOf(":::", StringComparison.OrdinalIgnoreCase));
                string str7 = str5.Substring(str5.IndexOf(":::", StringComparison.OrdinalIgnoreCase) + ":::".Length);
                WildcardPattern pattern = new WildcardPattern(str6, WildcardOptions.IgnoreCase);
                if (pattern.IsMatch(name) || this.MatchAnyAlias(str6))
                {
                    if (!WildcardPattern.ContainsWildcardCharacters(str7))
                    {
                        this.GetDefaultParameterValuePairsHelper(str6, str7, pair.Value, bindableParameters, aliasedParameters, result, parametersToRemove);
                    }
                    else
                    {
                        WildcardPattern namePattern = MemberMatch.GetNamePattern(str7);
                        List<MergedCompiledCommandParameter> list2 = new List<MergedCompiledCommandParameter>();
                        foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair2 in bindableParameters)
                        {
                            if (namePattern.IsMatch(pair2.Key))
                            {
                                list2.Add(pair2.Value);
                            }
                        }
                        foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair3 in aliasedParameters)
                        {
                            if (namePattern.IsMatch(pair3.Key))
                            {
                                list2.Add(pair3.Value);
                            }
                        }
                        if (list2.Count > 1)
                        {
                            if (!this._warningSet.Contains(str6 + ":::" + str7))
                            {
                                this._commandRuntime.WriteWarning(string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.MultipleParametersMatched, new object[] { str7 }));
                                this._warningSet.Add(str6 + ":::" + str7);
                            }
                        }
                        else if (list2.Count == 1)
                        {
                            if (!result.ContainsKey(list2[0]))
                            {
                                result.Add(list2[0], pair.Value);
                            }
                            else if (!pair.Value.Equals(result[list2[0]]))
                            {
                                if (!this._warningSet.Contains(str6 + ":::" + str7))
                                {
                                    this._commandRuntime.WriteWarning(string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.DifferentValuesAssignedToSingleParameter, new object[] { str7 }));
                                    this._warningSet.Add(str6 + ":::" + str7);
                                }
                                parametersToRemove.Add(list2[0]);
                            }
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (object obj2 in list)
                {
                    if (this.DefaultParameterValues.Contains(obj2))
                    {
                        this.DefaultParameterValues.Remove(obj2);
                    }
                    builder.Append(obj2.ToString() + ", ");
                }
                builder.Remove(builder.Length - 2, 2);
                string format = (list.Count > 1) ? ParameterBinderStrings.MultipleKeysInBadFormat : ParameterBinderStrings.SingleKeyInBadFormat;
                this._commandRuntime.WriteWarning(string.Format(CultureInfo.InvariantCulture, format, new object[] { builder }));
            }
            foreach (MergedCompiledCommandParameter parameter in parametersToRemove)
            {
                if (result.ContainsKey(parameter))
                {
                    result.Remove(parameter);
                }
            }
            if (result.Count > 0)
            {
                return result;
            }
            return null;
        }

        private void GetDefaultParameterValuePairsHelper(string cmdletName, string paramName, object paramValue, IDictionary<string, MergedCompiledCommandParameter> bindableParameters, IDictionary<string, MergedCompiledCommandParameter> bindableAlias, Dictionary<MergedCompiledCommandParameter, object> result, HashSet<MergedCompiledCommandParameter> parametersToRemove)
        {
            MergedCompiledCommandParameter parameter;
            bool flag = false;
            if (bindableParameters.TryGetValue(paramName, out parameter))
            {
                if (!result.ContainsKey(parameter))
                {
                    result.Add(parameter, paramValue);
                    return;
                }
                if (!paramValue.Equals(result[parameter]))
                {
                    flag = true;
                    parametersToRemove.Add(parameter);
                }
            }
            else if (bindableAlias.TryGetValue(paramName, out parameter))
            {
                if (!result.ContainsKey(parameter))
                {
                    result.Add(parameter, paramValue);
                    return;
                }
                if (!paramValue.Equals(result[parameter]))
                {
                    flag = true;
                    parametersToRemove.Add(parameter);
                }
            }
            if (flag && !this._warningSet.Contains(cmdletName + ":::" + paramName))
            {
                this._commandRuntime.WriteWarning(string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.DifferentValuesAssignedToSingleParameter, new object[] { paramName }));
                this._warningSet.Add(cmdletName + ":::" + paramName);
            }
        }

        private Collection<MergedCompiledCommandParameter> GetMissingMandatoryParameters(int validParameterSetCount, bool isPipelineInputExpected)
        {
            bool flag4;
            bool flag5;
            ParameterSetPromptingData data5;
            ParameterSetPromptingData data6;
            Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
            int defaultParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
            int parameterSetFlags = 0;
            Dictionary<int, ParameterSetPromptingData> promptingData = new Dictionary<int, ParameterSetPromptingData>();
            bool flag = false;
            bool flag2 = false;
            foreach (MergedCompiledCommandParameter parameter in base.UnboundParameters)
            {
                if (parameter.Parameter.IsMandatoryInSomeParameterSet)
                {
                    IEnumerable<ParameterSetSpecificMetadata> matchingParameterSetData = parameter.Parameter.GetMatchingParameterSetData(base._currentParameterSetFlag);
                    int num3 = 0;
                    bool flag3 = false;
                    foreach (ParameterSetSpecificMetadata metadata in matchingParameterSetData)
                    {
                        int num4 = this.NewParameterSetPromptingData(promptingData, parameter, metadata, defaultParameterSetFlag, isPipelineInputExpected);
                        if (num4 != 0)
                        {
                            flag = true;
                            flag3 = true;
                            if (num4 != int.MaxValue)
                            {
                                num3 |= base._currentParameterSetFlag & num4;
                                parameterSetFlags |= base._currentParameterSetFlag & num3;
                            }
                            else
                            {
                                flag2 = true;
                            }
                        }
                    }
                    if (!isPipelineInputExpected && flag3)
                    {
                        collection.Add(parameter);
                    }
                }
            }
            if (!flag || !isPipelineInputExpected)
            {
                return collection;
            }
            if (parameterSetFlags == 0)
            {
                parameterSetFlags = base._currentParameterSetFlag;
            }
            if (flag2)
            {
                int allParameterSetFlags = base.BindableParameters.AllParameterSetFlags;
                if (allParameterSetFlags == 0)
                {
                    allParameterSetFlags = int.MaxValue;
                }
                parameterSetFlags = base._currentParameterSetFlag & allParameterSetFlags;
            }
            if (((validParameterSetCount > 1) && (defaultParameterSetFlag != 0)) && (((defaultParameterSetFlag & parameterSetFlags) == 0) && ((defaultParameterSetFlag & base._currentParameterSetFlag) != 0)))
            {
                int parameterSet = 0;
                foreach (ParameterSetPromptingData data in promptingData.Values)
                {
                    if ((((data.ParameterSet & base._currentParameterSetFlag) != 0) && ((data.ParameterSet & defaultParameterSetFlag) == 0)) && (!data.IsAllSet && (data.PipelineableMandatoryParameters.Count > 0)))
                    {
                        parameterSet = data.ParameterSet;
                        break;
                    }
                }
                if (parameterSet == 0)
                {
                    parameterSetFlags = base._currentParameterSetFlag & ~parameterSetFlags;
                    base._currentParameterSetFlag = parameterSetFlags;
                    if (base._currentParameterSetFlag == defaultParameterSetFlag)
                    {
                        this.Command.SetParameterSetName(this.CurrentParameterSetName);
                    }
                    else
                    {
                        this._parameterSetToBePrioritizedInPipelingBinding = defaultParameterSetFlag;
                    }
                }
            }
            switch (ValidParameterSetCount(parameterSetFlags))
            {
                case 0:
                    this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                    return collection;

                case 1:
                    foreach (ParameterSetPromptingData data2 in promptingData.Values)
                    {
                        if (((data2.ParameterSet & parameterSetFlags) != 0) || data2.IsAllSet)
                        {
                            foreach (MergedCompiledCommandParameter parameter2 in data2.NonpipelineableMandatoryParameters.Keys)
                            {
                                collection.Add(parameter2);
                            }
                        }
                    }
                    return collection;

                default:
                    if (this._parameterSetToBePrioritizedInPipelingBinding != 0)
                    {
                        return collection;
                    }
                    flag4 = false;
                    if ((defaultParameterSetFlag == 0) || ((parameterSetFlags & defaultParameterSetFlag) == 0))
                    {
                        goto Label_04A2;
                    }
                    flag5 = false;
                    foreach (ParameterSetPromptingData data3 in promptingData.Values)
                    {
                        if ((!data3.IsAllSet && !data3.IsDefaultSet) && ((data3.PipelineableMandatoryParameters.Count > 0) && (data3.NonpipelineableMandatoryParameters.Count == 0)))
                        {
                            flag5 = true;
                            break;
                        }
                    }
                    break;
            }
            bool flag6 = false;
            foreach (ParameterSetPromptingData data4 in promptingData.Values)
            {
                if ((!data4.IsAllSet && !data4.IsDefaultSet) && (data4.PipelineableMandatoryByPropertyNameParameters.Count > 0))
                {
                    flag6 = true;
                    break;
                }
            }
            if (promptingData.TryGetValue(defaultParameterSetFlag, out data5))
            {
                bool flag7 = data5.PipelineableMandatoryParameters.Count > 0;
                if ((data5.PipelineableMandatoryByPropertyNameParameters.Count > 0) && !flag6)
                {
                    flag4 = true;
                }
                else if (flag7 && !flag5)
                {
                    flag4 = true;
                }
            }
            if (!flag4 && !flag5)
            {
                flag4 = true;
            }
            if ((!flag4 && promptingData.TryGetValue(int.MaxValue, out data6)) && (data6.NonpipelineableMandatoryParameters.Count > 0))
            {
                flag4 = true;
            }
            if (flag4)
            {
                parameterSetFlags = defaultParameterSetFlag;
                base._currentParameterSetFlag = defaultParameterSetFlag;
                this.Command.SetParameterSetName(this.CurrentParameterSetName);
                foreach (ParameterSetPromptingData data7 in promptingData.Values)
                {
                    if (((data7.ParameterSet & parameterSetFlags) != 0) || data7.IsAllSet)
                    {
                        foreach (MergedCompiledCommandParameter parameter3 in data7.NonpipelineableMandatoryParameters.Keys)
                        {
                            collection.Add(parameter3);
                        }
                    }
                }
            }
        Label_04A2:
            if (!flag4)
            {
                int chosenMandatorySet = 0;
                int num9 = 0;
                bool flag9 = false;
                bool flag10 = false;
                foreach (ParameterSetPromptingData data8 in promptingData.Values)
                {
                    if ((((data8.ParameterSet & parameterSetFlags) != 0) && !data8.IsAllSet) && (data8.PipelineableMandatoryByValueParameters.Count > 0))
                    {
                        if (flag9)
                        {
                            flag10 = true;
                            chosenMandatorySet = 0;
                            break;
                        }
                        chosenMandatorySet = data8.ParameterSet;
                        flag9 = true;
                    }
                }
                bool flag11 = false;
                bool flag12 = false;
                foreach (ParameterSetPromptingData data9 in promptingData.Values)
                {
                    if ((((data9.ParameterSet & parameterSetFlags) != 0) && !data9.IsAllSet) && (data9.PipelineableMandatoryByPropertyNameParameters.Count > 0))
                    {
                        if (flag11)
                        {
                            flag12 = true;
                            num9 = 0;
                            break;
                        }
                        num9 = data9.ParameterSet;
                        flag11 = true;
                    }
                }
                int num10 = 0;
                if ((flag9 & flag11) && (chosenMandatorySet == num9))
                {
                    num10 = chosenMandatorySet;
                }
                if (flag9 ^ flag11)
                {
                    num10 = flag9 ? chosenMandatorySet : num9;
                }
                if (num10 != 0)
                {
                    parameterSetFlags = num10;
                    int num11 = 0;
                    bool chosenSetContainsNonpipelineableMandatoryParameters = false;
                    foreach (ParameterSetPromptingData data10 in promptingData.Values)
                    {
                        if (((data10.ParameterSet & parameterSetFlags) != 0) || data10.IsAllSet)
                        {
                            if (!data10.IsAllSet)
                            {
                                chosenSetContainsNonpipelineableMandatoryParameters = data10.NonpipelineableMandatoryParameters.Count > 0;
                            }
                            foreach (MergedCompiledCommandParameter parameter4 in data10.NonpipelineableMandatoryParameters.Keys)
                            {
                                collection.Add(parameter4);
                            }
                        }
                        else
                        {
                            num11 |= data10.ParameterSet;
                        }
                    }
                    this.PreservePotentialParameterSets(num10, num11, chosenSetContainsNonpipelineableMandatoryParameters);
                    return collection;
                }
                bool flag14 = false;
                int otherMandatorySetsToBeIgnored = 0;
                foreach (ParameterSetPromptingData data11 in promptingData.Values)
                {
                    if ((((data11.ParameterSet & parameterSetFlags) != 0) || data11.IsAllSet) && (data11.NonpipelineableMandatoryParameters.Count > 0))
                    {
                        flag14 = true;
                        if (!data11.IsAllSet)
                        {
                            otherMandatorySetsToBeIgnored |= data11.ParameterSet;
                        }
                    }
                }
                if (flag14)
                {
                    if (chosenMandatorySet != 0)
                    {
                        parameterSetFlags = chosenMandatorySet;
                        int num13 = 0;
                        bool flag15 = false;
                        foreach (ParameterSetPromptingData data12 in promptingData.Values)
                        {
                            if (((data12.ParameterSet & parameterSetFlags) != 0) || data12.IsAllSet)
                            {
                                if (!data12.IsAllSet)
                                {
                                    flag15 = data12.NonpipelineableMandatoryParameters.Count > 0;
                                }
                                foreach (MergedCompiledCommandParameter parameter5 in data12.NonpipelineableMandatoryParameters.Keys)
                                {
                                    collection.Add(parameter5);
                                }
                            }
                            else
                            {
                                num13 |= data12.ParameterSet;
                            }
                        }
                        this.PreservePotentialParameterSets(chosenMandatorySet, num13, flag15);
                        return collection;
                    }
                    if (!flag10 && !flag12)
                    {
                        this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                    }
                    if (otherMandatorySetsToBeIgnored == 0)
                    {
                        return collection;
                    }
                    this.IgnoreOtherMandatoryParameterSets(otherMandatorySetsToBeIgnored);
                    if (base._currentParameterSetFlag == 0)
                    {
                        this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                    }
                    if (ValidParameterSetCount(base._currentParameterSetFlag) == 1)
                    {
                        this.Command.SetParameterSetName(this.CurrentParameterSetName);
                    }
                }
            }
            return collection;
        }

        private Dictionary<MergedCompiledCommandParameter, object> GetQualifiedParameterValuePairs(int currentParameterSetFlag, Dictionary<MergedCompiledCommandParameter, object> availableParameterValuePairs)
        {
            if (availableParameterValuePairs != null)
            {
                Dictionary<MergedCompiledCommandParameter, object> dictionary = new Dictionary<MergedCompiledCommandParameter, object>();
                int maxValue = int.MaxValue;
                foreach (MergedCompiledCommandParameter parameter in availableParameterValuePairs.Keys)
                {
                    if ((((parameter.Parameter.ParameterSetFlags & currentParameterSetFlag) != 0) || parameter.Parameter.IsInAllSets) && !base.BoundArguments.ContainsKey(parameter.Parameter.Name))
                    {
                        if (parameter.Parameter.ParameterSetFlags != 0)
                        {
                            maxValue &= parameter.Parameter.ParameterSetFlags;
                            if (maxValue == 0)
                            {
                                return null;
                            }
                        }
                        dictionary.Add(parameter, availableParameterValuePairs[parameter]);
                    }
                }
                if (dictionary.Count > 0)
                {
                    return dictionary;
                }
            }
            return null;
        }

        private void HandleCommandLineDynamicParameters(out ParameterBindingException outgoingBindingException)
        {
            outgoingBindingException = null;
            if (this._commandMetadata.ImplementsDynamicParameters)
            {
                using (ParameterBinderBase.bindingTracer.TraceScope("BIND cmd line args to DYNAMIC parameters.", new object[0]))
                {
                    _tracer.WriteLine("The Cmdlet supports the dynamic parameter interface", new object[0]);
                    IDynamicParameters command = this.Command as IDynamicParameters;
                    if (command != null)
                    {
                        if (this._dynamicParameterBinder == null)
                        {
                            object dynamicParameters;
                            _tracer.WriteLine("Getting the bindable object from the Cmdlet", new object[0]);
                            PSScriptCmdlet cmdlet = this.Command as PSScriptCmdlet;
                            if (cmdlet != null)
                            {
                                cmdlet.PrepareForBinding(((ScriptParameterBinder)base.DefaultParameterBinder).LocalScope, base.CommandLineParameters);
                            }
                            try
                            {
                                dynamicParameters = command.GetDynamicParameters();
                            }
                            catch (Exception exception)
                            {
                                CommandProcessorBase.CheckForSevereException(exception);
                                if (exception is ProviderInvocationException)
                                {
                                    throw;
                                }
                                ParameterBindingException exception2 = new ParameterBindingException(exception, ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, null, null, null, "ParameterBinderStrings", "GetDynamicParametersException", new object[] { exception.Message });
                                throw exception2;
                            }
                            if (dynamicParameters != null)
                            {
                                InternalParameterMetadata metadata;
                                ParameterBinderBase.bindingTracer.WriteLine("DYNAMIC parameter object: [{0}]", new object[] { dynamicParameters.GetType() });
                                _tracer.WriteLine("Creating a new parameter binder for the dynamic parameter object", new object[0]);
                                RuntimeDefinedParameterDictionary runtimeDefinedParameters = dynamicParameters as RuntimeDefinedParameterDictionary;
                                if (runtimeDefinedParameters != null)
                                {
                                    metadata = InternalParameterMetadata.Get(runtimeDefinedParameters, true, true);
                                    this._dynamicParameterBinder = new RuntimeDefinedParameterBinder(runtimeDefinedParameters, this.Command, base.CommandLineParameters);
                                }
                                else
                                {
                                    metadata = InternalParameterMetadata.Get(dynamicParameters.GetType(), base.Context, true);
                                    this._dynamicParameterBinder = new ReflectionParameterBinder(dynamicParameters, this.Command, base.CommandLineParameters);
                                }
                                foreach (MergedCompiledCommandParameter parameter in base.BindableParameters.AddMetadataForBinder(metadata, ParameterBinderAssociation.DynamicParameters))
                                {
                                    base.UnboundParameters.Add(parameter);
                                }
                                this._commandMetadata.DefaultParameterSetFlag = base.BindableParameters.GenerateParameterSetMappingFromMetadata(this._commandMetadata.DefaultParameterSetName);
                            }
                        }
                        if (this._dynamicParameterBinder == null)
                        {
                            _tracer.WriteLine("No dynamic parameter object was returned from the Cmdlet", new object[0]);
                        }
                        else if (base.UnboundArguments.Count > 0)
                        {
                            using (ParameterBinderBase.bindingTracer.TraceScope("BIND NAMED args to DYNAMIC parameters", new object[0]))
                            {
                                base.ReparseUnboundArguments();
                                base.UnboundArguments = this.BindParameters(base._currentParameterSetFlag, base.UnboundArguments);
                            }
                            using (ParameterBinderBase.bindingTracer.TraceScope("BIND POSITIONAL args to DYNAMIC parameters", new object[0]))
                            {
                                base.UnboundArguments = base.BindPositionalParameters(base.UnboundArguments, base._currentParameterSetFlag, this._commandMetadata.DefaultParameterSetFlag, out outgoingBindingException);
                            }
                        }
                    }
                }
            }
        }

        private void HandleRemainingArguments()
        {
            if (base.UnboundArguments.Count > 0)
            {
                MergedCompiledCommandParameter parameter = null;
                foreach (MergedCompiledCommandParameter parameter2 in base.UnboundParameters)
                {
                    ParameterSetSpecificMetadata parameterSetData = parameter2.Parameter.GetParameterSetData(base._currentParameterSetFlag);
                    if ((parameterSetData != null) && parameterSetData.ValueFromRemainingArguments)
                    {
                        if (parameter != null)
                        {
                            ParameterBindingException pbex = new ParameterBindingException(ErrorCategory.MetadataError, this.Command.MyInvocation, null, parameter2.Parameter.Name, parameter2.Parameter.Type, null, "ParameterBinderStrings", "AmbiguousParameterSet", new object[0]);
                            if (!base.DefaultParameterBindingInUse)
                            {
                                throw pbex;
                            }
                            base.ThrowElaboratedBindingException(pbex);
                        }
                        parameter = parameter2;
                    }
                }
                if (parameter != null)
                {
                    using (ParameterBinderBase.bindingTracer.TraceScope("BIND REMAININGARGUMENTS cmd line args to param: [{0}]", new object[] { parameter.Parameter.Name }))
                    {
                        ArrayList list = new ArrayList();
                        foreach (CommandParameterInternal internal2 in base.UnboundArguments)
                        {
                            if (internal2.ParameterNameSpecified)
                            {
                                list.Add(internal2.ParameterText);
                            }
                            if (internal2.ArgumentSpecified)
                            {
                                object argumentValue = internal2.ArgumentValue;
                                if ((argumentValue != AutomationNull.Value) && (argumentValue != UnboundParameter.Value))
                                {
                                    list.Add(argumentValue);
                                }
                            }
                        }
                        IScriptExtent argumentExtent = (base.UnboundArguments.Count == 1) ? base.UnboundArguments[0].ArgumentExtent : PositionUtilities.EmptyExtent;
                        CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameter.Parameter.Name, "-" + parameter.Parameter.Name + ":", argumentExtent, list, false);
                        try
                        {
                            this.BindParameter(argument, parameter, ParameterBindingFlags.ShouldCoerceType);
                        }
                        catch (ParameterBindingException exception2)
                        {
                            if ((list.Count == 1) && (list[0] is object[]))
                            {
                                argument.SetArgumentValue(base.UnboundArguments[0].ArgumentExtent, list[0]);
                                this.BindParameter(argument, parameter, ParameterBindingFlags.ShouldCoerceType);
                            }
                            else
                            {
                                if (!base.DefaultParameterBindingInUse)
                                {
                                    throw;
                                }
                                base.ThrowElaboratedBindingException(exception2);
                            }
                        }
                        base.UnboundArguments.Clear();
                    }
                }
            }
        }

        internal bool HandleUnboundMandatoryParameters(out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
        {
            return this.HandleUnboundMandatoryParameters(ValidParameterSetCount(base._currentParameterSetFlag), false, false, out missingMandatoryParameters);
        }

        internal bool HandleUnboundMandatoryParameters(int validParameterSetCount, bool promptForMandatory, bool isPipelineInputExpected, out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
        {
            bool flag = true;
            missingMandatoryParameters = this.GetMissingMandatoryParameters(validParameterSetCount, isPipelineInputExpected);
            if (missingMandatoryParameters.Count <= 0)
            {
                return flag;
            }
            if (promptForMandatory)
            {
                if (base.Context.EngineHostInterface == null)
                {
                    ParameterBinderBase.bindingTracer.WriteLine("ERROR: host does not support prompting for missing mandatory parameters", new object[0]);
                    string parameterName = BuildMissingParamsString(missingMandatoryParameters);
                    ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, parameterName, null, null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
                    throw exception;
                }
                Collection<FieldDescription> fieldDescriptionList = this.CreatePromptDataStructures(missingMandatoryParameters);
                Dictionary<string, PSObject> dictionary = this.PromptForMissingMandatoryParameters(fieldDescriptionList, missingMandatoryParameters);
                using (ParameterBinderBase.bindingTracer.TraceScope("BIND PROMPTED mandatory parameter args", new object[0]))
                {
                    foreach (KeyValuePair<string, PSObject> pair in dictionary)
                    {
                        CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, pair.Key, "-" + pair.Key + ":", PositionUtilities.EmptyExtent, pair.Value, false);
                        flag = this.BindParameter(argument, ParameterBindingFlags.ThrowOnParameterNotFound | ParameterBindingFlags.ShouldCoerceType);
                    }
                    return true;
                }
            }
            return false;
        }

        private void IgnoreOtherMandatoryParameterSets(int otherMandatorySetsToBeIgnored)
        {
            if (otherMandatorySetsToBeIgnored != 0)
            {
                if (base._currentParameterSetFlag == int.MaxValue)
                {
                    int allParameterSetFlags = base.BindableParameters.AllParameterSetFlags;
                    base._currentParameterSetFlag = allParameterSetFlags & ~otherMandatorySetsToBeIgnored;
                }
                else
                {
                    base._currentParameterSetFlag &= ~otherMandatorySetsToBeIgnored;
                }
            }
        }

        private bool InvokeAndBindDelayBindScriptBlock(PSObject inputToOperateOn, out bool thereWasSomethingToBind)
        {
            thereWasSomethingToBind = false;
            bool flag = true;
            foreach (KeyValuePair<MergedCompiledCommandParameter, DelayedScriptBlockArgument> pair in this._delayBindScriptBlocks)
            {
                thereWasSomethingToBind = true;
                CommandParameterInternal cpi = pair.Value._argument;
                MergedCompiledCommandParameter key = pair.Key;
                ScriptBlock argumentValue = cpi.ArgumentValue as ScriptBlock;
                Collection<PSObject> collection = null;
                Exception innerException = null;
                using (ParameterBinderBase.bindingTracer.TraceScope("Invoking delay-bind ScriptBlock", new object[0]))
                {
                    if (pair.Value._parameterBinder == this)
                    {
                        try
                        {
                            collection = argumentValue.DoInvoke(inputToOperateOn, inputToOperateOn, new object[0]);
                            pair.Value._evaluatedArgument = collection;
                        }
                        catch (RuntimeException exception2)
                        {
                            innerException = exception2;
                        }
                    }
                    else
                    {
                        collection = pair.Value._evaluatedArgument;
                    }
                }
                if (innerException != null)
                {
                    ParameterBindingException exception3 = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.Command.MyInvocation, base.GetErrorExtent(cpi), key.Parameter.Name, null, null, "ParameterBinderStrings", "ScriptBlockArgumentInvocationFailed", new object[] { innerException.Message });
                    throw exception3;
                }
                if ((collection == null) || (collection.Count == 0))
                {
                    ParameterBindingException exception4 = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.Command.MyInvocation, base.GetErrorExtent(cpi), key.Parameter.Name, null, null, "ParameterBinderStrings", "ScriptBlockArgumentNoOutput", new object[0]);
                    throw exception4;
                }
                object obj2 = collection;
                if (collection.Count == 1)
                {
                    obj2 = collection[0];
                }
                CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(cpi.ParameterExtent, cpi.ParameterName, "-" + cpi.ParameterName + ":", cpi.ArgumentExtent, obj2, false);
                if (!this.BindParameter(argument, key, ParameterBindingFlags.ShouldCoerceType))
                {
                    flag = false;
                }
            }
            return flag;
        }

        private static bool IsParameterScriptBlockBindable(MergedCompiledCommandParameter parameter)
        {
            bool flag = false;
            Type type = parameter.Parameter.Type;
            if (type == typeof(object))
            {
                flag = true;
            }
            else if (type == typeof(ScriptBlock))
            {
                flag = true;
            }
            else if (type.IsSubclassOf(typeof(ScriptBlock)))
            {
                flag = true;
            }
            else
            {
                ParameterCollectionTypeInformation collectionTypeInformation = parameter.Parameter.CollectionTypeInformation;
                if (collectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection)
                {
                    if (collectionTypeInformation.ElementType == typeof(object))
                    {
                        flag = true;
                    }
                    else if (collectionTypeInformation.ElementType == typeof(ScriptBlock))
                    {
                        flag = true;
                    }
                    else if (collectionTypeInformation.ElementType.IsSubclassOf(typeof(ScriptBlock)))
                    {
                        flag = true;
                    }
                }
            }
            _tracer.WriteLine("IsParameterScriptBlockBindable: result = {0}", new object[] { flag });
            return flag;
        }

        private bool MatchAnyAlias(string aliasName)
        {
            if (this._aliasList == null)
            {
                return false;
            }
            WildcardPattern pattern = new WildcardPattern(aliasName, WildcardOptions.IgnoreCase);
            foreach (string str in this._aliasList)
            {
                if (pattern.IsMatch(str))
                {
                    return true;
                }
            }
            return false;
        }

        private int NewParameterSetPromptingData(Dictionary<int, ParameterSetPromptingData> promptingData, MergedCompiledCommandParameter parameter, ParameterSetSpecificMetadata parameterSetMetadata, int defaultParameterSet, bool pipelineInputExpected)
        {
            int num = 0;
            int parameterSetFlag = parameterSetMetadata.ParameterSetFlag;
            if (parameterSetFlag == 0)
            {
                parameterSetFlag = int.MaxValue;
            }
            bool isDefaultSet = (defaultParameterSet != 0) && ((defaultParameterSet & parameterSetFlag) != 0);
            bool flag2 = false;
            if (parameterSetMetadata.IsMandatory)
            {
                num |= parameterSetFlag;
                flag2 = true;
            }
            bool flag3 = false;
            if (pipelineInputExpected && (parameterSetMetadata.ValueFromPipeline || parameterSetMetadata.ValueFromPipelineByPropertyName))
            {
                flag3 = true;
            }
            if (flag2)
            {
                ParameterSetPromptingData data;
                if (!promptingData.TryGetValue(parameterSetFlag, out data))
                {
                    data = new ParameterSetPromptingData(parameterSetFlag, isDefaultSet);
                    promptingData.Add(parameterSetFlag, data);
                }
                if (flag3)
                {
                    data.PipelineableMandatoryParameters[parameter] = parameterSetMetadata;
                    if (parameterSetMetadata.ValueFromPipeline)
                    {
                        data.PipelineableMandatoryByValueParameters[parameter] = parameterSetMetadata;
                    }
                    if (parameterSetMetadata.ValueFromPipelineByPropertyName)
                    {
                        data.PipelineableMandatoryByPropertyNameParameters[parameter] = parameterSetMetadata;
                    }
                    return num;
                }
                data.NonpipelineableMandatoryParameters[parameter] = parameterSetMetadata;
            }
            return num;
        }

        private void PreservePotentialParameterSets(int chosenMandatorySet, int otherMandatorySetsToBeIgnored, bool chosenSetContainsNonpipelineableMandatoryParameters)
        {
            if (chosenSetContainsNonpipelineableMandatoryParameters)
            {
                base._currentParameterSetFlag = chosenMandatorySet;
                this.Command.SetParameterSetName(this.CurrentParameterSetName);
            }
            else
            {
                this.IgnoreOtherMandatoryParameterSets(otherMandatorySetsToBeIgnored);
                this.Command.SetParameterSetName(this.CurrentParameterSetName);
                if (base._currentParameterSetFlag != chosenMandatorySet)
                {
                    this._parameterSetToBePrioritizedInPipelingBinding = chosenMandatorySet;
                }
            }
        }

        private Dictionary<string, PSObject> PromptForMissingMandatoryParameters(Collection<FieldDescription> fieldDescriptionList, Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
        {
            Dictionary<string, PSObject> dictionary = null;
            Exception exception = null;
            try
            {
                ParameterBinderBase.bindingTracer.WriteLine("PROMPTING for missing mandatory parameters using the host", new object[0]);
                string promptMessage = ParameterBinderStrings.PromptMessage;
                InvocationInfo myInvocation = this.Command.MyInvocation;
                string caption = StringUtil.Format(ParameterBinderStrings.PromptCaption, myInvocation.MyCommand.Name, myInvocation.PipelinePosition);
                dictionary = base.Context.EngineHostInterface.UI.Prompt(caption, promptMessage, fieldDescriptionList);
            }
            catch (NotImplementedException exception2)
            {
                exception = exception2;
            }
            catch (HostException exception3)
            {
                exception = exception3;
            }
            catch (PSInvalidOperationException exception4)
            {
                exception = exception4;
            }
            if (exception != null)
            {
                ParameterBinderBase.bindingTracer.WriteLine("ERROR: host does not support prompting for missing mandatory parameters", new object[0]);
                string str3 = BuildMissingParamsString(missingMandatoryParameters);
                ParameterBindingException exception5 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, str3, null, null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
                throw exception5;
            }
            if ((dictionary != null) && (dictionary.Count != 0))
            {
                return dictionary;
            }
            ParameterBinderBase.bindingTracer.WriteLine("ERROR: still missing mandatory parameters after PROMPTING", new object[0]);
            string parameterName = BuildMissingParamsString(missingMandatoryParameters);
            ParameterBindingException exception6 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, parameterName, null, null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
            throw exception6;
        }

        private int ResolveParameterSetAmbiguityBasedOnMandatoryParameters()
        {
            int parameterSetFlags = base._currentParameterSetFlag;
            IEnumerable<ParameterSetSpecificMetadata> enumerable = base.BoundParameters.Values.Concat<MergedCompiledCommandParameter>(base.UnboundParameters).SelectMany(x => x.Parameter.ParameterSetData.Values);
            int num2 = 0;
            foreach (ParameterSetSpecificMetadata metadata in enumerable)
            {
                num2 |= metadata.ParameterSetFlag;
            }
            parameterSetFlags &= num2;
            var v = base.UnboundParameters.SelectMany(x => x.Parameter.ParameterSetData.Values).Where(x => x.IsMandatory);
            foreach (ParameterSetSpecificMetadata metadata2 in v)
            {
                parameterSetFlags &= ~metadata2.ParameterSetFlag;
            }
            int num3 = ValidParameterSetCount(parameterSetFlags);
            if (num3 == 1)
            {
                base._currentParameterSetFlag = parameterSetFlags;
                this.Command.SetParameterSetName(this.CurrentParameterSetName);
                return num3;
            }
            return -1;
        }

        private void RestoreDefaultParameterValues(IEnumerable<MergedCompiledCommandParameter> parameters)
        {
            if (parameters == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameters");
            }
            foreach (MergedCompiledCommandParameter parameter in parameters)
            {
                if (parameter == null)
                {
                    continue;
                }
                CommandParameterInternal argumentToBind = null;
                foreach (CommandParameterInternal internal3 in this._defaultParameterValues.Values)
                {
                    if (string.Equals(parameter.Parameter.Name, internal3.ParameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        argumentToBind = internal3;
                        break;
                    }
                }
                if (argumentToBind != null)
                {
                    Exception innerException = null;
                    try
                    {
                        this.RestoreParameter(argumentToBind, parameter);
                    }
                    catch (SetValueException exception2)
                    {
                        innerException = exception2;
                    }
                    if (innerException != null)
                    {
                        Type typeSpecified = (argumentToBind.ArgumentValue == null) ? null : argumentToBind.ArgumentValue.GetType();
                        ParameterBindingException exception3 = new ParameterBindingException(innerException, ErrorCategory.WriteError, base.InvocationInfo, base.GetErrorExtent(argumentToBind), parameter.Parameter.Name, parameter.Parameter.Type, typeSpecified, "ParameterBinderStrings", "ParameterBindingFailed", new object[] { innerException.Message });
                        throw exception3;
                    }
                    if (base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                    {
                        base.BoundParameters.Remove(parameter.Parameter.Name);
                    }
                    if (!base.UnboundParameters.Contains(parameter))
                    {
                        base.UnboundParameters.Add(parameter);
                    }
                    if (base.BoundArguments.ContainsKey(parameter.Parameter.Name))
                    {
                        base.BoundArguments.Remove(parameter.Parameter.Name);
                    }
                }
                else
                {
                    if (!base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                    {
                        base.BoundParameters.Add(parameter.Parameter.Name, parameter);
                    }
                    base.UnboundParameters.Remove(parameter);
                }
            }
        }

        private bool RestoreParameter(CommandParameterInternal argumentToBind, MergedCompiledCommandParameter parameter)
        {
            switch (parameter.BinderAssociation)
            {
                case ParameterBinderAssociation.DeclaredFormalParameters:
                    base.DefaultParameterBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    break;

                case ParameterBinderAssociation.DynamicParameters:
                    if (this._dynamicParameterBinder != null)
                    {
                        this._dynamicParameterBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    }
                    break;

                case ParameterBinderAssociation.CommonParameters:
                    this.CommonParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    break;

                case ParameterBinderAssociation.ShouldProcessParameters:
                    this.ShouldProcessParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    break;

                case ParameterBinderAssociation.TransactionParameters:
                    this.TransactionParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    break;

                case ParameterBinderAssociation.PagingParameters:
                    this.PagingParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue);
                    break;
            }
            return true;
        }

        protected override void SaveDefaultScriptParameterValue(string name, object value)
        {
            this._defaultParameterValues.Add(name, CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, name, "-" + name + ":", PositionUtilities.EmptyExtent, value, false));
        }

        private void ThrowAmbiguousParameterSetException(int parameterSetFlags, MergedCommandParameterMetadata bindableParameters)
        {
            ParameterBindingException pbex = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, null, null, null, "ParameterBinderStrings", "AmbiguousParameterSet", new object[0]);
            for (int i = 1; parameterSetFlags != 0; i = i << 1)
            {
                int num2 = parameterSetFlags & 1;
                if (num2 == 1)
                {
                    string parameterSetName = bindableParameters.GetParameterSetName(i);
                    if (!string.IsNullOrEmpty(parameterSetName))
                    {
                        ParameterBinderBase.bindingTracer.WriteLine("Remaining valid parameter set: {0}", new object[] { parameterSetName });
                    }
                }
                parameterSetFlags = parameterSetFlags >> 1;
            }
            if (!base.DefaultParameterBindingInUse)
            {
                throw pbex;
            }
            base.ThrowElaboratedBindingException(pbex);
        }

        private int ValidateParameterSets(bool prePipelineInput, bool setDefault)
        {
            int num = ValidParameterSetCount(base._currentParameterSetFlag);
            if ((num == 0) && (base._currentParameterSetFlag != int.MaxValue))
            {
                this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                return num;
            }
            if (num > 1)
            {
                int defaultParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
                bool flag = defaultParameterSetFlag != 0;
                bool flag2 = base._currentParameterSetFlag == int.MaxValue;
                bool flag3 = base._currentParameterSetFlag == defaultParameterSetFlag;
                if (flag2 && !flag)
                {
                    return 1;
                }
                if ((!prePipelineInput && flag3) || (flag && ((base._currentParameterSetFlag & defaultParameterSetFlag) != 0)))
                {
                    string parameterSetName = base.BindableParameters.GetParameterSetName(defaultParameterSetFlag);
                    this.Command.SetParameterSetName(parameterSetName);
                    if (setDefault)
                    {
                        base._currentParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
                        num = 1;
                    }
                    return num;
                }
                if (prePipelineInput && this.AtLeastOneUnboundValidParameterSetTakesPipelineInput(base._currentParameterSetFlag))
                {
                    return num;
                }
                int num3 = this.ResolveParameterSetAmbiguityBasedOnMandatoryParameters();
                if (num3 != 1)
                {
                    this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                }
                return num3;
            }
            if (base._currentParameterSetFlag == int.MaxValue)
            {
                num = (base.BindableParameters.ParameterSetCount > 0) ? base.BindableParameters.ParameterSetCount : 1;
                if (!prePipelineInput || !this.AtLeastOneUnboundValidParameterSetTakesPipelineInput(base._currentParameterSetFlag))
                {
                    if (this._commandMetadata.DefaultParameterSetFlag != 0)
                    {
                        if (setDefault)
                        {
                            base._currentParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
                            num = 1;
                        }
                    }
                    else if (num > 1)
                    {
                        int num4 = this.ResolveParameterSetAmbiguityBasedOnMandatoryParameters();
                        if (num4 != 1)
                        {
                            this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                        }
                        num = num4;
                    }
                }
            }
            this.Command.SetParameterSetName(this.CurrentParameterSetName);
            return num;
        }

        private static int ValidParameterSetCount(int parameterSetFlags)
        {
            int num = 0;
            if (parameterSetFlags != int.MaxValue)
            {
                while (parameterSetFlags != 0)
                {
                    num += ((int)parameterSetFlags) & 1;
                    parameterSetFlags = parameterSetFlags >> 1;
                }
                return num;
            }
            return 1;
        }

        private void VerifyArgumentsProcessed(ParameterBindingException originalBindingException)
        {
            if (base.UnboundArguments.Count > 0)
            {
                ParameterBindingException exception;
                CommandParameterInternal cpi = base.UnboundArguments[0];
                Type typeSpecified = null;
                object argumentValue = cpi.ArgumentValue;
                if ((argumentValue != null) && (argumentValue != UnboundParameter.Value))
                {
                    typeSpecified = argumentValue.GetType();
                }
                if (cpi.ParameterNameSpecified)
                {
                    exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, base.GetParameterErrorExtent(cpi), cpi.ParameterName, null, typeSpecified, "ParameterBinderStrings", "NamedParameterNotFound", new object[0]);
                }
                else if (originalBindingException != null)
                {
                    exception = originalBindingException;
                }
                else
                {
                    string parameterName = "$null";
                    if (cpi.ArgumentValue != null)
                    {
                        try
                        {
                            parameterName = cpi.ArgumentValue.ToString();
                        }
                        catch (Exception exception2)
                        {
                            CommandProcessorBase.CheckForSevereException(exception2);
                            exception = new ParameterBindingArgumentTransformationException(exception2, ErrorCategory.InvalidData, base.InvocationInfo, null, null, null, cpi.ArgumentValue.GetType(), "ParameterBinderStrings", "ParameterArgumentTransformationErrorMessageOnly", new object[] { exception2.Message });
                            if (!base.DefaultParameterBindingInUse)
                            {
                                throw exception;
                            }
                            base.ThrowElaboratedBindingException(exception);
                        }
                    }
                    exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, null, parameterName, null, typeSpecified, "ParameterBinderStrings", "PositionalParameterNotFound", new object[0]);
                }
                if (!base.DefaultParameterBindingInUse)
                {
                    throw exception;
                }
                base.ThrowElaboratedBindingException(exception);
            }
        }

        private void VerifyParameterSetSelected()
        {
            if ((base.BindableParameters.ParameterSetCount > 1) && (base._currentParameterSetFlag == int.MaxValue))
            {
                if (((base._currentParameterSetFlag & this._commandMetadata.DefaultParameterSetFlag) != 0) && (this._commandMetadata.DefaultParameterSetFlag != int.MaxValue))
                {
                    ParameterBinderBase.bindingTracer.WriteLine("{0} valid parameter sets, using the DEFAULT PARAMETER SET: [{0}]", new object[] { base.BindableParameters.ParameterSetCount, this._commandMetadata.DefaultParameterSetName });
                    base._currentParameterSetFlag = this._commandMetadata.DefaultParameterSetFlag;
                }
                else
                {
                    ParameterBinderBase.bindingTracer.TraceError("ERROR: {0} valid parameter sets, but NOT DEFAULT PARAMETER SET.", new object[] { base.BindableParameters.ParameterSetCount });
                    this.ThrowAmbiguousParameterSetException(base._currentParameterSetFlag, base.BindableParameters);
                }
            }
        }

        private PSObject WrapBindingState()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> set2 = base.DefaultParameterBinder.CommandLineParameters.CopyBoundPositionalParameters();
            HashSet<string> set3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string str in base.BoundParameters.Keys)
            {
                set.Add(str);
            }
            foreach (string str2 in base.BoundDefaultParameters)
            {
                set3.Add(str2);
            }
            PSObject obj2 = new PSObject();
            obj2.Properties.Add(new PSNoteProperty("BoundParameters", set));
            obj2.Properties.Add(new PSNoteProperty("BoundPositionalParameters", set2));
            obj2.Properties.Add(new PSNoteProperty("BoundDefaultParameters", set3));
            return obj2;
        }

        internal Cmdlet Command { get; private set; }

        internal ReflectionParameterBinder CommonParametersBinder
        {
            get
            {
                if (this._commonParametersBinder == null)
                {
                    CommonParameters target = new CommonParameters(this._commandRuntime);
                    this._commonParametersBinder = new ReflectionParameterBinder(target, this.Command, base.CommandLineParameters);
                }
                return this._commonParametersBinder;
            }
        }

        internal string CurrentParameterSetName
        {
            get
            {
                string parameterSetName = base.BindableParameters.GetParameterSetName(base._currentParameterSetFlag);
                _tracer.WriteLine("CurrentParameterSetName = {0}", new object[] { parameterSetName });
                return parameterSetName;
            }
        }

        internal IDictionary DefaultParameterValues { get; set; }

        internal ReflectionParameterBinder PagingParametersBinder
        {
            get
            {
                if (this._pagingParameterBinder == null)
                {
                    PagingParameters target = new PagingParameters(this._commandRuntime);
                    this._pagingParameterBinder = new ReflectionParameterBinder(target, this.Command, base.CommandLineParameters);
                }
                return this._pagingParameterBinder;
            }
        }

        internal ReflectionParameterBinder ShouldProcessParametersBinder
        {
            get
            {
                if (this._shouldProcessParameterBinder == null)
                {
                    ShouldProcessParameters target = new ShouldProcessParameters(this._commandRuntime);
                    this._shouldProcessParameterBinder = new ReflectionParameterBinder(target, this.Command, base.CommandLineParameters);
                }
                return this._shouldProcessParameterBinder;
            }
        }

        internal ReflectionParameterBinder TransactionParametersBinder
        {
            get
            {
                if (this._transactionParameterBinder == null)
                {
                    TransactionParameters target = new TransactionParameters(this._commandRuntime);
                    this._transactionParameterBinder = new ReflectionParameterBinder(target, this.Command, base.CommandLineParameters);
                }
                return this._transactionParameterBinder;
            }
        }

        private enum CurrentlyBinding
        {
            ValueFromPipelineNoCoercion,
            ValueFromPipelineByPropertyNameNoCoercion,
            ValueFromPipelineWithCoercion,
            ValueFromPipelineByPropertyNameWithCoercion
        }

        private class DelayedScriptBlockArgument
        {
            internal CommandParameterInternal _argument;
            internal Collection<PSObject> _evaluatedArgument;
            internal CmdletParameterBinderController _parameterBinder;

            public override string ToString()
            {
                return this._argument.ArgumentValue.ToString();
            }
        }
    }
}

