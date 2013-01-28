using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Runtime
{
	internal abstract class InternalBufferManager
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected InternalBufferManager()
		{
		}

		public abstract void Clear();

		public static InternalBufferManager Create(long maxBufferPoolSize, int maxBufferSize)
		{
			if (maxBufferPoolSize != (long)0)
			{
				return new InternalBufferManager.PooledBufferManager(maxBufferPoolSize, maxBufferSize);
			}
			else
			{
				return InternalBufferManager.GCBufferManager.Value;
			}
		}

		public abstract void ReturnBuffer(byte[] buffer);

		public abstract byte[] TakeBuffer(int bufferSize);

		private class GCBufferManager : InternalBufferManager
		{
			private static InternalBufferManager.GCBufferManager @value;

			public static InternalBufferManager.GCBufferManager Value
			{
				get
				{
					return InternalBufferManager.GCBufferManager.@value;
				}
			}

			static GCBufferManager()
			{
				InternalBufferManager.GCBufferManager.@value = new InternalBufferManager.GCBufferManager();
			}

			private GCBufferManager()
			{
			}

			public override void Clear()
			{
			}

			public override void ReturnBuffer(byte[] buffer)
			{
			}

			public override byte[] TakeBuffer(int bufferSize)
			{
				return Fx.AllocateByteArray(bufferSize);
			}
		}

		private class PooledBufferManager : InternalBufferManager
		{
			private const int minBufferSize = 128;

			private const int maxMissesBeforeTuning = 8;

			private const int initialBufferCount = 1;

			private readonly object tuningLock;

			private int[] bufferSizes;

			private InternalBufferManager.PooledBufferManager.BufferPool[] bufferPools;

			private long memoryLimit;

			private long remainingMemory;

			private bool areQuotasBeingTuned;

			private int totalMisses;

			public PooledBufferManager(long maxMemoryToPool, int maxBufferSize)
			{
				int num;
				this.tuningLock = new object();
				this.memoryLimit = maxMemoryToPool;
				this.remainingMemory = maxMemoryToPool;
				List<InternalBufferManager.PooledBufferManager.BufferPool> bufferPools = new List<InternalBufferManager.PooledBufferManager.BufferPool>();
				int num1 = 128;
				while (true)
				{
					long num2 = this.remainingMemory / (long)num1;
					if (num2 > (long)0x7fffffff)
					{
						num = 0x7fffffff;
					}
					else
					{
						num = (int)num2;
					}
					int num3 = num;
					if (num3 > 1)
					{
						num3 = 1;
					}
					bufferPools.Add(InternalBufferManager.PooledBufferManager.BufferPool.CreatePool(num1, num3));
					InternalBufferManager.PooledBufferManager pooledBufferManager = this;
					pooledBufferManager.remainingMemory = pooledBufferManager.remainingMemory - (long)num3 * (long)num1;
					if (num1 >= maxBufferSize)
					{
						break;
					}
					long num4 = (long)num1 * (long)2;
					if (num4 <= (long)maxBufferSize)
					{
						num1 = (int)num4;
					}
					else
					{
						num1 = maxBufferSize;
					}
				}
				this.bufferPools = bufferPools.ToArray();
				this.bufferSizes = new int[(int)this.bufferPools.Length];
				int num5 = 0;
				while (num5 < (int)this.bufferPools.Length)
				{
					this.bufferSizes[num5] = this.bufferPools[num5].BufferSize;
					num5++;
				}
			}

			private void ChangeQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool, int delta)
			{
				if (TraceCore.BufferPoolChangeQuotaIsEnabled(Fx.Trace))
				{
					TraceCore.BufferPoolChangeQuota(Fx.Trace, bufferPool.BufferSize, delta);
				}
				InternalBufferManager.PooledBufferManager.BufferPool bufferPool1 = bufferPool;
				int limit = bufferPool1.Limit + delta;
				InternalBufferManager.PooledBufferManager.BufferPool bufferPool2 = InternalBufferManager.PooledBufferManager.BufferPool.CreatePool(bufferPool1.BufferSize, limit);
				for (int i = 0; i < limit; i++)
				{
					byte[] numArray = bufferPool1.Take();
					if (numArray == null)
					{
						break;
					}
					bufferPool2.Return(numArray);
					bufferPool2.IncrementCount();
				}
				InternalBufferManager.PooledBufferManager bufferSize = this;
				bufferSize.remainingMemory = bufferSize.remainingMemory - (long)(bufferPool1.BufferSize * delta);
				bufferPool = bufferPool2;
			}

			public override void Clear()
			{
				for (int i = 0; i < (int)this.bufferPools.Length; i++)
				{
					InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
					bufferPool.Clear();
				}
			}

			private void DecreaseQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool)
			{
				this.ChangeQuota(ref bufferPool, -1);
			}

			private int FindMostExcessivePool()
			{
				long num = (long)0;
				int num1 = -1;
				for (int i = 0; i < (int)this.bufferPools.Length; i++)
				{
					InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
					if (bufferPool.Peak < bufferPool.Limit)
					{
						long limit = (long)(bufferPool.Limit - bufferPool.Peak) * (long)bufferPool.BufferSize;
						if (limit > num)
						{
							num1 = i;
							num = limit;
						}
					}
				}
				return num1;
			}

			private int FindMostStarvedPool()
			{
				long num = (long)0;
				int num1 = -1;
				for (int i = 0; i < (int)this.bufferPools.Length; i++)
				{
					InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
					if (bufferPool.Peak == bufferPool.Limit)
					{
						long misses = (long)bufferPool.Misses * (long)bufferPool.BufferSize;
						if (misses > num)
						{
							num1 = i;
							num = misses;
						}
					}
				}
				return num1;
			}

			private InternalBufferManager.PooledBufferManager.BufferPool FindPool(int desiredBufferSize)
			{
				int num = 0;
				while (num < (int)this.bufferSizes.Length)
				{
					if (desiredBufferSize > this.bufferSizes[num])
					{
						num++;
					}
					else
					{
						return this.bufferPools[num];
					}
				}
				return null;
			}

			private void IncreaseQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool)
			{
				this.ChangeQuota(ref bufferPool, 1);
			}

			public override void ReturnBuffer(byte[] buffer)
			{
				InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.FindPool((int)buffer.Length);
				if (bufferPool != null)
				{
					if ((int)buffer.Length == bufferPool.BufferSize)
					{
						if (bufferPool.Return(buffer))
						{
							bufferPool.IncrementCount();
						}
					}
					else
					{
						throw Fx.Exception.Argument("buffer", InternalSR.BufferIsNotRightSizeForBufferManager);
					}
				}
			}

			public override byte[] TakeBuffer(int bufferSize)
			{
				InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.FindPool(bufferSize);
				if (bufferPool == null)
				{
					if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
					{
						TraceCore.BufferPoolAllocation(Fx.Trace, bufferSize);
					}
					return Fx.AllocateByteArray(bufferSize);
				}
				else
				{
					byte[] numArray = bufferPool.Take();
					if (numArray == null)
					{
						if (bufferPool.Peak == bufferPool.Limit)
						{
							InternalBufferManager.PooledBufferManager.BufferPool misses = bufferPool;
							misses.Misses = misses.Misses + 1;
							InternalBufferManager.PooledBufferManager pooledBufferManager = this;
							int num = pooledBufferManager.totalMisses + 1;
							int num1 = num;
							pooledBufferManager.totalMisses = num;
							if (num1 >= 8)
							{
								this.TuneQuotas();
							}
						}
						if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
						{
							TraceCore.BufferPoolAllocation(Fx.Trace, bufferPool.BufferSize);
						}
						return Fx.AllocateByteArray(bufferPool.BufferSize);
					}
					else
					{
						bufferPool.DecrementCount();
						return numArray;
					}
				}
			}

			private void TuneQuotas()
			{
				if (!this.areQuotasBeingTuned)
				{
					bool flag = false;
					try
					{
						Monitor.TryEnter(this.tuningLock, ref flag);
						if (!flag || this.areQuotasBeingTuned)
						{
							return;
						}
						else
						{
							this.areQuotasBeingTuned = true;
						}
					}
					finally
					{
						if (flag)
						{
							Monitor.Exit(this.tuningLock);
						}
					}
					int num = this.FindMostStarvedPool();
					if (num >= 0)
					{
						InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[num];
						if (this.remainingMemory < (long)bufferPool.BufferSize)
						{
							int num1 = this.FindMostExcessivePool();
							if (num1 >= 0)
							{
								this.DecreaseQuota(ref this.bufferPools[num1]);
							}
						}
						if (this.remainingMemory >= (long)bufferPool.BufferSize)
						{
							this.IncreaseQuota(ref this.bufferPools[num]);
						}
					}
					for (int i = 0; i < (int)this.bufferPools.Length; i++)
					{
						InternalBufferManager.PooledBufferManager.BufferPool bufferPool1 = this.bufferPools[i];
						bufferPool1.Misses = 0;
					}
					this.totalMisses = 0;
					this.areQuotasBeingTuned = false;
					return;
				}
				else
				{
					return;
				}
			}

			private abstract class BufferPool
			{
				private int bufferSize;

				private int count;

				private int limit;

				private int misses;

				private int peak;

				public int BufferSize
				{
					get
					{
						return this.bufferSize;
					}
				}

				public int Limit
				{
					get
					{
						return this.limit;
					}
				}

				public int Misses
				{
					get
					{
						return this.misses;
					}
					set
					{
						this.misses = value;
					}
				}

				public int Peak
				{
					get
					{
						return this.peak;
					}
				}

				public BufferPool(int bufferSize, int limit)
				{
					this.bufferSize = bufferSize;
					this.limit = limit;
				}

				public void Clear()
				{
					this.OnClear();
					this.count = 0;
				}

				internal static InternalBufferManager.PooledBufferManager.BufferPool CreatePool(int bufferSize, int limit)
				{
					if (bufferSize >= 0x14c08)
					{
						return new InternalBufferManager.PooledBufferManager.BufferPool.LargeBufferPool(bufferSize, limit);
					}
					else
					{
						return new InternalBufferManager.PooledBufferManager.BufferPool.SynchronizedBufferPool(bufferSize, limit);
					}
				}

				public void DecrementCount()
				{
					int num = this.count - 1;
					if (num >= 0)
					{
						this.count = num;
					}
				}

				public void IncrementCount()
				{
					int num = this.count + 1;
					if (num <= this.limit)
					{
						this.count = num;
						if (num > this.peak)
						{
							this.peak = num;
						}
					}
				}

				internal abstract void OnClear();

				internal abstract bool Return(byte[] buffer);

				internal abstract byte[] Take();

				private class LargeBufferPool : InternalBufferManager.PooledBufferManager.BufferPool
				{
					private Stack<byte[]> items;

					private object ThisLock
					{
						get
						{
							return this.items;
						}
					}

					internal LargeBufferPool(int bufferSize, int limit) : base(bufferSize, limit)
					{
						this.items = new Stack<byte[]>(limit);
					}

					internal override void OnClear()
					{
						lock (this.ThisLock)
						{
							this.items.Clear();
						}
					}

					internal override bool Return(byte[] buffer)
					{
						bool flag;
						lock (this.ThisLock)
						{
							if (this.items.Count >= base.Limit)
							{
								return false;
							}
							else
							{
								this.items.Push(buffer);
								flag = true;
							}
						}
						return flag;
					}

					internal override byte[] Take()
					{
						byte[] numArray;
						lock (this.ThisLock)
						{
							if (this.items.Count <= 0)
							{
								return null;
							}
							else
							{
								numArray = this.items.Pop();
							}
						}
						return numArray;
					}
				}

				private class SynchronizedBufferPool : InternalBufferManager.PooledBufferManager.BufferPool
				{
					private SynchronizedPool<byte[]> innerPool;

					internal SynchronizedBufferPool(int bufferSize, int limit) : base(bufferSize, limit)
					{
						this.innerPool = new SynchronizedPool<byte[]>(limit);
					}

					internal override void OnClear()
					{
						this.innerPool.Clear();
					}

					internal override bool Return(byte[] buffer)
					{
						return this.innerPool.Return(buffer);
					}

					internal override byte[] Take()
					{
						return this.innerPool.Take();
					}
				}
			}
		}
	}
}