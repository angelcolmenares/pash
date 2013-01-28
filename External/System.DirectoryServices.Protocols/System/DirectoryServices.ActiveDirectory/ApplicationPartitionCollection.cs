using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ApplicationPartitionCollection : ReadOnlyCollectionBase
	{
		public ApplicationPartition this[int index]
		{
			get
			{
				return (ApplicationPartition)base.InnerList[index];
			}
		}

		internal ApplicationPartitionCollection()
		{
		}

		internal ApplicationPartitionCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(ApplicationPartition applicationPartition)
		{
			if (applicationPartition != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ApplicationPartition item = (ApplicationPartition)base.InnerList[num];
					if (Utils.Compare(item.Name, applicationPartition.Name) != 0)
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
				throw new ArgumentNullException("applicationPartition");
			}
		}

		public void CopyTo(ApplicationPartition[] applicationPartitions, int index)
		{
			base.InnerList.CopyTo(applicationPartitions, index);
		}

		public int IndexOf(ApplicationPartition applicationPartition)
		{
			if (applicationPartition != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					ApplicationPartition item = (ApplicationPartition)base.InnerList[num];
					if (Utils.Compare(item.Name, applicationPartition.Name) != 0)
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
				throw new ArgumentNullException("applicationPartition");
			}
		}
	}
}