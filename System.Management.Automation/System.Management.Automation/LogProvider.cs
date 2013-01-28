namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal abstract class LogProvider
    {
        internal LogProvider()
        {
        }

        internal abstract void LogCommandHealthEvent(LogContext logContext, Exception exception);
        internal abstract void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState);
        internal abstract void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo);
        internal abstract void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState);
        internal abstract void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail);
        internal abstract void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception);
        internal abstract void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState);
        internal abstract void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue);
        internal virtual bool UseLoggingVariables()
        {
            return true;
        }
    }
}

