namespace System.Management.Automation
{
    using System;

    internal class ThrottlingJobChildAddedEventArgs : EventArgs
    {
        private readonly Job _addedChildJob;

        internal ThrottlingJobChildAddedEventArgs(Job addedChildJob)
        {
            this._addedChildJob = addedChildJob;
        }

        internal Job AddedChildJob
        {
            get
            {
                return this._addedChildJob;
            }
        }
    }
}

