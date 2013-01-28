using System;
using System.Runtime;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public class VerifyNameControl : DirectoryControl
	{
		private string name;

		private int flag;

		public int Flag
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

		public string ServerName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
			set
			{
				if (value != null)
				{
					this.name = value;
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public VerifyNameControl() : base("1.2.840.113556.1.4.1338", null, true, true)
		{
		}

		public VerifyNameControl(string serverName) : this()
		{
			if (serverName != null)
			{
				this.name = serverName;
				return;
			}
			else
			{
				throw new ArgumentNullException("serverName");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public VerifyNameControl(string serverName, int flag) : this(serverName)
		{
			this.flag = flag;
		}

		public override byte[] GetValue()
		{
			byte[] bytes = null;
			if (this.ServerName != null)
			{
				UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
				bytes = unicodeEncoding.GetBytes(this.ServerName);
			}
			object[] objArray = new object[2];
			objArray[0] = this.flag;
			objArray[1] = bytes;
			this.directoryControlValue = BerConverter.Encode("{io}", objArray);
			return base.GetValue();
		}
	}
}