namespace Microsoft.PowerShell.Commands.GetCounter
{
    using System;
    using System.Diagnostics;

    public class PerformanceCounterSample
    {
        private double _cookedValue;
        private PerformanceCounterType _counterType;
        private long _defaultScale;
        private string _instanceName;
        private long _multiCount;
        private string _path;
        private ulong _rawValue;
        private ulong _secondValue;
        private long _status;
        private ulong _timeBase;
        private DateTime _timeStamp;
        private ulong _timeStamp100nSec;

        internal PerformanceCounterSample()
        {
            this._path = "";
            this._instanceName = "";
            this._timeStamp = DateTime.MinValue;
        }

        internal PerformanceCounterSample(string path, string instanceName, double cookedValue, ulong rawValue, ulong secondValue, long multiCount, PerformanceCounterType counterType, long defaultScale, ulong timeBase, DateTime timeStamp, ulong timeStamp100nSec, long status)
        {
            this._path = "";
            this._instanceName = "";
            this._timeStamp = DateTime.MinValue;
            this._path = path;
            this._instanceName = instanceName;
            this._cookedValue = cookedValue;
            this._rawValue = rawValue;
            this._secondValue = secondValue;
            this._multiCount = multiCount;
            this._counterType = counterType;
            this._defaultScale = defaultScale;
            this._timeBase = timeBase;
            this._timeStamp = timeStamp;
            this._timeStamp100nSec = timeStamp100nSec;
            this._status = status;
        }

        public double CookedValue
        {
            get
            {
                return this._cookedValue;
            }
            set
            {
                this._cookedValue = value;
            }
        }

        public PerformanceCounterType CounterType
        {
            get
            {
                return this._counterType;
            }
            set
            {
                this._counterType = value;
            }
        }

        public long DefaultScale
        {
            get
            {
                return this._defaultScale;
            }
            set
            {
                this._defaultScale = value;
            }
        }

        public string InstanceName
        {
            get
            {
                return this._instanceName;
            }
            set
            {
                this._instanceName = value;
            }
        }

        public long MultipleCount
        {
            get
            {
                return this._multiCount;
            }
            set
            {
                this._multiCount = value;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        public ulong RawValue
        {
            get
            {
                return this._rawValue;
            }
            set
            {
                this._rawValue = value;
            }
        }

        public ulong SecondValue
        {
            get
            {
                return this._secondValue;
            }
            set
            {
                this._secondValue = value;
            }
        }

        public long Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
            }
        }

        public ulong TimeBase
        {
            get
            {
                return this._timeBase;
            }
            set
            {
                this._timeBase = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return this._timeStamp;
            }
            set
            {
                this._timeStamp = value;
            }
        }

        public ulong Timestamp100NSec
        {
            get
            {
                return this._timeStamp100nSec;
            }
            set
            {
                this._timeStamp100nSec = value;
            }
        }
    }
}

