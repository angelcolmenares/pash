using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class SearchResultEntryCollection : ReadOnlyCollectionBase
	{
		public SearchResultEntry this[int index]
		{
			get
			{
				return (SearchResultEntry)base.InnerList[index];
			}
		}

		internal SearchResultEntryCollection()
		{
		}

		internal int Add(SearchResultEntry entry)
		{
			return base.InnerList.Add(entry);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(SearchResultEntry value)
		{
			return base.InnerList.Contains(value);
		}

		public void CopyTo(SearchResultEntry[] values, int index)
		{
			base.InnerList.CopyTo(values, index);
		}

		public int IndexOf(SearchResultEntry value)
		{
			return base.InnerList.IndexOf(value);
		}
	}
}