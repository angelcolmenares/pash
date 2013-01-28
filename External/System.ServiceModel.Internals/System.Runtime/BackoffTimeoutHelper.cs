using System;
using System.Threading;

namespace System.Runtime
{
	internal sealed class BackoffTimeoutHelper
	{
		private readonly static int maxSkewMilliseconds;

		private readonly static long maxDriftTicks;

		private readonly static TimeSpan defaultInitialWaitTime;

		private readonly static TimeSpan defaultMaxWaitTime;

		private DateTime deadline;

		private TimeSpan maxWaitTime;

		private TimeSpan waitTime;

		private IOThreadTimer backoffTimer;

		private Action<object> backoffCallback;

		private object backoffState;

		private Random random;

		private TimeSpan originalTimeout;

		public TimeSpan OriginalTimeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.originalTimeout;
			}
		}

		static BackoffTimeoutHelper()
		{
			BackoffTimeoutHelper.maxSkewMilliseconds = (int)(IOThreadTimer.SystemTimeResolutionTicks / (long)0x2710);
			BackoffTimeoutHelper.maxDriftTicks = IOThreadTimer.SystemTimeResolutionTicks * (long)2;
			BackoffTimeoutHelper.defaultInitialWaitTime = TimeSpan.FromMilliseconds(1);
			BackoffTimeoutHelper.defaultMaxWaitTime = TimeSpan.FromMinutes(1);
		}

		internal BackoffTimeoutHelper(TimeSpan timeout) : this(timeout, BackoffTimeoutHelper.defaultMaxWaitTime)
		{
		}

		internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime) : this(timeout, maxWaitTime, BackoffTimeoutHelper.defaultInitialWaitTime)
		{
		}

		internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime, TimeSpan initialWaitTime)
		{
			this.random = new Random(this.GetHashCode());
			this.maxWaitTime = maxWaitTime;
			this.originalTimeout = timeout;
			this.Reset(timeout, initialWaitTime);
		}

		private void Backoff()
		{
			if (this.waitTime.Ticks < this.maxWaitTime.Ticks / (long)2)
			{
				this.waitTime = TimeSpan.FromTicks(this.waitTime.Ticks * (long)2);
			}
			else
			{
				this.waitTime = this.maxWaitTime;
			}
			if (this.deadline != DateTime.MaxValue)
			{
				TimeSpan utcNow = this.deadline - DateTime.UtcNow;
				if (this.waitTime > utcNow)
				{
					this.waitTime = utcNow;
					if (this.waitTime < TimeSpan.Zero)
					{
						this.waitTime = TimeSpan.Zero;
					}
				}
			}
		}

		public bool IsExpired()
		{
			if (this.deadline != DateTime.MaxValue)
			{
				return DateTime.UtcNow >= this.deadline;
			}
			else
			{
				return false;
			}
		}

		private void Reset(TimeSpan timeout, TimeSpan initialWaitTime)
		{
			if (timeout != TimeSpan.MaxValue)
			{
				this.deadline = DateTime.UtcNow + timeout;
			}
			else
			{
				this.deadline = DateTime.MaxValue;
			}
			this.waitTime = initialWaitTime;
		}

		public void WaitAndBackoff(Action<object> callback, object state)
		{
			if (this.backoffCallback != callback || this.backoffState != state)
			{
				if (this.backoffTimer != null)
				{
					this.backoffTimer.Cancel();
				}
				this.backoffCallback = callback;
				this.backoffState = state;
				this.backoffTimer = new IOThreadTimer(callback, state, false, BackoffTimeoutHelper.maxSkewMilliseconds);
			}
			TimeSpan timeSpan = this.WaitTimeWithDrift();
			this.Backoff();
			this.backoffTimer.Set(timeSpan);
		}

		public void WaitAndBackoff()
		{
			Thread.Sleep(this.WaitTimeWithDrift());
			this.Backoff();
		}

		private TimeSpan WaitTimeWithDrift()
		{
			return Ticks.ToTimeSpan(Math.Max(Ticks.FromTimeSpan(BackoffTimeoutHelper.defaultInitialWaitTime), Ticks.Add(Ticks.FromTimeSpan(this.waitTime), (long)this.random.Next() % ((long)2 * BackoffTimeoutHelper.maxDriftTicks + (long)1) - BackoffTimeoutHelper.maxDriftTicks)));
		}
	}
}