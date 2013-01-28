using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ForestTrustRelationshipCollisionCollection : ReadOnlyCollectionBase
	{
		public ForestTrustRelationshipCollision this[int index]
		{
			get
			{
				return (ForestTrustRelationshipCollision)base.InnerList[index];
			}
		}

		internal ForestTrustRelationshipCollisionCollection()
		{
		}

		internal int Add(ForestTrustRelationshipCollision collision)
		{
			return base.InnerList.Add(collision);
		}

		public bool Contains(ForestTrustRelationshipCollision collision)
		{
			if (collision != null)
			{
				return base.InnerList.Contains(collision);
			}
			else
			{
				throw new ArgumentNullException("collision");
			}
		}

		public void CopyTo(ForestTrustRelationshipCollision[] array, int index)
		{
			base.InnerList.CopyTo(array, index);
		}

		public int IndexOf(ForestTrustRelationshipCollision collision)
		{
			if (collision != null)
			{
				return base.InnerList.IndexOf(collision);
			}
			else
			{
				throw new ArgumentNullException("collision");
			}
		}
	}
}