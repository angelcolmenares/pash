using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class AsqRequestControl : DirectoryControl
	{
		private string name;

		public string AttributeName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.name = value;
			}
		}

		public AsqRequestControl() : base("1.2.840.113556.1.4.1504", null, true, true)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public AsqRequestControl(string attributeName) : this()
		{
			this.name = attributeName;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = this.name;
			this.directoryControlValue = BerConverter.Encode("{s}", objArray);
			return base.GetValue();
		}
	}
}