using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlySiteCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectorySite this[int index]
		{
			get
			{
				return (ActiveDirectorySite)base.InnerList[index];
			}
		}

		internal ReadOnlySiteCollection()
		{
		}

		internal ReadOnlySiteCollection(ArrayList sites)
		{
			for (int i = 0; i < sites.Count; i++)
			{
				this.Add((ActiveDirectorySite)sites[i]);
			}
		}

		internal int Add(ActiveDirectorySite site)
		{
			return base.InnerList.Add(site);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(ActiveDirectorySite site)
		{
			if (site != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySite item = (ActiveDirectorySite)base.InnerList[num];
					string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedEntry, PropertyManager.DistinguishedName);
					if (Utils.Compare(str, propertyValue) != 0)
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
				throw new ArgumentNullException("site");
			}
		}

		public void CopyTo(ActiveDirectorySite[] sites, int index)
		{
			base.InnerList.CopyTo(sites, index);
		}

		public int IndexOf(ActiveDirectorySite site)
		{
			if (site != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySite item = (ActiveDirectorySite)base.InnerList[num];
					string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedEntry, PropertyManager.DistinguishedName);
					if (Utils.Compare(str, propertyValue) != 0)
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
				throw new ArgumentNullException("site");
			}
		}
	}
}