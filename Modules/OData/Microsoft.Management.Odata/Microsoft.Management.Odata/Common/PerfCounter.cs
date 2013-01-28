using System;
using System.Diagnostics.PerformanceData;

namespace Microsoft.Management.Odata.Common
{
	internal class PerfCounter
	{
		private int counterId;

		private CounterSetInstance counterSetInstance;

		public PerfCounter(CounterSetInstance counterSetInstance, int counterId)
		{
			this.counterId = counterId;
			this.counterSetInstance = counterSetInstance;
		}

		public void Decrement()
		{
			object[] value = new object[4];
			value[0] = "Counter Id ";
			value[1] = this.counterId;
			value[2] = "\nBefore decrementing value ";
			value[3] = this.counterSetInstance.Counters[this.counterId].Value;
			TraceHelper.Current.DebugMessage(string.Concat(value));
			this.counterSetInstance.Counters[this.counterId].Decrement();
			object[] objArray = new object[4];
			objArray[0] = "Counter Id ";
			objArray[1] = this.counterId;
			objArray[2] = "\nAfter decrementing value ";
			objArray[3] = this.counterSetInstance.Counters[this.counterId].Value;
			TraceHelper.Current.DebugMessage(string.Concat(objArray));
		}

		public void Increment()
		{
			object[] value = new object[4];
			value[0] = "Counter Id ";
			value[1] = this.counterId;
			value[2] = "\nBefore incrementing value ";
			value[3] = this.counterSetInstance.Counters[this.counterId].Value;
			TraceHelper.Current.DebugMessage(string.Concat(value));
			this.counterSetInstance.Counters[this.counterId].Increment();
			object[] objArray = new object[4];
			objArray[0] = "Counter Id ";
			objArray[1] = this.counterId;
			objArray[2] = "\nAfter incrementing value ";
			objArray[3] = this.counterSetInstance.Counters[this.counterId].Value;
			TraceHelper.Current.DebugMessage(string.Concat(objArray));
		}
	}
}