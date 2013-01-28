using System;
using System.Runtime;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public class CrossDomainMoveControl : DirectoryControl
	{
		private string dcName;

		public string TargetDomainController
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dcName;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dcName = value;
			}
		}

		public CrossDomainMoveControl() : base("1.2.840.113556.1.4.521", null, true, true)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public CrossDomainMoveControl(string targetDomainController) : this()
		{
			this.dcName = targetDomainController;
		}

		public override byte[] GetValue()
		{
			if (this.dcName != null)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				byte[] bytes = uTF8Encoding.GetBytes(this.dcName);
				this.directoryControlValue = new byte[(int)bytes.Length + 2];
				for (int i = 0; i < (int)bytes.Length; i++)
				{
					this.directoryControlValue[i] = bytes[i];
				}
			}
			return base.GetValue();
		}
	}
}