namespace System.Management.Automation.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    internal class PSEtwLogProvider : LogProvider
    {
        private static readonly string PowerShellEventProviderGuid = "A0C1853B-5C40-4b15-8766-3CF1C58F985A";
        private static System.Diagnostics.Eventing.EventDescriptor _xferEventDescriptor = new System.Diagnostics.Eventing.EventDescriptor(0x1f05, 1, 0x11, 5, 20, 0, 0x4000000000000000L);
        private static EventProvider etwProvider = new EventProvider(new Guid(PowerShellEventProviderGuid));
        private static readonly string LogContextCommandName = EtwLoggingStrings.LogContextCommandName;
        private static readonly string LogContextCommandPath = EtwLoggingStrings.LogContextCommandPath;
        private static readonly string LogContextCommandType = EtwLoggingStrings.LogContextCommandType;
        private static readonly string LogContextEngineVersion = EtwLoggingStrings.LogContextEngineVersion;
        private static readonly string LogContextHostId = EtwLoggingStrings.LogContextHostId;
        private static readonly string LogContextHostName = EtwLoggingStrings.LogContextHostName;
        private static readonly string LogContextHostVersion = EtwLoggingStrings.LogContextHostVersion;
        private static readonly string LogContextPipelineId = EtwLoggingStrings.LogContextPipelineId;
        private static readonly string LogContextRunspaceId = EtwLoggingStrings.LogContextRunspaceId;
        private static readonly string LogContextScriptName = EtwLoggingStrings.LogContextScriptName;
        private static readonly string LogContextSequenceNumber = EtwLoggingStrings.LogContextSequenceNumber;
        private static readonly string LogContextSeverity = EtwLoggingStrings.LogContextSeverity;
        private static readonly string LogContextShellId = EtwLoggingStrings.LogContextShellId;
        private static readonly string LogContextTime = EtwLoggingStrings.LogContextTime;
        private static readonly string LogContextUser = EtwLoggingStrings.LogContextUser;

        private static void AppendAdditionalInfo(StringBuilder sb, Dictionary<string, string> additionalInfo)
        {
            if (additionalInfo != null)
            {
                foreach (KeyValuePair<string, string> pair in additionalInfo)
                {
                    sb.AppendLine(StringUtil.Format("{0} = {1}", pair.Key, pair.Value));
                }
            }
        }

        internal static void AppendException(StringBuilder sb, Exception except)
        {
            sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordMessage, except.Message));
            IContainsErrorRecord record = except as IContainsErrorRecord;
            if (record != null)
            {
                ErrorRecord errorRecord = record.ErrorRecord;
                if (errorRecord != null)
                {
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordId, errorRecord.FullyQualifiedErrorId));
                    ErrorDetails errorDetails = errorRecord.ErrorDetails;
                    if (errorDetails != null)
                    {
                        sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordRecommendedAction, errorDetails.RecommendedAction));
                    }
                }
            }
        }

        private static PSLevel GetPSLevelFromSeverity(string severity)
        {
            switch (severity)
            {
                case "Critical":
                case "Error":
                    return PSLevel.Error;

                case "Warning":
                    return PSLevel.Warning;
            }
            return PSLevel.Informational;
        }

        private static string GetPSLogUserData(ExecutionContext context)
        {
            if (context == null)
            {
                return string.Empty;
            }
            object variableValue = context.GetVariableValue(SpecialVariables.PSLogUserDataPath);
            if (variableValue == null)
            {
                return string.Empty;
            }
            return variableValue.ToString();
        }

        internal bool IsEnabled(PSLevel level, PSKeyword keywords)
        {
            return etwProvider.IsEnabled((byte) level, (long) keywords);
        }

        internal override void LogCommandHealthEvent(LogContext logContext, Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            AppendException(sb, exception);
            this.WriteEvent(PSEventId.Command_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, sb.ToString());
        }

        internal override void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState)
        {
            if (this.IsEnabled(PSLevel.Informational, PSKeyword.Cmdlets | PSKeyword.UseAlwaysAnalytic))
            {
                LogContext logContext = getLogContext();
                StringBuilder builder = new StringBuilder();
                if (logContext.CommandType != null)
                {
                    if (logContext.CommandType.Equals("SCRIPT", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.AppendLine(StringUtil.Format(EtwLoggingStrings.ScriptStateChange, newState.ToString()));
                    }
                    else
                    {
                        builder.AppendLine(StringUtil.Format(EtwLoggingStrings.CommandStateChange, logContext.CommandName, newState.ToString()));
                    }
                }
                PSTask commandStart = PSTask.CommandStart;
                if ((newState == CommandState.Stopped) || (newState == CommandState.Terminated))
                {
                    commandStart = PSTask.CommandStop;
                }
                this.WriteEvent(PSEventId.Command_Lifecycle, PSChannel.Analytic, PSOpcode.Method, commandStart, logContext, builder.ToString());
            }
        }

        private static string LogContextToString(LogContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(LogContextSeverity);
            builder.AppendLine(context.Severity);
            builder.Append(LogContextHostName);
            builder.AppendLine(context.HostName);
            builder.Append(LogContextHostVersion);
            builder.AppendLine(context.HostVersion);
            builder.Append(LogContextHostId);
            builder.AppendLine(context.HostId);
            builder.Append(LogContextEngineVersion);
            builder.AppendLine(context.EngineVersion);
            builder.Append(LogContextRunspaceId);
            builder.AppendLine(context.RunspaceId);
            builder.Append(LogContextPipelineId);
            builder.AppendLine(context.PipelineId);
            builder.Append(LogContextCommandName);
            builder.AppendLine(context.CommandName);
            builder.Append(LogContextCommandType);
            builder.AppendLine(context.CommandType);
            builder.Append(LogContextScriptName);
            builder.AppendLine(context.ScriptName);
            builder.Append(LogContextCommandPath);
            builder.AppendLine(context.CommandPath);
            builder.Append(LogContextSequenceNumber);
            builder.AppendLine(context.SequenceNumber);
            builder.Append(LogContextUser);
            builder.AppendLine(context.User);
            builder.Append(LogContextShellId);
            builder.AppendLine(context.ShellId);
            return builder.ToString();
        }

        internal override void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        {
            StringBuilder sb = new StringBuilder();
            AppendException(sb, exception);
            sb.AppendLine();
            AppendAdditionalInfo(sb, additionalInfo);
            this.WriteEvent(PSEventId.Engine_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, sb.ToString());
        }

        internal override void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
        {
            if (this.IsEnabled(PSLevel.Informational, PSKeyword.Cmdlets | PSKeyword.UseAlwaysAnalytic))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(StringUtil.Format(EtwLoggingStrings.EngineStateChange, previousState.ToString(), newState.ToString()));
                PSTask engineStart = PSTask.EngineStart;
                if (((newState == EngineState.Stopped) || (newState == EngineState.OutOfService)) || ((newState == EngineState.None) || (newState == EngineState.Degraded)))
                {
                    engineStart = PSTask.EngineStop;
                }
                this.WriteEvent(PSEventId.Engine_Lifecycle, PSChannel.Analytic, PSOpcode.Method, engineStart, logContext, builder.ToString());
            }
        }

        internal override void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
        {
            StringBuilder builder = new StringBuilder();
            if (pipelineExecutionDetail != null)
            {
                foreach (string str in pipelineExecutionDetail)
                {
                    builder.AppendLine(str);
                }
            }
            this.WriteEvent(PSEventId.Pipeline_Detail, PSChannel.Operational, PSOpcode.Method, PSTask.ExecutePipeline, logContext, builder.ToString());
        }

        internal override void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            AppendException(sb, exception);
            sb.AppendLine();
            Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
            additionalInfo.Add(EtwLoggingStrings.ProviderNameString, providerName);
            AppendAdditionalInfo(sb, additionalInfo);
            this.WriteEvent(PSEventId.Provider_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, sb.ToString());
        }

        internal override void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
        {
            if (this.IsEnabled(PSLevel.Informational, PSKeyword.Cmdlets | PSKeyword.UseAlwaysAnalytic))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(StringUtil.Format(EtwLoggingStrings.ProviderStateChange, providerName, newState.ToString()));
                PSTask providerStart = PSTask.ProviderStart;
                if (newState == ProviderState.Stopped)
                {
                    providerStart = PSTask.ProviderStop;
                }
                this.WriteEvent(PSEventId.Provider_Lifecycle, PSChannel.Analytic, PSOpcode.Method, providerStart, logContext, builder.ToString());
            }
        }

        internal override void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
        {
            if (this.IsEnabled(PSLevel.Informational, PSKeyword.Cmdlets | PSKeyword.UseAlwaysAnalytic))
            {
                StringBuilder builder = new StringBuilder();
                if (previousValue == null)
                {
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.SettingChangeNoPrevious, variableName, value));
                }
                else
                {
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.SettingChange, new object[] { variableName, previousValue, value }));
                }
                this.WriteEvent(PSEventId.Settings, PSChannel.Analytic, PSOpcode.Method, PSTask.ExecutePipeline, logContext, builder.ToString());
            }
        }

        internal void SetActivityIdForCurrentThread(Guid newActivityId)
        {
            Guid id = newActivityId;
            EventProvider.SetActivityId(ref id);
        }

        internal override bool UseLoggingVariables()
        {
            return false;
        }

        internal void WriteEvent(PSEventId id, PSChannel channel, PSOpcode opcode, PSTask task, LogContext logContext, string payLoad)
        {
            this.WriteEvent(id, channel, opcode, GetPSLevelFromSeverity(logContext.Severity), task, (PSKeyword) 0L, new object[] { LogContextToString(logContext), GetPSLogUserData(logContext.ExecutionContext), payLoad });
        }

        internal void WriteEvent(PSEventId id, PSChannel channel, PSOpcode opcode, PSLevel level, PSTask task, PSKeyword keyword, params object[] args)
        {
            long keywords = 0L;
            if ((keyword == PSKeyword.UseAlwaysAnalytic) || (keyword == PSKeyword.UseAlwaysOperational))
            {
                keywords = 0L;
            }
            else
            {
                keywords = (long) keyword;
            }
            System.Diagnostics.Eventing.EventDescriptor eventDescriptor = new System.Diagnostics.Eventing.EventDescriptor((int) id, 1, (byte) channel, (byte) level, (byte) opcode, (int) task, keywords);
            etwProvider.WriteEvent(ref eventDescriptor, args);
        }

        internal void WriteTransferEvent(Guid parentActivityId)
        {
            etwProvider.WriteTransferEvent(ref _xferEventDescriptor, parentActivityId, new object[] { EtwActivity.GetActivityId(), parentActivityId });
        }
    }
}

