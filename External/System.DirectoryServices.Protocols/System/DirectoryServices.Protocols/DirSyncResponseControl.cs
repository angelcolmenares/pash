using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class DirSyncResponseControl : DirectoryControl
	{
		private byte[] dirsyncCookie;

		private bool moreResult;

		private int size;

		public byte[] Cookie
		{
			get
			{
				if (this.dirsyncCookie != null)
				{
					byte[] numArray = new byte[(int)this.dirsyncCookie.Length];
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						numArray[i] = this.dirsyncCookie[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
		}

		public bool MoreData
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.moreResult;
			}
		}

		public int ResultSize
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.size;
			}
		}

		internal DirSyncResponseControl(byte[] cookie, bool moreData, int resultSize, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.841", controlValue, criticality, true)
		{
			this.dirsyncCookie = cookie;
			this.moreResult = moreData;
			this.size = resultSize;
		}
	}
}