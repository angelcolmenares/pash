using System;

namespace Microsoft.PowerShell.Activities
{
	internal class RetryCount
	{
		internal int ActionAttempts
		{
			get;
			set;
		}

		public RetryCount()
		{
		}
	}
}