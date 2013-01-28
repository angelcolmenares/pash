using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlyActiveDirectorySchemaPropertyCollection : ReadOnlyCollectionBase
	{
		public ActiveDirectorySchemaProperty this[int index]
		{
			get
			{
				return (ActiveDirectorySchemaProperty)base.InnerList[index];
			}
		}

		internal ReadOnlyActiveDirectorySchemaPropertyCollection()
		{
		}

		internal ReadOnlyActiveDirectorySchemaPropertyCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
					if (Utils.Compare(item.Name, schemaProperty.Name) != 0)
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
				throw new ArgumentNullException("schemaProperty");
			}
		}

		public void CopyTo(ActiveDirectorySchemaProperty[] properties, int index)
		{
			base.InnerList.CopyTo(properties, index);
		}

		public int IndexOf(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
					if (Utils.Compare(item.Name, schemaProperty.Name) != 0)
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
				throw new ArgumentNullException("schemaProperty");
			}
		}
	}
}