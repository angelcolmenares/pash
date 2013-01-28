using System;
using System.Runtime;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class OMObjectClass
	{
		public byte[] data;

		public byte[] Data
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.data;
			}
		}

		public OMObjectClass(byte[] data)
		{
			this.data = data;
		}

		public bool Equals(OMObjectClass OMObjectClass)
		{
			bool flag = true;
			if ((int)this.data.Length != (int)OMObjectClass.data.Length)
			{
				flag = false;
			}
			else
			{
				int num = 0;
				while (num < (int)this.data.Length)
				{
					if (this.data[num] == OMObjectClass.data[num])
					{
						num++;
					}
					else
					{
						flag = false;
						return flag;
					}
				}
			}
			return flag;
		}
	}
}