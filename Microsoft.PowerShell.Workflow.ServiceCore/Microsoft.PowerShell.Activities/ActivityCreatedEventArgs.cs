using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Activities
{
	internal class ActivityCreatedEventArgs : EventArgs
	{
		public System.Management.Automation.PowerShell PowerShellInstance
		{
			get;
			set;
		}

		internal ActivityCreatedEventArgs(System.Management.Automation.PowerShell instance)
		{
			this.PowerShellInstance = instance;
		}
	}
}