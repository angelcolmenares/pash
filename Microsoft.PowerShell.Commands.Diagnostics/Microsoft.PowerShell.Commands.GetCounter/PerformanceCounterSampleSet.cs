namespace Microsoft.PowerShell.Commands.GetCounter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Resources;

    public class PerformanceCounterSampleSet
    {
        private PerformanceCounterSample[] _counterSamples;
        private ResourceManager _resourceMgr;
        private DateTime _timeStamp;

        internal PerformanceCounterSampleSet()
        {
            this._timeStamp = DateTime.MinValue;
            this._resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
        }

        internal PerformanceCounterSampleSet(DateTime timeStamp, PerformanceCounterSample[] counterSamples, bool firstSet) : this()
        {
            this._timeStamp = timeStamp;
            this._counterSamples = counterSamples;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSample.CounterSamples", Justification="A string[] is required here because that is the type Powershell supports")]
        public PerformanceCounterSample[] CounterSamples
        {
            get
            {
                return this._counterSamples;
            }
            set
            {
                this._counterSamples = value;
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
    }
}

