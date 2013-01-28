using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class PageResultResponseControl : DirectoryControl
	{
		private byte[] pageCookie;

		private int count;

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
		}

		public int TotalCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.count;
			}
		}

		internal PageResultResponseControl(int count, byte[] cookie, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.319", controlValue, criticality, true)
		{
			this.count = count;
			this.pageCookie = cookie;
		}
	}
}