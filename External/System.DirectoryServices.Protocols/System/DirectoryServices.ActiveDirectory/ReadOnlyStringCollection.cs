using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReadOnlyStringCollection : ReadOnlyCollectionBase
	{
		public string this[int index]
		{
			get
			{
				object item = base.InnerList[index];
				if (item as Exception == null)
				{
					return (string)item;
				}
				else
				{
					throw (Exception)item;
				}
			}
		}

		internal ReadOnlyStringCollection()
		{
		}

		internal ReadOnlyStringCollection(ArrayList values)
		{
			if (values == null)
			{
				values = new ArrayList();
			}
			base.InnerList.AddRange(values);
		}

		internal void Add(string value)
		{
			base.InnerList.Add(value);
		}

		public bool Contains(string value)
		{
			if (value != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					string item = (string)base.InnerList[num];
					if (Utils.Compare(item, value) != 0)
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
				throw new ArgumentNullException("value");
			}
		}

		public void CopyTo(string[] values, int index)
		{
			base.InnerList.CopyTo(values, index);
		}

		public int IndexOf(string value)
		{
			if (value != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					string item = (string)base.InnerList[num];
					if (Utils.Compare(item, value) != 0)
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
				throw new ArgumentNullException("value");
			}
		}
	}
}