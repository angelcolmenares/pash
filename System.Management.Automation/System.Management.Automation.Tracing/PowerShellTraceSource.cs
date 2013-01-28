namespace System.Management.Automation.Tracing
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Text;

    public sealed class PowerShellTraceSource : IDisposable
    {
        private readonly BaseChannelWriter analyticChannel;
        private readonly BaseChannelWriter debugChannel;
        private bool disposed;
        private PowerShellTraceKeywords keywords;
        private readonly BaseChannelWriter operationsChannel;
        private PowerShellTraceTask task;

        internal PowerShellTraceSource(PowerShellTraceTask task, PowerShellTraceKeywords keywords)
        {
            if (this.IsEtwSupported)
            {
                this.debugChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Debug, keywords | (PowerShellTraceKeywords.None | PowerShellTraceKeywords.UseAlwaysDebug));
                this.analyticChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Analytic, keywords | (PowerShellTraceKeywords.None | PowerShellTraceKeywords.UseAlwaysAnalytic));
                this.operationsChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Operational, keywords | (PowerShellTraceKeywords.None | PowerShellTraceKeywords.UseAlwaysOperational));
                this.task = task;
                this.keywords = keywords;
            }
            else
            {
                this.debugChannel = NullWriter.Instance;
                this.analyticChannel = NullWriter.Instance;
                this.operationsChannel = NullWriter.Instance;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                GC.SuppressFinalize(this);
                this.debugChannel.Dispose();
                this.analyticChannel.Dispose();
                this.operationsChannel.Dispose();
            }
        }

        public bool TraceErrorRecord(ErrorRecord errorRecord)
        {
            if (errorRecord != null)
            {
                Exception exception = errorRecord.Exception;
                string message = "None";
                if (exception.InnerException != null)
                {
                    message = exception.InnerException.Message;
                }
                ErrorCategoryInfo categoryInfo = errorRecord.CategoryInfo;
                string str2 = "None";
                if (errorRecord.ErrorDetails != null)
                {
                    str2 = errorRecord.ErrorDetails.Message;
                }
                return this.debugChannel.TraceError(PowerShellTraceEvent.ErrorRecord, PowerShellTraceOperationCode.Exception, PowerShellTraceTask.None, new object[] { str2, categoryInfo.Category.ToString(), categoryInfo.Reason, categoryInfo.TargetName, errorRecord.FullyQualifiedErrorId, exception.Message, exception.StackTrace, message });
            }
            return this.debugChannel.TraceError(PowerShellTraceEvent.ErrorRecord, PowerShellTraceOperationCode.Exception, PowerShellTraceTask.None, new object[] { "NULL errorRecord" });
        }

        public bool TraceException(Exception exception)
        {
            if (exception != null)
            {
                string message = "None";
                if (exception.InnerException != null)
                {
                    message = exception.InnerException.Message;
                }
                return this.debugChannel.TraceError(PowerShellTraceEvent.Exception, PowerShellTraceOperationCode.Exception, PowerShellTraceTask.None, new object[] { exception.Message, exception.StackTrace, message });
            }
            return this.debugChannel.TraceError(PowerShellTraceEvent.Exception, PowerShellTraceOperationCode.Exception, PowerShellTraceTask.None, new object[] { "NULL exception" });
        }

        public bool TraceJob(Job job)
        {
            if (job != null)
            {
                return this.debugChannel.TraceDebug(PowerShellTraceEvent.Job, PowerShellTraceOperationCode.Method, PowerShellTraceTask.None, new object[] { job.Id.ToString(CultureInfo.InvariantCulture), job.InstanceId.ToString(), job.Name, job.Location, job.JobStateInfo.State.ToString(), job.Command });
            }
            return this.debugChannel.TraceDebug(PowerShellTraceEvent.Job, PowerShellTraceOperationCode.Method, PowerShellTraceTask.None, new object[] { job.Id.ToString(CultureInfo.InvariantCulture), job.InstanceId.ToString(), "NULL job" });
        }

        public bool TracePowerShellObject(PSObject powerShellObject)
        {
            return this.debugChannel.TraceDebug(PowerShellTraceEvent.PowerShellObject, PowerShellTraceOperationCode.Method, PowerShellTraceTask.None, new object[0]);
        }

        public bool TraceWSManConnectionInfo(WSManConnectionInfo connectionInfo)
        {
            return true;
        }

        public bool WriteMessage(string message)
        {
            return this.debugChannel.TraceInformational(PowerShellTraceEvent.TraceMessage, PowerShellTraceOperationCode.None, PowerShellTraceTask.None, new object[] { message });
        }

        public bool WriteMessage(string message, Guid instanceId)
        {
            return this.debugChannel.TraceInformational(PowerShellTraceEvent.TraceMessageGuid, PowerShellTraceOperationCode.None, PowerShellTraceTask.None, new object[] { message, instanceId });
        }

        public bool WriteMessage(string message1, string message2)
        {
            return this.debugChannel.TraceInformational(PowerShellTraceEvent.TraceMessage2, PowerShellTraceOperationCode.None, PowerShellTraceTask.None, new object[] { message1, message2 });
        }

        public void WriteMessage(string className, string methodName, Guid workflowId, string message, params string[] parameters)
        {
            PSEtwLog.LogAnalyticVerbose(PSEventId.Engine_Trace, PSOpcode.Method, PSTask.None, PSKeyword.UseAlwaysAnalytic, new object[] { className, methodName, workflowId.ToString(), (parameters == null) ? message : StringUtil.Format(message, (object[]) parameters), string.Empty, string.Empty, string.Empty, string.Empty });
        }

        public void WriteMessage(string className, string methodName, Guid workflowId, Job job, string message, params string[] parameters)
        {
            StringBuilder builder = new StringBuilder();
            if (job != null)
            {
                try
                {
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobName, job.Name));
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobId, job.Id.ToString(CultureInfo.InvariantCulture)));
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobInstanceId, job.InstanceId.ToString()));
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobLocation, job.Location));
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobState, job.JobStateInfo.State.ToString()));
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobCommand, job.Command));
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    this.TraceException(exception);
                    builder.Clear();
                    builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobName, EtwLoggingStrings.NullJobName));
                }
            }
            else
            {
                builder.AppendLine(StringUtil.Format(EtwLoggingStrings.JobName, EtwLoggingStrings.NullJobName));
            }
            PSEtwLog.LogAnalyticVerbose(PSEventId.Engine_Trace, PSOpcode.Method, PSTask.None, PSKeyword.UseAlwaysAnalytic, new object[] { className, methodName, workflowId.ToString(), (parameters == null) ? message : StringUtil.Format(message, (object[]) parameters), builder.ToString(), string.Empty, string.Empty, string.Empty });
        }

        public void WriteMessage(string className, string methodName, Guid workflowId, string activityName, Guid activityId, string message, params string[] parameters)
        {
            PSEtwLog.LogAnalyticVerbose(PSEventId.Engine_Trace, PSOpcode.Method, PSTask.None, PSKeyword.UseAlwaysAnalytic, new object[] { className, methodName, workflowId.ToString(), (parameters == null) ? message : StringUtil.Format(message, (object[]) parameters), string.Empty, activityName, activityId.ToString(), string.Empty });
        }

        public void WriteScheduledJobCompleteEvent(params object[] args)
        {
            PSEtwLog.LogOperationalInformation(PSEventId.ScheduledJob_Complete, PSOpcode.Method, PSTask.ScheduledJob, PSKeyword.UseAlwaysOperational, args);
        }

        public void WriteScheduledJobErrorEvent(params object[] args)
        {
            PSEtwLog.LogOperationalError(PSEventId.ScheduledJob_Error, PSOpcode.Exception, PSTask.ScheduledJob, PSKeyword.UseAlwaysOperational, args);
        }

        public void WriteScheduledJobStartEvent(params object[] args)
        {
            PSEtwLog.LogOperationalInformation(PSEventId.ScheduledJob_Start, PSOpcode.Method, PSTask.ScheduledJob, PSKeyword.UseAlwaysOperational, args);
        }

        public BaseChannelWriter AnalyticChannel
        {
            get
            {
                return this.analyticChannel;
            }
        }

        public BaseChannelWriter DebugChannel
        {
            get
            {
                return this.debugChannel;
            }
        }

        private bool IsEtwSupported
        {
            get
            {
                return (Environment.OSVersion.Version.Major >= 6);
            }
        }

        public PowerShellTraceKeywords Keywords
        {
            get
            {
                return this.keywords;
            }
        }

        public BaseChannelWriter OperationalChannel
        {
            get
            {
                return this.operationsChannel;
            }
        }

        public PowerShellTraceTask Task
        {
            get
            {
                return this.task;
            }
            set
            {
                this.task = value;
            }
        }
    }
}

