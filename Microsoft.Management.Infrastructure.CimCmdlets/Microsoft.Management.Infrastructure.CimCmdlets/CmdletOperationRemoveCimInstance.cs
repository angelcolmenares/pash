using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CmdletOperationRemoveCimInstance : CmdletOperationBase
	{
		private const string cimRemoveCimInstanceParameterName = "cimRemoveCimInstance";

		private CimRemoveCimInstance removeCimInstance;

		public CmdletOperationRemoveCimInstance(Cmdlet cmdlet, CimRemoveCimInstance cimRemoveCimInstance) : base(cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(cimRemoveCimInstance, "cimRemoveCimInstance");
			this.removeCimInstance = cimRemoveCimInstance;
		}

		public override void WriteObject(object sendToPipeline, XOperationContextBase context)
		{
			if (sendToPipeline as CimInstance == null)
			{
				base.WriteObject(sendToPipeline, context);
				return;
			}
			else
			{
				DebugHelper.WriteLog(">>>>CmdletOperationRemoveCimInstance::WriteObject", 4);
				this.removeCimInstance.RemoveCimInstance(sendToPipeline as CimInstance, context, this);
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