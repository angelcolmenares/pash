namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;

    internal sealed class RemoteDataEventArgs : EventArgs
    {
        private RemoteDataObject<PSObject> _rcvdData;

        internal RemoteDataEventArgs(RemoteDataObject<PSObject> receivedData)
        {
            if (receivedData == null)
            {
                throw PSTraceSource.NewArgumentNullException("receivedData");
            }
            this._rcvdData = receivedData;
        }

        public RemoteDataObject<PSObject> ReceivedData
        {
            get
            {
                return this._rcvdData;
            }
        }
    }
}

