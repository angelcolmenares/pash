using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class TopLevelNameCollection : ReadOnlyCollectionBase
	{
		public TopLevelName this[int index]
		{
			get
			{
				return (TopLevelName)base.InnerList[index];
			}
		}

		internal TopLevelNameCollection()
		{
		}

		internal int Add(TopLevelName name)
		{
			if (name != null)
			{
				return base.InnerList.Add(name);
			}
			else
			{
				throw new ArgumentNullException("name");
			}
		}

		public bool Contains(TopLevelName name)
		{
			if (name != null)
			{
				return base.InnerList.Contains(name);
			}
			else
			{
				throw new ArgumentNullException("name");
			}
		}

		public void CopyTo(TopLevelName[] names, int index)
		{
			base.InnerList.CopyTo(names, index);
		}

		public int IndexOf(TopLevelName name)
		{
			if (name != null)
			{
				return base.InnerList.IndexOf(name);
			}
			else
			{
				throw new ArgumentNullException("name");
			}
		}
	}
}