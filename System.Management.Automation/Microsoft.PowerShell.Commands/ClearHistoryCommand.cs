namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Clear", "History", SupportsShouldProcess=true, DefaultParameterSetName="IDParameter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135199")]
    public class ClearHistoryCommand : PSCmdlet
    {
        private string[] _commandline;
        private int _count = 0x20;
        private bool _countParamterSpecified;
        private int[] _id;
        private SwitchParameter _newest;
        private HistoryInfo[] entries;
        private History history;

        protected override void BeginProcessing()
        {
            this.history = ((LocalRunspace) base.Context.CurrentRunspace).History;
        }

        private void ClearHistoryByCmdLine()
        {
            if (this._countParamterSpecified && (this.Count < 0))
            {
                Exception exception = new ArgumentException(StringUtil.Format(HistoryStrings.InvalidCountValue, new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception, "ClearHistoryInvalidCountValue", ErrorCategory.InvalidArgument, this._count));
            }
            if (this._commandline != null)
            {
                if (!this._countParamterSpecified)
                {
                    foreach (string str in this._commandline)
                    {
                        this.ClearHistoryEntries(0L, 1, str, this._newest);
                    }
                }
                else if (this._commandline.Length > 1)
                {
                    Exception exception2 = new ArgumentException(StringUtil.Format(HistoryStrings.NoCountWithMultipleCmdLine, new object[0]));
                    base.ThrowTerminatingError(new ErrorRecord(exception2, "NoCountWithMultipleCmdLine ", ErrorCategory.InvalidArgument, this._commandline));
                }
                else
                {
                    this.ClearHistoryEntries(0L, this._count, this._commandline[0], this._newest);
                }
            }
        }

        private void ClearHistoryByID()
        {
            if (this._countParamterSpecified && (this.Count < 0))
            {
                Exception exception = new ArgumentException(StringUtil.Format("HistoryStrings", "InvalidCountValue"));
                base.ThrowTerminatingError(new ErrorRecord(exception, "ClearHistoryInvalidCountValue", ErrorCategory.InvalidArgument, this._count));
            }
            if (this._id != null)
            {
                if (!this._countParamterSpecified)
                {
                    int[] numArray = this._id;
                    for (int i = 0; i < numArray.Length; i++)
                    {
                        long id = numArray[i];
                        HistoryInfo entry = this.history.GetEntry(id);
                        if ((entry != null) && (entry.Id == id))
                        {
                            this.history.ClearEntry(entry.Id);
                        }
                        else
                        {
                            Exception exception2 = new ArgumentException(StringUtil.Format(HistoryStrings.NoHistoryForId, id));
                            base.WriteError(new ErrorRecord(exception2, "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, id));
                        }
                    }
                }
                else if (this._id.Length > 1)
                {
                    Exception exception3 = new ArgumentException(StringUtil.Format(HistoryStrings.NoCountWithMultipleIds, new object[0]));
                    base.ThrowTerminatingError(new ErrorRecord(exception3, "GetHistoryNoCountWithMultipleIds", ErrorCategory.InvalidArgument, this._count));
                }
                else
                {
                    long num2 = this._id[0];
                    this.ClearHistoryEntries(num2, this._count, null, this._newest);
                }
            }
            else if (!this._countParamterSpecified)
            {
                string target = StringUtil.Format(HistoryStrings.ClearHistoryWarning, "Warning");
                if (base.ShouldProcess(target))
                {
                    this.ClearHistoryEntries(0L, -1, null, this._newest);
                }
            }
            else
            {
                this.ClearHistoryEntries(0L, this._count, null, this._newest);
            }
        }

        private void ClearHistoryEntries(long id, int count, string cmdline, SwitchParameter newest)
        {
            if (cmdline == null)
            {
                if (id > 0L)
                {
                    HistoryInfo entry = this.history.GetEntry(id);
                    if ((entry == null) || (entry.Id != id))
                    {
                        Exception exception = new ArgumentException(StringUtil.Format(HistoryStrings.NoHistoryForId, id));
                        base.WriteError(new ErrorRecord(exception, "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, id));
                    }
                    this.entries = this.history.GetEntries(id, (long) count, newest);
                }
                else
                {
                    this.entries = this.history.GetEntries((long) 0L, (long) count, newest);
                }
            }
            else
            {
                WildcardPattern wildcardpattern = new WildcardPattern(cmdline, WildcardOptions.IgnoreCase);
                if (!this._countParamterSpecified && WildcardPattern.ContainsWildcardCharacters(cmdline))
                {
                    count = 0;
                }
                this.entries = this.history.GetEntries(wildcardpattern, (long) count, newest);
            }
            foreach (HistoryInfo info2 in this.entries)
            {
                if ((info2 != null) && !info2.Cleared)
                {
                    this.history.ClearEntry(info2.Id);
                }
            }
        }

        protected override void ProcessRecord()
        {
            switch (base.ParameterSetName.ToString())
            {
                case "IDParameter":
                    this.ClearHistoryByID();
                    return;

                case "CommandLineParameter":
                    this.ClearHistoryByCmdLine();
                    return;
            }
            base.ThrowTerminatingError(new ErrorRecord(new ArgumentException("Invalid ParameterSet Name"), "Unable to access the session history", ErrorCategory.InvalidOperation, null));
        }

        [Parameter(ParameterSetName="CommandLineParameter", HelpMessage="Specifies the name of a command in the session history"), ValidateNotNullOrEmpty]
        public string[] CommandLine
        {
            get
            {
                return this._commandline;
            }
            set
            {
                this._commandline = value;
            }
        }

        [Parameter(Mandatory=false, Position=1, HelpMessage="Clears the specified number of history entries"), ValidateRange(1, 0x7fffffff)]
        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                this._countParamterSpecified = true;
                this._count = value;
            }
        }

        [ValidateRange(1, 0x7fffffff), Parameter(ParameterSetName="IDParameter", Position=0, HelpMessage="Specifies the ID of a command in the session history.Clear history clears only the specified command")]
        public int[] Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        [Parameter(Mandatory=false, HelpMessage="Specifies whether new entries to be cleared or the default old ones.")]
        public SwitchParameter Newest
        {
            get
            {
                return this._newest;
            }
            set
            {
                this._newest = value;
            }
        }
    }
}

