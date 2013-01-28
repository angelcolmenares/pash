using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class AsyncResultErrorEventArgs : AsyncResultEventArgsBase
	{
		public readonly Exception error;

		public AsyncResultErrorEventArgs(CimSession session, IObservable<object> observable, Exception error) : base(session, observable, (AsyncResultType)1)
		{
			this.error = error;
		}

		public AsyncResultErrorEventArgs(CimSession session, IObservable<object> observable, Exception error, CimResultContext cimResultContext) : base(session, observable, (AsyncResultType)1, cimResultContext)
		{
			this.error = error;
		}
	}
}