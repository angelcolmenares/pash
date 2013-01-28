using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class GlobalCatalogCollection : ReadOnlyCollectionBase
	{
		public GlobalCatalog this[int index]
		{
			get
			{
				return (GlobalCatalog)base.InnerList[index];
			}
		}

		internal GlobalCatalogCollection()
		{
		}

		internal GlobalCatalogCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(GlobalCatalog globalCatalog)
		{
			if (globalCatalog != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					GlobalCatalog item = (GlobalCatalog)base.InnerList[num];
					if (Utils.Compare(item.Name, globalCatalog.Name) != 0)
					{
						num++;
					}
					else
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				throw new ArgumentNullException("globalCatalog");
			}
		}

		public void CopyTo(GlobalCatalog[] globalCatalogs, int index)
		{
			base.InnerList.CopyTo(globalCatalogs, index);
		}

		public int IndexOf(GlobalCatalog globalCatalog)
		{
			if (globalCatalog != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					GlobalCatalog item = (GlobalCatalog)base.InnerList[num];
					if (Utils.Compare(item.Name, globalCatalog.Name) != 0)
					{
						num++;
					}
					else
					{
						return num;
					}
				}
				return -1;
			}
			else
			{
				throw new ArgumentNullException("globalCatalog");
			}
		}
	}
}