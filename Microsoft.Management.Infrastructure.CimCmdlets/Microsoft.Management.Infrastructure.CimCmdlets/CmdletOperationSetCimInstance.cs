using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CmdletOperationSetCimInstance : CmdletOperationBase
	{
		private const string theCimSetCimInstanceParameterName = "theCimSetCimInstance";

		private CimSetCimInstance setCimInstance;

		public CmdletOperationSetCimInstance(Cmdlet cmdlet, CimSetCimInstance theCimSetCimInstance) : base(cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(theCimSetCimInstance, "theCimSetCimInstance");
			this.setCimInstance = theCimSetCimInstance;
		}

		public override void WriteObject(object sendToPipeline, XOperationContextBase context)
		{
			DebugHelper.WriteLogEx();
			if (sendToPipeline as CimInstance != null)
			{
				CimSetCimInstanceContext cimSetCimInstanceContext = context as CimSetCimInstanceContext;
				if (cimSetCimInstanceContext == null)
				{
					DebugHelper.WriteLog("Assert. CimSetCimInstance::SetCimInstance has NULL CimSetCimInstanceContext", 4);
				}
				else
				{
					if (string.Compare(cimSetCimInstanceContext.ParameterSetName, "QueryComputerSet", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(cimSetCimInstanceContext.ParameterSetName, "QuerySessionSet", StringComparison.OrdinalIgnoreCase) == 0)
					{
						this.setCimInstance.SetCimInstance(sendToPipeline as CimInstance, cimSetCimInstanceContext, this);
						return;
					}
					else
					{
						DebugHelper.WriteLog("Write the cimInstance to pipeline since this CimInstance is returned by SetCimInstance.", 4);
					}
				}
			}
			base.WriteObject(sendToPipeline, context);
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