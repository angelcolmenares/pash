using Microsoft.Management.Odata;
using System;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal class CacheEntry<TValue> : IDisposable
	{
		private object syncobj;

		private DateTime lastAccessedTime;

		private int lockCount;

		private TValue @value;

		public bool IsLocked
		{
			get
			{
				bool flag;
				lock (this.syncobj)
				{
					flag = this.lockCount > 0;
				}
				return flag;
			}
		}

		public DateTime LastAccessedTime
		{
			get
			{
				DateTime dateTime;
				lock (this.syncobj)
				{
					dateTime = this.lastAccessedTime;
				}
				return dateTime;
			}
		}

		public TValue Value
		{
			get
			{
				TValue tValue;
				lock (this.syncobj)
				{
					this.lastAccessedTime = DateTimeHelper.Now;
					tValue = this.@value;
				}
				return tValue;
			}
		}

		public CacheEntry(TValue value)
		{
			this.syncobj = new object();
			this.@value = value;
			this.lockCount = 1;
			this.lastAccessedTime = DateTimeHelper.Now;
		}

		public void Dispose()
		{
			lock (this.syncobj)
			{
				if (this.@value != null)
				{
					IDisposable disposable = (object)this.@value as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				this.lastAccessedTime = DateTime.MinValue;
				this.lockCount = -1;
				this.@value = default(TValue);
			}
			GC.SuppressFinalize(this);
		}

		public int Lock(out TValue value)
		{
			int num;
			lock (this.syncobj)
			{
				this.lastAccessedTime = DateTimeHelper.Now;
				value = this.@value;
				CacheEntry<TValue> cacheEntry = this;
				int num1 = cacheEntry.lockCount + 1;
				int num2 = num1;
				cacheEntry.lockCount = num1;
				num = num2;
			}
			return num;
		}

		internal int TestHookGetLockCount()
		{
			return this.lockCount;
		}

		internal StringBuilder ToTraceMessage(string message, StringBuilder builder)
		{
			builder.AppendLine(message);
			builder.AppendLine(string.Concat("LastAccessedTime = ", this.lastAccessedTime.ToString()));
			builder.AppendLine(string.Concat("Lock Count = ", this.lockCount.ToString()));
			if (this.@value != null)
			{
				builder.AppendLine(string.Concat("Value = ", this.@value.ToString()));
			}
			return builder;
		}

		public int Unlock()
		{
			int num;
			lock (this.syncobj)
			{
				if (this.lockCount != 0)
				{
					this.lastAccessedTime = DateTimeHelper.Now;
					CacheEntry<TValue> cacheEntry = this;
					int num1 = cacheEntry.lockCount - 1;
					int num2 = num1;
					cacheEntry.lockCount = num1;
					num = num2;
				}
				else
				{
					throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.UnlockAlreadyLockedCache, new object[0]));
				}
			}
			return num;
		}
	}
}