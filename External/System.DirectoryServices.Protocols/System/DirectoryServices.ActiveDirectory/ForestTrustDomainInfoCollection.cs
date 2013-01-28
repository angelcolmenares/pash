using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ForestTrustDomainInfoCollection : ReadOnlyCollectionBase
	{
		public ForestTrustDomainInformation this[int index]
		{
			get
			{
				return (ForestTrustDomainInformation)base.InnerList[index];
			}
		}

		internal ForestTrustDomainInfoCollection()
		{
		}

		internal int Add(ForestTrustDomainInformation info)
		{
			return base.InnerList.Add(info);
		}

		public bool Contains(ForestTrustDomainInformation information)
		{
			if (information != null)
			{
				return base.InnerList.Contains(information);
			}
			else
			{
				throw new ArgumentNullException("information");
			}
		}

		public void CopyTo(ForestTrustDomainInformation[] array, int index)
		{
			base.InnerList.CopyTo(array, index);
		}

		public int IndexOf(ForestTrustDomainInformation information)
		{
			if (information != null)
			{
				return base.InnerList.IndexOf(information);
			}
			else
			{
				throw new ArgumentNullException("information");
			}
		}
	}
}