using Microsoft.Management.Odata.Common;
using System;
using System.Text;

namespace Microsoft.Management.Odata.Core
{
	internal class UserDataCache : DictionaryCache<UserContext, UserData>, IDisposable
	{
		private const int UserDataCacheTimeout = 60;

		private CacheController userDataCacheController;

		private CacheController invokeCacheController;

		private bool disposed;

		public UserDataCache(int invokeLifeTime) : base(0x7fffffff)
		{
			this.userDataCacheController = new CacheController(60);
			this.userDataCacheController.RegisterCache(this);
			this.invokeCacheController = new CacheController(invokeLifeTime);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!this.disposed && disposing)
			{
				if (this.invokeCacheController != null)
				{
					this.invokeCacheController.Dispose();
					this.invokeCacheController = null;
				}
				if (this.userDataCacheController != null)
				{
					this.userDataCacheController.UnregisterCache(this);
					this.userDataCacheController.Dispose();
					this.userDataCacheController = null;
				}
				this.disposed = true;
			}
		}

		public UserDataCache.UserDataEnvelope Get(UserContext userContext)
		{
			TraceHelper.Current.DebugMessage(string.Concat("UserDataCache.Get. Getting user data envelope ", userContext.ToString()));
			UserData userDatum = null;
			if (!base.TryLockKey(userContext, out userDatum))
			{
				TraceHelper.Current.DebugMessage(string.Concat("UserDataCache.Get. User not found. Adding a new user data for ", userContext.ToString()));
				base.AddOrLockKey(userContext, new UserData(this.invokeCacheController), out userDatum);
			}
			this.Trace();
			return new UserDataCache.UserDataEnvelope(this, userContext, userDatum);
		}

		internal CacheController TestHookGetInvokeCacheController()
		{
			return this.invokeCacheController;
		}

		public void Trace()
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(this.ToTraceMessage("User Data Cache. Getting item", new StringBuilder()).ToString());
			}
		}

		internal class UserDataEnvelope : IDisposable
		{
			private DictionaryCache<UserContext, UserData> cache;

			private UserContext userContext;

			public UserData Data
			{
				get;
				private set;
			}

			public UserDataEnvelope(DictionaryCache<UserContext, UserData> cache, UserContext userContext, UserData data)
			{
				this.cache = cache;
				this.userContext = userContext;
				this.Data = data;
			}

			public void Dispose()
			{
				this.cache.TryUnlockKey(this.userContext);
				GC.SuppressFinalize(this);
			}
		}
	}
}