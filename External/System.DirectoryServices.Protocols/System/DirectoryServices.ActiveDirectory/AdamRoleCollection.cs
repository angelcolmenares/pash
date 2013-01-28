using System;
using System.Collections;
using System.ComponentModel;

namespace System.DirectoryServices.ActiveDirectory
{
	public class AdamRoleCollection : ReadOnlyCollectionBase
	{
		public AdamRole this[int index]
		{
			get
			{
				return (AdamRole)base.InnerList[index];
			}
		}

		internal AdamRoleCollection()
		{
		}

		internal AdamRoleCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(AdamRole role)
		{
			if (role < AdamRole.SchemaRole || role > AdamRole.NamingRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(AdamRole));
			}
			else
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					int item = (int)base.InnerList[num];
					if ((AdamRole)item != role)
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

		public void CopyTo(AdamRole[] roles, int index)
		{
			base.InnerList.CopyTo(roles, index);
		}

		public int IndexOf(AdamRole role)
		{
			if (role < AdamRole.SchemaRole || role > AdamRole.NamingRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(AdamRole));
			}
			else
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					int item = (int)base.InnerList[num];
					if ((AdamRole)item != role)
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