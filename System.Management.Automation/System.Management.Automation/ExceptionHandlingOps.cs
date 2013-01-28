namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class ExceptionHandlingOps
    {
        internal static void CheckActionPreference(FunctionContext funcContext, Exception exception)
        {
            ActionPreference preference;
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            CommandProcessorBase.CheckForSevereException(exception);
            RuntimeException exception2 = exception as RuntimeException;
            if (exception2 == null)
            {
                exception2 = ConvertToRuntimeException(exception, funcContext.CurrentPosition);
            }
            else
            {
                InterpreterError.UpdateExceptionErrorRecordPosition(exception2, funcContext.CurrentPosition);
            }
            RuntimeException.LockStackTrace(exception2);
            ExecutionContext context = funcContext._executionContext;
            Pipe outputPipe = funcContext._outputPipe;
            IScriptExtent scriptPosition = exception2.ErrorRecord.InvocationInfo.ScriptPosition;
            SetErrorVariables(scriptPosition, exception2, context, outputPipe);
            context.QuestionMarkVariableValue = false;
            bool flag = funcContext._traps.Any<Tuple<Type[], Action<FunctionContext>[], Type[]>>() && (funcContext._traps.Last<Tuple<Type[], Action<FunctionContext>[], Type[]>>().Item2 != null);
            if (!flag && !NeedToQueryForActionPreference(exception2, context))
            {
                throw exception2;
            }
            if (flag)
            {
                preference = ProcessTraps(funcContext, exception2);
            }
            else
            {
                preference = QueryForAction(exception2, exception2.Message, context);
            }
            context.QuestionMarkVariableValue = false;
            switch (preference)
            {
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Ignore:
                    return;

                case ActionPreference.Stop:
                    exception2.SuppressPromptInInterpreter = true;
                    throw exception2;
            }
            if (!flag && exception2.WasThrownFromThrowStatement)
            {
                throw exception2;
            }
            bool flag2 = ReportErrorRecord(scriptPosition, exception2, context);
            context.QuestionMarkVariableValue = false;
            if (!flag2)
            {
                throw exception2;
            }
        }

        internal static void ConvertToArgumentConversionException(Exception exception, string parameterName, object argument, string method, Type toType)
        {
            throw new MethodException("MethodArgumentConversionInvalidCastArgument", exception, ExtendedTypeSystem.MethodArgumentConversionException, new object[] { parameterName, argument, method, toType, exception.Message });
        }

        internal static RuntimeException ConvertToException(object result, IScriptExtent extent)
        {
            result = PSObject.Base(result);
            RuntimeException exception = result as RuntimeException;
            if (exception != null)
            {
                InterpreterError.UpdateExceptionErrorRecordPosition(exception, extent);
                exception.WasThrownFromThrowStatement = true;
                return exception;
            }
            ErrorRecord errorRecord = result as ErrorRecord;
            if (errorRecord != null)
            {
                exception = new RuntimeException(errorRecord.ToString(), errorRecord.Exception, errorRecord) {
                    WasThrownFromThrowStatement = true
                };
                InterpreterError.UpdateExceptionErrorRecordPosition(exception, extent);
                return exception;
            }
            Exception exception3 = result as Exception;
            if (exception3 != null)
            {
                errorRecord = new ErrorRecord(exception3, exception3.Message, ErrorCategory.OperationStopped, null);
                exception = new RuntimeException(exception3.Message, exception3, errorRecord) {
                    WasThrownFromThrowStatement = true
                };
                InterpreterError.UpdateExceptionErrorRecordPosition(exception, extent);
                return exception;
            }
            string message = LanguagePrimitives.IsNull(result) ? "ScriptHalted" : ParserOps.ConvertTo<string>(result, PositionUtilities.EmptyExtent);
            exception3 = new RuntimeException(message, null);
            errorRecord = new ErrorRecord(exception3, message, ErrorCategory.OperationStopped, null);
            exception = new RuntimeException(message, exception3, errorRecord) {
                WasThrownFromThrowStatement = true
            };
            exception.SetTargetObject(result);
            InterpreterError.UpdateExceptionErrorRecordPosition(exception, extent);
            return exception;
        }

        internal static void ConvertToMethodInvocationException(Exception exception, Type typeToThrow, string methodName, int numArgs, MemberInfo memberInfo = null)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            CommandProcessorBase.CheckForSevereException(exception);
            if (((!(exception is FlowControlException) && !(exception is ScriptCallDepthException)) && !(exception is PipelineStoppedException)) || ((memberInfo != null) && ((memberInfo.DeclaringType == typeof(PowerShell)) || (memberInfo.DeclaringType == typeof(Pipeline)))))
            {
                if (typeToThrow == typeof(MethodException))
                {
                    if (!(exception is MethodException))
                    {
                        throw new MethodInvocationException(exception.GetType().Name, exception, ExtendedTypeSystem.MethodInvocationException, new object[] { methodName, numArgs, exception.Message });
                    }
                }
                else
                {
                    if (methodName.StartsWith("set_", StringComparison.Ordinal) || methodName.StartsWith("get_", StringComparison.Ordinal))
                    {
                        methodName = methodName.Substring(4);
                    }
                    if (typeToThrow == typeof(GetValueInvocationException))
                    {
                        if (!(exception is GetValueException))
                        {
                            throw new GetValueInvocationException("ExceptionWhenGetting", exception, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { methodName, exception.Message });
                        }
                    }
                    else if (!(exception is SetValueException))
                    {
                        throw new SetValueInvocationException("ExceptionWhenSetting", exception, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { methodName, exception.Message });
                    }
                }
            }
        }

        internal static RuntimeException ConvertToRuntimeException(Exception exception, IScriptExtent extent)
        {
            RuntimeException exception2 = exception as RuntimeException;
            if (exception2 == null)
            {
                IContainsErrorRecord record = exception as IContainsErrorRecord;
                ErrorRecord errorRecord = (record != null) ? record.ErrorRecord : new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.OperationStopped, null);
                exception2 = new RuntimeException(exception.Message, exception, errorRecord);
            }
            InterpreterError.UpdateExceptionErrorRecordPosition(exception2, extent);
            return exception2;
        }

        internal static int FindMatchingHandler(MutableTuple tuple, RuntimeException rte, Type[] types, ExecutionContext context)
        {
            Exception replaceParentContainsErrorRecordException = rte;
            Exception innerException = rte.InnerException;
            int index = -1;
            if (innerException != null)
            {
                index = FindMatchingHandlerByType(innerException.GetType(), types);
                replaceParentContainsErrorRecordException = innerException;
            }
            if ((index == -1) || types[index].Equals(typeof(CatchAll)))
            {
                index = FindMatchingHandlerByType(rte.GetType(), types);
                replaceParentContainsErrorRecordException = rte;
            }
            if ((index == -1) || types[index].Equals(typeof(CatchAll)))
            {
                ActionPreferenceStopException exception3 = rte as ActionPreferenceStopException;
                if (exception3 != null)
                {
                    replaceParentContainsErrorRecordException = exception3.ErrorRecord.Exception;
                    if (replaceParentContainsErrorRecordException is RuntimeException)
                    {
                        return FindMatchingHandler(tuple, (RuntimeException) replaceParentContainsErrorRecordException, types, context);
                    }
                    if (replaceParentContainsErrorRecordException != null)
                    {
                        index = FindMatchingHandlerByType(replaceParentContainsErrorRecordException.GetType(), types);
                    }
                }
                else if ((rte is CmdletInvocationException) && (innerException != null))
                {
                    replaceParentContainsErrorRecordException = innerException.InnerException;
                    if (replaceParentContainsErrorRecordException != null)
                    {
                        index = FindMatchingHandlerByType(replaceParentContainsErrorRecordException.GetType(), types);
                    }
                }
            }
            if (index != -1)
            {
                ErrorRecord record = new ErrorRecord(rte.ErrorRecord, replaceParentContainsErrorRecordException);
                tuple.SetAutomaticVariable(AutomaticVariable.Underbar, record, context);
            }
            return index;
        }

        private static int FindMatchingHandlerByType(Type exceptionType, Type[] types)
        {
            int num;
            for (num = 0; num < types.Length; num++)
            {
                if (exceptionType.Equals(types[num]))
                {
                    return num;
                }
            }
            for (num = 0; num < types.Length; num++)
            {
                if (exceptionType.IsSubclassOf(types[num]))
                {
                    return num;
                }
            }
            for (num = 0; num < types.Length; num++)
            {
                if (types[num].Equals(typeof(CatchAll)))
                {
                    return num;
                }
            }
            return -1;
        }

        internal static ActionPreference InquireForActionPreference(string message, ExecutionContext context)
        {
            int num;
            InternalHostUserInterface uI = (InternalHostUserInterface) context.EngineHostInterface.UI;
            Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
            string continueLabel = ParserStrings.ContinueLabel;
            string continueHelpMessage = ParserStrings.ContinueHelpMessage;
            string silentlyContinueLabel = ParserStrings.SilentlyContinueLabel;
            string silentlyContinueHelpMessage = ParserStrings.SilentlyContinueHelpMessage;
            string breakLabel = ParserStrings.BreakLabel;
            string breakHelpMessage = ParserStrings.BreakHelpMessage;
            string suspendLabel = ParserStrings.SuspendLabel;
            string helpMessage = StringUtil.Format(ParserStrings.SuspendHelpMessage, new object[0]);
            choices.Add(new ChoiceDescription(continueLabel, continueHelpMessage));
            choices.Add(new ChoiceDescription(silentlyContinueLabel, silentlyContinueHelpMessage));
            choices.Add(new ChoiceDescription(breakLabel, breakHelpMessage));
            choices.Add(new ChoiceDescription(suspendLabel, helpMessage));
            string exceptionActionPromptCaption = ParserStrings.ExceptionActionPromptCaption;
            while ((num = uI.PromptForChoice(exceptionActionPromptCaption, message, choices, 0)) == 3)
            {
                context.EngineHostInterface.EnterNestedPrompt();
            }
            switch (num)
            {
                case 0:
                    return ActionPreference.Continue;

                case 1:
                    return ActionPreference.SilentlyContinue;
            }
            return ActionPreference.Stop;
        }

        internal static bool NeedToQueryForActionPreference(RuntimeException rte, ExecutionContext context)
        {
            return (((!context.ExceptionHandlerInEnclosingStatementBlock && (context.ShellFunctionErrorOutputPipe != null)) && (!context.CurrentPipelineStopping && !rte.SuppressPromptInInterpreter)) && !(rte is PipelineStoppedException));
        }

        private static ActionPreference ProcessTraps(FunctionContext funcContext, RuntimeException rte)
        {
            int index = -1;
            Exception replaceParentContainsErrorRecordException = null;
            Exception innerException = rte.InnerException;
            Type[] types = funcContext._traps.Last<Tuple<Type[], Action<FunctionContext>[], Type[]>>().Item1;
            Action<FunctionContext>[] actionArray = funcContext._traps.Last<Tuple<Type[], Action<FunctionContext>[], Type[]>>().Item2;
            if (innerException != null)
            {
                index = FindMatchingHandlerByType(innerException.GetType(), types);
                replaceParentContainsErrorRecordException = innerException;
            }
            if ((index == -1) || types[index].Equals(typeof(CatchAll)))
            {
                int num2 = FindMatchingHandlerByType(rte.GetType(), types);
                if (num2 != index)
                {
                    index = num2;
                    replaceParentContainsErrorRecordException = rte;
                }
            }
            if (index != -1)
            {
                try
                {
                    ErrorRecord errorRecord = rte.ErrorRecord;
                    ExecutionContext context = funcContext._executionContext;
                    if (context.CurrentCommandProcessor != null)
                    {
                        context.CurrentCommandProcessor.ForgetScriptException();
                    }
                    try
                    {
                        MutableTuple tuple = MutableTuple.MakeTuple(funcContext._traps.Last<Tuple<Type[], Action<FunctionContext>[], Type[]>>().Item3[index], Compiler.DottedLocalsNameIndexMap);
                        tuple.SetAutomaticVariable(AutomaticVariable.Underbar, new ErrorRecord(errorRecord, replaceParentContainsErrorRecordException), context);
                        for (int i = 1; i < 9; i++)
                        {
                            tuple.SetValue(i, funcContext._localsTuple.GetValue(i));
                        }
                        SessionStateScope scope = context.EngineSessionState.NewScope(false);
                        context.EngineSessionState.CurrentScope = scope;
                        scope.LocalsTuple = tuple;
                        FunctionContext context2 = new FunctionContext {
                            _scriptBlock = funcContext._scriptBlock,
                            _sequencePoints = funcContext._sequencePoints,
                            _executionContext = funcContext._executionContext,
                            _boundBreakpoints = funcContext._boundBreakpoints,
                            _outputPipe = funcContext._outputPipe,
                            _breakPoints = funcContext._breakPoints,
                            _localsTuple = tuple
                        };
                        actionArray[index](context2);
                    }
                    catch (TargetInvocationException exception3)
                    {
                        throw exception3.InnerException;
                    }
                    finally
                    {
                        context.EngineSessionState.RemoveScope(context.EngineSessionState.CurrentScope);
                    }
                    return QueryForAction(rte, replaceParentContainsErrorRecordException.Message, context);
                }
                catch (ContinueException)
                {
                    return ActionPreference.SilentlyContinue;
                }
                catch (BreakException)
                {
                    return ActionPreference.Stop;
                }
            }
            return ActionPreference.Stop;
        }

        internal static ActionPreference QueryForAction(RuntimeException rte, string message, ExecutionContext context)
        {
            bool flag;
            ActionPreference preference = context.GetEnumPreference<ActionPreference>(SpecialVariables.ErrorActionPreferenceVarPath, ActionPreference.Continue, out flag);
            if ((preference == ActionPreference.Inquire) && !rte.SuppressPromptInInterpreter)
            {
                return InquireForActionPreference(message, context);
            }
            return preference;
        }

        internal static bool ReportErrorRecord(IScriptExtent extent, RuntimeException rte, ExecutionContext context)
        {
            if (context.ShellFunctionErrorOutputPipe == null)
            {
                return false;
            }
            if (((rte.ErrorRecord.InvocationInfo == null) && (extent != null)) && (extent != PositionUtilities.EmptyExtent))
            {
                rte.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, extent, context));
            }
            PSObject obj2 = PSObject.AsPSObject(new ErrorRecord(rte.ErrorRecord, rte));
            PSNoteProperty member = new PSNoteProperty("writeErrorStream", true);
            obj2.Properties.Add(member);
            context.ShellFunctionErrorOutputPipe.Add(obj2);
            return true;
        }

        internal static void RestoreStoppingPipeline(ExecutionContext context, bool oldIsStopping)
        {
            LocalPipeline currentlyRunningPipeline = (LocalPipeline) context.CurrentRunspace.GetCurrentlyRunningPipeline();
            currentlyRunningPipeline.Stopper.IsStopping = oldIsStopping;
        }

        internal static void SetErrorVariables(IScriptExtent extent, RuntimeException rte, ExecutionContext context, Pipe outputPipe)
        {
            string newValue = null;
            Exception innerException = rte;
            int num = 0;
            while ((innerException != null) && (num++ < 10))
            {
                if (!string.IsNullOrEmpty(innerException.StackTrace))
                {
                    newValue = innerException.StackTrace;
                }
                innerException = innerException.InnerException;
            }
            context.SetVariable(SpecialVariables.StackTraceVarPath, newValue);
            InterpreterError.UpdateExceptionErrorRecordPosition(rte, extent);
            ErrorRecord record = rte.ErrorRecord.WrapException(rte);
            if (!(rte is PipelineStoppedException))
            {
                if (outputPipe != null)
                {
                    outputPipe.AppendVariableList(VariableStreamKind.Error, record);
                }
                context.AppendDollarError(record);
            }
        }

        internal static bool SuspendStoppingPipeline(ExecutionContext context)
        {
            LocalPipeline currentlyRunningPipeline = (LocalPipeline) context.CurrentRunspace.GetCurrentlyRunningPipeline();
            bool isStopping = currentlyRunningPipeline.Stopper.IsStopping;
            currentlyRunningPipeline.Stopper.IsStopping = false;
            return isStopping;
        }

        internal class CatchAll
        {
        }
    }
}

