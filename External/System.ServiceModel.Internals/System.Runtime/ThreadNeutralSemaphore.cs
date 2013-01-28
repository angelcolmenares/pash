using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Runtime
{
	internal class ThreadNeutralSemaphore
	{
		private static Action<object, TimeoutException> enteredAsyncCallback;

		private bool aborted;

		private Func<Exception> abortedExceptionGenerator;

		private int count;

		private int maxCount;

		private object ThisLock;

		private Queue<AsyncWaitHandle> waiters;

		private static Action<object, TimeoutException> EnteredAsyncCallback
		{
			get
			{
				if (ThreadNeutralSemaphore.enteredAsyncCallback == null)
				{
					ThreadNeutralSemaphore.enteredAsyncCallback = new Action<object, TimeoutException>(ThreadNeutralSemaphore.OnEnteredAsync);
				}
				return ThreadNeutralSemaphore.enteredAsyncCallback;
			}
		}

		private Queue<AsyncWaitHandle> Waiters
		{
			get
			{
				if (this.waiters == null)
				{
					this.waiters = new Queue<AsyncWaitHandle>();
				}
				return this.waiters;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ThreadNeutralSemaphore(int maxCount) : this(maxCount, null)
		{
		}

		public ThreadNeutralSemaphore(int maxCount, Func<Exception> abortedExceptionGenerator)
		{
			this.ThisLock = new object();
			this.maxCount = maxCount;
			this.abortedExceptionGenerator = abortedExceptionGenerator;
		}

		public void Abort()
		{
			lock (this.ThisLock)
			{
				if (!this.aborted)
				{
					this.aborted = true;
					if (this.waiters != null)
					{
						while (this.waiters.Count > 0)
						{
							AsyncWaitHandle asyncWaitHandle = this.waiters.Dequeue();
							asyncWaitHandle.Set();
						}
					}
				}
			}
		}

		internal static TimeoutException CreateEnterTimedOutException(TimeSpan timeout)
		{
			return new TimeoutException(InternalSR.LockTimeoutExceptionMessage(timeout));
		}

		private Exception CreateObjectAbortedException()
		{
			if (this.abortedExceptionGenerator == null)
			{
				return new OperationCanceledException(InternalSR.ThreadNeutralSemaphoreAborted);
			}
			else
			{
				return this.abortedExceptionGenerator();
			}
		}

		public void Enter(TimeSpan timeout)
		{
			if (this.TryEnter(timeout))
			{
				return;
			}
			else
			{
				throw Fx.Exception.AsError(ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout));
			}
		}

		public bool EnterAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
		{
			bool flag;
			lock (this.ThisLock)
			{
				if (!this.aborted)
				{
					if (this.count >= this.maxCount)
					{
						AsyncWaitHandle asyncWaitHandle = new AsyncWaitHandle();
						this.Waiters.Enqueue(asyncWaitHandle);
						return asyncWaitHandle.WaitAsync(ThreadNeutralSemaphore.EnteredAsyncCallback, new ThreadNeutralSemaphore.EnterAsyncData(this, asyncWaitHandle, callback, state), timeout);
					}
					else
					{
						ThreadNeutralSemaphore threadNeutralSemaphore = this;
						threadNeutralSemaphore.count = threadNeutralSemaphore.count + 1;
						flag = true;
					}
				}
				else
				{
					throw Fx.Exception.AsError(this.CreateObjectAbortedException());
				}
			}
			return flag;
		}

		private AsyncWaitHandle EnterCore()
		{
			AsyncWaitHandle asyncWaitHandle;
			lock (this.ThisLock)
			{
				if (!this.aborted)
				{
					if (this.count >= this.maxCount)
					{
						AsyncWaitHandle asyncWaitHandle1 = new AsyncWaitHandle();
						this.Waiters.Enqueue(asyncWaitHandle1);
						return asyncWaitHandle1;
					}
					else
					{
						ThreadNeutralSemaphore threadNeutralSemaphore = this;
						threadNeutralSemaphore.count = threadNeutralSemaphore.count + 1;
						asyncWaitHandle = null;
					}
				}
				else
				{
					throw Fx.Exception.AsError(this.CreateObjectAbortedException());
				}
			}
			return asyncWaitHandle;
		}

		public int Exit()
		{
			int num;
			int num1 = -1;
			lock (this.ThisLock)
			{
				if (!this.aborted)
				{
					if (this.count != 0)
					{
						if (this.waiters == null || this.waiters.Count == 0)
						{
							ThreadNeutralSemaphore threadNeutralSemaphore = this;
							threadNeutralSemaphore.count = threadNeutralSemaphore.count - 1;
							num = this.count;
						}
						else
						{
							AsyncWaitHandle asyncWaitHandle = this.waiters.Dequeue();
							num1 = this.count;
							asyncWaitHandle.Set();
							return num1;
						}
					}
					else
					{
						string invalidSemaphoreExit = InternalSR.InvalidSemaphoreExit;
						throw Fx.Exception.AsError(new SynchronizationLockException(invalidSemaphoreExit));
					}
				}
				else
				{
					num = num1;
				}
			}
			return num;
		}

		private static void OnEnteredAsync(object state, TimeoutException exception)
		{
			ThreadNeutralSemaphore.EnterAsyncData enterAsyncDatum = (ThreadNeutralSemaphore.EnterAsyncData)state;
			ThreadNeutralSemaphore semaphore = enterAsyncDatum.Semaphore;
			Exception exception1 = exception;
			if (exception != null && !semaphore.RemoveWaiter(enterAsyncDatum.Waiter))
			{
				exception1 = null;
			}
			if (semaphore.aborted)
			{
				exception1 = semaphore.CreateObjectAbortedException();
			}
			enterAsyncDatum.Callback(enterAsyncDatum.State, exception1);
		}

		private bool RemoveWaiter(AsyncWaitHandle waiter)
		{
			bool flag = false;
			lock (this.ThisLock)
			{
				for (int i = this.Waiters.Count; i > 0; i--)
				{
					AsyncWaitHandle asyncWaitHandle = this.Waiters.Dequeue();
					if (!object.ReferenceEquals(asyncWaitHandle, waiter))
					{
						this.Waiters.Enqueue(asyncWaitHandle);
					}
					else
					{
					}
				}
			}
			return flag;
		}

		public bool TryEnter()
		{
			bool flag;
			lock (this.ThisLock)
			{
				if (this.count >= this.maxCount)
				{
					flag = false;
				}
				else
				{
					ThreadNeutralSemaphore threadNeutralSemaphore = this;
					threadNeutralSemaphore.count = threadNeutralSemaphore.count + 1;
					flag = true;
				}
			}
			return flag;
		}

		public bool TryEnter(TimeSpan timeout)
		{
			AsyncWaitHandle asyncWaitHandle = this.EnterCore();
			if (asyncWaitHandle == null)
			{
				return true;
			}
			else
			{
				bool flag = !asyncWaitHandle.Wait(timeout);
				if (!this.aborted)
				{
					if (flag && !this.RemoveWaiter(asyncWaitHandle))
					{
						flag = false;
					}
					return !flag;
				}
				else
				{
					throw Fx.Exception.AsError(this.CreateObjectAbortedException());
				}
			}
		}

		private class EnterAsyncData
		{
			public FastAsyncCallback Callback
			{
				get;
				set;
			}

			public ThreadNeutralSemaphore Semaphore
			{
				get;
				set;
			}

			public object State
			{
				get;
				set;
			}

			public AsyncWaitHandle Waiter
			{
				get;
				set;
			}

			public EnterAsyncData(ThreadNeutralSemaphore semaphore, AsyncWaitHandle waiter, FastAsyncCallback callback, object state)
			{
				this.Waiter = waiter;
				this.Semaphore = semaphore;
				this.Callback = callback;
				this.State = state;
			}
		}
	}
}