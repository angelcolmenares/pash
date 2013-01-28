using System;
using System.Collections.Generic;

namespace System.Runtime
{
	internal class MruCache<TKey, TValue>
	where TKey : class
	where TValue : class
	{
		private LinkedList<TKey> mruList;

		private Dictionary<TKey, MruCache<TKey, TValue>.CacheEntry> items;

		private int lowWatermark;

		private int highWatermark;

		private MruCache<TKey, TValue>.CacheEntry mruEntry;

		public int Count
		{
			get
			{
				return this.items.Count;
			}
		}

		public MruCache(int watermark) : this(watermark * 4 / 5, watermark)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public MruCache(int lowWatermark, int highWatermark) : this(lowWatermark, highWatermark, null)
		{
		}

		public MruCache(int lowWatermark, int highWatermark, IEqualityComparer<TKey> comparer)
		{
			this.lowWatermark = lowWatermark;
			this.highWatermark = highWatermark;
			this.mruList = new LinkedList<TKey>();
			if (comparer != null)
			{
				this.items = new Dictionary<TKey, MruCache<TKey, TValue>.CacheEntry>(comparer);
				return;
			}
			else
			{
				this.items = new Dictionary<TKey, MruCache<TKey, TValue>.CacheEntry>();
				return;
			}
		}

		public void Add(TKey key, TValue value)
		{
			bool flag = false;
			try
			{
				if (this.items.Count == this.highWatermark)
				{
					int num = this.highWatermark - this.lowWatermark;
					for (int i = 0; i < num; i++)
					{
						TKey tKey = this.mruList.Last.Value;
						this.mruList.RemoveLast();
						TValue item = this.items[tKey].@value;
						this.items.Remove(tKey);
						this.OnSingleItemRemoved(item);
						this.OnItemAgedOutOfCache(item);
					}
				}
					MruCache<TKey, TValue>.CacheEntry cacheEntry;
					cacheEntry.node = this.mruList.AddFirst(key);
				cacheEntry.@value = value;
				this.items.Add(key, cacheEntry);
				this.mruEntry = cacheEntry;
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					this.Clear();
				}
			}
		}

		public void Clear()
		{
			this.mruList.Clear();
			this.items.Clear();
			this.mruEntry.@value = default(TValue);
			this.mruEntry.node = null;
		}

		protected virtual void OnItemAgedOutOfCache(TValue item)
		{
		}

		protected virtual void OnSingleItemRemoved(TValue item)
		{
		}

		public bool Remove(TKey key)
		{
			MruCache<TKey, TValue>.CacheEntry cacheEntry;
			if (!this.items.TryGetValue(key, out cacheEntry))
			{
				return false;
			}
			else
			{
				this.items.Remove(key);
				this.OnSingleItemRemoved(cacheEntry.@value);
				this.mruList.Remove(cacheEntry.node);
				if (object.ReferenceEquals(this.mruEntry.node, cacheEntry.node))
				{
					this.mruEntry.@value = default(TValue);
					this.mruEntry.node = null;
				}
				return true;
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			MruCache<TKey, TValue>.CacheEntry cacheEntry;
			if (this.mruEntry.node == null || key == null || !key.Equals(this.mruEntry.node.Value))
			{
				bool flag = this.items.TryGetValue(key, out cacheEntry);
				value = cacheEntry.@value;
				if (flag && this.mruList.Count > 1 && !object.ReferenceEquals(this.mruList.First, cacheEntry.node))
				{
					this.mruList.Remove(cacheEntry.node);
					this.mruList.AddFirst(cacheEntry.node);
					this.mruEntry = cacheEntry;
				}
				return flag;
			}
			else
			{
				value = this.mruEntry.@value;
				return true;
			}
		}

		private struct CacheEntry
		{
			internal TValue @value;

			internal LinkedListNode<TKey> node;

		}
	}
}