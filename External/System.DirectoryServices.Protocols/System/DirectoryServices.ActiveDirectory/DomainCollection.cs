using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class DomainCollection : ReadOnlyCollectionBase
	{
		public Domain this[int index]
		{
			get
			{
				return (Domain)base.InnerList[index];
			}
		}

		internal DomainCollection()
		{
		}

		internal DomainCollection(ArrayList values)
		{
			if (values != null)
			{
				for (int i = 0; i < values.Count; i++)
				{
					this.Add((Domain)values[i]);
				}
			}
		}

		internal int Add(Domain domain)
		{
			return base.InnerList.Add(domain);
		}

		internal void Clear()
		{
			base.InnerList.Clear();
		}

		public bool Contains(Domain domain)
		{
			if (domain != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					Domain item = (Domain)base.InnerList[num];
					if (Utils.Compare(item.Name, domain.Name) != 0)
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
				throw new ArgumentNullException("domain");
			}
		}

		public void CopyTo(Domain[] domains, int index)
		{
			base.InnerList.CopyTo(domains, index);
		}

		public int IndexOf(Domain domain)
		{
			if (domain != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					Domain item = (Domain)base.InnerList[num];
					if (Utils.Compare(item.Name, domain.Name) != 0)
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
				throw new ArgumentNullException("domain");
			}
		}
	}
}