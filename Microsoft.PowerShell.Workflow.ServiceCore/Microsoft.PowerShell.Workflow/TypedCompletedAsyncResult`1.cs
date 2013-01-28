using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class TypedCompletedAsyncResult<T> : TypedAsyncResult<T>
	{
		public TypedCompletedAsyncResult(T data, AsyncCallback callback, object state) : base(callback, state)
		{
			base.Complete(data, true);
		}

		public static T End(IAsyncResult result)
		{
			TypedCompletedAsyncResult<T> typedCompletedAsyncResult = result as TypedCompletedAsyncResult<T>;
			if (typedCompletedAsyncResult != null)
			{
				return TypedAsyncResult<T>.End(typedCompletedAsyncResult);
			}
			else
			{
				throw new ArgumentException(Resources.InvalidAsyncResult);
			}
		}
	}
}