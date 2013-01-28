using System;
using System.Runtime.Interop;
using System.Security;

namespace System.Runtime
{
	internal static class Ticks
	{
		public static long Now
		{
			[SecuritySafeCritical]
			get
			{
				long num = 0L;
				UnsafeNativeMethods.GetSystemTimeAsFileTime(out num);
				return num;
			}
		}

		public static long Add(long firstTicks, long secondTicks)
		{
			if (firstTicks == 0x7fffffffffffffffL || firstTicks == -9223372036854775808L)
			{
				return firstTicks;
			}
			else
			{
				if (secondTicks == 0x7fffffffffffffffL || secondTicks == -9223372036854775808L)
				{
					return secondTicks;
				}
				else
				{
					if (firstTicks < (long)0 || 0x7fffffffffffffffL - firstTicks > secondTicks)
					{
						if (firstTicks > (long)0 || -9223372036854775808L - firstTicks < secondTicks)
						{
							return firstTicks + secondTicks;
						}
						else
						{
							return -9223372036854775807L;
						}
					}
					else
					{
						return 0x7ffffffffffffffeL;
					}
				}
			}
		}

		public static long FromMilliseconds(int milliseconds)
		{
			return (long)milliseconds * (long)0x2710;
		}

		public static long FromTimeSpan(TimeSpan duration)
		{
			return duration.Ticks;
		}

		public static int ToMilliseconds(long ticks)
		{
			return (int)(ticks / (long)0x2710);
		}

		public static TimeSpan ToTimeSpan(long ticks)
		{
			return new TimeSpan(ticks);
		}
	}
}