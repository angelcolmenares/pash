namespace System.Management.Automation
{
    using System;

    internal class RemoteSessionStateEventArgs : EventArgs
    {
        private RemoteSessionStateInfo _remoteSessionStateInfo;

        internal RemoteSessionStateEventArgs (RemoteSessionStateInfo remoteSessionStateInfo)
		{
			if (remoteSessionStateInfo == null) {
				PSTraceSource.NewArgumentNullException ("remoteSessionStateInfo");
			}
			this._remoteSessionStateInfo = remoteSessionStateInfo;
        }

        public RemoteSessionStateInfo SessionStateInfo
        {
            get
            {
                return this._remoteSessionStateInfo;
            }
        }
    }
}

