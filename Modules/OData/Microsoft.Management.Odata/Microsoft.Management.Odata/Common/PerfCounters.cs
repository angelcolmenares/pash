using System;
using System.Diagnostics.PerformanceData;

namespace Microsoft.Management.Odata.Common
{
	internal class PerfCounters : IDisposable
	{
		private const int ActiveRequestsCounterId = 1;

		private const int ActiveRunspacesCounterId = 2;

		private const int ActiveUsersCounterId = 3;

		private const int UserQuotaViolationsPerSecCounterId = 4;

		private const int SystemQuotaViolationsPerSecCounterId = 5;

		private Guid providerGuid;

		private Guid providerCounterSetGuid;

		private bool disposed;

		private CounterSet counterSet;

		private CounterSetInstance counterSetInstance;

		public PerfCounter ActiveRequests
		{
			get;
			private set;
		}

		public PerfCounter ActiveRunspaces
		{
			get;
			private set;
		}

		public PerfCounter ActiveUsers
		{
			get;
			private set;
		}

		public PerfCounter SystemQuotaViolationsPerSec
		{
			get;
			private set;
		}

		public PerfCounter UserQuotaViolationsPerSec
		{
			get;
			private set;
		}

		public PerfCounters(Uri instanceName)
		{
			this.providerGuid = new Guid("{b0f9d01b-71f3-4d7d-b69e-5d1c5932b74d}");
			this.providerCounterSetGuid = new Guid("{e711142e-c6b7-41a9-ac1a-aa63c936cd55}");
			this.counterSet = new CounterSet(this.providerGuid, this.providerCounterSetGuid, CounterSetInstanceType.Multiple);
			this.counterSet.AddCounter(1, CounterType.RawData32);
			this.counterSet.AddCounter(2, CounterType.RawData32);
			this.counterSet.AddCounter(3, CounterType.RawData32);
			this.counterSet.AddCounter(4, CounterType.RateOfCountPerSecond32);
			this.counterSet.AddCounter(5, CounterType.RateOfCountPerSecond32);
			this.counterSetInstance = this.CreateInstance(instanceName);
		}

		public CounterSetInstance CreateInstance(Uri instanceName)
		{
			TraceHelper.Current.DebugMessage(string.Concat("Counter instance Url = ", instanceName.ToString()));
			string str = instanceName.ToString().Replace("/", "_");
			CounterSetInstance counterSetInstance = this.counterSet.CreateCounterSetInstance(str);
			this.ActiveRequests = new PerfCounter(counterSetInstance, 1);
			this.ActiveRunspaces = new PerfCounter(counterSetInstance, 2);
			this.ActiveUsers = new PerfCounter(counterSetInstance, 3);
			this.UserQuotaViolationsPerSec = new PerfCounter(counterSetInstance, 4);
			this.SystemQuotaViolationsPerSec = new PerfCounter(counterSetInstance, 5);
			return counterSetInstance;
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (disposeManagedResources && !this.disposed)
			{
				if (this.counterSetInstance != null)
				{
					this.counterSetInstance.Dispose();
					this.counterSetInstance = null;
				}
				if (this.counterSet != null)
				{
					this.counterSet.Dispose();
					this.counterSet = null;
				}
				this.disposed = true;
			}
		}
	}
}