namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;

    public class PSPerfCountersMgr
    {
        private ConcurrentDictionary<Guid, CounterSetInstanceBase> _CounterSetIdToInstanceMapping = new ConcurrentDictionary<Guid, CounterSetInstanceBase>();
        private ConcurrentDictionary<string, Guid> _CounterSetNameToIdMapping = new ConcurrentDictionary<string, Guid>();
        private static PSPerfCountersMgr _PSPerfCountersMgrInstance;
        private readonly PowerShellTraceSource _tracer = PowerShellTraceSourceFactory.GetTraceSource();

        private PSPerfCountersMgr()
        {
        }

        public bool AddCounterSetInstance(CounterSetRegistrarBase counterSetRegistrarInstance)
        {
            if (counterSetRegistrarInstance == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetRegistrarInstance");
                this._tracer.TraceException(exception);
                return false;
            }
            Guid counterSetId = counterSetRegistrarInstance.CounterSetId;
            string counterSetName = counterSetRegistrarInstance.CounterSetName;
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
            {
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A Counter Set Instance with id '{0}' is already registered", new object[] { counterSetId }));
                this._tracer.TraceException(exception2);
                return false;
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(counterSetName))
                {
                    Guid guid2;
                    if (this.IsCounterSetRegistered(counterSetName, out guid2))
                    {
                        InvalidOperationException exception3 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A Counter Set Instance with name '{0}' is already registered", new object[] { counterSetName }));
                        this._tracer.TraceException(exception3);
                        return false;
                    }
                    this._CounterSetNameToIdMapping.TryAdd(counterSetName, counterSetId);
                }
                this._CounterSetIdToInstanceMapping.TryAdd(counterSetId, counterSetRegistrarInstance.CounterSetInstance);
            }
            catch (OverflowException exception4)
            {
                this._tracer.TraceException(exception4);
                return false;
            }
            return true;
        }

        ~PSPerfCountersMgr()
        {
            this.RemoveAllCounterSets();
        }

        public string GetCounterSetInstanceName()
        {
            Process currentProcess = Process.GetCurrentProcess();
            return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { currentProcess.Id });
        }

        public bool IsCounterSetRegistered(Guid counterSetId, out CounterSetInstanceBase counterSetInst)
        {
            return this._CounterSetIdToInstanceMapping.TryGetValue(counterSetId, out counterSetInst);
        }

        public bool IsCounterSetRegistered(string counterSetName, out Guid counterSetId)
        {
            counterSetId = new Guid();
            if (counterSetName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetName");
                this._tracer.TraceException(exception);
                return false;
            }
            return this._CounterSetNameToIdMapping.TryGetValue(counterSetName, out counterSetId);
        }

        internal void RemoveAllCounterSets()
        {
            foreach (Guid guid in this._CounterSetIdToInstanceMapping.Keys)
            {
                this._CounterSetIdToInstanceMapping[guid].Dispose();
            }
            this._CounterSetIdToInstanceMapping.Clear();
            this._CounterSetNameToIdMapping.Clear();
        }

        public bool SetCounterValue(Guid counterSetId, int counterId, long counterValue = 1L, bool isNumerator = true)
        {
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
            {
                return counterSetInst.SetCounterValue(counterId, counterValue, isNumerator);
            }
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with id '{0}' is registered", new object[] { counterSetId }));
            this._tracer.TraceException(exception);
            return false;
        }

        public bool SetCounterValue(Guid counterSetId, string counterName, long counterValue = 1L, bool isNumerator = true)
        {
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
            {
                return counterSetInst.SetCounterValue(counterName, counterValue, isNumerator);
            }
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with id '{0}' is registered", new object[] { counterSetId }));
            this._tracer.TraceException(exception);
            return false;
        }

        public bool SetCounterValue(string counterSetName, int counterId, long counterValue = 1L, bool isNumerator = true)
        {
            Guid guid;
            if (counterSetName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetName");
                this._tracer.TraceException(exception);
                return false;
            }
            if (this.IsCounterSetRegistered(counterSetName, out guid))
            {
                CounterSetInstanceBase base2 = this._CounterSetIdToInstanceMapping[guid];
                return base2.SetCounterValue(counterId, counterValue, isNumerator);
            }
            InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with name '{0}' is registered", new object[] { counterSetName }));
            this._tracer.TraceException(exception2);
            return false;
        }

        public bool SetCounterValue(string counterSetName, string counterName, long counterValue = 1L, bool isNumerator = true)
        {
            Guid guid;
            if (counterSetName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetName");
                this._tracer.TraceException(exception);
                return false;
            }
            if (this.IsCounterSetRegistered(counterSetName, out guid))
            {
                CounterSetInstanceBase base2 = this._CounterSetIdToInstanceMapping[guid];
                return base2.SetCounterValue(counterName, counterValue, isNumerator);
            }
            InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with name '{0}' is registered", new object[] { counterSetName }));
            this._tracer.TraceException(exception2);
            return false;
        }

        public bool UpdateCounterByValue(Guid counterSetId, int counterId, long stepAmount = 1L, bool isNumerator = true)
        {
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
            {
                return counterSetInst.UpdateCounterByValue(counterId, stepAmount, isNumerator);
            }
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with id '{0}' is registered", new object[] { counterSetId }));
            this._tracer.TraceException(exception);
            return false;
        }

        public bool UpdateCounterByValue(Guid counterSetId, string counterName, long stepAmount = 1L, bool isNumerator = true)
        {
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
            {
                return counterSetInst.UpdateCounterByValue(counterName, stepAmount, isNumerator);
            }
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with id '{0}' is registered", new object[] { counterSetId }));
            this._tracer.TraceException(exception);
            return false;
        }

        public bool UpdateCounterByValue(string counterSetName, int counterId, long stepAmount = 1L, bool isNumerator = true)
        {
            Guid guid;
            if (counterSetName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetName");
                this._tracer.TraceException(exception);
                return false;
            }
            if (this.IsCounterSetRegistered(counterSetName, out guid))
            {
                CounterSetInstanceBase base2 = this._CounterSetIdToInstanceMapping[guid];
                return base2.UpdateCounterByValue(counterId, stepAmount, isNumerator);
            }
            InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with id '{0}' is registered", new object[] { guid }));
            this._tracer.TraceException(exception2);
            return false;
        }

        public bool UpdateCounterByValue(string counterSetName, string counterName, long stepAmount = 1L, bool isNumerator = true)
        {
            Guid guid;
            if (counterSetName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("counterSetName");
                this._tracer.TraceException(exception);
                return false;
            }
            if (this.IsCounterSetRegistered(counterSetName, out guid))
            {
                CounterSetInstanceBase base2 = this._CounterSetIdToInstanceMapping[guid];
                return base2.UpdateCounterByValue(counterName, stepAmount, isNumerator);
            }
            InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No Counter Set Instance with name {0} is registered", new object[] { counterSetName }));
            this._tracer.TraceException(exception2);
            return false;
        }

        public static PSPerfCountersMgr Instance
        {
            get
            {
                if (_PSPerfCountersMgrInstance == null)
                {
                    _PSPerfCountersMgrInstance = new PSPerfCountersMgr();
                }
                return _PSPerfCountersMgrInstance;
            }
        }
    }
}

