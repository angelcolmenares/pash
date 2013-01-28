using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class DomainControllerCollection : ReadOnlyCollectionBase
	{
		public DomainController this[int index]
		{
			get
			{
				return (DomainController)base.InnerList[index];
			}
		}

		internal DomainControllerCollection()
		{
		}

		internal DomainControllerCollection(ArrayList values)
		{
			if (values != null)
			{
				base.InnerList.AddRange(values);
			}
		}

		public bool Contains(DomainController domainController)
		{
			if (domainController != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DomainController item = (DomainController)base.InnerList[num];
					if (Utils.Compare(item.Name, domainController.Name) != 0)
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
				throw new ArgumentNullException("domainController");
			}
		}

		public void CopyTo(DomainController[] domainControllers, int index)
		{
			base.InnerList.CopyTo(domainControllers, index);
		}

		public int IndexOf(DomainController domainController)
		{
			if (domainController != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DomainController item = (DomainController)base.InnerList[num];
					if (Utils.Compare(item.Name, domainController.Name) != 0)
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
				throw new ArgumentNullException("domainController");
			}
		}
	}
}