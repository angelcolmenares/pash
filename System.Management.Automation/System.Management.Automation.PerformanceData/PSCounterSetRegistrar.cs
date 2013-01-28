namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Runtime.InteropServices;

    public class PSCounterSetRegistrar : CounterSetRegistrarBase
    {
        public PSCounterSetRegistrar(PSCounterSetRegistrar srcPSCounterSetRegistrar) : base(srcPSCounterSetRegistrar)
        {
            if (srcPSCounterSetRegistrar == null)
            {
                throw new ArgumentNullException("srcPSCounterSetRegistrar");
            }
        }

        public PSCounterSetRegistrar(Guid providerId, Guid counterSetId, CounterSetInstanceType counterSetInstType, CounterInfo[] counterInfoArray, string counterSetName = null) : base(providerId, counterSetId, counterSetInstType, counterInfoArray, counterSetName)
        {
        }

        protected override CounterSetInstanceBase CreateCounterSetInstance()
        {
            return new PSCounterSetInstance(this);
        }

        public override void DisposeCounterSetInstance()
        {
            base._counterSetInstanceBase.Dispose();
        }
    }
}

