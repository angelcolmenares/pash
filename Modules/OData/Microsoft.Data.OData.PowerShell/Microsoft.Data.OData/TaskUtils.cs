namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskUtils
    {
        private static Task completedTask;

        internal static Task FollowAlwaysWith(this Task antecedentTask, Action<Task> operation)
        {
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            antecedentTask.ContinueWith(delegate (Task t) {
                Exception exception = null;
                try
                {
                    operation(t);
                }
                catch (Exception exception2)
                {
                    if (!ExceptionUtils.IsCatchableExceptionType(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (exception == null)
                        {
                            taskCompletionSource.TrySetResult(null);
                            return;
                        }
                        taskCompletionSource.TrySetException(exception);
                        return;

                    case TaskStatus.Canceled:
                        if (exception == null)
                        {
                            taskCompletionSource.TrySetCanceled();
                            return;
                        }
                        taskCompletionSource.TrySetException(exception);
                        return;

                    case TaskStatus.Faulted:
                        taskCompletionSource.TrySetException(t.Exception);
                        return;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).IgnoreExceptions();
            return taskCompletionSource.Task;
        }

        internal static Task FollowOnFaultWith(this Task antecedentTask, Action<Task> operation)
        {
            return FollowOnFaultWithImplementation<object>(antecedentTask, t => null, operation);
        }

        internal static Task<TResult> FollowOnFaultWith<TResult>(this Task<TResult> antecedentTask, Action<Task<TResult>> operation)
        {
            return FollowOnFaultWithImplementation<TResult>(antecedentTask, t => ((Task<TResult>) t).Result, delegate (Task t) {
                operation((Task<TResult>) t);
            });
        }

        private static Task<TResult> FollowOnFaultWithImplementation<TResult>(Task antecedentTask, Func<Task, TResult> getTaskResult, Action<Task> operation)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            antecedentTask.ContinueWith(delegate (Task t) {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        taskCompletionSource.TrySetResult(getTaskResult(t));
                        return;

                    case TaskStatus.Canceled:
                        taskCompletionSource.TrySetCanceled();
                        break;

                    case TaskStatus.Faulted:
                        try
                        {
                            operation(t);
                            taskCompletionSource.TrySetException(t.Exception);
                        }
                        catch (Exception exception)
                        {
                            if (!ExceptionUtils.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            AggregateException exception2 = new AggregateException(new Exception[] { t.Exception, exception });
                            taskCompletionSource.TrySetException(exception2);
                        }
                        break;

                    default:
                        return;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).IgnoreExceptions();
            return taskCompletionSource.Task;
        }

        internal static Task FollowOnSuccessWith<TAntecedentTaskResult>(this Task<TAntecedentTaskResult> antecedentTask, Action<Task<TAntecedentTaskResult>> operation)
        {
            return FollowOnSuccessWithImplementation<object>(antecedentTask, delegate (Task t) {
                operation((Task<TAntecedentTaskResult>) t);
                return null;
            });
        }

        internal static Task<TFollowupTaskResult> FollowOnSuccessWith<TAntecedentTaskResult, TFollowupTaskResult>(this Task<TAntecedentTaskResult> antecedentTask, Func<Task<TAntecedentTaskResult>, TFollowupTaskResult> operation)
        {
            return FollowOnSuccessWithImplementation<TFollowupTaskResult>(antecedentTask, t => operation((Task<TAntecedentTaskResult>) t));
        }

        internal static Task FollowOnSuccessWith(this Task antecedentTask, Action<Task> operation)
        {
            return FollowOnSuccessWithImplementation<object>(antecedentTask, delegate (Task t) {
                operation(t);
                return null;
            });
        }

        internal static Task<TFollowupTaskResult> FollowOnSuccessWith<TFollowupTaskResult>(this Task antecedentTask, Func<Task, TFollowupTaskResult> operation)
        {
            return FollowOnSuccessWithImplementation<TFollowupTaskResult>(antecedentTask, operation);
        }

        private static void FollowOnSuccessWithContinuation<TResult>(Task antecedentTask, TaskCompletionSource<TResult> taskCompletionSource, Func<Task, TResult> operation)
        {
            switch (antecedentTask.Status)
            {
                case TaskStatus.RanToCompletion:
                    try
                    {
                        taskCompletionSource.TrySetResult(operation(antecedentTask));
                    }
                    catch (Exception exception)
                    {
                        if (!ExceptionUtils.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        taskCompletionSource.TrySetException(exception);
                    }
                    break;

                case TaskStatus.Canceled:
                    taskCompletionSource.TrySetCanceled();
                    break;

                case TaskStatus.Faulted:
                    taskCompletionSource.TrySetException(antecedentTask.Exception);
                    return;

                default:
                    return;
            }
        }

        private static Task<TResult> FollowOnSuccessWithImplementation<TResult>(Task antecedentTask, Func<Task, TResult> operation)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            antecedentTask.ContinueWith(delegate (Task taskToContinueOn) {
                FollowOnSuccessWithContinuation<TResult>(taskToContinueOn, taskCompletionSource, operation);
            }, TaskContinuationOptions.ExecuteSynchronously).IgnoreExceptions();
            return taskCompletionSource.Task;
        }

        internal static Task FollowOnSuccessWithTask(this Task antecedentTask, Func<Task, Task> operation)
        {
            TaskCompletionSource<Task> taskCompletionSource = new TaskCompletionSource<Task>();
            antecedentTask.ContinueWith(delegate (Task taskToContinueOn) {
                FollowOnSuccessWithContinuation<Task>(taskToContinueOn, taskCompletionSource, operation);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return taskCompletionSource.Task.Unwrap();
        }

        internal static Task<TFollowupTaskResult> FollowOnSuccessWithTask<TAntecedentTaskResult, TFollowupTaskResult>(this Task<TAntecedentTaskResult> antecedentTask, Func<Task<TAntecedentTaskResult>, Task<TFollowupTaskResult>> operation)
        {
            TaskCompletionSource<Task<TFollowupTaskResult>> taskCompletionSource = new TaskCompletionSource<Task<TFollowupTaskResult>>();
            antecedentTask.ContinueWith(delegate (Task<TAntecedentTaskResult> taskToContinueOn) {
                FollowOnSuccessWithContinuation<Task<TFollowupTaskResult>>(taskToContinueOn, taskCompletionSource, taskForOperation => operation((Task<TAntecedentTaskResult>) taskForOperation));
            }, TaskContinuationOptions.ExecuteSynchronously);
            return taskCompletionSource.Task.Unwrap<TFollowupTaskResult>();
        }

        internal static Task FollowOnSuccessWithTask<TAntecedentTaskResult>(this Task<TAntecedentTaskResult> antecedentTask, Func<Task<TAntecedentTaskResult>, Task> operation)
        {
            TaskCompletionSource<Task> taskCompletionSource = new TaskCompletionSource<Task>();
            antecedentTask.ContinueWith(delegate (Task<TAntecedentTaskResult> taskToContinueOn) {
                FollowOnSuccessWithContinuation<Task>(taskToContinueOn, taskCompletionSource, taskForOperation => operation((Task<TAntecedentTaskResult>) taskForOperation));
            }, TaskContinuationOptions.ExecuteSynchronously);
            return taskCompletionSource.Task.Unwrap();
        }

        internal static Task<T> GetCompletedTask<T>(T value)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            source.SetResult(value);
            return source.Task;
        }

        internal static Task GetFaultedTask(Exception exception)
        {
            return GetFaultedTask<object>(exception);
        }

        internal static Task<T> GetFaultedTask<T>(Exception exception)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            source.SetException(exception);
            return source.Task;
        }

        internal static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            return (factory.Scheduler ?? TaskScheduler.Current);
        }

        internal static Task GetTaskForSynchronousOperation(Action synchronousOperation)
        {
            try
            {
                synchronousOperation();
                return CompletedTask;
            }
            catch (Exception exception)
            {
                if (!ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                return GetFaultedTask(exception);
            }
        }

        internal static Task<T> GetTaskForSynchronousOperation<T>(Func<T> synchronousOperation)
        {
            try
            {
                return GetCompletedTask<T>(synchronousOperation());
            }
            catch (Exception exception)
            {
                if (!ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                return GetFaultedTask<T>(exception);
            }
        }

        internal static Task GetTaskForSynchronousOperationReturningTask(Func<Task> synchronousOperation)
        {
            try
            {
                return synchronousOperation();
            }
            catch (Exception exception)
            {
                if (!ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                return GetFaultedTask(exception);
            }
        }

        internal static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(delegate (Task t) {
                AggregateException exception = t.Exception;
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            return task;
        }

        internal static Task Iterate(this TaskFactory factory, IEnumerable<Task> source)
        {
            IEnumerator<Task> enumerator = source.GetEnumerator();
            TaskCompletionSource<object> trc = new TaskCompletionSource<object>(null, factory.CreationOptions);
            trc.Task.ContinueWith(delegate (Task<object> _) {
                enumerator.Dispose();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            Action<Task> recursiveBody = null;
            recursiveBody = delegate (Task antecedent) {
                try
                {
                    if ((antecedent != null) && antecedent.IsFaulted)
                    {
                        trc.TrySetException(antecedent.Exception);
                    }
                    else if (enumerator.MoveNext())
                    {
                        enumerator.Current.ContinueWith(recursiveBody).IgnoreExceptions();
                    }
                    else
                    {
                        trc.TrySetResult(null);
                    }
                }
                catch (Exception exception)
                {
                    if (!ExceptionUtils.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    OperationCanceledException exception2 = exception as OperationCanceledException;
                    if ((exception2 != null) && (exception2.CancellationToken == factory.CancellationToken))
                    {
                        trc.TrySetCanceled();
                    }
                    else
                    {
                        trc.TrySetException(exception);
                    }
                }
            };
            factory.StartNew(delegate {
                recursiveBody(null);
            }, CancellationToken.None, TaskCreationOptions.None, factory.GetTargetScheduler()).IgnoreExceptions();
            return trc.Task;
        }

        internal static Task CompletedTask
        {
            get
            {
                if (completedTask == null)
                {
                    TaskCompletionSource<object> source = new TaskCompletionSource<object>();
                    source.SetResult(null);
                    completedTask = source.Task;
                }
                return completedTask;
            }
        }
    }
}

