namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;

    internal class RemoteSessionStateMachineEventArgs : EventArgs
    {
        private System.Management.Automation.Remoting.RemoteSessionCapability _capability;
        private Exception _reason;
        private RemoteDataObject<PSObject> _remoteObject;
        private RemoteSessionEvent _stateEvent;

        internal RemoteSessionStateMachineEventArgs(RemoteSessionEvent stateEvent) : this(stateEvent, null)
        {
        }

        internal RemoteSessionStateMachineEventArgs (RemoteSessionEvent stateEvent, Exception reason)
		{
			this._stateEvent = stateEvent;
			this._reason = reason;
        }

        internal Exception Reason
        {
            get
            {
                return this._reason;
            }
        }

        internal RemoteDataObject<PSObject> RemoteData
        {
            get
            {
                return this._remoteObject;
            }
            set
            {
                this._remoteObject = value;
            }
        }

        internal System.Management.Automation.Remoting.RemoteSessionCapability RemoteSessionCapability
        {
            get
            {
                return this._capability;
            }
            set
            {
                this._capability = value;
            }
        }

        internal RemoteSessionEvent StateEvent
        {
            get
            {
                return this._stateEvent;
            }
        }
    }
}

