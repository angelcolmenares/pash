using System;

namespace System.Runtime
{
	internal class AsyncEventArgs<TArgument> : AsyncEventArgs
	{
		public TArgument Arguments
		{
			get;set;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public AsyncEventArgs()
		{
		}

		public virtual void Set(AsyncEventArgsCallback callback, TArgument arguments, object state)
		{
			base.SetAsyncState(callback, state);
			this.Arguments = arguments;
		}
	}
}