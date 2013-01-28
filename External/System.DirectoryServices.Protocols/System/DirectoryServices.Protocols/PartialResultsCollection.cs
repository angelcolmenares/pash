using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class PartialResultsCollection : ReadOnlyCollectionBase
	{
		public object this[int index]
		{
			get
			{
				return base.InnerList[index];
			}
		}

		internal PartialResultsCollection()
		{
		}

		internal int Add(object value)
		{
			return base.InnerList.Add(value);
		}

		public bool Contains(object value)
		{
			return base.InnerList.Contains(value);
		}

		public void CopyTo(object[] values, int index)
		{
			base.InnerList.CopyTo(values, index);
		}

		public int IndexOf(object value)
		{
			return base.InnerList.IndexOf(value);
		}
	}
}