using System;
using System.ComponentModel;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class ExtendedDNControl : DirectoryControl
	{
		private ExtendedDNFlag format;

		public ExtendedDNFlag Flag
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.format;
			}
			set
			{
				if (value < ExtendedDNFlag.HexString || value > ExtendedDNFlag.StandardString)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ExtendedDNFlag));
				}
				else
				{
					this.format = value;
					return;
				}
			}
		}

		public ExtendedDNControl() : base("1.2.840.113556.1.4.529", null, true, true)
		{
		}

		public ExtendedDNControl(ExtendedDNFlag flag) : this()
		{
			this.Flag = flag;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = (int)this.format;
			this.directoryControlValue = BerConverter.Encode("{i}", objArray);
			return base.GetValue();
		}
	}
}