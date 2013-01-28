namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Add", "History", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113279"), OutputType(new Type[] { typeof(HistoryInfo) })]
    public class AddHistoryCommand : PSCmdlet
    {
        private PSObject[] _inputObjects;
        private bool _passthru;

        protected override void BeginProcessing()
        {
            ((LocalPipeline) ((RunspaceBase) base.Context.CurrentRunspace).GetCurrentlyRunningPipeline()).AddHistoryEntryFromAddHistoryCmdlet();
        }

        private HistoryInfo GetHistoryInfoObject(PSObject mshObject)
        {
            PipelineState state;
            object obj5;
            Exception exception;
            if (mshObject == null)
            {
                goto Label_0146;
            }
            string propertyValue = GetPropertyValue(mshObject, "CommandLine") as string;
            if (propertyValue == null)
            {
                goto Label_0146;
            }
            object obj2 = GetPropertyValue(mshObject, "ExecutionStatus");
            if (obj2 == null)
            {
                goto Label_0146;
            }
            if (obj2 is PipelineState)
            {
                state = (PipelineState) obj2;
            }
            else
            {
                if (obj2 is PSObject)
                {
                    PSObject obj3 = obj2 as PSObject;
                    object baseObject = obj3.BaseObject;
                    if (baseObject is int)
                    {
                        state = (PipelineState) baseObject;
                        if ((state >= PipelineState.NotStarted) && (state <= PipelineState.Failed))
                        {
                            goto Label_00A9;
                        }
                    }
                    goto Label_0146;
                }
                if (!(obj2 is string))
                {
                    goto Label_0146;
                }
                try
                {
                    state = (PipelineState) Enum.Parse(typeof(PipelineState), (string) obj2);
                }
                catch (ArgumentException)
                {
                    goto Label_0146;
                }
            }
        Label_00A9:
            obj5 = GetPropertyValue(mshObject, "StartExecutionTime");
            if (obj5 != null)
            {
                DateTime time;
                if (obj5 is DateTime)
                {
                    time = (DateTime) obj5;
                }
                else
                {
                    if (!(obj5 is string))
                    {
                        goto Label_0146;
                    }
                    try
                    {
                        time = DateTime.Parse((string) obj5, CultureInfo.CurrentCulture);
                    }
                    catch (FormatException)
                    {
                        goto Label_0146;
                    }
                }
                obj5 = GetPropertyValue(mshObject, "EndExecutionTime");
                if (obj5 != null)
                {
                    DateTime time2;
                    if (obj5 is DateTime)
                    {
                        time2 = (DateTime) obj5;
                    }
                    else
                    {
                        if (!(obj5 is string))
                        {
                            goto Label_0146;
                        }
                        try
                        {
                            time2 = DateTime.Parse((string) obj5, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            goto Label_0146;
                        }
                    }
                    return new HistoryInfo(0L, propertyValue, state, time, time2);
                }
            }
        Label_0146:
            exception = new InvalidDataException(StringUtil.Format(HistoryStrings.AddHistoryInvalidInput, new object[0]));
            base.WriteError(new ErrorRecord(exception, "AddHistoryInvalidInput", ErrorCategory.InvalidData, mshObject));
            return null;
        }

        private static object GetPropertyValue(PSObject mshObject, string propertyName)
        {
            PSMemberInfo info = mshObject.Properties[propertyName];
            if (info == null)
            {
                return null;
            }
            return info.Value;
        }

        protected override void ProcessRecord()
        {
            History history = ((LocalRunspace) base.Context.CurrentRunspace).History;
            if (this.InputObject != null)
            {
                foreach (PSObject obj2 in this.InputObject)
                {
                    HistoryInfo historyInfoObject = this.GetHistoryInfoObject(obj2);
                    if (historyInfoObject != null)
                    {
                        long id = history.AddEntry(0L, historyInfoObject.CommandLine, historyInfoObject.ExecutionStatus, historyInfoObject.StartExecutionTime, historyInfoObject.EndExecutionTime, false);
                        if (this.Passthru != 0)
                        {
                            HistoryInfo entry = history.GetEntry(id);
                            base.WriteObject(entry);
                        }
                    }
                }
            }
        }

        [Parameter(Position=0, ValueFromPipeline=true)]
        public PSObject[] InputObject
        {
            get
            {
                return this._inputObjects;
            }
            set
            {
                this._inputObjects = value;
            }
        }

        [Parameter]
        public SwitchParameter Passthru
        {
            get
            {
                return this._passthru;
            }
            set
            {
                this._passthru = (bool) value;
            }
        }
    }
}

