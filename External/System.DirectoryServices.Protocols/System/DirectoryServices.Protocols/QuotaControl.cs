using System;
using System.Security.Principal;

namespace System.DirectoryServices.Protocols
{
	public class QuotaControl : DirectoryControl
	{
		private byte[] sid;

		public SecurityIdentifier QuerySid
		{
			get
			{
				if (this.sid != null)
				{
					return new SecurityIdentifier(this.sid, 0);
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (value != null)
				{
					this.sid = new byte[value.BinaryLength];
					value.GetBinaryForm(this.sid, 0);
					return;
				}
				else
				{
					this.sid = null;
					return;
				}
			}
		}

		public QuotaControl() : base("1.2.840.113556.1.4.1852", null, true, true)
		{
		}

		public QuotaControl(SecurityIdentifier querySid) : this()
		{
			this.QuerySid = querySid;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = this.sid;
			this.directoryControlValue = BerConverter.Encode("{o}", objArray);
			return base.GetValue();
		}
	}
}