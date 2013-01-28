namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using System;
    using System.Collections.Generic;

    internal class CimClassDeserializationCache<TKey>
    {
        private readonly Dictionary<TKey, CimClass> _cimClassIdToClass;

        public CimClassDeserializationCache()
        {
            this._cimClassIdToClass = new Dictionary<TKey, CimClass>();
        }

        internal void AddCimClassToCache(TKey key, CimClass cimClass)
        {
            if (this._cimClassIdToClass.Count >= DeserializationContext.MaxItemsInCimClassCache)
            {
                this._cimClassIdToClass.Clear();
            }
            this._cimClassIdToClass.Add(key, cimClass);
        }

        internal CimClass GetCimClassFromCache(TKey key)
        {
            CimClass class2;
            if (this._cimClassIdToClass.TryGetValue(key, out class2))
            {
                return class2;
            }
            return null;
        }
    }
}

