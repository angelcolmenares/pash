using System;
using System.Collections;
using System.ComponentModel;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectoryRoleCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectoryRole this[int index]
		{
			get
			{
				return (ActiveDirectoryRole)base.InnerList[index];
			}
		}

		internal ActiveDirectoryRoleCollection()
		{
		}

		internal ActiveDirectoryRoleCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(ActiveDirectoryRole role)
		{
			if (role < ActiveDirectoryRole.SchemaRole || role > ActiveDirectoryRole.InfrastructureRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(ActiveDirectoryRole));
			}
			else
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					int item = (int)base.InnerList[num];
					if ((ActiveDirectoryRole)item != role)
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
		}

		public void CopyTo(ActiveDirectoryRole[] roles, int index)
		{
			base.InnerList.CopyTo(roles, index);
		}

		public int IndexOf(ActiveDirectoryRole role)
		{
			if (role < ActiveDirectoryRole.SchemaRole || role > ActiveDirectoryRole.InfrastructureRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(ActiveDirectoryRole));
			}
			else
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					int item = (int)base.InnerList[num];
					if ((ActiveDirectoryRole)item != role)
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
		}
	}
}