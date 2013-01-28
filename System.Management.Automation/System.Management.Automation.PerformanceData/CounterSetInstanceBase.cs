namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.PerformanceData;
    using System.Globalization;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;

    public abstract class CounterSetInstanceBase : IDisposable
    {
        protected ConcurrentDictionary<int, CounterType> _counterIdToTypeMapping;
        protected ConcurrentDictionary<string, int> _counterNameToIdMapping;
        protected CounterSetRegistrarBase _counterSetRegistrarBase;
        private readonly PowerShellTraceSource _tracer = PowerShellTraceSourceFactory.GetTraceSource();

        protected CounterSetInstanceBase(CounterSetRegistrarBase counterSetRegistrarInst)
        {
            this._counterSetRegistrarBase = counterSetRegistrarInst;
            this._counterNameToIdMapping = new ConcurrentDictionary<string, int>();
            this._counterIdToTypeMapping = new ConcurrentDictionary<int, CounterType>();
            CounterInfo[] counterInfoArray = this._counterSetRegistrarBase.CounterInfoArray;
            for (int i = 0; i < counterInfoArray.Length; i++)
            {
                this._counterIdToTypeMapping.TryAdd(counterInfoArray[i].Id, counterInfoArray[i].Type);
                if (!string.IsNullOrWhiteSpace(counterInfoArray[i].Name))
                {
                    this._counterNameToIdMapping.TryAdd(counterInfoArray[i].Name, counterInfoArray[i].Id);
                }
            }
        }

        public abstract void Dispose();
        public abstract bool GetCounterValue(int counterId, bool isNumerator, out long counterValue);
        public abstract bool GetCounterValue(string counterName, bool isNumerator, out long counterValue);
        protected bool RetrieveTargetCounterIdIfValid(int counterId, bool isNumerator, out int targetCounterId)
        {
            targetCounterId = counterId;
            if (isNumerator)
            {
                goto Label_00C2;
            }
            bool flag = false;
            CounterType type2 = this._counterIdToTypeMapping[counterId];
            if (type2 <= CounterType.MultiTimerPercentageActive)
            {
                switch (type2)
                {
                    case CounterType.SampleFraction:
                    case CounterType.MultiTimerPercentageActive:
                    case CounterType.RawFraction32:
                    case CounterType.RawFraction64:
                        goto Label_0080;
                }
                goto Label_0082;
            }
            if (type2 <= CounterType.MultiTimerPercentageNotActive)
            {
                switch (type2)
                {
                    case CounterType.MultiTimerPercentageActive100Ns:
                    case CounterType.MultiTimerPercentageNotActive:
                        goto Label_0080;
                }
                goto Label_0082;
            }
            if (((type2 != CounterType.MultiTimerPercentageNotActive100Ns) && (type2 != CounterType.AverageTimer32)) && (type2 != CounterType.AverageCount64))
            {
                goto Label_0082;
            }
        Label_0080:
            flag = true;
        Label_0082:
            if (!flag)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Denominator for update not valid for the given counter id {0}", new object[] { counterId }));
                this._tracer.TraceException(exception);
                return false;
            }
            targetCounterId = counterId + 1;
        Label_00C2:
            return true;
        }

        public abstract bool SetCounterValue(int counterId, long counterValue, bool isNumerator);
        public abstract bool SetCounterValue(string counterName, long counterValue, bool isNumerator);
        public abstract bool UpdateCounterByValue(int counterId, long stepAmount, bool isNumerator);
        public abstract bool UpdateCounterByValue(string counterName, long stepAmount, bool isNumerator);
    }
}

