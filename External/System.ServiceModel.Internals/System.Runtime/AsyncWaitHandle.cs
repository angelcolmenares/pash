using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal class AsyncWaitHandle
	{
		private static Action<object> timerCompleteCallback;

		private List<AsyncWaitHandle.AsyncWaiter> asyncWaiters;

		private bool isSignaled;

		private EventResetMode resetMode;

		private object syncObject;

		private int syncWaiterCount;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public AsyncWaitHandle() : this(0)
		{
		}

		public AsyncWaitHandle(EventResetMode resetMode)
		{
			this.resetMode = resetMode;
			this.syncObject = new object();
		}

		private static void OnTimerComplete(object state)
		{
			AsyncWaitHandle.AsyncWaiter asyncWaiter = (AsyncWaitHandle.AsyncWaiter)state;
			AsyncWaitHandle parent = asyncWaiter.Parent;
			bool flag = false;
			lock (parent.syncObject)
			{
				if (parent.asyncWaiters != null && parent.asyncWaiters.Remove(asyncWaiter))
				{
					asyncWaiter.TimedOut = true;
				}
			}
			asyncWaiter.CancelTimer();
			if (flag)
			{
				asyncWaiter.Call();
			}
		}

		public void Reset()
		{
			this.isSignaled = false;
		}

		public void Set()
		{
			List<AsyncWaitHandle.AsyncWaiter> asyncWaiters = null;
			AsyncWaitHandle.AsyncWaiter item = null;
			if (!this.isSignaled)
			{
				lock (this.syncObject)
				{
					if (!this.isSignaled)
					{
						if (this.resetMode != EventResetMode.ManualReset)
						{
							if (this.syncWaiterCount <= 0)
							{
								if (this.asyncWaiters == null || this.asyncWaiters.Count <= 0)
								{
									this.isSignaled = true;
								}
								else
								{
									item = this.asyncWaiters[0];
									this.asyncWaiters.RemoveAt(0);
								}
							}
							else
							{
								Monitor.Pulse(this.syncObject);
							}
						}
						else
						{
							this.isSignaled = true;
							Monitor.PulseAll(this.syncObject);
							asyncWaiters = this.asyncWaiters;
							this.asyncWaiters = null;
						}
					}
				}
			}
			if (asyncWaiters != null)
			{
				foreach (AsyncWaitHandle.AsyncWaiter asyncWaiter in asyncWaiters)
				{
					asyncWaiter.CancelTimer();
					asyncWaiter.Call();
				}
			}
			if (item != null)
			{
				item.CancelTimer();
				item.Call();
			}
		}

		public bool Wait(TimeSpan timeout)
		{
			bool flag;
			if (!this.isSignaled || this.isSignaled && this.resetMode == EventResetMode.AutoReset)
			{
				lock (this.syncObject)
				{
					if (!this.isSignaled || this.resetMode != EventResetMode.AutoReset)
					{
						if (!this.isSignaled)
						{
							bool flag1 = false;
							try
							{
								try
								{
								}
								finally
								{
									AsyncWaitHandle asyncWaitHandle = this;
									asyncWaitHandle.syncWaiterCount = asyncWaitHandle.syncWaiterCount + 1;
									flag1 = true;
								}
								if (timeout != TimeSpan.MaxValue)
								{
									if (!Monitor.Wait(this.syncObject, timeout))
									{
										flag = false;
										return flag;
									}
								}
								else
								{
									if (!Monitor.Wait(this.syncObject, -1))
									{
										flag = false;
										return flag;
									}
								}
							}
							finally
							{
								if (flag1)
								{
									AsyncWaitHandle asyncWaitHandle1 = this;
									asyncWaitHandle1.syncWaiterCount = asyncWaitHandle1.syncWaiterCount - 1;
								}
							}
						}
					}
					else
					{
						this.isSignaled = false;
					}
					return true;
				}
				return flag;
			}
			return true;
		}

		public bool WaitAsync(Action<object, TimeoutException> callback, object state, TimeSpan timeout)
		{
			bool flag;
			if (!this.isSignaled || this.isSignaled && this.resetMode == EventResetMode.AutoReset)
			{
				lock (this.syncObject)
				{
					if (!this.isSignaled || this.resetMode != EventResetMode.AutoReset)
					{
						if (!this.isSignaled)
						{
							AsyncWaitHandle.AsyncWaiter asyncWaiter = new AsyncWaitHandle.AsyncWaiter(this, callback, state);
							if (this.asyncWaiters == null)
							{
								this.asyncWaiters = new List<AsyncWaitHandle.AsyncWaiter>();
							}
							this.asyncWaiters.Add(asyncWaiter);
							if (timeout != TimeSpan.MaxValue)
							{
								if (AsyncWaitHandle.timerCompleteCallback == null)
								{
									AsyncWaitHandle.timerCompleteCallback = new Action<object>(AsyncWaitHandle.OnTimerComplete);
								}
								asyncWaiter.SetTimer(AsyncWaitHandle.timerCompleteCallback, asyncWaiter, timeout);
							}
							flag = false;
							return flag;
						}
					}
					else
					{
						this.isSignaled = false;
					}
					return true;
				}
				return flag;
			}
			return true;
		}

		private class AsyncWaiter : ActionItem
		{
			[SecurityCritical]
			private Action<object, TimeoutException> callback;

			[SecurityCritical]
			private object state;

			private IOThreadTimer timer;

			private TimeSpan originalTimeout;

			public AsyncWaitHandle Parent
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get;
				private set;
			}

			public bool TimedOut
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get;
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				set;
			}

			[SecuritySafeCritical]
			public AsyncWaiter(AsyncWaitHandle parent, Action<object, TimeoutException> callback, object state)
			{
				this.Parent = parent;
				this.callback = callback;
				this.state = state;
			}

			[SecuritySafeCritical]
			public void Call()
			{
				base.Schedule();
			}

			public void CancelTimer()
			{
				if (this.timer != null)
				{
					this.timer.Cancel();
					this.timer = null;
				}
			}

			[SecurityCritical]
			protected override void Invoke()
			{
				TimeoutException timeoutException;
				Action<object, TimeoutException> action = this.callback;
				object obj = this.state;
				if (this.TimedOut)
				{
					timeoutException = new TimeoutException(InternalSR.TimeoutOnOperation(this.originalTimeout));
				}
				else
				{
					timeoutException = null;
				}
				action(obj, timeoutException);
			}

			public void SetTimer(Action<object> callback, object state, TimeSpan timeout)
			{
				if (this.timer == null)
				{
					this.originalTimeout = timeout;
					this.timer = new IOThreadTimer(callback, state, false);
					this.timer.Set(timeout);
					return;
				}
				else
				{
					throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.MustCancelOldTimer));
				}
			}
		}
	}
}