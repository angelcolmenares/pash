using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class AttributeMetadataCollection : ReadOnlyCollectionBase
	{
		public AttributeMetadata this[int index]
		{
			get
			{
				return (AttributeMetadata)base.InnerList[index];
			}
		}

		internal AttributeMetadataCollection()
		{
		}

		internal int Add(AttributeMetadata metadata)
		{
			return base.InnerList.Add(metadata);
		}

		public bool Contains(AttributeMetadata metadata)
		{
			if (metadata != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					AttributeMetadata item = (AttributeMetadata)base.InnerList[num];
					string name = item.Name;
					if (Utils.Compare(name, metadata.Name) != 0)
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
				throw new ArgumentNullException("metadata");
			}
		}

		public void CopyTo(AttributeMetadata[] metadata, int index)
		{
			base.InnerList.CopyTo(metadata, index);
		}

		public int IndexOf(AttributeMetadata metadata)
		{
			if (metadata != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					AttributeMetadata item = (AttributeMetadata)base.InnerList[num];
					if (Utils.Compare(item.Name, metadata.Name) != 0)
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
				throw new ArgumentNullException("metadata");
			}
		}
	}
}