using System;

namespace System.Runtime
{
	internal abstract class ScheduleActionItemAsyncResult : AsyncResult
	{
		private static Action<object> doWork;

		static ScheduleActionItemAsyncResult()
		{
			ScheduleActionItemAsyncResult.doWork = new Action<object>(ScheduleActionItemAsyncResult.DoWork);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ScheduleActionItemAsyncResult(AsyncCallback callback, object state) : base(callback, state)
		{
		}

		private static void DoWork(object state)
		{
			ScheduleActionItemAsyncResult scheduleActionItemAsyncResult = (ScheduleActionItemAsyncResult)state;
			Exception exception = null;
			try
			{
				scheduleActionItemAsyncResult.OnDoWork();
			}
			catch (Exception exception2)
			{
				Exception exception1 = exception2;
				if (!Fx.IsFatal(exception1))
				{
					exception = exception1;
				}
				else
				{
					throw;
				}
			}
			scheduleActionItemAsyncResult.Complete(false, exception);
		}

		public static void End(IAsyncResult result)
		{
			AsyncResult.End<ScheduleActionItemAsyncResult>(result);
		}

		protected abstract void OnDoWork();

		protected void Schedule()
		{
			ActionItem.Schedule(ScheduleActionItemAsyncResult.doWork, this);
		}
	}
}