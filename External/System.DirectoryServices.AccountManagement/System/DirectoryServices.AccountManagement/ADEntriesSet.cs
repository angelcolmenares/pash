using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class ADEntriesSet : ResultSet
	{
		private SearchResultCollection searchResults;

		private ADStoreCtx storeCtx;

		private IEnumerator enumerator;

		private SearchResult current;

		private bool endReached;

		private bool disposed;

		private object discriminant;

		internal override object CurrentAsPrincipal
		{
			get
			{
				return ADUtils.SearchResultAsPrincipal(this.current, this.storeCtx, this.discriminant);
			}
		}

		internal ADEntriesSet(SearchResultCollection src, ADStoreCtx storeCtx)
		{
			this.searchResults = src;
			this.storeCtx = storeCtx;
			this.enumerator = src.GetEnumerator();
		}

		internal ADEntriesSet(SearchResultCollection src, ADStoreCtx storeCtx, object discriminant) : this(src, storeCtx)
		{
			this.discriminant = discriminant;
		}

		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					this.searchResults.Dispose();
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		internal override bool MoveNext()
		{
			bool flag = this.enumerator.MoveNext();
			if (!flag)
			{
				this.endReached = true;
			}
			else
			{
				this.current = (SearchResult)this.enumerator.Current;
			}
			return flag;
		}

		internal override void Reset()
		{
			this.endReached = false;
			this.current = null;
			if (this.enumerator != null)
			{
				this.enumerator.Reset();
			}
		}
	}
}