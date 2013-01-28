using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal abstract class CimBaseAction
	{
		private XOperationContextBase context;

		protected XOperationContextBase Context
		{
			get
			{
				return this.context;
			}
			set
			{
				this.context = value;
			}
		}

		public CimBaseAction()
		{
		}

		public virtual void Execute(CmdletOperationBase cmdlet)
		{
		}
	}
}