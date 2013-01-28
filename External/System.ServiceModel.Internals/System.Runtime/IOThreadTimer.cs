using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.Interop;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal class IOThreadTimer
	{
		private static long systemTimeResolutionTicks;

		private Action<object> callback;

		private object callbackState;

		private long dueTime;

		private int index;

		private long maxSkew;

		private IOThreadTimer.TimerGroup timerGroup;

		private const int maxSkewInMillisecondsDefault = 100;

		public static long SystemTimeResolutionTicks
		{
			get
			{
				if (IOThreadTimer.systemTimeResolutionTicks == (long)-1)
				{
					IOThreadTimer.systemTimeResolutionTicks = IOThreadTimer.GetSystemTimeResolution();
				}
				return IOThreadTimer.systemTimeResolutionTicks;
			}
		}

		static IOThreadTimer()
		{
			IOThreadTimer.systemTimeResolutionTicks = (long)-1;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public IOThreadTimer(Action<object> callback, object callbackState, bool isTypicallyCanceledShortlyAfterBeingSet) : this(callback, callbackState, isTypicallyCanceledShortlyAfterBeingSet, 100)
		{
		}

		public IOThreadTimer(Action<object> callback, object callbackState, bool isTypicallyCanceledShortlyAfterBeingSet, int maxSkewInMilliseconds)
		{
			IOThreadTimer.TimerGroup volatileTimerGroup;
			this.callback = callback;
			this.callbackState = callbackState;
			this.maxSkew = Ticks.FromMilliseconds(maxSkewInMilliseconds);
			IOThreadTimer oThreadTimer = this;
			if (isTypicallyCanceledShortlyAfterBeingSet)
			{
				volatileTimerGroup = IOThreadTimer.TimerManager.Value.VolatileTimerGroup;
			}
			else
			{
				volatileTimerGroup = IOThreadTimer.TimerManager.Value.StableTimerGroup;
			}
			oThreadTimer.timerGroup = volatileTimerGroup;
		}

		public bool Cancel()
		{
			return IOThreadTimer.TimerManager.Value.Cancel(this);
		}

		[SecuritySafeCritical]
		private static long GetSystemTimeResolution()
		{
			int num = 0;
			uint num1 = 0;
			uint num2 = 0;
			if (UnsafeNativeMethods.GetSystemTimeAdjustment(out num, out num1, out num2) == 0)
			{
				return (long)0x249f0;
			}
			else
			{
				return (long)num1;
			}
		}

		public void Set(TimeSpan timeFromNow)
		{
			if (timeFromNow != TimeSpan.MaxValue)
			{
				this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromTimeSpan(timeFromNow)));
			}
		}

		public void Set(int millisecondsFromNow)
		{
			this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromMilliseconds(millisecondsFromNow)));
		}

		public void SetAt(long dueTime)
		{
			IOThreadTimer.TimerManager.Value.Set(this, dueTime);
		}

		private class TimerGroup
		{
			private IOThreadTimer.TimerQueue timerQueue;

			private IOThreadTimer.WaitableTimer waitableTimer;

			public IOThreadTimer.TimerQueue TimerQueue
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.timerQueue;
				}
			}

			public IOThreadTimer.WaitableTimer WaitableTimer
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.waitableTimer;
				}
			}

			public TimerGroup()
			{
				this.waitableTimer = new IOThreadTimer.WaitableTimer();
				this.waitableTimer.Set(0x7fffffffffffffffL);
				this.timerQueue = new IOThreadTimer.TimerQueue();
			}
		}

		private class TimerManager
		{
			private const long maxTimeToWaitForMoreTimers = 0x989680L;

			private static IOThreadTimer.TimerManager @value;

			private Action<object> onWaitCallback;

			private IOThreadTimer.TimerGroup stableTimerGroup;

			private IOThreadTimer.TimerGroup volatileTimerGroup;

			private IOThreadTimer.WaitableTimer[] waitableTimers;

			private bool waitScheduled;

			public IOThreadTimer.TimerGroup StableTimerGroup
			{
				get
				{
					return this.stableTimerGroup;
				}
			}

			private object ThisLock
			{
				get
				{
					return this;
				}
			}

			public static IOThreadTimer.TimerManager Value
			{
				get
				{
					return IOThreadTimer.TimerManager.@value;
				}
			}

			public IOThreadTimer.TimerGroup VolatileTimerGroup
			{
				get
				{
					return this.volatileTimerGroup;
				}
			}

			static TimerManager()
			{
				IOThreadTimer.TimerManager.@value = new IOThreadTimer.TimerManager();
			}

			public TimerManager()
			{
				this.onWaitCallback = new Action<object>(this.OnWaitCallback);
				this.stableTimerGroup = new IOThreadTimer.TimerGroup();
				this.volatileTimerGroup = new IOThreadTimer.TimerGroup();
				IOThreadTimer.WaitableTimer[] waitableTimer = new IOThreadTimer.WaitableTimer[2];
				waitableTimer[0] = this.stableTimerGroup.WaitableTimer;
				waitableTimer[1] = this.volatileTimerGroup.WaitableTimer;
				this.waitableTimers = waitableTimer;
			}

			public bool Cancel(IOThreadTimer timer)
			{
				bool flag;
				lock (this.ThisLock)
				{
					if (timer.index <= 0)
					{
						flag = false;
					}
					else
					{
						IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
						IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
						timerQueue.DeleteTimer(timer);
						if (timerQueue.Count <= 0)
						{
							IOThreadTimer.TimerGroup otherTimerGroup = this.GetOtherTimerGroup(timerGroup);
							if (otherTimerGroup.TimerQueue.Count == 0)
							{
								long now = Ticks.Now;
								long dueTime = timerGroup.WaitableTimer.DueTime - now;
								long num = otherTimerGroup.WaitableTimer.DueTime - now;
								if (dueTime > (long)0x989680 && num > (long)0x989680)
								{
									timerGroup.WaitableTimer.Set(Ticks.Add(now, (long)0x989680));
								}
							}
						}
						else
						{
							this.UpdateWaitableTimer(timerGroup);
						}
						flag = true;
					}
				}
				return flag;
			}

			private void EnsureWaitScheduled()
			{
				if (!this.waitScheduled)
				{
					this.ScheduleWait();
				}
			}

			private IOThreadTimer.TimerGroup GetOtherTimerGroup(IOThreadTimer.TimerGroup timerGroup)
			{
				if (!object.ReferenceEquals(timerGroup, this.volatileTimerGroup))
				{
					return this.volatileTimerGroup;
				}
				else
				{
					return this.stableTimerGroup;
				}
			}

			private void OnWaitCallback(object state)
			{
				WaitHandle.WaitAny(this.waitableTimers);
				long now = Ticks.Now;
				lock (this.ThisLock)
				{
					this.waitScheduled = false;
					this.ScheduleElapsedTimers(now);
					this.ReactivateWaitableTimers();
					this.ScheduleWaitIfAnyTimersLeft();
				}
			}

			private void ReactivateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
			{
				IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
				if (timerQueue.Count <= 0)
				{
					timerGroup.WaitableTimer.Set(0x7fffffffffffffffL);
					return;
				}
				else
				{
					timerGroup.WaitableTimer.Set(timerQueue.MinTimer.dueTime);
					return;
				}
			}

			private void ReactivateWaitableTimers()
			{
				this.ReactivateWaitableTimer(this.stableTimerGroup);
				this.ReactivateWaitableTimer(this.volatileTimerGroup);
			}

			private void ScheduleElapsedTimers(long now)
			{
				this.ScheduleElapsedTimers(this.stableTimerGroup, now);
				this.ScheduleElapsedTimers(this.volatileTimerGroup, now);
			}

			private void ScheduleElapsedTimers(IOThreadTimer.TimerGroup timerGroup, long now)
			{
				IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
				while (timerQueue.Count > 0)
				{
					IOThreadTimer minTimer = timerQueue.MinTimer;
					long num = minTimer.dueTime - now;
					if (num > minTimer.maxSkew)
					{
						break;
					}
					timerQueue.DeleteMinTimer();
					ActionItem.Schedule(minTimer.callback, minTimer.callbackState);
				}
			}

			private void ScheduleWait()
			{
				ActionItem.Schedule(this.onWaitCallback, null);
				this.waitScheduled = true;
			}

			private void ScheduleWaitIfAnyTimersLeft()
			{
				if (this.stableTimerGroup.TimerQueue.Count > 0 || this.volatileTimerGroup.TimerQueue.Count > 0)
				{
					this.ScheduleWait();
				}
			}

			public void Set(IOThreadTimer timer, long dueTime)
			{
				long num = dueTime - timer.dueTime;
				if (num < (long)0)
				{
					num = -num;
				}
				if (num > timer.maxSkew)
				{
					lock (this.ThisLock)
					{
						IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
						IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
						if (timer.index <= 0)
						{
							if (timerQueue.InsertTimer(timer, dueTime))
							{
								this.UpdateWaitableTimer(timerGroup);
								if (timerQueue.Count == 1)
								{
									this.EnsureWaitScheduled();
								}
							}
						}
						else
						{
							if (timerQueue.UpdateTimer(timer, dueTime))
							{
								this.UpdateWaitableTimer(timerGroup);
							}
						}
					}
				}
			}

			private void UpdateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
			{
				IOThreadTimer.WaitableTimer waitableTimer = timerGroup.WaitableTimer;
				IOThreadTimer minTimer = timerGroup.TimerQueue.MinTimer;
				long dueTime = waitableTimer.DueTime - minTimer.dueTime;
				if (dueTime < (long)0)
				{
					dueTime = -dueTime;
				}
				if (dueTime > minTimer.maxSkew)
				{
					waitableTimer.Set(minTimer.dueTime);
				}
			}
		}

		private class TimerQueue
		{
			private int count;

			private IOThreadTimer[] timers;

			public int Count
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.count;
				}
			}

			public IOThreadTimer MinTimer
			{
				get
				{
					return this.timers[1];
				}
			}

			public TimerQueue()
			{
				this.timers = new IOThreadTimer[4];
			}

			public void DeleteMinTimer()
			{
				IOThreadTimer minTimer = this.MinTimer;
				this.DeleteMinTimerCore();
				minTimer.index = 0;
				minTimer.dueTime = (long)0;
			}

			private void DeleteMinTimerCore()
			{
				int num;
				int num1;
				IOThreadTimer oThreadTimer;
				int num2 = this.count;
				if (num2 != 1)
				{
					IOThreadTimer[] oThreadTimerArray = this.timers;
					IOThreadTimer oThreadTimer1 = oThreadTimerArray[num2];
					int num3 = num2 - 1;
					num2 = num3;
					this.count = num3;
					int num4 = 1;
					do
					{
						num = num4 * 2;
						if (num > num2)
						{
							break;
						}
						if (num >= num2)
						{
							num1 = num;
							oThreadTimer = oThreadTimerArray[num1];
						}
						else
						{
							IOThreadTimer oThreadTimer2 = oThreadTimerArray[num];
							int num5 = num + 1;
							IOThreadTimer oThreadTimer3 = oThreadTimerArray[num5];
							if (oThreadTimer3.dueTime >= oThreadTimer2.dueTime)
							{
								oThreadTimer = oThreadTimer2;
								num1 = num;
							}
							else
							{
								oThreadTimer = oThreadTimer3;
								num1 = num5;
							}
						}
						if (oThreadTimer1.dueTime <= oThreadTimer.dueTime)
						{
							break;
						}
						oThreadTimerArray[num4] = oThreadTimer;
						oThreadTimer.index = num4;
						num4 = num1;
					}
					while (num < num2);
					oThreadTimerArray[num4] = oThreadTimer1;
					oThreadTimer1.index = num4;
					oThreadTimerArray[num2 + 1] = null;
					return;
				}
				else
				{
					this.count = 0;
					this.timers[1] = null;
					return;
				}
			}

			public void DeleteTimer(IOThreadTimer timer)
			{
				int num = timer.index;
				IOThreadTimer[] oThreadTimerArray = this.timers;
				while (true)
				{
					int num1 = num / 2;
					if (num1 < 1)
					{
						break;
					}
					IOThreadTimer oThreadTimer = oThreadTimerArray[num1];
					oThreadTimerArray[num] = oThreadTimer;
					oThreadTimer.index = num;
					num = num1;
				}
				timer.index = 0;
				timer.dueTime = (long)0;
				oThreadTimerArray[1] = null;
				this.DeleteMinTimerCore();
			}

			public bool InsertTimer(IOThreadTimer timer, long dueTime)
			{
				IOThreadTimer[] oThreadTimerArray = this.timers;
				int num = this.count + 1;
				if (num == (int)oThreadTimerArray.Length)
				{
					oThreadTimerArray = new IOThreadTimer[(int)oThreadTimerArray.Length * 2];
					Array.Copy(this.timers, oThreadTimerArray, (int)this.timers.Length);
					this.timers = oThreadTimerArray;
				}
				this.count = num;
				if (num > 1)
				{
					while (true)
					{
						int num1 = num / 2;
						if (num1 == 0)
						{
							break;
						}
						IOThreadTimer oThreadTimer = oThreadTimerArray[num1];
						if (oThreadTimer.dueTime <= dueTime)
						{
							break;
						}
						oThreadTimerArray[num] = oThreadTimer;
						oThreadTimer.index = num;
						num = num1;
					}
				}
				oThreadTimerArray[num] = timer;
				timer.index = num;
				timer.dueTime = dueTime;
				return num == 1;
			}

			public bool UpdateTimer(IOThreadTimer timer, long dueTime)
			{
				int num = timer.index;
				IOThreadTimer[] oThreadTimerArray = this.timers;
				int num1 = this.count;
				int num2 = num / 2;
				if (num2 == 0 || oThreadTimerArray[num2].dueTime <= dueTime)
				{
					int num3 = num * 2;
					if (num3 > num1 || oThreadTimerArray[num3].dueTime >= dueTime)
					{
						int num4 = num3 + 1;
						if (num4 > num1 || oThreadTimerArray[num4].dueTime >= dueTime)
						{
							timer.dueTime = dueTime;
							return num == 1;
						}
					}
				}
				this.DeleteTimer(timer);
				this.InsertTimer(timer, dueTime);
				return true;
			}
		}

		private class WaitableTimer : WaitHandle
		{
			private long dueTime;

			public long DueTime
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.dueTime;
				}
			}

			[SecuritySafeCritical]
			public WaitableTimer()
			{
				base.SafeWaitHandle = IOThreadTimer.WaitableTimer.TimerHelper.CreateWaitableTimer();
			}

			[SecuritySafeCritical]
			public void Set(long dueTime)
			{
				this.dueTime = IOThreadTimer.WaitableTimer.TimerHelper.Set(base.SafeWaitHandle, dueTime);
			}

			[SecurityCritical]
			private static class TimerHelper
			{
				public static SafeWaitHandle CreateWaitableTimer()
				{
					SafeWaitHandle safeWaitHandle = UnsafeNativeMethods.CreateWaitableTimer(IntPtr.Zero, false, null);
					if (!safeWaitHandle.IsInvalid)
					{
						return safeWaitHandle;
					}
					else
					{
						Exception win32Exception = new Win32Exception();
						safeWaitHandle.SetHandleAsInvalid();
						throw Fx.Exception.AsError(win32Exception);
					}
				}

				public static long Set(SafeWaitHandle timer, long dueTime)
				{
					if (UnsafeNativeMethods.SetWaitableTimer(timer, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false))
					{
						return dueTime;
					}
					else
					{
						throw Fx.Exception.AsError(new Win32Exception());
					}
				}
			}
		}
	}
}