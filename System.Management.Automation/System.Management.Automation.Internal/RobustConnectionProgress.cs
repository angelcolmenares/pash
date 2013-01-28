namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Threading;

    internal class RobustConnectionProgress
    {
        private string _activity = RemotingErrorIdStrings.RCProgressActivity;
        private bool _progressIsRunning;
        private ProgressRecord _progressRecord;
        private PSHost _psHost;
        private int _secondsRemaining;
        private int _secondsTotal;
        private long _sourceId;
        private string _status;
        private object _syncObject = new object();
        private Timer _updateTimer;

        private void RemoveProgressBar()
        {
            this._progressIsRunning = false;
            this._progressRecord.RecordType = ProgressRecordType.Completed;
            this._psHost.UI.WriteProgress(0L, this._progressRecord);
            this._updateTimer.Dispose();
            this._updateTimer = null;
        }

        public void StartProgress(long sourceId, string computerName, int secondsTotal, PSHost psHost)
        {
            if ((psHost != null) && (secondsTotal >= 1))
            {
                if (string.IsNullOrEmpty(computerName))
                {
                    throw new ArgumentNullException("computerName");
                }
                lock (this._syncObject)
                {
                    if (!this._progressIsRunning)
                    {
                        this._progressIsRunning = true;
                        this._sourceId = sourceId;
                        this._secondsTotal = secondsTotal;
                        this._secondsRemaining = secondsTotal;
                        this._psHost = psHost;
                        this._status = StringUtil.Format(RemotingErrorIdStrings.RCProgressStatus, computerName);
                        this._progressRecord = new ProgressRecord(0, this._activity, this._status);
                        this._updateTimer = new Timer(new TimerCallback(this.UpdateCallback), null, TimeSpan.Zero, new TimeSpan(0, 0, 1));
                    }
                }
            }
        }

        public void StopProgress(long sourceId)
        {
            lock (this._syncObject)
            {
                if (((sourceId == this._sourceId) || (sourceId == 0L)) && this._progressIsRunning)
                {
                    this.RemoveProgressBar();
                }
            }
        }

        private void UpdateCallback(object state)
        {
            lock (this._syncObject)
            {
                if (this._progressIsRunning)
                {
                    if (this._secondsRemaining > 0)
                    {
                        this._progressRecord.PercentComplete = ((this._secondsTotal - this._secondsRemaining) * 100) / this._secondsTotal;
                        this._progressRecord.SecondsRemaining = this._secondsRemaining--;
                        this._progressRecord.RecordType = ProgressRecordType.Processing;
                        this._psHost.UI.WriteProgress(0L, this._progressRecord);
                    }
                    else
                    {
                        this.RemoveProgressBar();
                    }
                }
            }
        }
    }
}

