using System;
using System.Collections;
using System.DirectoryServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationConnectionCollection : ReadOnlyCollectionBase
	{
		public ReplicationConnection this[int index]
		{
			get
			{
				return (ReplicationConnection)base.InnerList[index];
			}
		}

		internal ReplicationConnectionCollection()
		{
		}

		internal int Add(ReplicationConnection value)
		{
			return base.InnerList.Add(value);
		}

		public bool Contains(ReplicationConnection connection)
		{
			if (connection != null)
			{
				if (connection.existingConnection)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(connection.context, connection.cachedDirectoryEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ReplicationConnection item = (ReplicationConnection)base.InnerList[num];
						string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedDirectoryEntry, PropertyManager.DistinguishedName);
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
					object[] name = new object[1];
					name[0] = connection.Name;
					throw new InvalidOperationException(Res.GetString("ConnectionNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("connection");
			}
		}

		public void CopyTo(ReplicationConnection[] connections, int index)
		{
			base.InnerList.CopyTo(connections, index);
		}

		public int IndexOf(ReplicationConnection connection)
		{
			if (connection != null)
			{
				if (connection.existingConnection)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(connection.context, connection.cachedDirectoryEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ReplicationConnection item = (ReplicationConnection)base.InnerList[num];
						string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedDirectoryEntry, PropertyManager.DistinguishedName);
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
					object[] name = new object[1];
					name[0] = connection.Name;
					throw new InvalidOperationException(Res.GetString("ConnectionNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("connection");
			}
		}
	}
}