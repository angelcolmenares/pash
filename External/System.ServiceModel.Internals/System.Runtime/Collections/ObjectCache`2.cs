using System;
using System.Collections.Generic;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal class ObjectCache<TKey, TValue>
	where TValue : class
	{
		private ObjectCacheSettings settings;

		private Dictionary<TKey, ObjectCache<TKey, TValue>.Item> cacheItems;

		private bool idleTimeoutEnabled;

		private bool leaseTimeoutEnabled;

		private IOThreadTimer idleTimer;

		private static Action<object> onIdle;

		private bool disposed;

		private const int timerThreshold = 1;

		public int Count
		{
			get
			{
				return this.cacheItems.Count;
			}
		}

		public Action<TValue> DisposeItemCallback
		{
			get;set;
		}

		private object ThisLock
		{
			get
			{
				return this;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ObjectCache(ObjectCacheSettings settings) : this(settings, null)
		{
		}

		public ObjectCache(ObjectCacheSettings settings, IEqualityComparer<TKey> comparer)
		{
			this.settings = settings.Clone();
			this.cacheItems = new Dictionary<TKey, ObjectCache<TKey, TValue>.Item>(comparer);
			this.idleTimeoutEnabled = settings.IdleTimeout != TimeSpan.MaxValue;
			this.leaseTimeoutEnabled = settings.LeaseTimeout != TimeSpan.MaxValue;
		}

		public ObjectCacheItem<TValue> Add(TKey key, TValue value)
		{
			ObjectCacheItem<TValue> item;
			lock (this.ThisLock)
			{
				if (this.Count >= this.settings.CacheLimit || this.cacheItems.ContainsKey(key))
				{
					item = new ObjectCache<TKey, TValue>.Item(key, value, this.DisposeItemCallback);
				}
				else
				{
					item = this.InternalAdd(key, value);
				}
			}
			return item;
		}

		private static void Add<T>(ref List<T> list, T item)
		{
			if (list == null)
			{
				list = new List<T>();
			}
			list.Add(item);
		}

		public void Dispose()
		{
			lock (this.ThisLock)
			{
				foreach (ObjectCache<TKey, TValue>.Item value in this.cacheItems.Values)
				{
					if (value == null)
					{
						continue;
					}
					value.Dispose();
				}
				this.cacheItems.Clear();
				this.settings.CacheLimit = 0;
				this.disposed = true;
				if (this.idleTimer != null)
				{
					this.idleTimer.Cancel();
					this.idleTimer = null;
				}
			}
		}

		private void GatherExpiredItems(ref List<KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item>> expiredItems, bool calledFromTimer)
		{
			bool flag = false;
			bool count;
			if (this.Count != 0)
			{
				if (this.leaseTimeoutEnabled || this.idleTimeoutEnabled)
				{
					DateTime utcNow = DateTime.UtcNow;
					lock (this.ThisLock)
					{
						foreach (KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item> cacheItem in this.cacheItems)
						{
							if (!this.ShouldPurgeItem(cacheItem.Value, utcNow))
							{
								continue;
							}
							cacheItem.Value.LockedDispose();
							ObjectCache<TKey, TValue>.Add<KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item>>(ref expiredItems, cacheItem);
						}
						if (expiredItems != null)
						{
							for (int i = 0; i < expiredItems.Count; i++)
							{
								KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item> item = expiredItems[i];
								this.cacheItems.Remove(item.Key);
							}
						}
						if (!calledFromTimer)
						{
							count = false;
						}
						else
						{
							count = this.Count > 0;
						}
					}
					if (flag)
					{
						this.idleTimer.Set(this.settings.IdleTimeout);
					}
					return;
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

		private ObjectCache<TKey, TValue>.Item InternalAdd(TKey key, TValue value)
		{
			ObjectCache<TKey, TValue>.Item item = new ObjectCache<TKey, TValue>.Item(key, value, this);
			if (this.leaseTimeoutEnabled)
			{
				item.CreationTime = DateTime.UtcNow;
			}
			this.cacheItems.Add(key, item);
			this.StartTimerIfNecessary();
			return item;
		}

		private static void OnIdle(object state)
		{
			ObjectCache<TKey, TValue> objectCache = (ObjectCache<TKey, TValue>)state;
			objectCache.PurgeCache(true);
		}

		private void PurgeCache(bool calledFromTimer)
		{
			List<KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item>> keyValuePairs = null;
			lock (this.ThisLock)
			{
				this.GatherExpiredItems(ref keyValuePairs, calledFromTimer);
			}
			if (keyValuePairs != null)
			{
				for (int i = 0; i < keyValuePairs.Count; i++)
				{
					KeyValuePair<TKey, ObjectCache<TKey, TValue>.Item> item = keyValuePairs[i];
					item.Value.LocalDispose();
				}
			}
		}

		private bool Return(TKey key, ObjectCache<TKey, TValue>.Item cacheItem)
		{
			bool flag = false;
			if (!this.disposed)
			{
				cacheItem.InternalReleaseReference();
				DateTime utcNow = DateTime.UtcNow;
				if (this.idleTimeoutEnabled)
				{
					cacheItem.LastUsage = utcNow;
				}
				if (this.ShouldPurgeItem(cacheItem, utcNow))
				{
					this.cacheItems.Remove(key);
					cacheItem.LockedDispose();
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			return flag;
		}

		private bool ShouldPurgeItem(ObjectCache<TKey, TValue>.Item cacheItem, DateTime now)
		{
			if (cacheItem.ReferenceCount <= 0)
			{
				if (!this.idleTimeoutEnabled || !(now >= cacheItem.LastUsage + this.settings.IdleTimeout))
				{
					if (!this.leaseTimeoutEnabled || !(now - cacheItem.CreationTime >= this.settings.LeaseTimeout))
					{
						return false;
					}
					else
					{
						return true;
					}
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

		private void StartTimerIfNecessary()
		{
			if (this.idleTimeoutEnabled && this.Count > 1)
			{
				if (this.idleTimer == null)
				{
					if (ObjectCache<TKey, TValue>.onIdle == null)
					{
						ObjectCache<TKey, TValue>.onIdle = new Action<object>(ObjectCache<TKey, TValue>.OnIdle);
					}
					this.idleTimer = new IOThreadTimer(ObjectCache<TKey, TValue>.onIdle, this, false);
				}
				this.idleTimer.Set(this.settings.IdleTimeout);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ObjectCacheItem<TValue> Take(TKey key)
		{
			return this.Take(key, null);
		}

		public ObjectCacheItem<TValue> Take(TKey key, Func<TValue> initializerDelegate)
		{
			ObjectCacheItem<TValue> item;
			ObjectCache<TKey, TValue>.Item item1 = null;
			lock (this.ThisLock)
			{
				if (!this.cacheItems.TryGetValue(key, out item1))
				{
					if (initializerDelegate != null)
					{
						TValue tValue = initializerDelegate();
						if (this.Count < this.settings.CacheLimit)
						{
							item1 = this.InternalAdd(key, tValue);
						}
						else
						{
							item = new ObjectCache<TKey, TValue>.Item(key, tValue, this.DisposeItemCallback);
							return item;
						}
					}
					else
					{
						item = null;
						return item;
					}
				}
				else
				{
					item1.InternalAddReference();
				}
				return item1;
			}
			return item;
		}

		private class Item : ObjectCacheItem<TValue>
		{
			private readonly ObjectCache<TKey, TValue> parent;

			private readonly TKey key;

			private readonly Action<TValue> disposeItemCallback;

			private TValue @value;

			private int referenceCount;

			public DateTime CreationTime
			{
				get;set;
			}

			public DateTime LastUsage
			{
				get;set;
			}

			public int ReferenceCount
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.referenceCount;
				}
			}

			public override TValue Value
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.@value;
				}
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public Item(TKey key, TValue value, Action<TValue> disposeItemCallback) : this(key, value)
			{
				this.disposeItemCallback = disposeItemCallback;
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public Item(TKey key, TValue value, ObjectCache<TKey, TValue> parent) : this(key, value)
			{
				this.parent = parent;
			}

			private Item(TKey key, TValue value)
			{
				this.key = key;
				this.@value = value;
				this.referenceCount = 1;
			}

			public void Dispose()
			{
				if (this.Value != null)
				{
					Action<TValue> disposeItemCallback = this.disposeItemCallback;
					if (this.parent != null)
					{
						disposeItemCallback = this.parent.DisposeItemCallback;
					}
					if (disposeItemCallback == null)
					{
						if ((object)this.Value as IDisposable != null)
						{
							((IDisposable)(object)this.Value).Dispose();
						}
					}
					else
					{
						disposeItemCallback(this.Value);
					}
				}
				this.@value = default(TValue);
				this.referenceCount = -1;
			}

			internal void InternalAddReference()
			{
				ObjectCache<TKey, TValue>.Item<TKey, TValue> item = this;
				item.referenceCount = item.referenceCount + 1;
			}

			internal void InternalReleaseReference()
			{
				ObjectCache<TKey, TValue>.Item<TKey, TValue> item = this;
				item.referenceCount = item.referenceCount - 1;
			}

			public void LocalDispose()
			{
				base.Dispose();
			}

			public void LockedDispose()
			{
				this.referenceCount = -1;
			}

			public override void ReleaseReference()
			{
				bool flag;
				if (this.parent != null)
				{
					lock (this.parent.ThisLock)
					{
						if (this.referenceCount <= 1)
						{
							flag = this.parent.Return(this.key, this);
						}
						else
						{
							base.InternalReleaseReference();
							flag = false;
						}
					}
				}
				else
				{
					this.referenceCount = -1;
					flag = true;
				}
				if (flag)
				{
					base.LocalDispose();
				}
			}

			public override bool TryAddReference()
			{
				bool flag;
				if (this.parent == null || this.referenceCount == -1)
				{
					flag = false;
				}
				else
				{
					bool flag1 = false;
					lock (this.parent.ThisLock)
					{
						if (this.referenceCount != -1)
						{
							if (this.referenceCount != 0 || !this.parent.ShouldPurgeItem(this, DateTime.UtcNow))
							{
								ObjectCache<TKey, TValue>.Item<TKey, TValue> item = this;
								item.referenceCount = item.referenceCount + 1;
								flag = true;
							}
							else
							{
								base.LockedDispose();
								flag = false;
								this.parent.cacheItems.Remove(this.key);
							}
						}
						else
						{
							flag = false;
						}
					}
					if (flag1)
					{
						base.LocalDispose();
					}
				}
				return flag;
			}
		}
	}
}