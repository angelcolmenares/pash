namespace System.Management.Automation
{
    using System;

    public sealed class JobDataAddedEventArgs : EventArgs
    {
        private readonly PowerShellStreamType _dataType;
        private readonly int _index;
        private readonly Job _job;

        internal JobDataAddedEventArgs(Job job, PowerShellStreamType dataType, int index)
        {
            this._job = job;
            this._dataType = dataType;
            this._index = index;
        }

        public PowerShellStreamType DataType
        {
            get
            {
                return this._dataType;
            }
        }

        public int Index
        {
            get
            {
                return this._index;
            }
        }

        public Job SourceJob
        {
            get
            {
                return this._job;
            }
        }
    }
}

