using System;
using System.Threading;

namespace Microsoft.Management.Odata.Common
{
	internal class MonitorLock : IDisposable
	{
		private object lockedObject;

		private bool locked;

		public MonitorLock(object lockedObject)
		{
			this.lockedObject = lockedObject;
			Monitor.Enter(this.lockedObject);
			this.locked = true;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (this.locked)
			{
				Monitor.Exit(this.lockedObject);
				this.locked = false;
			}
		}

		public void Enter()
		{
			if (!this.locked)
			{
				Monitor.Enter(this.lockedObject);
				this.locked = true;
				return;
			}
			else
			{
				throw new LockRecursionException();
			}
		}

		public void Exit()
		{
			if (this.locked)
			{
				Monitor.Exit(this.lockedObject);
				this.locked = false;
				return;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
	}
}