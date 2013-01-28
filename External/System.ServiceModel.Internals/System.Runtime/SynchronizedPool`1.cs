using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime
{
	internal class SynchronizedPool<T>
	where T : class
	{
		private SynchronizedPool<T>.Entry[] entries;

		private SynchronizedPool<T>.GlobalPool globalPool;

		private int maxCount;

		private SynchronizedPool<T>.PendingEntry[] pending;

		private int promotionFailures;

		private const int maxPendingEntries = 128;

		private const int maxPromotionFailures = 64;

		private const int maxReturnsBeforePromotion = 64;

		private const int maxThreadItemsPerProcessor = 16;

		private object ThisLock
		{
			get
			{
				return this;
			}
		}

		public SynchronizedPool(int maxCount)
		{
			int num = maxCount;
			int processorCount = 16 + SynchronizedPool<T>.SynchronizedPoolHelper.ProcessorCount;
			if (num > processorCount)
			{
				num = processorCount;
			}
			this.maxCount = maxCount;
			this.entries = new SynchronizedPool<T>.Entry[num];
			this.pending = new SynchronizedPool<T>.PendingEntry[4];
			this.globalPool = new SynchronizedPool<T>.GlobalPool(maxCount);
		}

		public void Clear()
		{
			SynchronizedPool<T>.Entry[] entryArray = this.entries;
			for (int i = 0; i < (int)entryArray.Length; i++)
			{
				entryArray[i].@value = default(T);
			}
			this.globalPool.Clear();
		}

		private void HandlePromotionFailure(int thisThreadID)
		{
			int num = this.promotionFailures + 1;
			if (num < 64)
			{
				this.promotionFailures = num;
				return;
			}
			else
			{
				lock (this.ThisLock)
				{
					this.entries = new SynchronizedPool<T>.Entry[(int)this.entries.Length];
					this.globalPool.MaxCount = this.maxCount;
				}
				this.PromoteThread(thisThreadID);
				return;
			}
		}

		private bool PromoteThread(int thisThreadID)
		{
			bool flag;
			lock (this.ThisLock)
			{
				int num = 0;
				while (num < (int)this.entries.Length)
				{
					int num1 = this.entries[num].threadID;
					if (num1 != thisThreadID)
					{
						if (num1 != 0)
						{
							num++;
						}
						else
						{
							this.globalPool.DecrementMaxCount();
							this.entries[num].threadID = thisThreadID;
							flag = true;
							return flag;
						}
					}
					else
					{
						flag = true;
						return flag;
					}
				}
				return false;
			}
			return flag;
		}

		private void RecordReturnToGlobalPool(int thisThreadID)
		{
			SynchronizedPool<T>.PendingEntry[] pendingEntryArray = this.pending;
			int num = 0;
			while (num < (int)pendingEntryArray.Length)
			{
				int num1 = pendingEntryArray[num].threadID;
				if (num1 != thisThreadID)
				{
					if (num1 != 0)
					{
						num++;
					}
					else
					{
						return;
					}
				}
				else
				{
					int num2 = pendingEntryArray[num].returnCount + 1;
					if (num2 < 64)
					{
						pendingEntryArray[num].returnCount = num2;
						return;
					}
					else
					{
						pendingEntryArray[num].returnCount = 0;
						if (this.PromoteThread(thisThreadID))
						{
							break;
						}
						this.HandlePromotionFailure(thisThreadID);
						return;
					}
				}
			}
		}

		private void RecordTakeFromGlobalPool(int thisThreadID)
		{
			SynchronizedPool<T>.PendingEntry[] pendingEntryArray = this.pending;
			int num = 0;
			while (true)
			{
				if (num < (int)pendingEntryArray.Length)
				{
					int num1 = pendingEntryArray[num].threadID;
					if (num1 != thisThreadID)
					{
						if (num1 == 0)
						{
							lock (pendingEntryArray)
							{
								if (pendingEntryArray[num].threadID == 0)
								{
									pendingEntryArray[num].threadID = thisThreadID;
									break;
								}
							}
						}
						num++;
					}
					else
					{
						return;
					}
				}
				else
				{
					if ((int)pendingEntryArray.Length < 128)
					{
						SynchronizedPool<T>.PendingEntry[] pendingEntryArray1 = new SynchronizedPool<T>.PendingEntry[(int)pendingEntryArray.Length * 2];
						Array.Copy(pendingEntryArray, pendingEntryArray1, (int)pendingEntryArray.Length);
						this.pending = pendingEntryArray1;
						break;
					}
					else
					{
						this.pending = new SynchronizedPool<T>.PendingEntry[(int)pendingEntryArray.Length];
						return;
					}
				}
			}
		}

		public bool Return(T value)
		{
			int managedThreadId = Thread.CurrentThread.ManagedThreadId;
			if (managedThreadId != 0)
			{
				if (!this.ReturnToPerThreadPool(managedThreadId, value))
				{
					return this.ReturnToGlobalPool(managedThreadId, value);
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private bool ReturnToGlobalPool(int thisThreadID, T value)
		{
			this.RecordReturnToGlobalPool(thisThreadID);
			return this.globalPool.Return(value);
		}

		private bool ReturnToPerThreadPool(int thisThreadID, T value)
		{
			SynchronizedPool<T>.Entry[] entryArray = this.entries;
			int num = 0;
			while (num < (int)entryArray.Length)
			{
				int num1 = entryArray[num].threadID;
				if (num1 != thisThreadID)
				{
					if (num1 == 0)
					{
						break;
					}
					num++;
				}
				else
				{
					if (entryArray[num].@value != null)
					{
						return false;
					}
					else
					{
						entryArray[num].@value = value;
						return true;
					}
				}
			}
			return false;
		}

		public T Take()
		{
			int managedThreadId = Thread.CurrentThread.ManagedThreadId;
			if (managedThreadId != 0)
			{
				T t = this.TakeFromPerThreadPool(managedThreadId);
				if (t == null)
				{
					return this.TakeFromGlobalPool(managedThreadId);
				}
				else
				{
					return t;
				}
			}
			else
			{
				T t1 = default(T);
				return t1;
			}
		}

		private T TakeFromGlobalPool(int thisThreadID)
		{
			this.RecordTakeFromGlobalPool(thisThreadID);
			return this.globalPool.Take();
		}

		private T TakeFromPerThreadPool(int thisThreadID)
		{
			SynchronizedPool<T>.Entry[] entryArray = this.entries;
			int num = 0;
			while (num < (int)entryArray.Length)
			{
				int num1 = entryArray[num].threadID;
				if (num1 != thisThreadID)
				{
					if (num1 == 0)
					{
						break;
					}
					num++;
				}
				else
				{
					T t = entryArray[num].@value;
					if (t == null)
					{
						T t1 = default(T);
						return t1;
					}
					else
					{
						entryArray[num].@value = default(T);
						return t;
					}
				}
			}
			T t2 = default(T);
			return t2;
		}

		private struct Entry
		{
			public int threadID;

			public T @value;

		}

		private class GlobalPool
		{
			private Stack<T> items;

			private int maxCount;

			public int MaxCount
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.maxCount;
				}
				set
				{
					lock (this.ThisLock)
					{
						while (this.items.Count > value)
						{
							this.items.Pop();
						}
						this.maxCount = value;
					}
				}
			}

			private object ThisLock
			{
				get
				{
					return this;
				}
			}

			public GlobalPool(int maxCount)
			{
				this.items = new Stack<T>();
				this.maxCount = maxCount;
			}

			public void Clear()
			{
				lock (this.ThisLock)
				{
					this.items.Clear();
				}
			}

			public void DecrementMaxCount()
			{
				lock (this.ThisLock)
				{
					if (this.items.Count == this.maxCount)
					{
						this.items.Pop();
					}
					SynchronizedPool<T>.GlobalPool globalPool = this;
					globalPool.maxCount = globalPool.maxCount - 1;
				}
			}

			public bool Return(T value)
			{
				bool flag;
				if (this.items.Count < this.MaxCount)
				{
					lock (this.ThisLock)
					{
						if (this.items.Count >= this.MaxCount)
						{
							return false;
						}
						else
						{
							this.items.Push(value);
							flag = true;
						}
					}
					return flag;
				}
				return false;
			}

			public T Take()
			{
				T t;
				T t1;
				if (this.items.Count > 0)
				{
					lock (this.ThisLock)
					{
						if (this.items.Count <= 0)
						{
							t1 = default(T);
							return t1;
						}
						else
						{
							t = this.items.Pop();
						}
					}
					return t;
				}
				t1 = default(T);
				return t1;
			}
		}

		private struct PendingEntry
		{
			public int returnCount;

			public int threadID;

		}

		private static class SynchronizedPoolHelper
		{
			public readonly static int ProcessorCount;

			static SynchronizedPoolHelper()
			{
				SynchronizedPool<T>.SynchronizedPoolHelper.ProcessorCount = SynchronizedPool<T>.SynchronizedPoolHelper.GetProcessorCount();
			}

			[EnvironmentPermission(SecurityAction.Assert, Read="NUMBER_OF_PROCESSORS")]
			[SecuritySafeCritical]
			private static int GetProcessorCount()
			{
				return Environment.ProcessorCount;
			}
		}
	}
}