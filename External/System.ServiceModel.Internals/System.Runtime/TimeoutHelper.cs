using System;
using System.Threading;

namespace System.Runtime
{
	internal struct TimeoutHelper
	{
		private DateTime deadline;

		private bool deadlineSet;

		private TimeSpan originalTimeout;

		public readonly static TimeSpan MaxWait;

		public TimeSpan OriginalTimeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.originalTimeout;
			}
		}

		static TimeoutHelper()
		{
			TimeoutHelper.MaxWait = TimeSpan.FromMilliseconds(2147483647);
		}

		public TimeoutHelper(TimeSpan timeout)
		{
			this.originalTimeout = timeout;
			this.deadline = DateTime.MaxValue;
			this.deadlineSet = timeout == TimeSpan.MaxValue;
		}

		public static TimeSpan Add(TimeSpan timeout1, TimeSpan timeout2)
		{
			return Ticks.ToTimeSpan(Ticks.Add(Ticks.FromTimeSpan(timeout1), Ticks.FromTimeSpan(timeout2)));
		}

		public static DateTime Add(DateTime time, TimeSpan timeout)
		{
			if (!(timeout >= TimeSpan.Zero) || !(DateTime.MaxValue - time <= timeout))
			{
				if (!(timeout <= TimeSpan.Zero) || !(DateTime.MinValue - time >= timeout))
				{
					return time + timeout;
				}
				else
				{
					return DateTime.MinValue;
				}
			}
			else
			{
				return DateTime.MaxValue;
			}
		}

		public static TimeSpan Divide(TimeSpan timeout, int factor)
		{
			if (timeout != TimeSpan.MaxValue)
			{
				return Ticks.ToTimeSpan(Ticks.FromTimeSpan(timeout) / (long)factor + (long)1);
			}
			else
			{
				return TimeSpan.MaxValue;
			}
		}

		public TimeSpan ElapsedTime()
		{
			return this.originalTimeout - this.RemainingTime();
		}

		public static TimeSpan FromMilliseconds(int milliseconds)
		{
			if (milliseconds != -1)
			{
				return TimeSpan.FromMilliseconds((double)milliseconds);
			}
			else
			{
				return TimeSpan.MaxValue;
			}
		}

		public static bool IsTooLarge(TimeSpan timeout)
		{
			if (timeout <= TimeoutHelper.MaxWait)
			{
				return false;
			}
			else
			{
				return timeout != TimeSpan.MaxValue;
			}
		}

		public static TimeSpan Min(TimeSpan val1, TimeSpan val2)
		{
			if (val1 <= val2)
			{
				return val1;
			}
			else
			{
				return val2;
			}
		}

		public TimeSpan RemainingTime()
		{
			if (this.deadlineSet)
			{
				if (this.deadline != DateTime.MaxValue)
				{
					TimeSpan utcNow = this.deadline - DateTime.UtcNow;
					if (utcNow > TimeSpan.Zero)
					{
						return utcNow;
					}
					else
					{
						return TimeSpan.Zero;
					}
				}
				else
				{
					return TimeSpan.MaxValue;
				}
			}
			else
			{
				this.SetDeadline();
				return this.originalTimeout;
			}
		}

		private void SetDeadline()
		{
			this.deadline = DateTime.UtcNow + this.originalTimeout;
			this.deadlineSet = true;
		}

		public static DateTime Subtract(DateTime time, TimeSpan timeout)
		{
			return TimeoutHelper.Add(time, TimeSpan.Zero - timeout);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static void ThrowIfNegativeArgument(TimeSpan timeout)
		{
			TimeoutHelper.ThrowIfNegativeArgument(timeout, "timeout");
		}

		public static void ThrowIfNegativeArgument(TimeSpan timeout, string argumentName)
		{
			if (timeout >= TimeSpan.Zero)
			{
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, InternalSR.TimeoutMustBeNonNegative(argumentName, timeout));
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static void ThrowIfNonPositiveArgument(TimeSpan timeout)
		{
			TimeoutHelper.ThrowIfNonPositiveArgument(timeout, "timeout");
		}

		public static void ThrowIfNonPositiveArgument(TimeSpan timeout, string argumentName)
		{
			if (timeout > TimeSpan.Zero)
			{
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, InternalSR.TimeoutMustBePositive(argumentName, timeout));
			}
		}

		public static int ToMilliseconds(TimeSpan timeout)
		{
			if (timeout != TimeSpan.MaxValue)
			{
				long num = Ticks.FromTimeSpan(timeout);
				if (num / (long)0x2710 <= (long)0x7fffffff)
				{
					return Ticks.ToMilliseconds(num);
				}
				else
				{
					return 0x7fffffff;
				}
			}
			else
			{
				return -1;
			}
		}

		public static bool WaitOne(WaitHandle waitHandle, TimeSpan timeout)
		{
			TimeoutHelper.ThrowIfNegativeArgument(timeout);
			if (timeout != TimeSpan.MaxValue)
			{
				return waitHandle.WaitOne(timeout, false);
			}
			else
			{
				waitHandle.WaitOne();
				return true;
			}
		}
	}
}