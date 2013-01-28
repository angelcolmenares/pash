using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Runtime
{
	internal static class TaskExtensions
	{
		public static IAsyncResult AsAsyncResult<T>(this Task<T> task, AsyncCallback callback, object state)
		{
			if (task != null)
			{
				if (task.Status != TaskStatus.Created)
				{
					TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>(state);
					task.ContinueWith((Task<T> t) => {
						if (!t.IsFaulted)
						{
							if (!t.IsCanceled)
							{
								taskCompletionSource.TrySetResult(t.Result);
							}
							else
							{
								taskCompletionSource.TrySetCanceled();
							}
						}
						else
						{
							taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
						}
						if (callback != null)
						{
							callback(taskCompletionSource.Task);
						}
					}
					, TaskContinuationOptions.ExecuteSynchronously);
					return taskCompletionSource.Task;
				}
				else
				{
					throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.SFxTaskNotStarted));
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("task");
			}
		}

		public static IAsyncResult AsAsyncResult(this Task task, AsyncCallback callback, object state)
		{
			if (task != null)
			{
				if (task.Status != TaskStatus.Created)
				{
					TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>(state);
					task.ContinueWith((Task t) => {
						if (!t.IsFaulted)
						{
							if (!t.IsCanceled)
							{
								taskCompletionSource.TrySetResult(null);
							}
							else
							{
								taskCompletionSource.TrySetCanceled();
							}
						}
						else
						{
							taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
						}
						if (callback != null)
						{
							callback(taskCompletionSource.Task);
						}
					}
					, TaskContinuationOptions.ExecuteSynchronously);
					return taskCompletionSource.Task;
				}
				else
				{
					throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.SFxTaskNotStarted));
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("task");
			}
		}

		public static ConfiguredTaskAwaitable ContinueOnCapturedContextFlow(this Task task)
		{
			return task.ConfigureAwait(true);
		}

		public static ConfiguredTaskAwaitable<T> ContinueOnCapturedContextFlow<T>(this Task<T> task)
		{
			return task.ConfigureAwait(true);
		}

		public static ConfiguredTaskAwaitable SuppressContextFlow(this Task task)
		{
			return task.ConfigureAwait(false);
		}

		public static ConfiguredTaskAwaitable<T> SuppressContextFlow<T>(this Task<T> task)
		{
			return task.ConfigureAwait(false);
		}

		public static Task<TBase> Upcast<TDerived, TBase>(this Task<TDerived> task)
		where TDerived : TBase
		{
			if (task.Status == TaskStatus.RanToCompletion)
			{
				return Task.FromResult<TBase>((TBase)(object)task.Result);
			}
			else
			{
				return TaskExtensions.UpcastPrivate<TDerived, TBase>(task);
			}
		}

		private static async Task<TBase> UpcastPrivate<TDerived, TBase>(Task<TDerived> task)
		where TDerived : TBase
		{
			ConfiguredTaskAwaitable<TDerived> configuredTaskAwaitable = task.ConfigureAwait(false);
			TBase tBase = (TBase)(object)await configuredTaskAwaitable;
			return tBase;
		}

		public static void Wait<TException>(this Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException aggregateException1)
			{
				AggregateException aggregateException = aggregateException1;
				throw Fx.Exception.AsError<TException>(aggregateException);
			}
		}

		public static bool Wait<TException>(this Task task, int millisecondsTimeout)
		{
			bool flag;
			try
			{
				flag = task.Wait(millisecondsTimeout);
			}
			catch (AggregateException aggregateException1)
			{
				AggregateException aggregateException = aggregateException1;
				throw Fx.Exception.AsError<TException>(aggregateException);
			}
			return flag;
		}

		public static bool Wait<TException>(this Task task, TimeSpan timeout)
		{
			bool flag;
			try
			{
				if (timeout != TimeSpan.MaxValue)
				{
					flag = task.Wait(timeout);
				}
				else
				{
					flag = task.Wait(-1);
				}
			}
			catch (AggregateException aggregateException1)
			{
				AggregateException aggregateException = aggregateException1;
				throw Fx.Exception.AsError<TException>(aggregateException);
			}
			return flag;
		}

		public static void Wait(this Task task, TimeSpan timeout, Action<Exception, TimeSpan, string> exceptionConverter, string operationType)
		{
			bool flag = false;
			try
			{
				if (timeout <= TimeoutHelper.MaxWait)
				{
					flag = !task.Wait(timeout);
				}
				else
				{
					task.Wait();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (Fx.IsFatal(exception) || exceptionConverter == null)
				{
					throw;
				}
				else
				{
					exceptionConverter(exception, timeout, operationType);
				}
			}
			if (!flag)
			{
				return;
			}
			else
			{
				throw Fx.Exception.AsError(new TimeoutException(InternalSR.TaskTimedOutError(timeout)));
			}
		}
	}
}