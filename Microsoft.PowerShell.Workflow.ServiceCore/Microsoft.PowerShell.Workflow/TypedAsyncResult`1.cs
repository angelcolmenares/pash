using System;

namespace Microsoft.PowerShell.Workflow
{
	internal abstract class TypedAsyncResult<T> : FileStoreAsyncResult
	{
		private T data;

		public T Data
		{
			get
			{
				return this.data;
			}
		}

		protected TypedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
		{
		}

		protected void Complete(T data, bool completedSynchronously)
		{
			this.data = data;
			base.Complete(completedSynchronously);
		}

		public static T End(IAsyncResult result)
		{
			TypedAsyncResult<T> typedAsyncResult = FileStoreAsyncResult.End<TypedAsyncResult<T>>(result);
			return typedAsyncResult.Data;
		}
	}
}