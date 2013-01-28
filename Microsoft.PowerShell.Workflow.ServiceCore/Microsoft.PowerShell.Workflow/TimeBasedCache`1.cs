using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal class TimeBasedCache<T> : IEnumerable, IDisposable
	{
		private const int TimerFired = 1;

		private const int TimerReset = 0;

		private readonly int _timeoutInSeconds;

		private readonly object _timerServicingSyncObject;

		private readonly Timer _validationTimer;

		private int _timerFired;

		private readonly ConcurrentDictionary<Guid, Item<T>> _cache;

		internal ConcurrentDictionary<Guid, Item<T>> Cache
		{
			get
			{
				return this._cache;
			}
		}

		internal object TimerServicingSyncObject
		{
			get
			{
				return this._timerServicingSyncObject;
			}
		}

		internal TimeBasedCache(int timeoutInSeconds)
		{
			this._timerServicingSyncObject = new object();
			this._cache = new ConcurrentDictionary<Guid, Item<T>>();
			this._timeoutInSeconds = timeoutInSeconds;
			Timer timer = new Timer();
			timer.AutoReset = true;
			timer.Interval = (double)(this._timeoutInSeconds * 0x3e8);
			timer.Enabled = false;
			this._validationTimer = timer;
			this._validationTimer.Elapsed += new ElapsedEventHandler(this.ValidationTimerElapsed);
			this._validationTimer.Start();
		}

		internal void Add(Item<T> item)
		{
			item.Busy = true;
			item.Idle = false;
			this._cache.TryAdd(item.InstanceId, item);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				this._validationTimer.Dispose();
			}
		}

		public IEnumerator GetEnumerator()
		{
			return new TimeBasedCache<T>.CacheEnumerator(this._cache);
		}

		private void ValidationTimerElapsed(object sender, ElapsedEventArgs e)
		{
			Item<T> item = null;
			if (this._timerFired != 1)
			{
				lock (this.TimerServicingSyncObject)
				{
					if (this._timerFired != 1)
					{
						this._timerFired = 1;
						Collection<Item<T>> items = new Collection<Item<T>>();
						foreach (Item<T> value in this._cache.Values)
						{
							if (!value.Idle)
							{
								if (value.Busy)
								{
									continue;
								}
								value.Idle = true;
							}
							else
							{
								items.Add(value);
							}
						}
						foreach (Item<T> item1 in items)
						{
							this._cache.TryRemove(item1.InstanceId, out item);
							IDisposable disposable = (object)item.Value as IDisposable;
							if (disposable == null)
							{
								continue;
							}
							disposable.Dispose();
							item = null;
						}
						this._timerFired = 0;
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal class CacheEnumerator : IEnumerator
		{
			private readonly ConcurrentDictionary<Guid, Item<T>> _cache;

			private readonly IEnumerator _dictionaryEnumerator;

			private Item<T> _currentItem;

			public object Current
			{
				get
				{
					return this._currentItem;
				}
			}

			internal CacheEnumerator(ConcurrentDictionary<Guid, Item<T>> cache)
			{
				this._cache = cache;
				this._dictionaryEnumerator = this._cache.Keys.GetEnumerator();
			}

			public bool MoveNext()
			{
				if (this._dictionaryEnumerator.MoveNext())
				{
					Guid current = (Guid)this._dictionaryEnumerator.Current;
					this._currentItem = null;
					this._cache.TryGetValue(current, out this._currentItem);
					if (this._currentItem != null)
					{
						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				this._dictionaryEnumerator.Reset();
			}
		}
	}
}