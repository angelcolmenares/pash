using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimWriteResultObject : CimBaseAction
	{
		private object result;

		internal object Result
		{
			get
			{
				return this.result;
			}
		}

		public CimWriteResultObject(object result, XOperationContextBase theContext)
		{
			this.result = result;
			base.Context = theContext;
		}

		public override void Execute(CmdletOperationBase cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
			cmdlet.WriteObject(this.result, base.Context);
		}
	}
}