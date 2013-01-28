using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class GetRuleByNameNotFoundEventArgs : EventArgs
	{
		public string Name
		{
			get;
			private set;
		}

		public GetRuleByNameNotFoundEventArgs(string name)
		{
			this.Name = name;
		}
	}
}