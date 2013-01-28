namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal static class PipelineOps
    {
        private static CommandProcessorBase AddCommand(PipelineProcessor pipe, CommandParameterInternal[] commandElements, CommandBaseAst commandBaseAst, CommandRedirection[] redirections, System.Management.Automation.ExecutionContext context)
        {
            object parameterText;
            IScriptExtent parameterExtent;
            CommandProcessorBase base2;
            InternalCommand command;
            string str3;
            HelpCategory category;
            CommandAst ast = commandBaseAst as CommandAst;
            TokenKind kind = (ast != null) ? ast.InvocationOperator : TokenKind.Unknown;
            bool dotSource = kind == TokenKind.Dot;
            SessionStateInternal sessionState = null;
            int index = 0;
            PSModuleInfo info = PSObject.Base(commandElements[0].ArgumentValue) as PSModuleInfo;
            if (info != null)
            {
                if ((info.ModuleType == ModuleType.Binary) && (info.SessionState == null))
                {
                    throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "CantInvokeInBinaryModule", ParserStrings.CantInvokeInBinaryModule, new object[] { info.Name });
                }
                if (info.SessionState == null)
                {
                    throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "CantInvokeInNonImportedModule", ParserStrings.CantInvokeInNonImportedModule, new object[] { info.Name });
                }
                sessionState = info.SessionState.Internal;
                index++;
            }
            CommandParameterInternal internal3 = commandElements[index];
            if (internal3.ParameterNameSpecified)
            {
                parameterText = internal3.ParameterText;
                parameterExtent = internal3.ParameterExtent;
                if (!internal3.ArgumentSpecified)
                {
                }
            }
            else
            {
                parameterText = PSObject.Base(internal3.ArgumentValue);
                parameterExtent = internal3.ArgumentExtent;
            }
            string str = dotSource ? "." : ((kind == TokenKind.Ampersand) ? "&" : null);
            ScriptBlock scriptblock = parameterText as ScriptBlock;
            if (scriptblock != null)
            {
                base2 = CommandDiscovery.CreateCommandProcessorForScript(scriptblock, context, !dotSource, sessionState);
            }
            else
            {
                CommandInfo commandInfo = parameterText as CommandInfo;
                if (commandInfo != null)
                {
                    base2 = context.CommandDiscovery.LookupCommandProcessor(commandInfo, context.EngineSessionState.CurrentScope.ScopeOrigin, new bool?(!dotSource), sessionState);
                }
                else
                {
                    string str2 = (parameterText as string) ?? PSObject.ToStringParser(context, parameterText);
                    str = str ?? str2;
                    if (string.IsNullOrEmpty(str2))
                    {
                        throw InterpreterError.NewInterpreterException(parameterText, typeof(RuntimeException), parameterExtent, "BadExpression", ParserStrings.BadExpression, new object[] { dotSource ? "." : "&" });
                    }
                    try
                    {
                        if (sessionState != null)
                        {
                            SessionStateInternal engineSessionState = context.EngineSessionState;
                            try
                            {
                                context.EngineSessionState = sessionState;
                                base2 = context.CreateCommand(str2, dotSource);
                                goto Label_025D;
                            }
                            finally
                            {
                                context.EngineSessionState = engineSessionState;
                            }
                        }
                        base2 = context.CreateCommand(str2, dotSource);
                    }

                    catch (RuntimeException exception)
                    {
                        if (exception.ErrorRecord.InvocationInfo == null)
                        {
                            InvocationInfo invocationInfo = new InvocationInfo(null, parameterExtent, context) {
                                InvocationName = str
                            };
                            exception.ErrorRecord.SetInvocationInfo(invocationInfo);
                        }
                        throw;
                    }
                }
            }
        Label_025D:
            command = base2.Command;
            base2.UseLocalScope = !dotSource && ((command is ScriptCommand) || (command is PSScriptCmdlet));
            bool flag2 = base2 is NativeCommandProcessor;
            for (int i = index + 1; i < commandElements.Length; i++)
            {
                CommandParameterInternal parameter = commandElements[i];
                if ((!parameter.ParameterNameSpecified || !parameter.ParameterName.Equals("-", StringComparison.OrdinalIgnoreCase)) || flag2)
                {
                    if (parameter.ArgumentSplatted)
                    {
                        foreach (CommandParameterInternal internal6 in Splat(parameter.ArgumentValue, parameter.ArgumentExtent))
                        {
                            base2.AddParameter(internal6);
                        }
                    }
                    else
                    {
                        base2.AddParameter(parameter);
                    }
                }
            }
            if (base2.IsHelpRequested(out str3, out category))
            {
                base2 = CommandProcessorBase.CreateGetHelpCommandProcessor(context, str3, category);
            }
            base2.Command.InvocationExtent = commandBaseAst.Extent;
            base2.Command.MyInvocation.ScriptPosition = commandBaseAst.Extent;
            base2.Command.MyInvocation.InvocationName = str;
            pipe.Add(base2);
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            if (redirections != null)
            {
                foreach (CommandRedirection redirection in redirections)
                {
                    redirection.Bind(pipe, base2, context);
                    switch (redirection.FromStream)
                    {
                        case RedirectionStream.All:
                            flag3 = true;
                            flag4 = true;
                            flag5 = true;
                            flag6 = true;
                            break;

                        case RedirectionStream.Error:
                            flag3 = true;
                            break;

                        case RedirectionStream.Warning:
                            flag4 = true;
                            break;

                        case RedirectionStream.Verbose:
                            flag5 = true;
                            break;

                        case RedirectionStream.Debug:
                            flag6 = true;
                            break;
                    }
                }
            }
            if (!flag3)
            {
                if (context.ShellFunctionErrorOutputPipe != null)
                {
                    base2.CommandRuntime.ErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
                }
                else
                {
                    base2.CommandRuntime.ErrorOutputPipe.ExternalWriter = context.ExternalErrorOutput;
                }
            }
            if (!flag4 && (context.ExpressionWarningOutputPipe != null))
            {
                base2.CommandRuntime.WarningOutputPipe = context.ExpressionWarningOutputPipe;
                flag4 = true;
            }
            if (!flag5 && (context.ExpressionVerboseOutputPipe != null))
            {
                base2.CommandRuntime.VerboseOutputPipe = context.ExpressionVerboseOutputPipe;
                flag5 = true;
            }
            if (!flag6 && (context.ExpressionDebugOutputPipe != null))
            {
                base2.CommandRuntime.DebugOutputPipe = context.ExpressionDebugOutputPipe;
                flag6 = true;
            }
            if ((context.CurrentCommandProcessor != null) && (context.CurrentCommandProcessor.CommandRuntime != null))
            {
                if (!flag4 && (context.CurrentCommandProcessor.CommandRuntime.WarningOutputPipe != null))
                {
                    base2.CommandRuntime.WarningOutputPipe = context.CurrentCommandProcessor.CommandRuntime.WarningOutputPipe;
                }
                if (!flag5 && (context.CurrentCommandProcessor.CommandRuntime.VerboseOutputPipe != null))
                {
                    base2.CommandRuntime.VerboseOutputPipe = context.CurrentCommandProcessor.CommandRuntime.VerboseOutputPipe;
                }
                if (!flag6 && (context.CurrentCommandProcessor.CommandRuntime.DebugOutputPipe != null))
                {
                    base2.CommandRuntime.DebugOutputPipe = context.CurrentCommandProcessor.CommandRuntime.DebugOutputPipe;
                }
            }
            return base2;
        }

        private static void AddNoopCommandProcessor(PipelineProcessor pipelineProcessor, System.Management.Automation.ExecutionContext context)
        {
            CmdletInfo commandInfo = new CmdletInfo("Out-Null", typeof(OutNullCommand));
            CommandProcessorBase commandProcessor = context.CommandDiscovery.LookupCommandProcessor(commandInfo, context.EngineSessionState.CurrentScope.ScopeOrigin, false, null);
            pipelineProcessor.Add(commandProcessor);
        }

        internal static object CheckAutomationNullInCommandArgument(object obj)
        {
            if (obj == AutomationNull.Value)
            {
                return null;
            }
            object[] objArray = obj as object[];
            if (objArray == null)
            {
                return obj;
            }
            return CheckAutomationNullInCommandArgumentArray(objArray);
        }

        internal static object[] CheckAutomationNullInCommandArgumentArray(object[] objArray)
        {
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i] == AutomationNull.Value)
                {
                    objArray[i] = null;
                }
            }
            return objArray;
        }

        internal static void CheckForInterrupts(System.Management.Automation.ExecutionContext context)
        {
            if (context.Events != null)
            {
                context.Events.ProcessPendingActions();
            }
            if (context.CurrentPipelineStopping)
            {
                throw new PipelineStoppedException();
            }
        }

        internal static void FlushPipe(Pipe oldPipe, ArrayList arrayList)
        {
            foreach (object obj2 in arrayList)
            {
                oldPipe.Add(obj2);
            }
        }

        private static CommandParameterInternal GetCommandParameter(CommandParameterAst commandParameterAst, System.Management.Automation.ExecutionContext context)
        {
            ExpressionAst argument = commandParameterAst.Argument;
            IScriptExtent errorPosition = commandParameterAst.ErrorPosition;
            if (argument == null)
            {
                return CommandParameterInternal.CreateParameter(errorPosition, commandParameterAst.ParameterName, errorPosition.Text);
            }
            object obj2 = Compiler.GetExpressionValue(argument, context, (IList)null);
            bool spaceAfterParameter = (errorPosition.EndLineNumber != argument.Extent.StartLineNumber) || (errorPosition.EndColumnNumber != argument.Extent.StartColumnNumber);
            return CommandParameterInternal.CreateParameterWithArgument(errorPosition, commandParameterAst.ParameterName, errorPosition.Text, argument.Extent, obj2, spaceAfterParameter);
        }

        private static CommandRedirection GetCommandRedirection(RedirectionAst redirectionAst, System.Management.Automation.ExecutionContext context)
        {
            FileRedirectionAst ast = redirectionAst as FileRedirectionAst;
            if (ast != null)
            {
                object obj2 = Compiler.GetExpressionValue(ast.Location, context, (IList)null);
                return new FileRedirection(ast.FromStream, ast.Append, obj2.ToString());
            }
            MergingRedirectionAst ast2 = (MergingRedirectionAst) redirectionAst;
            return new MergingRedirection(ast2.FromStream, ast2.ToStream);
        }

        internal static ExitException GetExitException(object exitCodeObj)
        {
            int argument = 0;
            try
            {
                if (!LanguagePrimitives.IsNull(exitCodeObj))
                {
                    argument = ParserOps.ConvertTo<int>(exitCodeObj, PositionUtilities.EmptyExtent);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return new ExitException(argument);
        }

        private static string GetParameterText(string parameterName)
        {
            int length = parameterName.Length;
            while ((length > 0) && char.IsWhiteSpace(parameterName[length - 1]))
            {
                length--;
            }
            if ((length == 0) || (parameterName[length - 1] == ':'))
            {
                return ("-" + parameterName);
            }
            if (length == parameterName.Length)
            {
                return ("-" + parameterName + ":");
            }
            string str2 = parameterName.Substring(length);
            return ("-" + parameterName.Substring(0, length) + ":" + str2);
        }

        internal static SteppablePipeline GetSteppablePipeline(PipelineAst pipelineAst, CommandOrigin commandOrigin)
        {
            PipelineProcessor pipe = new PipelineProcessor();
            System.Management.Automation.ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            foreach (CommandAst ast in pipelineAst.PipelineElements.Cast<CommandAst>())
            {
                List<CommandParameterInternal> list = new List<CommandParameterInternal>();
                foreach (CommandElementAst ast2 in ast.CommandElements)
                {
                    CommandParameterAst commandParameterAst = ast2 as CommandParameterAst;
                    if (commandParameterAst != null)
                    {
                        list.Add(GetCommandParameter(commandParameterAst, executionContextFromTLS));
                    }
                    else
                    {
                        ExpressionAst expressionAst = (ExpressionAst) ast2;
                        object obj2 = Compiler.GetExpressionValue(expressionAst, executionContextFromTLS, (IList)null);
                        bool splatted = (expressionAst is VariableExpressionAst) && ((VariableExpressionAst) expressionAst).Splatted;
                        list.Add(CommandParameterInternal.CreateArgument(expressionAst.Extent, obj2, splatted));
                    }
                }
                List<CommandRedirection> list2 = new List<CommandRedirection>();
                foreach (RedirectionAst ast5 in ast.Redirections)
                {
                    list2.Add(GetCommandRedirection(ast5, executionContextFromTLS));
                }
                CommandProcessorBase base2 = AddCommand(pipe, list.ToArray(), ast, list2.ToArray(), executionContextFromTLS);
                base2.Command.CommandOriginInternal = commandOrigin;
                base2.CommandScope.ScopeOrigin = commandOrigin;
                base2.Command.MyInvocation.CommandOrigin = commandOrigin;
                CallStackFrame[] frameArray = executionContextFromTLS.Debugger.GetCallStack().ToArray<CallStackFrame>();
                if ((frameArray.Length > 0) && Regex.IsMatch(frameArray[0].Position.Text, "GetSteppablePipeline", RegexOptions.IgnoreCase))
                {
                    InvocationInfo myInvocation = base2.Command.MyInvocation;
                    myInvocation.InvocationName = frameArray[0].InvocationInfo.InvocationName;
                    if (frameArray.Length > 1)
                    {
                        IScriptExtent position = frameArray[1].Position;
                        if ((position != null) && (position != PositionUtilities.EmptyExtent))
                        {
                            myInvocation.DisplayScriptPosition = position;
                        }
                    }
                }
                if ((executionContextFromTLS.CurrentCommandProcessor != null) && (executionContextFromTLS.CurrentCommandProcessor.CommandRuntime != null))
                {
                    base2.CommandRuntime.SetMergeFromRuntime(executionContextFromTLS.CurrentCommandProcessor.CommandRuntime);
                }
            }
            return new SteppablePipeline(executionContextFromTLS, pipe);
        }

        internal static void InvokePipeline (object input, bool ignoreInput, CommandParameterInternal[][] pipeElements, CommandBaseAst[] pipeElementAsts, CommandRedirection[][] commandRedirections, FunctionContext funcContext)
		{
			PipelineProcessor pipelineProcessor = new PipelineProcessor ();
			System.Management.Automation.ExecutionContext context = funcContext._executionContext;
			Pipe pipe = funcContext._outputPipe;
			try {
				if (context.Events != null) {
					context.Events.ProcessPendingActions ();
				}
				if ((input == AutomationNull.Value) && !ignoreInput) {
					AddNoopCommandProcessor (pipelineProcessor, context);
				}
				CommandProcessorBase commandProcessor = null;
				CommandRedirection[] redirections = null;
				for (int i = 0; i < pipeElements.Length; i++) {
					redirections = (commandRedirections != null) ? commandRedirections [i] : null;
					commandProcessor = AddCommand (pipelineProcessor, pipeElements [i], pipeElementAsts [i], redirections, context);
				}
				if ((commandProcessor != null) && !commandProcessor.CommandRuntime.OutputPipe.IsRedirected) {
					pipelineProcessor.LinkPipelineSuccessOutput (pipe ?? new Pipe (new ArrayList ()));
					if (redirections != null) {
						foreach (CommandRedirection redirection in redirections) {
							if (redirection is MergingRedirection) {
								redirection.Bind (pipelineProcessor, commandProcessor, context);
							}
						}
					}
				}
				context.PushPipelineProcessor (pipelineProcessor);
				try {
					pipelineProcessor.SynchronousExecuteEnumerate (input, null, true);
				} finally {
					context.PopPipelineProcessor (false);
				}
			}
            finally
            {
                context.QuestionMarkVariableValue = !pipelineProcessor.ExecutionFailed;
                pipelineProcessor.Dispose();
            }
        }

        internal static void Nop()
        {
        }

        internal static object PipelineResult(ArrayList arrayList)
        {
            int count = arrayList.Count;
            if (count == 0)
            {
                return AutomationNull.Value;
            }
            object obj2 = (count == 1) ? arrayList[0] : arrayList.ToArray();
            arrayList.Clear();
            return obj2;
        }

        internal static IEnumerable<CommandParameterInternal> Splat(object splattedValue, IScriptExtent splatExtent)
        {
            splattedValue = PSObject.Base(splattedValue);
            IDictionary iteratorVariable0 = splattedValue as IDictionary;
            if (iteratorVariable0 != null)
            {
                IDictionaryEnumerator enumerator = iteratorVariable0.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    string parameterName = current.Key.ToString();
                    object iteratorVariable3 = current.Value;
                    string parameterText = GetParameterText(parameterName);
                    yield return CommandParameterInternal.CreateParameterWithArgument(splatExtent, parameterName, parameterText, splatExtent, iteratorVariable3, false);
                }
            }
            else
            {
                IEnumerable iteratorVariable5 = splattedValue as IEnumerable;
                if (iteratorVariable5 != null)
                {
                    IEnumerator iteratorVariable9 = iteratorVariable5.GetEnumerator();
                    while (iteratorVariable9.MoveNext())
                    {
                        object splattedArgument = iteratorVariable9.Current;
                        yield return SplatEnumerableElement(splattedArgument, splatExtent);
                    }
                }
                else
                {
                    yield return SplatEnumerableElement(splattedValue, splatExtent);
                }
            }
        }

        private static CommandParameterInternal SplatEnumerableElement(object splattedArgument, IScriptExtent splatExtent)
        {
            PSObject obj2 = splattedArgument as PSObject;
            if (obj2 != null)
            {
                PSPropertyInfo info = obj2.Properties["<CommandParameterName>"];
                object baseObject = obj2.BaseObject;
                if (((info != null) && (info.Value is string)) && (baseObject is string))
                {
                    return CommandParameterInternal.CreateParameter(splatExtent, (string) info.Value, (string) baseObject);
                }
            }
            return CommandParameterInternal.CreateArgument(splatExtent, splattedArgument, false);
        }

        
    }
}

