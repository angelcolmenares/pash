using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SAMQuerySet : ResultSet
	{
		private SAMStoreCtx storeCtx;

		private DirectoryEntry ctxBase;

		private DirectoryEntries entries;

		private IEnumerator enumerator;

		private DirectoryEntry current;

		private int sizeLimit;

		private List<string> schemaTypes;

		private SAMMatcher matcher;

		private int resultsReturned;

		private bool endReached;

		internal override object CurrentAsPrincipal
		{
			get
			{
				return SAMUtils.DirectoryEntryAsPrincipal(this.current, this.storeCtx);
			}
		}

		internal SAMQuerySet(List<string> schemaTypes, DirectoryEntries entries, DirectoryEntry ctxBase, int sizeLimit, SAMStoreCtx storeCtx, SAMMatcher samMatcher)
		{
			this.schemaTypes = schemaTypes;
			this.entries = entries;
			this.sizeLimit = sizeLimit;
			this.storeCtx = storeCtx;
			this.ctxBase = ctxBase;
			this.matcher = samMatcher;
			this.enumerator = this.entries.GetEnumerator();
		}

		private bool IsOfCorrectType(DirectoryEntry de)
		{
			bool flag;
			List<string>.Enumerator enumerator = this.schemaTypes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					if (!SAMUtils.IsOfObjectClass(de, current))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				enumerator.Dispose();
			}
			return flag;
		}

		internal override bool MoveNext()
		{
			bool flag;
			bool flag1 = false;
			if (this.sizeLimit != -1 && this.resultsReturned >= this.sizeLimit)
			{
				this.endReached = true;
			}
			if (!this.endReached)
			{
				do
				{
					flag = this.enumerator.MoveNext();
					flag1 = false;
					if (!flag)
					{
						continue;
					}
					DirectoryEntry current = (DirectoryEntry)this.enumerator.Current;
					if (!this.IsOfCorrectType(current) || !this.matcher.Matches(current))
					{
						flag1 = true;
					}
					else
					{
						this.current = current;
						SAMQuerySet sAMQuerySet = this;
						sAMQuerySet.resultsReturned = sAMQuerySet.resultsReturned + 1;
					}
				}
				while (flag1);
				return flag;
			}
			else
			{
				return false;
			}
		}

		internal override void Reset()
		{
			if (this.current != null)
			{
				this.endReached = false;
				this.current = null;
				if (this.enumerator != null)
				{
					this.enumerator.Reset();
				}
				this.resultsReturned = 0;
			}
		}
	}
}