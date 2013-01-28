namespace System.Management.Automation.Runspaces.Internal
{
    using System;

    internal sealed class PSConnectionRetryStatusEventArgs : EventArgs
    {
        private string _computerName;
        private object _infoRecord;
        private int _maxRetryConnectionTime;
        private PSConnectionRetryStatus _notification;
        internal const string FQIDAutoDisconnectStarting = "PowerShellNetworkFailedStartDisconnect";
        internal const string FQIDAutoDisconnectSucceeded = "PowerShellAutoDisconnectSucceeded";
        internal const string FQIDConnectionRetryAttempt = "PowerShellConnectionRetryAttempt";
        internal const string FQIDConnectionRetrySucceeded = "PowerShellConnectionRetrySucceeded";
        internal const string FQIDNetworkFailureDetected = "PowerShellNetworkFailureDetected";
        internal const string FQIDNetworkOrDisconnectFailed = "PowerShellNetworkOrDisconnectFailed";

        internal PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus notification, string computerName, int maxRetryConnectionTime, object infoRecord)
        {
            this._notification = notification;
            this._computerName = computerName;
            this._maxRetryConnectionTime = maxRetryConnectionTime;
            this._infoRecord = infoRecord;
        }

        internal string ComputerName
        {
            get
            {
                return this._computerName;
            }
        }

        internal object InformationRecord
        {
            get
            {
                return this._infoRecord;
            }
        }

        internal int MaxRetryConnectionTime
        {
            get
            {
                return this._maxRetryConnectionTime;
            }
        }

        internal PSConnectionRetryStatus Notification
        {
            get
            {
                return this._notification;
            }
        }
    }
}

