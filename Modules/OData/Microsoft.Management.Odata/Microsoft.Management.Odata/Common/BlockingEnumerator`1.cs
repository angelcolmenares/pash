using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Common
{
	internal class BlockingEnumerator<TItem> : IEnumerator<TItem>, IDisposable, IEnumerator
	{
		private IEnumerator<TItem> currentEnumerator;

		private AsyncDataStore<TItem> dataStore;

		public object Current
		{
			get
			{
				return this.currentEnumerator.Current;
			}
		}

		TItem System.Collections.Generic.IEnumerator<TItem>.Current
		{
			get
			{
				return this.currentEnumerator.Current;
			}
		}

		public BlockingEnumerator(AsyncDataStore<TItem> dataStore)
		{
			this.dataStore = dataStore;
			this.currentEnumerator = null;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (this.currentEnumerator == null || !this.currentEnumerator.MoveNext())
			{
				this.currentEnumerator = this.dataStore.Get();
				if (this.currentEnumerator != null)
				{
					return this.MoveNext();
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}