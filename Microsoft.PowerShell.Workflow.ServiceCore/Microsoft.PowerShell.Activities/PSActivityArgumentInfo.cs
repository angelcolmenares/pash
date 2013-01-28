using System;
using System.Activities;

namespace Microsoft.PowerShell.Activities
{
	public sealed class PSActivityArgumentInfo
	{
		public string Name
		{
			get;
			set;
		}

		public Argument Value
		{
			get;
			set;
		}

		public PSActivityArgumentInfo()
		{
		}
	}
}