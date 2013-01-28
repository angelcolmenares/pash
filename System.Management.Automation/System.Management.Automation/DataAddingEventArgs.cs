namespace System.Management.Automation
{
    using System;

    public sealed class DataAddingEventArgs : EventArgs
    {
        private object itemAdded;
        private Guid psInstanceId;

        internal DataAddingEventArgs(Guid psInstanceId, object itemAdded)
        {
            this.psInstanceId = psInstanceId;
            this.itemAdded = itemAdded;
        }

        public object ItemAdded
        {
            get
            {
                return this.itemAdded;
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

