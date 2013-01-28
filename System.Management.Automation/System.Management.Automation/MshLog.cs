namespace System.Management.Automation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Security;
    using System.Threading;

    internal static class MshLog
    {
        private const string _crimsonLogProviderAssemblyName = "MshCrimsonLog";
        private const string _crimsonLogProviderTypeName = "System.Management.Automation.Logging.CrimsonLogProvider";
        private static ConcurrentDictionary<string, Collection<LogProvider>> _logProviders = new ConcurrentDictionary<string, Collection<LogProvider>>();
        private static int _nextSequenceNumber = 0;
        internal const int EVENT_ID_CONFIGURATION_FAILURE = 0x67;
        internal const int EVENT_ID_GENERAL_HEALTH_ISSUE = 100;
        internal const int EVENT_ID_NETWORK_CONNECTIVITY_ISSUE = 0x66;
        internal const int EVENT_ID_PERFORMANCE_ISSUE = 0x68;
        internal const int EVENT_ID_RESOURCE_NOT_AVAILABLE = 0x65;
        internal const int EVENT_ID_SECURITY_ISSUE = 0x69;
        internal const int EVENT_ID_SYSTEM_OVERLOADED = 0x6a;
        internal const int EVENT_ID_UNEXPECTED_EXCEPTION = 0xc3;
        private static Collection<string> ignoredCommands = new Collection<string>();

        static MshLog()
        {
            ignoredCommands.Add("Out-Lineoutput");
            ignoredCommands.Add("Format-Default");
        }

        private static Collection<LogProvider> CreateLogProvider(string shellId)
        {
            Collection<LogProvider> collection = new Collection<LogProvider>();
            try
            {
                LogProvider item = new EventLogLogProvider(shellId);
                collection.Add(item);
                item = new PSEtwLogProvider();
                collection.Add(item);
                return collection;
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (SecurityException)
            {
            }
            collection.Add(new DummyLogProvider());
            return collection;
        }

        private static EngineState GetEngineState(System.Management.Automation.ExecutionContext executionContext)
        {
            return executionContext.EngineState;
        }

        internal static LogContext GetLogContext(System.Management.Automation.ExecutionContext executionContext, InvocationInfo invocationInfo)
        {
            return GetLogContext(executionContext, invocationInfo, Severity.Informational);
        }

        private static LogContext GetLogContext(System.Management.Automation.ExecutionContext executionContext, InvocationInfo invocationInfo, Severity severity)
        {
            if (executionContext == null)
            {
                return null;
            }
            LogContext context = new LogContext();
            string shellID = executionContext.ShellID;
            context.ExecutionContext = executionContext;
            context.ShellId = shellID;
            context.Severity = severity.ToString();
            if (executionContext.EngineHostInterface != null)
            {
                context.HostName = executionContext.EngineHostInterface.Name;
                context.HostVersion = executionContext.EngineHostInterface.Version.ToString();
                context.HostId = executionContext.EngineHostInterface.InstanceId.ToString();
            }
            if (executionContext.CurrentRunspace != null)
            {
                context.EngineVersion = executionContext.CurrentRunspace.Version.ToString();
                context.RunspaceId = executionContext.CurrentRunspace.InstanceId.ToString();
                Pipeline currentlyRunningPipeline = ((RunspaceBase) executionContext.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline != null)
                {
                    context.PipelineId = currentlyRunningPipeline.InstanceId.ToString(CultureInfo.CurrentCulture);
                }
            }
            context.SequenceNumber = NextSequenceNumber;
            try
            {
                if (executionContext.LogContextCache.User == null)
                {
                    context.User = Environment.UserDomainName + @"\" + Environment.UserName;
                    executionContext.LogContextCache.User = context.User;
                }
                else
                {
                    context.User = executionContext.LogContextCache.User;
                }
            }
            catch (InvalidOperationException)
            {
                context.User = Logging.UnknownUserName;
            }
            context.Time = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            if (invocationInfo != null)
            {
                context.ScriptName = invocationInfo.ScriptName;
                context.CommandLine = invocationInfo.Line;
                if (invocationInfo.MyCommand == null)
                {
                    return context;
                }
                context.CommandName = invocationInfo.MyCommand.Name;
                context.CommandType = invocationInfo.MyCommand.CommandType.ToString();
                CommandTypes commandType = invocationInfo.MyCommand.CommandType;
                if (commandType != CommandTypes.ExternalScript)
                {
                    if (commandType == CommandTypes.Application)
                    {
                        context.CommandPath = ((ApplicationInfo) invocationInfo.MyCommand).Path;
                    }
                    return context;
                }
                context.CommandPath = ((ExternalScriptInfo) invocationInfo.MyCommand).Path;
            }
            return context;
        }

        private static IEnumerable<LogProvider> GetLogProvider(System.Management.Automation.ExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("executionContext");
            }
            return GetLogProvider(executionContext.ShellID);
        }

        private static IEnumerable<LogProvider> GetLogProvider(LogContext logContext)
        {
            return GetLogProvider(logContext.ShellId);
        }

        private static IEnumerable<LogProvider> GetLogProvider(string shellId)
        {
            return _logProviders.GetOrAdd(shellId, new Func<string, Collection<LogProvider>>(MshLog.CreateLogProvider));
        }

        internal static void LogCommandHealthEvent(System.Management.Automation.ExecutionContext executionContext, Exception exception, Severity severity)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else if (exception == null)
            {
                PSTraceSource.NewArgumentNullException("exception");
            }
            else
            {
                InvocationInfo invocationInfo = null;
                IContainsErrorRecord record = exception as IContainsErrorRecord;
                if ((record != null) && (record.ErrorRecord != null))
                {
                    invocationInfo = record.ErrorRecord.InvocationInfo;
                }
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogCommandHealthEvent(provider, executionContext))
                    {
                        provider.LogCommandHealthEvent(GetLogContext(executionContext, invocationInfo, severity), exception);
                    }
                }
            }
        }

        internal static void LogCommandLifecycleEvent(System.Management.Automation.ExecutionContext executionContext, CommandState commandState, InvocationInfo invocationInfo)
        {
            Func<LogContext> getLogContext = null;
            LogContext logContext;
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else if (invocationInfo == null)
            {
                PSTraceSource.NewArgumentNullException("invocationInfo");
            }
            else if (!ignoredCommands.Contains(invocationInfo.MyCommand.Name))
            {
                logContext = null;
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogCommandLifecycleEvent(provider, executionContext))
                    {
                        if (getLogContext == null)
                        {
                            getLogContext = () => logContext ?? (logContext = GetLogContext(executionContext, invocationInfo));
                        }
                        provider.LogCommandLifecycleEvent(getLogContext, commandState);
                    }
                }
            }
        }

        internal static void LogCommandLifecycleEvent(System.Management.Automation.ExecutionContext executionContext, CommandState commandState, string commandName)
        {
            Func<LogContext> getLogContext = null;
            LogContext logContext;
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                logContext = null;
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogCommandLifecycleEvent(provider, executionContext))
                    {
                        if (getLogContext == null)
                        {
                            getLogContext = delegate {
                                if (logContext == null)
                                {
                                    logContext = GetLogContext(executionContext, null);
                                    logContext.CommandName = commandName;
                                }
                                return logContext;
                            };
                        }
                        provider.LogCommandLifecycleEvent(getLogContext, commandState);
                    }
                }
            }
        }

        internal static void LogEngineHealthEvent(System.Management.Automation.ExecutionContext executionContext, Exception exception, Severity severity)
        {
            LogEngineHealthEvent(executionContext, 100, exception, severity, (Dictionary<string, string>) null);
        }

        internal static void LogEngineHealthEvent(System.Management.Automation.ExecutionContext executionContext, int eventId, Exception exception, Severity severity)
        {
            LogEngineHealthEvent(executionContext, eventId, exception, severity, (Dictionary<string, string>) null);
        }

        internal static void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        {
            if (logContext == null)
            {
                PSTraceSource.NewArgumentNullException("logContext");
            }
            else if (exception == null)
            {
                PSTraceSource.NewArgumentNullException("exception");
            }
            else
            {
                foreach (LogProvider provider in GetLogProvider(logContext))
                {
                    provider.LogEngineHealthEvent(logContext, eventId, exception, additionalInfo);
                }
            }
        }

        internal static void LogEngineHealthEvent(System.Management.Automation.ExecutionContext executionContext, int eventId, Exception exception, Severity severity, Dictionary<string, string> additionalInfo)
        {
            LogEngineHealthEvent(executionContext, eventId, exception, severity, additionalInfo, EngineState.None);
        }

        internal static void LogEngineHealthEvent(System.Management.Automation.ExecutionContext executionContext, int eventId, Exception exception, Severity severity, EngineState newEngineState)
        {
            LogEngineHealthEvent(executionContext, eventId, exception, severity, null, newEngineState);
        }

        internal static void LogEngineHealthEvent(System.Management.Automation.ExecutionContext executionContext, int eventId, Exception exception, Severity severity, Dictionary<string, string> additionalInfo, EngineState newEngineState)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else if (exception == null)
            {
                PSTraceSource.NewArgumentNullException("exception");
            }
            else
            {
                InvocationInfo invocationInfo = null;
                IContainsErrorRecord record = exception as IContainsErrorRecord;
                if ((record != null) && (record.ErrorRecord != null))
                {
                    invocationInfo = record.ErrorRecord.InvocationInfo;
                }
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogEngineHealthEvent(provider, executionContext))
                    {
                        provider.LogEngineHealthEvent(GetLogContext(executionContext, invocationInfo, severity), eventId, exception, additionalInfo);
                    }
                }
                if (newEngineState != EngineState.None)
                {
                    LogEngineLifecycleEvent(executionContext, newEngineState, invocationInfo);
                }
            }
        }

        internal static void LogEngineLifecycleEvent(System.Management.Automation.ExecutionContext executionContext, EngineState engineState)
        {
            LogEngineLifecycleEvent(executionContext, engineState, null);
        }

        internal static void LogEngineLifecycleEvent(System.Management.Automation.ExecutionContext executionContext, EngineState engineState, InvocationInfo invocationInfo)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                EngineState previousState = GetEngineState(executionContext);
                if (engineState != previousState)
                {
                    foreach (LogProvider provider in GetLogProvider(executionContext))
                    {
                        if (NeedToLogEngineLifecycleEvent(provider, executionContext))
                        {
                            provider.LogEngineLifecycleEvent(GetLogContext(executionContext, invocationInfo), engineState, previousState);
                        }
                    }
                    SetEngineState(executionContext, engineState);
                }
            }
        }

        internal static void LogPipelineExecutionDetailEvent(System.Management.Automation.ExecutionContext executionContext, List<string> detail, InvocationInfo invocationInfo)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogPipelineExecutionDetailEvent(provider, executionContext))
                    {
                        provider.LogPipelineExecutionDetailEvent(GetLogContext(executionContext, invocationInfo), detail);
                    }
                }
            }
        }

        internal static void LogPipelineExecutionDetailEvent(System.Management.Automation.ExecutionContext executionContext, List<string> detail, string scriptName, string commandLine)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                LogContext logContext = GetLogContext(executionContext, null);
                logContext.CommandLine = commandLine;
                logContext.ScriptName = scriptName;
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogPipelineExecutionDetailEvent(provider, executionContext))
                    {
                        provider.LogPipelineExecutionDetailEvent(logContext, detail);
                    }
                }
            }
        }

        internal static void LogProviderHealthEvent(System.Management.Automation.ExecutionContext executionContext, string providerName, Exception exception, Severity severity)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else if (exception == null)
            {
                PSTraceSource.NewArgumentNullException("exception");
            }
            else
            {
                InvocationInfo invocationInfo = null;
                IContainsErrorRecord record = exception as IContainsErrorRecord;
                if ((record != null) && (record.ErrorRecord != null))
                {
                    invocationInfo = record.ErrorRecord.InvocationInfo;
                }
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogProviderHealthEvent(provider, executionContext))
                    {
                        provider.LogProviderHealthEvent(GetLogContext(executionContext, invocationInfo, severity), providerName, exception);
                    }
                }
            }
        }

        internal static void LogProviderLifecycleEvent(System.Management.Automation.ExecutionContext executionContext, string providerName, ProviderState providerState)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogProviderLifecycleEvent(provider, executionContext))
                    {
                        provider.LogProviderLifecycleEvent(GetLogContext(executionContext, null), providerName, providerState);
                    }
                }
            }
        }

        internal static void LogSettingsEvent(System.Management.Automation.ExecutionContext executionContext, string variableName, string newValue, string previousValue)
        {
            LogSettingsEvent(executionContext, variableName, newValue, previousValue, null);
        }

        internal static void LogSettingsEvent(System.Management.Automation.ExecutionContext executionContext, string variableName, string newValue, string previousValue, InvocationInfo invocationInfo)
        {
            if (executionContext == null)
            {
                PSTraceSource.NewArgumentNullException("executionContext");
            }
            else
            {
                foreach (LogProvider provider in GetLogProvider(executionContext))
                {
                    if (NeedToLogSettingsEvent(provider, executionContext))
                    {
                        provider.LogSettingsEvent(GetLogContext(executionContext, invocationInfo), variableName, newValue, previousValue);
                    }
                }
            }
        }

        private static bool NeedToLogCommandHealthEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogCommandHealthEventVarPath, false)));
        }

        private static bool NeedToLogCommandLifecycleEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogCommandLifecycleEventVarPath, false)));
        }

        private static bool NeedToLogEngineHealthEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogEngineHealthEventVarPath, true)));
        }

        private static bool NeedToLogEngineLifecycleEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogEngineLifecycleEventVarPath, true)));
        }

        private static bool NeedToLogPipelineExecutionDetailEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || true);
        }

        private static bool NeedToLogProviderHealthEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogProviderHealthEventVarPath, true)));
        }

        private static bool NeedToLogProviderLifecycleEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogProviderLifecycleEventVarPath, true)));
        }

        private static bool NeedToLogSettingsEvent(LogProvider logProvider, System.Management.Automation.ExecutionContext executionContext)
        {
            return (!logProvider.UseLoggingVariables() || LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogSettingsEventVarPath, true)));
        }

        internal static void SetDummyLog(string shellId)
        {
            Collection<LogProvider> providers = new Collection<LogProvider> {
                new DummyLogProvider()
            };
            _logProviders.AddOrUpdate(shellId, providers, (key, value) => providers);
        }

        private static void SetEngineState(System.Management.Automation.ExecutionContext executionContext, EngineState engineState)
        {
            executionContext.EngineState = engineState;
        }

        private static string NextSequenceNumber
        {
            get
            {
                return Convert.ToString(Interlocked.Increment(ref _nextSequenceNumber), CultureInfo.CurrentCulture);
            }
        }
    }
}

