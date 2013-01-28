using Microsoft.Management.Odata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal class QueueCache<TItem> : Cache
	{
		private ConcurrentQueue<CacheEntry<TItem>> cache;

		public QueueCache(int cacheSize) : base(cacheSize)
		{
			this.cache = new ConcurrentQueue<CacheEntry<TItem>>();
		}

		public void Clear()
		{
			CacheEntry<TItem> cacheEntry = null;
			while (this.cache.TryDequeue(out cacheEntry))
			{
			}
		}

		public override void DoCleanup(DateTime checkPoint)
		{
			CacheEntry<TItem> cacheEntry = null;
			CacheEntry<TItem> cacheEntry1 = null;
			for (bool i = this.cache.TryPeek(out cacheEntry); i && cacheEntry.LastAccessedTime < checkPoint; i = this.cache.TryPeek(out cacheEntry))
			{
				if (this.cache.TryDequeue(out cacheEntry1))
				{
					cacheEntry1.Dispose();
				}
			}
		}

		public void Enqueue(TItem item)
		{
			if (this.cache.Count < base.MaxCacheSize)
			{
				this.cache.Enqueue(new CacheEntry<TItem>(item));
				return;
			}
			else
			{
				object[] maxCacheSize = new object[1];
				maxCacheSize[0] = base.MaxCacheSize;
				throw new OverflowException(ExceptionHelpers.GetExceptionMessage(Resources.CannotAddMoreToCache, maxCacheSize));
			}
		}

		internal int TestHookGetCount()
		{
			return this.cache.Count;
		}

		public override StringBuilder ToTraceMessage(string message, StringBuilder builder)
		{
			builder.AppendLine(message);
			List<CacheEntry<TItem>> list = this.cache.ToList<CacheEntry<TItem>>();
			builder.AppendLine(string.Concat("Count = ", list.Count));
			list.ForEach((CacheEntry<TItem> item) => builder = item.ToTraceMessage("Cache entry", builder));
			return builder;
		}

		public bool TryDequeue(out TItem item)
		{
			CacheEntry<TItem> cacheEntry = null;
			item = default(TItem);
			bool flag = this.cache.TryDequeue(out cacheEntry);
			if (cacheEntry != null)
			{
				item = cacheEntry.Value;
			}
			return flag;
		}
	}
}