namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class DummyLogProvider : LogProvider
    {
        internal DummyLogProvider()
        {
        }

        internal override void LogCommandHealthEvent(LogContext logContext, Exception exception)
        {
        }

        internal override void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState)
        {
        }

        internal override void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        {
        }

        internal override void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
        {
        }

        internal override void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
        {
        }

        internal override void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
        {
        }

        internal override void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
        {
        }

        internal override void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
        {
        }
    }
}

