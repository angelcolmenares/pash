using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CmdletOperationInvokeCimMethod : CmdletOperationBase
	{
		private const string theCimInvokeCimMethodParameterName = "theCimInvokeCimMethod";

		private CimInvokeCimMethod cimInvokeCimMethod;

		public CmdletOperationInvokeCimMethod(Cmdlet cmdlet, CimInvokeCimMethod theCimInvokeCimMethod) : base(cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(theCimInvokeCimMethod, "theCimInvokeCimMethod");
			this.cimInvokeCimMethod = theCimInvokeCimMethod;
		}

		public override void WriteObject(object sendToPipeline, XOperationContextBase context)
		{
			DebugHelper.WriteLogEx();
			if (sendToPipeline as CimInstance == null)
			{
				base.WriteObject(sendToPipeline, context);
				return;
			}
			else
			{
				this.cimInvokeCimMethod.InvokeCimMethodOnCimInstance(sendToPipeline as CimInstance, context, this);
				return;
			}
		}

		public override void WriteObject(object sendToPipeline, bool enumerateCollection, XOperationContextBase context)
		{
			if (sendToPipeline as CimInstance == null)
			{
				base.WriteObject(sendToPipeline, enumerateCollection, context);
				return;
			}
			else
			{
				this.WriteObject(sendToPipeline, context);
				return;
			}
		}
	}
}