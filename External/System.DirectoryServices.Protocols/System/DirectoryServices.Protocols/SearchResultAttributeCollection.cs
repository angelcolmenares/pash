using System;
using System.Collections;
using System.Globalization;

namespace System.DirectoryServices.Protocols
{
	public class SearchResultAttributeCollection : DictionaryBase
	{
		public ICollection AttributeNames
		{
			get
			{
				return base.Dictionary.Keys;
			}
		}

		public DirectoryAttribute this[string attributeName]
		{
			get
			{
				if (attributeName != null)
				{
					object lower = attributeName.ToLower(CultureInfo.InvariantCulture);
					return (DirectoryAttribute)base.InnerHashtable[lower];
				}
				else
				{
					throw new ArgumentNullException("attributeName");
				}
			}
		}

		public ICollection Values
		{
			get
			{
				return base.Dictionary.Values;
			}
		}

		internal SearchResultAttributeCollection()
		{
		}

		internal void Add(string name, DirectoryAttribute value)
		{
			base.Dictionary.Add(name.ToLower(CultureInfo.InvariantCulture), value);
		}

		public bool Contains(string attributeName)
		{
			if (attributeName != null)
			{
				object lower = attributeName.ToLower(CultureInfo.InvariantCulture);
				return base.Dictionary.Contains(lower);
			}
			else
			{
				throw new ArgumentNullException("attributeName");
			}
		}

		public void CopyTo(DirectoryAttribute[] array, int index)
		{
			base.Dictionary.Values.CopyTo(array, index);
		}
	}
}