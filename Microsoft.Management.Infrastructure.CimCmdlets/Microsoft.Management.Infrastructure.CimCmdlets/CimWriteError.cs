using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimWriteError : CimSyncAction
	{
		private CimInstance error;

		private Exception exception;

		private InvocationContext invocationContext;

		private CimResultContext cimResultContext;

		internal Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		public CimWriteError(CimInstance error, InvocationContext context)
		{
			this.error = error;
			this.invocationContext = context;
		}

		public CimWriteError(Exception exception, InvocationContext context, CimResultContext cimResultContext)
		{
			this.exception = exception;
			this.invocationContext = context;
			this.cimResultContext = cimResultContext;
		}

		public override void Execute(CmdletOperationBase cmdlet)
		{
			Exception cimException;
			try
			{
				try
				{
					if (this.error != null)
					{
						cimException = new CimException(this.error);
					}
					else
					{
						cimException = this.Exception;
					}
					Exception exception = cimException;
					cmdlet.WriteError(ErrorToErrorRecord.ErrorRecordFromAnyException(this.invocationContext, exception, this.cimResultContext));
					this.responseType = CimResponseType.Yes;
				}
				catch
				{
					this.responseType = CimResponseType.NoToAll;
					throw;
				}
			}
			finally
			{
				this.OnComplete();
			}
		}
	}
}