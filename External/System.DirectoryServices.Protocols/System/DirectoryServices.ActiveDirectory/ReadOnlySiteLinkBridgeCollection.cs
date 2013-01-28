using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlySiteLinkBridgeCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectorySiteLinkBridge this[int index]
		{
			get
			{
				return (ActiveDirectorySiteLinkBridge)base.InnerList[index];
			}
		}

		internal ReadOnlySiteLinkBridgeCollection()
		{
		}

		internal int Add(ActiveDirectorySiteLinkBridge bridge)
		{
			return base.InnerList.Add(bridge);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(ActiveDirectorySiteLinkBridge bridge)
		{
			if (bridge != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(bridge.context, bridge.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySiteLinkBridge item = (ActiveDirectorySiteLinkBridge)base.InnerList[num];
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
				throw new ArgumentNullException("bridge");
			}
		}

		public void CopyTo(ActiveDirectorySiteLinkBridge[] bridges, int index)
		{
			base.InnerList.CopyTo(bridges, index);
		}

		public int IndexOf(ActiveDirectorySiteLinkBridge bridge)
		{
			if (bridge != null)
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(bridge.context, bridge.cachedEntry, PropertyManager.DistinguishedName);
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySiteLinkBridge item = (ActiveDirectorySiteLinkBridge)base.InnerList[num];
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
				throw new ArgumentNullException("bridge");
			}
		}
	}
}