using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADObjectSearchResultEnumerator : IEnumerator<ADObject>, IDisposable, IEnumerator
	{
		private const string _debugCategory = "ADObjectSearchResultEnumerator";

		private ADObjectSearcher _searcher;

		private int _searchSizeLimit;

		private int _searchPageSize;

		private object _pageCookie;

		private IList<ADObject> _currentPage;

		private int _positionInCurrentPage;

		private int _positionInResultSet;

		private bool _hasSizeLimitExceedInLastSearch;

		ADObject System.Collections.Generic.IEnumerator<Microsoft.ActiveDirectory.Management.ADObject>.Current
		{
			get
			{
				if (this._currentPage != null)
				{
					return this._currentPage[this._positionInCurrentPage];
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		public ADObject Current {
			get {
				if (this._currentPage != null)
				{
					return this._currentPage[this._positionInCurrentPage];
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}


		public ADObjectSearchResultEnumerator(ADObjectSearcher searcher) : this(searcher, searcher.PageSize, searcher.SizeLimit)
		{
		}

		public ADObjectSearchResultEnumerator(ADObjectSearcher searcher, int pageSize, int sizeLimit)
		{
			this._positionInCurrentPage = -1;
			this._positionInResultSet = -1;
			this._searcher = searcher;
			this._searchSizeLimit = sizeLimit;
			this._searchPageSize = pageSize;
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			bool flag = false;
			ADObjectSearchResultEnumerator aDObjectSearchResultEnumerator = this;
			aDObjectSearchResultEnumerator._positionInCurrentPage = aDObjectSearchResultEnumerator._positionInCurrentPage + 1;
			ADObjectSearchResultEnumerator aDObjectSearchResultEnumerator1 = this;
			aDObjectSearchResultEnumerator1._positionInResultSet = aDObjectSearchResultEnumerator1._positionInResultSet + 1;
			if (this._currentPage == null || this._positionInCurrentPage >= this._currentPage.Count)
			{
				if (this._currentPage == null || this._pageCookie != null)
				{
					DebugLogger.LogInfo("ADObjectSearchResultEnumerator", "Fetching next page.. and resetting current page position to 0");
					int num = this._searchPageSize;
					if (this._searchSizeLimit > 0)
					{
						int num1 = this._searchSizeLimit - this._positionInResultSet;
						if (num1 < this._searchPageSize)
						{
							num = num1 + 1;
						}
					}
					this._currentPage = this._searcher.PagedSearch(ref this._pageCookie, out this._hasSizeLimitExceedInLastSearch, num, 0);
					if (this._currentPage == null || this._currentPage.Count == 0)
					{
						flag = false;
					}
					else
					{
						this._positionInCurrentPage = 0;
						flag = true;
					}
				}
				else
				{
					DebugLogger.LogInfo("ADObjectSearchResultEnumerator", "pageCookie is null.. end of search results.");
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				if (this._searchSizeLimit <= 0 || this._positionInResultSet < this._searchSizeLimit || !this._hasSizeLimitExceedInLastSearch)
				{
					return false;
				}
				else
				{
					throw new ADException(StringResources.SearchSizeLimitExceeded);
				}
			}
			else
			{
				if (this._searchSizeLimit <= 0 || this._positionInResultSet < this._searchSizeLimit)
				{
					return true;
				}
				else
				{
					throw new ADException(StringResources.SearchSizeLimitExceeded);
				}
			}
		}

		void System.Collections.IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		void System.IDisposable.Dispose()
		{
			if (this._pageCookie != null)
			{
				this._searcher.AbandonPagedSearch(ref this._pageCookie);
			}
		}
	}
}