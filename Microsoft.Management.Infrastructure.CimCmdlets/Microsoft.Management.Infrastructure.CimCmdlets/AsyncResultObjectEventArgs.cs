using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class AsyncResultObjectEventArgs : AsyncResultEventArgsBase
	{
		public readonly object resultObject;

		public AsyncResultObjectEventArgs(CimSession session, IObservable<object> observable, object resultObject) : base(session, observable, 0)
		{
			this.resultObject = resultObject;
		}
	}
}