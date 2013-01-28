using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Internal
{
	internal class HashSetInternal<T> : IEnumerable<T>, IEnumerable
	{
		private readonly Dictionary<T, object> wrappedDictionary;

		public HashSetInternal()
		{
			this.wrappedDictionary = new Dictionary<T, object>();
		}

		public bool Add(T thingToAdd)
		{
			if (!this.wrappedDictionary.ContainsKey(thingToAdd))
			{
				this.wrappedDictionary[thingToAdd] = null;
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Contains(T item)
		{
			return this.wrappedDictionary.ContainsKey(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.wrappedDictionary.Keys.GetEnumerator();
		}

		public void Remove(T item)
		{
			this.wrappedDictionary.Remove(item);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}