namespace System.Runtime
{
	internal class AsyncEventArgs<TArgument, TResult> : AsyncEventArgs<TArgument>
	{
		public TResult Result
		{
			get;set;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public AsyncEventArgs()
		{
		}
	}
}