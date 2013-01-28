using System;

namespace Microsoft.Management.Odata.Common
{
	internal class LockedPerSecCounter : PerSecCounter
	{
		private object syncObject;

		public override int Value
		{
			get
			{
				int value;
				lock (this.syncObject)
				{
					value = base.Value;
				}
				return value;
			}
		}

		public LockedPerSecCounter()
		{
			this.syncObject = new object();
		}

		public override void Increment()
		{
			lock (this.syncObject)
			{
				base.Increment();
			}
		}
	}
}