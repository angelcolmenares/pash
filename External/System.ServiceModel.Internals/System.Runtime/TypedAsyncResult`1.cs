using System;

namespace System.Runtime
{
	internal abstract class TypedAsyncResult<T> : AsyncResult
	{
		private T data;

		public T Data
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.data;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TypedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
		{
		}

		protected void Complete(T data, bool completedSynchronously)
		{
			this.data = data;
			base.Complete(completedSynchronously);
		}

		public static T End(IAsyncResult result)
		{
			TypedAsyncResult<T> typedAsyncResult = AsyncResult.End<TypedAsyncResult<T>>(result);
			return typedAsyncResult.Data;
		}
	}
}