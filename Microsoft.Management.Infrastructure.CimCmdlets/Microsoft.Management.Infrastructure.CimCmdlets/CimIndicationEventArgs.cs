using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public abstract class CimIndicationEventArgs : EventArgs
	{
		internal object context;

		public object Context
		{
			get
			{
				return this.context;
			}
		}

		protected CimIndicationEventArgs()
		{
		}
	}
}