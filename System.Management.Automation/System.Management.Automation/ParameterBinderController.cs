namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [DebuggerDisplay("InvocationInfo = {InvocationInfo}")]
    internal abstract class ParameterBinderController
    {
        protected MergedCommandParameterMetadata _bindableParameters = new MergedCommandParameterMetadata();
        private readonly Dictionary<string, CommandParameterInternal> _boundArguments = new Dictionary<string, CommandParameterInternal>(StringComparer.OrdinalIgnoreCase);
        private readonly Collection<string> _boundDefaultParameters = new Collection<string>();
        private readonly Dictionary<string, MergedCompiledCommandParameter> _boundParameters = new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
        private readonly ExecutionContext _context;
        internal int _currentParameterSetFlag = int.MaxValue;
        private bool _defaultParameterBindingInUse;
        private readonly System.Management.Automation.InvocationInfo _invocationInfo;
        private readonly Collection<MergedCompiledCommandParameter> _parametersBoundThroughPipelineInput = new Collection<MergedCompiledCommandParameter>();
        internal int _prePipelineProcessingParameterSetFlags = int.MaxValue;
        private Collection<CommandParameterInternal> _unboundArguments = new Collection<CommandParameterInternal>();

        internal ParameterBinderController(System.Management.Automation.InvocationInfo invocationInfo, ExecutionContext context, ParameterBinderBase parameterBinder)
        {
            this.DefaultParameterBinder = parameterBinder;
            this._context = context;
            this._invocationInfo = invocationInfo;
        }

        internal static void AddArgumentsToCommandProcessor(CommandProcessorBase commandProcessor, object[] arguments)
        {
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    CommandParameterInternal internal2;
                    string arg = arguments[i] as string;
                    if (ArgumentLooksLikeParameter(arg))
                    {
                        int index = arg.IndexOf(':');
                        if ((index != -1) && (index != (arg.Length - 1)))
                        {
                            internal2 = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, arg.Substring(1, index - 1), arg, PositionUtilities.EmptyExtent, arg.Substring(index + 1).Trim(), false);
                        }
                        else if ((i == (arguments.Length - 1)) || (arg[arg.Length - 1] != ':'))
                        {
                            internal2 = CommandParameterInternal.CreateParameter(PositionUtilities.EmptyExtent, arg.Substring(1), arg);
                        }
                        else
                        {
                            internal2 = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, arg.Substring(1, arg.Length - 2), arg, PositionUtilities.EmptyExtent, arguments[i + 1], false);
                            i++;
                        }
                    }
                    else
                    {
                        internal2 = CommandParameterInternal.CreateArgument(PositionUtilities.EmptyExtent, arguments[i], false);
                    }
                    commandProcessor.AddParameter(internal2);
                }
            }
        }

        private static void AddNewPosition(SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result, int positionInParameterSet, MergedCompiledCommandParameter parameter, ParameterSetSpecificMetadata parameterSetData)
        {
            if (result.ContainsKey(positionInParameterSet))
            {
                Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters = result[positionInParameterSet];
                if (ContainsPositionalParameterInSet(positionalCommandParameters, parameter, parameterSetData.ParameterSetFlag))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                if (positionalCommandParameters.ContainsKey(parameter))
                {
                    positionalCommandParameters[parameter].ParameterSetData.Add(parameterSetData);
                }
                else
                {
                    PositionalCommandParameter parameter2 = new PositionalCommandParameter(parameter);
                    parameter2.ParameterSetData.Add(parameterSetData);
                    positionalCommandParameters.Add(parameter, parameter2);
                }
            }
            else
            {
                Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> dictionary2 = new Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>();
                PositionalCommandParameter parameter3 = new PositionalCommandParameter(parameter) {
                    ParameterSetData = { parameterSetData }
                };
                dictionary2.Add(parameter, parameter3);
                result.Add(positionInParameterSet, dictionary2);
            }
        }

        internal static bool ArgumentLooksLikeParameter(string arg)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(arg))
            {
                flag = arg[0].IsDash();
            }
            return flag;
        }

        internal virtual bool BindParameter(CommandParameterInternal argument, ParameterBindingFlags flags)
        {
            bool flag = false;
            MergedCompiledCommandParameter parameter = this.BindableParameters.GetMatchingParameter(argument.ParameterName, (flags & ParameterBindingFlags.ThrowOnParameterNotFound) != ParameterBindingFlags.None, true, new System.Management.Automation.InvocationInfo(this.InvocationInfo.MyCommand, argument.ParameterExtent));
            if (parameter == null)
            {
                return flag;
            }
            if (this.BoundParameters.ContainsKey(parameter.Parameter.Name))
            {
                ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetParameterErrorExtent(argument), argument.ParameterName, null, null, "ParameterBinderStrings", "ParameterAlreadyBound", new object[0]);
                throw exception;
            }
            flags &= ~ParameterBindingFlags.DelayBindScriptBlock;
            return this.BindParameter(this._currentParameterSetFlag, argument, parameter, flags);
        }

        internal virtual bool BindParameter(int parameterSets, CommandParameterInternal argument, MergedCompiledCommandParameter parameter, ParameterBindingFlags flags)
        {
            bool flag = false;
            if (parameter.BinderAssociation == ParameterBinderAssociation.DeclaredFormalParameters)
            {
                flag = this.DefaultParameterBinder.BindParameter(argument, parameter.Parameter, flags);
            }
            if (flag && ((flags & ParameterBindingFlags.IsDefaultValue) == ParameterBindingFlags.None))
            {
                this.UnboundParameters.Remove(parameter);
                this.BoundParameters.Add(parameter.Parameter.Name, parameter);
            }
            return flag;
        }

        internal abstract Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters);
        internal Collection<CommandParameterInternal> BindPositionalParameters(Collection<CommandParameterInternal> unboundArguments, int validParameterSets, int defaultParameterSet, out ParameterBindingException outgoingBindingException)
        {
            Collection<CommandParameterInternal> nonPositionalArguments = new Collection<CommandParameterInternal>();
            outgoingBindingException = null;
            if (unboundArguments.Count > 0)
            {
                SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> dictionary;
                List<CommandParameterInternal> unboundArgumentsCollection = new List<CommandParameterInternal>(unboundArguments);
                try
                {
                    dictionary = EvaluateUnboundPositionalParameters(this.UnboundParameters, this._currentParameterSetFlag);
                }
                catch (InvalidOperationException)
                {
                    ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, null, null, null, null, "ParameterBinderStrings", "AmbiguousPositionalParameterNoName", new object[0]);
                    throw exception;
                }
                if (dictionary.Count <= 0)
                {
                    return unboundArguments;
                }
                int unboundArgumentsIndex = 0;
                foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> dictionary2 in dictionary.Values)
                {
                    if (dictionary2.Count != 0)
                    {
                        CommandParameterInternal argument = GetNextPositionalArgument(unboundArgumentsCollection, nonPositionalArguments, ref unboundArgumentsIndex);
                        if (argument == null)
                        {
                            break;
                        }
                        bool flag = false;
                        if ((defaultParameterSet != 0) && ((validParameterSets & defaultParameterSet) != 0))
                        {
                            flag = this.BindPositionalParametersInSet(defaultParameterSet, dictionary2, argument, ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                        }
                        if (!flag)
                        {
                            flag = this.BindPositionalParametersInSet(validParameterSets, dictionary2, argument, ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                        }
                        if ((!flag && (defaultParameterSet != 0)) && ((validParameterSets & defaultParameterSet) != 0))
                        {
                            flag = this.BindPositionalParametersInSet(defaultParameterSet, dictionary2, argument, ParameterBindingFlags.DelayBindScriptBlock | ParameterBindingFlags.ShouldCoerceType, out outgoingBindingException);
                        }
                        if (!flag)
                        {
                            flag = this.BindPositionalParametersInSet(validParameterSets, dictionary2, argument, ParameterBindingFlags.DelayBindScriptBlock | ParameterBindingFlags.ShouldCoerceType, out outgoingBindingException);
                        }
                        if (!flag)
                        {
                            nonPositionalArguments.Add(argument);
                        }
                        else if (validParameterSets != this._currentParameterSetFlag)
                        {
                            validParameterSets = this._currentParameterSetFlag;
                            UpdatePositionalDictionary(dictionary, validParameterSets);
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

        private bool BindPositionalParametersInSet(int validParameterSets, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters, CommandParameterInternal argument, ParameterBindingFlags flags, out ParameterBindingException bindingException)
        {
            bool flag = false;
            bindingException = null;
            foreach (PositionalCommandParameter parameter in nextPositionalParameters.Values)
            {
                foreach (ParameterSetSpecificMetadata metadata in parameter.ParameterSetData)
                {
                    if (((validParameterSets & metadata.ParameterSetFlag) != 0) || metadata.IsInAllSets)
                    {
                        bool flag2 = false;
                        string name = parameter.Parameter.Parameter.Name;
                        ParameterBindingException pbex = null;
                        try
                        {
                            CommandParameterInternal internal2 = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, name, "-" + name + ":", argument.ArgumentExtent, argument.ArgumentValue, false);
                            flag2 = this.BindParameter(validParameterSets, internal2, parameter.Parameter, flags);
                        }
                        catch (ParameterBindingArgumentTransformationException exception2)
                        {
                            pbex = exception2;
                        }
                        catch (ParameterBindingValidationException exception3)
                        {
                            if (exception3.SwallowException)
                            {
                                flag2 = false;
                                bindingException = exception3;
                            }
                            else
                            {
                                pbex = exception3;
                            }
                        }
                        catch (ParameterBindingParameterDefaultValueException exception4)
                        {
                            pbex = exception4;
                        }
                        catch (ParameterBindingException exception5)
                        {
                            flag2 = false;
                            bindingException = exception5;
                        }
                        if (pbex != null)
                        {
                            if (!this.DefaultParameterBindingInUse)
                            {
                                throw pbex;
                            }
                            this.ThrowElaboratedBindingException(pbex);
                        }
                        if (flag2)
                        {
                            flag = true;
                            this.CommandLineParameters.MarkAsBoundPositionally(name);
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        internal void BindUnboundScriptParameters()
        {
            foreach (MergedCompiledCommandParameter parameter in this.UnboundParameters)
            {
                this.BindUnboundScriptParameterWithDefaultValue(parameter);
            }
        }

        internal void BindUnboundScriptParameterWithDefaultValue(MergedCompiledCommandParameter parameter)
        {
            ScriptParameterBinder defaultParameterBinder = (ScriptParameterBinder) this.DefaultParameterBinder;
            ScriptBlock script = defaultParameterBinder.Script;
            if (script.RuntimeDefinedParameters.ContainsKey(parameter.Parameter.Name))
            {
                bool recordBoundParameters = defaultParameterBinder.RecordBoundParameters;
                try
                {
                    defaultParameterBinder.RecordBoundParameters = false;
                    RuntimeDefinedParameter parameter2 = script.RuntimeDefinedParameters[parameter.Parameter.Name];
                    IList implicitUsingParameters = null;
                    if (this.DefaultParameterBinder.CommandLineParameters != null)
                    {
                        implicitUsingParameters = this.DefaultParameterBinder.CommandLineParameters.GetImplicitUsingParameters();
                    }
                    object defaultScriptParameterValue = defaultParameterBinder.GetDefaultScriptParameterValue(parameter2, implicitUsingParameters);
                    this.SaveDefaultScriptParameterValue(parameter.Parameter.Name, defaultScriptParameterValue);
                    CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameter.Parameter.Name, "-" + parameter.Parameter.Name + ":", PositionUtilities.EmptyExtent, defaultScriptParameterValue, false);
                    ParameterBindingFlags isDefaultValue = ParameterBindingFlags.IsDefaultValue;
                    if (parameter2.IsSet)
                    {
                        isDefaultValue |= ParameterBindingFlags.ShouldCoerceType;
                    }
                    this.BindParameter(int.MaxValue, argument, parameter, isDefaultValue);
                }
                finally
                {
                    defaultParameterBinder.RecordBoundParameters = recordBoundParameters;
                }
            }
        }

        internal void ClearUnboundArguments()
        {
            this._unboundArguments.Clear();
        }

        private static bool ContainsPositionalParameterInSet(Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters, MergedCompiledCommandParameter parameter, int parameterSet)
        {
            bool flag = false;
            foreach (KeyValuePair<MergedCompiledCommandParameter, PositionalCommandParameter> pair in positionalCommandParameters)
            {
                if (pair.Key == parameter)
                {
                    continue;
                }
                foreach (ParameterSetSpecificMetadata metadata in pair.Value.ParameterSetData)
                {
                    if (((metadata.ParameterSetFlag & parameterSet) != 0) || (metadata.ParameterSetFlag == parameterSet))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        internal static SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> EvaluateUnboundPositionalParameters(ICollection<MergedCompiledCommandParameter> unboundParameters, int validParameterSetFlag)
        {
            SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result = new SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>>();
            if (unboundParameters.Count > 0)
            {
                foreach (MergedCompiledCommandParameter parameter in unboundParameters)
                {
                    if (((parameter.Parameter.ParameterSetFlags & validParameterSetFlag) != 0) || parameter.Parameter.IsInAllSets)
                    {
                        foreach (ParameterSetSpecificMetadata metadata in parameter.Parameter.GetMatchingParameterSetData(validParameterSetFlag))
                        {
                            if (!metadata.ValueFromRemainingArguments)
                            {
                                int position = metadata.Position;
                                if (position != -2147483648)
                                {
                                    AddNewPosition(result, position, parameter, metadata);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected IScriptExtent GetErrorExtent(CommandParameterInternal cpi)
        {
            IScriptExtent errorExtent = cpi.ErrorExtent;
            if (errorExtent == PositionUtilities.EmptyExtent)
            {
                errorExtent = this.InvocationInfo.ScriptPosition;
            }
            return errorExtent;
        }

        private static CommandParameterInternal GetNextPositionalArgument(List<CommandParameterInternal> unboundArgumentsCollection, Collection<CommandParameterInternal> nonPositionalArguments, ref int unboundArgumentsIndex)
        {
            while (unboundArgumentsIndex < unboundArgumentsCollection.Count)
            {
                CommandParameterInternal item = unboundArgumentsCollection[unboundArgumentsIndex++];
                if (!item.ParameterNameSpecified)
                {
                    return item;
                }
                nonPositionalArguments.Add(item);
                if ((unboundArgumentsCollection.Count - 1) >= unboundArgumentsIndex)
                {
                    item = unboundArgumentsCollection[unboundArgumentsIndex];
                    if (!item.ParameterNameSpecified)
                    {
                        nonPositionalArguments.Add(item);
                        unboundArgumentsIndex++;
                    }
                }
            }
            return null;
        }

        protected IScriptExtent GetParameterErrorExtent(CommandParameterInternal cpi)
        {
            IScriptExtent parameterExtent = cpi.ParameterExtent;
            if (parameterExtent == PositionUtilities.EmptyExtent)
            {
                parameterExtent = this.InvocationInfo.ScriptPosition;
            }
            return parameterExtent;
        }

        private static bool IsSwitchAndSetValue(string argumentName, CommandParameterInternal argument, CompiledCommandParameter matchingParameter)
        {
            bool flag = false;
            if (matchingParameter.Type == typeof(SwitchParameter))
            {
                argument.ParameterName = argumentName;
                argument.SetArgumentValue(PositionUtilities.EmptyExtent, SwitchParameter.Present);
                flag = true;
            }
            return flag;
        }

        internal void ReparseUnboundArguments()
        {
            Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
            for (int i = 0; i < this._unboundArguments.Count; i++)
            {
                CommandParameterInternal item = this._unboundArguments[i];
                if (!item.ParameterNameSpecified || item.ArgumentSpecified)
                {
                    collection.Add(item);
                }
                else
                {
                    string parameterName = item.ParameterName;
                    MergedCompiledCommandParameter parameter = this._bindableParameters.GetMatchingParameter(parameterName, false, true, new System.Management.Automation.InvocationInfo(this.InvocationInfo.MyCommand, item.ParameterExtent));
                    if (parameter == null)
                    {
                        collection.Add(item);
                    }
                    else if (IsSwitchAndSetValue(parameterName, item, parameter.Parameter))
                    {
                        collection.Add(item);
                    }
                    else if ((this._unboundArguments.Count - 1) > i)
                    {
                        CommandParameterInternal internal3 = this._unboundArguments[i + 1];
                        if (internal3.ParameterNameSpecified)
                        {
                            if ((this._bindableParameters.GetMatchingParameter(internal3.ParameterName, false, true, new System.Management.Automation.InvocationInfo(this.InvocationInfo.MyCommand, internal3.ParameterExtent)) != null) || internal3.ParameterAndArgumentSpecified)
                            {
                                ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetParameterErrorExtent(item), parameter.Parameter.Name, parameter.Parameter.Type, null, "ParameterBinderStrings", "MissingArgument", new object[0]);
                                throw exception;
                            }
                            i++;
                            item.ParameterName = parameter.Parameter.Name;
                            item.SetArgumentValue(internal3.ArgumentExtent, internal3.ParameterText);
                            collection.Add(item);
                        }
                        else
                        {
                            i++;
                            item.ParameterName = parameter.Parameter.Name;
                            item.SetArgumentValue(internal3.ArgumentExtent, internal3.ArgumentValue);
                            collection.Add(item);
                        }
                    }
                    else
                    {
                        ParameterBindingException exception2 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetParameterErrorExtent(item), parameter.Parameter.Name, parameter.Parameter.Type, null, "ParameterBinderStrings", "MissingArgument", new object[0]);
                        throw exception2;
                    }
                }
            }
            this._unboundArguments = collection;
        }

        protected virtual void SaveDefaultScriptParameterValue(string name, object value)
        {
        }

        protected void ThrowElaboratedBindingException(ParameterBindingException pbex)
        {
            if (pbex == null)
            {
                throw PSTraceSource.NewArgumentNullException("pbex");
            }
            string message = pbex.Message;
            StringBuilder builder = new StringBuilder();
            foreach (string str2 in this.BoundDefaultParameters)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, " -{0}", new object[] { str2 });
            }
            string resourceId = "DefaultBindingErrorElaborationSingle";
            if (this.BoundDefaultParameters.Count > 1)
            {
                resourceId = "DefaultBindingErrorElaborationMultiple";
            }
            ParameterBindingException exception = new ParameterBindingException(pbex.InnerException, pbex, "ParameterBinderStrings", resourceId, new object[] { message, builder });
            throw exception;
        }

        internal static void UpdatePositionalDictionary(SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> positionalParameterDictionary, int validParameterSets)
        {
            foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> dictionary in positionalParameterDictionary.Values)
            {
                Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
                foreach (PositionalCommandParameter parameter in dictionary.Values)
                {
                    Collection<ParameterSetSpecificMetadata> parameterSetData = parameter.ParameterSetData;
                    for (int i = parameterSetData.Count - 1; i >= 0; i--)
                    {
                        if (((parameterSetData[i].ParameterSetFlag & validParameterSets) == 0) && !parameterSetData[i].IsInAllSets)
                        {
                            parameterSetData.RemoveAt(i);
                        }
                    }
                    if (parameterSetData.Count == 0)
                    {
                        collection.Add(parameter.Parameter);
                    }
                }
                foreach (MergedCompiledCommandParameter parameter2 in collection)
                {
                    dictionary.Remove(parameter2);
                }
            }
        }

        internal MergedCommandParameterMetadata BindableParameters
        {
            get
            {
                return this._bindableParameters;
            }
        }

        protected Dictionary<string, CommandParameterInternal> BoundArguments
        {
            get
            {
                return this._boundArguments;
            }
        }

        protected Collection<string> BoundDefaultParameters
        {
            get
            {
                return this._boundDefaultParameters;
            }
        }

        protected Dictionary<string, MergedCompiledCommandParameter> BoundParameters
        {
            get
            {
                return this._boundParameters;
            }
        }

        internal System.Management.Automation.CommandLineParameters CommandLineParameters
        {
            get
            {
                return this.DefaultParameterBinder.CommandLineParameters;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this._context;
            }
        }

        internal ParameterBinderBase DefaultParameterBinder { get; private set; }

        protected bool DefaultParameterBindingInUse
        {
            get
            {
                return this._defaultParameterBindingInUse;
            }
            set
            {
                this._defaultParameterBindingInUse = value;
            }
        }

        internal System.Management.Automation.InvocationInfo InvocationInfo
        {
            get
            {
                return this._invocationInfo;
            }
        }

        internal Collection<MergedCompiledCommandParameter> ParametersBoundThroughPipelineInput
        {
            get
            {
                return this._parametersBoundThroughPipelineInput;
            }
        }

        protected Collection<CommandParameterInternal> UnboundArguments
        {
            get
            {
                return this._unboundArguments;
            }
            set
            {
                this._unboundArguments = value;
            }
        }

        protected ICollection<MergedCompiledCommandParameter> UnboundParameters { get; set; }
    }
}

