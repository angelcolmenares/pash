using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADObjectSearchResult : IEnumerable<ADObject>, IEnumerable
	{
		private const string _debugCategory = "ADObjectSearchResult";

		private ADObjectSearcher _searcher;

		private int _pageSize;

		private int _sizeLimit;

		public ADObjectSearchResult(ADObjectSearcher searcher)
		{
			this._searcher = searcher;
			this._pageSize = this._searcher.PageSize;
			this._sizeLimit = this._searcher.SizeLimit;
		}

		public ADObjectSearchResult(ADObjectSearcher searcher, int pageSize, int sizeLimit)
		{
			this._searcher = searcher;
			this._pageSize = pageSize;
			this._sizeLimit = sizeLimit;
		}

		IEnumerator<ADObject> System.Collections.Generic.IEnumerable<Microsoft.ActiveDirectory.Management.ADObject>.GetEnumerator()
		{
			return new ADObjectSearchResultEnumerator(this._searcher, this._pageSize, this._sizeLimit);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new ADObjectSearchResultEnumerator(this._searcher, this._pageSize, this._sizeLimit);
		}
	}
}