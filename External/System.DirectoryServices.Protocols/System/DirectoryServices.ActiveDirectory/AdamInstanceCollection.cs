using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class AdamInstanceCollection : ReadOnlyCollectionBase
	{
		public AdamInstance this[int index]
		{
			get
			{
				return (AdamInstance)base.InnerList[index];
			}
		}

		internal AdamInstanceCollection()
		{
		}

		internal AdamInstanceCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(AdamInstance adamInstance)
		{
			if (adamInstance != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					AdamInstance item = (AdamInstance)base.InnerList[num];
					if (Utils.Compare(item.Name, adamInstance.Name) != 0)
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
				throw new ArgumentNullException("adamInstance");
			}
		}

		public void CopyTo(AdamInstance[] adamInstances, int index)
		{
			base.InnerList.CopyTo(adamInstances, index);
		}

		public int IndexOf(AdamInstance adamInstance)
		{
			if (adamInstance != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					AdamInstance item = (AdamInstance)base.InnerList[num];
					if (Utils.Compare(item.Name, adamInstance.Name) != 0)
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
				throw new ArgumentNullException("adamInstance");
			}
		}
	}
}