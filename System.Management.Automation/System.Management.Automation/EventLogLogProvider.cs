namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Threading;

    internal class EventLogLogProvider : LogProvider
    {
        private const int _baseCommandLifecycleEventId = 500;
        private const int _baseEngineLifecycleEventId = 400;
        private const int _baseProviderLifecycleEventId = 600;
        private const int _commandHealthEventId = 200;
        private EventLog _eventLog;
        private const int _invalidEventId = -1;
        private const int _pipelineExecutionDetailEventId = 800;
        private const int _providerHealthEventId = 300;
        private ResourceManager _resourceManager;
        private const int _settingsEventId = 700;
        private const int CommandHealthCategoryId = 2;
        private const int CommandLifecycleCategoryId = 5;
        private const int EngineHealthCategoryId = 1;
        private const int EngineLifecycleCategoryId = 4;
        private const int MaxLength = 0x3e80;
        private const int PipelineExecutionDetailCategoryId = 8;
        private const int ProviderHealthCategoryId = 3;
        private const int ProviderLifecycleCategoryId = 6;
        private const int SettingsCategoryId = 7;

        internal EventLogLogProvider(string shellId)
        {
            string str = this.SetupEventSource(shellId);
            this._eventLog = new EventLog();
            this._eventLog.Source = str;
            this._resourceManager = ResourceManagerCache.GetResourceManager(Assembly.GetExecutingAssembly(), "Logging");
        }

        private static void FillEventArgs(Hashtable mapArgs, Dictionary<string, string> additionalInfo)
        {
            if (additionalInfo == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    string str = (i + 1).ToString("d1", CultureInfo.CurrentCulture);
                    mapArgs["AdditionalInfo_Name" + str] = "";
                    mapArgs["AdditionalInfo_Value" + str] = "";
                }
            }
            else
            {
                string[] array = new string[additionalInfo.Count];
                string[] strArray2 = new string[additionalInfo.Count];
                additionalInfo.Keys.CopyTo(array, 0);
                additionalInfo.Values.CopyTo(strArray2, 0);
                for (int j = 0; j < 3; j++)
                {
                    string str2 = (j + 1).ToString("d1", CultureInfo.CurrentCulture);
                    if (j < array.Length)
                    {
                        mapArgs["AdditionalInfo_Name" + str2] = array[j];
                        mapArgs["AdditionalInfo_Value" + str2] = strArray2[j];
                    }
                    else
                    {
                        mapArgs["AdditionalInfo_Name" + str2] = "";
                        mapArgs["AdditionalInfo_Value" + str2] = "";
                    }
                }
            }
        }

        private static void FillEventArgs(Hashtable mapArgs, LogContext logContext)
        {
            mapArgs["Severity"] = logContext.Severity;
            mapArgs["SequenceNumber"] = logContext.SequenceNumber;
            mapArgs["HostName"] = logContext.HostName;
            mapArgs["HostVersion"] = logContext.HostVersion;
            mapArgs["HostId"] = logContext.HostId;
            mapArgs["EngineVersion"] = logContext.EngineVersion;
            mapArgs["RunspaceId"] = logContext.RunspaceId;
            mapArgs["PipelineId"] = logContext.PipelineId;
            mapArgs["CommandName"] = logContext.CommandName;
            mapArgs["CommandType"] = logContext.CommandType;
            mapArgs["ScriptName"] = logContext.ScriptName;
            mapArgs["CommandPath"] = logContext.CommandPath;
            mapArgs["CommandLine"] = logContext.CommandLine;
            mapArgs["User"] = logContext.User;
            mapArgs["Time"] = logContext.Time;
        }

        private static string FillMessageTemplate(string messageTemplate, Hashtable mapArgs)
        {
            StringBuilder builder = new StringBuilder();
            int startIndex = 0;
            while (true)
            {
                int index = messageTemplate.IndexOf('[', startIndex);
                if (index < 0)
                {
                    builder.Append(messageTemplate.Substring(startIndex));
                    return builder.ToString();
                }
                int num3 = messageTemplate.IndexOf(']', index + 1);
                if (num3 < 0)
                {
                    builder.Append(messageTemplate.Substring(startIndex));
                    return builder.ToString();
                }
                builder.Append(messageTemplate.Substring(startIndex, index - startIndex));
                startIndex = index;
                string key = messageTemplate.Substring(index + 1, (num3 - index) - 1);
                if (mapArgs.Contains(key))
                {
                    builder.Append(mapArgs[key]);
                    startIndex = num3 + 1;
                }
                else
                {
                    builder.Append("[");
                    startIndex++;
                }
            }
        }

        private static int GetCommandLifecycleEventId(CommandState commandState)
        {
            switch (commandState)
            {
                case CommandState.Started:
                    return 500;

                case CommandState.Stopped:
                    return 0x1f5;

                case CommandState.Terminated:
                    return 0x1f6;
            }
            return -1;
        }

        private static int GetEngineLifecycleEventId(EngineState engineState)
        {
            switch (engineState)
            {
                case EngineState.None:
                    return -1;

                case EngineState.Available:
                    return 400;

                case EngineState.Degraded:
                    return 0x191;

                case EngineState.OutOfService:
                    return 0x192;

                case EngineState.Stopped:
                    return 0x193;
            }
            return -1;
        }

        private string GetEventDetail(string contextId, Hashtable mapArgs)
        {
            return this.GetMessage(contextId, mapArgs);
        }

        private static EventLogEntryType GetEventLogEntryType(LogContext logContext)
        {
            switch (logContext.Severity)
            {
                case "Critical":
                case "Error":
                    return EventLogEntryType.Error;

                case "Warning":
                    return EventLogEntryType.Warning;
            }
            return EventLogEntryType.Information;
        }

        private string GetMessage(string messageId, Hashtable mapArgs)
        {
            if (this._resourceManager == null)
            {
                return "";
            }
            string str = this._resourceManager.GetString(messageId);
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return FillMessageTemplate(str, mapArgs);
        }

        private static string GetMessageDllPath(string shellId)
        {
            string directoryName = null;
            if (!string.IsNullOrEmpty(shellId))
            {
                directoryName = Path.GetDirectoryName(CommandDiscovery.GetShellPathFromRegistry(shellId));
            }
            if (string.IsNullOrEmpty(directoryName) && (Assembly.GetEntryAssembly() != null))
            {
                directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            return Path.Combine(directoryName, "pwrshmsg.dll");
        }

        private static int GetProviderLifecycleEventId(ProviderState providerState)
        {
            switch (providerState)
            {
                case ProviderState.Started:
                    return 600;

                case ProviderState.Stopped:
                    return 0x259;
            }
            return -1;
        }

        private List<string> GroupMessages(List<string> messages)
        {
            List<string> list = new List<string>();
            if ((messages != null) && (messages.Count != 0))
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < messages.Count; i++)
                {
                    if ((builder.Length + messages[i].Length) < 0x3e80)
                    {
                        builder.AppendLine(messages[i]);
                    }
                    else
                    {
                        list.Add(builder.ToString());
                        builder = new StringBuilder();
                        builder.AppendLine(messages[i]);
                    }
                }
                list.Add(builder.ToString());
            }
            return list;
        }

        internal override void LogCommandHealthEvent(LogContext logContext, Exception exception)
        {
            int num = 200;
            Hashtable mapArgs = new Hashtable();
            IContainsErrorRecord record = exception as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = record.ErrorRecord.CategoryInfo.Category;
                mapArgs["ErrorId"] = record.ErrorRecord.FullyQualifiedErrorId;
                if (record.ErrorRecord.ErrorDetails != null)
                {
                    mapArgs["ErrorMessage"] = record.ErrorRecord.ErrorDetails.Message;
                }
                else
                {
                    mapArgs["ErrorMessage"] = exception.Message;
                }
            }
            else
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = "";
                mapArgs["ErrorId"] = "";
                mapArgs["ErrorMessage"] = exception.Message;
            }
            FillEventArgs(mapArgs, logContext);
            EventInstance entry = new EventInstance((long) num, 2) {
                EntryType = GetEventLogEntryType(logContext)
            };
            string eventDetail = this.GetEventDetail("CommandHealthContext", mapArgs);
            this.LogEvent(entry, new object[] { mapArgs["ErrorMessage"], eventDetail });
        }

        internal override void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState)
        {
            LogContext logContext = getLogContext();
            int commandLifecycleEventId = GetCommandLifecycleEventId(newState);
            if (commandLifecycleEventId != -1)
            {
                Hashtable mapArgs = new Hashtable();
                mapArgs["NewCommandState"] = newState.ToString();
                FillEventArgs(mapArgs, logContext);
                EventInstance entry = new EventInstance((long) commandLifecycleEventId, 5) {
                    EntryType = EventLogEntryType.Information
                };
                string eventDetail = this.GetEventDetail("CommandLifecycleContext", mapArgs);
                this.LogEvent(entry, new object[] { logContext.CommandName, newState, eventDetail });
            }
        }

        internal override void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        {
            Hashtable mapArgs = new Hashtable();
            IContainsErrorRecord record = exception as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = record.ErrorRecord.CategoryInfo.Category;
                mapArgs["ErrorId"] = record.ErrorRecord.FullyQualifiedErrorId;
                if (record.ErrorRecord.ErrorDetails != null)
                {
                    mapArgs["ErrorMessage"] = record.ErrorRecord.ErrorDetails.Message;
                }
                else
                {
                    mapArgs["ErrorMessage"] = exception.Message;
                }
            }
            else
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = "";
                mapArgs["ErrorId"] = "";
                mapArgs["ErrorMessage"] = exception.Message;
            }
            FillEventArgs(mapArgs, logContext);
            FillEventArgs(mapArgs, additionalInfo);
            EventInstance entry = new EventInstance((long) eventId, 1) {
                EntryType = GetEventLogEntryType(logContext)
            };
            string eventDetail = this.GetEventDetail("EngineHealthContext", mapArgs);
            this.LogEvent(entry, new object[] { mapArgs["ErrorMessage"], eventDetail });
        }

        internal override void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
        {
            int engineLifecycleEventId = GetEngineLifecycleEventId(newState);
            if (engineLifecycleEventId != -1)
            {
                Hashtable mapArgs = new Hashtable();
                mapArgs["NewEngineState"] = newState.ToString();
                mapArgs["PreviousEngineState"] = previousState.ToString();
                FillEventArgs(mapArgs, logContext);
                EventInstance entry = new EventInstance((long) engineLifecycleEventId, 4) {
                    EntryType = EventLogEntryType.Information
                };
                string eventDetail = this.GetEventDetail("EngineLifecycleContext", mapArgs);
                this.LogEvent(entry, new object[] { newState, previousState, eventDetail });
            }
        }

        private void LogEvent(EventInstance entry, params object[] args)
        {
            try
            {
                this._eventLog.WriteEvent(entry, args);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (Win32Exception)
            {
            }
        }

        internal override void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
        {
            List<string> list = this.GroupMessages(pipelineExecutionDetail);
            for (int i = 0; i < list.Count; i++)
            {
                this.LogPipelineExecutionDetailEvent(logContext, list[i], i + 1, list.Count);
            }
        }

        private void LogPipelineExecutionDetailEvent(LogContext logContext, string pipelineExecutionDetail, int detailSequence, int detailTotal)
        {
            int num = 800;
            Hashtable mapArgs = new Hashtable();
            mapArgs["PipelineExecutionDetail"] = pipelineExecutionDetail;
            mapArgs["DetailSequence"] = detailSequence;
            mapArgs["DetailTotal"] = detailTotal;
            FillEventArgs(mapArgs, logContext);
            EventInstance entry = new EventInstance((long) num, 8) {
                EntryType = EventLogEntryType.Information
            };
            string eventDetail = this.GetEventDetail("PipelineExecutionDetailContext", mapArgs);
            this.LogEvent(entry, new object[] { logContext.CommandLine, eventDetail, pipelineExecutionDetail });
        }

        internal override void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
        {
            int num = 300;
            Hashtable mapArgs = new Hashtable();
            mapArgs["ProviderName"] = providerName;
            IContainsErrorRecord record = exception as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = record.ErrorRecord.CategoryInfo.Category;
                mapArgs["ErrorId"] = record.ErrorRecord.FullyQualifiedErrorId;
                if ((record.ErrorRecord.ErrorDetails != null) && !string.IsNullOrEmpty(record.ErrorRecord.ErrorDetails.Message))
                {
                    mapArgs["ErrorMessage"] = record.ErrorRecord.ErrorDetails.Message;
                }
                else
                {
                    mapArgs["ErrorMessage"] = exception.Message;
                }
            }
            else
            {
                mapArgs["ExceptionClass"] = exception.GetType().Name;
                mapArgs["ErrorCategory"] = "";
                mapArgs["ErrorId"] = "";
                mapArgs["ErrorMessage"] = exception.Message;
            }
            FillEventArgs(mapArgs, logContext);
            EventInstance entry = new EventInstance((long) num, 3) {
                EntryType = GetEventLogEntryType(logContext)
            };
            string eventDetail = this.GetEventDetail("ProviderHealthContext", mapArgs);
            this.LogEvent(entry, new object[] { mapArgs["ErrorMessage"], eventDetail });
        }

        internal override void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
        {
            int providerLifecycleEventId = GetProviderLifecycleEventId(newState);
            if (providerLifecycleEventId != -1)
            {
                Hashtable mapArgs = new Hashtable();
                mapArgs["ProviderName"] = providerName;
                mapArgs["NewProviderState"] = newState.ToString();
                FillEventArgs(mapArgs, logContext);
                EventInstance entry = new EventInstance((long) providerLifecycleEventId, 6) {
                    EntryType = EventLogEntryType.Information
                };
                string eventDetail = this.GetEventDetail("ProviderLifecycleContext", mapArgs);
                this.LogEvent(entry, new object[] { providerName, newState, eventDetail });
            }
        }

        internal override void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
        {
            int num = 700;
            Hashtable mapArgs = new Hashtable();
            mapArgs["VariableName"] = variableName;
            mapArgs["NewValue"] = value;
            mapArgs["PreviousValue"] = previousValue;
            FillEventArgs(mapArgs, logContext);
            EventInstance entry = new EventInstance((long) num, 7) {
                EntryType = EventLogEntryType.Information
            };
            string eventDetail = this.GetEventDetail("SettingsContext", mapArgs);
            this.LogEvent(entry, new object[] { variableName, value, previousValue, eventDetail });
        }

        internal string SetupEventSource(string shellId)
        {
            string str;
            if (string.IsNullOrEmpty(shellId))
            {
                str = "Default";
            }
            else
            {
                int num = shellId.LastIndexOf('.');
                if (num < 0)
                {
                    str = shellId;
                }
                else
                {
                    str = shellId.Substring(num + 1);
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = "Default";
                }
            }
            if (!EventLog.SourceExists(str))
            {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
                	throw new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, "Event source '{0}' is not registered", new object[] { str }));
				}
				else {
					EventLog.CreateEventSource(str, str + ".log");
					bool exists = EventLog.SourceExists(str);
				}
            }
            return str;
        }
    }
}

