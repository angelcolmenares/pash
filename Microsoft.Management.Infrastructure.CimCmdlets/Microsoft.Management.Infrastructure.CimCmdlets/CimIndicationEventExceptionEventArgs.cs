using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public class CimIndicationEventExceptionEventArgs : CimIndicationEventArgs
	{
		private Exception exception;

		public Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		public CimIndicationEventExceptionEventArgs(Exception theException)
		{
			this.context = null;
			this.exception = theException;
		}
	}
}