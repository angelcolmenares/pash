using Microsoft.Management.Odata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Management.Odata.Common
{
	internal class DictionaryCache<TKey, TValue> : Cache
	{
		private Dictionary<TKey, CacheEntry<TValue>> cache;

		private ReaderWriterLockSlim readerWriterLock;

		public DictionaryCache(int cacheSize) : base(cacheSize)
		{
			this.readerWriterLock = new ReaderWriterLockSlim();
			this.cache = new Dictionary<TKey, CacheEntry<TValue>>();
		}

		public void AddOrLockKey(TKey key, TValue value, out TValue valueAddedOrLocked)
		{
			CacheEntry<TValue> cacheEntry = null;
			valueAddedOrLocked = value;
			CacheEntry<TValue> cacheEntry1 = new CacheEntry<TValue>(value);
			using (WriterLock writerLock = new WriterLock(this.readerWriterLock))
			{
				if (!this.cache.TryGetValue(key, out cacheEntry))
				{
					if (this.cache.Count < base.MaxCacheSize)
					{
						this.cache.Add(key, cacheEntry1);
						valueAddedOrLocked = value;
					}
					else
					{
						object[] maxCacheSize = new object[1];
						maxCacheSize[0] = base.MaxCacheSize;
						throw new OverflowException(ExceptionHelpers.GetExceptionMessage(Resources.CannotAddMoreToCache, maxCacheSize));
					}
				}
				else
				{
					cacheEntry.Lock(out valueAddedOrLocked);
				}
			}
		}

		protected virtual bool CanItemBeRemoved(CacheEntry<TValue> cachedItem, DateTime checkPoint)
		{
			if (cachedItem.IsLocked || !(cachedItem.LastAccessedTime < checkPoint))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public override void DoCleanup(DateTime checkPoint)
		{
			using (WriterLock writerLock = new WriterLock(this.readerWriterLock))
			{
				List<TKey> tKeys = new List<TKey>();
				foreach (TKey key in this.cache.Keys)
				{
					CacheEntry<TValue> item = this.cache[key];
					if (!this.CanItemBeRemoved(item, checkPoint))
					{
						continue;
					}
					tKeys.Add(key);
				}
				foreach (TKey tKey in tKeys)
				{
					CacheEntry<TValue> cacheEntry = this.cache[tKey];
					this.cache.Remove(tKey);
					cacheEntry.Dispose();
				}
			}
		}

		internal int TestHookGetLockCount(TKey key)
		{
			CacheEntry<TValue> cacheEntry = null;
			int num;
			using (ReaderLock readerLock = new ReaderLock(this.readerWriterLock))
			{
				if (!this.cache.TryGetValue(key, out cacheEntry))
				{
					num = -1;
				}
				else
				{
					num = cacheEntry.TestHookGetLockCount();
				}
			}
			return num;
		}

		public List<KeyValuePair<TKey, TValue>> ToList()
		{
			List<KeyValuePair<TKey, TValue>> keyValuePairs;
			using (ReaderLock readerLock = new ReaderLock(this.readerWriterLock))
			{
				List<KeyValuePair<TKey, TValue>> keyValuePairs1 = new List<KeyValuePair<TKey, TValue>>();
				foreach (KeyValuePair<TKey, CacheEntry<TValue>> keyValuePair in this.cache)
				{
					keyValuePairs1.Add(new KeyValuePair<TKey, TValue>(keyValuePair.Key, keyValuePair.Value.Value));
				}
				keyValuePairs = keyValuePairs1;
			}
			return keyValuePairs;
		}

		public override StringBuilder ToTraceMessage(string message, StringBuilder builder)
		{
			builder.AppendLine(message);
			Dictionary<TKey, CacheEntry<TValue>> tKeys = new Dictionary<TKey, CacheEntry<TValue>>(this.cache);
			builder.AppendLine(string.Concat("Count = ", tKeys.Count));
			foreach (KeyValuePair<TKey, CacheEntry<TValue>> tKey in tKeys)
			{
				TKey key = tKey.Key;
				builder.AppendLine(string.Concat("Key = ", key.ToString()));
				if (tKey.Value == null)
				{
					builder.AppendLine("Value = <null>");
				}
				else
				{
					builder = tKey.Value.ToTraceMessage("Cache entry value", builder);
				}
			}
			return builder;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			CacheEntry<TValue> cacheEntry = null;
			bool flag;
			value = default(TValue);
			using (ReaderLock readerLock = new ReaderLock(this.readerWriterLock))
			{
				bool flag1 = this.cache.TryGetValue(key, out cacheEntry);
				flag = flag1;
				if (flag1)
				{
					value = cacheEntry.Value;
				}
			}
			return flag;
		}

		public bool TryLockKey(TKey key, out TValue value)
		{
			CacheEntry<TValue> cacheEntry = null;
			bool flag;
			value = default(TValue);
			using (ReaderLock readerLock = new ReaderLock(this.readerWriterLock))
			{
				bool flag1 = this.cache.TryGetValue(key, out cacheEntry);
				flag = flag1;
				if (flag1)
				{
					cacheEntry.Lock(out value);
				}
			}
			return flag;
		}

		public bool TryRemove(TKey key)
		{
			bool flag;
			using (WriterLock writerLock = new WriterLock(this.readerWriterLock))
			{
				if (this.cache.ContainsKey(key))
				{
					this.cache.Remove(key);
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		public bool TryUnlockKey(TKey key)
		{
			CacheEntry<TValue> cacheEntry = null;
			bool flag;
			using (ReaderLock readerLock = new ReaderLock(this.readerWriterLock))
			{
				bool flag1 = this.cache.TryGetValue(key, out cacheEntry);
				flag = flag1;
				if (flag1)
				{
					cacheEntry.Unlock();
				}
			}
			return flag;
		}
	}
}