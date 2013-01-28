using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADCmdletCache : IADCmdletCache
	{
		private const string _debugCategory = "ADCmdletCache";

		private Dictionary<string, object> _cache;

		internal ADCmdletCache()
		{
			this._cache = new Dictionary<string, object>();
		}

		internal void Clear()
		{
			this._cache.Clear();
		}

		public void ClearSubcache(string category)
		{
			this._cache.Remove(category);
		}

		public bool ContainsSubcache(string category)
		{
			return this._cache.ContainsKey(category);
		}

		public object GetSubcache(string category)
		{
			return this._cache[category];
		}

		public void SetSubcache(string category, object subcache)
		{
			this._cache[category] = subcache;
		}
	}
}