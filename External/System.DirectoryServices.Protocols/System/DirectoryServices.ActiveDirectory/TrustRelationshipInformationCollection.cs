using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class TrustRelationshipInformationCollection : ReadOnlyCollectionBase
	{
		public TrustRelationshipInformation this[int index]
		{
			get
			{
				return (TrustRelationshipInformation)base.InnerList[index];
			}
		}

		internal TrustRelationshipInformationCollection()
		{
		}

		internal TrustRelationshipInformationCollection(DirectoryContext context, string source, ArrayList trusts)
		{
			for (int i = 0; i < trusts.Count; i++)
			{
				TrustObject item = (TrustObject)trusts[i];
				if (item.TrustType != TrustType.Forest && item.TrustType != (TrustType.ParentChild | TrustType.CrossLink | TrustType.External | TrustType.Forest | TrustType.Kerberos | TrustType.Unknown))
				{
					TrustRelationshipInformation trustRelationshipInformation = new TrustRelationshipInformation(context, source, item);
					this.Add(trustRelationshipInformation);
				}
			}
		}

		internal int Add(TrustRelationshipInformation info)
		{
			return base.InnerList.Add(info);
		}

		public bool Contains(TrustRelationshipInformation information)
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

		public void CopyTo(TrustRelationshipInformation[] array, int index)
		{
			base.InnerList.CopyTo(array, index);
		}

		public int IndexOf(TrustRelationshipInformation information)
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