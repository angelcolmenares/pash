namespace System.Management.Automation
{
    using System;

    internal class RemoteSessionStateInfo
    {
        private Exception _reason;
        private RemoteSessionState _state;

        internal RemoteSessionStateInfo(RemoteSessionState state) : this(state, null)
        {
        }

        internal RemoteSessionStateInfo(RemoteSessionStateInfo sessionStateInfo)
        {
            this._state = sessionStateInfo.State;
            this._reason = sessionStateInfo.Reason;
        }

        internal RemoteSessionStateInfo(RemoteSessionState state, Exception reason)
        {
            this._state = state;
            this._reason = reason;
        }

        internal Exception Reason
        {
            get
            {
                return this._reason;
            }
        }

        internal RemoteSessionState State
        {
            get
            {
                return this._state;
            }
        }
    }
}

