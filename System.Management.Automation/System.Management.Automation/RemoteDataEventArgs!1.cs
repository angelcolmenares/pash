namespace System.Management.Automation
{
    using System;

    internal sealed class RemoteDataEventArgs<T> : EventArgs
    {
        private T data;

        internal RemoteDataEventArgs(object data)
        {
            this.data = (T) data;
        }

        internal T Data
        {
            get
            {
                return this.data;
            }
        }
    }
}

