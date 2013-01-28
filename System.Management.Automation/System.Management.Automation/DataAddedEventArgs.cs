namespace System.Management.Automation
{
    using System;

    public sealed class DataAddedEventArgs : EventArgs
    {
        private int index;
        private Guid psInstanceId;

        internal DataAddedEventArgs(Guid psInstanceId, int index)
        {
            this.psInstanceId = psInstanceId;
            this.index = index;
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public Guid PowerShellInstanceId
        {
            get
            {
                return this.psInstanceId;
            }
        }
    }
}

