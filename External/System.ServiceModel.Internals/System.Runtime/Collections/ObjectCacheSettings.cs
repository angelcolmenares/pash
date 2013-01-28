using System;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal class ObjectCacheSettings
	{
		private int cacheLimit;

		private TimeSpan idleTimeout;

		private TimeSpan leaseTimeout;

		private int purgeFrequency;

		private static TimeSpan DefaultIdleTimeout;

		private static TimeSpan DefaultLeaseTimeout;

		private const int DefaultCacheLimit = 64;

		private const int DefaultPurgeFrequency = 32;

		public int CacheLimit
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.cacheLimit;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.cacheLimit = value;
			}
		}

		public TimeSpan IdleTimeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.idleTimeout;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.idleTimeout = value;
			}
		}

		public TimeSpan LeaseTimeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.leaseTimeout;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.leaseTimeout = value;
			}
		}

		public int PurgeFrequency
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.purgeFrequency;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.purgeFrequency = value;
			}
		}

		static ObjectCacheSettings()
		{
			ObjectCacheSettings.DefaultIdleTimeout = TimeSpan.FromMinutes(2);
			ObjectCacheSettings.DefaultLeaseTimeout = TimeSpan.FromMinutes(5);
		}

		public ObjectCacheSettings()
		{
			this.CacheLimit = 64;
			this.IdleTimeout = ObjectCacheSettings.DefaultIdleTimeout;
			this.LeaseTimeout = ObjectCacheSettings.DefaultLeaseTimeout;
			this.PurgeFrequency = 32;
		}

		private ObjectCacheSettings(ObjectCacheSettings other)
		{
			this.CacheLimit = other.CacheLimit;
			this.IdleTimeout = other.IdleTimeout;
			this.LeaseTimeout = other.LeaseTimeout;
			this.PurgeFrequency = other.PurgeFrequency;
		}

		internal ObjectCacheSettings Clone()
		{
			return new ObjectCacheSettings(this);
		}
	}
}