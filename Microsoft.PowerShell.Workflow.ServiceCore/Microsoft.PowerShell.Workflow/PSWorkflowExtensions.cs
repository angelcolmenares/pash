using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Workflow
{
	public static class PSWorkflowExtensions
	{
		public static Func<IEnumerable<object>> CustomHandler
		{
			get;set;
		}

	}
}