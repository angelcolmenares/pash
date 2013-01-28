using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class SortResponseControl : DirectoryControl
	{
		private ResultCode result;

		private string name;

		public string AttributeName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		public ResultCode Result
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.result;
			}
		}

		internal SortResponseControl(ResultCode result, string attributeName, bool critical, byte[] value) : base("1.2.840.113556.1.4.474", value, critical, true)
		{
			this.result = result;
			this.name = attributeName;
		}
	}
}