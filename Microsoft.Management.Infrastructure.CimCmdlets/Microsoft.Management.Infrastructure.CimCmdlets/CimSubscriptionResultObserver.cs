using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSubscriptionResultObserver : CimResultObserver<CimSubscriptionResult>
	{
		public CimSubscriptionResultObserver(CimSession session, IObservable<object> observable) : base(session, observable)
		{
		}

		public CimSubscriptionResultObserver(CimSession session, IObservable<object> observable, CimResultContext context) : base(session, observable, context)
		{
		}

		public override void OnNext(CimSubscriptionResult value)
		{
			DebugHelper.WriteLogEx();
			base.OnNextCore(value);
		}
	}
}