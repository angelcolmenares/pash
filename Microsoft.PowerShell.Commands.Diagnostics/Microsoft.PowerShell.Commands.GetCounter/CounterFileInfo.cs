namespace Microsoft.PowerShell.Commands.GetCounter
{
    using System;

    public class CounterFileInfo
    {
        private DateTime _newestRecord;
        private DateTime _oldestRecord;
        private long _sampleCount;

        internal CounterFileInfo()
        {
            this._oldestRecord = DateTime.MinValue;
            this._newestRecord = DateTime.MaxValue;
        }

        internal CounterFileInfo(DateTime oldestRecord, DateTime newestRecord, long sampleCount)
        {
            this._oldestRecord = DateTime.MinValue;
            this._newestRecord = DateTime.MaxValue;
            this._oldestRecord = oldestRecord;
            this._newestRecord = newestRecord;
            this._sampleCount = sampleCount;
        }

        public DateTime NewestRecord
        {
            get
            {
                return this._newestRecord;
            }
        }

        public DateTime OldestRecord
        {
            get
            {
                return this._oldestRecord;
            }
        }

        public long SampleCount
        {
            get
            {
                return this._sampleCount;
            }
        }
    }
}

