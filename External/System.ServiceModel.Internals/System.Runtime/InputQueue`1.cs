using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Runtime
{
	internal sealed class InputQueue<T> : IDisposable
	where T : class
	{
		private static Action<object> completeOutstandingReadersCallback;

		private static Action<object> completeWaitersFalseCallback;

		private static Action<object> completeWaitersTrueCallback;

		private static Action<object> onDispatchCallback;

		private static Action<object> onInvokeDequeuedCallback;

		private InputQueue<T>.QueueState queueState;

		private InputQueue<T>.ItemQueue itemQueue;

		private Queue<InputQueue<T>.IQueueReader> readerQueue;

		private List<InputQueue<T>.IQueueWaiter> waiterList;

		private Func<Action<AsyncCallback, IAsyncResult>> AsyncCallbackGenerator
		{
			get;set;
		}

		public Action<T> DisposeItemCallback
		{
			get;set;
		}

		public int PendingCount
		{
			get
			{
				int itemCount;
				lock (this.ThisLock)
				{
					itemCount = this.itemQueue.ItemCount;
				}
				return itemCount;
			}
		}

		private object ThisLock
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.itemQueue;
			}
		}

		public InputQueue()
		{
			this.itemQueue = new InputQueue<T>.ItemQueue();
			this.readerQueue = new Queue<InputQueue<T>.IQueueReader>();
			this.waiterList = new List<InputQueue<T>.IQueueWaiter>();
			this.queueState = InputQueue<T>.QueueState.Open;
		}

		public InputQueue(Func<Action<AsyncCallback, IAsyncResult>> asyncCallbackGenerator) : this()
		{
			this.AsyncCallbackGenerator = asyncCallbackGenerator;
		}

		public IAsyncResult BeginDequeue(TimeSpan timeout, AsyncCallback callback, object state)
		{
			IAsyncResult asyncResult;
			InputQueue<T>.Item item = new InputQueue<T>.Item();
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Open)
				{
					if (this.queueState == InputQueue<T>.QueueState.Shutdown)
					{
						if (!this.itemQueue.HasAvailableItem)
						{
							if (this.itemQueue.HasAnyItem)
							{
								InputQueue<T>.AsyncQueueReader asyncQueueReader = new InputQueue<T>.AsyncQueueReader(this, timeout, callback, state);
								this.readerQueue.Enqueue(asyncQueueReader);
								asyncResult = asyncQueueReader;
								return asyncResult;
							}
						}
						else
						{
							item = this.itemQueue.DequeueAvailableItem();
						}
					}
				}
				else
				{
					if (!this.itemQueue.HasAvailableItem)
					{
						InputQueue<T>.AsyncQueueReader asyncQueueReader1 = new InputQueue<T>.AsyncQueueReader(this, timeout, callback, state);
						this.readerQueue.Enqueue(asyncQueueReader1);
						asyncResult = asyncQueueReader1;
						return asyncResult;
					}
					else
					{
						item = this.itemQueue.DequeueAvailableItem();
					}
				}
				InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
				return new CompletedAsyncResult<T>(item.GetValue(), callback, state);
			}
			return asyncResult;
		}

		public IAsyncResult BeginWaitForItem(TimeSpan timeout, AsyncCallback callback, object state)
		{
			IAsyncResult asyncResult;
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Open)
				{
					if (this.queueState == InputQueue<T>.QueueState.Shutdown && !this.itemQueue.HasAvailableItem && this.itemQueue.HasAnyItem)
					{
						InputQueue<T>.AsyncQueueWaiter asyncQueueWaiter = new InputQueue<T>.AsyncQueueWaiter(timeout, callback, state);
						this.waiterList.Add(asyncQueueWaiter);
						asyncResult = asyncQueueWaiter;
						return asyncResult;
					}
				}
				else
				{
					if (!this.itemQueue.HasAvailableItem)
					{
						InputQueue<T>.AsyncQueueWaiter asyncQueueWaiter1 = new InputQueue<T>.AsyncQueueWaiter(timeout, callback, state);
						this.waiterList.Add(asyncQueueWaiter1);
						asyncResult = asyncQueueWaiter1;
						return asyncResult;
					}
				}
				return new CompletedAsyncResult<bool>(true, callback, state);
			}
			return asyncResult;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Close()
		{
			this.Dispose();
		}

		private static void CompleteOutstandingReadersCallback(object state)
		{
			InputQueue<T>.IQueueReader[] queueReaderArray = (InputQueue<T>.IQueueReader[])state;
			for (int i = 0; i < (int)queueReaderArray.Length; i++)
			{
				InputQueue<T>.Item item = new InputQueue<T>.Item();
				queueReaderArray[i].Set(item);
			}
		}

		private static void CompleteWaiters(bool itemAvailable, InputQueue<T>.IQueueWaiter[] waiters)
		{
			for (int i = 0; i < (int)waiters.Length; i++)
			{
				waiters[i].Set(itemAvailable);
			}
		}

		private static void CompleteWaitersFalseCallback(object state)
		{
			InputQueue<T>.CompleteWaiters(false, (InputQueue<T>.IQueueWaiter[])state);
		}

		private static void CompleteWaitersLater(bool itemAvailable, InputQueue<T>.IQueueWaiter[] waiters)
		{
			if (!itemAvailable)
			{
				if (InputQueue<T>.completeWaitersFalseCallback == null)
				{
					InputQueue<T>.completeWaitersFalseCallback = new Action<object>(InputQueue<T>.CompleteWaitersFalseCallback);
				}
				ActionItem.Schedule(InputQueue<T>.completeWaitersFalseCallback, waiters);
				return;
			}
			else
			{
				if (InputQueue<T>.completeWaitersTrueCallback == null)
				{
					InputQueue<T>.completeWaitersTrueCallback = new Action<object>(InputQueue<T>.CompleteWaitersTrueCallback);
				}
				ActionItem.Schedule(InputQueue<T>.completeWaitersTrueCallback, waiters);
				return;
			}
		}

		private static void CompleteWaitersTrueCallback(object state)
		{
			InputQueue<T>.CompleteWaiters(true, (InputQueue<T>.IQueueWaiter[])state);
		}

		public T Dequeue(TimeSpan timeout)
		{
			T t = null;
			if (this.Dequeue(timeout, out t))
			{
				return t;
			}
			else
			{
				throw Fx.Exception.AsError(new TimeoutException(InternalSR.TimeoutInputQueueDequeue(timeout)));
			}
		}

		public bool Dequeue(TimeSpan timeout, out T value)
		{
			bool flag;
			InputQueue<T>.WaitQueueReader waitQueueReader = null;
			InputQueue<T>.Item item = new InputQueue<T>.Item();
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Open)
				{
					if (this.queueState != InputQueue<T>.QueueState.Shutdown)
					{
						value = default(T);
						flag = true;
						return flag;
					}
					else
					{
						if (!this.itemQueue.HasAvailableItem)
						{
							if (!this.itemQueue.HasAnyItem)
							{
								value = default(T);
								flag = true;
								return flag;
							}
							else
							{
								waitQueueReader = new InputQueue<T>.WaitQueueReader(this);
								this.readerQueue.Enqueue(waitQueueReader);
							}
						}
						else
						{
							item = this.itemQueue.DequeueAvailableItem();
						}
					}
				}
				else
				{
					if (!this.itemQueue.HasAvailableItem)
					{
						waitQueueReader = new InputQueue<T>.WaitQueueReader(this);
						this.readerQueue.Enqueue(waitQueueReader);
					}
					else
					{
						item = this.itemQueue.DequeueAvailableItem();
					}
				}
				goto Label0;
			}
			return flag;
		Label0:
			if (waitQueueReader == null)
			{
				InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
				value = item.GetValue();
				return true;
			}
			else
			{
				return waitQueueReader.Wait(timeout, out value);
			}
		}

		public void Dispatch()
		{
			bool flag = false;
			bool flag1;
			InputQueue<T>.IQueueReader queueReader = null;
			InputQueue<T>.Item item = new InputQueue<T>.Item();
			InputQueue<T>.IQueueReader[] queueReaderArray = null;
			InputQueue<T>.IQueueWaiter[] queueWaiterArray = null;
			lock (this.ThisLock)
			{
				if (this.queueState == InputQueue<T>.QueueState.Closed)
				{
					flag1 = false;
				}
				else
				{
					flag1 = this.queueState != InputQueue<T>.QueueState.Shutdown;
				}
				this.GetWaiters(out queueWaiterArray);
				if (this.queueState != InputQueue<T>.QueueState.Closed)
				{
					this.itemQueue.MakePendingItemAvailable();
					if (this.readerQueue.Count > 0)
					{
						item = this.itemQueue.DequeueAvailableItem();
						queueReader = this.readerQueue.Dequeue();
						if (this.queueState == InputQueue<T>.QueueState.Shutdown && this.readerQueue.Count > 0 && this.itemQueue.ItemCount == 0)
						{
							queueReaderArray = new InputQueue<T>.IQueueReader[this.readerQueue.Count];
							this.readerQueue.CopyTo(queueReaderArray, 0);
							this.readerQueue.Clear();
						}
					}
				}
			}
			if (queueReaderArray != null)
			{
				if (InputQueue<T>.completeOutstandingReadersCallback == null)
				{
					InputQueue<T>.completeOutstandingReadersCallback = new Action<object>(InputQueue<T>.CompleteOutstandingReadersCallback);
				}
				ActionItem.Schedule(InputQueue<T>.completeOutstandingReadersCallback, queueReaderArray);
			}
			if (queueWaiterArray != null)
			{
				InputQueue<T>.CompleteWaitersLater(flag, queueWaiterArray);
			}
			if (queueReader != null)
			{
				InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
				queueReader.Set(item);
			}
		}

		public void Dispose()
		{
			bool flag = false;
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Closed)
				{
					this.queueState = InputQueue<T>.QueueState.Closed;
				}
			}
			if (flag)
			{
				while (this.readerQueue.Count > 0)
				{
					InputQueue<T>.IQueueReader queueReader = this.readerQueue.Dequeue();
					InputQueue<T>.Item item = new InputQueue<T>.Item();
					queueReader.Set(item);
				}
				while (this.itemQueue.HasAnyItem)
				{
					InputQueue<T>.Item item1 = this.itemQueue.DequeueAnyItem();
					this.DisposeItem(item1);
					InputQueue<T>.InvokeDequeuedCallback(item1.DequeuedCallback);
				}
			}
		}

		private void DisposeItem(InputQueue<T>.Item item)
		{
			T value = item.Value;
			if (value != null)
			{
				if ((object)value as IDisposable == null)
				{
					Action<T> disposeItemCallback = this.DisposeItemCallback;
					if (disposeItemCallback != null)
					{
						disposeItemCallback(value);
					}
				}
				else
				{
					((IDisposable)(object)value).Dispose();
					return;
				}
			}
		}

		public bool EndDequeue(IAsyncResult result, out T value)
		{
			CompletedAsyncResult<T> completedAsyncResult = result as CompletedAsyncResult<T>;
			if (completedAsyncResult == null)
			{
				return InputQueue<T>.AsyncQueueReader.End(result, out value);
			}
			else
			{
				value = CompletedAsyncResult<T>.End(result);
				return true;
			}
		}

		public T EndDequeue(IAsyncResult result)
		{
			T t = null;
			if (this.EndDequeue(result, out t))
			{
				return t;
			}
			else
			{
				throw Fx.Exception.AsError(new TimeoutException());
			}
		}

		public bool EndWaitForItem(IAsyncResult result)
		{
			CompletedAsyncResult<bool> completedAsyncResult = result as CompletedAsyncResult<bool>;
			if (completedAsyncResult == null)
			{
				return InputQueue<T>.AsyncQueueWaiter.End(result);
			}
			else
			{
				return CompletedAsyncResult<bool>.End(result);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void EnqueueAndDispatch(T item)
		{
			this.EnqueueAndDispatch(item, null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void EnqueueAndDispatch(T item, Action dequeuedCallback)
		{
			this.EnqueueAndDispatch(item, dequeuedCallback, true);
		}

		public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
		{
			this.EnqueueAndDispatch(new InputQueue<T>.Item(exception, dequeuedCallback), canDispatchOnThisThread);
		}

		public void EnqueueAndDispatch(T item, Action dequeuedCallback, bool canDispatchOnThisThread)
		{
			this.EnqueueAndDispatch(new InputQueue<T>.Item(item, dequeuedCallback), canDispatchOnThisThread);
		}

		private void EnqueueAndDispatch(InputQueue<T>.Item item, bool canDispatchOnThisThread)
		{
			bool flag = false;
			bool flag1;
			bool flag2 = false;
			InputQueue<T>.IQueueReader queueReader = null;
			bool flag3 = false;
			InputQueue<T>.IQueueWaiter[] queueWaiterArray = null;
			lock (this.ThisLock)
			{
				if (this.queueState == InputQueue<T>.QueueState.Closed)
				{
					flag1 = false;
				}
				else
				{
					flag1 = this.queueState != InputQueue<T>.QueueState.Shutdown;
				}
				this.GetWaiters(out queueWaiterArray);
				if (this.queueState != InputQueue<T>.QueueState.Open)
				{
				}
				else
				{
					if (!canDispatchOnThisThread)
					{
						if (this.readerQueue.Count != 0)
						{
							this.itemQueue.EnqueuePendingItem(item);
						}
						else
						{
							this.itemQueue.EnqueueAvailableItem(item);
						}
					}
					else
					{
						if (this.readerQueue.Count != 0)
						{
							queueReader = this.readerQueue.Dequeue();
						}
						else
						{
							this.itemQueue.EnqueueAvailableItem(item);
						}
					}
				}
			}
			if (queueWaiterArray != null)
			{
				if (!canDispatchOnThisThread)
				{
					InputQueue<T>.CompleteWaitersLater(flag, queueWaiterArray);
				}
				else
				{
					InputQueue<T>.CompleteWaiters(flag, queueWaiterArray);
				}
			}
			if (queueReader != null)
			{
				InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
				queueReader.Set(item);
			}
			if (!flag3)
			{
				if (flag2)
				{
					InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
					this.DisposeItem(item);
				}
				return;
			}
			else
			{
				if (InputQueue<T>.onDispatchCallback == null)
				{
					InputQueue<T>.onDispatchCallback = new Action<object>(InputQueue<T>.OnDispatchCallback);
				}
				ActionItem.Schedule(InputQueue<T>.onDispatchCallback, this);
				return;
			}
		}

		public bool EnqueueWithoutDispatch(T item, Action dequeuedCallback)
		{
			return this.EnqueueWithoutDispatch(new InputQueue<T>.Item(item, dequeuedCallback));
		}

		public bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
		{
			return this.EnqueueWithoutDispatch(new InputQueue<T>.Item(exception, dequeuedCallback));
		}

		private bool EnqueueWithoutDispatch(InputQueue<T>.Item item)
		{
			bool flag;
			lock (this.ThisLock)
			{
				if (this.queueState == InputQueue<T>.QueueState.Closed || this.queueState == InputQueue<T>.QueueState.Shutdown)
				{
					this.DisposeItem(item);
					InputQueue<T>.InvokeDequeuedCallbackLater(item.DequeuedCallback);
					return false;
				}
				else
				{
					if (this.readerQueue.Count != 0 || this.waiterList.Count != 0)
					{
						this.itemQueue.EnqueuePendingItem(item);
						flag = true;
					}
					else
					{
						this.itemQueue.EnqueueAvailableItem(item);
						flag = false;
					}
				}
			}
			return flag;
		}

		private void GetWaiters(out InputQueue<T>.IQueueWaiter[] waiters)
		{
			if (this.waiterList.Count <= 0)
			{
				waiters = null;
				return;
			}
			else
			{
				waiters = this.waiterList.ToArray();
				this.waiterList.Clear();
				return;
			}
		}

		private static void InvokeDequeuedCallback(Action dequeuedCallback)
		{
			if (dequeuedCallback != null)
			{
				dequeuedCallback();
			}
		}

		private static void InvokeDequeuedCallbackLater(Action dequeuedCallback)
		{
			if (dequeuedCallback != null)
			{
				if (InputQueue<T>.onInvokeDequeuedCallback == null)
				{
					InputQueue<T>.onInvokeDequeuedCallback = new Action<object>(InputQueue<T>.OnInvokeDequeuedCallback);
				}
				ActionItem.Schedule(InputQueue<T>.onInvokeDequeuedCallback, dequeuedCallback);
			}
		}

		private static void OnDispatchCallback(object state)
		{
			((InputQueue<T>)state).Dispatch();
		}

		private static void OnInvokeDequeuedCallback(object state)
		{
			Action action = (Action)state;
			action();
		}

		private bool RemoveReader(InputQueue<T>.IQueueReader reader)
		{
			bool flag;
			lock (this.ThisLock)
			{
				if (this.queueState == InputQueue<T>.QueueState.Open || this.queueState == InputQueue<T>.QueueState.Shutdown)
				{
					bool flag1 = false;
					for (int i = this.readerQueue.Count; i > 0; i--)
					{
						InputQueue<T>.IQueueReader queueReader = this.readerQueue.Dequeue();
						if (!object.ReferenceEquals(queueReader, reader))
						{
							this.readerQueue.Enqueue(queueReader);
						}
						else
						{
							flag1 = true;
						}
					}
					flag = flag1;
				}
				else
				{
					return false;
				}
			}
			return flag;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Shutdown()
		{
			this.Shutdown(null);
		}

		public void Shutdown(Func<Exception> pendingExceptionGenerator)
		{
			Exception exception;
			InputQueue<T>.IQueueReader[] queueReaderArray = null;
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Shutdown)
				{
					if (this.queueState != InputQueue<T>.QueueState.Closed)
					{
						this.queueState = InputQueue<T>.QueueState.Shutdown;
						if (this.readerQueue.Count > 0 && this.itemQueue.ItemCount == 0)
						{
							queueReaderArray = new InputQueue<T>.IQueueReader[this.readerQueue.Count];
							this.readerQueue.CopyTo(queueReaderArray, 0);
							this.readerQueue.Clear();
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			if (queueReaderArray != null)
			{
				for (int i = 0; i < (int)queueReaderArray.Length; i++)
				{
					if (pendingExceptionGenerator != null)
					{
						exception = pendingExceptionGenerator();
					}
					else
					{
						exception = null;
					}
					Exception exception1 = exception;
					queueReaderArray[i].Set(new InputQueue<T>.Item(exception1, null));
				}
			}
		}

		public bool WaitForItem(TimeSpan timeout)
		{
			bool flag;
			InputQueue<T>.WaitQueueWaiter waitQueueWaiter = null;
			bool flag1 = false;
			lock (this.ThisLock)
			{
				if (this.queueState != InputQueue<T>.QueueState.Open)
				{
					if (this.queueState != InputQueue<T>.QueueState.Shutdown)
					{
						flag = true;
						return flag;
					}
					else
					{
						if (!this.itemQueue.HasAvailableItem)
						{
							if (!this.itemQueue.HasAnyItem)
							{
								flag = true;
								return flag;
							}
							else
							{
								waitQueueWaiter = new InputQueue<T>.WaitQueueWaiter();
								this.waiterList.Add(waitQueueWaiter);
							}
						}
						else
						{
							flag1 = true;
						}
					}
				}
				else
				{
					if (!this.itemQueue.HasAvailableItem)
					{
						waitQueueWaiter = new InputQueue<T>.WaitQueueWaiter();
						this.waiterList.Add(waitQueueWaiter);
					}
					else
					{
						flag1 = true;
					}
				}
				goto Label0;
			}
			return flag;
		Label0:
			if (waitQueueWaiter == null)
			{
				return flag1;
			}
			else
			{
				return waitQueueWaiter.Wait(timeout);
			}
		}

		private class AsyncQueueReader : AsyncResult, InputQueue<T>.IQueueReader
		{
			private static Action<object> timerCallback;

			private bool expired;

			private InputQueue<T> inputQueue;

			private T item;

			private IOThreadTimer timer;

			static AsyncQueueReader()
			{
				InputQueue<T>.AsyncQueueReader.timerCallback = new Action<object>(InputQueue<T>.AsyncQueueReader.TimerCallback);
			}

			public AsyncQueueReader(InputQueue<T> inputQueue, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
			{
				if (inputQueue.AsyncCallbackGenerator != null)
				{
					base.VirtualCallback = inputQueue.AsyncCallbackGenerator();
				}
				this.inputQueue = inputQueue;
				if (timeout != TimeSpan.MaxValue)
				{
					this.timer = new IOThreadTimer(InputQueue<T>.AsyncQueueReader.timerCallback, this, false);
					this.timer.Set(timeout);
				}
			}

			public static bool End(IAsyncResult result, out T value)
			{
				InputQueue<T>.AsyncQueueReader asyncQueueReader = AsyncResult.End<InputQueue<T>.AsyncQueueReader>(result);
				if (!asyncQueueReader.expired)
				{
					value = asyncQueueReader.item;
					return true;
				}
				else
				{
					value = default(T);
					return false;
				}
			}

			public void Set(InputQueue<T>.Item item)
			{
				this.item = item.Value;
				if (this.timer != null)
				{
					this.timer.Cancel();
				}
				base.Complete(false, item.Exception);
			}

			private static void TimerCallback(object state)
			{
				InputQueue<T>.AsyncQueueReader asyncQueueReader = (InputQueue<T>.AsyncQueueReader)state;
				if (asyncQueueReader.inputQueue.RemoveReader(asyncQueueReader))
				{
					asyncQueueReader.expired = true;
					asyncQueueReader.Complete(false);
				}
			}
		}

		private class AsyncQueueWaiter : AsyncResult, InputQueue<T>.IQueueWaiter
		{
			private static Action<object> timerCallback;

			private bool itemAvailable;

			private object thisLock;

			private IOThreadTimer timer;

			private object ThisLock
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.thisLock;
				}
			}

			static AsyncQueueWaiter()
			{
				InputQueue<T>.AsyncQueueWaiter.timerCallback = new Action<object>(InputQueue<T>.AsyncQueueWaiter.TimerCallback);
			}

			public AsyncQueueWaiter(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
			{
				this.thisLock = new object();
				if (timeout != TimeSpan.MaxValue)
				{
					this.timer = new IOThreadTimer(InputQueue<T>.AsyncQueueWaiter.timerCallback, this, false);
					this.timer.Set(timeout);
				}
			}

			public static bool End(IAsyncResult result)
			{
				InputQueue<T>.AsyncQueueWaiter asyncQueueWaiter = AsyncResult.End<InputQueue<T>.AsyncQueueWaiter>(result);
				return asyncQueueWaiter.itemAvailable;
			}

			public void Set(bool itemAvailable)
			{
				bool flag;
				bool flag1;
				lock (this.ThisLock)
				{
					if (this.timer == null)
					{
						flag1 = true;
					}
					else
					{
						flag1 = this.timer.Cancel();
					}
					flag = flag1;
					this.itemAvailable = itemAvailable;
				}
				if (flag)
				{
					base.Complete(false);
				}
			}

			private static void TimerCallback(object state)
			{
				InputQueue<T>.AsyncQueueWaiter asyncQueueWaiter = (InputQueue<T>.AsyncQueueWaiter)state;
				asyncQueueWaiter.Complete(false);
			}
		}

		private interface IQueueReader
		{
			void Set(InputQueue<T>.Item item);
		}

		private interface IQueueWaiter
		{
			void Set(bool itemAvailable);
		}

		private struct Item
		{
			private Action dequeuedCallback;

			private Exception exception;

			private T @value;

			public Action DequeuedCallback
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.dequeuedCallback;
				}
			}

			public Exception Exception
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.exception;
				}
			}

			public T Value
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.@value;
				}
			}

			public Item(T value, Action dequeuedCallback) : this(value, null, dequeuedCallback)
			{
			}

			public Item(Exception exception, Action dequeuedCallback) : this(default(T), exception, dequeuedCallback)
			{
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			private Item(T value, Exception exception, Action dequeuedCallback)
			{
				this.@value = value;
				this.exception = exception;
				this.dequeuedCallback = dequeuedCallback;
			}

			public T GetValue()
			{
				if (this.exception == null)
				{
					return this.@value;
				}
				else
				{
					throw Fx.Exception.AsError(this.exception);
				}
			}
		}

		private class ItemQueue
		{
			private int head;

			private InputQueue<T>.Item[] items;

			private int pendingCount;

			private int totalCount;

			public bool HasAnyItem
			{
				get
				{
					return this.totalCount > 0;
				}
			}

			public bool HasAvailableItem
			{
				get
				{
					return this.totalCount > this.pendingCount;
				}
			}

			public int ItemCount
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.totalCount;
				}
			}

			public ItemQueue()
			{
				this.items = new InputQueue<T>.Item[1];
			}

			public InputQueue<T>.Item DequeueAnyItem()
			{
				if (this.pendingCount == this.totalCount)
				{
					InputQueue<T>.ItemQueue itemQueue = this;
					itemQueue.pendingCount = itemQueue.pendingCount - 1;
				}
				return this.DequeueItemCore();
			}

			public InputQueue<T>.Item DequeueAvailableItem()
			{
				Fx.AssertAndThrow(this.totalCount != this.pendingCount, "ItemQueue does not contain any available items");
				return this.DequeueItemCore();
			}

			private InputQueue<T>.Item DequeueItemCore()
			{
				Fx.AssertAndThrow(this.totalCount != 0, "ItemQueue does not contain any items");
				InputQueue<T>.Item item = this.items[this.head];
				this.items[this.head] = new InputQueue<T>.Item();
				InputQueue<T>.ItemQueue itemQueue = this;
				itemQueue.totalCount = itemQueue.totalCount - 1;
				this.head = (this.head + 1) % (int)this.items.Length;
				return item;
			}

			public void EnqueueAvailableItem(InputQueue<T>.Item item)
			{
				this.EnqueueItemCore(item);
			}

			private void EnqueueItemCore(InputQueue<T>.Item item)
			{
				if (this.totalCount == (int)this.items.Length)
				{
					InputQueue<T>.Item[] itemArray = new InputQueue<T>.Item[(int)this.items.Length * 2];
					for (int i = 0; i < this.totalCount; i++)
					{
						itemArray[i] = this.items[(this.head + i) % (int)this.items.Length];
					}
					this.head = 0;
					this.items = itemArray;
				}
				int length = (this.head + this.totalCount) % (int)this.items.Length;
				this.items[length] = item;
				InputQueue<T>.ItemQueue itemQueue = this;
				itemQueue.totalCount = itemQueue.totalCount + 1;
			}

			public void EnqueuePendingItem(InputQueue<T>.Item item)
			{
				this.EnqueueItemCore(item);
				InputQueue<T>.ItemQueue itemQueue = this;
				itemQueue.pendingCount = itemQueue.pendingCount + 1;
			}

			public void MakePendingItemAvailable()
			{
				Fx.AssertAndThrow(this.pendingCount != 0, "ItemQueue does not contain any pending items");
				InputQueue<T>.ItemQueue itemQueue = this;
				itemQueue.pendingCount = itemQueue.pendingCount - 1;
			}
		}

		private enum QueueState
		{
			Open,
			Shutdown,
			Closed
		}

		private class WaitQueueReader : IQueueReader
		{
			private Exception exception;

			private InputQueue<T> inputQueue;

			private T item;

			private ManualResetEvent waitEvent;

			public WaitQueueReader(InputQueue<T> inputQueue)
			{
				this.inputQueue = inputQueue;
				this.waitEvent = new ManualResetEvent(false);
			}

			public void Set(InputQueue<T>.Item item)
			{
				lock (this)
				{
					this.exception = item.Exception;
					this.item = item.Value;
					this.waitEvent.Set();
				}
			}

			public bool Wait(TimeSpan timeout, out T value)
			{
				bool flag;
				bool flag1 = false;
				try
				{
					if (!TimeoutHelper.WaitOne(this.waitEvent, timeout))
					{
						if (!this.inputQueue.RemoveReader(this))
						{
							this.waitEvent.WaitOne();
						}
						else
						{
							value = default(T);
							flag1 = true;
							flag = false;
							return flag;
						}
					}
					flag1 = true;
					goto Label0;
				}
				finally
				{
					if (flag1)
					{
						this.waitEvent.Close();
					}
				}
				return flag;
			Label0:
				if (this.exception == null)
				{
					value = this.item;
					return true;
				}
				else
				{
					throw Fx.Exception.AsError(this.exception);
				}
			}
		}

		private class WaitQueueWaiter : IQueueWaiter
		{
			private bool itemAvailable;

			private ManualResetEvent waitEvent;

			public WaitQueueWaiter()
			{
				this.waitEvent = new ManualResetEvent(false);
			}

			public void Set(bool itemAvailable)
			{
				lock (this)
				{
					this.itemAvailable = itemAvailable;
					this.waitEvent.Set();
				}
			}

			public bool Wait(TimeSpan timeout)
			{
				if (TimeoutHelper.WaitOne(this.waitEvent, timeout))
				{
					return this.itemAvailable;
				}
				else
				{
					return false;
				}
			}
		}
	}
}