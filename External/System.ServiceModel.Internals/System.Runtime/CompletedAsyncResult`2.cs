using System;

namespace System.Runtime
{
	internal class CompletedAsyncResult<TResult, TParameter> : AsyncResult
	{
		private TResult resultData;

		private TParameter parameter;

		public CompletedAsyncResult(TResult resultData, TParameter parameter, AsyncCallback callback, object state) : base(callback, state)
		{
			this.resultData = resultData;
			this.parameter = parameter;
			base.Complete(true);
		}

		public static TResult End(IAsyncResult result, out TParameter parameter)
		{
			Fx.AssertAndThrowFatal(result.IsCompleted, "CompletedAsyncResult<T> was not completed!");
			CompletedAsyncResult<TResult, TParameter> completedAsyncResult = AsyncResult.End<CompletedAsyncResult<TResult, TParameter>>(result);
			parameter = completedAsyncResult.parameter;
			return completedAsyncResult.resultData;
		}
	}
}