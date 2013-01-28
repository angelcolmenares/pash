using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal abstract class CimSubscriptionEventArgs : EventArgs
	{
		protected object context;

		public object Context
		{
			get
			{
				return this.context;
			}
		}

		protected CimSubscriptionEventArgs()
		{
		}
	}
}