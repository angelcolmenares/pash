namespace System.Management.Automation
{
    using System;

    internal class PSEventArgs<T> : EventArgs
    {
        internal T Args;

        public PSEventArgs(T args)
        {
            this.Args = args;
        }
    }
}

