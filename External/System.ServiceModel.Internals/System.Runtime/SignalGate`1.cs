using System;

namespace System.Runtime
{
	internal class SignalGate<T> : SignalGate
	{
		private T result;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SignalGate()
		{
		}

		public bool Signal(T result)
		{
			this.result = result;
			return base.Signal();
		}

		public bool Unlock(out T result)
		{
			if (!base.Unlock())
			{
				result = default(T);
				return false;
			}
			else
			{
				result = this.result;
				return true;
			}
		}
	}
}