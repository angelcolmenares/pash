using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CmdletOperationTestCimSession : CmdletOperationBase
	{
		private const string theCimNewSessionParameterName = "theCimNewSession";

		private CimNewSession cimNewSession;

		public CmdletOperationTestCimSession(Cmdlet cmdlet, CimNewSession theCimNewSession) : base(cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(theCimNewSession, "theCimNewSession");
			this.cimNewSession = theCimNewSession;
		}

		public override void WriteObject(object sendToPipeline, XOperationContextBase context)
		{
			DebugHelper.WriteLogEx();
			if (sendToPipeline as CimSession == null)
			{
				if (sendToPipeline as PSObject == null)
				{
					object[] objArray = new object[1];
					objArray[0] = sendToPipeline;
					DebugHelper.WriteLog("Ignore other type object {0}", 1, objArray);
					return;
				}
				else
				{
					DebugHelper.WriteLog("Write PSObject to pipeline", 1);
					base.WriteObject(sendToPipeline, context);
					return;
				}
			}
			else
			{
				DebugHelper.WriteLog("Call CimNewSession::AddSessionToCache", 1);
				this.cimNewSession.AddSessionToCache(sendToPipeline as CimSession, context, this);
				return;
			}
		}
	}
}