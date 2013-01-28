using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlyDirectoryServerCollection : ReadOnlyCollectionBase
	{
		public DirectoryServer this[int index]
		{
			get
			{
				return (DirectoryServer)base.InnerList[index];
			}
		}

		internal ReadOnlyDirectoryServerCollection()
		{
		}

		internal ReadOnlyDirectoryServerCollection(ArrayList values)
		{
			if (values != null)
			{
				for (int i = 0; i < values.Count; i++)
				{
					this.Add((DirectoryServer)values[i]);
				}
			}
		}

		internal int Add(DirectoryServer server)
		{
			return base.InnerList.Add(server);
		}

		internal void AddRange(ICollection servers)
		{
			base.InnerList.AddRange(servers);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(DirectoryServer directoryServer)
		{
			if (directoryServer != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DirectoryServer item = (DirectoryServer)base.InnerList[num];
					if (Utils.Compare(item.Name, directoryServer.Name) != 0)
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
				throw new ArgumentNullException("directoryServer");
			}
		}

		public void CopyTo(DirectoryServer[] directoryServers, int index)
		{
			base.InnerList.CopyTo(directoryServers, index);
		}

		public int IndexOf(DirectoryServer directoryServer)
		{
			if (directoryServer != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DirectoryServer item = (DirectoryServer)base.InnerList[num];
					if (Utils.Compare(item.Name, directoryServer.Name) != 0)
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
				throw new ArgumentNullException("directoryServer");
			}
		}
	}
}