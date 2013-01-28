using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class AsqResponseControl : DirectoryControl
	{
		private ResultCode result;

		public ResultCode Result
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.result;
			}
		}

		internal AsqResponseControl(int result, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.1504", controlValue, criticality, true)
		{
			this.result = (ResultCode)result;
		}
	}
}