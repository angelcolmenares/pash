using System;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal class IOThreadScheduler
	{
		private static IOThreadScheduler current;

		private readonly IOThreadScheduler.ScheduledOverlapped overlapped;

		[SecurityCritical]
		private readonly IOThreadScheduler.Slot[] slots;

		[SecurityCritical]
		private readonly IOThreadScheduler.Slot[] slotsLowPri;

		private int headTail;

		private int headTailLowPri;

		private const int MaximumCapacity = 0x8000;

		private int SlotMask
		{
			[SecurityCritical]
			get
			{
				return (int)this.slots.Length - 1;
			}
		}

		private int SlotMaskLowPri
		{
			[SecurityCritical]
			get
			{
				return (int)this.slotsLowPri.Length - 1;
			}
		}

		static IOThreadScheduler()
		{
			IOThreadScheduler.current = new IOThreadScheduler(32, 32);
		}

		[SecuritySafeCritical]
		private IOThreadScheduler(int capacity, int capacityLowPri)
		{
			this.headTail = -131072;
			this.headTailLowPri = -65536;
			this.slots = new IOThreadScheduler.Slot[capacity];
			this.slotsLowPri = new IOThreadScheduler.Slot[capacityLowPri];
			this.overlapped = new IOThreadScheduler.ScheduledOverlapped();
		}

		[SecuritySafeCritical]
		private void Cleanup()
		{
			if (this.overlapped != null)
			{
				this.overlapped.Cleanup();
			}
		}

		[SecurityCritical]
		private void CompletionCallback(out Action<object> callback, out object state)
		{
			int num;
			int num1 = this.headTail;
			while (true)
			{
				bool flag = IOThreadScheduler.Bits.Count(num1) == 0;
				if (flag)
				{
					num = this.headTailLowPri;
					while (IOThreadScheduler.Bits.CountNoIdle(num) != 0)
					{
						int num2 = Interlocked.CompareExchange(ref this.headTailLowPri, IOThreadScheduler.Bits.IncrementLo(num), num);
						num = num2;
						if (num != num2)
						{
							continue;
						}
						this.overlapped.Post(this);
						this.slotsLowPri[num & this.SlotMaskLowPri].DequeueWorkItem(out callback, out state);
						return;
					}
				}
				int num3 = Interlocked.CompareExchange(ref this.headTail, IOThreadScheduler.Bits.IncrementLo(num1), num1);
				num1 = num3;
				if (num1 == num3)
				{
					if (flag)
					{
						num = this.headTailLowPri;
						if (IOThreadScheduler.Bits.CountNoIdle(num) == 0)
						{
							break;
						}
						num1 = IOThreadScheduler.Bits.IncrementLo(num1);
						if (num1 != Interlocked.CompareExchange(ref this.headTail, num1 + 0x10000, num1))
						{
							break;
						}
						num1 = num1 + 0x10000;
					}
					else
					{
						this.overlapped.Post(this);
						this.slots[num1 & this.SlotMask].DequeueWorkItem(out callback, out state);
						return;
					}
				}
			}
			callback = null;
			state = null;
		}

		~IOThreadScheduler()
		{
			try
			{
				if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
				{
					this.Cleanup();
				}
			}
			finally
			{
				//this.Finalize();
			}
		}

		[SecurityCritical]
		private bool ScheduleCallbackHelper(Action<object> callback, object state)
		{
			bool flag = false;
			int num = Interlocked.Add(ref this.headTail, 0x10000);
			bool flag1 = IOThreadScheduler.Bits.Count(num) == 0;
			if (flag1)
			{
				num = Interlocked.Add(ref this.headTail, 0x10000);
			}
			if (IOThreadScheduler.Bits.Count(num) != -1)
			{
				bool flag2 = this.slots[num >> 16 & this.SlotMask].TryEnqueueWorkItem(callback, state, out flag);
				if (flag)
				{
					IOThreadScheduler oThreadScheduler = new IOThreadScheduler(Math.Min((int)this.slots.Length * 2, 0x8000), (int)this.slotsLowPri.Length);
					Interlocked.CompareExchange<IOThreadScheduler>(ref IOThreadScheduler.current, oThreadScheduler, this);
				}
				if (flag1)
				{
					this.overlapped.Post(this);
				}
				return flag2;
			}
			else
			{
				throw Fx.AssertAndThrowFatal("Head/Tail overflow!");
			}
		}

		[SecurityCritical]
		private bool ScheduleCallbackLowPriHelper(Action<object> callback, object state)
		{
			bool flag = false;
			int num = Interlocked.Add(ref this.headTailLowPri, 0x10000);
			bool flag1 = false;
			if (IOThreadScheduler.Bits.CountNoIdle(num) == 1)
			{
				int num1 = this.headTail;
				if (IOThreadScheduler.Bits.Count(num1) == -1)
				{
					int num2 = Interlocked.CompareExchange(ref this.headTail, num1 + 0x10000, num1);
					if (num1 == num2)
					{
						flag1 = true;
					}
				}
			}
			if (IOThreadScheduler.Bits.CountNoIdle(num) != 0)
			{
				bool flag2 = this.slotsLowPri[num >> 16 & this.SlotMaskLowPri].TryEnqueueWorkItem(callback, state, out flag);
				if (flag)
				{
					IOThreadScheduler oThreadScheduler = new IOThreadScheduler((int)this.slots.Length, Math.Min((int)this.slotsLowPri.Length * 2, 0x8000));
					Interlocked.CompareExchange<IOThreadScheduler>(ref IOThreadScheduler.current, oThreadScheduler, this);
				}
				if (flag1)
				{
					this.overlapped.Post(this);
				}
				return flag2;
			}
			else
			{
				throw Fx.AssertAndThrowFatal("Low-priority Head/Tail overflow!");
			}
		}

		[SecurityCritical]
		public static void ScheduleCallbackLowPriNoFlow(Action<object> callback, object state)
		{
			if (callback != null)
			{
				bool flag = false;
				while (!flag)
				{
					try
					{
					}
					finally
					{
						flag = IOThreadScheduler.current.ScheduleCallbackLowPriHelper(callback, state);
					}
				}
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentNull("callback");
			}
		}

		[SecurityCritical]
		public static void ScheduleCallbackNoFlow(Action<object> callback, object state)
		{
			if (callback != null)
			{
				bool flag = false;
				while (!flag)
				{
					try
					{
					}
					finally
					{
						flag = IOThreadScheduler.current.ScheduleCallbackHelper(callback, state);
					}
				}
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentNull("callback");
			}
		}

		[SecurityCritical]
		private bool TryCoalesce(out Action<object> callback, out object state)
		{
			int num;
			int num1 = this.headTail;
			do
			{
			Label0:
				if (IOThreadScheduler.Bits.Count(num1) <= 0)
				{
					int num2 = this.headTailLowPri;
					if (IOThreadScheduler.Bits.CountNoIdle(num2) <= 0)
					{
						callback = null;
						state = null;
						return false;
					}
					else
					{
						int num3 = Interlocked.CompareExchange(ref this.headTailLowPri, IOThreadScheduler.Bits.IncrementLo(num2), num2);
						num2 = num3;
						if (num2 != num3)
						{
							num1 = this.headTail;
							goto Label0;
						}
						else
						{
							this.slotsLowPri[num2 & this.SlotMaskLowPri].DequeueWorkItem(out callback, out state);
							return true;
						}
					}
				}
				else
				{
					num = Interlocked.CompareExchange(ref this.headTail, IOThreadScheduler.Bits.IncrementLo(num1), num1);
					num1 = num;
				}
			}
			while (num1 != num);
			this.slots[num1 & this.SlotMask].DequeueWorkItem(out callback, out state);
			return true;
		}

		private static class Bits
		{
			public const int HiShift = 16;

			public const int HiOne = 0x10000;

			public const int LoHiBit = 0x8000;

			public const int HiHiBit = -2147483648;

			public const int LoCountMask = 0x7fff;

			public const int HiCountMask = 0x7fff0000;

			public const int LoMask = 0xffff;

			public const int HiMask = -65536;

			public const int HiBits = -2147450880;

			public static int Count(int slot)
			{
				return ((slot >> 16) - slot + 2 & 0xffff) - 1;
			}

			public static int CountNoIdle(int slot)
			{
				return (slot >> 16) - slot + 1 & 0xffff;
			}

			public static int IncrementLo(int slot)
			{
				return slot + 1 & 0xffff | slot & -65536;
			}

			public static bool IsComplete(int gate)
			{
				return (gate & -65536) == gate << 16;
			}
		}

		[SecurityCritical]
		private class ScheduledOverlapped
		{
			private readonly unsafe NativeOverlapped* nativeOverlapped;

			private IOThreadScheduler scheduler;

			public ScheduledOverlapped()
			{
				unsafe {
					this.nativeOverlapped = (new Overlapped()).UnsafePack(Fx.ThunkCallback(new IOCompletionCallback(this.IOCallback)), null);
				}
			}

			public void Cleanup()
			{
				if (this.scheduler == null)
				{
					unsafe {
						Overlapped.Free(this.nativeOverlapped);
					}
					return;
				}
				else
				{
					throw Fx.AssertAndThrowFatal("Cleanup called on an overlapped that is in-flight.");
				}
			}

			private unsafe void IOCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
			{
				Action<object> action = null;
				object obj = null;
				IOThreadScheduler oThreadScheduler = this.scheduler;
				this.scheduler = null;
				try
				{
				}
				finally
				{
					oThreadScheduler.CompletionCallback(out action, out obj);
				}
				bool flag = true;
				while (flag)
				{
					if (action != null)
					{
						action(obj);
					}
					try
					{
					}
					finally
					{
						flag = oThreadScheduler.TryCoalesce(out action, out obj);
					}
				}
			}

			public void Post (IOThreadScheduler iots)
			{
				this.scheduler = iots;
				unsafe {
					ThreadPool.UnsafeQueueNativeOverlapped (this.nativeOverlapped);
				}
			}
		}

		private struct Slot
		{
			private int gate;

			private Action<object> callback;

			private object state;

			public void DequeueWorkItem(out Action<object> callback, out object state)
			{
				int num = Interlocked.Add(ref this.gate, 0x10000);
				if ((num & 0x8000) != 0)
				{
					if ((num & 0x7fff0000) != 0x10000)
					{
						callback = null;
						state = null;
						if (IOThreadScheduler.Bits.IsComplete(num))
						{
							Interlocked.CompareExchange(ref this.gate, 0, num);
						}
					}
					else
					{
						callback = this.callback;
						state = this.state;
						this.state = null;
						this.callback = null;
						if ((num & 0x7fff) != 1 || Interlocked.CompareExchange(ref this.gate, 0, num) != num)
						{
							num = Interlocked.Add(ref this.gate, -2147483648);
							if (IOThreadScheduler.Bits.IsComplete(num))
							{
								Interlocked.CompareExchange(ref this.gate, 0, num);
								return;
							}
						}
					}
					return;
				}
				else
				{
					callback = null;
					state = null;
					return;
				}
			}

			public bool TryEnqueueWorkItem(Action<object> callback, object state, out bool wrapped)
			{
				int num = Interlocked.Increment(ref this.gate);
				wrapped = (num & 0x7fff) != 1;
				if (!wrapped)
				{
					this.state = state;
					this.callback = callback;
					num = Interlocked.Add(ref this.gate, 0x8000);
					if ((num & 0x7fff0000) != 0)
					{
						this.state = null;
						this.callback = null;
						if (num >> 16 != (num & 0x7fff) || Interlocked.CompareExchange(ref this.gate, 0, num) != num)
						{
							num = Interlocked.Add(ref this.gate, -2147483648);
							if (IOThreadScheduler.Bits.IsComplete(num))
							{
								Interlocked.CompareExchange(ref this.gate, 0, num);
							}
						}
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					if ((num & 0x8000) != 0 && IOThreadScheduler.Bits.IsComplete(num))
					{
						Interlocked.CompareExchange(ref this.gate, 0, num);
					}
					return false;
				}
			}
		}
	}
}