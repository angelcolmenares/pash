namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Invoke", "History", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113344")]
    public class InvokeHistoryCommand : PSCmdlet
    {
        private string _commandLine;
        private long _historyId = -1L;
        private string _id;
        private bool _multipleIdProvided;

        protected override void EndProcessing()
        {
            if (this._multipleIdProvided)
            {
                Exception exception = new ArgumentException(StringUtil.Format(HistoryStrings.InvokeHistoryMultipleCommandsError, new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception, "InvokeHistoryMultipleCommandsError", ErrorCategory.InvalidArgument, null));
            }
            History history = ((LocalRunspace) base.Context.CurrentRunspace).History;
            HistoryInfo historyEntryToInvoke = this.GetHistoryEntryToInvoke(history);
            LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((LocalRunspace) base.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
            if (!currentlyRunningPipeline.PresentInInvokeHistoryEntryList(historyEntryToInvoke))
            {
                currentlyRunningPipeline.AddToInvokeHistoryEntryList(historyEntryToInvoke);
            }
            else
            {
                Exception exception2 = new InvalidOperationException(StringUtil.Format(HistoryStrings.InvokeHistoryLoopDetected, new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception2, "InvokeHistoryLoopDetected", ErrorCategory.InvalidOperation, null));
            }
            this.ReplaceHistoryString(historyEntryToInvoke);
            string commandLine = historyEntryToInvoke.CommandLine;
            if (base.ShouldProcess(commandLine))
            {
                try
                {
                    base.Host.UI.WriteLine(commandLine);
                }
                catch (HostException)
                {
                }
                Collection<PSObject> sendToPipeline = base.InvokeCommand.InvokeScript(commandLine, false, PipelineResultTypes.Warning, null, null);
                if (sendToPipeline.Count > 0)
                {
                    base.WriteObject(sendToPipeline, true);
                }
                currentlyRunningPipeline.RemoveFromInvokeHistoryEntryList(historyEntryToInvoke);
            }
        }

        private HistoryInfo GetHistoryEntryToInvoke(History history)
        {
            HistoryInfo entry = null;
            if (this._id == null)
            {
                HistoryInfo[] infoArray = history.GetEntries((long) 0L, 1L, true);
                if (infoArray.Length == 1)
                {
                    return infoArray[0];
                }
                Exception exception = new InvalidOperationException(StringUtil.Format(HistoryStrings.NoLastHistoryEntryFound, new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception, "InvokeHistoryNoLastHistoryEntryFound", ErrorCategory.InvalidOperation, null));
                return entry;
            }
            this.PopulateIdAndCommandLine();
            if (this._commandLine == null)
            {
                if (this._historyId <= 0L)
                {
                    Exception exception3 = new ArgumentOutOfRangeException("Id", StringUtil.Format(HistoryStrings.InvalidIdGetHistory, this._historyId));
                    base.ThrowTerminatingError(new ErrorRecord(exception3, "InvokeHistoryInvalidIdGetHistory", ErrorCategory.InvalidArgument, this._historyId));
                    return entry;
                }
                entry = history.GetEntry(this._historyId);
                if ((entry == null) || (entry.Id != this._historyId))
                {
                    Exception exception4 = new ArgumentException(StringUtil.Format(HistoryStrings.NoHistoryForId, this._historyId));
                    base.ThrowTerminatingError(new ErrorRecord(exception4, "InvokeHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, this._historyId));
                }
                return entry;
            }
            HistoryInfo[] infoArray2 = history.GetEntries((long) 0L, -1L, false);
            for (int i = infoArray2.Length - 1; i >= 0; i--)
            {
                if (infoArray2[i].CommandLine.StartsWith(this._commandLine, StringComparison.CurrentCulture))
                {
                    entry = infoArray2[i];
                    break;
                }
            }
            if (entry == null)
            {
                Exception exception2 = new ArgumentException(StringUtil.Format(HistoryStrings.NoHistoryForCommandline, this._commandLine));
                base.ThrowTerminatingError(new ErrorRecord(exception2, "InvokeHistoryNoHistoryForCommandline", ErrorCategory.ObjectNotFound, this._commandLine));
            }
            return entry;
        }

        private void PopulateIdAndCommandLine()
        {
            if (this._id != null)
            {
                try
                {
                    this._historyId = (long) LanguagePrimitives.ConvertTo(this._id, typeof(long), CultureInfo.InvariantCulture);
                }
                catch (PSInvalidCastException)
                {
                    this._commandLine = this._id;
                }
            }
        }

        private void ReplaceHistoryString(HistoryInfo entry)
        {
            LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((LocalRunspace) base.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
            if (currentlyRunningPipeline.AddToHistory)
            {
                currentlyRunningPipeline.HistoryString = entry.CommandLine;
            }
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id != null)
                {
                    this._multipleIdProvided = true;
                }
                this._id = value;
            }
        }
    }
}

