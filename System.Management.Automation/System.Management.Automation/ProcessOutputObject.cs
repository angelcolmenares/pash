namespace System.Management.Automation
{
    using System;

    internal class ProcessOutputObject
    {
        private object data;
        private MinishellStream stream;

        internal ProcessOutputObject(object data, MinishellStream stream)
        {
            this.data = data;
            this.stream = stream;
        }

        internal object Data
        {
            get
            {
                return this.data;
            }
        }

        internal MinishellStream Stream
        {
            get
            {
                return this.stream;
            }
        }
    }
}

