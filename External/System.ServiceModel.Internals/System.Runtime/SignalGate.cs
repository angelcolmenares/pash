using System;
using System.Threading;

namespace System.Runtime
{
	internal class SignalGate
	{
		private int state;

		internal bool IsLocked
		{
			get
			{
				return this.state == 0;
			}
		}

		internal bool IsSignalled
		{
			get
			{
				return this.state == 3;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SignalGate()
		{
		}

		public bool Signal()
		{
			int num = this.state;
			if (num == 0)
			{
				num = Interlocked.CompareExchange(ref this.state, 1, 0);
			}
			if (num != 2)
			{
				if (num != 0)
				{
					this.ThrowInvalidSignalGateState();
				}
				return false;
			}
			else
			{
				this.state = 3;
				return true;
			}
		}

		private void ThrowInvalidSignalGateState()
		{
			throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.InvalidSemaphoreExit));
		}

		public bool Unlock()
		{
			int num = this.state;
			if (num == 0)
			{
				num = Interlocked.CompareExchange(ref this.state, 2, 0);
			}
			if (num != 1)
			{
				if (num != 0)
				{
					this.ThrowInvalidSignalGateState();
				}
				return false;
			}
			else
			{
				this.state = 3;
				return true;
			}
		}

		private static class GateState
		{
			public const int Locked = 0;

			public const int SignalPending = 1;

			public const int Unlocked = 2;

			public const int Signalled = 3;

		}
	}
}