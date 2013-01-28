using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class PageResultRequestControl : DirectoryControl
	{
		private int size;

		private byte[] pageCookie;

		public byte[] Cookie
		{
			get
			{
				if (this.pageCookie != null)
				{
					byte[] numArray = new byte[(int)this.pageCookie.Length];
					for (int i = 0; i < (int)this.pageCookie.Length; i++)
					{
						numArray[i] = this.pageCookie[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.pageCookie = value;
			}
		}

		public int PageSize
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.size;
			}
			set
			{
				if (value >= 0)
				{
					this.size = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public PageResultRequestControl() : base("1.2.840.113556.1.4.319", null, true, true)
		{
			this.size = 0x200;
		}

		public PageResultRequestControl(int pageSize) : this()
		{
			this.PageSize = pageSize;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PageResultRequestControl(byte[] cookie) : this()
		{
			this.pageCookie = cookie;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[2];
			objArray[0] = this.size;
			objArray[1] = this.pageCookie;
			object[] objArray1 = objArray;
			this.directoryControlValue = BerConverter.Encode("{io}", objArray1);
			return base.GetValue();
		}
	}
}