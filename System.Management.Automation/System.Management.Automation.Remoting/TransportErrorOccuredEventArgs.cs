namespace System.Management.Automation.Remoting
{
    using System;

    internal class TransportErrorOccuredEventArgs : EventArgs
    {
        private PSRemotingTransportException exception;
        private TransportMethodEnum method;

        internal TransportErrorOccuredEventArgs(PSRemotingTransportException e, TransportMethodEnum m)
        {
            this.exception = e;
            this.method = m;
        }

        internal PSRemotingTransportException Exception
        {
            get
            {
                return this.exception;
            }
            set
            {
                this.exception = value;
            }
        }

        internal TransportMethodEnum ReportingTransportMethod
        {
            get
            {
                return this.method;
            }
        }
    }
}

