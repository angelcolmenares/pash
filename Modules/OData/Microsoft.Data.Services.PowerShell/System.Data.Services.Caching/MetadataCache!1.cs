namespace System.Data.Services.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Services.Providers;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class MetadataCache<T>
    {
        private static readonly Dictionary<MetadataCacheKey, T> cache;
        private static readonly ReaderWriterLockSlim cacheLock;

        static MetadataCache()
        {
            MetadataCache<T>.cache = new Dictionary<MetadataCacheKey, T>(new MetadataCacheKey.Comparer());
            MetadataCache<T>.cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        internal static T AddCacheItem(Type serviceType, object dataContextInstance, T item)
        {
            T local;
            MetadataCacheKey key = new MetadataCacheKey(serviceType, DbContextHelper.GetObjectContext(dataContextInstance));
            MetadataCache<T>.cacheLock.EnterWriteLock();
            try
            {
                if (!MetadataCache<T>.cache.TryGetValue(key, out local))
                {
                    MetadataCache<T>.cache.Add(key, item);
                    local = item;
                }
            }
            finally
            {
                MetadataCache<T>.cacheLock.ExitWriteLock();
            }
            return local;
        }

        internal static T TryLookup(Type serviceType, object dataContextInstance)
        {
            T local;
            MetadataCacheKey key = new MetadataCacheKey(serviceType, DbContextHelper.GetObjectContext(dataContextInstance));
            MetadataCache<T>.cacheLock.EnterReadLock();
            try
            {
                MetadataCache<T>.cache.TryGetValue(key, out local);
            }
            finally
            {
                MetadataCache<T>.cacheLock.ExitReadLock();
            }
            return local;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MetadataCacheKey
        {
            private readonly string dataContextConnection;
            private readonly int hashCode;
            private readonly Type serviceType;
            internal MetadataCacheKey(Type serviceType, ObjectContext dataContextInstance)
            {
                this.serviceType = serviceType;
                this.dataContextConnection = null;
                this.hashCode = this.serviceType.GetHashCode();
                if (dataContextInstance != null)
                {
                    EntityConnection connection = dataContextInstance.Connection as EntityConnection;
                    if (connection != null)
                    {
                        this.dataContextConnection = new EntityConnectionStringBuilder(connection.ConnectionString).Metadata;
                        this.hashCode ^= this.dataContextConnection.GetHashCode();
                    }
                }
            }
            internal class Comparer : IEqualityComparer<MetadataCache<T>.MetadataCacheKey>
            {
                public bool Equals(MetadataCache<T>.MetadataCacheKey x, MetadataCache<T>.MetadataCacheKey y)
                {
                    return ((x.dataContextConnection == y.dataContextConnection) && (x.serviceType == y.serviceType));
                }

                public int GetHashCode(MetadataCache<T>.MetadataCacheKey obj)
                {
                    return obj.hashCode;
                }
            }
        }
    }
}

