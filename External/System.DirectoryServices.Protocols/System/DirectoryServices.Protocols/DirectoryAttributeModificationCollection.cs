using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryAttributeModificationCollection : CollectionBase
	{
		public DirectoryAttributeModification this[int index]
		{
			get
			{
				return (DirectoryAttributeModification)base.List[index];
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

		public DirectoryAttributeModificationCollection()
		{
			Utility.CheckOSVersion();
		}

		public int Add(DirectoryAttributeModification attribute)
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

		public void AddRange(DirectoryAttributeModification[] attributes)
		{
			if (attributes != null)
			{
				DirectoryAttributeModification[] directoryAttributeModificationArray = attributes;
				int num = 0;
				while (num < (int)directoryAttributeModificationArray.Length)
				{
					DirectoryAttributeModification directoryAttributeModification = directoryAttributeModificationArray[num];
					if (directoryAttributeModification != null)
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

		public void AddRange(DirectoryAttributeModificationCollection attributeCollection)
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

		public bool Contains(DirectoryAttributeModification value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(DirectoryAttributeModification[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(DirectoryAttributeModification value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, DirectoryAttributeModification value)
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
				if (value as DirectoryAttributeModification != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "DirectoryAttributeModification";
					throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "value");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
			}
		}

		public void Remove(DirectoryAttributeModification value)
		{
			base.List.Remove(value);
		}
	}
}