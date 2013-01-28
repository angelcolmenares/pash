namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Runtime.InteropServices;

    public abstract class CounterSetRegistrarBase
    {
        private readonly CounterInfo[] _counterInfoArray;
        private readonly Guid _counterSetId;
        protected CounterSetInstanceBase _counterSetInstanceBase;
        private readonly CounterSetInstanceType _counterSetInstanceType;
        private readonly string _counterSetName;
        private readonly Guid _providerId;

        protected CounterSetRegistrarBase(CounterSetRegistrarBase srcCounterSetRegistrarBase)
        {
            if (srcCounterSetRegistrarBase == null)
            {
                throw new ArgumentNullException("srcCounterSetRegistrarBase");
            }
            this._providerId = srcCounterSetRegistrarBase._providerId;
            this._counterSetId = srcCounterSetRegistrarBase._counterSetId;
            this._counterSetInstanceType = srcCounterSetRegistrarBase._counterSetInstanceType;
            this._counterSetName = srcCounterSetRegistrarBase._counterSetName;
            CounterInfo[] infoArray = srcCounterSetRegistrarBase._counterInfoArray;
            this._counterInfoArray = new CounterInfo[infoArray.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                this._counterInfoArray[i] = new CounterInfo(infoArray[i].Id, infoArray[i].Type, infoArray[i].Name);
            }
        }

        protected CounterSetRegistrarBase(Guid providerId, Guid counterSetId, CounterSetInstanceType counterSetInstType, CounterInfo[] counterInfoArray, string counterSetName = null)
        {
            this._providerId = providerId;
            this._counterSetId = counterSetId;
            this._counterSetInstanceType = counterSetInstType;
            this._counterSetName = counterSetName;
            if ((counterInfoArray == null) || (counterInfoArray.Length == 0))
            {
                throw new ArgumentNullException("counterInfoArray");
            }
            this._counterInfoArray = new CounterInfo[counterInfoArray.Length];
            for (int i = 0; i < counterInfoArray.Length; i++)
            {
                this._counterInfoArray[i] = new CounterInfo(counterInfoArray[i].Id, counterInfoArray[i].Type, counterInfoArray[i].Name);
            }
            this._counterSetInstanceBase = null;
        }

        protected abstract CounterSetInstanceBase CreateCounterSetInstance();
        public abstract void DisposeCounterSetInstance();

        public CounterInfo[] CounterInfoArray
        {
            get
            {
                return this._counterInfoArray;
            }
        }

        public Guid CounterSetId
        {
            get
            {
                return this._counterSetId;
            }
        }

        public CounterSetInstanceBase CounterSetInstance
        {
            get
            {
                if (this._counterSetInstanceBase == null)
                {
                    this._counterSetInstanceBase = this.CreateCounterSetInstance();
                }
                return this._counterSetInstanceBase;
            }
        }

        public CounterSetInstanceType CounterSetInstType
        {
            get
            {
                return this._counterSetInstanceType;
            }
        }

        public string CounterSetName
        {
            get
            {
                return this._counterSetName;
            }
        }

        public Guid ProviderId
        {
            get
            {
                return this._providerId;
            }
        }
    }
}

