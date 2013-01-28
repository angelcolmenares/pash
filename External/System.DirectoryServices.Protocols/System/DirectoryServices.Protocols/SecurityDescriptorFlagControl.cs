using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class SecurityDescriptorFlagControl : DirectoryControl
	{
		private SecurityMasks flag;

		public SecurityMasks SecurityMasks
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

		public SecurityDescriptorFlagControl() : base("1.2.840.113556.1.4.801", null, true, true)
		{
		}

		public SecurityDescriptorFlagControl(SecurityMasks masks) : this()
		{
			this.SecurityMasks = masks;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = (int)this.flag;
			this.directoryControlValue = BerConverter.Encode("{i}", objArray);
			return base.GetValue();
		}
	}
}