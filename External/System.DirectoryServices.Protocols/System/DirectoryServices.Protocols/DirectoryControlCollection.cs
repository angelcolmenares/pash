using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryControlCollection : CollectionBase
	{
		public DirectoryControl this[int index]
		{
			get
			{
				return (DirectoryControl)base.List[index];
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
					throw new ArgumentNullException("value");
				}
			}
		}

		public DirectoryControlCollection()
		{
			Utility.CheckOSVersion();
		}

		public int Add(DirectoryControl control)
		{
			if (control != null)
			{
				return base.List.Add(control);
			}
			else
			{
				throw new ArgumentNullException("control");
			}
		}

		public void AddRange(DirectoryControl[] controls)
		{
			if (controls != null)
			{
				DirectoryControl[] directoryControlArray = controls;
				int num = 0;
				while (num < (int)directoryControlArray.Length)
				{
					DirectoryControl directoryControl = directoryControlArray[num];
					if (directoryControl != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException(Res.GetString("ContainNullControl"), "controls");
					}
				}
				base.InnerList.AddRange(controls);
				return;
			}
			else
			{
				throw new ArgumentNullException("controls");
			}
		}

		public void AddRange(DirectoryControlCollection controlCollection)
		{
			if (controlCollection != null)
			{
				int count = controlCollection.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(controlCollection[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("controlCollection");
			}
		}

		public bool Contains(DirectoryControl value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(DirectoryControl[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(DirectoryControl value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, DirectoryControl value)
		{
			if (value != null)
			{
				base.List.Insert(index, value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (value as DirectoryControl != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "DirectoryControl";
					throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(DirectoryControl value)
		{
			base.List.Remove(value);
		}
	}
}