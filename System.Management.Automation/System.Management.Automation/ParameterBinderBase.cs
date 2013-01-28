namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    [DebuggerDisplay("Command = {command}")]
    internal abstract class ParameterBinderBase
    {
        [TraceSource("ParameterBinding", "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.")]
        internal static PSTraceSource bindingTracer = PSTraceSource.GetTracer("ParameterBinding", "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.", false);
        private InternalCommand command;
        private System.Management.Automation.CommandLineParameters commandLineParameters;
        private System.Management.Automation.ExecutionContext context;
        private EngineIntrinsics engine;
        private System.Management.Automation.InvocationInfo invocationInfo;
        internal bool RecordBoundParameters;
        private object target;
        [TraceSource("ParameterBinderBase", "A abstract helper class for the CommandProcessor that binds parameters to the specified object.")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterBinderBase", "A abstract helper class for the CommandProcessor that binds parameters to the specified object.");

        internal ParameterBinderBase(System.Management.Automation.InvocationInfo invocationInfo, System.Management.Automation.ExecutionContext context, InternalCommand command)
        {
            this.RecordBoundParameters = true;
            bindingTracer.ShowHeaders = false;
            this.command = command;
            this.invocationInfo = invocationInfo;
            this.context = context;
            this.engine = context.EngineIntrinsics;
        }

        internal ParameterBinderBase(object target, System.Management.Automation.InvocationInfo invocationInfo, System.Management.Automation.ExecutionContext context, InternalCommand command)
        {
            this.RecordBoundParameters = true;
            bindingTracer.ShowHeaders = false;
            this.command = command;
            this.target = target;
            this.invocationInfo = invocationInfo;
            this.context = context;
            this.engine = context.EngineIntrinsics;
        }

        internal abstract void BindParameter(string name, object value);
        internal virtual bool BindParameter(CommandParameterInternal parameter, CompiledCommandParameter parameterMetadata, ParameterBindingFlags flags)
        {
            bool flag = false;
            bool flag2 = (flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None;
            if (parameter == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameter");
            }
            if (parameterMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterMetadata");
            }
            using (bindingTracer.TraceScope("BIND arg [{0}] to parameter [{1}]", new object[] { parameter.ArgumentValue, parameterMetadata.Name }))
            {
                parameter.ParameterName = parameterMetadata.Name;
                object argumentValue = parameter.ArgumentValue;
                ScriptParameterBinder binder = this as ScriptParameterBinder;
                bool bindingScriptCmdlet = false;
                if (binder != null)
                {
                    bindingScriptCmdlet = binder.Script.UsesCmdletBinding;
                }
                foreach (ArgumentTransformationAttribute attribute in parameterMetadata.ArgumentTransformationAttributes)
                {
                    using (bindingTracer.TraceScope("Executing DATA GENERATION metadata: [{0}]", new object[] { attribute.GetType() }))
                    {
                        try
                        {
                            ArgumentTypeConverterAttribute attribute2 = attribute as ArgumentTypeConverterAttribute;
                            if (attribute2 != null)
                            {
                                if (flag2)
                                {
                                    argumentValue = attribute2.Transform(this.engine, argumentValue, true, bindingScriptCmdlet);
                                }
                            }
                            else
                            {
                                argumentValue = attribute.Transform(this.engine, argumentValue);
                            }
                            bindingTracer.WriteLine("result returned from DATA GENERATION: {0}", new object[] { argumentValue });
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            bindingTracer.WriteLine("ERROR: DATA GENERATION: {0}", new object[] { exception.Message });
                            ParameterBindingException exception2 = new ParameterBindingArgumentTransformationException(exception, ErrorCategory.InvalidData, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, (argumentValue == null) ? null : argumentValue.GetType(), "ParameterBinderStrings", "ParameterArgumentTransformationError", new object[] { exception.Message });
                            throw exception2;
                        }
                    }
                }
                if (flag2)
                {
                    argumentValue = this.CoerceTypeAsNeeded(parameter, parameterMetadata.Name, parameterMetadata.Type, parameterMetadata.CollectionTypeInformation, argumentValue);
                }
                else if (!this.ShouldContinueUncoercedBind(parameter, parameterMetadata, flags, ref argumentValue))
                {
                    goto Label_040E;
                }
                if ((parameterMetadata.PSTypeName != null) && (argumentValue != null))
                {
                    IEnumerable enumerable = LanguagePrimitives.GetEnumerable(argumentValue);
                    if (enumerable != null)
                    {
                        foreach (object obj3 in enumerable)
                        {
                            this.ValidatePSTypeName(parameter, parameterMetadata, !flag2, obj3);
                        }
                    }
                    else
                    {
                        this.ValidatePSTypeName(parameter, parameterMetadata, !flag2, argumentValue);
                    }
                }
                if ((flags & ParameterBindingFlags.IsDefaultValue) == ParameterBindingFlags.None)
                {
                    foreach (ValidateArgumentsAttribute attribute3 in parameterMetadata.ValidationAttributes)
                    {
                        using (bindingTracer.TraceScope("Executing VALIDATION metadata: [{0}]", new object[] { attribute3.GetType() }))
                        {
                            try
                            {
                                attribute3.InternalValidate(argumentValue, this.engine);
                            }
                            catch (Exception exception3)
                            {
                                CommandProcessorBase.CheckForSevereException(exception3);
                                bindingTracer.WriteLine("ERROR: VALIDATION FAILED: {0}", new object[] { exception3.Message });
                                ParameterBindingValidationException exception4 = new ParameterBindingValidationException(exception3, ErrorCategory.InvalidData, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, (argumentValue == null) ? null : argumentValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationError", new object[] { exception3.Message });
                                throw exception4;
                            }
                            tracer.WriteLine("Validation attribute on {0} returned {1}.", new object[] { parameterMetadata.Name, flag });
                        }
                    }
                    if (IsParameterMandatory(parameterMetadata))
                    {
                        this.ValidateNullOrEmptyArgument(parameter, parameterMetadata, parameterMetadata.Type, argumentValue, true);
                    }
                }
                Exception innerException = null;
                try
                {
                    this.BindParameter(parameter.ParameterName, argumentValue);
                    flag = true;
                }
                catch (SetValueException exception6)
                {
                    innerException = exception6;
                }
                if (innerException != null)
                {
                    Type typeSpecified = (argumentValue == null) ? null : argumentValue.GetType();
                    ParameterBindingException exception7 = new ParameterBindingException(innerException, ErrorCategory.WriteError, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, typeSpecified, "ParameterBinderStrings", "ParameterBindingFailed", new object[] { innerException.Message });
                    throw exception7;
                }
            Label_040E:;
                bindingTracer.WriteLine("BIND arg [{0}] to param [{1}] {2}", new object[] { argumentValue, parameter.ParameterName, flag ? "SUCCESSFUL" : "SKIPPED" });
                if (flag)
                {
                    if (this.RecordBoundParameters)
                    {
                        this.CommandLineParameters.Add(parameter.ParameterName, argumentValue);
                    }
                    MshCommandRuntime commandRuntime = this.Command.commandRuntime as MshCommandRuntime;
                    if ((commandRuntime != null) && commandRuntime.LogPipelineExecutionDetail)
                    {
                        IEnumerable source = LanguagePrimitives.GetEnumerable(argumentValue);
                        if (source != null)
                        {
                            string parameterValue = string.Join(", ", source.Cast<object>().ToArray<object>());
                            commandRuntime.PipelineProcessor.LogExecutionParameterBinding(this.InvocationInfo, parameter.ParameterName, parameterValue);
                        }
                        else
                        {
                            commandRuntime.PipelineProcessor.LogExecutionParameterBinding(this.InvocationInfo, parameter.ParameterName, (argumentValue == null) ? "" : argumentValue.ToString());
                        }
                    }
                }
                return flag;
            }
        }

        private object CoerceTypeAsNeeded(CommandParameterInternal argument, string parameterName, Type toType, ParameterCollectionTypeInformation collectionTypeInfo, object currentValue)
        {
            if (argument == null)
            {
                throw PSTraceSource.NewArgumentNullException("argument");
            }
            if (toType == null)
            {
                throw PSTraceSource.NewArgumentNullException("toType");
            }
            if (collectionTypeInfo == null)
            {
                collectionTypeInfo = new ParameterCollectionTypeInformation(toType);
            }
            object result = currentValue;
            using (bindingTracer.TraceScope("COERCE arg to [{0}]", new object[] { toType }))
            {
                Type c = null;
                try
                {
                    if (IsNullParameterValue(currentValue))
                    {
                        return this.HandleNullParameterForSpecialTypes(argument, parameterName, toType, currentValue);
                    }
                    c = currentValue.GetType();
                    if (toType.IsAssignableFrom(c))
                    {
                        bindingTracer.WriteLine("Parameter and arg types the same, no coercion is needed.", new object[0]);
                        return currentValue;
                    }
                    bindingTracer.WriteLine("Trying to convert argument value from {0} to {1}", new object[] { c, toType });
                    if (toType == typeof(PSObject))
                    {
                        if ((this.command != null) && (currentValue == this.command.CurrentPipelineObject.BaseObject))
                        {
                            currentValue = this.command.CurrentPipelineObject;
                        }
                        bindingTracer.WriteLine("The parameter is of type [{0}] and the argument is an PSObject, so the parameter value is the argument value wrapped into an PSObject.", new object[] { toType });
                        return LanguagePrimitives.AsPSObjectOrNull(currentValue);
                    }
                    if ((toType == typeof(string)) && (c == typeof(PSObject)))
                    {
                        PSObject obj3 = (PSObject) currentValue;
                        if (obj3 == AutomationNull.Value)
                        {
                            bindingTracer.WriteLine("CONVERT a null PSObject to a null string.", new object[0]);
                            return null;
                        }
                    }
                    if (((toType == typeof(bool)) || (toType == typeof(SwitchParameter))) || (toType == typeof(bool?)))
                    {
                        Type type = null;
                        if (c == typeof(PSObject))
                        {
                            PSObject obj4 = (PSObject) currentValue;
                            currentValue = obj4.BaseObject;
                            if (currentValue is SwitchParameter)
                            {
                                SwitchParameter parameter = (SwitchParameter) currentValue;
                                currentValue = parameter.IsPresent;
                            }
                            type = currentValue.GetType();
                        }
                        else
                        {
                            type = c;
                        }
                        if (type == typeof(bool))
                        {
                            if (LanguagePrimitives.IsBooleanType(toType))
                            {
                                return ParserOps.BoolToObject((bool) currentValue);
                            }
                            return new SwitchParameter((bool) currentValue);
                        }
                        if (type == typeof(int))
                        {
                            if (((int) LanguagePrimitives.ConvertTo(currentValue, typeof(int), CultureInfo.InvariantCulture)) != 0)
                            {
                                if (LanguagePrimitives.IsBooleanType(toType))
                                {
                                    return ParserOps.BoolToObject(true);
                                }
                                return new SwitchParameter(true);
                            }
                            if (LanguagePrimitives.IsBooleanType(toType))
                            {
                                return ParserOps.BoolToObject(false);
                            }
                            return new SwitchParameter(false);
                        }
                        if (LanguagePrimitives.IsNumeric(Type.GetTypeCode(type)))
                        {
                            double num = (double) LanguagePrimitives.ConvertTo(currentValue, typeof(double), CultureInfo.InvariantCulture);
                            if (num == 0.0)
                            {
                                if (LanguagePrimitives.IsBooleanType(toType))
                                {
                                    return ParserOps.BoolToObject(false);
                                }
                                return new SwitchParameter(false);
                            }
                            if (LanguagePrimitives.IsBooleanType(toType))
                            {
                                return ParserOps.BoolToObject(true);
                            }
                            return new SwitchParameter(true);
                        }
                        ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, c, "ParameterBinderStrings", "CannotConvertArgument", new object[] { type, "" });
                        throw exception;
                    }
                    if ((collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric) || (collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.IList))
                    {
                        object obj5 = PSObject.Base(currentValue);
                        if (obj5 != null)
                        {
                            ConversionRank conversionRank = LanguagePrimitives.GetConversionRank(obj5.GetType(), toType);
                            if ((((conversionRank == ConversionRank.Constructor) || (conversionRank == ConversionRank.ImplicitCast)) || (conversionRank == ConversionRank.ExplicitCast)) && LanguagePrimitives.TryConvertTo(currentValue, toType, Thread.CurrentThread.CurrentCulture, out result))
                            {
                                return result;
                            }
                        }
                    }
                    if (collectionTypeInfo.ParameterCollectionType != ParameterCollectionType.NotCollection)
                    {
                        bindingTracer.WriteLine("ENCODING arg into collection", new object[0]);
                        bool coercionRequired = false;
                        return this.EncodeCollection(argument, parameterName, collectionTypeInfo, toType, currentValue, collectionTypeInfo.ElementType != null, out coercionRequired);
                    }
                    if (((((GetIList(currentValue) != null) && (toType != typeof(object))) && ((toType != typeof(PSObject)) && (toType != typeof(PSListModifier)))) && ((!toType.IsGenericType || (toType.GetGenericTypeDefinition() != typeof(PSListModifier<>))) && (!toType.IsGenericType || (toType.GetGenericTypeDefinition() != typeof(FlagsExpression<>))))) && !toType.IsEnum)
                    {
                        throw new NotSupportedException();
                    }
                    bindingTracer.WriteLine("CONVERT arg type to param type using LanguagePrimitives.ConvertTo", new object[0]);
                    bool flag2 = false;
                    if (this.context.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                    {
                        object obj6 = PSObject.Base(currentValue);
                        bool flag3 = obj6 is PSObject;
                        bool flag4 = (obj6 != null) && typeof(IDictionary).IsAssignableFrom(obj6.GetType());
                        flag2 = ((((PSLanguageMode) this.Command.CommandInfo.DefiningLanguageMode) == PSLanguageMode.FullLanguage) && !flag3) && !flag4;
                    }
                    try
                    {
                        if (flag2)
                        {
                            this.context.LanguageMode = PSLanguageMode.FullLanguage;
                        }
                        result = LanguagePrimitives.ConvertTo(currentValue, toType, Thread.CurrentThread.CurrentCulture);
                    }
                    finally
                    {
                        if (flag2)
                        {
                            this.context.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                        }
                    }
                    bindingTracer.WriteLine("CONVERT SUCCESSFUL using LanguagePrimitives.ConvertTo: [{0}]", new object[] { (result == null) ? "null" : result.ToString() });
                    return result;
                }
                catch (NotSupportedException exception2)
                {
                    bindingTracer.TraceError("ERROR: COERCE FAILED: arg [{0}] could not be converted to the parameter type [{1}]", new object[] { (result == null) ? "null" : result, toType });
                    ParameterBindingException exception3 = new ParameterBindingException(exception2, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, c, "ParameterBinderStrings", "CannotConvertArgument", new object[] { (result == null) ? "null" : result, exception2.Message });
                    throw exception3;
                }
                catch (PSInvalidCastException exception4)
                {
                    object[] args = new object[] { result ?? "null", toType };
                    bindingTracer.TraceError("ERROR: COERCE FAILED: arg [{0}] could not be converted to the parameter type [{1}]", args);
                    ParameterBindingException exception5 = new ParameterBindingException(exception4, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, c, "ParameterBinderStrings", "CannotConvertArgumentNoMessage", new object[] { exception4.Message });
                    throw exception5;
                }
            }
            return result;
        }

        private object EncodeCollection(CommandParameterInternal argument, string parameterName, ParameterCollectionTypeInformation collectionTypeInformation, Type toType, object currentValue, bool coerceElementTypeIfNeeded, out bool coercionRequired)
        {
            object obj2 = null;
            coercionRequired = false;
            bindingTracer.WriteLine("Binding collection parameter {0}: argument type [{1}], parameter type [{2}], collection type {3}, element type [{4}], {5}", new object[] { parameterName, (currentValue == null) ? "null" : currentValue.GetType().Name, toType, collectionTypeInformation.ParameterCollectionType, collectionTypeInformation.ElementType, coerceElementTypeIfNeeded ? "coerceElementType" : "no coerceElementType" });
            if (currentValue != null)
            {
                int length = 1;
                Type elementType = collectionTypeInformation.ElementType;
                IList iList = GetIList(currentValue);
                if (iList != null)
                {
                    length = iList.Count;
                    tracer.WriteLine("current value is an IList with {0} elements", new object[] { length });
                    bindingTracer.WriteLine("Arg is IList with {0} elements", new object[] { length });
                }
                object obj3 = null;
                IList list2 = null;
                MethodInfo info = null;
                bool flag = toType == typeof(Array);
                if ((collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array) || flag)
                {
                    if (flag)
                    {
                        elementType = typeof(object);
                    }
                    bindingTracer.WriteLine("Creating array with element type [{0}] and {1} elements", new object[] { elementType, length });
                    obj3 = list2 = Array.CreateInstance(elementType, length);
                }
                else
                {
                    if ((collectionTypeInformation.ParameterCollectionType != ParameterCollectionType.IList) && (collectionTypeInformation.ParameterCollectionType != ParameterCollectionType.ICollectionGeneric))
                    {
                        return obj2;
                    }
                    bindingTracer.WriteLine("Creating collection [{0}]", new object[] { toType });
                    bool flag2 = false;
                    Exception innerException = null;
                    try
                    {
                        obj3 = Activator.CreateInstance(toType, BindingFlags.Default, null, new object[0], CultureInfo.InvariantCulture);
                        if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
                        {
                            list2 = (IList) obj3;
                        }
                        else
                        {
                            Type type2 = collectionTypeInformation.ElementType;
                            BindingFlags bindingAttr = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
                            Exception exception2 = null;
                            try
                            {
                                info = toType.GetMethod("Add", bindingAttr, null, new Type[] { type2 }, null);
                            }
                            catch (AmbiguousMatchException exception3)
                            {
                                bindingTracer.WriteLine("Ambiguous match to Add(T) for type " + toType.FullName + ": " + exception3.Message, new object[0]);
                                exception2 = exception3;
                            }
                            catch (ArgumentException exception4)
                            {
                                bindingTracer.WriteLine("ArgumentException matching Add(T) for type " + toType.FullName + ": " + exception4.Message, new object[0]);
                                exception2 = exception4;
                            }
                            if (null == info)
                            {
                                ParameterBindingException exception5 = new ParameterBindingException(exception2, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, (currentValue == null) ? null : currentValue.GetType(), "ParameterBinderStrings", "CannotExtractAddMethod", new object[] { (exception2 == null) ? "" : exception2.Message });
                                throw exception5;
                            }
                        }
                    }
                    catch (ArgumentException exception6)
                    {
                        flag2 = true;
                        innerException = exception6;
                    }
                    catch (NotSupportedException exception7)
                    {
                        flag2 = true;
                        innerException = exception7;
                    }
                    catch (TargetInvocationException exception8)
                    {
                        flag2 = true;
                        innerException = exception8;
                    }
                    catch (MethodAccessException exception9)
                    {
                        flag2 = true;
                        innerException = exception9;
                    }
                    catch (MemberAccessException exception10)
                    {
                        flag2 = true;
                        innerException = exception10;
                    }
                    catch (InvalidComObjectException exception11)
                    {
                        flag2 = true;
                        innerException = exception11;
                    }
                    catch (COMException exception12)
                    {
                        flag2 = true;
                        innerException = exception12;
                    }
                    catch (TypeLoadException exception13)
                    {
                        flag2 = true;
                        innerException = exception13;
                    }
                    if (flag2)
                    {
                        ParameterBindingException exception14 = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, currentValue.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[] { (obj2 == null) ? "null" : obj2, (innerException == null) ? "" : innerException.Message });
                        throw exception14;
                    }
                }
                if (iList != null)
                {
                    int num2 = 0;
                    bindingTracer.WriteLine("Argument type {0} is IList", new object[] { currentValue.GetType() });
                    foreach (object obj4 in iList)
                    {
                        object obj5 = PSObject.Base(obj4);
                        if (coerceElementTypeIfNeeded)
                        {
                            bindingTracer.WriteLine("COERCE collection element from type {0} to type {1}", new object[] { (obj4 == null) ? "null" : obj4.GetType().Name, elementType });
                            obj5 = this.CoerceTypeAsNeeded(argument, parameterName, elementType, null, obj4);
                        }
                        else if ((null != elementType) && (obj5 != null))
                        {
                            Type type = obj5.GetType();
                            Type c = elementType;
                            if ((type != c) && !type.IsSubclassOf(c))
                            {
                                bindingTracer.WriteLine("COERCION REQUIRED: Did not attempt to coerce collection element from type {0} to type {1}", new object[] { (obj4 == null) ? "null" : obj4.GetType().Name, elementType });
                                coercionRequired = true;
                                break;
                            }
                        }
                        try
                        {
                            if ((collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array) || flag)
                            {
                                bindingTracer.WriteLine("Adding element of type {0} to array position {1}", new object[] { (obj5 == null) ? "null" : obj5.GetType().Name, num2 });
                                list2[num2++] = obj5;
                            }
                            else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
                            {
                                bindingTracer.WriteLine("Adding element of type {0} via IList.Add", new object[] { (obj5 == null) ? "null" : obj5.GetType().Name });
                                list2.Add(obj5);
                            }
                            else
                            {
                                bindingTracer.WriteLine("Adding element of type {0} via ICollection<T>::Add()", new object[] { (obj5 == null) ? "null" : obj5.GetType().Name });
                                info.Invoke(obj3, new object[] { obj5 });
                            }
                        }
                        catch (Exception exception15)
                        {
                            CommandProcessorBase.CheckForSevereException(exception15);
                            if ((exception15 is TargetInvocationException) && (exception15.InnerException != null))
                            {
                                exception15 = exception15.InnerException;
                            }
                            ParameterBindingException exception16 = new ParameterBindingException(exception15, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, (obj5 == null) ? null : obj5.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[] { (obj5 == null) ? "null" : obj5, exception15.Message });
                            throw exception16;
                        }
                    }
                }
                else
                {
                    bindingTracer.WriteLine("Argument type {0} is not IList, treating this as scalar", new object[] { (currentValue == null) ? "null" : currentValue.GetType().Name });
                    if (elementType != null)
                    {
                        if (coerceElementTypeIfNeeded)
                        {
                            bindingTracer.WriteLine("Coercing scalar arg value to type {0}", new object[] { elementType });
                            currentValue = this.CoerceTypeAsNeeded(argument, parameterName, elementType, null, currentValue);
                        }
                        else
                        {
                            Type type5 = currentValue.GetType();
                            Type type6 = elementType;
                            if ((type5 != type6) && !type5.IsSubclassOf(type6))
                            {
                                bindingTracer.WriteLine("COERCION REQUIRED: Did not coerce scalar arg value to type {1}", new object[] { elementType });
                                coercionRequired = true;
                                return obj2;
                            }
                        }
                    }
                    try
                    {
                        if ((collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array) || flag)
                        {
                            bindingTracer.WriteLine("Adding scalar element of type {0} to array position {1}", new object[] { (currentValue == null) ? "null" : currentValue.GetType().Name, 0 });
                            list2[0] = currentValue;
                        }
                        else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
                        {
                            bindingTracer.WriteLine("Adding scalar element of type {0} via IList.Add", new object[] { (currentValue == null) ? "null" : currentValue.GetType().Name });
                            list2.Add(currentValue);
                        }
                        else
                        {
                            bindingTracer.WriteLine("Adding scalar element of type {0} via ICollection<T>::Add()", new object[] { (currentValue == null) ? "null" : currentValue.GetType().Name });
                            info.Invoke(obj3, new object[] { currentValue });
                        }
                    }
                    catch (Exception exception17)
                    {
                        CommandProcessorBase.CheckForSevereException(exception17);
                        if ((exception17 is TargetInvocationException) && (exception17.InnerException != null))
                        {
                            exception17 = exception17.InnerException;
                        }
                        ParameterBindingException exception18 = new ParameterBindingException(exception17, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, (currentValue == null) ? null : currentValue.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[] { (currentValue == null) ? "null" : currentValue, exception17.Message });
                        throw exception18;
                    }
                }
                if (!coercionRequired)
                {
                    obj2 = obj3;
                }
            }
            return obj2;
        }

        internal abstract object GetDefaultParameterValue(string name);
        protected IScriptExtent GetErrorExtent(CommandParameterInternal cpi)
        {
            IScriptExtent errorExtent = cpi.ErrorExtent;
            if (errorExtent == PositionUtilities.EmptyExtent)
            {
                errorExtent = this.InvocationInfo.ScriptPosition;
            }
            return errorExtent;
        }

        internal static IList GetIList(object value)
        {
            IList list = value as IList;
            if (list != null)
            {
                tracer.WriteLine("argument is IList", new object[0]);
                return list;
            }
            PSObject obj2 = value as PSObject;
            if (obj2 != null)
            {
                IList baseObject = obj2.BaseObject as IList;
                if (baseObject != null)
                {
                    tracer.WriteLine("argument is PSObject with BaseObject as IList", new object[0]);
                    list = baseObject;
                }
            }
            return list;
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

        private object HandleNullParameterForSpecialTypes(CommandParameterInternal argument, string parameterName, Type toType, object currentValue)
        {
            if (toType == typeof(bool))
            {
                bindingTracer.WriteLine("ERROR: No argument is specified for parameter and parameter type is BOOL", new object[0]);
                ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(argument), parameterName, toType, null, "ParameterBinderStrings", "ParameterArgumentValidationErrorNullNotAllowed", new object[] { "" });
                throw exception;
            }
            if (toType == typeof(SwitchParameter))
            {
                bindingTracer.WriteLine("Arg is null or not present, parameter type is SWITCHPARAMTER, value is true.", new object[0]);
                return SwitchParameter.Present;
            }
            if (currentValue == UnboundParameter.Value)
            {
                bindingTracer.TraceError("ERROR: No argument was specified for the parameter and the parameter is not of type bool", new object[0]);
                ParameterBindingException exception2 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetParameterErrorExtent(argument), parameterName, toType, null, "ParameterBinderStrings", "MissingArgument", new object[0]);
                throw exception2;
            }
            bindingTracer.WriteLine("Arg is null, parameter type not bool or SwitchParameter, value is null.", new object[0]);
            return null;
        }

        private static bool IsNullParameterValue(object currentValue)
        {
            bool flag = false;
            if (((currentValue != null) && (currentValue != AutomationNull.Value)) && (currentValue != UnboundParameter.Value))
            {
                return flag;
            }
            return true;
        }

        private static bool IsParameterMandatory(CompiledCommandParameter parameterMetadata)
        {
            bool flag = false;
            foreach (ParameterSetSpecificMetadata metadata in parameterMetadata.ParameterSetData.Values)
            {
                if (metadata.IsMandatory)
                {
                    flag = true;
                    break;
                }
            }
            tracer.WriteLine("isMandatory = {0}", new object[] { flag });
            return flag;
        }

        private bool ShouldContinueUncoercedBind(CommandParameterInternal parameter, CompiledCommandParameter parameterMetadata, ParameterBindingFlags flags, ref object parameterValue)
        {
            bool flag = false;
            Type c = parameterMetadata.Type;
            if (parameterValue == null)
            {
                return (((c == null) || ((flags & ParameterBindingFlags.IsDefaultValue) != ParameterBindingFlags.None)) || (!c.IsValueType && (c != typeof(string))));
            }
            Type type = parameterValue.GetType();
            if (type == c)
            {
                return true;
            }
            if (type.IsSubclassOf(c))
            {
                return true;
            }
            if (c.IsAssignableFrom(type))
            {
                return true;
            }
            if ((parameterValue is PSObject) && !((PSObject) parameterValue).immediateBaseObjectIsEmpty)
            {
                parameterValue = ((PSObject) parameterValue).BaseObject;
                type = parameterValue.GetType();
                if (((type == c) || type.IsSubclassOf(c)) || c.IsAssignableFrom(type))
                {
                    return true;
                }
            }
            if (parameterMetadata.CollectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection)
            {
                bool coercionRequired = false;
                object obj2 = this.EncodeCollection(parameter, parameterMetadata.Name, parameterMetadata.CollectionTypeInformation, c, parameterValue, false, out coercionRequired);
                if ((obj2 != null) && !coercionRequired)
                {
                    parameterValue = obj2;
                    flag = true;
                }
            }
            return flag;
        }

        private void ValidateNullOrEmptyArgument(CommandParameterInternal parameter, CompiledCommandParameter parameterMetadata, Type argumentType, object parameterValue, bool recurseIntoCollections)
        {
            if ((parameterValue == null) && (argumentType != typeof(bool?)))
            {
                if (!parameterMetadata.AllowsNullArgument)
                {
                    bindingTracer.WriteLine("ERROR: Argument cannot be null", new object[0]);
                    ParameterBindingValidationException exception = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, argumentType, (parameterValue == null) ? null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationErrorNullNotAllowed", new object[0]);
                    throw exception;
                }
            }
            else if (argumentType == typeof(string))
            {
                string str = parameterValue as string;
                if ((str.Length == 0) && !parameterMetadata.AllowsEmptyStringArgument)
                {
                    bindingTracer.WriteLine("ERROR: Argument cannot be an empty string", new object[0]);
                    ParameterBindingValidationException exception2 = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, (parameterValue == null) ? null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationErrorEmptyStringNotAllowed", new object[0]);
                    throw exception2;
                }
            }
            else if (recurseIntoCollections)
            {
                switch (parameterMetadata.CollectionTypeInformation.ParameterCollectionType)
                {
                    case ParameterCollectionType.IList:
                    case ParameterCollectionType.Array:
                    case ParameterCollectionType.ICollectionGeneric:
                    {
                        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(parameterValue);
                        bool flag = true;
                        while (ParserOps.MoveNext(null, null, enumerator))
                        {
                            object obj2 = ParserOps.Current(null, enumerator);
                            flag = false;
                            this.ValidateNullOrEmptyArgument(parameter, parameterMetadata, parameterMetadata.CollectionTypeInformation.ElementType, obj2, false);
                        }
                        if (flag && !parameterMetadata.AllowsEmptyCollectionArgument)
                        {
                            bindingTracer.WriteLine("ERROR: Argument cannot be an empty collection", new object[0]);
                            ParameterBindingValidationException exception3 = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, (parameterValue == null) ? null : parameterValue.GetType(), "ParameterBinderStrings", (parameterMetadata.CollectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array) ? "ParameterArgumentValidationErrorEmptyArrayNotAllowed" : "ParameterArgumentValidationErrorEmptyCollectionNotAllowed", new object[0]);
                            throw exception3;
                        }
                        return;
                    }
                }
            }
        }

        private void ValidatePSTypeName(CommandParameterInternal parameter, CompiledCommandParameter parameterMetadata, bool retryOtherBindingAfterFailure, object parameterValue)
        {
            if (parameterValue != null)
            {
                IEnumerable<string> internalTypeNames = PSObject.AsPSObject(parameterValue).InternalTypeNames;
                string pSTypeName = parameterMetadata.PSTypeName;
                if (!internalTypeNames.Contains<string>(pSTypeName, StringComparer.OrdinalIgnoreCase))
                {
                    ParameterBindingException exception2;
                    object[] arguments = new object[] { ((this.invocationInfo != null) && (this.invocationInfo.MyCommand != null)) ? this.invocationInfo.MyCommand.Name : string.Empty, parameterMetadata.Name, parameterMetadata.Type, parameterValue.GetType(), 0, 0, pSTypeName };
                    PSInvalidCastException innerException = new PSInvalidCastException(ErrorCategory.InvalidArgument.ToString(), null, ResourceManagerCache.GetResourceString("ParameterBinderStrings", "MismatchedPSTypeName"), arguments);
                    if (!retryOtherBindingAfterFailure)
                    {
                        exception2 = new ParameterBindingArgumentTransformationException(innerException, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, parameterValue.GetType(), "ParameterBinderStrings", "MismatchedPSTypeName", new object[] { pSTypeName });
                    }
                    else
                    {
                        exception2 = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.InvocationInfo, this.GetErrorExtent(parameter), parameterMetadata.Name, parameterMetadata.Type, parameterValue.GetType(), "ParameterBinderStrings", "MismatchedPSTypeName", new object[] { pSTypeName });
                    }
                    throw exception2;
                }
            }
        }

        internal InternalCommand Command
        {
            get
            {
                return this.command;
            }
        }

        internal System.Management.Automation.CommandLineParameters CommandLineParameters
        {
            get
            {
                if (this.commandLineParameters == null)
                {
                    this.commandLineParameters = new System.Management.Automation.CommandLineParameters();
                }
                return this.commandLineParameters;
            }
            set
            {
                this.commandLineParameters = value;
            }
        }

        internal System.Management.Automation.ExecutionContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal System.Management.Automation.InvocationInfo InvocationInfo
        {
            get
            {
                return this.invocationInfo;
            }
        }

        internal object Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }
    }
}

