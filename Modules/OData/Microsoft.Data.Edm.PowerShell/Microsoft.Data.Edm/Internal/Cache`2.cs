using System;
using System.Threading;

namespace Microsoft.Data.Edm.Internal
{
	internal class Cache<TContainer, TProperty>
	{
		private object @value;

		public Cache()
		{
			this.@value = CacheHelper.Unknown;
		}

		public void Clear(Func<TContainer, TProperty> onCycle)
		{
			object obj = null;
			bool flag = false;
			try
			{
				Func<TContainer, TProperty> func = onCycle;
				object obj1 = func;
				if (func == null)
				{
					obj1 = this;
				}
				obj = obj1;
				Monitor.Enter(obj1, ref flag);
				if (this.@value != CacheHelper.CycleSentinel && this.@value != CacheHelper.SecondPassCycleSentinel)
				{
					this.@value = CacheHelper.Unknown;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(obj);
				}
			}
		}

		public TProperty GetValue(TContainer container, Func<TContainer, TProperty> compute, Func<TContainer, TProperty> onCycle)
		{
			TProperty tProperty;
			TProperty value;
			object obj;
			Func<TContainer, TProperty> func = onCycle;
			object obj1 = func;
			if (func == null)
			{
				obj1 = this;
			}
			object obj2 = obj1;
			object obj3 = this.@value;
			if (obj3 != CacheHelper.Unknown)
			{
				if (obj3 != CacheHelper.CycleSentinel)
				{
					if (obj3 == CacheHelper.SecondPassCycleSentinel)
					{
						lock (obj2)
						{
							if (this.@value != CacheHelper.SecondPassCycleSentinel)
							{
								if (this.@value == CacheHelper.Unknown)
								{
									value = this.GetValue(container, compute, onCycle);
									return value;
								}
							}
							else
							{
								this.@value = onCycle(container);
							}
							obj3 = this.@value;
							return (TProperty)obj3;
						}
					}
					else
					{
						return (TProperty)obj3;
					}
				}
				else
				{
					lock (obj2)
					{
						if (this.@value != CacheHelper.CycleSentinel)
						{
							if (this.@value == CacheHelper.Unknown)
							{
								value = this.GetValue(container, compute, onCycle);
								return value;
							}
						}
						else
						{
							this.@value = CacheHelper.SecondPassCycleSentinel;
							try
							{
								compute(container);
							}
							catch
							{
								this.@value = CacheHelper.CycleSentinel;
								throw;
							}
							if (this.@value == CacheHelper.SecondPassCycleSentinel)
							{
								this.@value = onCycle(container);
							}
						}
						obj3 = this.@value;
						return (TProperty)obj3;
					}
				}
				return value;
			}
			else
			{
				lock (obj2)
				{
					if (this.@value == CacheHelper.Unknown)
					{
						this.@value = CacheHelper.CycleSentinel;
						try
						{
							tProperty = compute(container);
						}
						catch
						{
							this.@value = CacheHelper.Unknown;
							throw;
						}
						if (this.@value == CacheHelper.CycleSentinel)
						{
							Cache<TContainer, TProperty> cache = this;
							if (typeof(TProperty) == typeof(bool))
							{
								obj = CacheHelper.BoxedBool((bool)(object)tProperty);
							}
							else
							{
								obj = tProperty;
							}
							cache.@value = obj;
						}
					}
					obj3 = this.@value;
				}
			}
			return (TProperty)obj3;
		}
	}
}