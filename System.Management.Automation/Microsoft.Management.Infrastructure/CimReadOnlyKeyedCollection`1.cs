using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Generic
{
	public abstract class CimReadOnlyKeyedCollection<T> : IEnumerable<T>, IEnumerable
	{
		public abstract int Count
		{
			get;
		}

		public abstract T this[string itemName]
		{
			get;
		}

		internal CimReadOnlyKeyedCollection()
		{
		}

		public abstract IEnumerator<T> GetEnumerator();

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}