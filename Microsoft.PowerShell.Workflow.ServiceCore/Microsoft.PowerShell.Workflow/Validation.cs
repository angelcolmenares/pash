using System;
using System.Activities;

namespace Microsoft.PowerShell.Workflow
{
	public static class Validation
	{
		public static Func<Activity, bool> CustomHandler
		{
			get;set;
		}

	}
}