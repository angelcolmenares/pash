namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [OutputType(new Type[] { typeof(HistoryInfo) }), Cmdlet("Get", "History", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113317")]
    public class GetHistoryCommand : PSCmdlet
    {
        private int _count;
        private bool _countParameterSpecified;
        private long[] _id;

        protected override void ProcessRecord()
        {
            History history = ((LocalRunspace) base.Context.CurrentRunspace).History;
            if (this._id != null)
            {
                if (!this._countParameterSpecified)
                {
                    foreach (long num in this._id)
                    {
                        HistoryInfo entry = history.GetEntry(num);
                        if ((entry != null) && (entry.Id == num))
                        {
                            base.WriteObject(entry);
                        }
                        else
                        {
                            Exception exception = new ArgumentException(StringUtil.Format(HistoryStrings.NoHistoryForId, num));
                            base.WriteError(new ErrorRecord(exception, "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, num));
                        }
                    }
                }
                else if (this._id.Length > 1)
                {
                    Exception exception2 = new ArgumentException(StringUtil.Format(HistoryStrings.NoCountWithMultipleIds, new object[0]));
                    base.ThrowTerminatingError(new ErrorRecord(exception2, "GetHistoryNoCountWithMultipleIds", ErrorCategory.InvalidArgument, this._count));
                }
                else
                {
                    long id = this._id[0];
                    base.WriteObject(history.GetEntries(id, (long) this._count, false), true);
                }
            }
            else
            {
                if (!this._countParameterSpecified)
                {
                    this._count = history.Buffercapacity();
                }
                HistoryInfo[] infoArray = history.GetEntries((long) 0L, (long) this._count, true);
                for (long i = infoArray.Length - 1; i >= 0L; i -= 1L)
                {
                    base.WriteObject(infoArray[(int) ((IntPtr) i)]);
                }
            }
        }

        [Parameter(Position=1), ValidateRange(0, 0x7fff)]
        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                this._countParameterSpecified = true;
                this._count = value;
            }
        }

        [Parameter(Position=0, ValueFromPipeline=true), ValidateRange(1L, 0x7fffffffffffffffL)]
        public long[] Id
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
    }
}

