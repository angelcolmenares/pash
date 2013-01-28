using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class IgnoreResultObserver : CimResultObserver<CimInstance>
	{
		public IgnoreResultObserver(CimSession session, IObservable<object> observable) : base(session, observable)
		{
		}

		public override void OnNext(CimInstance value)
		{
			DebugHelper.WriteLogEx();
		}
	}
}