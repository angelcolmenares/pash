namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class PSStreamObject
    {
        internal PSStreamObject(PSStreamObjectType objectType, object value) : this(objectType, value, Guid.Empty)
        {
        }

        internal PSStreamObject(PSStreamObjectType objectType, object value, Guid id)
        {
            this.ObjectType = objectType;
            this.Value = value;
            this.Id = id;
        }

        internal static void AddSourceJobNoteProperty(PSObject psObj, Guid instanceId)
        {
            if (psObj.Properties[RemotingConstants.SourceJobInstanceId] != null)
            {
                psObj.Properties.Remove(RemotingConstants.SourceJobInstanceId);
            }
            psObj.Properties.Add(new PSNoteProperty(RemotingConstants.SourceJobInstanceId, instanceId));
        }

        internal static ErrorRecord AddSourceTagToError(ErrorRecord errorRecord, Guid sourceId)
        {
            if (errorRecord == null)
            {
                return null;
            }
            if (errorRecord.ErrorDetails == null)
            {
                errorRecord.ErrorDetails = new ErrorDetails(string.Empty);
            }
            errorRecord.ErrorDetails.RecommendedAction = CreateInformationalMessage(sourceId, errorRecord.ErrorDetails.RecommendedAction);
            return errorRecord;
        }

        internal static string CreateInformationalMessage(Guid instanceId, string message)
        {
            StringBuilder builder = new StringBuilder(instanceId.ToString());
            builder.Append(":");
            builder.Append(message);
            return builder.ToString();
        }

        private static void GetIdentifierInfo(string message, out Guid jobInstanceId, out string computerName)
        {
            jobInstanceId = Guid.Empty;
            computerName = string.Empty;
            if (message != null)
            {
                string[] strArray = message.Split(new char[] { ':' }, 3);
                if (strArray.Length == 3)
                {
                    if (!Guid.TryParse(strArray[0], out jobInstanceId))
                    {
                        jobInstanceId = Guid.Empty;
                    }
                    computerName = strArray[1];
                }
            }
        }

        private static void InvokeCmdletMethodAndWaitForResults<T>(CmdletMethodInvoker<T> cmdletMethodInvoker, Cmdlet cmdlet)
        {
            cmdletMethodInvoker.MethodResult = default(T);
            try
            {
                T local = cmdletMethodInvoker.Action(cmdlet);
                lock (cmdletMethodInvoker.SyncObject)
                {
                    cmdletMethodInvoker.MethodResult = local;
                }
            }
            catch (Exception exception)
            {
                lock (cmdletMethodInvoker.SyncObject)
                {
                    cmdletMethodInvoker.ExceptionThrownOnCmdletThread = exception;
                }
                throw;
            }
            finally
            {
                if (cmdletMethodInvoker.Finished != null)
                {
                    cmdletMethodInvoker.Finished.Set();
                }
            }
        }

        internal void WriteStreamObject(Cmdlet cmdlet, bool overrideInquire = false)
        {
            switch (this.ObjectType)
            {
                case PSStreamObjectType.Output:
                    cmdlet.WriteObject(this.Value);
                    return;

                case PSStreamObjectType.Error:
                {
                    ErrorRecord errorRecord = (ErrorRecord) this.Value;
                    errorRecord.PreserveInvocationInfoOnce = true;
                    MshCommandRuntime commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (commandRuntime == null)
                    {
                        break;
                    }
                    commandRuntime.WriteError(errorRecord, overrideInquire);
                    return;
                }
                case PSStreamObjectType.MethodExecutor:
                    ((ClientMethodExecutor) this.Value).Execute(cmdlet);
                    return;

                case PSStreamObjectType.Warning:
                {
                    string message = (string) this.Value;
                    WarningRecord record = new WarningRecord(message);
                    MshCommandRuntime runtime3 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime3 == null)
                    {
                        break;
                    }
                    runtime3.WriteWarning(record, overrideInquire);
                    return;
                }
                case PSStreamObjectType.BlockingError:
                {
                    CmdletMethodInvoker<object> cmdletMethodInvoker = (CmdletMethodInvoker<object>) this.Value;
                    InvokeCmdletMethodAndWaitForResults<object>(cmdletMethodInvoker, cmdlet);
                    return;
                }
                case PSStreamObjectType.ShouldMethod:
                {
                    CmdletMethodInvoker<bool> invoker2 = (CmdletMethodInvoker<bool>) this.Value;
                    InvokeCmdletMethodAndWaitForResults<bool>(invoker2, cmdlet);
                    return;
                }
                case PSStreamObjectType.WarningRecord:
                {
                    WarningRecord record5 = (WarningRecord) this.Value;
                    MshCommandRuntime runtime6 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime6 == null)
                    {
                        break;
                    }
                    runtime6.AppendWarningVarList(record5);
                    return;
                }
                case PSStreamObjectType.Debug:
                {
                    string str = (string) this.Value;
                    DebugRecord record2 = new DebugRecord(str);
                    MshCommandRuntime runtime2 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime2 == null)
                    {
                        break;
                    }
                    runtime2.WriteDebug(record2, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Progress:
                {
                    MshCommandRuntime runtime5 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime5 == null)
                    {
                        break;
                    }
                    runtime5.WriteProgress((ProgressRecord) this.Value, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Verbose:
                {
                    string str3 = (string) this.Value;
                    VerboseRecord record4 = new VerboseRecord(str3);
                    MshCommandRuntime runtime4 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime4 == null)
                    {
                        break;
                    }
                    runtime4.WriteVerbose(record4, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Exception:
                {
                    Exception exception = (Exception) this.Value;
                    throw exception;
                }
                default:
                    return;
            }
        }

        internal void WriteStreamObject(Cmdlet cmdlet, bool writeSourceIdentifier, bool overrideInquire)
        {
            if (writeSourceIdentifier)
            {
                this.WriteStreamObject(cmdlet, this.Id, overrideInquire);
            }
            else
            {
                this.WriteStreamObject(cmdlet, overrideInquire);
            }
        }

        internal void WriteStreamObject(Cmdlet cmdlet, Guid instanceId, bool overrideInquire = false)
        {
            switch (this.ObjectType)
            {
                case PSStreamObjectType.Output:
                    if (instanceId != Guid.Empty)
                    {
                        PSObject psObj = this.Value as PSObject;
                        if (psObj != null)
                        {
                            AddSourceJobNoteProperty(psObj, instanceId);
                        }
                    }
                    cmdlet.WriteObject(this.Value);
                    return;

                case PSStreamObjectType.Error:
                {
                    ErrorRecord errorRecord = (ErrorRecord) this.Value;
                    if ((!(errorRecord is RemotingErrorRecord) && (errorRecord.ErrorDetails != null)) && !string.IsNullOrEmpty(errorRecord.ErrorDetails.RecommendedAction))
                    {
                        string str;
                        Guid guid;
                        GetIdentifierInfo(errorRecord.ErrorDetails.RecommendedAction, out guid, out str);
                        errorRecord = new RemotingErrorRecord(errorRecord, new OriginInfo(str, Guid.Empty, guid));
                    }
                    errorRecord.PreserveInvocationInfoOnce = true;
                    MshCommandRuntime commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (commandRuntime == null)
                    {
                        break;
                    }
                    commandRuntime.WriteError(errorRecord, overrideInquire);
                    return;
                }
                case PSStreamObjectType.MethodExecutor:
                case PSStreamObjectType.BlockingError:
                case PSStreamObjectType.ShouldMethod:
                case PSStreamObjectType.WarningRecord:
                    this.WriteStreamObject(cmdlet, overrideInquire);
                    break;

                case PSStreamObjectType.Warning:
                {
                    string message = (string) this.Value;
                    WarningRecord record = new WarningRecord(message);
                    MshCommandRuntime runtime2 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime2 == null)
                    {
                        break;
                    }
                    runtime2.WriteWarning(record, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Debug:
                {
                    string str5 = (string) this.Value;
                    DebugRecord record7 = new DebugRecord(str5);
                    MshCommandRuntime runtime5 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime5 == null)
                    {
                        break;
                    }
                    runtime5.WriteDebug(record7, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Progress:
                {
                    ProgressRecord progressRecord = (ProgressRecord) this.Value;
                    if (!(progressRecord is RemotingProgressRecord))
                    {
                        Guid guid2;
                        string str4;
                        GetIdentifierInfo(progressRecord.CurrentOperation, out guid2, out str4);
                        OriginInfo originInfo = new OriginInfo(str4, Guid.Empty, guid2);
                        progressRecord = new RemotingProgressRecord(progressRecord, originInfo);
                    }
                    MshCommandRuntime runtime4 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime4 == null)
                    {
                        break;
                    }
                    runtime4.WriteProgress(progressRecord, overrideInquire);
                    return;
                }
                case PSStreamObjectType.Verbose:
                {
                    string str3 = (string) this.Value;
                    VerboseRecord record4 = new VerboseRecord(str3);
                    MshCommandRuntime runtime3 = cmdlet.CommandRuntime as MshCommandRuntime;
                    if (runtime3 == null)
                    {
                        break;
                    }
                    runtime3.WriteVerbose(record4, overrideInquire);
                    return;
                }
                default:
                    return;
            }
        }

        internal Guid Id { get; set; }

        internal PSStreamObjectType ObjectType { get; set; }

        internal object Value { get; set; }
    }
}

