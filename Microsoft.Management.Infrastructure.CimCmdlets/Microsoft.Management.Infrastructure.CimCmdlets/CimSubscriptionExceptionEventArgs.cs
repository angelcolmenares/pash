using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSubscriptionExceptionEventArgs : CimSubscriptionEventArgs
	{
		private Exception exception;

		public Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		public CimSubscriptionExceptionEventArgs(Exception theException)
		{
			this.context = null;
			this.exception = theException;
		}
	}
}