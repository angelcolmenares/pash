using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryAttributeCollection : CollectionBase
	{
		public DirectoryAttribute this[int index]
		{
			get
			{
				return (DirectoryAttribute)base.List[index];
			}
			set
			{
				if (value != null)
				{
					base.List[index] = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
				}
			}
		}

		public DirectoryAttributeCollection()
		{
			Utility.CheckOSVersion();
		}

		public int Add(DirectoryAttribute attribute)
		{
			if (attribute != null)
			{
				return base.List.Add(attribute);
			}
			else
			{
				throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
			}
		}

		public void AddRange(DirectoryAttribute[] attributes)
		{
			if (attributes != null)
			{
				DirectoryAttribute[] directoryAttributeArray = attributes;
				int num = 0;
				while (num < (int)directoryAttributeArray.Length)
				{
					DirectoryAttribute directoryAttribute = directoryAttributeArray[num];
					if (directoryAttribute != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
					}
				}
				base.InnerList.AddRange(attributes);
				return;
			}
			else
			{
				throw new ArgumentNullException("attributes");
			}
		}

		public void AddRange(DirectoryAttributeCollection attributeCollection)
		{
			if (attributeCollection != null)
			{
				int count = attributeCollection.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(attributeCollection[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("attributeCollection");
			}
		}

		public bool Contains(DirectoryAttribute value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(DirectoryAttribute[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(DirectoryAttribute value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, DirectoryAttribute value)
		{
			if (value != null)
			{
				base.List.Insert(index, value);
				return;
			}
			else
			{
				throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
			}
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (value as DirectoryAttribute != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "DirectoryAttribute";
					throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "value");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
			}
		}

		public void Remove(DirectoryAttribute value)
		{
			base.List.Remove(value);
		}
	}
}