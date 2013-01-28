using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class DirSyncRequestControl : DirectoryControl
	{
		private byte[] dirsyncCookie;

		private DirectorySynchronizationOptions flag;

		private int count;

		public int AttributeCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.count;
			}
			set
			{
				if (value >= 0)
				{
					this.count = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

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
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dirsyncCookie = value;
			}
		}

		public DirectorySynchronizationOptions Option
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.flag;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.flag = value;
			}
		}

		public DirSyncRequestControl() : base("1.2.840.113556.1.4.841", null, true, true)
		{
			this.count = 0x100000;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirSyncRequestControl(byte[] cookie) : this()
		{
			this.dirsyncCookie = cookie;
		}

		public DirSyncRequestControl(byte[] cookie, DirectorySynchronizationOptions option) : this(cookie)
		{
			this.Option = option;
		}

		public DirSyncRequestControl(byte[] cookie, DirectorySynchronizationOptions option, int attributeCount) : this(cookie, option)
		{
			this.AttributeCount = attributeCount;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[3];
			objArray[0] = (int)this.flag;
			objArray[1] = this.count;
			objArray[2] = this.dirsyncCookie;
			object[] objArray1 = objArray;
			this.directoryControlValue = BerConverter.Encode("{iio}", objArray1);
			return base.GetValue();
		}
	}
}