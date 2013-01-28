using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlyActiveDirectorySchemaClassCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectorySchemaClass this[int index]
		{
			get
			{
				return (ActiveDirectorySchemaClass)base.InnerList[index];
			}
		}

		internal ReadOnlyActiveDirectorySchemaClassCollection()
		{
		}

		internal ReadOnlyActiveDirectorySchemaClassCollection(ICollection values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySchemaClass item = (ActiveDirectorySchemaClass)base.InnerList[num];
					if (Utils.Compare(item.Name, schemaClass.Name) != 0)
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
				throw new ArgumentNullException("schemaClass");
			}
		}

		public void CopyTo(ActiveDirectorySchemaClass[] classes, int index)
		{
			base.InnerList.CopyTo(classes, index);
		}

		public int IndexOf(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySchemaClass item = (ActiveDirectorySchemaClass)base.InnerList[num];
					if (Utils.Compare(item.Name, schemaClass.Name) != 0)
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
				throw new ArgumentNullException("schemaClass");
			}
		}
	}
}