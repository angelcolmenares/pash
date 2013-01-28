using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class SearchResultReferenceCollection : ReadOnlyCollectionBase
	{
		public SearchResultReference this[int index]
		{
			get
			{
				return (SearchResultReference)base.InnerList[index];
			}
		}

		internal SearchResultReferenceCollection()
		{
		}

		internal int Add(SearchResultReference reference)
		{
			return base.InnerList.Add(reference);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(SearchResultReference value)
		{
			return base.InnerList.Contains(value);
		}

		public void CopyTo(SearchResultReference[] values, int index)
		{
			base.InnerList.CopyTo(values, index);
		}

		public int IndexOf(SearchResultReference value)
		{
			return base.InnerList.IndexOf(value);
		}
	}
}