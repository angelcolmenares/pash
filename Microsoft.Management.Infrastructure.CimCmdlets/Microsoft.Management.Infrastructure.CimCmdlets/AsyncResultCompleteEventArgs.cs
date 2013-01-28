using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class AsyncResultCompleteEventArgs : AsyncResultEventArgsBase
	{
		public AsyncResultCompleteEventArgs(CimSession session, IObservable<object> observable) : base(session, observable, (AsyncResultType)2)
		{
		}
	}
}