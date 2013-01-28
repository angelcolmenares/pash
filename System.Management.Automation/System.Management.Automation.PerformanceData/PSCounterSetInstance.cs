namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.PerformanceData;
    using System.Globalization;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;

    public class PSCounterSetInstance : CounterSetInstanceBase
    {
        private CounterSet _CounterSet;
        private CounterSetInstance _CounterSetInstance;
        private bool _Disposed;
        private readonly PowerShellTraceSource _tracer;

        public PSCounterSetInstance(CounterSetRegistrarBase counterSetRegBaseObj) : base(counterSetRegBaseObj)
        {
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this.CreateCounterSetInstance();
        }

        private void CreateCounterSetInstance()
        {
            this._CounterSet = new CounterSet(base._counterSetRegistrarBase.ProviderId, base._counterSetRegistrarBase.CounterSetId, base._counterSetRegistrarBase.CounterSetInstType);
            foreach (CounterInfo info in base._counterSetRegistrarBase.CounterInfoArray)
            {
                if (info.Name == null)
                {
                    this._CounterSet.AddCounter(info.Id, info.Type);
                }
                else
                {
                    this._CounterSet.AddCounter(info.Id, info.Type, info.Name);
                }
            }
            string counterSetInstanceName = PSPerfCountersMgr.Instance.GetCounterSetInstanceName();
            this._CounterSetInstance = this._CounterSet.CreateCounterSetInstance(counterSetInstanceName);
        }

        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this._CounterSetInstance.Dispose();
                    this._CounterSet.Dispose();
                }
                this._Disposed = true;
            }
        }

        ~PSCounterSetInstance()
        {
            this.Dispose(false);
        }

        public override bool GetCounterValue(int counterId, bool isNumerator, out long counterValue)
        {
            int num;
            counterValue = -1L;
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (base.RetrieveTargetCounterIdIfValid(counterId, isNumerator, out num))
            {
                CounterData data = this._CounterSetInstance.Counters[num];
                if (data != null)
                {
                    counterValue = data.Value;
                    return true;
                }
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter id {0} failed", new object[] { counterId }));
                this._tracer.TraceException(exception2);
            }
            return false;
        }

        public override bool GetCounterValue(string counterName, bool isNumerator, out long counterValue)
        {
            counterValue = -1L;
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (counterName == null)
            {
                ArgumentNullException exception2 = new ArgumentNullException("counterName");
                this._tracer.TraceException(exception2);
                return false;
            }
            try
            {
                int counterId = base._counterNameToIdMapping[counterName];
                return this.GetCounterValue(counterId, isNumerator, out counterValue);
            }
            catch (KeyNotFoundException)
            {
                InvalidOperationException exception3 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter name {0} failed", new object[] { counterName }));
                this._tracer.TraceException(exception3);
                return false;
            }
        }

        public override bool SetCounterValue(int counterId, long counterValue, bool isNumerator)
        {
            int num;
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (base.RetrieveTargetCounterIdIfValid(counterId, isNumerator, out num))
            {
                CounterData data = this._CounterSetInstance.Counters[num];
                if (data != null)
                {
                    data.Value = counterValue;
                    return true;
                }
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter id {0} failed", new object[] { counterId }));
                this._tracer.TraceException(exception2);
            }
            return false;
        }

        public override bool SetCounterValue(string counterName, long counterValue, bool isNumerator)
        {
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (counterName == null)
            {
                ArgumentNullException exception2 = new ArgumentNullException("counterName");
                this._tracer.TraceException(exception2);
                return false;
            }
            try
            {
                int counterId = base._counterNameToIdMapping[counterName];
                return this.SetCounterValue(counterId, counterValue, isNumerator);
            }
            catch (KeyNotFoundException)
            {
                InvalidOperationException exception3 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter name {0} failed", new object[] { counterName }));
                this._tracer.TraceException(exception3);
                return false;
            }
        }

        private void UpdateCounterByValue(CounterData TargetCounterData, long stepAmount)
        {
            if (stepAmount == -1L)
            {
                TargetCounterData.Decrement();
            }
            else if (stepAmount == 1L)
            {
                TargetCounterData.Increment();
            }
            else
            {
                TargetCounterData.IncrementBy(stepAmount);
            }
        }

        public override bool UpdateCounterByValue(int counterId, long stepAmount, bool isNumerator)
        {
            int num;
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (base.RetrieveTargetCounterIdIfValid(counterId, isNumerator, out num))
            {
                CounterData targetCounterData = this._CounterSetInstance.Counters[num];
                if (targetCounterData != null)
                {
                    this.UpdateCounterByValue(targetCounterData, stepAmount);
                    return true;
                }
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter id {0} failed", new object[] { counterId }));
                this._tracer.TraceException(exception2);
            }
            return false;
        }

        public override bool UpdateCounterByValue(string counterName, long stepAmount, bool isNumerator)
        {
            if (this._Disposed)
            {
                ObjectDisposedException exception = new ObjectDisposedException("PSCounterSetInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            if (counterName == null)
            {
                ArgumentNullException exception2 = new ArgumentNullException("counterName");
                this._tracer.TraceException(exception2);
                return false;
            }
            try
            {
                int counterId = base._counterNameToIdMapping[counterName];
                return this.UpdateCounterByValue(counterId, stepAmount, isNumerator);
            }
            catch (KeyNotFoundException)
            {
                InvalidOperationException exception3 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lookup for counter corresponding to counter name {0} failed", new object[] { counterName }));
                this._tracer.TraceException(exception3);
                return false;
            }
        }
    }
}

