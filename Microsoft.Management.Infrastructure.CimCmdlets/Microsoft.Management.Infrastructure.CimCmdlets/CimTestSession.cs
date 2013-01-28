using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimTestSession : CimAsyncOperation
	{
		internal CimTestSession()
		{
		}

		internal void TestCimSession(string computerName, CimSessionProxy proxy)
		{
			DebugHelper.WriteLogEx();
			base.SubscribeEventAndAddProxytoCache(proxy);
			proxy.TestConnectionAsync();
		}
	}
}