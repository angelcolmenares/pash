using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlySiteLinkCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectorySiteLink this[int index]
		{
			get
			{
				return (ActiveDirectorySiteLink)base.InnerList[index];
			}
		}

		internal ReadOnlySiteLinkCollection()
		{
		}

		internal int Add(ActiveDirectorySiteLink link)
		{
			return base.InnerList.Add(link);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySiteLink item = (ActiveDirectorySiteLink)base.InnerList[num];
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
				throw new ArgumentNullException("link");
			}
		}

		public void CopyTo(ActiveDirectorySiteLink[] links, int index)
		{
			base.InnerList.CopyTo(links, index);
		}

		public int IndexOf(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySiteLink item = (ActiveDirectorySiteLink)base.InnerList[num];
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
				throw new ArgumentNullException("link");
			}
		}
	}
}