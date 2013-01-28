using System;
using System.Collections.Generic;
using System.Timers;

namespace Microsoft.Management.Odata.Common
{
	internal class CacheController : IDisposable
	{
		private HashSet<Cache> caches;

		private PswsTimer timer;

		private object syncObject;

		private bool disposed;

		public CacheController(int timeout)
		{
			this.syncObject = new object();
			this.timer = new PswsTimer(new ElapsedEventHandler(this.OnElapsedEvent), timeout);
			this.caches = new HashSet<Cache>();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this.disposed && disposeManagedResources && this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}
			this.disposed = true;
		}

		private void OnElapsedEvent(object source, EventArgs e)
		{
			HashSet<Cache> caches;
			lock (this.syncObject)
			{
				caches = new HashSet<Cache>(this.caches);
			}
			DateTime now = DateTimeHelper.Now - new TimeSpan(0, 0, this.timer.Timeout);
			foreach (Cache cach in caches)
			{
				cach.DoCleanup(now);
			}
		}

		public void RegisterCache(Cache cache)
		{
			lock (this.syncObject)
			{
				this.caches.Add(cache);
			}
		}

		internal PswsTimer TestHookGetTimer()
		{
			return this.timer;
		}

		public void UnregisterCache(Cache cache)
		{
			lock (this.syncObject)
			{
				this.caches.Remove(cache);
			}
		}
	}
}