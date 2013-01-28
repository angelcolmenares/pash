namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class CimClassSerializationCache<TKey>
    {
        private readonly HashSet<TKey> _cimClassesHeldByDeserializer;

        public CimClassSerializationCache()
        {
            this._cimClassesHeldByDeserializer = new HashSet<TKey>(EqualityComparer<TKey>.Default);
        }

        internal void AddClassToCache(TKey key)
        {
            if (this._cimClassesHeldByDeserializer.Count >= DeserializationContext.MaxItemsInCimClassCache)
            {
                this._cimClassesHeldByDeserializer.Clear();
            }
            this._cimClassesHeldByDeserializer.Add(key);
        }

        internal bool DoesDeserializerAlreadyHaveCimClass(TKey key)
        {
            return this._cimClassesHeldByDeserializer.Contains(key);
        }
    }
}

