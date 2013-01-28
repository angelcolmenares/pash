using System;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
	internal abstract class AsyncCmdletHelper : IThrottleOperation
	{
		protected Exception internalException;

		internal Exception InternalException
		{
			get
			{
				return this.internalException;
			}
		}

		protected AsyncCmdletHelper()
		{
		}
	}
}