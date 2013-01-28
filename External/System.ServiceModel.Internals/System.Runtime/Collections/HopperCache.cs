using System;
using System.Collections;
using System.Threading;

namespace System.Runtime.Collections
{
	internal class HopperCache
	{
		private readonly int hopperSize;

		private readonly bool weak;

		private Hashtable outstandingHopper;

		private Hashtable strongHopper;

		private Hashtable limitedHopper;

		private int promoting;

		private HopperCache.LastHolder mruEntry;

		public HopperCache(int hopperSize, bool weak)
		{
			this.hopperSize = hopperSize;
			this.weak = weak;
			this.outstandingHopper = new Hashtable(hopperSize * 2);
			this.strongHopper = new Hashtable(hopperSize * 2);
			this.limitedHopper = new Hashtable(hopperSize * 2);
		}

		public void Add(object key, object value)
		{
			if (this.weak && !object.ReferenceEquals(value, DBNull.Value))
			{
				value = new WeakReference(value);
			}
			if (this.strongHopper.Count < this.hopperSize * 2)
			{
				this.strongHopper[key] = value;
			}
			else
			{
				Hashtable hashtables = this.limitedHopper;
				hashtables.Clear();
				hashtables.Add(key, value);
				try
				{
				}
				finally
				{
					this.limitedHopper = this.strongHopper;
					this.strongHopper = hashtables;
				}
			}
		}

		public object GetValue(object syncObject, object key)
		{
			WeakReference weakReference;
			object target;
			object obj;
			object target1;
			object obj1;
			HopperCache.LastHolder lastHolder = this.mruEntry;
			if (lastHolder != null && key.Equals(lastHolder.Key))
			{
				if (this.weak)
				{
					WeakReference value = lastHolder.Value as WeakReference;
					weakReference = value;
					if (value == null)
					{
						return lastHolder.Value;
					}
					target = weakReference.Target;
					if (target == null)
					{
						this.mruEntry = null;
						goto Label0;
					}
					else
					{
						return target;
					}
				}
				return lastHolder.Value;
			}
		Label0:
			object item = this.outstandingHopper[key];
			if (this.weak)
			{
				WeakReference weakReference1 = item as WeakReference;
				weakReference = weakReference1;
				if (weakReference1 == null)
				{
					goto Label3;
				}
				obj = weakReference.Target;
				goto Label2;
			}
		Label3:
			obj = item;
		Label2:
			target = obj;
			if (target == null)
			{
				item = this.strongHopper[key];
				if (this.weak)
				{
					WeakReference weakReference2 = item as WeakReference;
					weakReference = weakReference2;
					if (weakReference2 == null)
					{
						goto Label5;
					}
					target1 = weakReference.Target;
					goto Label4;
				}
			Label5:
				target1 = item;
			Label4:
				target = target1;
				if (target == null)
				{
					item = this.limitedHopper[key];
					if (this.weak)
					{
						WeakReference weakReference3 = item as WeakReference;
						weakReference = weakReference3;
						if (weakReference3 == null)
						{
							goto Label7;
						}
						obj1 = weakReference.Target;
						goto Label6;
					}
				Label7:
					obj1 = item;
				Label6:
					target = obj1;
					if (target == null)
					{
						return null;
					}
				}
				this.mruEntry = new HopperCache.LastHolder(key, item);
				int num = 1;
				try
				{
					try
					{
					}
					finally
					{
						num = Interlocked.CompareExchange(ref this.promoting, 1, 0);
					}
					if (num == 0)
					{
						if (this.outstandingHopper.Count < this.hopperSize)
						{
							this.outstandingHopper[key] = item;
						}
						else
						{
							lock (syncObject)
							{
								Hashtable hashtables = this.limitedHopper;
								hashtables.Clear();
								hashtables.Add(key, item);
								try
								{
								}
								finally
								{
									this.limitedHopper = this.strongHopper;
									this.strongHopper = this.outstandingHopper;
									this.outstandingHopper = hashtables;
								}
							}
						}
					}
				}
				finally
				{
					if (num == 0)
					{
						this.promoting = 0;
					}
				}
				return target;
			}
			else
			{
				this.mruEntry = new HopperCache.LastHolder(key, item);
				return target;
			}
		}

		private class LastHolder
		{
			private readonly object key;

			private readonly object @value;

			internal object Key
			{
				get
				{
					return this.key;
				}
			}

			internal object Value
			{
				get
				{
					return this.@value;
				}
			}

			internal LastHolder(object key, object value)
			{
				this.key = key;
				this.@value = value;
			}
		}
	}
}