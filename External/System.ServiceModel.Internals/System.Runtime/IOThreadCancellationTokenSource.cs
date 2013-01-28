using System;
using System.Threading;

namespace System.Runtime
{
	internal class IOThreadCancellationTokenSource : IDisposable
	{
		private readonly static Action<object> onCancel;

		private readonly TimeSpan timeout;

		private CancellationTokenSource source;

		private CancellationToken? token;

		private IOThreadTimer timer;

		public CancellationToken Token
		{
			get
			{
				if (!this.token.HasValue)
				{
					if (this.timeout < TimeoutHelper.MaxWait)
					{
						this.timer = new IOThreadTimer(IOThreadCancellationTokenSource.onCancel, this, true);
						this.source = new CancellationTokenSource();
						this.timer.Set(this.timeout);
						this.token = new CancellationToken?(this.source.Token);
					}
					else
					{
						this.token = new CancellationToken?(CancellationToken.None);
					}
				}
				return this.token.Value;
			}
		}

		static IOThreadCancellationTokenSource()
		{
			IOThreadCancellationTokenSource.onCancel = Fx.ThunkCallback<object>(new Action<object>(IOThreadCancellationTokenSource.OnCancel));
		}

		public IOThreadCancellationTokenSource(TimeSpan timeout)
		{
			TimeoutHelper.ThrowIfNegativeArgument(timeout);
			this.timeout = timeout;
		}

		public IOThreadCancellationTokenSource(int timeout) : this(TimeSpan.FromMilliseconds((double)timeout))
		{
		}

		private void Cancel()
		{
			this.source.Cancel();
			this.source.Dispose();
			this.source = null;
		}

		public void Dispose()
		{
			if (this.source != null && this.timer.Cancel())
			{
				this.source.Dispose();
				this.source = null;
			}
		}

		private static void OnCancel(object obj)
		{
			IOThreadCancellationTokenSource oThreadCancellationTokenSource = (IOThreadCancellationTokenSource)obj;
			oThreadCancellationTokenSource.Cancel();
		}
	}
}