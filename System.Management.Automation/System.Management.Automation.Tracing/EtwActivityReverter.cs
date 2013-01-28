namespace System.Management.Automation.Tracing
{
    using System;

    internal class EtwActivityReverter : IEtwActivityReverter, IDisposable
    {
        private readonly IEtwEventCorrelator _correlator;
        private bool _isDisposed;
        private readonly Guid _oldActivityId;

        public EtwActivityReverter(IEtwEventCorrelator correlator, Guid oldActivityId)
        {
            this._correlator = correlator;
            this._oldActivityId = oldActivityId;
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this._correlator.CurrentActivityId = this._oldActivityId;
                this._isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void RevertCurrentActivityId()
        {
            this.Dispose();
        }
    }
}

