namespace System.Management.Automation.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal static class PSEtwLog
    {
        private static PSEtwLogProvider provider = new PSEtwLogProvider();

        internal static void LogAnalyticError(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Error, task, keyword, args);
        }

        internal static void LogAnalyticInformational(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Informational, task, keyword, args);
        }

        internal static void LogAnalyticVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Verbose, task, keyword, args);
        }

        internal static void LogAnalyticVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, long objectId, long fragmentId, int isStartFragment, int isEndFragment, int fragmentLength, PSETWBinaryBlob fragmentData)
        {
            if (provider.IsEnabled(PSLevel.Verbose, keyword))
            {
                string str = BitConverter.ToString(fragmentData.blob, fragmentData.offset, fragmentData.length);
                str = string.Format(CultureInfo.InvariantCulture, "0x{0}", new object[] { str.Replace("-", "") });
                provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Verbose, task, keyword, new object[] { objectId, fragmentId, isStartFragment, isEndFragment, fragmentLength, str });
            }
        }

        internal static void LogAnalyticWarning(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Warning, task, keyword, args);
        }

        internal static void LogCommandHealthEvent(LogContext logContext, Exception exception)
        {
            provider.LogCommandHealthEvent(logContext, exception);
        }

        internal static void LogCommandLifecycleEvent(LogContext logContext, CommandState newState)
        {
            provider.LogCommandLifecycleEvent(() => logContext, newState);
        }

        internal static void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        {
            provider.LogEngineHealthEvent(logContext, eventId, exception, additionalInfo);
        }

        internal static void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
        {
            provider.LogEngineLifecycleEvent(logContext, newState, previousState);
        }

        internal static void LogOperationalError(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Error, task, keyword, args);
        }

        internal static void LogOperationalError(PSEventId id, PSOpcode opcode, PSTask task, LogContext logContext, string payLoad)
        {
            provider.WriteEvent(id, PSChannel.Operational, opcode, task, logContext, payLoad);
        }

        internal static void LogOperationalInformation(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Informational, task, keyword, args);
        }

        internal static void LogOperationalVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
        {
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Verbose, task, keyword, args);
        }

        internal static void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
        {
            provider.LogPipelineExecutionDetailEvent(logContext, pipelineExecutionDetail);
        }

        internal static void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
        {
            provider.LogProviderHealthEvent(logContext, providerName, exception);
        }

        internal static void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
        {
            provider.LogProviderLifecycleEvent(logContext, providerName, newState);
        }

        internal static void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
        {
            provider.LogSettingsEvent(logContext, variableName, value, previousValue);
        }

        internal static void ReplaceActivityIdForCurrentThread(Guid newActivityId, PSEventId eventForOperationalChannel, PSEventId eventForAnalyticChannel, PSKeyword keyword, PSTask task)
        {
            provider.SetActivityIdForCurrentThread(newActivityId);
            WriteTransferEvent(newActivityId, eventForOperationalChannel, eventForAnalyticChannel, keyword, task);
        }

        internal static void SetActivityIdForCurrentThread(Guid newActivityId)
        {
            provider.SetActivityIdForCurrentThread(newActivityId);
        }

        internal static void WriteTransferEvent(Guid parentActivityId)
        {
            provider.WriteTransferEvent(parentActivityId);
        }

        internal static void WriteTransferEvent(Guid relatedActivityId, PSEventId eventForOperationalChannel, PSEventId eventForAnalyticChannel, PSKeyword keyword, PSTask task)
        {
            provider.WriteEvent(eventForOperationalChannel, PSChannel.Operational, PSOpcode.Method, PSLevel.Informational, task, PSKeyword.UseAlwaysOperational, new object[0]);
            provider.WriteEvent(eventForAnalyticChannel, PSChannel.Analytic, PSOpcode.Method, PSLevel.Informational, task, PSKeyword.UseAlwaysAnalytic, new object[0]);
        }
    }
}

