using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADAsqResponseControl : DirectoryControl
	{
		private ResultCode _result;

		public ResultCode Result
		{
			get
			{
				return this._result;
			}
		}

		public ADAsqResponseControl(int result, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.1504", controlValue, criticality, true)
		{
			this._result = (ResultCode)result;
		}
	}
}