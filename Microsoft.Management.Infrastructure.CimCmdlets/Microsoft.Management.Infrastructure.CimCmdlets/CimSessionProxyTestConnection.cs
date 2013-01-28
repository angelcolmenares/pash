using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionProxyTestConnection : CimSessionProxy
	{
		public CimSessionProxyTestConnection(string computerName, CimSessionOptions sessionOptions) : base(computerName, sessionOptions)
		{
		}

		protected override void PreOperationDeleteEvent(OperationEventArgs args)
		{
			object[] objArray = new object[1];
			objArray[0] = args.success;
			DebugHelper.WriteLogEx("test connection result {0}", 0, objArray);
			if (args.success)
			{
				CimWriteResultObject cimWriteResultObject = new CimWriteResultObject(base.CimSession, base.ContextObject);
				base.FireNewActionEvent(cimWriteResultObject);
			}
		}
	}
}