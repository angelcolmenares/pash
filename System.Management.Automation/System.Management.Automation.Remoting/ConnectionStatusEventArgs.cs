namespace System.Management.Automation.Remoting
{
    using System;

    internal class ConnectionStatusEventArgs : EventArgs
    {
        private ConnectionStatus _notification;

        internal ConnectionStatusEventArgs(ConnectionStatus notification)
        {
            this._notification = notification;
        }

        internal ConnectionStatus Notification
        {
            get
            {
                return this._notification;
            }
        }
    }
}

