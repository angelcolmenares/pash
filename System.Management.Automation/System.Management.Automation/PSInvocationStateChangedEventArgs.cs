namespace System.Management.Automation
{
    using System;

    public sealed class PSInvocationStateChangedEventArgs : EventArgs
    {
        private PSInvocationStateInfo executionStateInfo;

        internal PSInvocationStateChangedEventArgs(PSInvocationStateInfo psStateInfo)
        {
            this.executionStateInfo = psStateInfo;
        }

        public PSInvocationStateInfo InvocationStateInfo
        {
            get
            {
                return this.executionStateInfo;
            }
        }
    }
}

