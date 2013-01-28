using System;
using System.Collections.Generic;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExtensionCache
	{
		private Dictionary<string, ExtensionCacheValue> cache;

		internal Dictionary<string, ExtensionCacheValue> properties
		{
			get
			{
				return this.cache;
			}
		}

		internal ExtensionCache()
		{
			this.cache = new Dictionary<string, ExtensionCacheValue>();
		}

		internal bool TryGetValue(string attr, out ExtensionCacheValue o)
		{
			return this.cache.TryGetValue(attr, out o);
		}
	}
}