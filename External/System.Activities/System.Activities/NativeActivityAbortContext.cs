using System;

namespace System.Activities
{
	public sealed class NativeActivityAbortContext : ActivityContext
	{
		internal NativeActivityAbortContext (Exception reason)
		{
			if (reason == null)
				throw new ArgumentNullException ("reason");
			Reason = reason;
		}

		public Exception Reason { get; private set; }
	}
}
