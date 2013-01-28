using System;
using System.Timers;

namespace Microsoft.Management.Odata.Common
{
	internal class PswsTimer : IDisposable
	{
		private BoundedInteger timeout;

		private Timer timer;

		private ElapsedEventHandler elapsedEventHandler;

		private bool continuous;

		private bool disposed;

		public bool Enabled
		{
			get
			{
				return this.timer.Enabled;
			}
		}

		public int Timeout
		{
			get
			{
				return this.timeout.Value;
			}
			set
			{
				if (this.timeout.Value != value)
				{
					this.Stop();
					this.timeout.Value = value;
					this.timer = PswsTimer.CreateTimer(this.elapsedEventHandler, this.timeout.Value, this.continuous, true);
				}
			}
		}

		public PswsTimer(ElapsedEventHandler elapsedEventHandler, int timeout) : this(elapsedEventHandler, timeout, true, true)
		{
		}

		public PswsTimer(ElapsedEventHandler elapsedEventHandler, int timeout, bool continuous) : this(elapsedEventHandler, timeout, continuous, true)
		{
		}

		public PswsTimer(ElapsedEventHandler elapsedEventHandler, int timeout, bool continuous, bool startAtCreation)
		{
			this.timeout = new BoundedInteger(timeout, 1, 0x7fffffff);
			this.elapsedEventHandler = elapsedEventHandler;
			this.continuous = continuous;
			this.timer = PswsTimer.CreateTimer(elapsedEventHandler, this.timeout.Value, this.continuous, startAtCreation);
			this.disposed = false;
		}

		private static Timer CreateTimer(ElapsedEventHandler elapsedEventHandler, int timeout, bool continuous, bool startAtCreation)
		{
			Timer timer = new Timer((double)(timeout * 0x3e8));
			timer.Elapsed += elapsedEventHandler;
			timer.AutoReset = continuous;
			timer.Enabled = startAtCreation;
			return timer;
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
				this.timer.Stop();
				this.timer.Dispose();
			}
			this.disposed = true;
		}

		public void Start()
		{
			this.timer.Start();
		}

		public void Stop()
		{
			this.timer.Enabled = false;
			this.timer.Stop();
		}

		internal void TestHookFireTimer()
		{
			this.elapsedEventHandler(this, null);
		}

		internal void TestHookStopTimer()
		{
			this.Stop();
		}
	}
}