using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal abstract class AsyncResultEventArgsBase : EventArgs
	{
		public readonly CimSession session;

		public readonly IObservable<object> observable;

		public readonly AsyncResultType resultType;

		public readonly CimResultContext context;

		public AsyncResultEventArgsBase(CimSession session, IObservable<object> observable, AsyncResultType resultType)
		{
			this.session = session;
			this.observable = observable;
			this.resultType = resultType;
		}

		public AsyncResultEventArgsBase(CimSession session, IObservable<object> observable, AsyncResultType resultType, CimResultContext cimResultContext)
		{
			this.session = session;
			this.observable = observable;
			this.resultType = resultType;
			this.context = cimResultContext;
		}
	}
}