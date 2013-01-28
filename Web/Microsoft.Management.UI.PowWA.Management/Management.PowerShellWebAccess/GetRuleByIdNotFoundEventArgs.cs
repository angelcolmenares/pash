using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class GetRuleByIdNotFoundEventArgs : EventArgs
	{
		public int Id
		{
			get;
			private set;
		}

		public GetRuleByIdNotFoundEventArgs(int id)
		{
			this.Id = id;
		}
	}
}