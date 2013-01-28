using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation.Runspaces;

	public class HistoryInfoCommandEqualityComparer : IEqualityComparer<HistoryInfo>
	{
		#region IEqualityComparer implementation		

		public bool Equals (HistoryInfo x, HistoryInfo y)
		{
			var cmd1 = x == null ? string.Empty : x.CommandLine;
			var cmd2 = y == null ? string.Empty : y.CommandLine;
			return cmd1.Equals (cmd2);
		}		

		public int GetHashCode (HistoryInfo obj)
		{
			return obj == null ? -1 : obj.CommandLine.GetHashCode ();
		}		

		#endregion
	}

    public class HistoryInfo
    {
        private bool _cleared;
        private string _cmdline;
        private DateTime _endTime;
        private long _id;
        private long _pipelineId;
        private DateTime _startTime;
        private PipelineState _status;

        private HistoryInfo(HistoryInfo history)
        {
            this._id = history._id;
            this._pipelineId = history._pipelineId;
            this._cmdline = history._cmdline;
            this._status = history._status;
            this._startTime = history._startTime;
            this._endTime = history._endTime;
            this._cleared = history._cleared;
        }

        internal HistoryInfo(long pipelineId, string cmdline, PipelineState status, DateTime startTime, DateTime endTime)
        {
            this._pipelineId = pipelineId;
            this._cmdline = cmdline;
            this._status = status;
            this._startTime = startTime;
            this._endTime = endTime;
            this._cleared = false;
        }

        public HistoryInfo Clone()
        {
            return new HistoryInfo(this);
        }

        internal void SetCommand(string command)
        {
            this._cmdline = command;
        }

        internal void SetEndTime(DateTime endTime)
        {
            this._endTime = endTime;
        }

        internal void SetId(long id)
        {
            this._id = id;
        }

        internal void SetStatus(PipelineState status)
        {
            this._status = status;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this._cmdline))
            {
                return base.ToString();
            }
            return this._cmdline;
        }

        internal bool Cleared
        {
            get
            {
                return this._cleared;
            }
            set
            {
                this._cleared = value;
            }
        }

        public string CommandLine
        {
            get
            {
                return this._cmdline;
            }
        }

        public DateTime EndExecutionTime
        {
            get
            {
                return this._endTime;
            }
        }

        public PipelineState ExecutionStatus
        {
            get
            {
                return this._status;
            }
        }

        public long Id
        {
            get
            {
                return this._id;
            }
        }

        public DateTime StartExecutionTime
        {
            get
            {
                return this._startTime;
            }
        }
    }
}

