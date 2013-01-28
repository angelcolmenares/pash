using System;
using System.Collections.Specialized;
using System.DirectoryServices;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class ADSearcher
	{
		private DirectorySearcher searcher;

		private static TimeSpan defaultTimeSpan;

		public string Filter
		{
			get
			{
				return this.searcher.Filter;
			}
			set
			{
				this.searcher.Filter = value;
			}
		}

		public StringCollection PropertiesToLoad
		{
			get
			{
				return this.searcher.PropertiesToLoad;
			}
		}

		static ADSearcher()
		{
			ADSearcher.defaultTimeSpan = new TimeSpan(0, 120, 0);
		}

		public ADSearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope)
		{
			this.searcher = new DirectorySearcher(searchRoot, filter, propertiesToLoad, scope);
			this.searcher.CacheResults = false;
			this.searcher.ClientTimeout = ADSearcher.defaultTimeSpan;
			this.searcher.ServerPageTimeLimit = ADSearcher.defaultTimeSpan;
			this.searcher.PageSize = 0x200;
		}

		public ADSearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope, bool pagedSearch, bool cacheResults)
		{
			this.searcher = new DirectorySearcher(searchRoot, filter, propertiesToLoad, scope);
			this.searcher.ClientTimeout = ADSearcher.defaultTimeSpan;
			if (pagedSearch)
			{
				this.searcher.PageSize = 0x200;
				this.searcher.ServerPageTimeLimit = ADSearcher.defaultTimeSpan;
			}
			if (!cacheResults)
			{
				this.searcher.CacheResults = false;
				return;
			}
			else
			{
				this.searcher.CacheResults = true;
				return;
			}
		}

		public void Dispose()
		{
			this.searcher.Dispose();
		}

		public SearchResultCollection FindAll()
		{
			return this.searcher.FindAll();
		}

		public SearchResult FindOne()
		{
			return this.searcher.FindOne();
		}
	}
}